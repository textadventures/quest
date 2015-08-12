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

            var prepend = new StringBuilder();

            var result = ProcessNode(root, -1, prepend);
            result = prepend.ToString() + result;

            System.IO.File.WriteAllText("output.ts", result);

            Console.WriteLine("Done " + result.Length);
            Console.ReadKey();
        }

        static string ProcessNode(SyntaxNode node, int depth, StringBuilder prepend)
        {
            switch (node.Kind())
            {
                case SyntaxKind.CompilationUnit:
                    return ProcessChildNodes(node, depth, prepend);
                case SyntaxKind.OptionStatement:
                case SyntaxKind.ImportsStatement:
                case SyntaxKind.ClassStatement:
                case SyntaxKind.EndClassStatement:
                case SyntaxKind.ImplementsStatement:
                case SyntaxKind.EnumStatement:
                case SyntaxKind.EndEnumStatement:
                    // ignore;
                    break;
                case SyntaxKind.ClassBlock:
                    var className = ((ClassStatementSyntax)node.ChildNodes().First()).Identifier.Text;
                    var classResult = string.Format("class {0} {{\n{1}}}\n", className, ProcessChildNodes(node, depth, prepend));
                    if (depth == 0) return classResult;
                    prepend.Append(classResult);
                    return null;
                case SyntaxKind.EnumBlock:
                    var enumName = ((EnumStatementSyntax)node.ChildNodes().First()).Identifier.Text;
                    var values = node.ChildNodes().OfType<EnumMemberDeclarationSyntax>().Select(n => n.Initializer == null ? SafeName(n.Identifier.Text) : SafeName(n.Identifier.Text) + " = " + n.Initializer.Value);
                    prepend.Append(string.Format("enum {0} {{{1}}};\n", enumName, string.Join(", ", values)));
                    return null;
                case SyntaxKind.EnumMemberDeclaration:
                    return ((EnumMemberDeclarationSyntax)node).Identifier.Text;
                default:
                    return string.Format("{0}// UNKNOWN {1}\n", Tabs(depth), node.Kind());
            }

            return null;
        }

        private static string SafeName(string name)
        {
            // remove [ and ] from around name if necessary
            if (name.StartsWith("[")) name = name.Substring(1, name.Length - 2);
            return name;
        }

        static string ProcessChildNodes(SyntaxNode node, int depth, StringBuilder prepend)
        {
            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes())
            {
                sb.Append(ProcessNode(childNode, depth + 1, prepend));
            }
            return sb.ToString();
        }

        static string Tabs(int depth)
        {
            return new string('\t', depth);
        }
    }
}
