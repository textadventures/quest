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
        static SemanticModel Model;

        static void Main(string[] args)
        {
            var text = System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\LegacyGame.vb");
            var tree = VisualBasicSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var compilation = VisualBasicCompilation.Create("Legacy").AddSyntaxTrees(tree);
            Model = compilation.GetSemanticModel(tree);

            var prepend = new StringBuilder();

            var result = ProcessNode(root, -1, prepend, false);
            result = prepend.ToString() + result;

            System.IO.File.WriteAllText(@"..\..\output.ts", result);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "tsc.exe",
                UseShellExecute = false,
                Arguments = @"..\..\output.ts",
            });

            Console.WriteLine("Done " + result.Length);
            Console.ReadKey();
        }

        static string ProcessNode(SyntaxNode node, int depth, StringBuilder prepend, bool inClass, List<string> classFields = null)
        {
            switch (node.Kind())
            {
                case SyntaxKind.CompilationUnit:
                    return ProcessChildNodes(node, depth, prepend, false, classFields);
                case SyntaxKind.OptionStatement:
                case SyntaxKind.ImportsStatement:
                case SyntaxKind.ClassStatement:
                case SyntaxKind.EndClassStatement:
                case SyntaxKind.ImplementsStatement:
                case SyntaxKind.EnumStatement:
                case SyntaxKind.EndEnumStatement:
                case SyntaxKind.FunctionStatement:
                case SyntaxKind.EndFunctionStatement:
                    // ignore;
                    break;
                case SyntaxKind.ClassBlock:
                    var className = ((ClassStatementSyntax)node.ChildNodes().First()).Identifier.Text;
                    var classResult = string.Format("class {0} {{\n{1}}}\n", className, ProcessChildNodes(node, depth, prepend, true, new List<string>()));
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
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.LocalDeclarationStatement:
                    var variables = (VariableDeclaratorSyntax)node.ChildNodes().First();
                    var names = variables.Names.Select(n => n.Identifier.Text);

                    if (inClass) classFields.AddRange(names);

                    string initializer = null;

                    if (variables.Initializer != null)
                    {
                        var eq = variables.Initializer as EqualsValueSyntax;
                        initializer = " = " + ProcessExpression(eq.Value, classFields);
                    }

                    if (variables.AsClause == null)
                    {
                        return Tabs(depth) + "var " + string.Join(", ", names) + initializer + ";\n";
                    }
                    else
                    {
                        var asType = variables.AsClause.ChildNodes().First();
                        var varType = GetVarType(asType);

                        var lines = names.Select(n => string.Format("{2}{0}: {1}{3};", n, varType, !inClass ? "var " : "", initializer));
                        return string.Join("\n", lines.Select(l => Tabs(depth) + l)) + "\n";
                    }
                case SyntaxKind.FunctionBlock:
                    var block = (MethodBlockSyntax)node;
                    var name = block.SubOrFunctionStatement.Identifier.Text;
                    var type = GetVarType(block.SubOrFunctionStatement.AsClause.Type);
                    // TODO: Need the parameters
                    return string.Format("{0}{1}(): {2} {{\n{3}{0}}}\n", Tabs(depth), name, type, ProcessChildNodes(node, depth, prepend, false, classFields));
                default:
                    return string.Format("{0}// UNKNOWN {1}\n", Tabs(depth), node.Kind());
            }

            return null;
        }

        static string ProcessExpression(ExpressionSyntax expr, List<string> classFields)
        {
            var objectCreation = expr as ObjectCreationExpressionSyntax;
            if (objectCreation != null)
            {
                var type = GetVarType(objectCreation.Type);
                return "new " + type + "()";
            }

            var memberAccess = expr as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                return ProcessExpression(memberAccess.Expression, classFields) + "." + memberAccess.Name.Identifier.Text;
            }

            var identifierName = expr as IdentifierNameSyntax;
            if (identifierName != null)
            {
                var name = identifierName.Identifier.Text;
                if (classFields.Contains(name)) return "this." + name;
                return name;
            }

            var literal = expr as LiteralExpressionSyntax;
            if (literal != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(literal.Token.Value);
            }

            var invocation = expr as InvocationExpressionSyntax;
            if (invocation != null)
            {
                var identifier = invocation.Expression as IdentifierNameSyntax;
                if (identifier != null)
                {
                    var typeInfo = Model.GetTypeInfo(identifier);
                    if (typeInfo.Type != null && typeInfo.Type.Kind == SymbolKind.ArrayType)
                    {
                        var arg = ProcessExpression(invocation.ArgumentList.Arguments[0].GetExpression(), classFields);
                        var name = identifier.Identifier.ValueText;
                        if (classFields.Contains(name)) name = "this." + name;
                        return name + "[" + arg + "]";
                    }
                }

                return "'expr'";
            }

            var binary = expr as BinaryExpressionSyntax;
            if (binary != null)
            {
                var op = binary.OperatorToken.ValueText;
                if (op == "&") op = "+";
                return ProcessExpression(binary.Left, classFields) + " " + op + " " + ProcessExpression(binary.Right, classFields);
            }

            var collectionInitializer = expr as CollectionInitializerSyntax;
            if (collectionInitializer != null)
            {
                return "'expr'";
            }

            var paranthesized = expr as ParenthesizedExpressionSyntax;
            if (paranthesized != null)
            {
                return "'expr'";
            }

            var arrayCreation = expr as ArrayCreationExpressionSyntax;
            if (arrayCreation != null)
            {
                return "[]";
            }

            throw new InvalidOperationException();
        }

        private static string GetVarType(SyntaxNode asType)
        {
            var predefinedType = asType as PredefinedTypeSyntax;
            if (predefinedType != null)
            {
                return ConvertType(predefinedType.Keyword.Text);
            }

            var genericName = asType as GenericNameSyntax;
            if (genericName != null)
            {
                return "any";
            }

            var identifierName = asType as IdentifierNameSyntax;
            if (identifierName != null)
            {
                return identifierName.Identifier.Text;
            }

            var objectCreationExpressionSyntax = asType as ObjectCreationExpressionSyntax;
            if (objectCreationExpressionSyntax != null)
            {
                // TODO - need to get type and instantiate it
                return "any";
            }

            var arrayTypeSyntax = asType as ArrayTypeSyntax;
            if (arrayTypeSyntax != null)
            {
                return GetVarType(arrayTypeSyntax.ElementType) + "[]";
            }

            var qualifiedNameSyntax = asType as QualifiedNameSyntax;
            if (qualifiedNameSyntax != null)
            {
                // this is things like System.IO.Stream which will need to be removed
                return "any";
            }

            throw new InvalidOperationException();
        }

        private static string ConvertType(string type)
        {
            if (type == "Integer") return "number";
            if (type == "Double") return "number";
            if (type == "String") return "string";
            if (type == "Boolean") return "boolean";
            return type;
        }

        private static string SafeName(string name)
        {
            // remove [ and ] from around name if necessary
            if (name.StartsWith("[")) name = name.Substring(1, name.Length - 2);
            return name;
        }

        static string ProcessChildNodes(SyntaxNode node, int depth, StringBuilder prepend, bool inClass, List<string> classFields)
        {
            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes())
            {
                sb.Append(ProcessNode(childNode, depth + 1, prepend, inClass, classFields));
            }
            return sb.ToString();
        }

        static string Tabs(int depth)
        {
            return new string('\t', depth);
        }
    }
}
