define(['scriptrunner'], function (scriptrunner) {
    return {
        parameters: [1],
        execute: function (ctx) {
            scriptrunner.evaluateExpression(ctx.parameters[0], function (result) {
                ctx.onReturn(result);
            });
        }
    };
});