using NCalc;
using NCalc.Cache;
using NCalc.Factories;
using NCalc.Visitors;

namespace QuestViva.Engine.Expressions;

/// <summary>
///     An NCalc <see cref="Expression" /> that evaluates with <see cref="QuestAsyncEvaluationVisitor" />.
/// </summary>
public class QuestNCalcExpression(
    string expression,
    ExpressionContext context,
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache logicalExpressionCache)
    : Expression(expression, context, logicalExpressionFactory, logicalExpressionCache)
{
    protected override AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(CancellationToken cancellationToken = default) =>
        new QuestAsyncEvaluationVisitor(Context, cancellationToken);
}

/// <summary>
///     NCalc's evaluation visitor, with FLEE's "not" semantics: logical for booleans, but bitwise
///     complement for integers (so "not 5" is -6). NCalc raises no event for unary operators, so
///     unlike and/or/xor this can't be handled in an event handler in NcalcExpressionEvaluator.
/// </summary>
public class QuestAsyncEvaluationVisitor : AsyncEvaluationVisitor
{
    private readonly ExpressionContext _context;

    public QuestAsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
        : base(context, cancellationToken)
    {
        _context = context;
    }

    public override async Task<object?> Visit(UnaryExpression expression)
    {
        if (expression.Type != UnaryExpressionType.Not)
        {
            return await base.Visit(expression);
        }

        var value = await EvaluateAsync(expression.Expression);
        return value switch
        {
            int i => ~i,
            long l => ~l,
            byte or short or ushort or uint or sbyte => ~Convert.ToInt64(value),
            ulong u => ~u,
            _ => !Convert.ToBoolean(value, _context.CultureInfo)
        };
    }
}
