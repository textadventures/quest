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
            var input = new StringBuilder();
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\LegacyGame.vb"));
            input.Append("\n");
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\ChangeLog.vb"));
            input.Append("\n");
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\Config.vb"));
            input.Append("\n");
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\RoomExit.vb"));
            input.Append("\n");
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\RoomExits.vb"));
            input.Append("\n");
            input.Append(System.IO.File.ReadAllText(@"..\..\..\..\..\Legacy\TextFormatter.vb"));

            var source = input.ToString();
            source = System.Text.RegularExpressions.Regex.Replace(source, @"\'\<NOCONVERT.*?NOCONVERT\>", "", System.Text.RegularExpressions.RegexOptions.Singleline);

            var tree = VisualBasicSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var compilation = VisualBasicCompilation.Create("Legacy").AddSyntaxTrees(tree);
            Model = compilation.GetSemanticModel(tree);

            var prepend = new StringBuilder();
            prepend.Append(System.IO.File.ReadAllText(@"..\..\stub.ts"));

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
                case SyntaxKind.SubStatement:
                case SyntaxKind.EndSubStatement:
                case SyntaxKind.SubNewStatement:
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
                case SyntaxKind.SubBlock:
                    var block = (MethodBlockSyntax)node;
                    var name = block.SubOrFunctionStatement.Identifier.Text;

                    string type = "void";
                    if (block.SubOrFunctionStatement.AsClause != null)
                    {
                        type = GetVarType(block.SubOrFunctionStatement.AsClause.Type);
                    }

                    var parameters = block.SubOrFunctionStatement.ParameterList.Parameters.Select(p => ProcessParameter(p));

                    return string.Format("{0}{1}({4}): {2} {{\n{3}{0}}}\n",
                        Tabs(depth),
                        name,
                        type,
                        ProcessChildNodes(node, depth, prepend, false, classFields),
                        string.Join(", ", parameters));
                case SyntaxKind.ConstructorBlock:
                    var ctor = (ConstructorBlockSyntax)node;
                    var ctorParams = ((SubNewStatementSyntax)ctor.ChildNodes().First()).ParameterList.Parameters.Select(p => ProcessParameter(p));
                    return string.Format("{0}constructor({1}) {{\n{2}{0}}}\n",
                        Tabs(depth),
                        string.Join(", ", ctorParams),
                        ProcessChildNodes(node, depth, prepend, false, classFields));
                case SyntaxKind.SimpleAssignmentStatement:
                    var assign = (AssignmentStatementSyntax)node;
                    var left = ProcessExpression(assign.Left, classFields);
                    var right = ProcessExpression(assign.Right, classFields);
                    return string.Format("{0}{1} = {2};\n", Tabs(depth), left, right);
                case SyntaxKind.ReturnStatement:
                    var returnStatement = (ReturnStatementSyntax)node;
                    return string.Format("{0}return {1};\n", Tabs(depth), ProcessExpression(returnStatement.Expression, classFields));
                case SyntaxKind.ExpressionStatement:
                    return Tabs(depth) + ProcessExpression(((ExpressionStatementSyntax)node).Expression, classFields) + ";\n";
                default:
                    return string.Format("{0}// UNKNOWN {1}\n", Tabs(depth), node.Kind());
            }

            return null;
        }

        static string ProcessParameter(ParameterSyntax parameter)
        {
            var asType = parameter.AsClause.ChildNodes().First();
            var varType = GetVarType(asType);
            if (parameter.Default != null)
            {
                var defaultValue = ProcessExpression(parameter.Default.Value, null);
                return parameter.Identifier.Identifier.ValueText + ": " + varType + " = " + defaultValue;
            }
            return parameter.Identifier.Identifier.ValueText + ": " + varType;
        }

        static string ProcessExpression(ExpressionSyntax expr, List<string> classFields)
        {
            if (expr == null)
            {
                return "null";
            }

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
                if (classFields != null && classFields.Contains(name)) return "this." + name;
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
                var args = string.Join(", ", invocation.ArgumentList.Arguments.Select(a => ProcessExpression(a.GetExpression(), classFields)));
                var typeInfo = Model.GetTypeInfo(invocation.Expression);
                var identifier = invocation.Expression as IdentifierNameSyntax;
                if (identifier != null)
                {
                    var isArray = (typeInfo.Type != null && typeInfo.Type.Kind == SymbolKind.ArrayType);
                    if (!isArray)
                    {
                        // hmm, the above doesn't catch local variables which are arrays. Fudge it by checking for lowercase first letter.
                        var firstLetter = identifier.Identifier.ValueText.Substring(0, 1);
                        if (firstLetter.ToLower() == firstLetter) isArray = true;
                    }

                    var name = identifier.Identifier.ValueText;
                    if (classFields.Contains(name)) name = "this." + name;

                    if (isArray)
                    {
                        var arg = ProcessExpression(invocation.ArgumentList.Arguments[0].GetExpression(), classFields);
                        return name + "[" + arg + "]";
                    }

                    return string.Format("{0}({1})", name, args);
                }

                if (typeInfo.Type != null && typeInfo.Type.Kind == SymbolKind.ArrayType)
                {
                    return string.Format("{0}[{1}]", ProcessExpression(invocation.Expression, classFields), args);
                }

                return string.Format("{0}({1})", ProcessExpression(invocation.Expression, classFields), args);
            }

            var binary = expr as BinaryExpressionSyntax;
            if (binary != null)
            {
                var op = binary.OperatorToken.ValueText;
                if (op == "&") op = "+";
                if (op == "=") op = "==";
                return ProcessExpression(binary.Left, classFields) + " " + op + " " + ProcessExpression(binary.Right, classFields);
            }

            var unary = expr as UnaryExpressionSyntax;
            if (unary != null)
            {
                if (unary.OperatorToken.Text == "Not")
                {
                    return "!" + ProcessExpression(unary.Operand, classFields);
                }
                if (unary.OperatorToken.Text == "-")
                {
                    return "-" + ProcessExpression(unary.Operand, classFields);
                }
                throw new InvalidOperationException();
            }

            var paranthesized = expr as ParenthesizedExpressionSyntax;
            if (paranthesized != null)
            {
                return string.Format("({0})", ProcessExpression(paranthesized.Expression, classFields));
            }

            var arrayCreation = expr as ArrayCreationExpressionSyntax;
            if (arrayCreation != null)
            {
                return "[]";
            }

            var cast = expr as PredefinedCastExpressionSyntax;
            if (cast != null)
            {
                if (cast.Keyword.Text == "CInt")
                {
                    return string.Format("parseInt({0})", ProcessExpression(cast.Expression, classFields));
                }
                if (cast.Keyword.Text == "CStr")
                {
                    return string.Format("({0}).toString()", ProcessExpression(cast.Expression, classFields));
                }
                throw new InvalidOperationException();
            }

            var directCast = expr as DirectCastExpressionSyntax;
            if (directCast != null)
            {
                return ProcessExpression(directCast.Expression, classFields);
            }

            var predefinedType = expr as PredefinedTypeSyntax;
            if (predefinedType != null)
            {
                return predefinedType.Keyword.Text;
            }

            var ternary = expr as TernaryConditionalExpressionSyntax;
            if (ternary != null)
            {
                return string.Format("{0} ? {1} : {2}", ProcessExpression(ternary.Condition, classFields), ProcessExpression(ternary.WhenTrue, classFields), ProcessExpression(ternary.WhenFalse, classFields));
            }

            var me = expr as MeExpressionSyntax;
            if (me != null)
            {
                return "this";
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
                var type = GetVarType(objectCreationExpressionSyntax.Type);
                return type + " = " + (type == "any" ? "{}" : "new " + type + "()");
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
            if (type == "Byte") return "number";
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
            if (classFields != null && classFields.Count == 0)
            {
                foreach (var childNode in node.ChildNodes())
                {
                    var kind = childNode.Kind();
                    if (kind == SyntaxKind.FunctionBlock || kind == SyntaxKind.SubBlock)
                    {
                        var block = (MethodBlockSyntax)childNode;
                        var name = block.SubOrFunctionStatement.Identifier.Text;
                        classFields.Add(name);
                    }
                }
            }
            
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
