using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    partial class GameLoader
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

        public class LoadStatusEventArgs : EventArgs
        {
            public LoadStatusEventArgs(string status)
            {
                Status = status;
            }

            public string Status { get; private set; }
        }

        private WorldModel m_worldModel;
        private List<string> m_errors = new List<string>();
        private ScriptFactory m_scriptFactory;
        private ImplicitTypes m_implicitTypes = new ImplicitTypes();
        private Stack<FileData> m_currentFile = new Stack<FileData>();

        public delegate void FilenameUpdatedHandler(string filename);
        public event FilenameUpdatedHandler FilenameUpdated;

        public event EventHandler<LoadStatusEventArgs> LoadStatus;

        public GameLoader(WorldModel worldModel, LoadMode mode)
        {
            m_worldModel = worldModel;
            m_scriptFactory = new ScriptFactory(worldModel);
            m_scriptFactory.ErrorHandler += AddError;
            AddLoaders(mode);
            AddExtendedAttributeLoaders(mode);
            AddXMLLoaders(mode);
        }

        public bool Load(string filename)
        {
            IsCompiledFile = false;

            if (System.IO.Path.GetExtension(filename) == ".quest")
            {
                filename = LoadCompiledFile(filename);
            }

            XmlReader reader = null;

            try
            {
                reader = XmlReader.Create(filename);

                do
                {
                    reader.Read();
                } while (reader.NodeType != XmlNodeType.Element);

                if (reader.Name == "asl")
                {
                    string version = reader.GetAttribute("version");

                    if (string.IsNullOrEmpty(version))
                    {
                        AddError("No ASL version number found");
                    }

                    if (version != "500")
                    {
                        AddError("Incorrect ASL version number");
                    }

                    string originalFile = reader.GetAttribute("original");

                    if (!string.IsNullOrEmpty(originalFile) && System.IO.Path.GetExtension(originalFile) == ".quest")
                    {
                        LoadCompiledFile(originalFile);
                    }

                    if (!string.IsNullOrEmpty(originalFile))
                    {
                        FilenameUpdated(originalFile);
                    }
                }
                else
                {
                    AddError("File must begin with an ASL element");
                }

                LoadXML(reader);
            }
            catch (XmlException e)
            {
                AddError(string.Format("Invalid XML: {0}", e.Message));
            }
            catch (Exception e)
            {
                AddError(string.Format("Error: {0}", e.Message));
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            if (m_errors.Count == 0)
            {
                try
                {
                    ResolveGame();
                    ValidateGame();
                }
                catch (Exception e)
                {
                    AddError(string.Format("Error: {0}", e.Message));
                }
            }

            return (m_errors.Count == 0);
        }

        private string LoadCompiledFile(string filename)
        {
            PackageReader packageReader = new PackageReader();
            var result = packageReader.LoadPackage(filename);
            ResourcesFolder = result.Folder;
            IsCompiledFile = true;
            return result.GameFile;
        }

        private void LoadXML(XmlReader reader)
        {
            Element current = null;
            IXMLLoader currentLoader = null;

            // Set the "IsEditorLibrary" flag for any library with type="editor", and its sub-libraries
            bool isEditorLibrary = false;
            if (m_currentFile.Count > 0 && m_currentFile.Peek().IsEditorLibrary) isEditorLibrary = true;
            if (reader.GetAttribute("type") == "editor") isEditorLibrary = true;

            FileData data = new FileData
            {
                Filename = reader.BaseURI,
                IsEditorLibrary = isEditorLibrary
            };

            m_currentFile.Push(data);
            UpdateLoadStatus();

            while (reader.Read())
            {
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
                        currentLoader.SetText(reader.ReadContentAsString(), ref current);
                        // if we've eaten the content of this element, then the reader will have gone
                        // past the EndElement already, so we need to trigger the EndElement here
                        GetLoader(reader.Name, current).EndElement(reader, ref current);
                        currentLoader = null;
                        break;
                }
            }

            m_currentFile.Pop();
            UpdateLoadStatus();
        }

        private void UpdateLoadStatus()
        {
            string status;
            if (m_currentFile.Count > 0)
            {
                status = "Loading " + System.IO.Path.GetFileName(m_currentFile.Peek().Filename);
            }
            else
            {
                status = "";
            }
            UpdateStatus(status);
        }

        private void UpdateStatus(string status)
        {
            if (LoadStatus != null)
            {
                LoadStatus(this, new LoadStatusEventArgs(status));
            }
        }

        private void AddedElement(Element newElement)
        {
            newElement.MetaFields[MetaFieldDefinitions.Filename] = m_currentFile.Peek().Filename;
            newElement.MetaFields[MetaFieldDefinitions.Library] = (m_currentFile.Count > 1);
            newElement.MetaFields[MetaFieldDefinitions.EditorLibrary] = (m_currentFile.Peek().IsEditorLibrary);
        }

        private IXMLLoader GetLoader(string name, Element current)
        {
            IXMLLoader loader;
            if (!m_xmlLoaders.TryGetValue(name, out loader) && current != null) loader = m_defaultXmlLoader;
            if (loader == null) throw new Exception(string.Format("Unrecognised tag '{0}' outside object definition", name));
            return loader;
        }

        private Regex m_templateRegex = new Regex(@"\[(?<name>.*?)\]");

        private string GetTemplateAttribute(XmlReader reader, string attribute)
        {
            return GetTemplate(reader.GetAttribute(attribute));
        }

        private string GetTemplateContents(XmlReader reader)
        {
            return GetTemplate(reader.ReadElementContentAsString());
        }

        private string GetTemplate(string text)
        {
            if (text == null) return null;

            while (m_templateRegex.IsMatch(text))
            {
                string templateName = m_templateRegex.Match(text).Groups["name"].Value;
                string templateValue = m_worldModel.Template.GetText(templateName);
                if (templateValue == null)
                {
                    AddError(string.Format("Undefined template '{0}'", templateName));
                    return null;
                }
                text = m_templateRegex.Replace(text, templateValue, 1);
            }
            return text;
        }

        // resolves "lazy" exit strings into actual objects,
        // and allows scripts to refer to procedures that aren't defined until later in the XML
        private void ResolveGame()
        {
            int total = m_worldModel.Elements.Count();
            int count = 0;
            foreach (Element e in m_worldModel.Elements.GetElements())
            {
                count++;
                UpdateStatus(string.Format("Setting up element {0} / {1} ({2:P0})", count, total, 1.0 * count / total));
                e.Fields.LazyFields.Resolve(m_scriptFactory);
            }
        }

        private void ValidateGame()
        {
            // Check that exits don't point to invalid room names
            //    -- now impossible...
            //foreach (Exit exit in m_worldModel.Exits.Values)
            //{
            //    if (!m_worldModel.Objects.ContainsKey(exit.To))
            //    {
            //        AddError(string.Format("Exit '{0}' in '{1}' points to room '{2}', which does not exist", exit.Alias, exit.Parent, exit.To));
            //    }
            //}
        }

        private void AddError(string error)
        {
            m_errors.Add(error);
        }

        public List<string> Errors
        {
            get { return m_errors; }
        }

        private class RequiredAttribute
        {
            public string Name { get; set; }
            public bool UseTemplate { get; set; }
            public bool AllowBlank { get; set; }
            public bool Required { get; set; }

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

            public RequiredAttribute(string name, bool useTemplate)
                : this(name)
            {
                UseTemplate = useTemplate;
            }

            public RequiredAttribute(bool required, string name)
                : this(name)
            {
                Required = required;
            }

            public RequiredAttribute(string name)
            {
                Name = name;
                Required = true;
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
                Attributes = new List<RequiredAttribute>();
                foreach (string attrib in attribs)
                {
                    Attributes.Add(new RequiredAttribute(attrib, canUseTemplates, false));
                }
            }
        }

        private Dictionary<string, string> GetRequiredAttributes(XmlReader reader, RequiredAttributes attribs)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (RequiredAttribute attrib in attribs.Attributes)
            {
                string value;
                if (attrib.UseTemplate)
                {
                    value = GetTemplateAttribute(reader, attrib.Name);
                }
                else
                {
                    value = reader.GetAttribute(attrib.Name);
                }

                if (attrib.Required && (value == null || (!attrib.AllowBlank && value.Length == 0)))
                {
                    throw new Exception(string.Format("Missing required attribute '{0}' in '{1}' tag", attrib.Name, reader.LocalName));
                }

                result.Add(attrib.Name, value);
            }

            return result;
        }

        private class ImplicitTypes
        {
            private Dictionary<string, string> m_implicitTypes = new Dictionary<string, string>();

            public void Add(string element, string property, string type)
            {
                m_implicitTypes.Add(GetKey(element, property), type);
            }

            public string Get(string element, string property)
            {
                string result;
                if (m_implicitTypes.TryGetValue(GetKey(element, property), out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }

            private string GetKey(string element, string property)
            {
                return element + "~" + property;
            }
        }

        public string ResourcesFolder { get; set; }
        public bool IsCompiledFile { get; private set; }
    }
}
