using System.Text;
using System.Xml;

namespace QuestViva.Engine;

internal interface IOutputLogger
{
    void Save(string? html);
    void Clear();
}

internal class OutputLogger : IOutputLogger
{
    private readonly WorldModel _worldModel;

    public OutputLogger(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public void Save(string? html)
    {
        var element = _worldModel.Elements.GetSingle(ElementType.Output) ??
                      _worldModel.GetElementFactory(ElementType.Output).Create();

        element.Fields.Set("html", html);
    }

    public void Clear()
    {
    }
}

internal class LegacyOutputLogger : IOutputLogger
{
    private readonly StringBuilder _text = new();
    private readonly WorldModel _worldModel;
    private bool _anyText;

    public LegacyOutputLogger(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public void Clear()
    {
        _text.Clear();
        _anyText = false;
    }

    public void Save(string? html)
    {
        var element = _worldModel.Elements.GetSingle(ElementType.Output);
        if (element == null)
        {
            element = _worldModel.GetElementFactory(ElementType.Output).Create();
        }

        element.Fields.Set("text", _text.ToString());
    }

    public void AddText(string text, bool linebreak = true)
    {
        if (_anyText)
        {
            _text.Append((linebreak ? "<br/>" : string.Empty) + Environment.NewLine + text);
        }
        else
        {
            _text.Append(text);
            _anyText = true;
        }
    }

    public void AddPicture(string filename)
    {
        _text.Append(string.Format("<output_picture filename=\"{0}\"/>", filename));
    }

    public void SetFontName(string? fontName)
    {
        _text.Append(string.Format("<output_setfontname name=\"{0}\"/>", fontName));
    }

    public void SetFontSize(string? fontSize)
    {
        _text.Append(string.Format("<output_setfontsize size=\"{0}\"/>", fontSize));
    }

    public async Task DisplayOutputAsync(string? text)
    {
        text = "<output>" + text + "</output>";
        var output = new StringBuilder();

        var settings = new XmlReaderSettings {IgnoreWhitespace = false};
        var reader = XmlReader.Create(new StringReader(text), settings);

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "output":
                            break;
                        case "output_picture":
                            if (output.Length > 0)
                            {
                                _worldModel.Print(output.ToString());
                                output.Clear();
                            }

                            var filename = reader.GetAttribute("filename");
                            if (filename != null)
                            {
                                await _worldModel.PlayerUi.ShowPictureAsync(filename);
                            }

                            break;
                        case "output_setfontsize":
                            if (output.Length > 0)
                            {
                                _worldModel.Print(output.ToString(), false);
                                output.Clear();
                            }

                            var size = reader.GetAttribute("size");
                            // WorldModel's OutputLogger is this instance whenever legacy output is being replayed
                            SetFontSize(size);
                            // Written by SetFontSize above, so always present
                            _worldModel.PlayerUi.SetFontSize(size!);
                            break;
                        case "output_setfontname":
                            if (output.Length > 0)
                            {
                                _worldModel.Print(output.ToString(), false);
                                output.Clear();
                            }

                            var name = reader.GetAttribute("name");
                            SetFontName(name);
                            _worldModel.PlayerUi.SetFont(name!);
                            break;
                        default:
                            output.Append("<" + reader.Name);
                            if (reader.HasAttributes)
                            {
                                for (var i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    output.Append(string.Format(" {0}=\"{1}\"", reader.Name, reader.Value));
                                }

                                reader.MoveToElement();
                            }

                            if (reader.IsEmptyElement)
                            {
                                output.Append("/>");
                            }
                            else
                            {
                                output.Append(">");
                            }

                            break;
                    }

                    break;
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                    output.Append(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    switch (reader.Name)
                    {
                        case "output":
                            break;
                        case "output_picture":
                            break;
                        default:
                            output.Append(string.Format("</{0}>", reader.Name));
                            break;
                    }

                    break;
            }
        }

        _worldModel.Print(output.ToString());
    }
}