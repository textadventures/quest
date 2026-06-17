#nullable disable
using QuestViva.Common;

namespace QuestViva.Engine;

public sealed class Walkthroughs : IWalkthroughs
{
    private readonly Dictionary<string, IWalkthrough> m_walkthroughs = new();

    public Walkthroughs(WorldModel worldModel)
    {
        foreach (var walkthroughElement in worldModel.Elements.GetElements(ElementType.Walkthrough))
        {
            m_walkthroughs.Add(walkthroughElement.Name, new Walkthrough(walkthroughElement, this));
        }
    }

    IDictionary<string, IWalkthrough> IWalkthroughs.Walkthroughs => m_walkthroughs;
}

public class Walkthrough : IWalkthrough
{
    private readonly Element m_element;
    private readonly IWalkthroughs m_walkthroughs;

    public Walkthrough(Element element, Walkthroughs walkthroughs)
    {
        m_element = element;
        m_walkthroughs = walkthroughs;
    }

    public string[] Steps
    {
        get
        {
            if (m_element.Parent == null)
            {
                return ThisSteps();
            }

            var result = new List<string>();
            result.AddRange(m_walkthroughs.Walkthroughs[m_element.Parent.Name].Steps);
            result.AddRange(ThisSteps());
            return result.ToArray();
        }
    }

    private string[] ThisSteps()
    {
        var result = new List<string>();
        IEnumerable<string> steps = m_element.Fields[FieldDefinitions.Steps];
        if (steps != null)
        {
            result.AddRange(steps);
        }

        return result.ToArray();
    }
}