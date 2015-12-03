define(['scriptrunner', 'state'], function (scriptrunner, state) {
    return {
        execute: function (ctx) {
            scriptrunner.evaluateExpression(ctx.parameters.value, function (result) {
                if (ctx.parameters.elementExpr) {
                    scriptrunner.evaluateExpression(ctx.parameters.elementExpr, function (element) {
                        if (element.type !== 'element') {
                            throw 'Expected element, got ' + element;
                        }
                        state.set(element.name, ctx.parameters.variable, result);
                        ctx.complete();
                    });
                }
                else {
                    ctx.locals[ctx.parameters.variable] = result;
                    ctx.complete();
                }
            });
        }
    };
});