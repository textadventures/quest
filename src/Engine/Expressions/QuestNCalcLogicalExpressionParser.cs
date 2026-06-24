// Based on https://github.com/ncalc/ncalc/blob/master/src/NCalc.Core/Parser/LogicalExpressionParser.cs

using ExtendedNumerics;
using NCalc;
using NCalc.Exceptions;
using NCalc.Parser;
using Parlot;
using Parlot.Fluent;
using Identifier = NCalc.Identifier;

namespace QuestViva.Engine.Expressions;

/// <summary>
///     Class responsible for parsing strings into <see cref="LogicalExpression" /> objects.
/// </summary>
public static class QuestNCalcLogicalExpressionParser
{
    private const string InvalidTokenMessage = "Invalid token in expression";
    private static readonly Parser<LogicalExpression> Parser;

    private static readonly ValueExpression True = new(true);
    private static readonly ValueExpression False = new(false);

    static QuestNCalcLogicalExpressionParser()
    {
        /*
         * Grammar:
         * expression     => ternary ( ( "-" | "+" ) ternary )* ;
         * ternary        => logical ( "?" logical ":" logical)?
         * logical        => equality ( ( "and" | "or" | "xor" ) equality )* ;
         * equality       => relational ( ( "=" | "!=" | ... ) relational )* ;
         * relational     => shift ( ( ">=" | ">" | ... ) shift )* ;
         * shift          => additive ( ( "<<" | ">>" ) additive )* ;
         * additive       => multiplicative ( ( "-" | "+" ) multiplicative )* ;
         * multiplicative => exponential ( "/" | "*" | "%") exponential )* ;
         * exponential    => postfix ( "^" postfix )* ;
         * postfix        => primary ( "[" expression "]" | "." identifier "(" arguments ")" )* ;
         * unary          => ( "-" | "not" | "!" ) primary
         *
         * primary        => NUMBER
         *                  | STRING
         *                  | "true"
         *                  | "false"
         *                  | ("[" | "{") anything ("]" | "}")
         *                  | function
         *                  | list
         *                  | "(" expression ")" ;
         *
         * function       => Identifier "(" arguments ")"
         * arguments      => expression ( ("," | ";") expression )*
         */
        // The Deferred helper creates a parser that can be referenced by others before it is defined
        var expression = Deferred<LogicalExpression>();

        var exponentNumberPart =
            Literals.Text("e", true).SkipAnd(Literals.Integer())
                .ThenElse<long?>(x => x, null);

        // [integral_value]['.'decimal_value}]['e'exponent_value]
        var number =
            SkipWhiteSpace(OneOf(
                    Literals.Char('.')
                        .SkipAnd(ZeroOrMany(Literals.Char('0')).ThenElse(x => x.Count, 0)
                            .And(ZeroOrOne(Literals.Integer(NumberOptions.None)).ThenElse<long?>(x => x, 0))
                            .Then(x =>
                            {
                                if (x is {Item1: 0, Item2: 0})
                                {
                                    throw new NCalcParserException(InvalidTokenMessage);
                                }

                                return (x.Item1, x.Item2);
                            }))
                        .And(exponentNumberPart)
                        .Then(x => (0L, x.Item1.Item1, x.Item1.Item2, x.Item2)),
                    Literals.Integer()
                        .And(Literals.Char('.')
                            .SkipAnd(ZeroOrMany(Literals.Char('0')).ThenElse(x => x.Count, 0))
                            .And(ZeroOrOne(Literals.Integer(NumberOptions.None)))
                            .ThenElse<(int, long?)>(x => (x.Item1, x.Item2), (0, null)))
                        .And(exponentNumberPart)
                        .Then(x => (x.Item1, x.Item2.Item1, x.Item2.Item2, x.Item3))
                ))
                .Then<LogicalExpression>((ctx, x) =>
                {
                    var integralValue = x.Item1;
                    var zeroCount = x.Item2;
                    var decimalPart = x.Item3;
                    var exponentPart = x.Item4;

                    double result = integralValue;

                    // decimal part?
                    if (decimalPart != null && decimalPart.Value != 0)
                    {
                        var digits = Math.Floor(Math.Log10(decimalPart.Value) + 1) + zeroCount;
                        result += decimalPart.Value / Math.Pow(10, digits);
                    }

                    // exponent part?
                    if (exponentPart != null)
                    {
                        var left = BigDecimal.Parse(result);
                        var right = BigDecimal.Pow(10, exponentPart.Value);

                        var res = BigDecimal.Multiply(left, right);

                        if (res > double.MaxValue)
                        {
                            result = double.PositiveInfinity;
                        }
                        else if (res < double.MinValue)
                        {
                            result = double.NegativeInfinity;
                        }
                        else
                        {
                            result = (double) res;
                        }
                    }

                    if (((LogicalExpressionParserContext) ctx).ParserOptions.DecimalAsDefault)
                    {
                        return new ValueExpression((decimal) result);
                    }

                    if (decimalPart != null || exponentPart != null)
                    {
                        return new ValueExpression(result);
                    }

                    return new ValueExpression((long) result);
                });

        var comma = Terms.Char(',');
        var divided = Terms.Text("/");
        var times = Terms.Text("*");
        var modulo = Terms.Text("%");
        var minus = Terms.Text("-");
        var plus = Terms.Text("+");

        var equal = OneOf(Terms.Text("=="), Terms.Text("="));
        var notEqual = OneOf(Terms.Text("<>"), Terms.Text("!="));
        var @in = Terms.Text("in", true);
        var notIn = Terms.Text("not in", true);

        var greater = Terms.Text(">");
        var greaterOrEqual = Terms.Text(">=");
        var lesser = Terms.Text("<");
        var lesserOrEqual = Terms.Text("<=");

        var leftShift = Terms.Text("<<");
        var rightShift = Terms.Text(">>");

        var exponent = Terms.Text("^"); // FLEE's exponentiation operator
        var openParen = Terms.Char('(');
        var closeParen = Terms.Char(')');
        var dot = Terms.Char('.');
        var openBrace = Terms.Char('[');
        var closeBrace = Terms.Char(']');
        var openCurlyBrace = Terms.Char('{');
        var closeCurlyBrace = Terms.Char('}');
        var questionMark = Terms.Char('?');
        var colon = Terms.Char(':');
        var semicolon = Terms.Char(';');

        var identifier = Terms.Identifier();

        var not = OneOf(
            Terms.Text("NOT", true).AndSkip(OneOf(Literals.WhiteSpace().Or(Not(AnyCharBefore(openParen))))),
            Terms.Text("!"));
        var and = OneOf(Terms.Text("AND", true), Terms.Text("&&"));
        var or = OneOf(Terms.Text("OR", true), Terms.Text("||"));

        var bitwiseAnd = Terms.Text("&");
        var bitwiseOr = Terms.Text("|");
        var bitwiseNot = Terms.Text("~");

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen);

        var braceIdentifier = openBrace
            .SkipAnd(AnyCharBefore(closeBrace, consumeDelimiter: true, failOnEof: true).ElseError("Brace not closed."));

        var curlyBraceIdentifier =
            openCurlyBrace.SkipAnd(AnyCharBefore(closeCurlyBrace, consumeDelimiter: true, failOnEof: true)
                .ElseError("Brace not closed."));

        // ("[" | "{") identifier ("]" | "}")
        var identifierExpression = OneOf(
                braceIdentifier,
                curlyBraceIdentifier,
                identifier)
            .Then<LogicalExpression>(x => new Identifier(x.ToString()!));

        // list => "(" (expression ("," expression)*)? ")"
        var populatedList =
            Between(openParen, Separated(comma.Or(semicolon), expression),
                    closeParen.ElseError("Parenthesis not closed."))
                .Then<LogicalExpression>(values => new LogicalExpressionList(values.ToList()));

        var emptyList = openParen.AndSkip(closeParen).Then<LogicalExpression>(_ => new LogicalExpressionList());

        var list = OneOf(emptyList, populatedList);

        var function = identifier
            .And(list)
            .Then<LogicalExpression>(x =>
                new Function(new Identifier(x.Item1.ToString()!), (LogicalExpressionList) x.Item2));

        var booleanTrue = Terms.Text("true", true)
            .Then<LogicalExpression>(True);
        var booleanFalse = Terms.Text("false", true)
            .Then<LogicalExpression>(False);

        var singleQuotesStringValue =
            Terms.String(StringLiteralQuotes.Single)
                .Then<LogicalExpression>((ctx, value) =>
                {
                    if (value.Length == 1 &&
                        ((LogicalExpressionParserContext) ctx).ParserOptions.AllowCharValues)
                    {
                        return new ValueExpression(value.Span[0]);
                    }

                    return new ValueExpression(value.ToString()!);
                });

        var doubleQuotesStringValue =
            Terms
                .String(StringLiteralQuotes.Double)
                .Then<LogicalExpression>(value => new ValueExpression(value.ToString()!));

        var stringValue = OneOf(singleQuotesStringValue, doubleQuotesStringValue);

        var charIsNumber = Literals.Pattern(char.IsNumber);

        var dateDefinition = charIsNumber
            .AndSkip(divided)
            .And(charIsNumber)
            .AndSkip(divided)
            .And(charIsNumber);

        // date => number/number/number
        var date = dateDefinition.Then<LogicalExpression>(date =>
        {
            if (DateTime.TryParse($"{date.Item1}/{date.Item2}/{date.Item3}", out var result))
            {
                return new ValueExpression(result);
            }

            throw new FormatException("Invalid DateTime format.");
        });

        // time => number:number:number
        var timeDefinition = charIsNumber
            .AndSkip(colon)
            .And(charIsNumber)
            .AndSkip(colon)
            .And(charIsNumber);

        var time = timeDefinition.Then<LogicalExpression>(time =>
        {
            if (TimeSpan.TryParse($"{time.Item1}:{time.Item2}:{time.Item3}", out var result))
            {
                return new ValueExpression(result);
            }

            throw new FormatException("Invalid TimeSpan format.");
        });

        // dateAndTime => number/number/number number:number:number
        var dateAndTime = dateDefinition.AndSkip(Literals.WhiteSpace()).And(timeDefinition)
            .Then<LogicalExpression>(dateTime =>
            {
                if (DateTime.TryParse(
                        $"{dateTime.Item1}/{dateTime.Item2}/{dateTime.Item3} {dateTime.Item4.Item1}:{dateTime.Item4.Item2}:{dateTime.Item4.Item3}",
                        out var result))
                {
                    return new ValueExpression(result);
                }

                throw new FormatException("Invalid DateTime format.");
            });

        // datetime => '#' dateAndTime | date | time  '#';
        var dateTime = Terms
            .Char('#')
            .SkipAnd(OneOf(dateAndTime, date, time))
            .AndSkip(Literals.Char('#'));

        var decimalNumber = Terms.Number<decimal>()
            .Then<LogicalExpression>(d => new ValueExpression(d));
        var doubleNumber = Terms.Number<double>(NumberOptions.Float)
            .Then<LogicalExpression>(d => new ValueExpression(d));

        var decimalOrDoubleNumber = OneOf(decimalNumber, doubleNumber);

        var eightHexSequence = Terms
            .Pattern(Character.IsHexDigit, 8, 8);

        var fourHexSequence = Terms
            .Pattern(Character.IsHexDigit, 4, 4);

        var twelveHexSequence = Terms
            .Pattern(Character.IsHexDigit, 12, 12);

        var thirtyTwoHexSequence = Terms
            .Pattern(Character.IsHexDigit, 32, 32);

        var guidWithHyphens = eightHexSequence
            .AndSkip(minus)
            .And(fourHexSequence)
            .AndSkip(minus)
            .And(fourHexSequence)
            .AndSkip(minus)
            .And(fourHexSequence)
            .AndSkip(minus)
            .And(twelveHexSequence)
            .Then<LogicalExpression>(g =>
                new ValueExpression(Guid.Parse(g.Item1.ToString() + g.Item2 + g.Item3 + g.Item4 + g.Item5)));

        var guidWithoutHyphens = thirtyTwoHexSequence
            .AndSkip(Not(decimalOrDoubleNumber))
            .Then<LogicalExpression>(g => new ValueExpression(Guid.Parse(g.ToString()!)));

        var guid = OneOf(guidWithHyphens, guidWithoutHyphens);

        // primary => GUID | NUMBER | identifier| DateTime | string | function | boolean | groupExpression | list ;
        var primary = OneOf(
            guid,
            number,
            decimalOrDoubleNumber,
            booleanTrue,
            booleanFalse,
            dateTime,
            stringValue,
            function,
            groupExpression,
            identifierExpression,
            list);

        // postfix => primary ( "[" expression "]" | "." identifier "(" args ")" )* ;
        // Handles Quest's list/dictionary subscript syntax: list[0], dict[key], func()[0]
        // and instance method call syntax: str.StartsWith("x"), func().Method(args)
        var subscriptSuffix = openBrace.SkipAnd(expression).AndSkip(closeBrace)
            .Then<Func<LogicalExpression, LogicalExpression>>(index =>
                receiver => new Function(new Identifier("__Quest_Index__"),
                    new LogicalExpressionList([receiver, index])));

        var methodCallSuffix = dot.SkipAnd(identifier).And(list)
            .Then<Func<LogicalExpression, LogicalExpression>>(x =>
            {
                var methodName = x.Item1.ToString()!;
                var args = (LogicalExpressionList)x.Item2;
                return receiver => new Function(new Identifier("__Quest_MethodCall__"),
                    new LogicalExpressionList([receiver, new ValueExpression(methodName), ..args]));
            });

        var postfix = primary.And(ZeroOrMany(OneOf(subscriptSuffix, methodCallSuffix)))
            .Then<LogicalExpression>(x =>
            {
                var result = x.Item1;
                foreach (var transform in x.Item2)
                {
                    result = transform(result);
                }
                return result;
            });

        // Deferred so exponential can reference unary (its right operand) before unary is defined.
        var unary = Deferred<LogicalExpression>();

        // exponential => unary ( "^" unary )* ;
        // Right operand is unary (not postfix) so that e.g. "e ^ -x" parses correctly.
        var exponential = postfix.And(ZeroOrMany(exponent.And(unary)))
            .Then(static x =>
            {
                LogicalExpression result = null!;

                if (x.Item2.Count == 0)
                {
                    result = x.Item1;
                }
                else if (x.Item2.Count == 1)
                {
                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, x.Item2[0].Item2);
                }
                else
                {
                    for (var i = x.Item2.Count - 1; i > 0; i--)
                    {
                        result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item2[i - 1].Item2,
                            x.Item2[i].Item2);
                    }

                    result = new BinaryExpression(BinaryExpressionType.Exponentiation, x.Item1, result);
                }

                return result;
            });

        // ( "-" | "~" ) unary | exponential;
        // This is where Quest's parser differs from the NCalc default, as we handle "not" further down.
        unary.Parser = exponential.Unary(
            (minus, value => new UnaryExpression(UnaryExpressionType.Negate, value)),
            (bitwiseNot, value => new UnaryExpression(UnaryExpressionType.BitwiseNot, value))
        );

        // multiplicative => unary ( ( "/" | "*" | "%" ) unary )* ;
        var multiplicative = unary.LeftAssociative(
            (divided, static (a, b) => new BinaryExpression(BinaryExpressionType.Div, a, b)),
            (times, static (a, b) => new BinaryExpression(BinaryExpressionType.Times, a, b)),
            (modulo, static (a, b) => new BinaryExpression(BinaryExpressionType.Modulo, a, b))
        );

        // additive => multiplicative ( ( "-" | "+" ) multiplicative )* ;
        var additive = multiplicative.LeftAssociative(
            (plus, static (a, b) => new BinaryExpression(BinaryExpressionType.Plus, a, b)),
            (minus, static (a, b) => new BinaryExpression(BinaryExpressionType.Minus, a, b))
        );

        // shift => additive ( ( "<<" | ">>" ) additive )* ;
        var shift = additive.LeftAssociative(
            (leftShift, static (a, b) => new BinaryExpression(BinaryExpressionType.LeftShift, a, b)),
            (rightShift, static (a, b) => new BinaryExpression(BinaryExpressionType.RightShift, a, b))
        );

        // relational => shift ( ( ">=" | "<=" | "<" | ">" | "in" | "not in" ) shift )* ;
        var relational = shift.And(ZeroOrMany(OneOf(
                    greaterOrEqual.Then(BinaryExpressionType.GreaterOrEqual),
                    lesserOrEqual.Then(BinaryExpressionType.LesserOrEqual),
                    lesser.Then(BinaryExpressionType.Lesser),
                    greater.Then(BinaryExpressionType.Greater),
                    @in.Then(BinaryExpressionType.In),
                    notIn.Then(BinaryExpressionType.NotIn)
                )
                .And(shift)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            });

        var equality = relational.And(ZeroOrMany(OneOf(
                    equal.Then(BinaryExpressionType.Equal),
                    notEqual.Then(BinaryExpressionType.NotEqual))
                .And(relational)))
            .Then(static x =>
            {
                var result = x.Item1;
                foreach (var op in x.Item2)
                {
                    result = new BinaryExpression(op.Item1, result, op.Item2);
                }

                return result;
            });

        // This is where Quest's parser differs from the NCalc default, as we want to handle
        // "not" here, rather than earlier. This is because we need to handle expressions like
        // "not my_object = null" as "not (my_object = null)" instead of "(not my_object) = null".
        var notOperator = equality.Unary(
            (not, value => new UnaryExpression(UnaryExpressionType.Not, value))
        );

        var andParser = and.Then<Func<LogicalExpression, LogicalExpression, LogicalExpression>>(
                _ => (a, b) => new BinaryExpression(BinaryExpressionType.And, a, b))
            .Or(bitwiseAnd.Then<Func<LogicalExpression, LogicalExpression, LogicalExpression>>(
                _ => (a, b) => new BinaryExpression(BinaryExpressionType.BitwiseAnd, a, b)));

        var orParser = or.Then<Func<LogicalExpression, LogicalExpression, LogicalExpression>>(
                _ => (a, b) => new BinaryExpression(BinaryExpressionType.Or, a, b))
            .Or(bitwiseOr.Then<Func<LogicalExpression, LogicalExpression, LogicalExpression>>(
                _ => (a, b) => new BinaryExpression(BinaryExpressionType.BitwiseOr, a, b)));

        // "xor" text keyword → logical XOR: (a or b) and not (a and b), to avoid type issues
        // with BitwiseXOr which returns UInt64 when applied to booleans.
        // Note: "^" is exponentiation (FLEE compat), not XOR.
        var xorParser = Terms.Text("XOR", true).Then<Func<LogicalExpression, LogicalExpression, LogicalExpression>>(
            _ => (a, b) =>
            {
                var aOrB = new BinaryExpression(BinaryExpressionType.Or, a, b);
                var aAndB = new BinaryExpression(BinaryExpressionType.And, a, b);
                return new BinaryExpression(BinaryExpressionType.And, aOrB,
                    new UnaryExpression(UnaryExpressionType.Not, aAndB));
            });

        // logical => equality ( ( "and" | "or" | "xor" ) equality )* ;
        var logical = notOperator.And(ZeroOrMany(OneOf(andParser, orParser, xorParser).And(notOperator)))
            .Then(x =>
            {
                var result = x.Item1;
                foreach (var (combiner, right) in x.Item2)
                {
                    result = combiner(result, right);
                }

                return result;
            });

        // ternary => logical("?" logical ":" logical) ?
        var ternary = logical.And(ZeroOrOne(questionMark.SkipAnd(logical).AndSkip(colon).And(logical)))
            .Then(x => x.Item2.Item1 == null
                ? x.Item1
                : new TernaryExpression(x.Item1, x.Item2.Item1, x.Item2.Item2))
            .Or(logical);

        var operatorSequence = ternary.LeftAssociative(
            (OneOrMany(OneOf(
                    divided, times, modulo, plus,
                    minus, leftShift, rightShift, greaterOrEqual,
                    lesserOrEqual, greater, lesser, equal,
                    notEqual)),
                static (_, _) => throw new InvalidOperationException("Unknown operator sequence.")));

        expression.Parser = operatorSequence;
        var expressionParser = expression.AndSkip(ZeroOrMany(Literals.WhiteSpace(true))).Eof()
            .ElseError(InvalidTokenMessage);

        AppContext.TryGetSwitch("NCalc.EnableParlotParserCompilation", out var enableParserCompilation);

        Parser = enableParserCompilation ? expressionParser.Compile() : expressionParser;
    }

    public static LogicalExpression Parse(LogicalExpressionParserContext context)
    {
        if (Parser.TryParse(context, out var result, out var error))
        {
            return result;
        }

        string message;
        if (error != null)
        {
            message = $"{error.Message} at position {error.Position}";
        }
        else
        {
            message = $"Error parsing the expression at position {context.Scanner.Cursor.Position}";
        }

        throw new NCalcParserException(message);
    }
}