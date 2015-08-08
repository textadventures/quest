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

            ProcessNode(root, 0);

            Console.ReadKey();
        }

        static void ProcessNode(SyntaxNode node, int depth)
        {
            var count = 0;
            foreach (var childNode in node.ChildNodes())
            {
                Console.WriteLine(string.Format("{0}{1} {2}", new string('\t', depth), count++, childNode.Kind()));
                ProcessNode(childNode, depth + 1);
            }
        }
    }
}
