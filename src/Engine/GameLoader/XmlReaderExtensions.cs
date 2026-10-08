using System.Xml;

namespace QuestViva.Engine.GameLoader;

internal static class XmlReaderExtensions
{
    // Like ReadElementContentAsString, but leaves the reader on the element's own end tag rather
    // than on the node after it. Every read loop in the loader calls Read() next, so moving any
    // further skips whatever follows: harmless whitespace in a pretty-printed file, but the next
    // element, or the parent's closing tag, when it's all on one line. See issue #2345.
    public static string ReadElementContentLeavingEndTag(this XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Element)
        {
            throw new XmlException($"Expected an element, but found {reader.NodeType}");
        }

        if (reader.IsEmptyElement)
        {
            return string.Empty;
        }

        reader.Read();
        var content = reader.NodeType == XmlNodeType.EndElement ? string.Empty : reader.ReadContentAsString();

        if (reader.NodeType != XmlNodeType.EndElement)
        {
            throw new XmlException($"Unexpected element '{reader.Name}' inside a text-only element");
        }

        return content;
    }
}
