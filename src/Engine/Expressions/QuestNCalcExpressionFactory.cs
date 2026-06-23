// Based on https://github.com/ncalc/ncalc/blob/master/src/NCalc.Core/Factories/LogicalExpressionFactory.cs

using System.Globalization;
using NCalc;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Factories;
using NCalc.Parser;

namespace QuestViva.Engine.Expressions;

public class QuestNCalcExpressionFactory : ILogicalExpressionFactory
{
    private static QuestNCalcExpressionFactory? _instance;

    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None,
        CancellationToken ct = new())
    {
        try
        {
            var parserOptions = LogicalExpressionParserOptionsExtensions.Create(options, CultureInfo.InvariantCulture);
            var parserContext = new LogicalExpressionParserContext(expression, parserOptions, ct);
            var logicalExpression = QuestNCalcLogicalExpressionParser.Parse(parserContext);

            return logicalExpression ?? throw new ArgumentNullException(nameof(logicalExpression));
        }
        catch (Exception exception)
        {
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }

    public LogicalExpression Create(string expression, CultureInfo cultureInfo,
        ExpressionOptions options = ExpressionOptions.None,
        CancellationToken ct = new())
    {
        return Create(expression, options, ct);
    }

    public static QuestNCalcExpressionFactory GetInstance()
    {
        return _instance ??= new QuestNCalcExpressionFactory();
    }
}