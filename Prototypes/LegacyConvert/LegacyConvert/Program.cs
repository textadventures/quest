using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\LegacyGame.vb");
            var tree = VisualBasicSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var count = 0;

            foreach (var node in root.ChildNodes())
            {
                Console.WriteLine(string.Format("{0}\t{1}", count++, node.Kind()));

                if (node.Kind() == SyntaxKind.ClassBlock)
                {
                    foreach (var childNode in node.ChildNodes())
                    {
                        Console.WriteLine(string.Format("{0}\t{1}", count++, childNode.Kind()));
                        Console.WriteLine(childNode);
                        Console.WriteLine("************************\n\n\n");
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
