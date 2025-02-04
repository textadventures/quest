using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuestViva.Common;

namespace TextAdventures.Quest
{
    public sealed class Walkthroughs : IWalkthroughs
    {
        private Dictionary<string, IWalkthrough> m_walkthroughs = new Dictionary<string, IWalkthrough>();

        public Walkthroughs(WorldModel worldModel)
        {
            foreach (Element walkthroughElement in worldModel.Elements.GetElements(ElementType.Walkthrough))
            {
                m_walkthroughs.Add(walkthroughElement.Name, new Walkthrough(walkthroughElement, this));
            }
        }

        IDictionary<string, IWalkthrough> IWalkthroughs.Walkthroughs
        {
            get { return m_walkthroughs; }
        }
    }

    public class Walkthrough : IWalkthrough
    {
        private Element m_element;
        private IWalkthroughs m_walkthroughs;

        public Walkthrough(Element element, Walkthroughs walkthroughs)
        {
            m_element = element;
            m_walkthroughs = walkthroughs;
        }

        public List<string> Steps
        {
            get {
                if (m_element.Parent == null)
                {
                    return ThisSteps();
                }
                else
                {
                    List<string> result = new List<string>();
                    result.AddRange(m_walkthroughs.Walkthroughs[m_element.Parent.Name].Steps);
                    result.AddRange(ThisSteps());
                    return result;
                }
            }
        }

        private List<string> ThisSteps()
        {
            List<string> result = new List<string>();
            IEnumerable<string> steps = m_element.Fields[FieldDefinitions.Steps];
            if (steps != null)
            {
                result.AddRange(steps);
            }
            return result;
        }
    }
}
