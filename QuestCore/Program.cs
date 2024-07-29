using QuestCore;

if (args.Length == 0)
{
    Console.WriteLine("Usage: QUESTCORE.EXE <filename>");
    return;
}

string filename = args[0];
Runner runner = new Runner(filename);
runner.Start();