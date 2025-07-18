using System;
using System.Text;
using System.Xml;

namespace QuestViva.Engine.GameLoader;

internal class GameXmlWriter
{
    internal class GameXmlWriterOptions
    {
        public bool IncludeWalkthrough { get; init; }
    }

    private readonly StringBuilder _output;
    private readonly XmlWriter _writer;

    public const string IndentChars = "  ";

    public GameXmlWriter(SaveMode mode, GameXmlWriterOptions? options = null)
    {
        Mode = mode;
        _output = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = IndentChars,
            OmitXmlDeclaration = true
        };
        _writer = XmlWriter.Create(_output, settings);
        options ??= new GameXmlWriterOptions
        {
            IncludeWalkthrough = (mode != SaveMode.Package)
        };
        Options = options;
    }

    public void WriteComment(string text)
    {
        _writer.WriteComment(text);
    }

    public void WriteStartElement(string localName)
    {
        _writer.WriteStartElement(localName);
        IndentLevel++;
    }

    public void WriteEndElement()
    {
        _writer.WriteEndElement();
        IndentLevel--;
    }

    public void WriteAttributeString(string localName, string? value)
    {
        _writer.WriteAttributeString(localName, value);
    }

    public void WriteString(string text)
    {
        // When writing a string containing newlines, the XmlWriter won't indent the ending tag at all.
        // So, add the right amount of indenting to the end of the string, so that the ending tag will
        // be indented correctly.
        if (text.Contains(Environment.NewLine))
        {
            for (int i = 0; i < IndentLevel - 1; i++)
            {
                text += IndentChars;
            }
        }
        if (UseCDATA(text))
        {
            _writer.WriteCData(text);
        }
        else
        {
            _writer.WriteString(text);
        }
    }

    private static bool UseCDATA(string input)
    {
        return input.Contains('>') || input.Contains('<') || input.Contains('&');
    }

    public void WriteElementString(string localName, string? value)
    {
        _writer.WriteElementString(localName, value);
    }

    public void Close()
    {
        _writer.Close();
    }

    public override string ToString()
    {
        return _output.ToString();
    }

    public int IndentLevel { get; private set; }

    public SaveMode Mode { get; }

    public GameXmlWriterOptions Options { get; }
}