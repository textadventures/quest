/* global jsep */

(function () {
    window.quest = window.quest || {};
    
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
    
    window.quest.load = function (data) {
        // TODO: Eventually this will be called with a full .aslx file
        // (Note - only a single file from a .quest, we don't need to worry ever about including
        // external libraries)

        var script = parseScript(data);
        executeScript(script);
    };

    var commands = {
        'msg': {
            parameters: [1],
            execute: function (ctx) {               
                evaluateExpression(ctx.parameters[0], function (result) {
                    console.log(result);
                    ctx.complete();
                });
            }
        },
        'if': {
            create: function (line) {
                var parameters = getParameterInternal(line, '(', ')');
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
                    }
                    else {
                        // TODO: Run "else" script if it exists
                    }
                    ctx.complete();
                });
            }
        },
        'for': {
            create: function (line) {
                var parameterAndScript = getParameterInternal(line, '(', ')');
                var loopScript = parseScript(parameterAndScript.after);
                var parameters = splitParameters(parameterAndScript.parameter);
                
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
                if (ctx.parameters.elementExpr) {
                    console.log(ctx.parameters.elementExpr + " dot " + ctx.parameters.variable + " = " + ctx.parameters.value);
                    ctx.complete();
                }
                else {
                    evaluateExpression(ctx.parameters.value, function (result) {
                        ctx.locals[ctx.parameters.variable] = result;
                        ctx.complete();
                    });
                }
            }
        },
        '=>': {
            execute: function (ctx) {
                console.log(ctx.parameters.appliesTo + " => " + ctx.parameters.value);
                ctx.complete();
            }
        }
    };

    var getScript = function (line) {
        // based on WorldModel.ScriptFactory.GetScriptConstructor

        var command, keyword, parameters;

        for (var candidate in commands) {
            if (line.substring(0, candidate.length) === candidate) {
                // TODO: Must be non-word character afterwards, see original function
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
        }

        if (!command) return null;

        if (command.create) {
            parameters = command.create(line);
        }
        else if (!parameters) {
            parameters = parseParameters(getAndSplitParameters(line));
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
    
    var getSetScript = function (line) {
        // based on SetScriptConstuctor
        
        var isScript = false;
        var offset = 0;
        
        var obscuredScript = obscureStrings(line);
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
            offset = 1;
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

        return {
            keyword: keyword,
            command: commands[keyword],
            parameters: {
                elementExpr: elementExpr,
                variable: variable,
                value: parseExpression(line.substr(eqPos + 1 + offset).trim())
            } 
        };
    };

    var parseScript = function (text) {
        text = removeSurroundingBraces(text);

        var result = [];
        while (true) {
            var scriptLine = getScriptLine(text);

            if (!scriptLine) break;
            if (scriptLine.line.length !== 0) {
                var script = getScript(scriptLine.line);
                
                if (!script) {
                    console.log('Unrecognised script command: ' + scriptLine.line);
                }
                else {
                    result.push(script);
                }
            }

            if (!scriptLine.after) break;
            text = scriptLine.after;
        }

        return result;
    };

    var removeSurroundingBraces = function (text) {
        // based on WorldModel.Utility.RemoveSurroundingBraces

        text = text.trim();
        if (text.substring(0, 1) === '{' && text.substring(text.length - 1, text.length) === '}') {
            return text.substring(1, text.length - 1);
        }
        return text;
    };

    var getScriptLine = function (text) {
        // based on WorldModel.Utility.GetScript
        // return one line of the script, and the remaining script

        var result;
        var obscuredScript = obscureStrings(text);
        var bracePos = obscuredScript.indexOf('{');
        var crlfPos = obscuredScript.indexOf('\n');
        var commentPos = obscuredScript.indexOf('//');
        if (crlfPos === -1) return {
            line: text.trim()
        };

        if (bracePos === - 1 || crlfPos < bracePos || (commentPos !== -1 && commentPos < bracePos && commentPos < crlfPos)) {
            return {
                line: text.substring(0, crlfPos).trim(),
                after: text.substring(crlfPos + 1)
            };
        }

        var beforeBrace = text.substring(0, bracePos);
        var parameterResult = getParameterInternal(text, '{', '}');
        var insideBraces = parameterResult.parameter;

        if (insideBraces.indexOf('\n') !== -1) {
            result = beforeBrace + '{' + insideBraces + '}';
        }
        else {
            result = beforeBrace + insideBraces;
        }

        return {
            line: result.trim(),
            after: parameterResult.after
        };
    };

    var getAndSplitParameters = function (text) {
        var parameter = getParameter(text);
        if (!parameter) return [];
        return splitParameters(parameter);
    };
    
    var splitParameters = function (parameter) {
        // based on WorldModel.Utility.SplitParameter
        var result = [];
        var inQuote = false;
        var processNextCharacter = true;
        var bracketCount = 0;
        var curParam = [];

        for (var i = 0; i < parameter.length; i++) {
            var c = parameter.charAt(i);
            var processThisCharacter = processNextCharacter;
            processNextCharacter = true;

            if (processThisCharacter) {
                if (c === '\\') {
                    // Don't process the character after a backslash
                    processNextCharacter = false;
                }
                else if (c === '"') {
                    inQuote = !inQuote;
                }
                else {
                    if (!inQuote) {
                        if (c === '(') bracketCount++;
                        if (c === ')') bracketCount--;
                        if (bracketCount === 0 && c === ',') {
                            result.push(curParam.join('').trim());
                            curParam = [];
                            continue;
                        }
                    }
                }
            }

            curParam.push(c);
        }

        result.push(curParam.join('').trim());
        return result;
    };

    var getParameter = function (text) {
        var result = getParameterInternal(text, '(', ')');
        if (!result) return null;
        return result.parameter;
    };

    var getParameterInternal = function (text, open, close) {
        // based on WorldModel.Utility.GetParameterInt

        var obscuredText = obscureStrings(text);
        var start = obscuredText.indexOf(open);
        if (start === -1) return null;

        var finished = false;
        var braceCount = 1;
        var pos = start;

        while (!finished) {
            pos++;
            var curChar = obscuredText.charAt(pos);
            if (curChar === open) braceCount++;
            if (curChar === close) braceCount--;
            if (braceCount === 0 || pos === obscuredText.length - 1) finished = true;
        }

        if (braceCount !== 0) {
            throw 'Missing ' + close;
        }

        return {
            parameter: text.substring(start + 1, pos),
            after: text.substring(pos + 1)
        };
    };

    var obscureStrings = function (input) {
        // based on WorldModel.Utility.ObscureStrings

        var sections = splitQuotes(input);
        var result = [];

        var insideQuote = false;
        for (var i = 0; i < sections.length; i++) {
            var section = sections[i];
            if (insideQuote) {
                result.push(Array(section.length + 1).join('-'));
            }
            else {
                result.push(section);
            }
            if (i < sections.length - 1) {
                result.push('"');
            }
            insideQuote = !insideQuote;
        }
        return result.join('');
    };

    var splitQuotes = function (text) {
        // based on WorldModel.Utility.SplitQuotes

        var result = [];
        var processNextCharacter = true;
        var curParam = [];
        var gotCloseQuote = true;

        for (var i = 0; i < text.length; i++) {
            var curChar = text.charAt(i);

            var processThisCharacter = processNextCharacter;
            processNextCharacter = true;

            if (processThisCharacter) {
                if (curChar === '\\') {
                    // Don't process the character after a backslash
                    processNextCharacter = false;
                }
                else if (curChar === '"') {
                    result.push(curParam.join(''));
                    gotCloseQuote = !gotCloseQuote;
                    curParam = [];
                    continue;
                }
            }

            curParam.push(curChar);
        }

        if (!gotCloseQuote) {
            throw 'Missing quote character in ' + text;
        }

        result.push(curParam.join(''));
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

    var executeScript = function (script) {
        if (callstack.length !== 0) {
            throw 'Existing callstack is not empty';
        }
        
        callstack = [{
            script: script,
            index: 0,
            locals: {},
        }];
        
        executeNext();
    };
    
    var nestedExecuteNextCount = 0;
    
    var executeNext = function () {
        if (callstack.length === 0) return;
        
        var frame = callstack[callstack.length - 1];
        
        if (frame.index === frame.script.length) {
            callstack.pop();
            executeNext();
            return;
        }
        
        var script = frame.script[frame.index++];
        
        var go = function () {
            script.command.execute({
                parameters: script.parameters,
                locals: getLocals(),
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
    
    var getLocals = function () {
        // If current frame has no locals (e.g. inside an "if" script),
        // navigate upwards to find the parent locals.

        var frameIndex = callstack.length - 1;
        while (true) {
            if (callstack[frameIndex].locals) return callstack[frameIndex].locals;
            frameIndex--;
            if (frameIndex === -1) {
                throw 'Could not find local variables';
            } 
        }
    };
        
    var evaluateExpression = function (expr, complete) {
        var frame = callstack[callstack.length - 1];
        frame.expressionStack = [{
           tree: expr.tree,
           complete: function (result) {
                complete(result);
           }
        }];
        
        evaluateNext();
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
                var locals = getLocals();
                if (!(tree.name in locals)) {
                    throw 'Unknown variable ' + tree.name;
                }
                expressionFrame.complete(locals[tree.name]);
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
                // TODO: Evaluate function arguments
                
                callFunction(tree.callee.name, null, function (result) {
                    expressionFrame.complete(result);
                });
                break;
            default:
                throw 'Unknown expression tree type: ' + tree.type;
        }
    };
    
    var binaryOperator = function (operator, left, right) {
        switch (operator) {
            case '=':
                return left === right;
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
    
    var functions = {
        'GetInput': function (args, complete) {
            // TODO: Override input handler
            
            setTimeout(function () {
                complete("test");
            }, 200);
        }
    };
    
    var callFunction = function (name, args, complete) {
        var fn = functions[name];
        if (!fn) {
            throw 'Unrecognised function ' + name;
        }
        fn(args, complete);
    };
})();