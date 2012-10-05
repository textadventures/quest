using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
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
        ValidationResult SetAttribute(string attribute, object value);
        IEnumerable<string> GetAffectedRelatedAttributes(string attribute);
        string GetSelectedFilter(string filterGroup);
        void SetSelectedFilter(string filterGroup, string filter);
        bool ReadOnly { get; set; }
        IEnumerable<string> GetVariablesInScope();

        // Usually set to True, but set to false for ExpressionTemplateEditorData as any update
        // to an expression template parameter can only be persisted by saving the entire parent
        // expression.
        bool IsDirectlySaveable { get; }

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
