define(['state'], function (state) {
	var callstack = [];
    
    var getCallstack = function () {
        return callstack;
    };

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
           expr: expr.expr,
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
    
    var lastExpr = null;
    
    var evaluateNext = function () {
        var frame = callstack[callstack.length - 1];
        var expressionFrame = frame.expressionStack[frame.expressionStack.length - 1];
        var tree = expressionFrame.tree;
        if (expressionFrame.expr) lastExpr = expressionFrame.expr;
        try {
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
        }
        catch (e) {
            if (lastExpr) {
                console.log('Error evaluating expression: ' + lastExpr);
                console.log(e);
                lastExpr = null;
            }
            
            throw e;
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
            if (typeof input === 'undefined' || input === null) return 0;
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
        executeScript: executeScript,
        evaluateExpressions: evaluateExpressions,
        evaluateExpression: evaluateExpression,
        getCallstack: getCallstack,
        executeNext: executeNext,
        callFunction: callFunction
    }
});