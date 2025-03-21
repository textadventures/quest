#nullable disable
namespace QuestViva.Engine.Types
{
    internal class LazyObjectReference
    {
        public LazyObjectReference(string objectName)
        {
            ObjectName = objectName;
        }

        public string ObjectName { get; private set; }
    }
}
