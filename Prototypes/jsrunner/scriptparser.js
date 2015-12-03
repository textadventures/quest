define(['jsep'], function (jsep) {
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
    
    return {
        removeSurroundingBraces: removeSurroundingBraces,
        getScriptLine: getScriptLine,
        getAndSplitParameters: getAndSplitParameters,
        obscureStrings: obscureStrings,
        getParameter: getParameter,
        getParameterInternal: getParameterInternal,
        splitParameters: splitParameters
    };
});