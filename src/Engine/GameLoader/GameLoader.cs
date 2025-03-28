using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using QuestViva.Common;

namespace QuestViva.Engine.GameLoader;

internal partial class GameLoader
{
    public enum LoadMode
    {
        Play,
        Edit
    }

    private struct FileData
    {
        public string Filename;
        public bool IsEditorLibrary;
    }

    public class LoadStatusEventArgs(string status) : EventArgs
    {
        public string Status { get; private set; } = status;
    }

    private readonly ImplicitTypes _implicitTypes = new();
    private readonly Stack<FileData> _currentFile = new();

    public delegate void FilenameUpdatedHandler(string filename);
    public event FilenameUpdatedHandler? FilenameUpdated;

    public event EventHandler<LoadStatusEventArgs>? LoadStatus;

    private static readonly Dictionary<string, WorldModelVersion> Versions = new()
    {
        {"500", WorldModelVersion.v500},
        {"510", WorldModelVersion.v510},
        {"520", WorldModelVersion.v520},
        {"530", WorldModelVersion.v530},
        {"540", WorldModelVersion.v540},
        {"550", WorldModelVersion.v550},
        {"580", WorldModelVersion.v580},
    };

    public GameLoader(WorldModel worldModel, LoadMode mode, bool? isCompiled = null)
    {
        IsCompiledFile = isCompiled ?? false;
        WorldModel = worldModel;
        ScriptFactory = new ScriptFactory(worldModel);
        ScriptFactory.ErrorHandler += AddError;
        AddLoaders(mode);
        AddExtendedAttributeLoaders(mode);
        AddXmlLoaders(mode);
    }
        
    public async Task<bool> Load(GameData gameData, Stream? saveData)
    {
        Stream dataStream;

        if (saveData != null)
        {
            dataStream = saveData;
        }
        else
        {
            if (Path.GetExtension(gameData.Filename) == ".quest")
            {
                dataStream = await LoadCompiledFile(gameData);
            }
            else
            {
                dataStream = gameData.Data;
            }
        }

        var timer = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var reader = new XmlTextReader(dataStream);
            do
            {
                reader.Read();
            } while (reader.NodeType != XmlNodeType.Element);

            if (reader.Name == "asl")
            {
                var version = reader.GetAttribute("version");

                if (string.IsNullOrEmpty(version))
                {
                    AddError("No ASL version number found");
                }
                else if (!Versions.TryGetValue(version, out var value))
                {
                    AddError("Incorrect ASL version number");
                }
                else
                {
                    WorldModel.Version = value;
                    WorldModel.VersionString = version;
                }

                var originalFile = reader.GetAttribute("original");

                if (!string.IsNullOrEmpty(originalFile) && Path.GetExtension(originalFile) == ".quest")
                {
                    IsCompiledFile = true;
                }

                if (!string.IsNullOrEmpty(originalFile))
                {
                    FilenameUpdated?.Invoke(originalFile);
                }
            }
            else
            {
                AddError("File must begin with an ASL element");
            }

            LoadXml(gameData.Filename, reader);

            reader.Close();
        }
        catch (XmlException e)
        {
            AddError($"Invalid XML: {e.Message}");
        }
        catch (Exception e)
        {
            AddError($"Error: {e.Message}");
        }
            
        System.Diagnostics.Debug.WriteLine($"XML load time: {timer.ElapsedMilliseconds}ms");
        timer.Restart();

        if (Errors.Count == 0)
        {
            try
            {
                ResolveGame();
            }
            catch (Exception e)
            {
                AddError($"Error: {e.Message}");
            }
        }
            
        System.Diagnostics.Debug.WriteLine($"ResolveGame: {timer.ElapsedMilliseconds}ms");

        return (Errors.Count == 0);
    }

    private async Task<Stream> LoadCompiledFile(GameData gameData)
    {
        var packageReader = new PackageReader();
        var result = await packageReader.LoadPackage(gameData);
        WorldModel.ResourceGetter = result.GetFile;
        WorldModel.GetResourceNames = result.GetFileNames;
        IsCompiledFile = true;
        return result.GameFile;
    }

    private void LoadXml(string filename, XmlReader reader)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();

        Element? current = null;
        IXmlLoader? currentLoader = null;

        // Set the "IsEditorLibrary" flag for any library with type="editor", and its sub-libraries
        var isEditorLibrary = _currentFile.Count > 0 && _currentFile.Peek().IsEditorLibrary ||
                              reader.GetAttribute("type") == "editor";

        if (!IsCompiledFile && _currentFile.Count == 0 && WorldModel.Version >= WorldModelVersion.v530)
        {
            ScanForTemplates(filename);
        }

        var data = new FileData
        {
            Filename = reader.BaseURI,
            IsEditorLibrary = isEditorLibrary
        };

        _currentFile.Push(data);
        UpdateLoadStatus();

        while (reader.Read())
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentLoader = GetLoader(reader.Name, current);
                    currentLoader.StartElement(reader, ref current);
                    break;
                case XmlNodeType.EndElement:
                    GetLoader(reader.Name, current).EndElement(reader, ref current);
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    currentLoader?.SetText(reader.ReadContentAsString(), ref current);
                    // if we've eaten the content of this element, then the reader will have gone
                    // past the EndElement already, so we need to trigger the EndElement here
                    GetLoader(reader.Name, current).EndElement(reader, ref current);
                    currentLoader = null;
                    break;
            }
        }

        _currentFile.Pop();
        UpdateLoadStatus();
            
        System.Diagnostics.Debug.WriteLine($"Parsed {filename} in {timer.ElapsedMilliseconds}ms");
    }

    private void UpdateLoadStatus()
    {
        string status;
        if (_currentFile.Count > 0)
        {
            status = "Loading " + Path.GetFileName(_currentFile.Peek().Filename);
        }
        else
        {
            status = "Finished loading files";
        }
        UpdateStatus(status);
    }

    private void UpdateStatus(string status)
    {
        LoadStatus?.Invoke(this, new LoadStatusEventArgs(status));
    }

    private void AddedElement(Element newElement)
    {
        newElement.MetaFields[MetaFieldDefinitions.Filename] = _currentFile.Peek().Filename;
        newElement.MetaFields[MetaFieldDefinitions.Library] = (_currentFile.Count > 1);
        newElement.MetaFields[MetaFieldDefinitions.EditorLibrary] = (_currentFile.Peek().IsEditorLibrary);
    }

    private IXmlLoader GetLoader(string name, Element? current)
    {
        if (!_xmlLoaders.TryGetValue(name, out var loader) && current != null) loader = _defaultXmlLoader;
        if (loader == null) throw new Exception($"Unrecognised tag '{name}' outside object definition");
        return loader;
    }

    private string GetTemplateAttribute(XmlReader reader, string attribute)
    {
        return GetTemplate(reader.GetAttribute(attribute));
    }

    private string GetTemplateContents(XmlReader reader)
    {
        return GetTemplate(reader.ReadElementContentAsString());
    }

    private string GetTemplate(string? text)
    {
        return WorldModel.Template.ReplaceTemplateText(text);
    }

    // resolves "lazy" exit strings into actual objects,
    // and allows scripts to refer to procedures that aren't defined until later in the XML
    private void ResolveGame()
    {
        UpdateStatus("Initialising elements...");
        foreach (var e in WorldModel.Elements.GetElements())
        {
            e.Fields.LazyFields.Resolve(ScriptFactory);
        }
    }

    private void AddError(object? sender, ScriptFactory.AddErrorEventArgs e)
    {
        AddError(e.Error);
    }

    private void AddError(string error)
    {
        Errors.Add(error);
    }

    public List<string> Errors { get; } = [];

    private class RequiredAttribute(string name)
    {
        public string Name { get; set; } = name;
        public bool UseTemplate { get; set; }
        public bool AllowBlank { get; set; }
        public bool Required { get; set; } = true;

        public RequiredAttribute(string name, bool useTemplate, bool allowBlank, bool required)
            : this(name, useTemplate, allowBlank)
        {
            Required = required;
        }

        public RequiredAttribute(string name, bool useTemplate, bool allowBlank)
            : this(name, useTemplate)
        {
            AllowBlank = allowBlank;
        }

        private RequiredAttribute(string name, bool useTemplate)
            : this(name)
        {
            UseTemplate = useTemplate;
        }

        public RequiredAttribute(bool required, string name)
            : this(name)
        {
            Required = required;
        }
    }

    private class RequiredAttributes
    {
        public List<RequiredAttribute> Attributes { get; set; }
        public RequiredAttributes(params RequiredAttribute[] attribs)
        {
            Attributes = new List<RequiredAttribute>(attribs);
        }
        public RequiredAttributes(bool canUseTemplates, params string[] attribs)
        {
            Attributes = [];
            foreach (var attrib in attribs)
            {
                Attributes.Add(new RequiredAttribute(attrib, canUseTemplates, false));
            }
        }
    }

    private Dictionary<string, string?> GetRequiredAttributes(XmlReader reader, RequiredAttributes attribs)
    {
        var result = new Dictionary<string, string?>();

        foreach (var attrib in attribs.Attributes)
        {
            var value = attrib.UseTemplate
                ? GetTemplateAttribute(reader, attrib.Name)
                : reader.GetAttribute(attrib.Name);

            if (attrib.Required && (value == null || (!attrib.AllowBlank && value.Length == 0)))
            {
                throw new Exception($"Missing required attribute '{attrib.Name}' in '{reader.LocalName}' tag");
            }

            result.Add(attrib.Name, value);
        }

        return result;
    }

    private void ScanForTemplates(string filename)
    {
        // We only do one pass of a file, but this means that it's difficult for a game
        // file to override any templates specified in libraries. To allow for this, we
        // do a preliminary pass of the base .aslx file to scan for any template definitions,
        // then we add those and mark them as non-overwritable.

        var timer = System.Diagnostics.Stopwatch.StartNew();

        var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        XmlReader reader = new XmlTextReader(stream);

        var templateLoader = new TemplateLoader
        {
            GameLoader = this,
            IsBaseTemplateLoader = true
        };
        Element? e = null;

        while (reader.Read())
        {
            if (reader is {NodeType: XmlNodeType.Element, Name: "template"})
            {
                templateLoader.Load(reader, ref e);
            }
        }
            
        System.Diagnostics.Debug.WriteLine($"Scanned for templates in {filename} in {timer.ElapsedTicks}ms");
    }

    private class ImplicitTypes
    {
        private readonly Dictionary<string, string> _implicitTypes = new();

        public void Add(string element, string property, string type)
        {
            _implicitTypes.Add(GetKey(element, property), type);
        }

        public string? Get(string element, string property)
        {
            return _implicitTypes.GetValueOrDefault(GetKey(element, property));
        }

        private static string GetKey(string element, string property)
        {
            return element + "~" + property;
        }
    }

    public bool IsCompiledFile { get; private set; }
}