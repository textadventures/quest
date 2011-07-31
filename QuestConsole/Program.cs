using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: QUESTCONSOLE.EXE <filename>");
                return;
            }

            string filename = args[0];
            Runner runner = new Runner(filename);
            runner.Start();
        }
    }
}
