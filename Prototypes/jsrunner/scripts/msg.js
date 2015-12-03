define(['ui', 'scriptrunner'], function (ui, scriptrunner) {
    return {
        parameters: [1],
        execute: function (ctx) {               
            scriptrunner.evaluateExpression(ctx.parameters[0], function (result) {
                ui.print(result);
                ctx.complete();
            });
        }
    };
});