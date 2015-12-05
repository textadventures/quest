define(['require', 'scriptrunner', 'scriptparser', 'expressions'], function (require, scriptrunner, scriptParser, expressions) {
    return {
        create: function (line) {
            var scripts = require('scripts');
            var parameters = scriptParser.getParameterInternal(line, '(', ')');
            
            // TODO...

            return {
                expression: expressions.parseExpression(parameters.parameter)
            };
        },
        execute: function (ctx) {
            // TODO...
            ctx.complete();
        }
    };
});