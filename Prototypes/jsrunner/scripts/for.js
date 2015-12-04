define(['require', 'scriptrunner', 'scriptparser', 'expressions'], function (require, scriptrunner, scriptParser, expressions) {
    return {
        create: function (line) {
            var scripts = require('scripts');
            var parameterAndScript = scriptParser.getParameterInternal(line, '(', ')');
            var loopScript = scripts.parseScript(parameterAndScript.after);
            var parameters = scriptParser.splitParameters(parameterAndScript.parameter);
            
            if (parameters.length !== 3 && parameters.length !== 4) {
                throw '"for" script should have 3 or 4 parameters: ' + line;
            }

            return {
                variable: parameters[0],
                from: expressions.parseExpression(parameters[1]),
                to: expressions.parseExpression(parameters[2]),
                step: parameters.length === 3 ? null : expressions.parseExpression(parameters[3]),
                loopScript: loopScript
            };
        },
        execute: function (ctx) {
            var go = function (fromResult, toResult, stepResult) {
                if (toResult < fromResult) {
                    ctx.complete();
                    return;
                }
                
                ctx.locals[ctx.parameters.variable] = fromResult;
                var iterations = 0;
                
                var runLoop = function () {
                    if (ctx.locals[ctx.parameters.variable] <= toResult) {
                        var script = [].concat(ctx.parameters.loopScript);
                        script.push({
                            command: {
                                execute: function () {
                                    ctx.locals[ctx.parameters.variable] = ctx.locals[ctx.parameters.variable] + stepResult;
                                    iterations++;
                                    if (iterations < 1000) {
                                        runLoop();
                                    }
                                    else {
                                        setTimeout(function () {
                                            iterations = 0;
                                            runLoop();
                                        }, 0);
                                    }
                                }
                            }
                        });
                        scriptrunner.getCallstack().push({
                            script: script,
                            index: 0,
                        });
                    }
                    ctx.complete();
                };
                
                runLoop();
            };
            
            scriptrunner.evaluateExpression(ctx.parameters.from, function (fromResult) {
                scriptrunner.evaluateExpression(ctx.parameters.to, function (toResult) {
                    if (ctx.parameters.step) {
                        scriptrunner.evaluateExpression(ctx.parameters.step, function (stepResult) {
                            go (fromResult, toResult, stepResult);
                        });
                    }
                    else {
                        go (fromResult, toResult, 1);
                    }
                });
            });
        }
    };
});