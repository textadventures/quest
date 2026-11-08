using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task<string> EvaluateInlineExpressions(string s)
    {
        // Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>

        if (ASLVersion < 391)
        {
            return s;
        }

        int bracePos;
        var curPos = 1;
        var resultLine = "";

        do
        {
            bracePos = Strings.InStr(curPos, s, "{");

            if (bracePos != 0)
            {
                resultLine = resultLine + Strings.Mid(s, curPos, bracePos - curPos);

                if (Strings.Mid(s, bracePos, 2) == "{{")
                {
                    // {{ = {
                    curPos = bracePos + 2;
                    resultLine = resultLine + "{";
                }
                else
                {
                    var EndBracePos = Strings.InStr(bracePos + 1, s, "}");
                    if (EndBracePos == 0)
                    {
                        LogASLError("Expected } in '" + s + "'", LogType.WarningError);
                        return "<ERROR>";
                    }

                    var expression = Strings.Mid(s, bracePos + 1, EndBracePos - bracePos - 1);
                    var expResult = ExpressionHandler(expression);
                    if (expResult.Success != ExpressionSuccess.OK)
                    {
                        LogASLError("Error evaluating expression in <" + s + "> - " + expResult.Message);
                        return "<ERROR>";
                    }

                    resultLine = resultLine + expResult.Result;
                    curPos = EndBracePos + 1;
                }
            }
            else
            {
                resultLine = resultLine + Strings.Mid(s, curPos);
            }
        } while (!((bracePos == 0) | (curPos > Strings.Len(s))));

        // Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        curPos = 1;
        do
        {
            bracePos = Strings.InStr(curPos, resultLine, "}}");
            if (bracePos != 0)
            {
                resultLine = Strings.Left(resultLine, bracePos) + Strings.Mid(resultLine, bracePos + 2);
                curPos = bracePos + 1;
            }
        } while (!((bracePos == 0) | (curPos > Strings.Len(resultLine))));

        return resultLine;
    }

    private ExpressionResult ExpressionHandler(string expr)
    {
        int openBracketPos, endBracketPos;
        var res = new ExpressionResult();

        // Find brackets, recursively call ExpressionHandler
        do
        {
            openBracketPos = Strings.InStr(expr, "(");
            if (openBracketPos != 0)
            {
                // Find equivalent closing bracket
                var BracketCount = 1;
                endBracketPos = 0;
                for (int i = openBracketPos + 1, loopTo = Strings.Len(expr); i <= loopTo; i++)
                {
                    if (Strings.Mid(expr, i, 1) == "(")
                    {
                        BracketCount = BracketCount + 1;
                    }
                    else if (Strings.Mid(expr, i, 1) == ")")
                    {
                        BracketCount = BracketCount - 1;
                    }

                    if (BracketCount == 0)
                    {
                        endBracketPos = i;
                        break;
                    }
                }

                if (endBracketPos != 0)
                {
                    var NestedResult =
                        ExpressionHandler(Strings.Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1));
                    if (NestedResult.Success != ExpressionSuccess.OK)
                    {
                        res.Success = NestedResult.Success;
                        res.Message = NestedResult.Message;
                        return res;
                    }

                    expr = Strings.Left(expr, openBracketPos - 1) + " " + NestedResult.Result + " " +
                           Strings.Mid(expr, endBracketPos + 1);
                }

                else
                {
                    res.Message = "Missing closing bracket";
                    res.Success = ExpressionSuccess.Fail;
                    return res;
                }
            }
        } while (openBracketPos != 0);

        // Split expression into elements, e.g.:
        // 2 + 3 * 578.2 / 36
        // E O E O EEEEE O EE      where E=Element, O=Operator

        var numElements = 1;
        string[] elements;
        elements = new string[2];
        var numOperators = 0;
        var operators = new string[1];
        bool newElement;

        var obscuredExpr = ObscureNumericExps(expr);

        for (int i = 1, loopTo1 = Strings.Len(expr); i <= loopTo1; i++)
        {
            switch (Strings.Mid(obscuredExpr, i, 1) ?? "")
            {
                case "+":
                case "*":
                case "/":
                {
                    newElement = true;
                    break;
                }
                case "-":
                {
                    // A minus often means subtraction, so it's a new element. But sometimes
                    // it just denotes a negative number. In this case, the current element will
                    // be empty.

                    if (string.IsNullOrEmpty(Strings.Trim(elements[numElements])))
                    {
                        newElement = false;
                    }
                    else
                    {
                        newElement = true;
                    }

                    break;
                }

                default:
                {
                    newElement = false;
                    break;
                }
            }

            if (newElement)
            {
                numElements = numElements + 1;
                Array.Resize(ref elements, numElements + 1);

                numOperators = numOperators + 1;
                Array.Resize(ref operators, numOperators + 1);
                operators[numOperators] = Strings.Mid(expr, i, 1);
            }
            else
            {
                elements[numElements] = elements[numElements] + Strings.Mid(expr, i, 1);
            }
        }

        // Check Elements are numeric, and trim spaces
        for (int i = 1, loopTo2 = numElements; i <= loopTo2; i++)
        {
            elements[i] = Strings.Trim(elements[i]);

            if (!Information.IsNumeric(elements[i]))
            {
                res.Message = "Syntax error evaluating expression - non-numeric element '" + elements[i] + "'";
                res.Success = ExpressionSuccess.Fail;
                return res;
            }
        }

        var opNum = 0;

        var result = default(double);
        do
        {
            // Go through the Operators array to find next calculation to perform

            for (int i = 1, loopTo3 = numOperators; i <= loopTo3; i++)
            {
                if ((operators[i] == "/") | (operators[i] == "*"))
                {
                    opNum = i;
                    break;
                }
            }

            if (opNum == 0)
            {
                for (int i = 1, loopTo4 = numOperators; i <= loopTo4; i++)
                {
                    if ((operators[i] == "+") | (operators[i] == "-"))
                    {
                        opNum = i;
                        break;
                    }
                }
            }

            // If OpNum is still 0, there are no calculations left to do.

            if (opNum != 0)
            {
                var val1 = Conversion.Val(elements[opNum]);
                var val2 = Conversion.Val(elements[opNum + 1]);

                switch (operators[opNum] ?? "")
                {
                    case "/":
                    {
                        if (val2 == 0d)
                        {
                            res.Message = "Division by zero";
                            res.Success = ExpressionSuccess.Fail;
                            return res;
                        }

                        result = val1 / val2;
                        break;
                    }
                    case "*":
                    {
                        result = val1 * val2;
                        break;
                    }
                    case "+":
                    {
                        result = val1 + val2;
                        break;
                    }
                    case "-":
                    {
                        result = val1 - val2;
                        break;
                    }
                }

                elements[opNum] = result.ToString(CultureInfo.InvariantCulture);

                // Remove this operator, and Elements(OpNum+1) from the arrays
                for (int i = opNum, loopTo5 = numOperators - 1; i <= loopTo5; i++)
                {
                    operators[i] = operators[i + 1];
                }

                for (int i = opNum + 1, loopTo6 = numElements - 1; i <= loopTo6; i++)
                {
                    elements[i] = elements[i + 1];
                }

                numOperators = numOperators - 1;
                numElements = numElements - 1;
                Array.Resize(ref operators, numOperators + 1);
                Array.Resize(ref elements, numElements + 1);
            }
        } while (!((opNum == 0) | (numOperators == 0)));

        res.Success = ExpressionSuccess.OK;
        res.Result = elements[1];
        return res;
    }

    private string ObscureNumericExps(string s)
    {
        // Obscures + or - next to E in Double-type variables with exponents
        // e.g. 2.345E+20 becomes 2.345EX20
        // This stops huge numbers breaking parsing of maths functions

        int ep;
        var result = s;

        var pos = 1;
        do
        {
            ep = Strings.InStr(pos, result, "E");
            if (ep != 0)
            {
                result = Strings.Left(result, ep) + "X" + Strings.Mid(result, ep + 2);
                pos = ep + 2;
            }
        } while (ep != 0);

        return result;
    }

    private async Task<string> ConvertVarsIn(string s, Context ctx)
    {
        return await GetParameter("<" + s + ">", ctx);
    }

    private ArrayResult GetArrayIndex(string varName, Context ctx)
    {
        var result = new ArrayResult();

        if ((Strings.InStr(varName, "[") == 0) | (Strings.InStr(varName, "]") == 0))
        {
            result.Name = varName;
            return result;
        }

        var beginPos = Strings.InStr(varName, "[");
        var endPos = Strings.InStr(varName, "]");
        var data = Strings.Mid(varName, beginPos + 1, endPos - beginPos - 1);

        if (Information.IsNumeric(data))
        {
            result.Index = Conversions.ToInteger(data);
        }
        else
        {
            result.Index = (int) Math.Round(GetNumericContents(data, ctx));
        }

        result.Name = Strings.Left(varName, beginPos - 1);
        return result;
    }

    private async Task<string> ConvertParameter(string parameter, string convertChar, ConvertType action, Context ctx)
    {
        // Returns a string with functions, string and
        // numeric variables executed or converted as
        // appropriate, read for display/etc.

        var result = "";
        var pos = 1;
        var finished = false;

        do
        {
            var varPos = Strings.InStr(pos, parameter, convertChar);
            if (varPos == 0)
            {
                varPos = Strings.Len(parameter) + 1;
                finished = true;
            }

            var currentBit = Strings.Mid(parameter, pos, varPos - pos);
            result = result + currentBit;

            if (!finished)
            {
                var nextPos = Strings.InStr(varPos + 1, parameter, convertChar);

                if (nextPos == 0)
                {
                    LogASLError("Line parameter <" + parameter + "> has missing " + convertChar, LogType.WarningError);
                    return "<ERROR>";
                }

                var varName = Strings.Mid(parameter, varPos + 1, nextPos - 1 - varPos);

                if (string.IsNullOrEmpty(varName))
                {
                    result = result + convertChar;
                }

                else if (action == ConvertType.Strings)
                {
                    result = result + GetStringContents(varName, ctx);
                }
                else if (action == ConvertType.Functions)
                {
                    varName = await EvaluateInlineExpressions(varName);
                    result = result + await DoFunction(varName, ctx);
                }
                else if (action == ConvertType.Numeric)
                {
                    result = result + Strings.Trim(InvariantStrings.Str(GetNumericContents(varName, ctx)));
                }
                else if (action == ConvertType.Collectables)
                {
                    result = result + Strings.Trim(InvariantStrings.Str(GetCollectableAmount(varName)));
                }

                pos = nextPos + 1;
            }
        } while (!finished);

        return result;
    }

    private async Task ExecSetVar(string varInfo, Context ctx)
    {
        // Sets variable contents from a script parameter.
        // Eg <var1;7> sets numeric variable var1
        // to 7

        var scp = Strings.InStr(varInfo, ";");
        var varName = Strings.Trim(Strings.Left(varInfo, scp - 1));
        var varCont = Strings.Trim(Strings.Mid(varInfo, scp + 1));
        var idx = GetArrayIndex(varName, ctx);

        if (Information.IsNumeric(idx.Name))
        {
            LogASLError("Invalid numeric variable name '" + idx.Name + "' - variable names cannot be numeric",
                LogType.WarningError);
            return;
        }

        try
        {
            if (ASLVersion >= 391)
            {
                var expResult = ExpressionHandler(varCont);
                if (expResult.Success == ExpressionSuccess.OK)
                {
                    varCont = expResult.Result;
                }
                else
                {
                    varCont = "0";
                    LogASLError("Error setting numeric variable <" + varInfo + "> : " + expResult.Message,
                        LogType.WarningError);
                }
            }
            else
            {
                var obscuredVarInfo = ObscureNumericExps(varCont);
                var opPos = Strings.InStr(obscuredVarInfo, "+");
                if (opPos == 0)
                {
                    opPos = Strings.InStr(obscuredVarInfo, "*");
                }

                if (opPos == 0)
                {
                    opPos = Strings.InStr(obscuredVarInfo, "/");
                }

                if (opPos == 0)
                {
                    opPos = Strings.InStr(2, obscuredVarInfo, "-");
                }

                if (opPos != 0)
                {
                    var op = Strings.Mid(varCont, opPos, 1);
                    var num1 = Conversion.Val(Strings.Left(varCont, opPos - 1));
                    var num2 = Conversion.Val(Strings.Mid(varCont, opPos + 1));

                    switch (op ?? "")
                    {
                        case "+":
                        {
                            varCont = InvariantStrings.Str(num1 + num2);
                            break;
                        }
                        case "-":
                        {
                            varCont = InvariantStrings.Str(num1 - num2);
                            break;
                        }
                        case "*":
                        {
                            varCont = InvariantStrings.Str(num1 * num2);
                            break;
                        }
                        case "/":
                        {
                            if (num2 != 0d)
                            {
                                varCont = InvariantStrings.Str(num1 / num2);
                            }
                            else
                            {
                                LogASLError("Division by zero - The result of this operation has been set to zero.",
                                    LogType.WarningError);
                                varCont = "0";
                            }

                            break;
                        }
                    }
                }
            }

            await SetNumericVariableContents(idx.Name, Conversion.Val(varCont), ctx, idx.Index);
        }
        catch
        {
            LogASLError("Error setting variable '" + idx.Name + "' to '" + varCont + "'", LogType.WarningError);
        }
    }

    private double GetNumericContents(string name, Context ctx, bool noError = false)
    {
        var numNumber = default(int);
        int arrayIndex;
        var exists = false;

        // First, see if the variable already exists. If it
        // does, get its contents. If not, generate an error.

        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var op = Strings.InStr(name, "[");
            var cp = Strings.InStr(name, "]");
            var arrayIndexData = Strings.Mid(name, op + 1, cp - op - 1);
            if (Information.IsNumeric(arrayIndexData))
            {
                arrayIndex = Conversions.ToInteger(arrayIndexData);
            }
            else
            {
                arrayIndex = (int) Math.Round(GetNumericContents(arrayIndexData, ctx));
            }

            name = Strings.Left(name, op - 1);
        }
        else
        {
            arrayIndex = 0;
        }

        if (_numberNumericVariables > 0)
        {
            for (int i = 1, loopTo = _numberNumericVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            if (!noError)
            {
                LogASLError("No numeric variable '" + name + "' defined.", LogType.WarningError);
            }

            return -32767;
        }

        if (arrayIndex > _numericVariable[numNumber].VariableUBound)
        {
            if (!noError)
            {
                LogASLError("Array index of '" + name + "[" + Strings.Trim(InvariantStrings.Str(arrayIndex)) + "]' too big.",
                    LogType.WarningError);
            }

            return -32766;
        }

        // Now, set the contents
        return Conversion.Val(_numericVariable[numNumber].VariableContents[arrayIndex]);
    }

    private class ArrayResult
    {
        public int Index;
        public string Name;
    }

    private async Task SetNumericVariableContents(string name, double content, Context ctx, int arrayIndex = 0)
    {
        var numNumber = default(int);
        var exists = false;

        if (Information.IsNumeric(name))
        {
            LogASLError(
                "Illegal numeric variable name '" + name +
                "' - check you didn't put % around the variable name in the ASL code", LogType.WarningError);
            return;
        }

        // First, see if variable already exists. If it does,
        // modify it. If not, create it.

        if (_numberNumericVariables > 0)
        {
            for (int i = 1, loopTo = _numberNumericVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            _numberNumericVariables = _numberNumericVariables + 1;
            numNumber = _numberNumericVariables;
            Array.Resize(ref _numericVariable, numNumber + 1);
            _numericVariable[numNumber] = new VariableType();
            _numericVariable[numNumber].VariableUBound = arrayIndex;
        }

        if (arrayIndex > _numericVariable[numNumber].VariableUBound)
        {
            Array.Resize(ref _numericVariable[numNumber].VariableContents, arrayIndex + 1);
            _numericVariable[numNumber].VariableUBound = arrayIndex;
        }

        // Now, set the contents
        _numericVariable[numNumber].VariableName = name;
        Array.Resize(ref _numericVariable[numNumber].VariableContents, _numericVariable[numNumber].VariableUBound + 1);
        _numericVariable[numNumber].VariableContents[arrayIndex] = content.ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(_numericVariable[numNumber].OnChangeScript) & !_gameIsRestoring)
        {
            var script = _numericVariable[numNumber].OnChangeScript;
            await ExecuteScript(script, ctx);
        }

        if (!string.IsNullOrEmpty(_numericVariable[numNumber].DisplayString))
        {
            await UpdateStatusVars(ctx);
        }
    }

    private async Task<SetResult> SetUnknownVariableType(string variableData, Context ctx)
    {
        var scp = Strings.InStr(variableData, ";");
        if (scp == 0)
        {
            return SetResult.Error;
        }

        var name = Strings.Trim(Strings.Left(variableData, scp - 1));
        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var pos = Strings.InStr(name, "[");
            name = Strings.Left(name, pos - 1);
        }

        var contents = Strings.Trim(Strings.Mid(variableData, scp + 1));

        for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
        {
            if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                await ExecSetString(variableData, ctx);
                return SetResult.Found;
            }
        }

        for (int i = 1, loopTo1 = _numberNumericVariables; i <= loopTo1; i++)
        {
            if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                await ExecSetVar(variableData, ctx);
                return SetResult.Found;
            }
        }

        for (int i = 1, loopTo2 = _numCollectables; i <= loopTo2; i++)
        {
            if ((Strings.LCase(_collectables[i].Name) ?? "") == (Strings.LCase(name) ?? ""))
            {
                ExecuteSetCollectable(variableData, ctx);
                return SetResult.Found;
            }
        }

        return SetResult.Unfound;
    }

    private async Task ExecSetString(string info, Context ctx)
    {
        // Sets string contents from a script parameter.
        // Eg <string1;contents> sets string variable string1
        // to "contents"

        var scp = Strings.InStr(info, ";");
        var name = Strings.Trim(Strings.Left(info, scp - 1));
        var value = Strings.Mid(info, scp + 1);

        if (Information.IsNumeric(name))
        {
            LogASLError("Invalid string name '" + name + "' - string names cannot be numeric", LogType.WarningError);
            return;
        }

        if (ASLVersion >= 281)
        {
            value = Strings.Trim(value);
            if ((Strings.Left(value, 1) == "[") & (Strings.Right(value, 1) == "]"))
            {
                value = Strings.Mid(value, 2, Strings.Len(value) - 2);
            }
        }

        var idx = GetArrayIndex(name, ctx);
        await SetStringContents(idx.Name, value, ctx, idx.Index);
    }

    private string GetStringContents(string name, Context ctx)
    {
        var returnAlias = false;
        var arrayIndex = 0;

        // Check for property shortcut
        var cp = Strings.InStr(name, ":");
        if (cp != 0)
        {
            var objName = Strings.Trim(Strings.Left(name, cp - 1));
            var propName = Strings.Trim(Strings.Mid(name, cp + 1));

            var obp = Strings.InStr(objName, "(");
            if (obp != 0)
            {
                var cbp = Strings.InStr(obp, objName, ")");
                if (cbp != 0)
                {
                    objName = GetStringContents(Strings.Mid(objName, obp + 1, cbp - obp - 1), ctx);
                }
            }

            return GetObjectProperty(propName, GetObjectIdNoAlias(objName));
        }

        if (Strings.Left(name, 1) == "@")
        {
            returnAlias = true;
            name = Strings.Mid(name, 2);
        }

        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var bp = Strings.InStr(name, "[");
            var ep = Strings.InStr(name, "]");
            var arrayIndexData = Strings.Mid(name, bp + 1, ep - bp - 1);
            if (Information.IsNumeric(arrayIndexData))
            {
                arrayIndex = Conversions.ToInteger(arrayIndexData);
            }
            else
            {
                arrayIndex = (int) Math.Round(GetNumericContents(arrayIndexData, ctx));
                if (arrayIndex == -32767)
                {
                    LogASLError(
                        "Array index in '" + name +
                        "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)",
                        LogType.WarningError);
                    return "";
                }
            }

            name = Strings.Left(name, bp - 1);
        }

        // First, see if the string already exists. If it does,
        // get its contents. If not, generate an error.

        var exists = false;
        var id = default(int);

        if (_numberStringVariables > 0)
        {
            for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            LogASLError("No string variable '" + name + "' defined.", LogType.WarningError);
            return "";
        }

        if (arrayIndex > _stringVariable[id].VariableUBound)
        {
            LogASLError("Array index of '" + name + "[" + Strings.Trim(InvariantStrings.Str(arrayIndex)) + "]' too big.",
                LogType.WarningError);
            return "";
        }

        // Now, set the contents
        if (!returnAlias)
        {
            return _stringVariable[id].VariableContents[arrayIndex];
        }

        return _objs[GetObjectIdNoAlias(_stringVariable[id].VariableContents[arrayIndex])].ObjectAlias;
    }

    internal async Task SetStringContents(string name, string value, Context ctx, int arrayIndex = 0)
    {
        var id = default(int);
        var exists = false;

        if (string.IsNullOrEmpty(name))
        {
            LogASLError("Internal error - tried to set empty string name to '" + value + "'", LogType.WarningError);
            return;
        }

        if (ASLVersion >= 281)
        {
            var bp = Strings.InStr(name, "[");
            if (bp != 0)
            {
                arrayIndex = GetArrayIndex(name, ctx).Index;
                name = Strings.Left(name, bp - 1);
            }
        }

        if (arrayIndex < 0)
        {
            LogASLError(
                "'" + name + "[" + Strings.Trim(InvariantStrings.Str(arrayIndex)) + "]' is invalid - did not assign to array",
                LogType.WarningError);
            return;
        }

        // First, see if the string already exists. If it does,
        // modify it. If not, create it.

        if (_numberStringVariables > 0)
        {
            for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            _numberStringVariables = _numberStringVariables + 1;
            id = _numberStringVariables;
            Array.Resize(ref _stringVariable, id + 1);
            _stringVariable[id] = new VariableType();
            _stringVariable[id].VariableUBound = arrayIndex;
        }

        if (arrayIndex > _stringVariable[id].VariableUBound)
        {
            Array.Resize(ref _stringVariable[id].VariableContents, arrayIndex + 1);
            _stringVariable[id].VariableUBound = arrayIndex;
        }

        // Now, set the contents
        _stringVariable[id].VariableName = name;
        Array.Resize(ref _stringVariable[id].VariableContents, _stringVariable[id].VariableUBound + 1);
        _stringVariable[id].VariableContents[arrayIndex] = value;

        if (!string.IsNullOrEmpty(_stringVariable[id].OnChangeScript))
        {
            var script = _stringVariable[id].OnChangeScript;
            await ExecuteScript(script, ctx);
        }

        if (!string.IsNullOrEmpty(_stringVariable[id].DisplayString))
        {
            await UpdateStatusVars(ctx);
        }
    }
}
