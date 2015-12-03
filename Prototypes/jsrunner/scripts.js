define(['jsep',
    'state',
    'ui',
    'scriptparser',
    'scriptrunner',
    'scripts/msg',
    'scripts/set',
    'scripts/setscript',
    'scripts/request',
    'scripts/return',
    'scripts/invoke'
    ],
    function (jsep, state, ui, scriptParser, scriptRunner, msg, set, setscript, request, returnScript, invoke) {
        
    jsep.removeUnaryOp('~');
    jsep.addUnaryOp('not');
        
    jsep.removeBinaryOp('>>>');
    jsep.removeBinaryOp('<<');
    jsep.removeBinaryOp('>>');
    
    jsep.removeBinaryOp('==');
    jsep.removeBinaryOp('===');
    jsep.removeBinaryOp('!==');
    jsep.addBinaryOp('=', 6);
    jsep.addBinaryOp('<>');
    
    jsep.addBinaryOp('^', 10);
    
    jsep.removeBinaryOp('||');
    jsep.removeBinaryOp('|');
    jsep.addBinaryOp('or', 1);
    
    jsep.removeBinaryOp('&&');
    jsep.removeBinaryOp('&');
    jsep.addBinaryOp('and', 2);
    
    var evaluateExpression = scriptRunner.evaluateExpression;
    var evaluateExpressions = scriptRunner.evaluateExpressions;
    var getCallstack = scriptRunner.getCallstack;
    
    var commands = {
        'if': {
            create: function (line) {
                var parameters = scriptParser.getParameterInternal(line, '(', ')');
                var thenScript = parseScript(parameters.after);

                return {
                    expression: parseExpression(parameters.parameter),
                    then: thenScript
                };
            },
            execute: function (ctx) {
                evaluateExpression(ctx.parameters.expression, function (result) {
                    if (result) {
                        getCallstack().push({
                            script: ctx.parameters.then,
                            index: 0,
                        });
                        ctx.complete();
                    }
                    else {
                        var evaluateElse = function () {
                            if (ctx.parameters.else) {
                                getCallstack().push({
                                    script: ctx.parameters.else,
                                    index: 0,
                                });
                            }
                            ctx.complete();
                        };
                        
                        if (ctx.parameters.elseIf) {
                            var index = 0;
                            
                            var evaluateElseIf = function () {
                                evaluateExpression(ctx.parameters.elseIf[index].expression, function (result) {
                                    if (result) {
                                        getCallstack().push({
                                            script: ctx.parameters.elseIf[index].script,
                                            index: 0,
                                        });
                                        ctx.complete();
                                    }
                                    else {
                                        index++;
                                        if (index < ctx.parameters.elseIf.length) {
                                            evaluateElseIf();
                                        }
                                        else {
                                            evaluateElse();
                                        }
                                    }
                                });
                            };
                            evaluateElseIf();
                        }
                        else {
                            evaluateElse();
                        }
                    }
                });
            }
        },
        'for': {
            create: function (line) {
                var parameterAndScript = scriptParser.getParameterInternal(line, '(', ')');
                var loopScript = parseScript(parameterAndScript.after);
                var parameters = scriptParser.splitParameters(parameterAndScript.parameter);
                
                if (parameters.length !== 3 && parameters.length !== 4) {
                    throw '"for" script should have 3 or 4 parameters: ' + line;
                }

                return {
                    variable: parameters[0],
                    from: parseExpression(parameters[1]),
                    to: parseExpression(parameters[2]),
                    step: parameters.length === 3 ? null : parseExpression(parameters[3]),
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
                            getCallstack().push({
                                script: script,
                                index: 0,
                            });
                        }
                        ctx.complete();
                    };
                    
                    runLoop();
                };
                
                evaluateExpression(ctx.parameters.from, function (fromResult) {
                    evaluateExpression(ctx.parameters.to, function (toResult) {
                        if (ctx.parameters.step) {
                            evaluateExpression(ctx.parameters.step, function (stepResult) {
                                go (fromResult, toResult, stepResult);
                            });
                        }
                        else {
                            go (fromResult, toResult, 1);
                        }
                    });
                });
            }
        },
        'JS.': {
            create: function (line) {
                var parameters = parseParameters(scriptParser.getAndSplitParameters(line));
                var jsFunction = line.match(/^JS\.([\w\.\@]*)/)[1];

                return {
                    arguments: parameters,
                    jsFunction: jsFunction
                };
            },
            execute: function (ctx) {
                evaluateExpressions(ctx.parameters.arguments, function (results) {
                    var fn = window[ctx.parameters.jsFunction];
                    fn.apply(window, results);
                    ctx.complete();
                });
            }
        },
    };
    
    commands.msg = msg;
    commands['='] = set;
    commands['=>'] = setscript;
    commands.request = request;
    commands['return'] = returnScript;
    commands.invoke = invoke;
    
    var getSetScript = function (line) {
        // based on SetScriptConstuctor
        
        var isScript = false;
        
        var obscuredScript = scriptParser.obscureStrings(line);
        var bracePos = obscuredScript.indexOf('{');
        if (bracePos !== - 1) {
            // only want to look for = and => before any other scripts which may
            // be defined on the same line, for example procedure calls of type
            //     MyProcedureCall (5) { some other script }

            obscuredScript = obscuredScript.substring(0, bracePos);
        }
        
        var eqPos = obscuredScript.indexOf('=>');
        if (eqPos !== -1) {
            isScript = true;
        }
        else {
            eqPos = obscuredScript.indexOf('=');
        }
        
        if (eqPos === -1) return null;
        
        var keyword = isScript ? '=>' : '=';
        var appliesTo = line.substr(0, eqPos).trim();
        var lastDot = appliesTo.lastIndexOf('.');
        
        var elementExpr = lastDot === - 1 ? null : appliesTo.substr(0, lastDot);
        var variable = lastDot === -1 ? appliesTo : appliesTo.substr(lastDot + 1);
        
        var value;
        if (isScript) {
            value = parseScript(line.substr(eqPos + 2).trim());
        }
        else {
            value = parseExpression(line.substr(eqPos + 1).trim());
        }

        return {
            keyword: keyword,
            command: commands[keyword],
            parameters: {
                elementExpr: elementExpr === null ? null : parseExpression(elementExpr),
                variable: variable,
                value: value
            } 
        };
    };
    
    var getFunctionCallScript = function (line) {
        // based on FunctionCallScriptConstructor
        
        var paramExpressions, procName, paramScript = null;
        
        var param = scriptParser.getParameterInternal(line, '(', ')');        
        
        if (param && param.after) {
            // Handle functions of the form
            //    SomeFunction (parameter) { script }            
            paramScript = parseScript(param.after);
        }
        
        if (!param && !paramScript) {
            procName = line;
        }
        else {
            var parameters = scriptParser.splitParameters(param.parameter);
            procName = line.substr(0, line.indexOf('(')).trim();
            if (param.parameter.trim().length > 0) {
                paramExpressions = parseParameters(parameters);
            }
        }
        
        return {
            command: {
                execute: function (ctx) {
                    var args = [];
                    var index = 0;
                    var evaluateArgs = function () {
                        if (typeof ctx.parameters.expressions === 'undefined' || index === ctx.parameters.expressions.length) {
                            if (paramScript) {
                                args.push(paramScript);
                            }
                            scriptRunner.callFunction(procName, args, function () {
                                ctx.complete();
                            });
                            return;
                        }
                        evaluateExpression(ctx.parameters.expressions[index], function (result) {
                            index++;
                            args.push(result);
                            evaluateArgs();
                        });
                    };
                    evaluateArgs();
                }
            },
            parameters: {
                expressions: paramExpressions,
                script: paramScript
            }
        };
    };
    
    var getScript = function (line, lastIf) {
        // based on WorldModel.ScriptFactory.GetScriptConstructor

        var command, keyword, parameters;
        
        if (line.substring(0, 2) === '//') return null;
        
        if (line.substring(0, 4) === 'else') {
            if (!lastIf) {
                throw 'Unexpected "else" (error with parent "if"?):' + line;
            }
            if (line.substring(0, 7) === 'else if') {
                if (!lastIf.elseIf) lastIf.elseIf = [];
                var elseIfParameters = scriptParser.getParameterInternal(line, '(', ')');
                var elseIfExpression = parseExpression(elseIfParameters.parameter);
                var elseIfScript = parseScript(elseIfParameters.after);
                lastIf.elseIf.push({
                    expression: elseIfExpression,
                    script: elseIfScript
                });
            }
            else {
                lastIf.else = parseScript(line.substring(5));
            }
            return null;
        }

        for (var candidate in commands) {
            if (line.substring(0, candidate.length) === candidate &&
                (line.length === candidate.length || line.substr(candidate.length).match(/^\W/) || candidate === 'JS.')) {
                keyword = candidate;
                command = commands[candidate];
            }
        }
        
        if (!command) {
            // see if it's a set script
            
            var setScript = getSetScript(line);
            if (setScript) {
                command = setScript.command;
                keyword = setScript.keyword;
                parameters = setScript.parameters;
            }
            else {
                // see if it's a function call
                var functionCall = getFunctionCallScript(line);
                if (functionCall) {
                    command = functionCall.command;
                    parameters = functionCall.parameters;
                }
            }
        }

        if (!command) {
            console.log('Unrecognised script command: ' + line);
            return null;
        }

        if (command.create) {
            parameters = command.create(line);
        }
        else if (!parameters) {
            parameters = parseParameters(scriptParser.getAndSplitParameters(line));
            if (command.parameters.indexOf(parameters.length) === -1) {
                throw 'Expected ' + command.parameters.join(',') + ' parameters in command: ' + line;
            }
        }

        return {
            keyword: keyword,
            command: command,
            line: line,
            parameters: parameters
        };
    };
    
    var parseScript = function (text) {
        var lastIf;
        
        text = scriptParser.removeSurroundingBraces(text);

        var result = [];
        while (true) {
            var scriptLine = scriptParser.getScriptLine(text);

            if (!scriptLine) break;
            if (scriptLine.line.length !== 0) {
                var script = getScript(scriptLine.line, lastIf);
                
                if (script) {
                    result.push(script);
                    if (script.keyword === 'if') lastIf = script.parameters;
                }
            }

            if (!scriptLine.after) break;
            text = scriptLine.after;
        }

        return result;
    };
    
    var parseExpression = function (expr) {
        return {
            expr: expr,
            tree: jsep(expr)
        };
    };
    
    var parseParameters = function (parameters) {
        return parameters.map(parseExpression);
    };
    
    return {
        parseScript: parseScript,
        executeScript: scriptRunner.executeScript
    };
});