using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditorAttributeData
    {
        string AttributeName { get; }
        bool IsInherited { get; }
        string Source { get; }
        bool IsDefaultType { get; }
    }

    public interface IEditorData
    {
        string Name { get; }
        object GetAttribute(string attribute);
        void SetAttribute(string attribute, object value);
        IEnumerable<string> GetAffectedRelatedAttributes(string attribute);
        string GetSelectedFilter(string filterGroup);
        void SetSelectedFilter(string filterGroup, string filter);

        event EventHandler Changed;
    }

    public interface IEditorDataExtendedAttributeInfo : IEditorData
    {
        IEnumerable<IEditorAttributeData> GetAttributeData();
        IEditorAttributeData GetAttributeData(string attribute);
        void RemoveAttribute(string attribute);
        IEnumerable<IEditorAttributeData> GetInheritedTypes();
        bool IsLibraryElement { get; }
        string Filename { get; }
        void MakeElementLocal();
    }
}
