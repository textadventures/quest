define(['scriptrunner'], function (scriptrunner) {
    return {
        parameters: [1],
        execute: function (ctx) {
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