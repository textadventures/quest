// Based on https://github.com/ncalc/ncalc/blob/master/src/NCalc.Core/Factories/LogicalExpressionFactory.cs

#nullable enable
using System;
using System.Globalization;
using System.Threading;
using NCalc;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Parser;

namespace QuestViva.Engine.Expressions;

public class QuestNCalcExpressionFactory : ILogicalExpressionFactory
{
    private static QuestNCalcExpressionFactory? _instance;

    public static QuestNCalcExpressionFactory GetInstance()
    {
        return _instance ??= new QuestNCalcExpressionFactory();
    }
    
    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None,
        CancellationToken ct = new())
    {
        try
        {
            var parserContext = new LogicalExpressionParserContext(expression, options);
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
        => Create(expression, options, ct);
}