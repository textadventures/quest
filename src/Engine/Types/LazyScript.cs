namespace QuestViva.Engine.Types
{
    internal class LazyScript
    {
        public LazyScript(string script)
        {
            Script = script;
        }

        public string Script { get; private set; }
    }
}
