using QuestViva.Common;

namespace QuestViva.Engine;

public sealed class Walkthroughs : IWalkthroughs
{
    private readonly Dictionary<string, IWalkthrough> _walkthroughs = new();

    public Walkthroughs(WorldModel worldModel)
    {
        foreach (var walkthroughElement in worldModel.Elements.GetElements(ElementType.Walkthrough))
        {
            _walkthroughs.Add(walkthroughElement.Name, new Walkthrough(walkthroughElement, this));
        }
    }

    IDictionary<string, IWalkthrough> IWalkthroughs.Walkthroughs => _walkthroughs;
}

public class Walkthrough : IWalkthrough
{
    private readonly Element _element;
    private readonly IWalkthroughs _walkthroughs;

    public Walkthrough(Element element, Walkthroughs walkthroughs)
    {
        _element = element;
        _walkthroughs = walkthroughs;
    }

    public string[] Steps
    {
        get
        {
            if (_element.Parent == null)
            {
                return ThisSteps();
            }

            var result = new List<string>();
            result.AddRange(_walkthroughs.Walkthroughs[_element.Parent.Name].Steps);
            result.AddRange(ThisSteps());
            return result.ToArray();
        }
    }

    private string[] ThisSteps()
    {
        var result = new List<string>();
        IEnumerable<string>? steps = _element.Fields[FieldDefinitions.Steps];
        if (steps != null)
        {
            result.AddRange(steps);
        }

        return result.ToArray();
    }
}