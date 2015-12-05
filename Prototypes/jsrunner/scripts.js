define([
    'state',
    'ui',
    'expressions',
    'scriptparser',
    'scriptrunner',
    'scripts/msg',
    'scripts/set',
    'scripts/setscript',
    'scripts/request',
    'scripts/return',
    'scripts/invoke',
    'scripts/if',
    'scripts/switch',
    'scripts/for',
    'scripts/js'
    ],
function (state, ui, expressions, scriptParser, scriptRunner) {
    
    var commands = {
        'msg': require('scripts/msg'),
        '=': require('scripts/set'),
        '=>': require('scripts/setscript'),
        'request': require('scripts/request'),
        'return': require('scripts/return'),
        'invoke': require('scripts/invoke'),
        'if': require('scripts/if'),
        'switch': require('scripts/switch'),
        'for': require('scripts/for'),
        'JS.': require('scripts/js')
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
            value = expressions.parseExpression(line.substr(eqPos + 1).trim());
        }

        return {
            keyword: keyword,
            command: commands[keyword],
            parameters: {
                elementExpr: elementExpr === null ? null : expressions.parseExpression(elementExpr),
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
                        scriptRunner.evaluateExpression(ctx.parameters.expressions[index], function (result) {
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
                var elseIfExpression = expressions.parseExpression(elseIfParameters.parameter);
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
    
    var parseParameters = function (parameters) {
        return parameters.map(expressions.parseExpression);
    };
    
    return {
        parseScript: parseScript,
        executeScript: scriptRunner.executeScript,
        getScript: getScript,
        parseParameters: parseParameters
    };
});