define(['jsep', 'state', 'ui', 'scriptparser'], function (jsep, state, ui, scriptParser) {
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
    
    var commands = {
        'msg': {
            parameters: [1],
            execute: function (ctx) {               
                evaluateExpression(ctx.parameters[0], function (result) {
                    ui.print(result);
                    ctx.complete();
                });
            }
        },
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
                        callstack.push({
                            script: ctx.parameters.then,
                            index: 0,
                        });
                        ctx.complete();
                    }
                    else {
                        var evaluateElse = function () {
                            if (ctx.parameters.else) {
                                callstack.push({
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
                                        callstack.push({
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
                            callstack.push({
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
        '=': {
            execute: function (ctx) {
                evaluateExpression(ctx.parameters.value, function (result) {
                    if (ctx.parameters.elementExpr) {
                        evaluateExpression(ctx.parameters.elementExpr, function (element) {
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
        },
        '=>': {
            execute: function (ctx) {
                console.log(ctx.parameters.appliesTo + " => " + ctx.parameters.value);
                ctx.complete();
            }
        },
        'request': {
            parameters: [2],
            execute: function (ctx) {               
                evaluateExpression(ctx.parameters[1], function (data) {
                    var request = ctx.parameters[0].expr;
                    switch (request) {
                        case 'Show':
                            ui.show(data);
                            break;
                        case 'Hide':
                            ui.hide(data);
                            break;
                        default:
                            console.log('Unhandled request type ' + request);
                    }
                    
                    ctx.complete();
                });
            }
        },
        'return': {
            parameters: [1],
            execute: function (ctx) {
                evaluateExpression(ctx.parameters[0], function (result) {
                    ctx.onReturn(result);
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
        'invoke': {
            parameters: [1],
            execute: function (ctx) {
                evaluateExpression(ctx.parameters[0], function (result) {
                    callstack.push({
                        script: result,
                        locals: {},
                        index: 0,
                        onReturn: ctx.complete
                    });
                    executeNext();
                });
            }
        }
    };
    
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
                            callFunction(procName, args, function () {
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
    
    var callstack = [];

    var executeScript = function (script, locals) {
        if (callstack.length !== 0) {
            throw 'Existing callstack is not empty';
        }
        
        callstack = [{
            script: script,
            index: 0,
            locals: locals || {},
        }];
        
        executeNext();
    };
    
    var nestedExecuteNextCount = 0;
    
    var executeNext = function () {
        if (callstack.length === 0) return;
        
        // An "if" script is inside a child frame. The parent frame is the one
        // with the local variables and any "return" script.
        var parentFrameIndex = getParentFrameIndex();
        var parentFrame = callstack[parentFrameIndex];
        var frame = callstack[callstack.length - 1];
        
        if (parentFrame.finished) {
            var framesToRemove = callstack.length - parentFrameIndex;
            callstack.splice(-framesToRemove);
            executeNext();
            return;
        }
        
        if (frame.index === frame.script.length) {
            callstack.pop();
            executeNext();
            return;
        }
        
        var script = frame.script[frame.index++];
        
        var go = function () {
            script.command.execute({
                parameters: script.parameters,
                locals: parentFrame.locals,
                onReturn: function (result) {
                    parentFrame.finished = true;
                    parentFrame.onReturn(result);
                },
                complete: function () {
                    executeNext();
                }
            });
        };
        
        if (nestedExecuteNextCount < 1000) {
            nestedExecuteNextCount++;
            go();
            nestedExecuteNextCount--;
        }
        else {
            setTimeout(function () {
                go();
            }, 0);
        }
    };
    
    var getParentFrameIndex = function () {
        var frameIndex = callstack.length - 1;
        while (true) {
            if (callstack[frameIndex].locals) return frameIndex;
            frameIndex--;
            if (frameIndex === -1) {
                throw 'Could not find parent frame';
            } 
        }
    };
    
    var getParentFrame = function () {
        return callstack[getParentFrameIndex()];
    };
        
    var evaluateExpression = function (expr, complete) {
        if (!expr.tree) {
            throw 'Not an expression: ' + expr;
        }
        var frame = callstack[callstack.length - 1];
        frame.expressionStack = [{
           tree: expr.tree,
           complete: function (result) {
                complete(result);
           }
        }];
        
        evaluateNext();
    };
    
    var evaluateExpressions = function (exprs, complete) {
        var index = 0;
        var results = [];
        var go = function () {
            if (index === exprs.length) {
                complete(results);
            }
            else {
                evaluateExpression(exprs[index], function (result) {
                    results.push(result);
                    index++;
                    go();
                });
            }
        };
        go();
    };
    
    var evaluateNext = function () {
        var frame = callstack[callstack.length - 1];
        var expressionFrame = frame.expressionStack[frame.expressionStack.length - 1];
        var tree = expressionFrame.tree;
        switch (tree.type) {
            case 'Literal':
                expressionFrame.complete(tree.value);
                break;
            case 'Identifier':
                var locals = getParentFrame().locals;
                if (tree.name in locals) {
                    expressionFrame.complete(locals[tree.name]);                
                }
                else if (state.isElement(tree.name)) {
                    expressionFrame.complete({
                        'type': 'element',
                        'name': tree.name
                    });
                }
                else {
                    throw 'Unknown variable ' + tree.name;
                }
                
                break;
            case 'BinaryExpression':
                frame.expressionStack.push({
                    tree: tree.left,
                    complete: function (leftResult) {
                        frame.expressionStack.push({
                            tree: tree.right,
                            complete: function (rightResult) {
                                expressionFrame.complete(binaryOperator(tree.operator, leftResult, rightResult));
                            }
                        });
                        evaluateNext();
                    }
                });
                evaluateNext();
                break;
            case 'CallExpression':
                if (tree.callee.type !== 'Identifier') {
                    throw 'Function name must be an identifier';
                }
                var index = 0;
                var args = [];
                var evaluateArgs = function () {
                    if (index == tree.arguments.length) {
                        callFunction(tree.callee.name, args, function (result) {
                            expressionFrame.complete(result);
                        });
                        return;
                    }
                    frame.expressionStack.push({
                        tree: tree.arguments[index],
                        complete: function (result) {
                            index++;
                            args.push(result);
                            evaluateArgs();
                        }
                    });
                    evaluateNext();
                };
                evaluateArgs();
                
                break;
            case 'MemberExpression':
                if (tree.computed) {
                    throw 'Unsupported expression';
                }
                if (tree.property.type !== 'Identifier') {
                    throw 'Attribute name must be an identifier';
                }
                frame.expressionStack.push({
                    tree: tree.object,
                    complete: function (result) {
                        if (result.type !== 'element') {
                            throw 'Expected element, got ' + result;
                        }
                        expressionFrame.complete(state.get(result.name, tree.property.name));
                    }
                });
                evaluateNext();
                break;
            case 'UnaryExpression':
                if (tree.operator != 'not') {
                    throw 'Unrecognised operator: ' + tree.operator;
                }
                frame.expressionStack.push({
                    tree: tree.argument,
                    complete: function (result) {
                        expressionFrame.complete(!result);
                    }
                });
                evaluateNext();
                break;
            default:
                throw 'Unknown expression tree type: ' + tree.type;
        }
    };
    
    var binaryOperator = function (operator, left, right) {
        switch (operator) {
            case '=':
                return left === right;
            case '!=':
            case '<>':
                return left !== right;
            case '<':
                return left < right;
            case '>':
                return left > right;
            case '+':
                return left + right;
            case '-':
                return left - right;
            case '*':
                return left * right;
            case '/':
                return left / right;
            case 'and':
                return left && right;
            case 'or':
                return left || right;
            default:
                throw 'Undefined operator ' + operator;
        }
    };
    
    var asyncFunctions = {
        'GetInput': function (args, complete) {
            // TODO: Override input handler
            
            setTimeout(function () {
                complete("test");
            }, 200);
        }
    };
    
    var functions = {
        // String Functions
        'Left': function (args, complete) {
            var input = args[0];
            var length = args[1];
            return input.substring(0, length);
        },
        'Right': function (args, complete) {
            var input = args[0];
            var length = args[1];
            return input.substring(input.length - length - 1);
        },
        'Mid': function (args, complete) {
            var input = args[0];
            var start = args[1];
            if (args.length > 2) {
                var length = args[2];
                return input.substr(start - 1, length);
            }
            return input.substr(start - 1);
        },
        'UCase': function (args, complete) {
            var input = args[0];
            return input.toUpperCase();
        },
        'LCase': function (args, complete) {
            var input = args[0];
            return input.toLowerCase();
        },
        'LengthOf': function (args, complete) {
            var input = args[0];
            if (typeof input === 'undefined') return 0;
            return input.length;
        },
        'CapFirst': function (args, complete) {
            var input = args[0];
            return input.substring(0, 1).toUpperCase() + input.substring(1);
        },
        'Instr': function (args, complete) {
            var input, search;
            if (args.length > 2) {
                var start = args[0];
                input = args[1];
                search = args[2];
                return input.indexOf(search, start - 1) + 1;
            }
            input = args[0];
            search = args[1];
            return input.indexOf(search) + 1;
        },
        'InstrRev': function (args, complete) {
            var input, search;
            if (args.length > 2) {
                var start = args[0];
                input = args[1];
                search = args[2];
                return input.lastIndexOf(search, start - 1) + 1;
            }
            input = args[0];
            search = args[1];
            return input.lastIndexOf(search) + 1;
        },
        'StartsWith': function (args, complete) {
            var input = args[0];
            var search = args[1];
            return input.indexOf(search) === 0;
        },
        'EndsWith': function (args, complete) {
            var input = args[0];
            var search = args[1];
            return input.indexOf(search) === input.length - search.length;
        },
        'Split': function (args, complete) {
            var input = args[0];
            var splitChar = args[1];
            return input.split(splitChar);
        },
        'Join': function (args, complete) {
            var input = args[0];
            var joinChar = args[1];
            return input.join(joinChar);
        },
        'IsNumeric': function (args, complete) {
            var input = args[0];
            return !isNaN(parseFloat(input)) && isFinite(input);
        },
        'Replace': function (args, complete) {
            var input = args[0];
            var oldString = args[1];
            var newString = args[2];
            return input.split(oldString).join(newString);
        },
        'Trim': function (args, complete) {
            var input = args[0];
            return input.trim();
        },
        'LTrim': function (args, complete) {
            var input = args[0];
            return input.replace(/^\s+/,"");
        },
        'RTrim': function (args, complete) {
            var input = args[0];
            return input.replace(/\s+$/,"");
        },
        'Asc': function (args, complete) {
            var input = args[0];
            return input.charCodeAt(0);
        },
        'Chr': function (args, complete) {
            var input = args[0];
            return String.fromCharCode(input);
        }
    };
    
    var callFunction = function (name, args, complete) {
        var fn;
        
        if (state.functionExists(name)) {
            fn = state.getFunctionDefinition(name);
            var argumentValues = {};
            if (fn.parameters) {
                for (var i = 0; i < fn.parameters.length; i++) {
                    if (i >= args.length) break;
                    argumentValues[fn.parameters[i]] = args[i];
                }
            }
            callstack.push({
                script: fn.script,
                locals: argumentValues,
                index: 0,
                onReturn: complete
            });
            executeNext();
            return;
        }
        
        fn = functions[name];
        if (!fn) {
            fn = asyncFunctions[name];
            if (!fn) {
                // TODO: Throw
                //throw 'Unrecognised function ' + name;
                console.log('Unrecognised function ' + name);
                complete();
                return;
            }
            fn(args, complete);
        }
        
        complete(fn(args));
    };
    
    return {
        parseScript: parseScript,
        executeScript: executeScript
    };
});