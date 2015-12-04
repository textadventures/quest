define(['require', 'scriptrunner', 'scriptparser'], function (require, scriptrunner, scriptParser) {
    return {
        create: function (line) {
            var scripts = require('scripts');
            var parameters = scripts.parseParameters(scriptParser.getAndSplitParameters(line));
            var jsFunction = line.match(/^JS\.([\w\.\@]*)/)[1];

            return {
                arguments: parameters,
                jsFunction: jsFunction
            };
        },
        execute: function (ctx) {
            scriptrunner.evaluateExpressions(ctx.parameters.arguments, function (results) {
                var fn = window[ctx.parameters.jsFunction];
                fn.apply(window, results);
                ctx.complete();
            });
        }
    };
});