using System;
using System.Collections.Generic;
using System.Linq;
using QuestViva.Engine.Scripts;
using QuestViva.Utility;

namespace QuestViva.Engine.GameLoader;

public enum SaveMode
{
    SavedGame,
    Editor,
    Package
}

internal partial class GameSaver
{
    private readonly WorldModel _worldModel;
    private readonly Dictionary<ElementType, IElementsSaver> _elementsSavers = new();
    private readonly Dictionary<ElementType, IElementSaver> _elementSavers = new();
    private Dictionary<string, string>? _impliedTypes;
    private SaveMode _mode;

    public GameSaver(WorldModel worldModel)
    {
        _worldModel = worldModel;

        // Use Reflection to create instances of all IElementSavers (save individual elements)
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IElementSaver)))
        {
            AddElementSaver((IElementSaver)Activator.CreateInstance(t)!);
        }

        // Use Reflection to create instances of all IElementsSavers (save all elements of a type)
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IElementsSaver)))
        {
            AddElementsSaver((IElementsSaver)Activator.CreateInstance(t)!);
        }
    }

    public WorldModelVersion Version => _worldModel.Version;

    private void AddElementSaver(IElementSaver saver)
    {
        saver.GameSaver = this;
        _elementSavers.Add(saver.AppliesTo, saver);
    }

    private void AddElementsSaver(IElementsSaver saver)
    {
        saver.GameSaver = this;
        _elementsSavers.Add(saver.AppliesTo, saver);
    }

    public string Save(SaveMode mode, bool? includeWalkthrough = null, string? html = null)
    {
        _mode = mode;
        GameXmlWriter.GameXmlWriterOptions? options = null;
        if (includeWalkthrough.HasValue)
        {
            options = new GameXmlWriter.GameXmlWriterOptions { IncludeWalkthrough = includeWalkthrough.Value };
        }
        var writer = new GameXmlWriter(mode, options);

        UpdateImpliedTypesCache();
            
        writer.WriteComment($"Saved by Quest Viva {Common.VersionInfo.Version}");
        writer.WriteStartElement("asl");
        if (mode == SaveMode.Editor)
        {
            _worldModel.Version = WorldModelVersion.v580;
            _worldModel.VersionString = "580";
        }
        writer.WriteAttributeString("version", _worldModel.VersionString);

        if (mode == SaveMode.SavedGame)
        {
            writer.WriteAttributeString("original", _worldModel.Filename);
            _worldModel.OutputLogger?.Save(html);
        }

        foreach (var t in Enum.GetValues<ElementType>())
        {
            if (_elementsSavers.TryGetValue(t, out var value))
            {
                value.Save(writer, _worldModel);
            }
            else
            {
                // Save the elements individually
                if (_elementSavers.TryGetValue(t, out var saver))
                {
                    if (!saver.AutoSave)
                    {
                        continue;
                    }

                    foreach (var e in _worldModel.Elements.GetElements(t).Where(CanSave))
                    {
                        saver.Save(writer, e);
                    }
                }
                else
                {
                    throw new Exception("ERROR: No ElementSaver for type " + t.ToString());
                }
            }
        }

        writer.WriteEndElement();
        writer.Close();

        return writer.ToString();
    }

    private bool CanSave(Element e)
    {
        switch (_mode)
        {
            case SaveMode.SavedGame:
                return true;
            case SaveMode.Editor:
                return !e.MetaFields[MetaFieldDefinitions.Library];
            case SaveMode.Package:
                if (e.ElemType == ElementType.IncludedLibrary) return false;
                if (e.MetaFields[MetaFieldDefinitions.EditorLibrary]) return false;
                return true;
            default:
                throw new Exception("SaveMode not implemented");
        }
    }

    public string SaveScript(GameXmlWriter writer, IScript script, int indent)
    {
        return Utility.IndentScript(script.Save(), writer.IndentLevel + indent, GameXmlWriter.IndentChars);
    }

    private void UpdateImpliedTypesCache()
    {
        _impliedTypes = new Dictionary<string, string>();
        foreach (var impliedType in _worldModel.Elements.GetElements(ElementType.ImpliedType))
        {
            var element = impliedType.Fields[FieldDefinitions.Element];
            var property = impliedType.Fields[FieldDefinitions.Property];
            var type = impliedType.Fields[FieldDefinitions.Type];
            _impliedTypes.Add(GetImpliedTypeKey(element, property), type);
        }
    }

    internal bool IsImpliedType(Element element, string attribute, string type)
    {
        var elementType = element.ElemType == ElementType.Object ? element.TypeString : element.ElementTypeString;

        if (_impliedTypes != null &&
            _impliedTypes.TryGetValue(GetImpliedTypeKey(elementType, attribute), out var impliedType))
        {
            return type == impliedType;
        }

        return type == "string";
    }

    private string GetImpliedTypeKey(string element, string property)
    {
        return string.Concat(element, "~", property);
    }

    private interface IElementsSaver
    {
        ElementType AppliesTo { get; }
        void Save(GameXmlWriter writer, WorldModel worldModel);
        GameSaver GameSaver { set; }
    }

    private interface IElementSaver
    {
        ElementType AppliesTo { get; }
        void Save(GameXmlWriter writer, Element e);
        GameSaver GameSaver { set; }
        bool AutoSave { get; }
    }

    private abstract class ElementSaverBase : IElementSaver
    {
        private FieldSaver? _fieldSaver;
        private GameSaver? _gameSaver;
        private readonly List<string> _ignoreFields = [];

        protected ElementSaverBase()
        {
            AddIgnoreField("name");
            AddIgnoreField("elementtype");
        }

        protected void AddIgnoreField(string name)
        {
            _ignoreFields.Add(name);
        }

        protected void SaveFields(GameXmlWriter writer, Element e)
        {
            // reverse Types list when saving as it's a Stack, and we want to
            // save types in the same order as they were loaded
            foreach (var includedType in e.Fields.Types.Reverse())
            {
                if (!CanSaveTypeName(writer, includedType.Name, e))
                {
                    continue;
                }

                writer.WriteStartElement("inherit");
                writer.WriteAttributeString("name", includedType.Name);
                writer.WriteEndElement();
            }

            IEnumerable<string> fieldNames = e.Fields.FieldNames;
            if (writer.Mode != SaveMode.Editor)
            {
                fieldNames = fieldNames.Union(e.Fields.FieldExtensionNames);
            }

            foreach (var attribute in fieldNames)
            {
                if (!CanSaveAttribute(attribute, e))
                {
                    continue;
                }

                var value = e.Fields.Get(attribute);
                _fieldSaver?.Save(writer, e, attribute, value);
            }
        }

        protected virtual void Initialise()
        {
        }

        protected virtual bool CanSaveAttribute(string attribute, Element e)
        {
            return !_ignoreFields.Contains(attribute);
        }

        protected virtual bool CanSaveTypeName(GameXmlWriter writer, string type, Element e)
        {
            if (writer.Mode != SaveMode.Package)
            {
                return !WorldModel.DefaultTypeNames.ContainsValue(type);
            }

            if (e.WorldModel.Elements.Get(ElementType.ObjectType, type).MetaFields[MetaFieldDefinitions.EditorLibrary])
            {
                return false;
            }
            return !WorldModel.DefaultTypeNames.ContainsValue(type);
        }

        public abstract ElementType AppliesTo { get; }

        public abstract void Save(GameXmlWriter writer, Element e);

        public GameSaver GameSaver
        {
            get => _gameSaver!;
            set
            {
                _gameSaver = value;
                _fieldSaver = new FieldSaver(_gameSaver);
                Initialise();
            }
        }

        public virtual bool AutoSave => true;
    }
}