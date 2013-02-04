using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class AttributeSubEditorControlData:IEditorControl
    {
        private static Dictionary<string, string> s_allTypes = new Dictionary<string, string> {
            {"string","String"},
            {"boolean","Boolean"},
            {"int","Integer"},
            {"double","Double"},
            {"script","Script"},
            {"stringlist","String List"},
            {"object","Object"},
            {"simplepattern","Command pattern"},
            {"stringdictionary","String dictionary"},
            {"scriptdictionary","Script dictionary"},
            {"null","Null"}
        };

        private string m_attribute;

        public AttributeSubEditorControlData(string attribute)
        {
            m_attribute = attribute;
        }

        protected virtual Dictionary<string, string> AllowedTypes
        {
            get { return s_allTypes; }
        }

        public string Attribute
        {
            get { return m_attribute; }
        }

        public string Caption
        {
            get { return null; }
        }

        public string ControlType
        {
            get { return null; }
        }

        public bool Expand
        {
            get { return false; }
        }

        public bool GetBool(string tag)
        {
            return false;
        }

        public IDictionary<string, string> GetDictionary(string tag)
        {
            if (tag == "types")
            {
                return AllowedTypes;
            }
            if (tag == "editors")
            {
                return null;
            }
            throw new NotImplementedException();
        }

        public int? GetInt(string tag)
        {
            return null;
        }

        public double? GetDouble(string tag)
        {
            return null;
        }

        public IEnumerable<string> GetListString(string tag)
        {
            throw new NotImplementedException();
        }

        public virtual string GetString(string tag)
        {
            switch (tag)
            {
                case "checkbox":
                    return "True";
                case "editprompt":
                case "valueprompt":
                    return "Please enter a value";
                case "keyprompt":
                    return "Please enter a key";
                default:
                    return null;
            }
        }

        public int? Height
        {
            get { return null; }
        }

        public bool IsControlVisible(IEditorData data)
        {
            return true;
        }

        public int? Width
        {
            get { return null; }
        }

        public IEditorDefinition Parent
        {
            get { return null; }
        }

        public bool IsControlVisibleInSimpleMode
        {
            get { return true; }
        }

        public string Id
        {
            get { throw new NotImplementedException(); }
        }
    }
}
