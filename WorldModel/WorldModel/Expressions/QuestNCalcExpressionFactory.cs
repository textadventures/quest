// Based on https://github.com/ncalc/ncalc/blob/master/src/NCalc.Core/Factories/LogicalExpressionFactory.cs

#nullable enable
using System;
using NCalc;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Parser;

namespace TextAdventures.Quest.Expressions;

public class QuestNCalcExpressionFactory : ILogicalExpressionFactory
{
    private static QuestNCalcExpressionFactory? _instance;

    public static QuestNCalcExpressionFactory GetInstance()
    {
        return _instance ??= new QuestNCalcExpressionFactory();
    }
    
    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None)
    {
        try
        {
            var parserContext = new LogicalExpressionParserContext(expression, options);
            var logicalExpression = QuestNCalcLogicalExpressionParser.Parse(parserContext);

            if (logicalExpression is null)
                throw new ArgumentNullException(nameof(logicalExpression));

            return logicalExpression;
        }
        catch (Exception exception)
        {
            throw new NCalcParserException("Error parsing the expression.", exception);
        }
    }
}