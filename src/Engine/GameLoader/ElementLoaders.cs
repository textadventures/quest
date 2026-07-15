using System.Text.RegularExpressions;
using System.Xml;
using QuestViva.Engine.Scripts;
using QuestViva.Engine.Types;

// ReSharper disable UnusedType.Local

namespace QuestViva.Engine.GameLoader;

internal partial class GameLoader
{
    private readonly Dictionary<string, IXmlLoader> _xmlLoaders = new();
    private IXmlLoader _defaultXmlLoader = null!;

    private Dictionary<string, IAttributeLoader> AttributeLoaders { get; } = new();
    private Dictionary<string, IExtendedAttributeLoader> ExtendedAttributeLoaders { get; } = new();

    private WorldModel WorldModel { get; }
    private ScriptFactory ScriptFactory { get; }

    private void AddXmlLoaders(LoadMode mode)
    {
        IXmlLoader[] loaders =
        [
            new ASLElementLoader(),
            new CommandLoader(),
            new DelegateLoader(),
            new DynamicTemplateLoader(),
            new EditorControlLoader(),
            new EditorImpliedTypeLoader(),
            new EditorIncludeLoader(),
            new EditorLoader(),
            new EditorTabLoader(),
            new ExitLoader(),
            new FunctionLoader(),
            new GameElementLoader(),
            new ImpliedTypeLoader(),
            new IncludeLoader(),
            new InheritLoader(),
            new JavascriptReferenceLoader(),
            new LibraryLoader(),
            new ObjectLoader(),
            new OutputLoader(),
            new ResourceLoader(),
            new TemplateLoader(),
            new TimerLoader(),
            new TurnScriptLoader(),
            new TypeLoader(),
            new VerbLoader(),
            new VerbTemplateLoader(),
            new WalkthroughLoader(),
        ];
        foreach (var loader in loaders) AddXmlLoader(loader, mode);

        _defaultXmlLoader = new DefaultXmlLoader();
        InitXmlLoader(_defaultXmlLoader);
    }

    private void AddXmlLoader(IXmlLoader loader, LoadMode mode)
    {
        InitXmlLoader(loader);
        if (loader.AppliesTo != null && loader.SupportsMode(mode))
        {
            _xmlLoaders.Add(loader.AppliesTo, loader);
        }
    }

    private void InitXmlLoader(IXmlLoader loader)
    {
        loader.GameLoader = this;
        loader.AddError += AddError;
        loader.LoadNestedXml += LoadXml;
    }

    private delegate void LoadNestedXmlHandler(Stream stream, XmlReader newReader);

    private interface IXmlLoader
    {
        string? AppliesTo { get; }
        GameLoader GameLoader { set; }
        event AddErrorHandler AddError;
        event LoadNestedXmlHandler LoadNestedXml;
        void StartElement(XmlReader reader, ref Element? current);
        void EndElement(XmlReader reader, ref Element? current);
        void SetText(string text, ref Element? current);
        bool SupportsMode(LoadMode mode);
    }

    private abstract class XmlLoaderBase : IXmlLoader
    {
        private GameLoader _loader = null!;
        protected WorldModel WorldModel { get; private set; } = null!;

        protected virtual bool CanContainNestedAttributes => false;

        public event AddErrorHandler? AddError;
        public event LoadNestedXmlHandler? LoadNestedXml;
        public abstract string? AppliesTo { get; }

        public void StartElement(XmlReader reader, ref Element? current)
        {
            var createdObject = Load(reader, ref current);

            if (createdObject is Element element)
            {
                GameLoader.AddedElement(element);
            }

            if (!CanContainNestedAttributes)
            {
                return;
            }

            if (createdObject != null && !reader.IsEmptyElement)
            {
                current = (Element) createdObject;
            }
        }

        public void EndElement(XmlReader reader, ref Element? current)
        {
            if (!CanContainNestedAttributes)
            {
                return;
            }

            current = current!.Parent == null ? null : current.Parent;
        }

        public GameLoader GameLoader
        {
            get => _loader;
            set
            {
                _loader = value;
                WorldModel = _loader.WorldModel;
            }
        }

        public virtual void SetText(string text, ref Element? current)
        {
        }

        public virtual bool SupportsMode(LoadMode mode)
        {
            return true;
        }

        public abstract object? Load(XmlReader reader, ref Element? current);

        protected void RaiseError(string error)
        {
            AddError?.Invoke(error);
        }

        protected void LoadXml(Stream stream, XmlReader newReader)
        {
            LoadNestedXml?.Invoke(stream, newReader);
        }
    }

    private partial class CommandLoader : XmlLoaderBase
    {
        private readonly RequiredAttributes _requiredAttributes = new(
            new RequiredAttribute("name", true, true, false),
            new RequiredAttribute("pattern", true, true, false),
            new RequiredAttribute("unresolved", true, true, false),
            new RequiredAttribute("template", false, true, false));

        public override string AppliesTo => "command";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            return Load(reader, ref current, null);
        }

        protected object Load(XmlReader reader, ref Element? current, string? defaultName)
        {
            var data = GameLoader.GetRequiredAttributes(reader, _requiredAttributes);

            var pattern = data["pattern"];
            var name = data["name"];
            var template = data["template"];

            var anonymous = false;

            if (string.IsNullOrEmpty(name))
            {
                name = defaultName;
            }

            if (string.IsNullOrEmpty(name))
            {
                anonymous = true;
                name = GetUniqueCommandId(pattern);
            }

            var newCommand = WorldModel.ObjectFactory.CreateCommand(name);

            if (anonymous)
            {
                newCommand.Fields[FieldDefinitions.Anonymous] = true;
            }

            if (current != null)
            {
                newCommand.Parent = current;
            }

            if (pattern != null)
            {
                newCommand.Fields[FieldDefinitions.Pattern] = pattern;
            }

            if (template != null)
            {
                LoadTemplate(newCommand, template);
            }

            var unresolved = data["unresolved"];
            if (!string.IsNullOrEmpty(unresolved))
            {
                newCommand.Fields[FieldDefinitions.Unresolved] = unresolved;
            }

            return newCommand;
        }

        private void LoadTemplate(Element newCommand, string template)
        {
            var pattern = WorldModel.Template.GetText(template);
            if (WorldModel.EditMode)
            {
                newCommand.Fields.Set(FieldDefinitions.Pattern.Property,
                    new EditorCommandPattern(Utility.ConvertVerbSimplePatternForEditor(pattern)));
            }
            else
            {
                if (WorldModel.Version >= WorldModelVersion.v530)
                {
                    var verbs = pattern.Split(';');
                    newCommand.Fields[FieldDefinitions.DisplayVerb] = verbs[0].Trim();
                }

                LoadPattern(newCommand, pattern);
            }
        }

        protected virtual void LoadPattern(Element newCommand, string pattern)
        {
            newCommand.Fields[FieldDefinitions.Pattern] = Utility.ConvertVerbSimplePattern(pattern, null);
        }

        public override void SetText(string text, ref Element? current)
        {
            if (current == null)
            {
                throw new Exception("Current element is not set");
            }

            current.Fields.LazyFields.AddScript("script", GameLoader.GetTemplate(text));
        }

        [GeneratedRegex("[A-Za-z0-9]+")]
        private partial Regex m_regex();

        private string GetUniqueCommandId(string? pattern)
        {
            var name = pattern == null ? null : m_regex().Match(pattern.Replace(" ", "")).Value;

            if (string.IsNullOrEmpty(name) || WorldModel.ObjectExists(name))
            {
                name = WorldModel.GetUniqueId(name);
            }

            return name;
        }
    }

    private class VerbLoader : CommandLoader
    {
        public override string AppliesTo => "verb";

        public override object Load(XmlReader reader, ref Element? current)
        {
            var property = reader.GetAttribute("property");

            var newCommand = (Element) base.Load(reader, ref current, property);

            newCommand.Fields[FieldDefinitions.Property] = property;

            var response = reader.GetAttribute("response");
            if (!string.IsNullOrEmpty(response))
            {
                newCommand.Fields[FieldDefinitions.DefaultTemplate] = response;
            }

            newCommand.Fields.LazyFields.AddType("defaultverb");
            newCommand.Fields[FieldDefinitions.IsVerb] = true;

            return newCommand;
        }

        public override void SetText(string text, ref Element? current)
        {
            if (current == null)
            {
                throw new Exception("Current element is not set");
            }

            var contents = GameLoader.GetTemplate(text);
            current.Fields[FieldDefinitions.DefaultText] = contents;
        }

        protected override void LoadPattern(Element newCommand, string pattern)
        {
            newCommand.Fields.LazyFields.AddAction(() =>
            {
                newCommand.Fields[FieldDefinitions.Pattern] =
                    Utility.ConvertVerbSimplePattern(pattern, newCommand.Fields[FieldDefinitions.Separator]);
            });
        }
    }

    private abstract class IncludeLoaderBase : XmlLoaderBase
    {
        public override string AppliesTo => "include";
        protected abstract LoadMode LoaderMode { get; }

        public override object? Load(XmlReader reader, ref Element? current)
        {
            var filename = GameLoader.GetTemplateAttribute(reader, "ref");
            if (filename.Length == 0)
            {
                return null;
            }

            var stream = WorldModel.GetLibraryStream(filename);
            var newReader = new XmlTextReader(stream);
            while (newReader.NodeType != XmlNodeType.Element && !newReader.EOF)
            {
                newReader.Read();
            }

            if (newReader.Name != "library")
            {
                RaiseError($"Included file '{filename}' is not a library");
            }

            LoadXml(stream, newReader);
            return LoadInternal(filename);
        }

        protected abstract object? LoadInternal(string file);

        public override bool SupportsMode(LoadMode mode)
        {
            return mode == LoaderMode;
        }
    }

    private class IncludeLoader : IncludeLoaderBase
    {
        protected override LoadMode LoaderMode => LoadMode.Play;

        protected override object? LoadInternal(string file)
        {
            return null;
        }
    }

    private class EditorIncludeLoader : IncludeLoaderBase
    {
        protected override LoadMode LoaderMode => LoadMode.Edit;

        protected override object LoadInternal(string file)
        {
            var include = WorldModel.GetElementFactory(ElementType.IncludedLibrary).Create();
            include.Fields[FieldDefinitions.Filename] = file;
            include.Fields[FieldDefinitions.Anonymous] = true;
            return include;
        }
    }

    private class LibraryLoader : XmlLoaderBase
    {
        // This class doesn't need to do anything. The initial <library> tag is loaded from the XML
        // by the IncludeLoader, but we still need something to handle the closing tag.

        public override string AppliesTo => "library";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            return null;
        }
    }

    private class ASLElementLoader : XmlLoaderBase
    {
        // This class doesn't need to do anything, it just handles the closing </asl> tag.

        public override string AppliesTo => "asl";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            return null;
        }
    }

    private class DefaultXmlLoader : XmlLoaderBase
    {
        private static readonly Dictionary<string, string> LegacyTypeMappings = new()
        {
            {"list", "simplestringlist"},
            {"stringdictionary", "simplestringdictionary"},
            {"objectdictionary", "simpleobjectdictionary"}
        };

        public override string? AppliesTo => null;

        public override object? Load(XmlReader reader, ref Element? current)
        {
            if (current == null)
            {
                throw new Exception("Current element is not set");
            }

            var attribute = reader.Name;
            if (attribute == "attr")
            {
                attribute = reader.GetAttribute("name");
            }

            if (attribute == null)
            {
                throw new Exception("Invalid attribute");
            }

            var type = reader.GetAttribute("type");

            WorldModel.AddAttributeName(attribute);

            if (type == null)
            {
                var currentElementType = current.Fields.GetString("type");
                if (string.IsNullOrEmpty(currentElementType))
                {
                    // the type property is the object type, so is not set for other element types.
                    currentElementType = current.Fields.GetString("elementtype");
                }

                type = GameLoader._implicitTypes.Get(currentElementType, attribute);
            }

            // map old to new type names if necessary (but not in included library files)
            if (type != null && WorldModel.Version <= WorldModelVersion.v530 &&
                !current.MetaFields[MetaFieldDefinitions.Library] &&
                LegacyTypeMappings.TryGetValue(type, out var legacyType))
            {
                type = legacyType;
            }

            if (type != null && GameLoader.ExtendedAttributeLoaders.TryGetValue(type, out var extendedAttributeLoader))
            {
                extendedAttributeLoader.Load(reader, current);
            }
            else
            {
                string value;

                try
                {
                    value = GameLoader.GetTemplateContents(reader);
                }
                catch (XmlException)
                {
                    RaiseError(
                        $"Error loading XML data for '{current.Name}.{attribute}' - ensure that it contains no nested XML");
                    return null;
                }

                type ??= value.Length > 0 ? "string" : "boolean";

                if (GameLoader.AttributeLoaders.TryGetValue(type, out var attributeLoader))
                {
                    attributeLoader.Load(current, attribute, value);
                }
                else
                {
                    if (WorldModel.Elements.TryGetValue(ElementType.Delegate, type, out var del))
                    {
                        var proc = WorldModel.GetElementFactory(ElementType.Delegate).Create();
                        proc.MetaFields[MetaFieldDefinitions.DelegateImplementation] = true;
                        proc.Fields.LazyFields.AddScript(FieldDefinitions.Script.Property, value);
                        current.Fields.Set(attribute, new DelegateImplementation(type, del, proc));
                    }
                    else
                    {
                        RaiseError($"Unrecognised attribute type '{type}' in '{current.Name}.{attribute}'");
                    }
                }
            }

            return null;
        }
    }

    private class GameElementLoader : XmlLoaderBase
    {
        public override string AppliesTo => "game";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            var name = reader.GetAttribute("name");
            WorldModel.SetGameName(name ?? "");
            WorldModel.Game.Fields[FieldDefinitions.GameName] = name;
            return WorldModel.Game;
        }
    }

    // TO DO: This should derive from ElementLoaderBase
    private class ObjectLoader : XmlLoaderBase
    {
        public override string AppliesTo => "object";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            return current != null
                ? WorldModel.ObjectFactory.CreateObject(reader.GetAttribute("name"), current)
                : WorldModel.ObjectFactory.CreateObject(reader.GetAttribute("name"));
        }
    }

    private class ExitLoader : XmlLoaderBase
    {
        public override string AppliesTo => "exit";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            // current can be null here for some reason, e.g. in
            // https://textadventures.co.uk/games/view/_tz-z3689ku5mhp_6gc7fw/guttersnipe-carnival-of-regrets

            var alias = reader.GetAttribute("alias");
            var to = reader.GetAttribute("to");
            var id = reader.GetAttribute("name");
            if (string.IsNullOrEmpty(alias) && !WorldModel.EditMode)
            {
                alias = to;
            }

            return string.IsNullOrEmpty(id)
                ? WorldModel.ObjectFactory.CreateExitLazy(alias, current, to)
                : WorldModel.ObjectFactory.CreateExitLazy(id, alias, current, to);
        }
    }

    private class TurnScriptLoader : XmlLoaderBase
    {
        public override string AppliesTo => "turnscript";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            var id = reader.GetAttribute("name");
            return WorldModel.ObjectFactory.CreateTurnScript(id, current);
        }
    }

    private class TypeLoader : XmlLoaderBase
    {
        public override string AppliesTo => "type";

        protected override bool CanContainNestedAttributes => true;

        public override object Load(XmlReader reader, ref Element? current)
        {
            return WorldModel.GetElementFactory(ElementType.ObjectType).Create(reader.GetAttribute("name"));
        }
    }

    private class TemplateLoader : XmlLoaderBase
    {
        public bool IsBaseTemplateLoader { get; init; }

        public override string AppliesTo => "template";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            var isCommandTemplate = reader.GetAttribute("templatetype") == "command";
            return AddTemplate(reader.GetAttribute("name"), reader.ReadElementContentAsString(), isCommandTemplate);
        }

        private Element? AddTemplate(string? t, string text, bool isCommandTemplate)
        {
            if (!string.IsNullOrEmpty(t))
            {
                return WorldModel.Template.AddTemplate(t, text, isCommandTemplate, IsBaseTemplateLoader);
            }

            RaiseError("Expected 'name' attribute in template");
            return null;
        }
    }

    private class VerbTemplateLoader : XmlLoaderBase
    {
        public override string AppliesTo => "verbtemplate";

        public override object Load(XmlReader reader, ref Element? current)
        {
            return AddVerbTemplate(reader.GetAttribute("name"), reader.ReadElementContentAsString());
        }

        private Element AddVerbTemplate(string? c, string text)
        {
            return WorldModel.Template.AddVerbTemplate(c, text, GameLoader._currentFile.Peek().Filename);
        }
    }

    // TO DO: Use RequiredAttributes in the above XML loaders too...

    private class DynamicTemplateLoader : XmlLoaderBase
    {
        public override string AppliesTo => "dynamictemplate";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            return AddDynamicTemplate(reader.GetAttribute("name"), reader.ReadElementContentAsString());
        }

        private Element? AddDynamicTemplate(string? t, string expression)
        {
            if (!string.IsNullOrEmpty(t))
            {
                return WorldModel.Template.AddDynamicTemplate(t, expression);
            }

            RaiseError("Expected 'name' attribute in template");
            return null;
        }
    }

    private abstract class FunctionLoaderBase : XmlLoaderBase
    {
        private readonly string[] _delimiters = [", ", ","];

        protected RequiredAttributes RequiredAttributes { get; } = new(
            new RequiredAttribute("name"),
            new RequiredAttribute(false, "parameters"),
            new RequiredAttribute(false, "type"));

        protected void SetupProcedure(Element proc, string? returnType, string script, string name, string? parameters)
        {
            string[]? paramNames = null;

            if (!string.IsNullOrEmpty(parameters))
            {
                paramNames = parameters.Split(_delimiters, StringSplitOptions.None).Select(p => p.Trim()).ToArray();
            }

            Type? returns = null;
            if (!string.IsNullOrEmpty(returnType))
            {
                try
                {
                    returns = WorldModel.ConvertTypeNameToType(returnType);
                }
                catch (ArgumentOutOfRangeException)
                {
                    RaiseError($"Unrecognised function return type '{returnType}' in '{name}'");
                }
            }

            proc.Fields[FieldDefinitions.ParamNames] = new QuestList<string>(paramNames);
            if (returns != null)
            {
                proc.Fields[FieldDefinitions.ReturnType] = WorldModel.ConvertTypeToTypeName(returns);
            }

            proc.Fields.LazyFields.AddScript(FieldDefinitions.Script.Property, script);
        }
    }

    private class FunctionLoader : FunctionLoaderBase
    {
        public override string AppliesTo => "function";

        public override object Load(XmlReader reader, ref Element? current)
        {
            return AddProcedure(reader);
        }

        private Element AddProcedure(XmlReader reader)
        {
            var data = GameLoader.GetRequiredAttributes(reader, RequiredAttributes);
            var name = data["name"] ?? throw new InvalidOperationException("Function name is null");
            var proc = WorldModel.AddProcedure(name);
            SetupProcedure(proc, data["type"], GameLoader.GetTemplateContents(reader), name, data["parameters"]);
            return proc;
        }
    }

    private class InheritLoader : XmlLoaderBase
    {
        public override string AppliesTo => "inherit";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            if (current == null)
            {
                throw new Exception("Current element is not set");
            }

            current.Fields.LazyFields.AddType(reader.GetAttribute("name"));
            return null;
        }
    }

    private class WalkthroughLoader : ElementLoaderBase
    {
        public override string AppliesTo => "walkthrough";

        protected override ElementType CreateElementType => ElementType.Walkthrough;

        protected override string IdPrefix => "walkthrough";
    }

    private class DelegateLoader : FunctionLoaderBase
    {
        public override string AppliesTo => "delegate";

        public override object Load(XmlReader reader, ref Element? current)
        {
            return AddDelegate(reader);
        }

        private Element AddDelegate(XmlReader reader)
        {
            var data = GameLoader.GetRequiredAttributes(reader, RequiredAttributes);
            var name = data["name"] ?? throw new InvalidOperationException("Delegate name is null");
            var del = WorldModel.AddDelegate(name);
            SetupProcedure(del, data["type"], GameLoader.GetTemplateContents(reader), name, data["parameters"]);
            return del;
        }
    }

    private abstract class ImpliedTypeLoaderBase : XmlLoaderBase
    {
        private readonly RequiredAttributes _required = new(false, "element", "property", "type");

        public override string AppliesTo => "implied";
        protected abstract LoadMode LoaderMode { get; }

        public override object? Load(XmlReader reader, ref Element? current)
        {
            var data = GameLoader.GetRequiredAttributes(reader, _required);
            var element = data["element"] ?? throw new InvalidOperationException("Element is null");
            var property = data["property"] ?? throw new InvalidOperationException("Property is null");
            var type = data["type"] ?? throw new InvalidOperationException("Type is null");
            GameLoader._implicitTypes.Add(element, property, type);
            return LoadInternal(element, property, type);
        }

        protected abstract object? LoadInternal(string element, string property, string type);

        public override bool SupportsMode(LoadMode mode)
        {
            return mode == LoaderMode;
        }
    }

    private class ImpliedTypeLoader : ImpliedTypeLoaderBase
    {
        protected override LoadMode LoaderMode => LoadMode.Play;

        protected override object? LoadInternal(string element, string property, string type)
        {
            return null;
        }
    }

    private class EditorImpliedTypeLoader : ImpliedTypeLoaderBase
    {
        protected override LoadMode LoaderMode => LoadMode.Edit;

        protected override object LoadInternal(string element, string property, string type)
        {
            var e = WorldModel.GetElementFactory(ElementType.ImpliedType).Create();
            e.Fields[FieldDefinitions.Element] = element;
            e.Fields[FieldDefinitions.Property] = property;
            e.Fields[FieldDefinitions.Type] = type;
            return e;
        }
    }

    private abstract class ElementLoaderBase : XmlLoaderBase
    {
        protected override bool CanContainNestedAttributes => true;

        protected abstract ElementType CreateElementType { get; }

        protected virtual string? IdPrefix => AppliesTo;

        public override object Load(XmlReader reader, ref Element? current)
        {
            var name = reader.GetAttribute("name");
            if (string.IsNullOrEmpty(name))
            {
                name = WorldModel.GetUniqueId(IdPrefix);
            }

            var newElement = WorldModel.GetElementFactory(CreateElementType).Create(name);
            newElement.Parent = current;
            return newElement;
        }
    }

    private class EditorLoader : ElementLoaderBase
    {
        public override string AppliesTo => "editor";

        protected override ElementType CreateElementType => ElementType.Editor;

        protected override string IdPrefix => "editor";
    }

    private class EditorTabLoader : ElementLoaderBase
    {
        public override string AppliesTo => "tab";

        protected override ElementType CreateElementType => ElementType.EditorTab;

        protected override string IdPrefix => "editor";
    }

    private class EditorControlLoader : ElementLoaderBase
    {
        public override string AppliesTo => "control";

        protected override ElementType CreateElementType => ElementType.EditorControl;

        protected override string IdPrefix => "editor";
    }

    private class JavascriptReferenceLoader : XmlLoaderBase
    {
        public override string AppliesTo => "javascript";

        public override object? Load(XmlReader reader, ref Element? current)
        {
            var jsRef = WorldModel.GetElementFactory(ElementType.Javascript).Create();
            jsRef.Fields[FieldDefinitions.Anonymous] = true;
            var file = GameLoader.GetTemplateAttribute(reader, "src");
            if (file.Length == 0)
            {
                return null;
            }

            if (WorldModel.Version == WorldModelVersion.v500)
            {
                // Quest 5.0 would incorrectly save a full path name. We only want the filename.
                file = Path.GetFileName(file);
            }

            jsRef.Fields[FieldDefinitions.Src] = file;

            return jsRef;
        }
    }

    private class TimerLoader : ElementLoaderBase
    {
        public override string AppliesTo => "timer";

        protected override ElementType CreateElementType => ElementType.Timer;
    }

    private class ResourceLoader : ElementLoaderBase
    {
        public override string AppliesTo => "resource";

        protected override ElementType CreateElementType => ElementType.Resource;

        public override object Load(XmlReader reader, ref Element? current)
        {
            var resourceRef = WorldModel.GetElementFactory(ElementType.Resource).Create();
            resourceRef.Fields[FieldDefinitions.Anonymous] = true;
            var file = GameLoader.GetTemplateAttribute(reader, "src");
            resourceRef.Fields[FieldDefinitions.Src] = file;

            return resourceRef;
        }
    }

    private class OutputLoader : ElementLoaderBase
    {
        protected override ElementType CreateElementType => ElementType.Output;

        public override string AppliesTo => "output";
    }
}