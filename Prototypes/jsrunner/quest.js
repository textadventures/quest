(function () {
    window.quest = window.quest || {};
    
    window.quest.load = function (data) {
        // TODO: Eventually this will be called with a full .aslx file
        // (Note - only a single file from a .quest, we don't need to worry ever about including
        // external libraries)

        var script = parseScript(data);
        executeScript(script);
    };

    var parseScript = function (text) {
        while (true) {
            var scriptLine = getScriptLine(text);

            if (!scriptLine) break;
            if (scriptLine.line.length !== 0) {
                console.log(scriptLine.line);
            }

            if (!scriptLine.after) break;
            text = scriptLine.after;
        }
    };

    var getScriptLine = function (text) {
        // based on WorldModel.Utility.GetScript
        // return one line of the script, and the remaining script

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

    var getParameterInternal = function (text, open, close) {
        // based on WorldModel.Utility.GetParameterInt

        var afterParameter = null;
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
        }
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

    var executeScript = function (script) {
        console.log("executeScript not yet implemented");
    };
})();