define(['scriptrunner'], function (scriptrunner) {
    return {
        parameters: [1, 2],
        execute: function (ctx) {
            // TODO: Second parameter is a dictionary of parameters to pass in as locals
            scriptrunner.evaluateExpression(ctx.parameters[0], function (result) {
                scriptrunner.getCallstack().push({
                    script: result,
                    locals: {},
                    index: 0,
                    onReturn: ctx.complete
                });
                scriptrunner.executeNext();
            });
        }
    };
});