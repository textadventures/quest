Option Strict On
Option Explicit On
Option Infer On

Imports System.Collections.Generic
Imports System.Linq
Imports System.Net

Public Class LegacyGame

    Implements IASL
    Implements IASLTimer

    Public Enum State
        Ready       ' game is not doing any processing, and is ready for a command
        Working     ' game is processing a command
        Waiting     ' while processing a command, game has encountered e.g. an "enter" script, and is awaiting further input
        Finished    ' game is over
    End Enum

    Private Class DefineBlock
        Public StartLine As Integer
        Public EndLine As Integer
    End Class

    Friend Class Context
        Public CallingObjectId As Integer
        Public NumParameters As Integer
        Public Parameters() As String
        Public FunctionReturnValue As String
        Public AllowRealNamesInCommand As Boolean
        Public DontProcessCommand As Boolean
        Public CancelExec As Boolean
        Public StackCounter As Integer
    End Class

    Private Function CopyContext(ctx As Context) As Context
        Dim result As Context = New Context
        result.CallingObjectId = ctx.CallingObjectId
        result.NumParameters = ctx.NumParameters
        result.Parameters = ctx.Parameters
        result.FunctionReturnValue = ctx.FunctionReturnValue
        result.AllowRealNamesInCommand = ctx.AllowRealNamesInCommand
        result.DontProcessCommand = ctx.DontProcessCommand
        result.CancelExec = ctx.CancelExec
        result.StackCounter = ctx.StackCounter
        Return result
    End Function

    Friend Enum LogType
        Misc
        FatalError
        WarningError
        Init
        LibraryWarningError
        Warning
        UserError
        InternalError
    End Enum

    Private _defineBlockParams As Dictionary(Of String, Dictionary(Of String, String))

    Friend Enum Direction
        None = -1
        Out = 0
        North = 1
        South = 2
        East = 3
        West = 4
        NorthWest = 5
        NorthEast = 6
        SouthWest = 7
        SouthEast = 8
        Up = 9
        Down = 10
    End Enum

    Private Class ItemType
        Public Name As String
        Public Got As Boolean
    End Class

    Private Class Collectable
        Public Name As String
        Public Type As String
        Public Value As Double
        Public Display As String
        Public DisplayWhenZero As Boolean
    End Class

    Friend Class PropertyType
        Public PropertyName As String
        Public PropertyValue As String
    End Class

    Friend Class ActionType
        Public ActionName As String
        Public Script As String
    End Class

    Friend Class UseDataType
        Public UseObject As String
        Public UseType As UseType
        Public UseScript As String
    End Class

    Friend Class GiveDataType
        Public GiveObject As String
        Public GiveType As GiveType
        Public GiveScript As String
    End Class

    Private Class PropertiesActions
        Public Properties As String
        Public NumberActions As Integer
        Public Actions() As ActionType
        Public NumberTypesIncluded As Integer
        Public TypesIncluded() As String
    End Class

    Private Class VariableType
        Public VariableName As String
        Public VariableContents() As String
        Public VariableUBound As Integer
        Public DisplayString As String
        Public OnChangeScript As String
        Public NoZeroDisplay As Boolean
    End Class

    Private Class SynonymType
        Public OriginalWord As String
        Public ConvertTo As String
    End Class

    Private Class TimerType
        Public TimerName As String
        Public TimerInterval As Integer
        Public TimerActive As Boolean
        Public TimerAction As String
        Public TimerTicks As Integer
        Public BypassThisTurn As Boolean
    End Class

    Friend Class UserDefinedCommandType
        Public CommandText As String
        Public CommandScript As String
    End Class

    Friend Class TextAction
        Public Data As String
        Public Type As TextActionType
    End Class

    Friend Enum TextActionType
        Text
        Script
        [Nothing]
        [Default]
    End Enum

    Friend Class ScriptText
        Public Text As String
        Public Script As String
    End Class

    Friend Class PlaceType
        Public PlaceName As String
        Public Prefix As String
        Public PlaceAlias As String
        Public Script As String
    End Class

    Friend Class RoomType
        Public RoomName As String
        Public RoomAlias As String
        Public Commands() As UserDefinedCommandType
        Public NumberCommands As Integer
        Public Description As New TextAction
        Public Out As New ScriptText
        Public East As New TextAction
        Public West As New TextAction
        Public North As New TextAction
        Public South As New TextAction
        Public NorthEast As New TextAction
        Public NorthWest As New TextAction
        Public SouthEast As New TextAction
        Public SouthWest As New TextAction
        Public Up As New TextAction
        Public Down As New TextAction
        Public InDescription As String
        Public Look As String
        Public Places() As PlaceType
        Public NumberPlaces As Integer
        Public Prefix As String
        Public Script As String
        Public Use() As ScriptText
        Public NumberUse As Integer
        Public ObjId As Integer
        Public BeforeTurnScript As String
        Public AfterTurnScript As String
        Public Exits As RoomExits
    End Class

    Friend Class ObjectType
        Public ObjectName As String
        Public ObjectAlias As String
        Public Detail As String
        Public ContainerRoom As String
        Public Exists As Boolean
        Public IsGlobal As Boolean
        Public Prefix As String
        Public Suffix As String
        Public Gender As String
        Public Article As String
        Public DefinitionSectionStart As Integer
        Public DefinitionSectionEnd As Integer
        Public Visible As Boolean
        Public GainScript As String
        Public LoseScript As String
        Public NumberProperties As Integer
        Public Properties() As PropertyType
        Public Speak As New TextAction
        Public Take As New TextAction
        Public IsRoom As Boolean
        Public IsExit As Boolean
        Public CorresRoom As String
        Public CorresRoomId As Integer
        Public Loaded As Boolean
        Public NumberActions As Integer
        Public Actions() As ActionType
        Public NumberUseData As Integer
        Public UseData() As UseDataType
        Public UseAnything As String
        Public UseOnAnything As String
        Public Use As String
        Public NumberGiveData As Integer
        Public GiveData() As GiveDataType
        Public GiveAnything As String
        Public GiveToAnything As String
        Public DisplayType As String
        Public NumberTypesIncluded As Integer
        Public TypesIncluded() As String
        Public NumberAltNames As Integer
        Public AltNames() As String
        Public AddScript As New TextAction
        Public RemoveScript As New TextAction
        Public OpenScript As New TextAction
        Public CloseScript As New TextAction
    End Class

    Private Class ChangeType
        Public AppliesTo As String
        Public Change As String
    End Class

    Private Class GameChangeDataType
        Public NumberChanges As Integer
        Public ChangeData() As ChangeType
    End Class

    Private Class ResourceType
        Public ResourceName As String
        Public ResourceStart As Integer
        Public ResourceLength As Integer
        Public Extracted As Boolean
    End Class

    Private Class ExpressionResult
        Public Result As String
        Public Success As ExpressionSuccess
        Public Message As String
    End Class

    Friend Enum PlayerError
        BadCommand
        BadGo
        BadGive
        BadCharacter
        NoItem
        ItemUnwanted
        BadLook
        BadThing
        DefaultLook
        DefaultSpeak
        BadItem
        DefaultTake
        BadUse
        DefaultUse
        DefaultOut
        BadPlace
        BadExamine
        DefaultExamine
        BadTake
        CantDrop
        DefaultDrop
        BadDrop
        BadPronoun
        AlreadyOpen
        AlreadyClosed
        CantOpen
        CantClose
        DefaultOpen
        DefaultClose
        BadPut
        CantPut
        DefaultPut
        CantRemove
        AlreadyPut
        DefaultRemove
        Locked
        DefaultWait
        AlreadyTaken
    End Enum

    Private Enum ItType
        Inanimate
        Male
        Female
    End Enum

    Private Enum SetResult
        [Error]
        Found
        Unfound
    End Enum

    Private Enum Thing
        Character
        [Object]
        Room
    End Enum

    Private Enum ConvertType
        Strings
        Functions
        Numeric
        Collectables
    End Enum

    Friend Enum UseType
        UseOnSomething
        UseSomethingOn
    End Enum

    Friend Enum GiveType
        GiveToSomething
        GiveSomethingTo
    End Enum

    Private Enum VarType
        [String]
        Numeric
    End Enum

    Private Enum StopType
        Win
        Lose
        Null
    End Enum

    Private Enum ExpressionSuccess
        OK
        Fail
    End Enum

    Private _openErrorReport As String
    Private _casKeywords(255) As String 'Tokenised CAS keywords
    Private _lines() As String 'Stores the lines of the ASL script/definitions
    Private _defineBlocks() As DefineBlock 'Stores the start and end lines of each 'define' section
    Private _numberSections As Integer 'Number of define sections
    Private _gameName As String 'The name of the game
    Friend _nullContext As New Context
    Private _changeLogRooms As ChangeLog
    Private _changeLogObjects As ChangeLog
    Private _defaultProperties As PropertiesActions
    Private _defaultRoomProperties As PropertiesActions
    Friend _rooms() As RoomType
    Friend _numberRooms As Integer
    Private _numericVariable() As VariableType
    Private _numberNumericVariables As Integer
    Private _stringVariable() As VariableType
    Private _numberStringVariables As Integer
    Private _synonyms() As SynonymType
    Private _numberSynonyms As Integer
    Private _items() As ItemType
    Private _chars() As ObjectType
    Friend _objs() As ObjectType
    Private _numberChars As Integer
    Friend _numberObjs As Integer
    Private _numberItems As Integer
    Friend _currentRoom As String
    Private _collectables() As Collectable
    Private _numCollectables As Integer
    Private _gamePath As String
    Private _gameFileName As String
    Private _saveGameFile As String
    Private _defaultFontName As String
    Private _defaultFontSize As Double
    Private _autoIntro As Boolean
    Private _commandOverrideModeOn As Boolean
    Private _commandOverrideVariable As String
    Private _afterTurnScript As String
    Private _beforeTurnScript As String
    Private _outPutOn As Boolean
    Private _gameAslVersion As Integer
    Private _choiceNumber As Integer
    Private _gameLoadMethod As String    ' TODO: Make enum
    Private _timers() As TimerType
    Private _numberTimers As Integer
    Private _numDisplayStrings As Integer
    Private _numDisplayNumerics As Integer
    Private _gameFullyLoaded As Boolean
    Private _gameChangeData As New GameChangeDataType
    Private _lastIt As Integer
    Private _lastItMode As ItType
    Private _thisTurnIt As Integer
    Private _thisTurnItMode As ItType
    Private _badCmdBefore As String
    Private _badCmdAfter As String
    Private _numResources As Integer
    Private _resources() As ResourceType
    Private _resourceFile As String
    Private _resourceOffset As Integer
    Private _startCatPos As Integer
    Private _useAbbreviations As Boolean
    Private _loadedFromQsg As Boolean  ' TODO: Same as _gameLoadMethod
    Private _beforeSaveScript As String
    Private _onLoadScript As String
    Private _numSkipCheckFiles As Integer
    Private _skipCheckFile() As String
    Private _compassExits As New List(Of ListData)
    Private _gotoExits As New List(Of ListData)
    Private _textFormatter As New TextFormatter
    Private _log As New List(Of String)
    Private _casFileData As String
    Private _commandLock As Object = New Object
    Private _stateLock As Object = New Object
    Private _state As State = State.Ready
    Private _waitLock As Object = New Object
    Private _readyForCommand As Boolean = True
    Private _gameLoading As Boolean
    Private _random As New Random()
    Private _tempFolder As String
    Private _playerErrorMessageString(38) As String
    Private _listVerbs As New Dictionary(Of ListType, List(Of String))
    Private _filename As String
    Private _originalFilename As String
    Private _data As InitGameData
    Private _player As IPlayer
    Private _gameFinished As Boolean
    Private _gameIsRestoring As Boolean
    Private _useStaticFrameForPictures As Boolean
    Private _fileData As String
    Private _fileDataPos As Integer
    Private _questionResponse As Boolean

    Public Sub New(filename As String, originalFilename As String)
        _tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath, "Quest", Guid.NewGuid().ToString())
        LoadCASKeywords()
        _gameLoadMethod = "normal"
        _filename = filename
        _originalFilename = originalFilename

        ' Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
        ' for compatibility with games which have non-fatal errors in them.
        _numSkipCheckFiles = 3
        ReDim _skipCheckFile(3)
        _skipCheckFile(1) = "bargain.cas"
        _skipCheckFile(2) = "easymoney.asl"
        _skipCheckFile(3) = "musicvf1.cas"
    End Sub

    Public Class InitGameData
        Public Data As Byte()
        Public SourceFile As String
    End Class

    Public Sub New(data As InitGameData)
        Me.New(Nothing, Nothing)
        _data = data
    End Sub

    Private Function RemoveFormatting(s As String) As String
        Dim code As String
        Dim pos, len As Integer

        Do
            pos = InStr(s, "|")
            If pos <> 0 Then
                code = Mid(s, pos + 1, 3)

                If Left(code, 1) = "b" Then
                    len = 1
                ElseIf Left(code, 2) = "xb" Then
                    len = 2
                ElseIf Left(code, 1) = "u" Then
                    len = 1
                ElseIf Left(code, 2) = "xu" Then
                    len = 2
                ElseIf Left(code, 1) = "i" Then
                    len = 1
                ElseIf Left(code, 2) = "xi" Then
                    len = 2
                ElseIf Left(code, 2) = "cr" Then
                    len = 2
                ElseIf Left(code, 2) = "cb" Then
                    len = 2
                ElseIf Left(code, 2) = "cl" Then
                    len = 2
                ElseIf Left(code, 2) = "cy" Then
                    len = 2
                ElseIf Left(code, 2) = "cg" Then
                    len = 2
                ElseIf Left(code, 1) = "n" Then
                    len = 1
                ElseIf Left(code, 2) = "xn" Then
                    len = 2
                ElseIf Left(code, 1) = "s" Then
                    len = 3
                ElseIf Left(code, 2) = "jc" Then
                    len = 2
                ElseIf Left(code, 2) = "jl" Then
                    len = 2
                ElseIf Left(code, 2) = "jr" Then
                    len = 2
                ElseIf Left(code, 1) = "w" Then
                    len = 1
                ElseIf Left(code, 1) = "c" Then
                    len = 1
                End If

                If len = 0 Then
                    ' unknown code
                    len = 1
                End If

                s = Left(s, pos - 1) & Mid(s, pos + len + 1)
            End If

        Loop Until pos = 0

        Return s
    End Function

    Private Function CheckSections() As Boolean
        Dim defines, braces As Integer
        Dim checkLine As String = ""
        Dim bracePos As Integer
        Dim pos As Integer
        Dim section As String = ""
        Dim hasErrors As Boolean
        Dim skipBlock As Boolean
        _openErrorReport = ""
        hasErrors = False
        defines = 0
        braces = 0

        For i = 1 To UBound(_lines)
            If Not BeginsWith(_lines(i), "#!qdk-note: ") Then
                If BeginsWith(_lines(i), "define ") Then
                    section = _lines(i)
                    braces = 0
                    defines = defines + 1
                    skipBlock = BeginsWith(_lines(i), "define text") Or BeginsWith(_lines(i), "define synonyms")
                ElseIf Trim(_lines(i)) = "end define" Then
                    defines = defines - 1

                    If defines < 0 Then
                        LogASLError("Extra 'end define' after block '" & section & "'", LogType.FatalError)
                        _openErrorReport = _openErrorReport & "Extra 'end define' after block '" & section & "'" & vbCrLf
                        hasErrors = True
                        defines = 0
                    End If

                    If braces > 0 Then
                        LogASLError("Missing } in block '" & section & "'", LogType.FatalError)
                        _openErrorReport = _openErrorReport & "Missing } in block '" & section & "'" & vbCrLf
                        hasErrors = True
                    ElseIf braces < 0 Then
                        LogASLError("Too many } in block '" & section & "'", LogType.FatalError)
                        _openErrorReport = _openErrorReport & "Too many } in block '" & section & "'" & vbCrLf
                        hasErrors = True
                    End If
                End If

                If Left(_lines(i), 1) <> "'" And Not skipBlock Then
                    checkLine = ObliterateParameters(_lines(i))
                    If BeginsWith(checkLine, "'<ERROR;") Then
                        ' ObliterateParameters denotes a mismatched $, ( etc.
                        ' by prefixing line with '<ERROR;*; where * is the mismatched
                        ' character
                        LogASLError("Expected closing " & Mid(checkLine, 9, 1) & " character in '" & ReportErrorLine(_lines(i)) & "'", LogType.FatalError)
                        _openErrorReport = _openErrorReport & "Expected closing " & Mid(checkLine, 9, 1) & " character in '" & ReportErrorLine(_lines(i)) & "'." & vbCrLf
                        Return False
                    End If
                End If

                If Left(Trim(checkLine), 1) <> "'" Then
                    ' Now check {
                    pos = 1
                    Do
                        bracePos = InStr(pos, checkLine, "{")
                        If bracePos <> 0 Then
                            pos = bracePos + 1
                            braces = braces + 1
                        End If
                    Loop Until bracePos = 0 Or pos > Len(checkLine)

                    ' Now check }
                    pos = 1
                    Do
                        bracePos = InStr(pos, checkLine, "}")
                        If bracePos <> 0 Then
                            pos = bracePos + 1
                            braces = braces - 1
                        End If
                    Loop Until bracePos = 0 Or pos > Len(checkLine)
                End If
            End If
        Next i

        If defines > 0 Then
            LogASLError("Missing 'end define'", LogType.FatalError)
            _openErrorReport = _openErrorReport & "Missing 'end define'." & vbCrLf
            hasErrors = True
        End If

        Return Not hasErrors
    End Function

    Private Function ConvertFriendlyIfs() As Boolean
        ' Converts
        '   if (%something% < 3) then ...
        ' to
        '   if is <%something%;lt;3> then ...
        ' and also repeat until ...

        ' Returns False if successful

        Dim convPos, symbPos As Integer
        Dim symbol As String
        Dim endParamPos As Integer
        Dim paramData As String
        Dim startParamPos As Integer
        Dim firstData, secondData As String
        Dim obscureLine, newParam, varObscureLine As String
        Dim bracketCount As Integer

        For i = 1 To UBound(_lines)
            obscureLine = ObliterateParameters(_lines(i))
            convPos = InStr(obscureLine, "if (")
            If convPos = 0 Then
                convPos = InStr(obscureLine, "until (")
            End If
            If convPos = 0 Then
                convPos = InStr(obscureLine, "while (")
            End If
            If convPos = 0 Then
                convPos = InStr(obscureLine, "not (")
            End If
            If convPos = 0 Then
                convPos = InStr(obscureLine, "and (")
            End If
            If convPos = 0 Then
                convPos = InStr(obscureLine, "or (")
            End If


            If convPos <> 0 Then
                varObscureLine = ObliterateVariableNames(_lines(i))
                If BeginsWith(varObscureLine, "'<ERROR;") Then
                    ' ObliterateVariableNames denotes a mismatched #, % or $
                    ' by prefixing line with '<ERROR;*; where * is the mismatched
                    ' character
                    LogASLError("Expected closing " & Mid(varObscureLine, 9, 1) & " character in '" & ReportErrorLine(_lines(i)) & "'", LogType.FatalError)
                    Return True
                End If
                startParamPos = InStr(convPos, _lines(i), "(")

                endParamPos = 0
                bracketCount = 1
                For j = startParamPos + 1 To Len(_lines(i))
                    If Mid(_lines(i), j, 1) = "(" Then
                        bracketCount = bracketCount + 1
                    ElseIf Mid(_lines(i), j, 1) = ")" Then
                        bracketCount = bracketCount - 1
                    End If
                    If bracketCount = 0 Then
                        endParamPos = j
                        Exit For
                    End If
                Next j

                If endParamPos = 0 Then
                    LogASLError("Expected ) in '" & ReportErrorLine(_lines(i)) & "'", LogType.FatalError)
                    Return True
                End If

                paramData = Mid(_lines(i), startParamPos + 1, (endParamPos - startParamPos) - 1)

                symbPos = InStr(paramData, "!=")
                If symbPos = 0 Then
                    symbPos = InStr(paramData, "<>")
                    If symbPos = 0 Then
                        symbPos = InStr(paramData, "<=")
                        If symbPos = 0 Then
                            symbPos = InStr(paramData, ">=")
                            If symbPos = 0 Then
                                symbPos = InStr(paramData, "<")
                                If symbPos = 0 Then
                                    symbPos = InStr(paramData, ">")
                                    If symbPos = 0 Then
                                        symbPos = InStr(paramData, "=")
                                        If symbPos = 0 Then
                                            LogASLError("Unrecognised 'if' condition in '" & ReportErrorLine(_lines(i)) & "'", LogType.FatalError)
                                            Return True
                                        Else
                                            symbol = "="
                                        End If
                                    Else
                                        symbol = ">"
                                    End If
                                Else
                                    symbol = "<"
                                End If
                            Else
                                symbol = ">="
                            End If
                        Else
                            symbol = "<="
                        End If
                    Else
                        symbol = "<>"
                    End If
                Else
                    symbol = "<>"
                End If


                firstData = Trim(Left(paramData, symbPos - 1))
                secondData = Trim(Mid(paramData, symbPos + Len(symbol)))

                If symbol = "=" Then
                    newParam = "is <" & firstData & ";" & secondData & ">"
                Else
                    newParam = "is <" & firstData & ";"
                    If symbol = "<" Then
                        newParam = newParam & "lt"
                    ElseIf symbol = ">" Then
                        newParam = newParam & "gt"
                    ElseIf symbol = ">=" Then
                        newParam = newParam & "gt="
                    ElseIf symbol = "<=" Then
                        newParam = newParam & "lt="
                    ElseIf symbol = "<>" Then
                        newParam = newParam & "!="
                    End If
                    newParam = newParam & ";" & secondData & ">"
                End If

                _lines(i) = Left(_lines(i), startParamPos - 1) & newParam & Mid(_lines(i), endParamPos + 1)

                ' Repeat processing this line, in case there are
                ' further changes to be made.
                i = i - 1
            End If
        Next i

        Return False
    End Function

    Private Sub ConvertMultiLineSections()
        Dim startLine, braceCount As Integer
        Dim thisLine, lineToAdd As String
        Dim lastBrace As Integer
        Dim i As Integer
        Dim restOfLine, procName As String
        Dim endLineNum As Integer
        Dim afterLastBrace, z As String
        Dim startOfOrig As String
        Dim testLine As String
        Dim testBraceCount As Integer
        Dim obp, cbp As Integer
        Dim curProc As Integer

        i = 1
        Do
            z = _lines(_defineBlocks(i).StartLine)
            If ((Not BeginsWith(z, "define text ")) And (Not BeginsWith(z, "define menu ")) And z <> "define synonyms") Then
                For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                    If InStr(_lines(j), "{") > 0 Then

                        afterLastBrace = ""
                        thisLine = Trim(_lines(j))

                        procName = "<!intproc" & curProc & ">"

                        ' see if this brace's corresponding closing
                        ' brace is on same line:

                        testLine = Mid(_lines(j), InStr(_lines(j), "{") + 1)
                        testBraceCount = 1
                        Do
                            obp = InStr(testLine, "{")
                            cbp = InStr(testLine, "}")
                            If obp = 0 Then obp = Len(testLine) + 1
                            If cbp = 0 Then cbp = Len(testLine) + 1
                            If obp < cbp Then
                                testBraceCount = testBraceCount + 1
                                testLine = Mid(testLine, obp + 1)
                            ElseIf cbp < obp Then
                                testBraceCount = testBraceCount - 1
                                testLine = Mid(testLine, cbp + 1)
                            End If
                        Loop Until obp = cbp Or testBraceCount = 0

                        If testBraceCount <> 0 Then
                            AddLine("define procedure " & procName)
                            startLine = UBound(_lines)
                            restOfLine = Trim(Right(thisLine, Len(thisLine) - InStr(thisLine, "{")))
                            braceCount = 1
                            If restOfLine <> "" Then AddLine(restOfLine)

                            For m = 1 To Len(restOfLine)
                                If Mid(restOfLine, m, 1) = "{" Then
                                    braceCount = braceCount + 1
                                ElseIf Mid(restOfLine, m, 1) = "}" Then
                                    braceCount = braceCount - 1
                                End If
                            Next m

                            If braceCount <> 0 Then
                                Dim k = j + 1
                                Do
                                    For m = 1 To Len(_lines(k))
                                        If Mid(_lines(k), m, 1) = "{" Then
                                            braceCount = braceCount + 1
                                        ElseIf Mid(_lines(k), m, 1) = "}" Then
                                            braceCount = braceCount - 1
                                        End If

                                        If braceCount = 0 Then
                                            lastBrace = m
                                            Exit For
                                        End If
                                    Next m

                                    If braceCount <> 0 Then
                                        'put Lines(k) into another variable, as
                                        'AddLine ReDims Lines, which it can't do if
                                        'passed Lines(x) as a parameter.
                                        lineToAdd = _lines(k)
                                        AddLine(lineToAdd)
                                    Else
                                        AddLine(Left(_lines(k), lastBrace - 1))
                                        afterLastBrace = Trim(Mid(_lines(k), lastBrace + 1))
                                    End If

                                    'Clear original line
                                    _lines(k) = ""
                                    k = k + 1
                                Loop While braceCount <> 0
                            End If

                            AddLine("end define")
                            endLineNum = UBound(_lines)

                            _numberSections = _numberSections + 1
                            ReDim Preserve _defineBlocks(_numberSections)
                            _defineBlocks(_numberSections) = New DefineBlock
                            _defineBlocks(_numberSections).StartLine = startLine
                            _defineBlocks(_numberSections).EndLine = endLineNum

                            'Change original line where the { section
                            'started to call the new procedure.
                            startOfOrig = Trim(Left(thisLine, InStr(thisLine, "{") - 1))
                            _lines(j) = startOfOrig & " do " & procName & " " & afterLastBrace
                            curProc = curProc + 1

                            ' Process this line again in case there was stuff after the last brace that included
                            ' more braces. e.g. } else {
                            j = j - 1
                        End If
                    End If
                Next j
            End If
            i = i + 1
        Loop Until i > _numberSections

        ' Join next-line "else"s to corresponding "if"s

        For i = 1 To _numberSections
            z = _lines(_defineBlocks(i).StartLine)
            If ((Not BeginsWith(z, "define text ")) And (Not BeginsWith(z, "define menu ")) And z <> "define synonyms") Then
                For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                    If BeginsWith(_lines(j), "else ") Then

                        'Go upwards to find "if" statement that this
                        'belongs to

                        For k = j To _defineBlocks(i).StartLine + 1 Step -1
                            If BeginsWith(_lines(k), "if ") Or InStr(ObliterateParameters(_lines(k)), " if ") <> 0 Then
                                _lines(k) = _lines(k) & " " & Trim(_lines(j))
                                _lines(j) = ""
                                k = _defineBlocks(i).StartLine
                            End If
                        Next k
                    End If
                Next j
            End If
        Next i


    End Sub

    Private Function ErrorCheck() As Boolean
        ' Parses ASL script for errors. Returns TRUE if OK;
        ' False if a critical error is encountered.
        Dim curBegin, curEnd As Integer
        Dim hasErrors As Boolean
        Dim curPos As Integer
        Dim numParamStart, numParamEnd As Integer
        Dim finLoop, inText As Boolean

        hasErrors = False
        inText = False

        ' Checks for incorrect number of < and > :
        For i = 1 To UBound(_lines)
            numParamStart = 0
            numParamEnd = 0

            If BeginsWith(_lines(i), "define text ") Then
                inText = True
            End If
            If inText And Trim(LCase(_lines(i))) = "end define" Then
                inText = False
            End If

            If Not inText Then
                'Find number of <'s:
                curPos = 1
                finLoop = False
                Do
                    If InStr(curPos, _lines(i), "<") <> 0 Then
                        numParamStart = numParamStart + 1
                        curPos = InStr(curPos, _lines(i), "<") + 1
                    Else
                        finLoop = True
                    End If
                Loop Until finLoop

                'Find number of >'s:
                curPos = 1
                finLoop = False
                Do
                    If InStr(curPos, _lines(i), ">") <> 0 Then
                        numParamEnd = numParamEnd + 1
                        curPos = InStr(curPos, _lines(i), ">") + 1
                    Else
                        finLoop = True
                    End If
                Loop Until finLoop

                If numParamStart > numParamEnd Then
                    LogASLError("Expected > in " & ReportErrorLine(_lines(i)), LogType.FatalError)
                    hasErrors = True
                ElseIf numParamStart < numParamEnd Then
                    LogASLError("Too many > in " & ReportErrorLine(_lines(i)), LogType.FatalError)
                    hasErrors = True
                End If
            End If
        Next i

        'Exit if errors found
        If hasErrors = True Then
            Return True
        End If

        ' Checks that define sections have parameters:
        For i = 1 To _numberSections
            curBegin = _defineBlocks(i).StartLine
            curEnd = _defineBlocks(i).EndLine

            If BeginsWith(_lines(curBegin), "define game") Then
                If InStr(_lines(curBegin), "<") = 0 Then
                    LogASLError("'define game' has no parameter - game has no name", LogType.FatalError)
                    Return True
                End If
            Else
                If Not BeginsWith(_lines(curBegin), "define synonyms") And Not BeginsWith(_lines(curBegin), "define options") Then
                    If InStr(_lines(curBegin), "<") = 0 Then
                        LogASLError(_lines(curBegin) & " has no parameter", LogType.FatalError)
                        hasErrors = True
                    End If
                End If
            End If
        Next i

        Return hasErrors
    End Function

    Private Function GetAfterParameter(s As String) As String
        ' Returns everything after the end of the first parameter
        ' in a string, i.e. for "use <thing> do <myproc>" it
        ' returns "do <myproc>"
        Dim eop As Integer
        eop = InStr(s, ">")

        If eop = 0 Or eop + 1 > Len(s) Then
            Return ""
        Else
            Return Trim(Mid(s, eop + 1))
        End If

    End Function

    Private Function ObliterateParameters(s As String) As String

        Dim inParameter As Boolean
        Dim exitCharacter As String = ""
        Dim curChar As String
        Dim outputLine As String = ""
        Dim obscuringFunctionName As Boolean

        inParameter = False

        For i = 1 To Len(s)
            curChar = Mid(s, i, 1)

            If inParameter Then
                If exitCharacter = ")" Then
                    If InStr("$#%", curChar) > 0 Then
                        ' We might be converting a line like:
                        '   if ( $rand(1;10)$ < 3 ) then {
                        ' and we don't want it to end up like this:
                        '   if (~~~~~~~~~~~)$ <~~~~~~~~~~~
                        ' which will cause all sorts of confustion. So,
                        ' we get rid of everything between the $ characters
                        ' in this case, and set a flag so we know what we're
                        ' doing.

                        obscuringFunctionName = True
                        exitCharacter = curChar

                        ' Move along please

                        outputLine = outputLine & "~"
                        i = i + 1
                        curChar = Mid(s, i, 1)
                    End If
                End If
            End If

            If Not inParameter Then
                outputLine = outputLine & curChar
                If curChar = "<" Then
                    inParameter = True
                    exitCharacter = ">"
                End If
                If curChar = "(" Then
                    inParameter = True
                    exitCharacter = ")"
                End If
            Else
                If curChar = exitCharacter Then
                    If Not obscuringFunctionName Then
                        inParameter = False
                        outputLine = outputLine & curChar
                    Else
                        ' We've finished obscuring the function name,
                        ' now let's find the next ) as we were before
                        ' we found this dastardly function
                        obscuringFunctionName = False
                        exitCharacter = ")"
                        outputLine = outputLine & "~"
                    End If
                Else
                    outputLine = outputLine & "~"
                End If
            End If
        Next i

        If inParameter Then
            Return "'<ERROR;" & exitCharacter & ";" & outputLine
        Else
            Return outputLine
        End If

    End Function

    Private Function ObliterateVariableNames(s As String) As String
        Dim inParameter As Boolean
        Dim exitCharacter As String = ""
        Dim outputLine As String = ""
        Dim curChar As String

        inParameter = False

        For i = 1 To Len(s)
            curChar = Mid(s, i, 1)
            If Not inParameter Then
                outputLine = outputLine & curChar
                If curChar = "$" Then
                    inParameter = True
                    exitCharacter = "$"
                End If
                If curChar = "#" Then
                    inParameter = True
                    exitCharacter = "#"
                End If
                If curChar = "%" Then
                    inParameter = True
                    exitCharacter = "%"
                End If
                ' The ~ was for collectables, and this syntax only
                ' exists in Quest 2.x. The ~ was only finally
                ' allowed to be present on its own in ASL 320.
                If curChar = "~" And _gameAslVersion < 320 Then
                    inParameter = True
                    exitCharacter = "~"
                End If
            Else
                If curChar = exitCharacter Then
                    inParameter = False
                    outputLine = outputLine & curChar
                Else
                    outputLine = outputLine & "X"
                End If
            End If
        Next i

        If inParameter Then
            outputLine = "'<ERROR;" & exitCharacter & ";" & outputLine
        End If

        Return outputLine

    End Function

    Private Sub RemoveComments()
        Dim aposPos As Integer
        Dim inTextBlock As Boolean
        Dim inSynonymsBlock As Boolean
        Dim oblitLine As String

        ' If in a synonyms block, we want to remove lines which are comments, but
        ' we don't want to remove synonyms that contain apostrophes, so we only
        ' get rid of lines with an "'" at the beginning or with " '" in them

        For i = 1 To UBound(_lines)

            If BeginsWith(_lines(i), "'!qdk-note:") Then
                _lines(i) = "#!qdk-note:" & GetEverythingAfter(_lines(i), "'!qdk-note:")
            Else
                If BeginsWith(_lines(i), "define text ") Then
                    inTextBlock = True
                ElseIf Trim(_lines(i)) = "define synonyms" Then
                    inSynonymsBlock = True
                ElseIf BeginsWith(_lines(i), "define type ") Then
                    inSynonymsBlock = True
                ElseIf Trim(_lines(i)) = "end define" Then
                    inTextBlock = False
                    inSynonymsBlock = False
                End If

                If Not inTextBlock And Not inSynonymsBlock Then
                    If InStr(_lines(i), "'") > 0 Then
                        oblitLine = ObliterateParameters(_lines(i))
                        If Not BeginsWith(oblitLine, "'<ERROR;") Then
                            aposPos = InStr(oblitLine, "'")

                            If aposPos <> 0 Then
                                _lines(i) = Trim(Left(_lines(i), aposPos - 1))
                            End If
                        End If
                    End If
                ElseIf inSynonymsBlock Then
                    If Left(Trim(_lines(i)), 1) = "'" Then
                        _lines(i) = ""
                    Else
                        ' we look for " '", not "'" in synonyms lines
                        aposPos = InStr(ObliterateParameters(_lines(i)), " '")
                        If aposPos <> 0 Then
                            _lines(i) = Trim(Left(_lines(i), aposPos - 1))
                        End If
                    End If
                End If
            End If

        Next i
    End Sub

    Private Function ReportErrorLine(s As String) As String
        ' We don't want to see the "!intproc" in logged error reports lines.
        ' This function replaces these "do" lines with a nicer-looking "..." for error reporting.

        Dim replaceFrom As Integer

        replaceFrom = InStr(s, "do <!intproc")
        If replaceFrom <> 0 Then
            Return Left(s, replaceFrom - 1) & "..."
        Else
            Return s
        End If
    End Function

    Private Function YesNo(yn As Boolean) As String
        If yn = True Then Return "Yes" Else Return "No"
    End Function

    Private Function IsYes(yn As String) As Boolean
        If LCase(yn) = "yes" Then IsYes = True Else IsYes = False
    End Function

    Friend Function BeginsWith(s As String, text As String) As Boolean
        ' Compares the beginning of the line with a given
        ' string. Case insensitive.

        ' Example: beginswith("Hello there","HeLlO")=TRUE

        Return Left(LTrim(LCase(s)), Len(text)) = LCase(text)
    End Function

    Private Function ConvertCasKeyword(casChar As String) As String
        Dim c As Byte = System.Text.Encoding.GetEncoding(1252).GetBytes(casChar)(0)
        Dim keyword As String = _casKeywords(c)

        If keyword = "!cr" Then
            keyword = vbCrLf
        End If

        Return keyword
    End Function

    Private Sub ConvertMultiLines()
        'Goes through each section capable of containing
        'script commands and puts any multiple-line script commands
        'into separate procedures. Also joins multiple-line "if"
        'statements.

        'This calls RemoveComments after joining lines, so that lines
        'with "'" as part of a multi-line parameter are not destroyed,
        'before looking for braces.

        For i = UBound(_lines) To 1 Step -1
            If Right(_lines(i), 2) = "__" Then
                _lines(i) = Left(_lines(i), Len(_lines(i)) - 2) & LTrim(_lines(i + 1))
                _lines(i + 1) = ""
                'Recalculate this line again
                i = i + 1
            ElseIf Right(_lines(i), 1) = "_" Then
                _lines(i) = Left(_lines(i), Len(_lines(i)) - 1) & LTrim(_lines(i + 1))
                _lines(i + 1) = ""
                'Recalculate this line again
                i = i + 1
            End If
        Next i

        RemoveComments()
    End Sub

    Private Function GetDefineBlock(blockname As String) As DefineBlock

        ' Returns the start and end points of a named block.
        ' Returns 0 if block not found.

        Dim l, blockType As String

        Dim result = New DefineBlock
        result.StartLine = 0
        result.EndLine = 0

        For i = 1 To _numberSections
            ' Get the first line of the define section:
            l = _lines(_defineBlocks(i).StartLine)

            ' Now, starting from the first word after 'define',
            ' retrieve the next word and compare it to blockname:

            ' Add a space for define blocks with no parameter
            If InStr(8, l, " ") = 0 Then l = l & " "

            blockType = Mid(l, 8, InStr(8, l, " ") - 8)

            If blockType = blockname Then
                ' Return the start and end points
                result.StartLine = _defineBlocks(i).StartLine
                result.EndLine = _defineBlocks(i).EndLine
                Return result
            End If
        Next i

        Return result
    End Function

    Private Function DefineBlockParam(blockname As String, param As String) As DefineBlock
        ' Returns the start and end points of a named block

        Dim cache As Dictionary(Of String, String)
        Dim result = New DefineBlock

        param = "k" & param ' protect against numeric block names

        If Not _defineBlockParams.ContainsKey(blockname) Then
            ' Lazily create cache of define block params

            cache = New Dictionary(Of String, String)
            _defineBlockParams.Add(blockname, cache)

            For i = 1 To _numberSections
                ' get the word after "define", e.g. "procedure"
                Dim blockType = GetEverythingAfter(_lines(_defineBlocks(i).StartLine), "define ")
                Dim sp = InStr(blockType, " ")
                If sp <> 0 Then
                    blockType = Trim(Left(blockType, sp - 1))
                End If

                If blockType = blockname Then
                    Dim blockKey = GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext, False)

                    blockKey = "k" & blockKey

                    If Not cache.ContainsKey(blockKey) Then
                        cache.Add(blockKey, _defineBlocks(i).StartLine & "," & _defineBlocks(i).EndLine)
                    Else
                        ' silently ignore duplicates
                    End If
                End If
            Next i
        Else
            cache = _defineBlockParams.Item(blockname)
        End If

        If cache.ContainsKey(param) Then
            Dim blocks = Split(cache.Item(param), ",")
            result.StartLine = CInt(blocks(0))
            result.EndLine = CInt(blocks(1))
        End If

        Return result

    End Function

    Friend Function GetEverythingAfter(s As String, text As String) As String
        If Len(text) > Len(s) Then
            Return ""
        End If
        Return Right(s, Len(s) - Len(text))
    End Function

    Private Function Keyword2CAS(KWord As String) As String
        Dim k = ""

        If KWord = "" Then
            Return ""
        End If

        For i = 0 To 255
            If LCase(KWord) = LCase(_casKeywords(i)) Then
                Return Chr(i)
            End If
        Next i

        Return Keyword2CAS("!unknown") & KWord & Keyword2CAS("!unknown")
    End Function

    Private Sub LoadCASKeywords()
        'Loads data required for conversion of CAS files

        Dim questDatLines As String() = GetResourceLines(My.Resources.QuestDAT)

        For Each line In questDatLines
            If Left(line, 1) <> "#" Then
                'Lines isn't a comment - so parse it.
                Dim scp = InStr(line, ";")
                Dim keyword = Trim(Left(line, scp - 1))
                Dim num = CInt(Right(line, Len(line) - scp))
                _casKeywords(num) = keyword
            End If
        Next
    End Sub

    Private Function GetResourceLines(res As Byte()) As String()
        Dim enc As New System.Text.UTF8Encoding()
        Dim resFile As String = enc.GetString(res)
        Return Split(resFile, Chr(13) + Chr(10))
    End Function

    Private Function ParseFile(ByRef filename As String) As Boolean
        'Returns FALSE if failed.

        Dim hasErrors As Boolean
        Dim result As Boolean
        Dim libCode(0) As String
        Dim libLines As Integer
        Dim ignoreMode, skipCheck As Boolean
        Dim c, d, l As Integer
        Dim libFileHandle As Integer
        Dim libResourceLines As String()
        Dim libFile As String
        Dim libLine As String
        Dim inDefGameBlock, gameLine As Integer
        Dim inDefSynBlock, synLine As Integer
        Dim libFoundThisSweep As Boolean
        Dim libFileName As String
        Dim libraryList(0) As String
        Dim numLibraries As Integer
        Dim libraryAlreadyIncluded As Boolean
        Dim inDefTypeBlock As Integer
        Dim typeBlockName As String
        Dim typeLine As Integer
        Dim defineCount, curLine As Integer

        _defineBlockParams = New Dictionary(Of String, Dictionary(Of String, String))

        result = True

        ' Parses file and returns the positions of each main
        ' 'define' block. Supports nested defines.

        If LCase(Right(filename, 4)) = ".zip" Then
            _originalFilename = filename
            filename = GetUnzippedFile(filename)
            _gamePath = System.IO.Path.GetDirectoryName(filename)
        End If

        If LCase(Right(filename, 4)) = ".asl" Or LCase(Right(filename, 4)) = ".txt" Then
            'Read file into Lines array
            Dim fileData As String

            If Config.ReadGameFileFromAzureBlob Then
                Using client As New WebClient
                    fileData = client.DownloadString(filename)

                    Dim fileBytes As Byte() = client.DownloadData(filename)
                    m_gameId = TextAdventures.Utility.Utility.CalculateMD5Hash(fileBytes)
                End Using
            Else
                fileData = System.IO.File.ReadAllText(filename)
            End If

            Dim aslLines As String() = fileData.Split(Chr(13))
            ReDim _lines(aslLines.Length)
            _lines(0) = ""

            For l = 1 To aslLines.Length
                _lines(l) = aslLines(l - 1)
                _lines(l) = RemoveTabs(_lines(l))
                _lines(l) = _lines(l).Trim(" "c, Chr(10), Chr(13))
            Next

            l = aslLines.Length

        ElseIf LCase(Right(filename, 4)) = ".cas" Then
            LogASLError("Loading CAS")
            LoadCASFile(filename)
            l = UBound(_lines)

        Else
            Throw New InvalidOperationException("Unrecognized file extension")
        End If

        ' Add libraries to end of code:

        numLibraries = 0

        Do
            libFoundThisSweep = False
            For i = l To 1 Step -1
                ' We search for includes backwards as a game might include
                ' some-general.lib and then something-specific.lib which needs
                ' something-general; if we include something-specific first,
                ' then something-general afterwards, something-general's startscript
                ' gets executed before something-specific's, as we execute the
                ' lib startscripts backwards as well
                If BeginsWith(_lines(i), "!include ") Then
                    libFileName = GetParameter(_lines(i), _nullContext)
                    'Clear !include statement
                    _lines(i) = ""
                    libraryAlreadyIncluded = False
                    LogASLError("Including library '" & libFileName & "'...", LogType.Init)

                    For j = 1 To numLibraries
                        If LCase(libFileName) = LCase(libraryList(j)) Then
                            libraryAlreadyIncluded = True
                            Exit For
                        End If
                    Next j

                    If libraryAlreadyIncluded Then
                        LogASLError("     - Library already included.", LogType.Init)
                    Else
                        numLibraries = numLibraries + 1
                        ReDim Preserve libraryList(numLibraries)
                        libraryList(numLibraries) = libFileName

                        libFoundThisSweep = True
                        libResourceLines = Nothing

                        libFile = _gamePath & libFileName
                        LogASLError(" - Searching for " & libFile & " (game path)", LogType.Init)
                        libFileHandle = FreeFile()

                        If System.IO.File.Exists(libFile) Then
                            FileOpen(libFileHandle, libFile, OpenMode.Input)
                        Else
                            ' File was not found; try standard Quest libraries (stored here as resources)

                            LogASLError("     - Library not found in game path.", LogType.Init)
                            LogASLError(" - Searching for " & libFile & " (standard libraries)", LogType.Init)
                            libResourceLines = GetLibraryLines(libFileName)

                            If libResourceLines Is Nothing Then
                                LogASLError("Library not found.", LogType.FatalError)
                                _openErrorReport = _openErrorReport & "Library '" & libraryList(numLibraries) & "' not found." & vbCrLf
                                Return False
                            End If
                        End If

                        LogASLError("     - Found library, opening...", LogType.Init)

                        libLines = 0

                        If libResourceLines Is Nothing Then
                            Do Until EOF(libFileHandle)
                                libLines = libLines + 1
                                libLine = LineInput(libFileHandle)
                                libLine = RemoveTabs(libLine)
                                ReDim Preserve libCode(libLines)
                                libCode(libLines) = Trim(libLine)
                            Loop
                            FileClose(libFileHandle)
                        Else
                            For Each ResLibLine As String In libResourceLines
                                libLines = libLines + 1
                                ReDim Preserve libCode(libLines)
                                libLine = ResLibLine
                                libLine = RemoveTabs(libLine)
                                libCode(libLines) = Trim(libLine)
                            Next
                        End If

                        Dim libVer = -1

                        If libCode(1) = "!library" Then
                            For c = 1 To libLines
                                If BeginsWith(libCode(c), "!asl-version ") Then
                                    libVer = CInt(GetParameter(libCode(c), _nullContext))
                                    Exit For
                                End If
                            Next c
                        Else
                            'Old library
                            libVer = 100
                        End If

                        If libVer = -1 Then
                            LogASLError(" - Library has no asl-version information.", LogType.LibraryWarningError)
                            libVer = 200
                        End If

                        ignoreMode = False
                        For c = 1 To libLines
                            If BeginsWith(libCode(c), "!include ") Then
                                ' Quest only honours !include in a library for asl-version
                                ' 311 or later, as it ignored them in versions < 3.11
                                If libVer >= 311 Then
                                    AddLine(libCode(c))
                                    l = l + 1
                                End If
                            ElseIf Left(libCode(c), 1) <> "!" And Left(libCode(c), 1) <> "'" And Not ignoreMode Then
                                AddLine(libCode(c))
                                l = l + 1
                            Else
                                If libCode(c) = "!addto game" Then
                                    inDefGameBlock = 0
                                    For d = 1 To UBound(_lines)
                                        If BeginsWith(_lines(d), "define game ") Then
                                            inDefGameBlock = 1
                                        ElseIf BeginsWith(_lines(d), "define ") Then
                                            If inDefGameBlock <> 0 Then
                                                inDefGameBlock = inDefGameBlock + 1
                                            End If
                                        ElseIf _lines(d) = "end define" And inDefGameBlock = 1 Then
                                            gameLine = d
                                            d = UBound(_lines)
                                        ElseIf _lines(d) = "end define" Then
                                            If inDefGameBlock <> 0 Then
                                                inDefGameBlock = inDefGameBlock - 1
                                            End If
                                        End If
                                    Next d

                                    Do
                                        c = c + 1
                                        If Not BeginsWith(libCode(c), "!end") Then
                                            ReDim Preserve _lines(UBound(_lines) + 1)
                                            For d = UBound(_lines) To gameLine + 1 Step -1
                                                _lines(d) = _lines(d - 1)
                                            Next d

                                            ' startscript lines in a library are prepended
                                            ' with "lib" internally so they are executed
                                            ' before any startscript specified by the
                                            ' calling ASL file, for asl-versions 311 and
                                            ' later.

                                            ' similarly, commands in a library. NB: without this, lib
                                            ' verbs have lower precedence than game verbs anyway. Also
                                            ' lib commands have lower precedence than game commands. We
                                            ' only need this code so that game verbs have a higher
                                            ' precedence than lib commands.

                                            ' we also need it so that lib verbs have a higher
                                            ' precedence than lib commands.

                                            If libVer >= 311 And BeginsWith(libCode(c), "startscript ") Then
                                                _lines(gameLine) = "lib " & libCode(c)
                                            ElseIf libVer >= 392 And (BeginsWith(libCode(c), "command ") Or BeginsWith(libCode(c), "verb ")) Then
                                                _lines(gameLine) = "lib " & libCode(c)
                                            Else
                                                _lines(gameLine) = libCode(c)
                                            End If

                                            l = l + 1
                                            gameLine = gameLine + 1
                                        End If
                                    Loop Until BeginsWith(libCode(c), "!end")
                                ElseIf libCode(c) = "!addto synonyms" Then
                                    inDefSynBlock = 0
                                    For d = 1 To UBound(_lines)
                                        If _lines(d) = "define synonyms" Then
                                            inDefSynBlock = 1
                                        ElseIf _lines(d) = "end define" And inDefSynBlock = 1 Then
                                            synLine = d
                                            d = UBound(_lines)
                                        End If
                                    Next d

                                    If inDefSynBlock = 0 Then
                                        'No "define synonyms" block in game - so add it
                                        AddLine("define synonyms")
                                        AddLine("end define")
                                        synLine = UBound(_lines)
                                    End If

                                    Do
                                        c = c + 1
                                        If Not BeginsWith(libCode(c), "!end") Then
                                            ReDim Preserve _lines(UBound(_lines) + 1)
                                            For d = UBound(_lines) To synLine + 1 Step -1
                                                _lines(d) = _lines(d - 1)
                                            Next d
                                            _lines(synLine) = libCode(c)
                                            l = l + 1
                                            synLine = synLine + 1
                                        End If
                                    Loop Until BeginsWith(libCode(c), "!end")
                                ElseIf BeginsWith(libCode(c), "!addto type ") Then
                                    inDefTypeBlock = 0
                                    typeBlockName = LCase(GetParameter(libCode(c), _nullContext))
                                    For d = 1 To UBound(_lines)
                                        If LCase(_lines(d)) = "define type <" & typeBlockName & ">" Then
                                            inDefTypeBlock = 1
                                        ElseIf _lines(d) = "end define" And inDefTypeBlock = 1 Then
                                            typeLine = d
                                            d = UBound(_lines)
                                        End If
                                    Next d

                                    If inDefTypeBlock = 0 Then
                                        'No "define type (whatever)" block in game - so add it
                                        AddLine("define type <" & typeBlockName & ">")
                                        AddLine("end define")
                                        typeLine = UBound(_lines)
                                    End If

                                    Do
                                        c = c + 1
                                        If c > libLines Then Exit Do
                                        If Not BeginsWith(libCode(c), "!end") Then
                                            ReDim Preserve _lines(UBound(_lines) + 1)
                                            For d = UBound(_lines) To typeLine + 1 Step -1
                                                _lines(d) = _lines(d - 1)
                                            Next d
                                            _lines(typeLine) = libCode(c)
                                            l = l + 1
                                            typeLine = typeLine + 1
                                        End If
                                    Loop Until BeginsWith(libCode(c), "!end")


                                ElseIf libCode(c) = "!library" Then
                                    'ignore
                                ElseIf BeginsWith(libCode(c), "!asl-version ") Then
                                    'ignore
                                ElseIf BeginsWith(libCode(c), "'") Then
                                    'ignore
                                ElseIf BeginsWith(libCode(c), "!QDK") Then
                                    ignoreMode = True
                                ElseIf BeginsWith(libCode(c), "!end") Then
                                    ignoreMode = False
                                End If
                            End If
                        Next c
                    End If
                End If
            Next i
        Loop Until libFoundThisSweep = False

        skipCheck = False

        Dim lastSlashPos, slashPos As Integer
        Dim curPos = 1
        Do
            slashPos = InStr(curPos, filename, "\")
            If slashPos <> 0 Then lastSlashPos = slashPos
            curPos = slashPos + 1
        Loop Until slashPos = 0
        Dim filenameNoPath = LCase(Mid(filename, lastSlashPos + 1))

        For i = 1 To _numSkipCheckFiles
            If filenameNoPath = _skipCheckFile(i) Then
                skipCheck = True
                Exit For
            End If
        Next i

        If filenameNoPath = "musicvf1.cas" Then
            _useStaticFrameForPictures = True
        End If

        'RemoveComments called within ConvertMultiLines
        ConvertMultiLines()

        If Not skipCheck Then
            If Not CheckSections() Then
                Return False
            End If
        End If

        _numberSections = 1

        For i = 1 To l
            ' find section beginning with 'define'
            If BeginsWith(_lines(i), "define") = True Then
                ' Now, go through until we reach an 'end define'. However, if we
                ' encounter another 'define' there is a nested define. So, if we
                ' encounter 'define' we increment the definecount. When we find an
                ' 'end define' we decrement it. When definecount is zero, we have
                ' found the end of the section.

                defineCount = 1

                ' Don't count the current line - we know it begins with 'define'...
                curLine = i + 1
                Do
                    If BeginsWith(_lines(curLine), "define") = True Then
                        defineCount = defineCount + 1
                    ElseIf BeginsWith(_lines(curLine), "end define") = True Then
                        defineCount = defineCount - 1
                    End If
                    curLine = curLine + 1
                Loop Until defineCount = 0
                curLine = curLine - 1

                ' Now, we know that the define section begins at i and ends at
                ' curline. Remember where the section begins and ends:

                ReDim Preserve _defineBlocks(_numberSections)
                _defineBlocks(_numberSections) = New DefineBlock
                _defineBlocks(_numberSections).StartLine = i
                _defineBlocks(_numberSections).EndLine = curLine

                _numberSections = _numberSections + 1
                i = curLine
            End If
        Next i

        _numberSections = _numberSections - 1

        Dim gotGameBlock = False
        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define game ") Then
                gotGameBlock = True
                Exit For
            End If
        Next i

        If Not gotGameBlock Then
            _openErrorReport = _openErrorReport & "No 'define game' block." & vbCrLf
            Return False
        End If

        ConvertMultiLineSections()

        hasErrors = ConvertFriendlyIfs()
        If Not hasErrors Then hasErrors = ErrorCheck()

        If hasErrors Then
            Throw New InvalidOperationException("Errors found in game file.")
        End If

        _saveGameFile = ""

        Return result
    End Function

    Friend Sub LogASLError(err As String, Optional type As LogType = LogType.Misc)
        If type = LogType.FatalError Then
            err = "FATAL ERROR: " & err
        ElseIf type = LogType.WarningError Then
            err = "ERROR: " & err
        ElseIf type = LogType.LibraryWarningError Then
            err = "WARNING ERROR (LIBRARY): " & err
        ElseIf type = LogType.Init Then
            err = "INIT: " & err
        ElseIf type = LogType.Warning Then
            err = "WARNING: " & err
        ElseIf type = LogType.UserError Then
            err = "ERROR (REQUESTED): " & err
        ElseIf type = LogType.InternalError Then
            err = "INTERNAL ERROR: " & err
        End If

        _log.Add(err)
    End Sub

    Friend Function GetParameter(s As String, ctx As Context, Optional convertStringVariables As Boolean = True) As String
        ' Returns the parameters between < and > in a string
        Dim newParam As String
        Dim startPos As Integer
        Dim endPos As Integer

        startPos = InStr(s, "<")
        endPos = InStr(s, ">")

        If startPos = 0 Or endPos = 0 Then
            LogASLError("Expected parameter in '" & ReportErrorLine(s) & "'", LogType.WarningError)
            Return ""
        End If

        Dim retrParam = Mid(s, startPos + 1, (endPos - startPos) - 1)

        If convertStringVariables Then
            If _gameAslVersion >= 320 Then
                newParam = ConvertParameter(ConvertParameter(ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx)
            Else
                If Not Left(retrParam, 9) = "~Internal" Then
                    newParam = ConvertParameter(ConvertParameter(ConvertParameter(ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions, ctx)
                Else
                    newParam = retrParam
                End If
            End If
        Else
            newParam = retrParam
        End If

        Return EvaluateInlineExpressions(newParam)
    End Function

    Private Sub AddLine(line As String)
        'Adds a line to the game script
        Dim numLines As Integer

        numLines = UBound(_lines) + 1
        ReDim Preserve _lines(numLines)
        _lines(numLines) = line
    End Sub

    Private Sub LoadCASFile(filename As String)
        Dim endLineReached, exitTheLoop As Boolean
        Dim textMode As Boolean
        Dim casVersion As Integer
        Dim startCat As String = ""
        Dim endCatPos As Integer
        Dim fileData As String = ""
        Dim chkVer As String
        Dim j As Integer
        Dim curLin, textData As String
        Dim cpos, nextLinePos As Integer
        Dim c, tl, ckw, d As String

        ReDim _lines(0)

        If Config.ReadGameFileFromAzureBlob Then
            Using client As New WebClient
                Dim url As String = filename
                Dim baseAddress As Uri = New Uri(url)
                Dim directory As Uri = New Uri(baseAddress, ".")

                Try
                    fileData = client.DownloadString(url)
                Catch ex As WebException
                    url = directory.OriginalString + "_game.cas"
                    fileData = client.DownloadString(url)
                End Try

                Dim parts As String() = directory.OriginalString.Split("/"c)
                m_gameId = parts(parts.Length - 2)
            End Using
        Else
            fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252))
        End If

        chkVer = Left(fileData, 7)
        If chkVer = "QCGF001" Then
            casVersion = 1
        ElseIf chkVer = "QCGF002" Then
            casVersion = 2
        ElseIf chkVer = "QCGF003" Then
            casVersion = 3
        Else
            Throw New InvalidOperationException("Invalid or corrupted CAS file.")
        End If

        If casVersion = 3 Then
            startCat = Keyword2CAS("!startcat")
        End If

        For i = 9 To Len(fileData)
            If casVersion = 3 And Mid(fileData, i, 1) = startCat Then
                ' Read catalog
                _startCatPos = i
                endCatPos = InStr(j, fileData, Keyword2CAS("!endcat"))
                ReadCatalog(Mid(fileData, j + 1, endCatPos - j - 1))
                _resourceFile = filename
                _resourceOffset = endCatPos + 1
                i = Len(fileData)
                _casFileData = fileData
            Else

                curLin = ""
                endLineReached = False
                If textMode = True Then
                    textData = Mid(fileData, i, InStr(i, fileData, Chr(253)) - (i - 1))
                    textData = Left(textData, Len(textData) - 1)
                    cpos = 1
                    Dim finished = False

                    If textData <> "" Then

                        Do
                            nextLinePos = InStr(cpos, textData, Chr(0))
                            If nextLinePos = 0 Then
                                nextLinePos = Len(textData) + 1
                                finished = True
                            End If
                            tl = DecryptString(Mid(textData, cpos, nextLinePos - cpos))
                            AddLine(tl)
                            cpos = nextLinePos + 1
                        Loop Until finished

                    End If

                    textMode = False
                    i = InStr(i, fileData, Chr(253))
                End If

                j = i
                Do
                    ckw = Mid(fileData, j, 1)
                    c = ConvertCasKeyword(ckw)

                    If c = vbCrLf Then
                        endLineReached = True
                    Else
                        If Left(c, 1) <> "!" Then
                            curLin = curLin & c & " "
                        Else
                            If c = "!quote" Then
                                exitTheLoop = False
                                curLin = curLin & "<"
                                Do
                                    j = j + 1
                                    d = Mid(fileData, j, 1)
                                    If d <> Chr(0) Then
                                        curLin = curLin & DecryptString(d)
                                    Else
                                        curLin = curLin & "> "
                                        exitTheLoop = True
                                    End If
                                Loop Until exitTheLoop
                            ElseIf c = "!unknown" Then
                                exitTheLoop = False
                                Do
                                    j = j + 1
                                    d = Mid(fileData, j, 1)
                                    If d <> Chr(254) Then
                                        curLin = curLin & d
                                    Else
                                        exitTheLoop = True
                                    End If
                                Loop Until exitTheLoop
                                curLin = curLin & " "
                            End If
                        End If
                    End If

                    j = j + 1
                Loop Until endLineReached
                AddLine(Trim(curLin))
                If BeginsWith(curLin, "define text") Or (casVersion >= 2 And (BeginsWith(curLin, "define synonyms") Or BeginsWith(curLin, "define type") Or BeginsWith(curLin, "define menu"))) Then
                    textMode = True
                End If
                'j is already at correct place, but i will be
                'incremented - so put j back one or we will miss a
                'character.
                i = j - 1
            End If
        Next i
    End Sub

    Private Function DecryptString(s As String) As String
        Dim output = ""
        For i = 1 To Len(s)
            Dim v As Byte = System.Text.Encoding.GetEncoding(1252).GetBytes(Mid(s, i, 1))(0)
            output = output & Chr(v Xor 255)
        Next i

        Return output
    End Function

    Private Function RemoveTabs(s As String) As String
        If InStr(s, Chr(9)) > 0 Then
            'Remove tab characters and change them into
            'spaces; otherwise they bugger up the Trim
            'commands.
            Dim cpos = 1
            Dim finished = False
            Do
                Dim tabChar = InStr(cpos, s, Chr(9))
                If tabChar <> 0 Then
                    s = Left(s, tabChar - 1) & Space(4) & Mid(s, tabChar + 1)
                    cpos = tabChar + 1
                Else
                    finished = True
                End If
            Loop Until finished
        End If

        Return s
    End Function

    Private Sub DoAddRemove(childId As Integer, parentId As Integer, add As Boolean, ctx As Context)
        If add Then
            AddToObjectProperties("parent=" & _objs(parentId).ObjectName, childId, ctx)
            _objs(childId).ContainerRoom = _objs(parentId).ContainerRoom
        Else
            AddToObjectProperties("not parent", childId, ctx)
        End If

        If _gameAslVersion >= 410 Then
            ' Putting something in a container implicitly makes that
            ' container "seen". Otherwise we could try to "look at" the
            ' object we just put in the container and have disambigution fail!
            AddToObjectProperties("seen", parentId, ctx)
        End If

        UpdateVisibilityInContainers(ctx, _objs(parentId).ObjectName)
    End Sub

    Private Sub DoLook(id As Integer, ctx As Context, Optional showExamineError As Boolean = False, Optional showDefaultDescription As Boolean = True)
        Dim objectContents As String
        Dim foundLook = False

        ' First, set the "seen" property, and for ASL >= 391, update visibility for any
        ' object that is contained by this object.

        If _gameAslVersion >= 391 Then
            AddToObjectProperties("seen", id, ctx)
            UpdateVisibilityInContainers(ctx, _objs(id).ObjectName)
        End If

        ' First look for action, then look
        ' for property, then check define
        ' section:

        Dim lookLine As String
        Dim o = _objs(id)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = "look" Then
                foundLook = True
                ExecuteScript(o.Actions(i).Script, ctx)
                Exit For
            End If
        Next i

        If Not foundLook Then
            For i = 1 To o.NumberProperties
                If o.Properties(i).PropertyName = "look" Then
                    ' do this odd RetrieveParameter stuff to convert any variables
                    Print(GetParameter("<" & o.Properties(i).PropertyValue & ">", ctx), ctx)
                    foundLook = True
                    Exit For
                End If
            Next i
        End If

        If Not foundLook Then
            For i = o.DefinitionSectionStart To o.DefinitionSectionEnd
                If BeginsWith(_lines(i), "look ") Then

                    lookLine = Trim(GetEverythingAfter(_lines(i), "look "))

                    If Left(lookLine, 1) = "<" Then
                        Print(GetParameter(_lines(i), ctx), ctx)
                    Else
                        ExecuteScript(lookLine, ctx, id)
                    End If

                    foundLook = True
                End If
            Next i
        End If

        If _gameAslVersion >= 391 Then
            objectContents = ListContents(id, ctx)
        Else
            objectContents = ""
        End If

        If Not foundLook And showDefaultDescription Then
            Dim err As PlayerError

            If showExamineError Then
                err = PlayerError.DefaultExamine
            Else
                err = PlayerError.DefaultLook
            End If

            ' Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            ' any contents.

            If objectContents = "" Then PlayerErrorMessage(err, ctx)
        End If

        If objectContents <> "" And objectContents <> "<script>" Then Print(objectContents, ctx)

    End Sub

    Private Sub DoOpenClose(id As Integer, open As Boolean, showLook As Boolean, ctx As Context)
        If open Then
            AddToObjectProperties("opened", id, ctx)
            If showLook Then DoLook(id, ctx, , False)
        Else
            AddToObjectProperties("not opened", id, ctx)
        End If

        UpdateVisibilityInContainers(ctx, _objs(id).ObjectName)
    End Sub

    Private Function EvaluateInlineExpressions(s As String) As String
        ' Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>

        If _gameAslVersion < 391 Then
            Return s
        End If

        Dim bracePos As Integer
        Dim curPos = 1
        Dim resultLine = ""

        Do
            bracePos = InStr(curPos, s, "{")

            If bracePos <> 0 Then

                resultLine = resultLine & Mid(s, curPos, bracePos - curPos)

                If Mid(s, bracePos, 2) = "{{" Then
                    ' {{ = {
                    curPos = bracePos + 2
                    resultLine = resultLine & "{"
                Else
                    Dim EndBracePos = InStr(bracePos + 1, s, "}")
                    If EndBracePos = 0 Then
                        LogASLError("Expected } in '" & s & "'", LogType.WarningError)
                        Return "<ERROR>"
                    Else
                        Dim expression = Mid(s, bracePos + 1, EndBracePos - bracePos - 1)
                        Dim expResult = ExpressionHandler(expression)
                        If expResult.Success <> ExpressionSuccess.OK Then
                            LogASLError("Error evaluating expression in <" & s & "> - " & expResult.Message)
                            Return "<ERROR>"
                        End If

                        resultLine = resultLine & expResult.Result
                        curPos = EndBracePos + 1
                    End If
                End If
            Else
                resultLine = resultLine & Mid(s, curPos)
            End If
        Loop Until bracePos = 0 Or curPos > Len(s)

        ' Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        curPos = 1
        Do
            bracePos = InStr(curPos, resultLine, "}}")
            If bracePos <> 0 Then
                resultLine = Left(resultLine, bracePos) & Mid(resultLine, bracePos + 2)
                curPos = bracePos + 1
            End If
        Loop Until bracePos = 0 Or curPos > Len(resultLine)

        Return resultLine
    End Function

    Private Sub ExecAddRemove(cmd As String, ctx As Context)
        Dim childId As Integer
        Dim childName As String
        Dim doAdd As Boolean
        Dim sepPos, parentId, sepLen As Integer
        Dim parentName As String
        Dim verb As String = ""
        Dim action As String
        Dim foundAction As Boolean
        Dim actionScript As String = ""
        Dim propertyExists As Boolean
        Dim textToPrint As String
        Dim isContainer As Boolean
        Dim errorMsg As String = ""     ' TODO: This was passed ByRef - needs fixing
        Dim gotObject As Boolean
        Dim childLength As Integer
        Dim noParentSpecified = False

        If BeginsWith(cmd, "put ") Then
            verb = "put"
            doAdd = True
            sepPos = InStr(cmd, " on ")
            sepLen = 4
            If sepPos = 0 Then
                sepPos = InStr(cmd, " in ")
                sepLen = 4
            End If
            If sepPos = 0 Then
                sepPos = InStr(cmd, " onto ")
                sepLen = 6
            End If
        ElseIf BeginsWith(cmd, "add ") Then
            verb = "add"
            doAdd = True
            sepPos = InStr(cmd, " to ")
            sepLen = 4
        ElseIf BeginsWith(cmd, "remove ") Then
            verb = "remove"
            doAdd = False
            sepPos = InStr(cmd, " from ")
            sepLen = 6
        End If

        If sepPos = 0 Then
            noParentSpecified = True
            sepPos = Len(cmd) + 1
        End If

        childLength = sepPos - (Len(verb) + 2)

        If childLength < 0 Then
            PlayerErrorMessage(PlayerError.BadCommand, ctx)
            _badCmdBefore = verb
            Exit Sub
        End If

        childName = Trim(Mid(cmd, Len(verb) + 2, childLength))

        gotObject = False

        If _gameAslVersion >= 392 And doAdd Then
            childId = Disambiguate(childName, _currentRoom & ";inventory", ctx)

            If childId > 0 Then
                If _objs(childId).ContainerRoom = "inventory" Then
                    gotObject = True
                Else
                    ' Player is not carrying the object they referred to. So, first take the object.
                    Print("(first taking " & _objs(childId).Article & ")", ctx)
                    ' Try to take the object
                    ctx.AllowRealNamesInCommand = True
                    ExecCommand("take " & _objs(childId).ObjectName, ctx, False, , True)

                    If _objs(childId).ContainerRoom = "inventory" Then gotObject = True
                End If

                If Not gotObject Then
                    _badCmdBefore = verb
                    Exit Sub
                End If
            Else
                If childId <> -2 Then PlayerErrorMessage(PlayerError.NoItem, ctx)
                _badCmdBefore = verb
                Exit Sub
            End If

        Else
            childId = Disambiguate(childName, "inventory;" & _currentRoom, ctx)

            If childId <= 0 Then
                If childId <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                _badCmdBefore = verb
                Exit Sub
            End If
        End If

        If noParentSpecified And doAdd Then
            SetStringContents("quest.error.article", _objs(childId).Article, ctx)
            PlayerErrorMessage(PlayerError.BadPut, ctx)
            Exit Sub
        End If

        If doAdd Then
            action = "add"
        Else
            action = "remove"
        End If

        If Not noParentSpecified Then
            parentName = Trim(Mid(cmd, sepPos + sepLen))

            parentId = Disambiguate(parentName, _currentRoom & ";inventory", ctx)

            If parentId <= 0 Then
                If parentId <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                _badCmdBefore = Left(cmd, sepPos + sepLen)
                Exit Sub
            End If
        Else
            ' Assume the player was referring to the parent that the object is already in,
            ' if it is even in an object already

            If Not IsYes(GetObjectProperty("parent", childId, True, False)) Then
                PlayerErrorMessage(PlayerError.CantRemove, ctx)
                Exit Sub
            End If

            parentId = GetObjectIdNoAlias(GetObjectProperty("parent", childId, False, True))
        End If

        ' Check if parent is a container

        isContainer = IsYes(GetObjectProperty("container", parentId, True, False))

        If Not isContainer Then
            If doAdd Then
                PlayerErrorMessage(PlayerError.CantPut, ctx)
            Else
                PlayerErrorMessage(PlayerError.CantRemove, ctx)
            End If
            Exit Sub
        End If

        ' Check object is already held by that parent

        If IsYes(GetObjectProperty("parent", childId, True, False)) Then
            If doAdd And LCase(GetObjectProperty("parent", childId, False, False)) = LCase(_objs(parentId).ObjectName) Then
                PlayerErrorMessage(PlayerError.AlreadyPut, ctx)
            End If
        End If

        ' NEW: Check parent and child are accessible to player
        If Not PlayerCanAccessObject(childId, , errorMsg) Then
            If doAdd Then
                PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, errorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, errorMsg)
            End If
            Exit Sub
        End If
        If Not PlayerCanAccessObject(parentId, , errorMsg) Then
            If doAdd Then
                PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, errorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, errorMsg)
            End If
            Exit Sub
        End If

        ' Check if parent is a closed container

        If Not IsYes(GetObjectProperty("surface", parentId, True, False)) And Not IsYes(GetObjectProperty("opened", parentId, True, False)) Then
            ' Not a surface and not open, so can't add to this closed container.
            If doAdd Then
                PlayerErrorMessage(PlayerError.CantPut, ctx)
            Else
                PlayerErrorMessage(PlayerError.CantRemove, ctx)
            End If
            Exit Sub
        End If

        ' Now check if it can be added to (or removed from)

        ' First check for an action
        Dim o = _objs(parentId)
        For i = 1 To o.NumberActions
            If LCase(o.Actions(i).ActionName) = action Then
                foundAction = True
                actionScript = o.Actions(i).Script
                Exit For
            End If
        Next i

        If foundAction Then
            SetStringContents("quest." & LCase(action) & ".object.name", _objs(childId).ObjectName, ctx)
            ExecuteScript(actionScript, ctx, parentId)
        Else
            ' Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, parentId, True, False))

            If Not propertyExists Then
                ' Show error message
                If doAdd Then
                    PlayerErrorMessage(PlayerError.CantPut, ctx)
                Else
                    PlayerErrorMessage(PlayerError.CantRemove, ctx)
                End If
            Else
                textToPrint = GetObjectProperty(action, parentId, False, False)
                If textToPrint = "" Then
                    ' Show default message
                    If doAdd Then
                        PlayerErrorMessage(PlayerError.DefaultPut, ctx)
                    Else
                        PlayerErrorMessage(PlayerError.DefaultRemove, ctx)
                    End If
                Else
                    Print(textToPrint, ctx)
                End If

                DoAddRemove(childId, parentId, doAdd, ctx)

            End If
        End If

    End Sub

    Private Sub ExecAddRemoveScript(parameter As String, add As Boolean, ctx As Context)
        Dim childId, parentId As Integer
        Dim commandName As String
        Dim childName As String
        Dim parentName As String = ""
        Dim scp As Integer

        If add Then
            commandName = "add"
        Else
            commandName = "remove"
        End If

        scp = InStr(parameter, ";")
        If scp = 0 And add Then
            LogASLError("No parent specified in '" & commandName & " <" & parameter & ">", LogType.WarningError)
            Exit Sub
        End If

        If scp <> 0 Then
            childName = LCase(Trim(Left(parameter, scp - 1)))
            parentName = LCase(Trim(Mid(parameter, scp + 1)))
        Else
            childName = LCase(Trim(parameter))
        End If

        childId = GetObjectIdNoAlias(childName)
        If childId = 0 Then
            LogASLError("Invalid child object name specified in '" & commandName & " <" & parameter & ">", LogType.WarningError)
            Exit Sub
        End If

        If scp <> 0 Then
            parentId = GetObjectIdNoAlias(parentName)
            If parentId = 0 Then
                LogASLError("Invalid parent object name specified in '" & commandName & " <" & parameter & ">", LogType.WarningError)
                Exit Sub
            End If

            DoAddRemove(childId, parentId, add, ctx)
        Else
            AddToObjectProperties("not parent", childId, ctx)
            UpdateVisibilityInContainers(ctx, _objs(parentId).ObjectName)
        End If
    End Sub

    Private Sub ExecOpenClose(cmd As String, ctx As Context)
        Dim id As Integer
        Dim name As String
        Dim doOpen As Boolean
        Dim isOpen, foundAction As Boolean
        Dim action As String = ""
        Dim actionScript As String = ""
        Dim propertyExists As Boolean
        Dim textToPrint As String
        Dim isContainer As Boolean
        Dim errorMsg As String = ""

        If BeginsWith(cmd, "open ") Then
            action = "open"
            doOpen = True
        ElseIf BeginsWith(cmd, "close ") Then
            action = "close"
            doOpen = False
        End If

        name = GetEverythingAfter(cmd, action & " ")

        id = Disambiguate(name, _currentRoom & ";inventory", ctx)

        If id <= 0 Then
            If id <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
            _badCmdBefore = action
            Exit Sub
        End If

        ' Check if it's even a container

        isContainer = IsYes(GetObjectProperty("container", id, True, False))

        If Not isContainer Then
            If doOpen Then
                PlayerErrorMessage(PlayerError.CantOpen, ctx)
            Else
                PlayerErrorMessage(PlayerError.CantClose, ctx)
            End If
            Exit Sub
        End If

        ' Check if it's already open (or closed)

        isOpen = IsYes(GetObjectProperty("opened", id, True, False))

        If doOpen And isOpen Then
            ' Object is already open
            PlayerErrorMessage(PlayerError.AlreadyOpen, ctx)
            Exit Sub
        ElseIf Not doOpen And Not isOpen Then
            ' Object is already closed
            PlayerErrorMessage(PlayerError.AlreadyClosed, ctx)
            Exit Sub
        End If

        ' NEW: Check if it's accessible, i.e. check it's not itself inside another closed container

        If Not PlayerCanAccessObject(id, , errorMsg) Then
            If doOpen Then
                PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, errorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, errorMsg)
            End If
            Exit Sub
        End If

        ' Now check if it can be opened (or closed)

        ' First check for an action
        Dim o = _objs(id)
        For i = 1 To o.NumberActions
            If LCase(o.Actions(i).ActionName) = action Then
                foundAction = True
                actionScript = o.Actions(i).Script
                Exit For
            End If
        Next i

        If foundAction Then
            ExecuteScript(actionScript, ctx, id)
        Else
            ' Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, id, True, False))

            If Not propertyExists Then
                ' Show error message
                If doOpen Then
                    PlayerErrorMessage(PlayerError.CantOpen, ctx)
                Else
                    PlayerErrorMessage(PlayerError.CantClose, ctx)
                End If
            Else
                textToPrint = GetObjectProperty(action, id, False, False)
                If textToPrint = "" Then
                    ' Show default message
                    If doOpen Then
                        PlayerErrorMessage(PlayerError.DefaultOpen, ctx)
                    Else
                        PlayerErrorMessage(PlayerError.DefaultClose, ctx)
                    End If
                Else
                    Print(textToPrint, ctx)
                End If

                DoOpenClose(id, doOpen, True, ctx)

            End If
        End If

    End Sub

    Private Sub ExecuteSelectCase(script As String, ctx As Context)
        ' ScriptLine passed will look like this:
        '   select case <whatever> do <!intprocX>
        ' with all the case statements in the intproc.

        Dim afterLine = GetAfterParameter(script)

        If Not BeginsWith(afterLine, "do <!intproc") Then
            LogASLError("No case block specified for '" & script & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim blockName = GetParameter(afterLine, ctx)
        Dim block = DefineBlockParam("procedure", blockName)
        Dim checkValue = GetParameter(script, ctx)
        Dim caseMatch = False

        For i = block.StartLine + 1 To block.EndLine - 1
            ' Go through all the cases until we find the one that matches

            If _lines(i) <> "" Then
                If Not BeginsWith(_lines(i), "case ") Then
                    LogASLError("Invalid line in 'select case' block: '" & _lines(i) & "'", LogType.WarningError)
                Else
                    Dim caseScript = ""

                    If BeginsWith(_lines(i), "case else ") Then
                        caseMatch = True
                        caseScript = GetEverythingAfter(_lines(i), "case else ")
                    Else
                        Dim thisCase = GetParameter(_lines(i), ctx)
                        Dim finished = False

                        Do
                            Dim SCP = InStr(thisCase, ";")
                            If SCP = 0 Then
                                SCP = Len(thisCase) + 1
                                finished = True
                            End If

                            Dim condition = Trim(Left(thisCase, SCP - 1))
                            If condition = checkValue Then
                                caseScript = GetAfterParameter(_lines(i))
                                caseMatch = True
                                finished = True
                            Else
                                thisCase = Mid(thisCase, SCP + 1)
                            End If
                        Loop Until finished
                    End If

                    If caseMatch Then
                        ExecuteScript(caseScript, ctx)
                        Exit Sub
                    End If
                End If
            End If
        Next i

    End Sub

    Private Function ExecVerb(cmd As String, ctx As Context, Optional libCommands As Boolean = False) As Boolean
        Dim gameBlock As DefineBlock
        Dim foundVerb = False
        Dim verbProperty As String = ""
        Dim script As String = ""
        Dim verbsList As String
        Dim thisVerb As String = ""
        Dim scp As Integer
        Dim id As Integer
        Dim verbObject As String = ""
        Dim verbTag As String
        Dim thisScript As String = ""

        If Not libCommands Then
            verbTag = "verb "
        Else
            verbTag = "lib verb "
        End If

        gameBlock = GetDefineBlock("game")
        For i = gameBlock.StartLine + 1 To gameBlock.EndLine - 1
            If BeginsWith(_lines(i), verbTag) Then
                verbsList = GetParameter(_lines(i), ctx)

                ' The property or action the verb uses is either after a colon,
                ' or it's the first (or only) verb on the line.

                Dim colonPos = InStr(verbsList, ":")
                If colonPos <> 0 Then
                    verbProperty = LCase(Trim(Mid(verbsList, colonPos + 1)))
                    verbsList = Trim(Left(verbsList, colonPos - 1))
                Else
                    scp = InStr(verbsList, ";")
                    If scp = 0 Then
                        verbProperty = LCase(verbsList)
                    Else
                        verbProperty = LCase(Trim(Left(verbsList, scp - 1)))
                    End If
                End If

                ' Now let's see if this matches:

                Do
                    scp = InStr(verbsList, ";")
                    If scp = 0 Then
                        thisVerb = LCase(verbsList)
                    Else
                        thisVerb = LCase(Trim(Left(verbsList, scp - 1)))
                    End If

                    If BeginsWith(cmd, thisVerb & " ") Then
                        foundVerb = True
                        verbObject = GetEverythingAfter(cmd, thisVerb & " ")
                        script = Trim(Mid(_lines(i), InStr(_lines(i), ">") + 1))
                    End If

                    If scp <> 0 Then
                        verbsList = Trim(Mid(verbsList, scp + 1))
                    End If
                Loop Until scp = 0 Or Trim(verbsList) = "" Or foundVerb

                If foundVerb Then Exit For

            End If
        Next i

        If foundVerb Then

            id = Disambiguate(verbObject, "inventory;" & _currentRoom, ctx)

            If id < 0 Then
                If id <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                _badCmdBefore = thisVerb
            Else
                SetStringContents("quest.error.article", _objs(id).Article, ctx)

                Dim foundAction = False

                ' Now see if this object has the relevant action or property
                Dim o = _objs(id)
                For i = 1 To o.NumberActions
                    If LCase(o.Actions(i).ActionName) = verbProperty Then
                        foundAction = True
                        thisScript = o.Actions(i).Script
                        Exit For
                    End If
                Next i

                If thisScript <> "" Then
                    ' Avoid an RTE "this array is fixed or temporarily locked"
                    ExecuteScript(thisScript, ctx, id)
                End If

                If Not foundAction Then
                    ' Check properties for a message
                    For i = 1 To o.NumberProperties
                        If LCase(o.Properties(i).PropertyName) = verbProperty Then
                            foundAction = True
                            Print(o.Properties(i).PropertyValue, ctx)
                            Exit For
                        End If
                    Next i
                End If

                If Not foundAction Then
                    ' Execute the default script from the verb definition
                    ExecuteScript(script, ctx)
                End If
            End If
        End If

        Return foundVerb
    End Function

    Private Function ExpressionHandler(expr As String) As ExpressionResult
        Dim openBracketPos, endBracketPos As Integer
        Dim res As New ExpressionResult

        ' Find brackets, recursively call ExpressionHandler
        Do
            openBracketPos = InStr(expr, "(")
            If openBracketPos <> 0 Then
                ' Find equivalent closing bracket
                Dim BracketCount = 1
                endBracketPos = 0
                For i = openBracketPos + 1 To Len(expr)
                    If Mid(expr, i, 1) = "(" Then
                        BracketCount = BracketCount + 1
                    ElseIf Mid(expr, i, 1) = ")" Then
                        BracketCount = BracketCount - 1
                    End If

                    If BracketCount = 0 Then
                        endBracketPos = i
                        Exit For
                    End If
                Next i

                If endBracketPos <> 0 Then
                    Dim NestedResult = ExpressionHandler(Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1))
                    If NestedResult.Success <> ExpressionSuccess.OK Then
                        res.Success = NestedResult.Success
                        res.Message = NestedResult.Message
                        Return res
                    End If

                    expr = Left(expr, openBracketPos - 1) & " " & NestedResult.Result & " " & Mid(expr, endBracketPos + 1)

                Else
                    res.Message = "Missing closing bracket"
                    res.Success = ExpressionSuccess.Fail
                    Return res

                End If
            End If
        Loop Until openBracketPos = 0

        ' Split expression into elements, e.g.:
        '       2 + 3 * 578.2 / 36
        '       E O E O EEEEE O EE      where E=Element, O=Operator

        Dim numElements = 1
        Dim elements() As String
        ReDim elements(1)
        Dim numOperators = 0
        Dim operators(0) As String
        Dim newElement As Boolean

        Dim obscuredExpr = ObscureNumericExps(expr)

        For i = 1 To Len(expr)
            Select Case Mid(obscuredExpr, i, 1)
                Case "+", "*", "/"
                    newElement = True
                Case "-"
                    ' A minus often means subtraction, so it's a new element. But sometimes
                    ' it just denotes a negative number. In this case, the current element will
                    ' be empty.

                    If Trim(elements(numElements)) = "" Then
                        newElement = False
                    Else
                        newElement = True
                    End If
                Case Else
                    newElement = False
            End Select

            If newElement Then
                numElements = numElements + 1
                ReDim Preserve elements(numElements)

                numOperators = numOperators + 1
                ReDim Preserve operators(numOperators)
                operators(numOperators) = Mid(expr, i, 1)
            Else
                elements(numElements) = elements(numElements) & Mid(expr, i, 1)
            End If
        Next i

        ' Check Elements are numeric, and trim spaces
        For i = 1 To numElements
            elements(i) = Trim(elements(i))

            If Not IsNumeric(elements(i)) Then
                res.Message = "Syntax error evaluating expression - non-numeric element '" & elements(i) & "'"
                res.Success = ExpressionSuccess.Fail
                Return res
            End If
        Next i

        Dim opNum = 0

        Do
            ' Go through the Operators array to find next calculation to perform

            For i = 1 To numOperators
                If operators(i) = "/" Or operators(i) = "*" Then
                    opNum = i
                    Exit For
                End If
            Next i

            If opNum = 0 Then
                For i = 1 To numOperators
                    If operators(i) = "+" Or operators(i) = "-" Then
                        opNum = i
                        Exit For
                    End If
                Next i
            End If

            ' If OpNum is still 0, there are no calculations left to do.

            If opNum <> 0 Then

                Dim val1 = CDbl(elements(opNum))
                Dim val2 = CDbl(elements(opNum + 1))
                Dim result As Double

                Select Case operators(opNum)
                    Case "/"
                        If val2 = 0 Then
                            res.Message = "Division by zero"
                            res.Success = ExpressionSuccess.Fail
                            Return res
                        End If
                        result = val1 / val2
                    Case "*"
                        result = val1 * val2
                    Case "+"
                        result = val1 + val2
                    Case "-"
                        result = val1 - val2
                End Select

                elements(opNum) = CStr(result)

                ' Remove this operator, and Elements(OpNum+1) from the arrays
                For i = opNum To numOperators - 1
                    operators(i) = operators(i + 1)
                Next i
                For i = opNum + 1 To numElements - 1
                    elements(i) = elements(i + 1)
                Next i
                numOperators = numOperators - 1
                numElements = numElements - 1
                ReDim Preserve operators(numOperators)
                ReDim Preserve elements(numElements)

            End If
        Loop Until opNum = 0 Or numOperators = 0

        res.Success = ExpressionSuccess.OK
        res.Result = elements(1)
        Return res

    End Function

    Private Function ListContents(id As Integer, ctx As Context) As String
        ' Returns a formatted list of the contents of a container.
        ' If the list action causes a script to be run instead, ListContents
        ' returns "<script>"

        Dim contentsIDs(0) As Integer

        If Not IsYes(GetObjectProperty("container", id, True, False)) Then
            Return ""
        End If

        If Not IsYes(GetObjectProperty("opened", id, True, False)) And Not IsYes(GetObjectProperty("transparent", id, True, False)) And Not IsYes(GetObjectProperty("surface", id, True, False)) Then
            ' Container is closed, so return "list closed" property if there is one.

            If DoAction(id, "list closed", ctx, False) Then
                Return "<script>"
            Else
                Return GetObjectProperty("list closed", id, False, False)
            End If
        End If

        ' populate contents string

        Dim numContents = 0

        For i = 1 To _numberObjs
            If _objs(i).Exists And _objs(i).Visible Then
                If LCase(GetObjectProperty("parent", i, False, False)) = LCase(_objs(id).ObjectName) Then
                    numContents = numContents + 1
                    ReDim Preserve contentsIDs(numContents)
                    contentsIDs(numContents) = i
                End If
            End If
        Next i

        Dim contents = ""

        If numContents > 0 Then
            ' Check if list property is set.

            If DoAction(id, "list", ctx, False) Then
                Return "<script>"
            End If

            If IsYes(GetObjectProperty("list", id, True, False)) Then
                ' Read header, if any
                Dim listString = GetObjectProperty("list", id, False, False)
                Dim displayList = True

                If listString <> "" Then
                    If Right(listString, 1) = ":" Then
                        contents = Left(listString, Len(listString) - 1) & " "
                    Else
                        ' If header doesn't end in a colon, then the header is the only text to print
                        contents = listString
                        displayList = False
                    End If
                Else
                    contents = UCase(Left(_objs(id).Article, 1)) & Mid(_objs(id).Article, 2) & " contains "
                End If

                If displayList Then
                    For i = 1 To numContents
                        If i > 1 Then
                            If i < numContents Then
                                contents = contents & ", "
                            Else
                                contents = contents & " and "
                            End If
                        End If

                        Dim o = _objs(contentsIDs(i))
                        If o.Prefix <> "" Then contents = contents & o.Prefix
                        If o.ObjectAlias <> "" Then
                            contents = contents & "|b" & o.ObjectAlias & "|xb"
                        Else
                            contents = contents & "|b" & o.ObjectName & "|xb"
                        End If
                        If o.Suffix <> "" Then contents = contents & " " & o.Suffix
                    Next i
                End If

                Return contents & "."
            End If
            ' The "list" property is not set, so do not list contents.
            Return ""
        End If

        ' Container is empty, so return "list empty" property if there is one.

        If DoAction(id, "list empty", ctx, False) Then
            Return "<script>"
        Else
            Return GetObjectProperty("list empty", id, False, False)
        End If

    End Function

    Private Function ObscureNumericExps(s As String) As String
        ' Obscures + or - next to E in Double-type variables with exponents
        '   e.g. 2.345E+20 becomes 2.345EX20
        ' This stops huge numbers breaking parsing of maths functions

        Dim EPos, CurPos As Integer
        Dim OutputString As String
        OutputString = s

        CurPos = 1
        Do
            EPos = InStr(CurPos, OutputString, "E")
            If EPos <> 0 Then
                OutputString = Left(OutputString, EPos) & "X" & Mid(OutputString, EPos + 2)
                CurPos = EPos + 2
            End If
        Loop Until EPos = 0

        Return OutputString
    End Function

    Private Sub ProcessListInfo(line As String, id As Integer)
        Dim listInfo As New TextAction
        Dim propName As String = ""

        If BeginsWith(line, "list closed <") Then
            listInfo.Type = TextActionType.Text
            listInfo.Data = GetParameter(line, _nullContext)
            propName = "list closed"
        ElseIf Trim(line) = "list closed off" Then
            ' default for list closed is off anyway
            Exit Sub
        ElseIf BeginsWith(line, "list closed") Then
            listInfo.Type = TextActionType.Script
            listInfo.Data = GetEverythingAfter(line, "list closed")
            propName = "list closed"


        ElseIf BeginsWith(line, "list empty <") Then
            listInfo.Type = TextActionType.Text
            listInfo.Data = GetParameter(line, _nullContext)
            propName = "list empty"
        ElseIf Trim(line) = "list empty off" Then
            ' default for list empty is off anyway
            Exit Sub
        ElseIf BeginsWith(line, "list empty") Then
            listInfo.Type = TextActionType.Script
            listInfo.Data = GetEverythingAfter(line, "list empty")
            propName = "list empty"


        ElseIf Trim(line) = "list off" Then
            AddToObjectProperties("not list", id, _nullContext)
            Exit Sub
        ElseIf BeginsWith(line, "list <") Then
            listInfo.Type = TextActionType.Text
            listInfo.Data = GetParameter(line, _nullContext)
            propName = "list"
        ElseIf BeginsWith(line, "list ") Then
            listInfo.Type = TextActionType.Script
            listInfo.Data = GetEverythingAfter(line, "list ")
            propName = "list"
        End If

        If propName <> "" Then
            If listInfo.Type = TextActionType.Text Then
                AddToObjectProperties(propName & "=" & listInfo.Data, id, _nullContext)
            Else
                AddToObjectActions("<" & propName & "> " & listInfo.Data, id, _nullContext)
            End If
        End If
    End Sub

    Private Function GetHTMLColour(colour As String, defaultColour As String) As String
        ' Converts a Quest foreground or background colour setting into an HTML colour

        colour = LCase(colour)

        If colour = "" Or colour = "0" Then colour = defaultColour

        Select Case colour
            Case "white"
                Return "FFFFFF"
            Case "black"
                Return "000000"
            Case "blue"
                Return "0000FF"
            Case "yellow"
                Return "FFFF00"
            Case "red"
                Return "FF0000"
            Case "green"
                Return "00FF00"
            Case Else
                Return colour
        End Select
    End Function

    Private Sub DoPrint(text As String)
        RaiseEvent PrintText(_textFormatter.OutputHTML(text))
    End Sub

    Private Sub DestroyExit(exitData As String, ctx As Context)
        Dim fromRoom As String = ""
        Dim toRoom As String = ""
        Dim roomId, exitId As Integer

        Dim scp = InStr(exitData, ";")
        If scp = 0 Then
            LogASLError("No exit name specified in 'destroy exit <" & exitData & ">'")
            Exit Sub
        End If

        Dim roomExit As RoomExit
        If _gameAslVersion >= 410 Then
            roomExit = FindExit(exitData)
            If roomExit Is Nothing Then
                LogASLError("Can't find exit in 'destroy exit <" & exitData & ">'")
                Exit Sub
            End If

            roomExit.Parent.RemoveExit(roomExit)

        Else

            fromRoom = LCase(Trim(Left(exitData, scp - 1)))
            toRoom = Trim(Mid(exitData, scp + 1))

            ' Find From Room:
            Dim found = False

            For i = 1 To _numberRooms
                If LCase(_rooms(i).RoomName) = fromRoom Then
                    found = True
                    roomId = i
                    Exit For
                End If
            Next i

            If Not found Then
                LogASLError("No such room '" & fromRoom & "'")
                Exit Sub
            End If

            found = False
            Dim r = _rooms(roomId)

            For i = 1 To r.NumberPlaces
                If r.Places(i).PlaceName = toRoom Then
                    exitId = i
                    found = True
                    Exit For
                End If
            Next i

            If found Then
                For i = exitId To r.NumberPlaces - 1
                    r.Places(i) = r.Places(i + 1)
                Next i
                ReDim Preserve r.Places(r.NumberPlaces - 1)
                r.NumberPlaces = r.NumberPlaces - 1
            End If
        End If

        ' Update quest.* vars and obj list
        ShowRoomInfo(_currentRoom, ctx, True)
        UpdateObjectList(ctx)

        AddToChangeLog("room " & fromRoom, "destroy exit " & toRoom)
    End Sub

    Private Sub DoClear()
        _player.ClearScreen()
    End Sub

    Private Sub DoWait()
        _player.DoWait()
        ChangeState(State.Waiting)

        SyncLock _waitLock
            System.Threading.Monitor.Wait(_waitLock)
        End SyncLock
    End Sub

    Private Sub ExecuteFlag(line As String, ctx As Context)
        Dim propertyString As String = ""

        If BeginsWith(line, "on ") Then
            propertyString = GetParameter(line, ctx)
        ElseIf BeginsWith(line, "off ") Then
            propertyString = "not " & GetParameter(line, ctx)
        End If

        ' Game object always has ObjID 1
        AddToObjectProperties(propertyString, 1, ctx)
    End Sub

    Private Function ExecuteIfFlag(flag As String) As Boolean
        ' Game ObjID is 1
        Return GetObjectProperty(flag, 1, True) = "yes"
    End Function

    Private Sub ExecuteIncDec(line As String, ctx As Context)
        Dim var As String
        Dim change As Double
        Dim param = GetParameter(line, ctx)

        Dim sc = InStr(param, ";")
        If sc = 0 Then
            change = 1
            var = param
        Else
            change = Val(Mid(param, sc + 1))
            var = Trim(Left(param, sc - 1))
        End If

        Dim value = GetNumericContents(var, ctx, True)
        If value <= -32766 Then value = 0

        If BeginsWith(line, "inc ") Then
            value = value + change
        ElseIf BeginsWith(line, "dec ") Then
            value = value - change
        End If

        Dim arrayIndex = GetArrayIndex(var, ctx)
        SetNumericVariableContents(var, value, ctx, arrayIndex)
    End Sub

    Private Function ExtractFile(file As String) As String
        Dim length, startPos As Integer
        Dim extracted As Boolean
        Dim resId As Integer

        If _resourceFile = "" Then Return ""

        ' Find file in catalog

        Dim found = False

        For i = 1 To _numResources
            If LCase(file) = LCase(_resources(i).ResourceName) Then
                found = True
                startPos = _resources(i).ResourceStart + _resourceOffset
                length = _resources(i).ResourceLength
                extracted = _resources(i).Extracted
                resId = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("Unable to extract '" & file & "' - not present in resources.", LogType.WarningError)
            Return Nothing
        End If

        Dim fileName = System.IO.Path.Combine(_tempFolder, file)
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName))

        If Not extracted Then
            ' Extract file from cached CAS data
            Dim fileData = Mid(_casFileData, startPos, length)

            ' Write file to temp dir
            System.IO.File.WriteAllText(fileName, fileData, System.Text.Encoding.GetEncoding(1252))

            _resources(resId).Extracted = True
        End If

        Return fileName
    End Function

    Private Sub AddObjectAction(id As Integer, name As String, script As String, Optional noUpdate As Boolean = False)

        ' Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        ' AddToGiveInfo again leading to a big loop

        Dim actionNum As Integer
        Dim foundExisting = False

        Dim o = _objs(id)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = name Then
                foundExisting = True
                actionNum = i
                Exit For
            End If
        Next i

        If Not foundExisting Then
            o.NumberActions = o.NumberActions + 1
            ReDim Preserve o.Actions(o.NumberActions)
            o.Actions(o.NumberActions) = New ActionType
            actionNum = o.NumberActions
        End If

        o.Actions(actionNum).ActionName = name
        o.Actions(actionNum).Script = script

        ObjectActionUpdate(id, name, script, noUpdate)
    End Sub

    Private Sub AddToChangeLog(appliesTo As String, changeData As String)
        _gameChangeData.NumberChanges = _gameChangeData.NumberChanges + 1
        ReDim Preserve _gameChangeData.ChangeData(_gameChangeData.NumberChanges)
        _gameChangeData.ChangeData(_gameChangeData.NumberChanges) = New ChangeType
        _gameChangeData.ChangeData(_gameChangeData.NumberChanges).AppliesTo = appliesTo
        _gameChangeData.ChangeData(_gameChangeData.NumberChanges).Change = changeData
    End Sub

    Private Sub AddToObjectChangeLog(appliesToType As ChangeLog.AppliesTo, appliesTo As String, element As String, changeData As String)
        Dim changeLog As ChangeLog

        ' NOTE: We're only actually ever using the object changelog.
        ' Rooms only get logged for creating rooms and creating/destroying exits, so we don't
        ' need the refactored ChangeLog component for those.

        Select Case appliesToType
            Case ChangeLog.AppliesTo.Object
                changeLog = _changeLogObjects
            Case ChangeLog.AppliesTo.Room
                changeLog = _changeLogRooms
            Case Else
                Throw New ArgumentOutOfRangeException()
        End Select

        changeLog.AddItem(appliesTo, element, changeData)
    End Sub

    Private Sub AddToGiveInfo(id As Integer, giveData As String)
        Dim giveType As GiveType
        Dim actionName As String
        Dim actionScript As String

        Dim o = _objs(id)

        If BeginsWith(giveData, "to ") Then
            giveData = GetEverythingAfter(giveData, "to ")
            If BeginsWith(giveData, "anything ") Then
                o.GiveToAnything = GetEverythingAfter(giveData, "anything ")
                AddObjectAction(id, "give to anything", o.GiveToAnything, True)
                Exit Sub
            Else
                giveType = GiveType.GiveToSomething
                actionName = "give to "
            End If
        Else
            If BeginsWith(giveData, "anything ") Then
                o.GiveAnything = GetEverythingAfter(giveData, "anything ")

                AddObjectAction(id, "give anything", o.GiveAnything, True)
                Exit Sub
            Else
                giveType = GiveType.GiveSomethingTo
                actionName = "give "
            End If
        End If

        If Left(Trim(giveData), 1) = "<" Then
            Dim name = GetParameter(giveData, _nullContext)
            Dim dataId As Integer

            actionName = actionName & "'" & name & "'"

            Dim found = False
            For i = 1 To o.NumberGiveData
                If o.GiveData(i).GiveType = giveType And LCase(o.GiveData(i).GiveObject) = LCase(name) Then
                    dataId = i
                    found = True
                    Exit For
                End If
            Next i

            If Not found Then
                o.NumberGiveData = o.NumberGiveData + 1
                ReDim Preserve o.GiveData(o.NumberGiveData)
                o.GiveData(o.NumberGiveData) = New GiveDataType
                dataId = o.NumberGiveData
            End If

            Dim EP = InStr(giveData, ">")
            o.GiveData(dataId).GiveType = giveType
            o.GiveData(dataId).GiveObject = name
            o.GiveData(dataId).GiveScript = Mid(giveData, EP + 2)

            actionScript = o.GiveData(dataId).GiveScript
            AddObjectAction(id, actionName, actionScript, True)
        End If
    End Sub

    Friend Sub AddToObjectActions(actionInfo As String, id As Integer, ctx As Context)
        Dim actionNum As Integer
        Dim foundExisting = False

        Dim name = LCase(GetParameter(actionInfo, ctx))
        Dim ep = InStr(actionInfo, ">")
        If ep = Len(actionInfo) Then
            LogASLError("No script given for '" & name & "' action data", LogType.WarningError)
            Exit Sub
        End If

        Dim script = Trim(Mid(actionInfo, ep + 1))

        Dim o = _objs(id)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = name Then
                foundExisting = True
                actionNum = i
                Exit For
            End If
        Next i

        If Not foundExisting Then
            o.NumberActions = o.NumberActions + 1
            ReDim Preserve o.Actions(o.NumberActions)
            o.Actions(o.NumberActions) = New ActionType
            actionNum = o.NumberActions
        End If

        o.Actions(actionNum).ActionName = name
        o.Actions(actionNum).Script = script

        ObjectActionUpdate(id, name, script)
    End Sub

    Private Sub AddToObjectAltNames(altNames As String, id As Integer)
        Dim o = _objs(id)

        Do
            Dim endPos = InStr(altNames, ";")
            If endPos = 0 Then endPos = Len(altNames) + 1
            Dim curName = Trim(Left(altNames, endPos - 1))

            If curName <> "" Then
                o.NumberAltNames = o.NumberAltNames + 1
                ReDim Preserve o.AltNames(o.NumberAltNames)
                o.AltNames(o.NumberAltNames) = curName
            End If

            altNames = Mid(altNames, endPos + 1)
        Loop Until Trim(altNames) = ""
    End Sub

    Friend Sub AddToObjectProperties(propertyInfo As String, id As Integer, ctx As Context)
        If id = 0 Then Return

        If Right(propertyInfo, 1) <> ";" Then
            propertyInfo = propertyInfo & ";"
        End If

        Do
            Dim scp = InStr(propertyInfo, ";")
            Dim info = Left(propertyInfo, scp - 1)
            propertyInfo = Trim(Mid(propertyInfo, scp + 1))

            Dim name, value As String
            Dim num As Integer

            If info = "" Then Exit Do

            Dim ep = InStr(info, "=")
            If ep <> 0 Then
                name = Trim(Left(info, ep - 1))
                value = Trim(Mid(info, ep + 1))
            Else
                name = info
                value = ""
            End If

            Dim falseProperty = False
            If BeginsWith(name, "not ") And value = "" Then
                falseProperty = True
                name = GetEverythingAfter(name, "not ")
            End If

            Dim o = _objs(id)

            Dim found = False
            For i = 1 To o.NumberProperties
                If LCase(o.Properties(i).PropertyName) = LCase(name) Then
                    found = True
                    num = i
                    i = o.NumberProperties
                End If
            Next i

            If Not found Then
                o.NumberProperties = o.NumberProperties + 1
                ReDim Preserve o.Properties(o.NumberProperties)
                o.Properties(o.NumberProperties) = New PropertyType
                num = o.NumberProperties
            End If

            If falseProperty Then
                o.Properties(num).PropertyName = ""
            Else
                o.Properties(num).PropertyName = name
                o.Properties(num).PropertyValue = value
            End If

            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs(id).ObjectName, name, "properties " & info)

            Select Case name
                Case "alias"
                    If o.IsRoom Then
                        _rooms(o.CorresRoomId).RoomAlias = value
                    Else
                        o.ObjectAlias = value
                    End If
                    If _gameFullyLoaded Then
                        UpdateObjectList(ctx)
                        UpdateItems(ctx)
                    End If
                Case "prefix"
                    If o.IsRoom Then
                        _rooms(o.CorresRoomId).Prefix = value
                    Else
                        If value <> "" Then
                            o.Prefix = value & " "
                        Else
                            o.Prefix = ""
                        End If
                    End If
                Case "indescription"
                    If o.IsRoom Then _rooms(o.CorresRoomId).InDescription = value
                Case "description"
                    If o.IsRoom Then
                        _rooms(o.CorresRoomId).Description.Data = value
                        _rooms(o.CorresRoomId).Description.Type = TextActionType.Text
                    End If
                Case "look"
                    If o.IsRoom Then
                        _rooms(o.CorresRoomId).Look = value
                    End If
                Case "suffix"
                    o.Suffix = value
                Case "displaytype"
                    o.DisplayType = value
                    If _gameFullyLoaded Then UpdateObjectList(ctx)
                Case "gender"
                    o.Gender = value
                Case "article"
                    o.Article = value
                Case "detail"
                    o.Detail = value
                Case "hidden"
                    If falseProperty Then
                        o.Exists = True
                    Else
                        o.Exists = False
                    End If

                    If _gameFullyLoaded Then UpdateObjectList(ctx)
                Case "invisible"
                    If falseProperty Then
                        o.Visible = True
                    Else
                        o.Visible = False
                    End If

                    If _gameFullyLoaded Then UpdateObjectList(ctx)
                Case "take"
                    If _gameAslVersion >= 392 Then
                        If falseProperty Then
                            o.Take.Type = TextActionType.Nothing
                        Else
                            If value = "" Then
                                o.Take.Type = TextActionType.Default
                            Else
                                o.Take.Type = TextActionType.Text
                                o.Take.Data = value
                            End If
                        End If
                    End If
            End Select
        Loop Until Len(Trim(propertyInfo)) = 0
    End Sub

    Private Sub AddToUseInfo(id As Integer, useData As String)
        Dim useType As UseType

        Dim o = _objs(id)

        If BeginsWith(useData, "on ") Then
            useData = GetEverythingAfter(useData, "on ")
            If BeginsWith(useData, "anything ") Then
                o.UseOnAnything = GetEverythingAfter(useData, "anything ")
                Exit Sub
            Else
                useType = UseType.UseOnSomething
            End If
        Else
            If BeginsWith(useData, "anything ") Then
                o.UseAnything = GetEverythingAfter(useData, "anything ")
                Exit Sub
            Else
                useType = UseType.UseSomethingOn
            End If
        End If

        If Left(Trim(useData), 1) = "<" Then
            Dim objectName = GetParameter(useData, _nullContext)
            Dim dataId As Integer
            Dim found = False

            For i = 1 To o.NumberUseData
                If o.UseData(i).UseType = useType And LCase(o.UseData(i).UseObject) = LCase(objectName) Then
                    dataId = i
                    found = True
                    Exit For
                End If
            Next i

            If Not found Then
                o.NumberUseData = o.NumberUseData + 1
                ReDim Preserve o.UseData(o.NumberUseData)
                o.UseData(o.NumberUseData) = New UseDataType
                dataId = o.NumberUseData
            End If

            Dim ep = InStr(useData, ">")
            o.UseData(dataId).UseType = useType
            o.UseData(dataId).UseObject = objectName
            o.UseData(dataId).UseScript = Mid(useData, ep + 2)
        Else
            o.Use = Trim(useData)
        End If

    End Sub

    Private Function CapFirst(s As String) As String
        Return UCase(Left(s, 1)) & Mid(s, 2)
    End Function

    Private Function ConvertVarsIn(s As String, ctx As Context) As String
        Return GetParameter("<" & s & ">", ctx)
    End Function

    Private Function DisambObjHere(ctx As Context, id As Integer, firstPlace As String, Optional twoPlaces As Boolean = False, Optional secondPlace As String = "", Optional isExit As Boolean = False) As Boolean

        Dim isSeen As Boolean
        Dim onlySeen = False

        If firstPlace = "game" Then
            firstPlace = ""
            If secondPlace = "seen" Then
                twoPlaces = False
                secondPlace = ""
                onlySeen = True
                Dim roomObjId = _rooms(GetRoomID(_objs(id).ContainerRoom, ctx)).ObjId

                If _objs(id).ContainerRoom = "inventory" Then
                    isSeen = True
                Else
                    If IsYes(GetObjectProperty("visited", roomObjId, True, False)) Then
                        isSeen = True
                    Else
                        If IsYes(GetObjectProperty("seen", id, True, False)) Then
                            isSeen = True
                        End If
                    End If
                End If

            End If
        End If

        If ((twoPlaces = False And (LCase(_objs(id).ContainerRoom) = LCase(firstPlace) Or firstPlace = "")) Or (twoPlaces = True And (LCase(_objs(id).ContainerRoom) = LCase(firstPlace) Or LCase(_objs(id).ContainerRoom) = LCase(secondPlace)))) And _objs(id).Exists = True And _objs(id).IsExit = isExit Then
            If Not onlySeen Then
                Return True
            End If
            Return isSeen
        End If

        Return False
    End Function

    Private Sub ExecClone(cloneString As String, ctx As Context)
        Dim id As Integer
        Dim newName, cloneTo As String

        Dim SC = InStr(cloneString, ";")
        If SC = 0 Then
            LogASLError("No new object name specified in 'clone <" & cloneString & ">", LogType.WarningError)
            Exit Sub
        Else
            Dim objectToClone = Trim(Left(cloneString, SC - 1))
            id = GetObjectIdNoAlias(objectToClone)

            Dim SC2 = InStr(SC + 1, cloneString, ";")
            If SC2 = 0 Then
                cloneTo = _objs(id).ContainerRoom
                newName = Trim(Mid(cloneString, SC + 1))
            Else
                cloneTo = Trim(Mid(cloneString, SC2 + 1))
                newName = Trim(Mid(cloneString, SC + 1, (SC2 - SC) - 1))
            End If
        End If

        _numberObjs = _numberObjs + 1
        ReDim Preserve _objs(_numberObjs)
        _objs(_numberObjs) = New ObjectType
        _objs(_numberObjs) = _objs(id)
        _objs(_numberObjs).ContainerRoom = cloneTo
        _objs(_numberObjs).ObjectName = newName

        If _objs(id).IsRoom Then
            ' This is a room so create the corresponding room as well

            _numberRooms = _numberRooms + 1
            ReDim Preserve _rooms(_numberRooms)
            _rooms(_numberRooms) = New RoomType
            _rooms(_numberRooms) = _rooms(_objs(id).CorresRoomId)
            _rooms(_numberRooms).RoomName = newName
            _rooms(_numberRooms).ObjId = _numberObjs

            _objs(_numberObjs).CorresRoom = newName
            _objs(_numberObjs).CorresRoomId = _numberRooms

            AddToChangeLog("room " & newName, "create")
        Else
            AddToChangeLog("object " & newName, "create " & _objs(_numberObjs).ContainerRoom)
        End If

        UpdateObjectList(ctx)
    End Sub

    Private Sub ExecOops(correction As String, ctx As Context)
        If _badCmdBefore <> "" Then
            If _badCmdAfter = "" Then
                ExecCommand(_badCmdBefore & " " & correction, ctx, False)
            Else
                ExecCommand(_badCmdBefore & " " & correction & " " & _badCmdAfter, ctx, False)
            End If
        End If
    End Sub

    Private Sub ExecType(typeData As String, ctx As Context)
        Dim id As Integer
        Dim found As Boolean
        Dim scp = InStr(typeData, ";")

        If scp = 0 Then
            LogASLError("No type name given in 'type <" & typeData & ">'")
            Exit Sub
        End If

        Dim objName = Trim(Left(typeData, scp - 1))
        Dim typeName = Trim(Mid(typeData, scp + 1))

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(objName) Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object in 'type <" & typeData & ">'")
            Exit Sub
        End If

        Dim o = _objs(id)

        o.NumberTypesIncluded = o.NumberTypesIncluded + 1
        ReDim Preserve o.TypesIncluded(o.NumberTypesIncluded)
        o.TypesIncluded(o.NumberTypesIncluded) = typeName

        Dim propertyData = GetPropertiesInType(typeName)
        AddToObjectProperties(propertyData.Properties, id, ctx)
        For i = 1 To propertyData.NumberActions
            AddObjectAction(id, propertyData.Actions(i).ActionName, propertyData.Actions(i).Script)
        Next i

        ' New as of Quest 4.0. Fixes bug that "if type" would fail for any
        ' parent types included by the "type" command.
        For i = 1 To propertyData.NumberTypesIncluded
            o.NumberTypesIncluded = o.NumberTypesIncluded + 1
            ReDim Preserve o.TypesIncluded(o.NumberTypesIncluded)
            o.TypesIncluded(o.NumberTypesIncluded) = propertyData.TypesIncluded(i)
        Next i
    End Sub

    Private Function ExecuteIfAction(actionData As String) As Boolean
        Dim id As Integer

        Dim scp = InStr(actionData, ";")

        If scp = 0 Then
            LogASLError("No action name given in condition 'action <" & actionData & ">' ...", LogType.WarningError)
            Return False
        End If

        Dim objName = Trim(Left(actionData, scp - 1))
        Dim actionName = Trim(Mid(actionData, scp + 1))
        Dim found = False

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(objName) Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object '" & objName & "' in condition 'action <" & actionData & ">' ...", LogType.WarningError)
            Return False
        End If

        Dim o = _objs(id)

        For i = 1 To o.NumberActions
            If LCase(o.Actions(i).ActionName) = LCase(actionName) Then
                Return True
            End If
        Next i

        Return False
    End Function

    Private Function ExecuteIfType(typeData As String) As Boolean
        Dim id As Integer

        Dim scp = InStr(typeData, ";")

        If scp = 0 Then
            LogASLError("No type name given in condition 'type <" & typeData & ">' ...", LogType.WarningError)
            Return False
        End If

        Dim objName = Trim(Left(typeData, scp - 1))
        Dim typeName = Trim(Mid(typeData, scp + 1))

        Dim found = False

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(objName) Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object '" & objName & "' in condition 'type <" & typeData & ">' ...", LogType.WarningError)
            Return False
        End If

        Dim o = _objs(id)

        For i = 1 To o.NumberTypesIncluded
            If LCase(o.TypesIncluded(i)) = LCase(typeName) Then
                Return True
            End If
        Next i

        Return False
    End Function

    ' TODO: varName was ByRef
    Private Function GetArrayIndex(varName As String, ctx As Context) As Integer
        Dim index As Integer

        If InStr(varName, "[") <> 0 And InStr(varName, "]") <> 0 Then
            Dim beginPos = InStr(varName, "[")
            Dim endPos = InStr(varName, "]")
            Dim data = Mid(varName, beginPos + 1, (endPos - beginPos) - 1)
            If IsNumeric(data) Then
                index = CInt(data)
            Else
                index = CInt(GetNumericContents(data, ctx))
            End If
            varName = Left(varName, beginPos - 1)
        End If

        Return index
    End Function

    Friend Function Disambiguate(name As String, containedIn As String, ctx As Context, Optional isExit As Boolean = False) As Integer
        ' Returns object ID being referred to by player.
        ' Returns -1 if object doesn't exist, calling function
        '   then expected to print relevant error.
        ' Returns -2 if "it" meaningless, prints own error.
        ' If it returns an object ID, it also sets quest.lastobject to the name
        ' of the object referred to.
        ' If ctx.AllowRealNamesInCommand is True, will allow an object's real
        ' name to be used even when the object has an alias - this is used when
        ' Disambiguate has been called after an "exec" command to prevent the
        ' player having to choose an object from the disambiguation menu twice

        Dim numberCorresIds = 0
        Dim idNumbers(0) As Integer
        Dim firstPlace As String
        Dim secondPlace As String = ""
        Dim twoPlaces As Boolean
        Dim descriptionText() As String
        Dim validNames() As String
        Dim numValidNames As Integer

        name = Trim(name)

        SetStringContents("quest.lastobject", "", ctx)

        If InStr(containedIn, ";") <> 0 Then
            Dim scp = InStr(containedIn, ";")
            twoPlaces = True
            firstPlace = Trim(Left(containedIn, scp - 1))
            secondPlace = Trim(Mid(containedIn, scp + 1))
        Else
            twoPlaces = False
            firstPlace = containedIn
        End If

        If ctx.AllowRealNamesInCommand Then
            For i = 1 To _numberObjs
                If DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace) Then
                    If LCase(_objs(i).ObjectName) = LCase(name) Then
                        SetStringContents("quest.lastobject", _objs(i).ObjectName, ctx)
                        Return i
                    End If
                End If
            Next i
        End If

        ' If player uses "it", "them" etc. as name:
        If name = "it" Or name = "them" Or name = "this" Or name = "those" Or name = "these" Or name = "that" Then
            SetStringContents("quest.error.pronoun", name, ctx)
            If _lastIt <> 0 And _lastItMode = ItType.Inanimate And DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace) Then
                SetStringContents("quest.lastobject", _objs(_lastIt).ObjectName, ctx)
                Return _lastIt
            Else
                PlayerErrorMessage(PlayerError.BadPronoun, ctx)
                Return -2
            End If
        ElseIf name = "him" Then
            SetStringContents("quest.error.pronoun", name, ctx)
            If _lastIt <> 0 And _lastItMode = ItType.Male And DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace) Then
                SetStringContents("quest.lastobject", _objs(_lastIt).ObjectName, ctx)
                Return _lastIt
            Else
                PlayerErrorMessage(PlayerError.BadPronoun, ctx)
                Return -2
            End If
        ElseIf name = "her" Then
            SetStringContents("quest.error.pronoun", name, ctx)
            If _lastIt <> 0 And _lastItMode = ItType.Female And DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace) Then
                SetStringContents("quest.lastobject", _objs(_lastIt).ObjectName, ctx)
                Return _lastIt
            Else
                PlayerErrorMessage(PlayerError.BadPronoun, ctx)
                Return -2
            End If
        End If

        _thisTurnIt = 0

        If BeginsWith(name, "the ") Then
            name = GetEverythingAfter(name, "the ")
        End If

        For i = 1 To _numberObjs
            If DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit) Then
                numValidNames = _objs(i).NumberAltNames + 1
                ReDim validNames(numValidNames)
                validNames(1) = _objs(i).ObjectAlias
                For j = 1 To _objs(i).NumberAltNames
                    validNames(j + 1) = _objs(i).AltNames(j)
                Next j

                For j = 1 To numValidNames
                    If ((LCase(validNames(j)) = LCase(name)) Or ("the " & LCase(name) = LCase(validNames(j)))) Then
                        numberCorresIds = numberCorresIds + 1
                        ReDim Preserve idNumbers(numberCorresIds)
                        idNumbers(numberCorresIds) = i
                        j = numValidNames
                    End If
                Next j
            End If
        Next i

        If _gameAslVersion >= 391 And numberCorresIds = 0 And _useAbbreviations And Len(name) > 0 Then
            ' Check for abbreviated object names

            For i = 1 To _numberObjs
                If DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit) Then
                    Dim thisName As String
                    If _objs(i).ObjectAlias <> "" Then thisName = LCase(_objs(i).ObjectAlias) Else thisName = LCase(_objs(i).ObjectName)
                    If _gameAslVersion >= 410 Then
                        If _objs(i).Prefix <> "" Then thisName = Trim(LCase(_objs(i).Prefix)) & " " & thisName
                        If _objs(i).Suffix <> "" Then thisName = thisName & " " & Trim(LCase(_objs(i).Suffix))
                    End If
                    If InStr(" " & thisName, " " & LCase(name)) <> 0 Then
                        numberCorresIds = numberCorresIds + 1
                        ReDim Preserve idNumbers(numberCorresIds)
                        idNumbers(numberCorresIds) = i
                    End If
                End If
            Next i
        End If

        If numberCorresIds = 1 Then
            SetStringContents("quest.lastobject", _objs(idNumbers(1)).ObjectName, ctx)
            _thisTurnIt = idNumbers(1)

            Select Case _objs(idNumbers(1)).Article
                Case "him"
                    _thisTurnItMode = ItType.Male
                Case "her"
                    _thisTurnItMode = ItType.Female
                Case Else
                    _thisTurnItMode = ItType.Inanimate
            End Select

            Return idNumbers(1)
        ElseIf numberCorresIds > 1 Then
            ReDim descriptionText(numberCorresIds)

            Dim question = "Please select which " & name & " you mean:"
            Print("- |i" & question & "|xi", ctx)

            Dim menuItems As New Dictionary(Of String, String)

            For i = 1 To numberCorresIds
                descriptionText(i) = _objs(idNumbers(i)).Detail
                If descriptionText(i) = "" Then
                    If _objs(idNumbers(i)).Prefix = "" Then
                        descriptionText(i) = _objs(idNumbers(i)).ObjectAlias
                    Else
                        descriptionText(i) = _objs(idNumbers(i)).Prefix & _objs(idNumbers(i)).ObjectAlias
                    End If
                End If

                menuItems.Add(CStr(i), descriptionText(i))

            Next i

            Dim mnu As New MenuData(question, menuItems, False)
            Dim response As String = ShowMenu(mnu)

            _choiceNumber = CInt(response)

            SetStringContents("quest.lastobject", _objs(idNumbers(_choiceNumber)).ObjectName, ctx)

            _thisTurnIt = idNumbers(_choiceNumber)

            Select Case _objs(idNumbers(_choiceNumber)).Article
                Case "him"
                    _thisTurnItMode = ItType.Male
                Case "her"
                    _thisTurnItMode = ItType.Female
                Case Else
                    _thisTurnItMode = ItType.Inanimate
            End Select

            Print("- " & descriptionText(_choiceNumber) & "|n", ctx)

            Return idNumbers(_choiceNumber)
        End If

        _thisTurnIt = _lastIt
        SetStringContents("quest.error.object", name, ctx)
        Return -1
    End Function

    Private Function DisplayStatusVariableInfo(id As Integer, type As VarType, ctx As Context) As String
        Dim displayData As String = ""
        Dim ep As Integer

        If type = VarType.String Then
            displayData = ConvertVarsIn(_stringVariable(id).DisplayString, ctx)
            ep = InStr(displayData, "!")

            If ep <> 0 Then
                displayData = Left(displayData, ep - 1) & _stringVariable(id).VariableContents(0) & Mid(displayData, ep + 1)
            End If
        ElseIf type = VarType.Numeric Then
            If _numericVariable(id).NoZeroDisplay And Val(_numericVariable(id).VariableContents(0)) = 0 Then
                Return ""
            End If
            displayData = ConvertVarsIn(_numericVariable(id).DisplayString, ctx)
            ep = InStr(displayData, "!")

            If ep <> 0 Then
                displayData = Left(displayData, ep - 1) & _numericVariable(id).VariableContents(0) & Mid(displayData, ep + 1)
            End If

            If InStr(displayData, "*") > 0 Then
                Dim firstStar = InStr(displayData, "*")
                Dim secondStar = InStr(firstStar + 1, displayData, "*")
                Dim beforeStar = Left(displayData, firstStar - 1)
                Dim afterStar = Mid(displayData, secondStar + 1)
                Dim betweenStar = Mid(displayData, firstStar + 1, (secondStar - firstStar) - 1)

                If CDbl(_numericVariable(id).VariableContents(0)) <> 1 Then
                    displayData = beforeStar & betweenStar & afterStar
                Else
                    displayData = beforeStar & afterStar
                End If
            End If
        End If

        Return displayData
    End Function

    Friend Function DoAction(ObjID As Integer, ActionName As String, ctx As Context, Optional LogError As Boolean = True) As Boolean
        Dim FoundAction As Boolean
        Dim ActionScript As String = ""

        Dim o = _objs(ObjID)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = LCase(ActionName) Then
                FoundAction = True
                ActionScript = o.Actions(i).Script
                Exit For
            End If
        Next i

        If Not FoundAction Then
            If LogError Then LogASLError("No such action '" & ActionName & "' defined for object '" & o.ObjectName & "'")
            Return False
        End If

        Dim NewThread As Context = CopyContext(ctx)
        NewThread.CallingObjectId = ObjID

        ExecuteScript(ActionScript, NewThread, ObjID)

        Return True
    End Function

    Public Function HasAction(ObjID As Integer, ActionName As String) As Boolean
        Dim o = _objs(ObjID)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = LCase(ActionName) Then
                Return True
            End If
        Next i

        Return False
    End Function

    Private Sub ExecForEach(scriptLine As String, ctx As Context)
        Dim inLocation, scriptToRun As String
        Dim isExit As Boolean
        Dim isRoom As Boolean

        If BeginsWith(scriptLine, "object ") Then
            scriptLine = GetEverythingAfter(scriptLine, "object ")
            If Not BeginsWith(scriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each object " & ReportErrorLine(scriptLine) & "'", LogType.WarningError)
                Exit Sub
            End If
        ElseIf BeginsWith(scriptLine, "exit ") Then
            scriptLine = GetEverythingAfter(scriptLine, "exit ")
            If Not BeginsWith(scriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each exit " & ReportErrorLine(scriptLine) & "'", LogType.WarningError)
                Exit Sub
            End If
            isExit = True
        ElseIf BeginsWith(scriptLine, "room ") Then
            scriptLine = GetEverythingAfter(scriptLine, "room ")
            If Not BeginsWith(scriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each room " & ReportErrorLine(scriptLine) & "'", LogType.WarningError)
                Exit Sub
            End If
            isRoom = True
        Else
            LogASLError("Unknown type in 'for each " & ReportErrorLine(scriptLine) & "'", LogType.WarningError)
            Exit Sub
        End If

        scriptLine = GetEverythingAfter(scriptLine, "in ")

        If BeginsWith(scriptLine, "game ") Then
            inLocation = ""
            scriptToRun = GetEverythingAfter(scriptLine, "game ")
        Else
            inLocation = LCase(GetParameter(scriptLine, ctx))
            Dim bracketPos = InStr(scriptLine, ">")
            scriptToRun = Trim(Mid(scriptLine, bracketPos + 1))
        End If

        For i = 1 To _numberObjs
            If inLocation = "" Or LCase(_objs(i).ContainerRoom) = inLocation Then
                If _objs(i).IsRoom = isRoom And _objs(i).IsExit = isExit Then
                    SetStringContents("quest.thing", _objs(i).ObjectName, ctx)
                    ExecuteScript(scriptToRun, ctx)
                End If
            End If
        Next i
    End Sub

    Private Sub ExecuteAction(data As String, ctx As Context)
        Dim actionName As String
        Dim script As String
        Dim actionNum As Integer
        Dim id As Integer
        Dim foundExisting = False
        Dim foundObject = False

        Dim param = GetParameter(data, ctx)
        Dim scp = InStr(param, ";")
        If scp = 0 Then
            LogASLError("No action name specified in 'action " & data & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim objName = Trim(Left(param, scp - 1))
        actionName = Trim(Mid(param, scp + 1))

        Dim ep = InStr(data, ">")
        If ep = Len(Trim(data)) Then
            script = ""
        Else
            script = Trim(Mid(data, ep + 1))
        End If

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(objName) Then
                foundObject = True
                id = i
                Exit For
            End If
        Next i

        If Not foundObject Then
            LogASLError("No such object '" & objName & "' in 'action " & data & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim o = _objs(id)

        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = actionName Then
                foundExisting = True
                actionNum = i
                Exit For
            End If
        Next i

        If Not foundExisting Then
            o.NumberActions = o.NumberActions + 1
            ReDim Preserve o.Actions(o.NumberActions)
            o.Actions(o.NumberActions) = New ActionType
            actionNum = o.NumberActions
        End If

        o.Actions(actionNum).ActionName = actionName
        o.Actions(actionNum).Script = script

        ObjectActionUpdate(id, actionName, script)
    End Sub

    Private Function ExecuteCondition(condition As String, ctx As Context) As Boolean
        Dim result, thisNot As Boolean

        If BeginsWith(condition, "not ") Then
            thisNot = True
            condition = GetEverythingAfter(condition, "not ")
        Else
            thisNot = False
        End If

        If BeginsWith(condition, "got ") Then
            result = ExecuteIfGot(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "has ") Then
            result = ExecuteIfHas(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "ask ") Then
            result = ExecuteIfAsk(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "is ") Then
            result = ExecuteIfIs(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "here ") Then
            result = ExecuteIfHere(GetParameter(condition, ctx), ctx)
        ElseIf BeginsWith(condition, "exists ") Then
            result = ExecuteIfExists(GetParameter(condition, ctx), False)
        ElseIf BeginsWith(condition, "real ") Then
            result = ExecuteIfExists(GetParameter(condition, ctx), True)
        ElseIf BeginsWith(condition, "property ") Then
            result = ExecuteIfProperty(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "action ") Then
            result = ExecuteIfAction(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "type ") Then
            result = ExecuteIfType(GetParameter(condition, ctx))
        ElseIf BeginsWith(condition, "flag ") Then
            result = ExecuteIfFlag(GetParameter(condition, ctx))
        End If

        If thisNot Then result = Not result

        Return result
    End Function

    Private Function ExecuteConditions(list As String, ctx As Context) As Boolean
        Dim conditions() As String
        Dim numConditions = 0
        Dim operations() As String
        Dim obscuredConditionList = ObliterateParameters(list)
        Dim pos = 1
        Dim isFinalCondition = False

        Do
            numConditions = numConditions + 1
            ReDim Preserve conditions(numConditions)
            ReDim Preserve operations(numConditions)

            Dim nextCondition = "AND"
            Dim nextConditionPos = InStr(pos, obscuredConditionList, "and ")
            If nextConditionPos = 0 Then
                nextConditionPos = InStr(pos, obscuredConditionList, "or ")
                nextCondition = "OR"
            End If

            If nextConditionPos = 0 Then
                nextConditionPos = Len(obscuredConditionList) + 2
                isFinalCondition = True
                nextCondition = "FINAL"
            End If

            Dim thisCondition = Trim(Mid(list, pos, nextConditionPos - pos - 1))
            conditions(numConditions) = thisCondition
            operations(numConditions) = nextCondition

            ' next condition starts from space after and/or
            pos = InStr(nextConditionPos, obscuredConditionList, " ")
        Loop Until isFinalCondition

        operations(0) = "AND"
        Dim result = True

        For i = 1 To numConditions
            Dim thisResult = ExecuteCondition(conditions(i), ctx)

            If operations(i - 1) = "AND" Then
                result = thisResult And result
            ElseIf operations(i - 1) = "OR" Then
                result = thisResult Or result
            End If
        Next i

        Return result
    End Function

    Private Sub ExecuteCreate(data As String, ctx As Context)
        Dim newName As String

        If BeginsWith(data, "room ") Then
            newName = GetParameter(data, ctx)
            _numberRooms = _numberRooms + 1
            ReDim Preserve _rooms(_numberRooms)
            _rooms(_numberRooms) = New RoomType
            _rooms(_numberRooms).RoomName = newName

            _numberObjs = _numberObjs + 1
            ReDim Preserve _objs(_numberObjs)
            _objs(_numberObjs) = New ObjectType
            _objs(_numberObjs).ObjectName = newName
            _objs(_numberObjs).IsRoom = True
            _objs(_numberObjs).CorresRoom = newName
            _objs(_numberObjs).CorresRoomId = _numberRooms

            _rooms(_numberRooms).ObjId = _numberObjs

            AddToChangeLog("room " & newName, "create")

            If _gameAslVersion >= 410 Then
                AddToObjectProperties(_defaultRoomProperties.Properties, _numberObjs, ctx)
                For j = 1 To _defaultRoomProperties.NumberActions
                    AddObjectAction(_numberObjs, _defaultRoomProperties.Actions(j).ActionName, _defaultRoomProperties.Actions(j).Script)
                Next j

                _rooms(_numberRooms).Exits = New RoomExits(Me)
                _rooms(_numberRooms).Exits.ObjID = _rooms(_numberRooms).ObjId
            End If

        ElseIf BeginsWith(data, "object ") Then
            Dim paramData = GetParameter(data, ctx)
            Dim scp = InStr(paramData, ";")
            Dim containerRoom As String

            If scp = 0 Then
                newName = paramData
                containerRoom = ""
            Else
                newName = Trim(Left(paramData, scp - 1))
                containerRoom = Trim(Mid(paramData, scp + 1))
            End If

            _numberObjs = _numberObjs + 1
            ReDim Preserve _objs(_numberObjs)
            _objs(_numberObjs) = New ObjectType

            Dim o = _objs(_numberObjs)
            o.ObjectName = newName
            o.ObjectAlias = newName
            o.ContainerRoom = containerRoom
            o.Exists = True
            o.Visible = True
            o.Gender = "it"
            o.Article = "it"

            AddToChangeLog("object " & newName, "create " & _objs(_numberObjs).ContainerRoom)

            If _gameAslVersion >= 410 Then
                AddToObjectProperties(_defaultProperties.Properties, _numberObjs, ctx)
                For j = 1 To _defaultProperties.NumberActions
                    AddObjectAction(_numberObjs, _defaultProperties.Actions(j).ActionName, _defaultProperties.Actions(j).Script)
                Next j
            End If

            If Not _gameLoading Then UpdateObjectList(ctx)

        ElseIf BeginsWith(data, "exit ") Then
            ExecuteCreateExit(data, ctx)
        End If
    End Sub

    Private Sub ExecuteCreateExit(data As String, ctx As Context)
        Dim scrRoom As String
        Dim destRoom As String = ""
        Dim destId As Integer
        Dim exitData = GetEverythingAfter(data, "exit ")
        Dim newName = GetParameter(data, ctx)
        Dim scp = InStr(newName, ";")

        If _gameAslVersion < 410 Then
            If scp = 0 Then
                LogASLError("No exit destination given in 'create exit " & exitData & "'", LogType.WarningError)
                Exit Sub
            End If
        End If

        If scp = 0 Then
            scrRoom = Trim(newName)
        Else
            scrRoom = Trim(Left(newName, scp - 1))
        End If
        Dim srcId = GetRoomID(scrRoom, ctx)

        If srcId = 0 Then
            LogASLError("No such room '" & scrRoom & "'", LogType.WarningError)
            Exit Sub
        End If

        If _gameAslVersion < 410 Then
            ' only do destination room check for ASL <410, as can now have scripts on dynamically
            ' created exits, so the destination doesn't necessarily have to exist.

            destRoom = Trim(Mid(newName, scp + 1))
            If destRoom <> "" Then
                destId = GetRoomID(destRoom, ctx)

                If destId = 0 Then
                    LogASLError("No such room '" & destRoom & "'", LogType.WarningError)
                    Exit Sub
                End If
            End If
        End If

        ' If it's a "go to" exit, check if it already exists:
        Dim exists = False
        If BeginsWith(exitData, "<") Then
            If _gameAslVersion >= 410 Then
                exists = _rooms(srcId).Exits.Places.ContainsKey(destRoom)
            Else
                For i = 1 To _rooms(srcId).NumberPlaces
                    If LCase(_rooms(srcId).Places(i).PlaceName) = LCase(destRoom) Then
                        exists = True
                        Exit For
                    End If
                Next i
            End If

            If exists Then
                LogASLError("Exit from '" & scrRoom & "' to '" & destRoom & "' already exists", LogType.WarningError)
                Exit Sub
            End If
        End If

        Dim paramPos = InStr(exitData, "<")
        Dim saveData As String
        If paramPos = 0 Then
            saveData = exitData
        Else
            saveData = Left(exitData, paramPos - 1)
            ' We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData & "<" & GetParameter(exitData, ctx) & ">"
        End If
        AddToChangeLog("room " & _rooms(srcId).RoomName, "exit " & saveData)

        Dim r = _rooms(srcId)

        If _gameAslVersion >= 410 Then
            r.Exits.AddExitFromCreateScript(exitData, ctx)
        Else
            If BeginsWith(exitData, "north ") Then
                r.North.Data = destRoom
                r.North.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "south ") Then
                r.South.Data = destRoom
                r.South.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "east ") Then
                r.East.Data = destRoom
                r.East.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "west ") Then
                r.West.Data = destRoom
                r.West.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "northeast ") Then
                r.NorthEast.Data = destRoom
                r.NorthEast.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "northwest ") Then
                r.NorthWest.Data = destRoom
                r.NorthWest.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "southeast ") Then
                r.SouthEast.Data = destRoom
                r.SouthEast.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "southwest ") Then
                r.SouthWest.Data = destRoom
                r.SouthWest.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "up ") Then
                r.Up.Data = destRoom
                r.Up.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "down ") Then
                r.Down.Data = destRoom
                r.Down.Type = TextActionType.Text
            ElseIf BeginsWith(exitData, "out ") Then
                r.Out.Text = destRoom
            ElseIf BeginsWith(exitData, "<") Then
                r.NumberPlaces = r.NumberPlaces + 1
                ReDim Preserve r.Places(r.NumberPlaces)
                r.Places(r.NumberPlaces) = New PlaceType
                r.Places(r.NumberPlaces).PlaceName = destRoom
            Else
                LogASLError("Invalid direction in 'create exit " & exitData & "'", LogType.WarningError)
            End If
        End If

        If Not _gameLoading Then
            ' Update quest.doorways variables
            ShowRoomInfo(_currentRoom, ctx, True)

            UpdateObjectList(ctx)

            If _gameAslVersion < 410 Then
                If _currentRoom = _rooms(srcId).RoomName Then
                    UpdateDoorways(srcId, ctx)
                ElseIf _currentRoom = _rooms(destId).RoomName Then
                    UpdateDoorways(destId, ctx)
                End If
            Else
                ' Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                ' for current room anyway.
                UpdateDoorways(GetRoomID(_currentRoom, ctx), ctx)
            End If
        End If
    End Sub

    Private Sub ExecDrop(obj As String, ctx As Context)
        Dim found As Boolean
        Dim parentId, id As Integer

        id = Disambiguate(obj, "inventory", ctx)

        If id > 0 Then
            found = True
        Else
            found = False
        End If

        If Not found Then
            If id <> -2 Then
                If _gameAslVersion >= 391 Then
                    PlayerErrorMessage(PlayerError.NoItem, ctx)
                Else
                    PlayerErrorMessage(PlayerError.BadDrop, ctx)
                End If
            End If
            _badCmdBefore = "drop"
            Exit Sub
        End If

        ' If object is inside a container, it must be removed before it can be dropped.
        Dim isInContainer = False
        If _gameAslVersion >= 391 Then
            If IsYes(GetObjectProperty("parent", id, True, False)) Then
                isInContainer = True
                Dim parent = GetObjectProperty("parent", id, False, False)
                parentId = GetObjectIdNoAlias(parent)
            End If
        End If

        Dim dropFound = False
        Dim dropStatement As String = ""

        For i = _objs(id).DefinitionSectionStart To _objs(id).DefinitionSectionEnd
            If BeginsWith(_lines(i), "drop ") Then
                dropStatement = GetEverythingAfter(_lines(i), "drop ")
                dropFound = True
                Exit For
            End If
        Next i

        SetStringContents("quest.error.article", _objs(id).Article, ctx)

        If Not dropFound Or BeginsWith(dropStatement, "everywhere") Then
            If isInContainer Then
                ' So, we want to drop an object that's in a container or surface. So first
                ' we have to remove the object from that container.

                Dim parentDisplayName As String

                If _objs(parentId).ObjectAlias <> "" Then
                    parentDisplayName = _objs(parentId).ObjectAlias
                Else
                    parentDisplayName = _objs(parentId).ObjectName
                End If

                Print("(first removing " & _objs(id).Article & " from " & parentDisplayName & ")", ctx)

                ' Try to remove the object
                ctx.AllowRealNamesInCommand = True
                ExecCommand("remove " & _objs(id).ObjectName, ctx, False, , True)

                If GetObjectProperty("parent", id, False, False) <> "" Then
                    ' removing the object failed
                    Exit Sub
                End If
            End If
        End If

        If Not dropFound Then
            PlayerErrorMessage(PlayerError.DefaultDrop, ctx)
            PlayerItem(_objs(id).ObjectName, False, ctx)
        Else
            If BeginsWith(dropStatement, "everywhere") Then
                PlayerItem(_objs(id).ObjectName, False, ctx)
                If InStr(dropStatement, "<") <> 0 Then
                    Print(GetParameter(s:=dropStatement, ctx:=ctx), ctx)
                Else
                    PlayerErrorMessage(PlayerError.DefaultDrop, ctx)
                End If
            ElseIf BeginsWith(dropStatement, "nowhere") Then
                If InStr(dropStatement, "<") <> 0 Then
                    Print(GetParameter(s:=dropStatement, ctx:=ctx), ctx)
                Else
                    PlayerErrorMessage(PlayerError.CantDrop, ctx)
                End If
            Else
                ExecuteScript(dropStatement, ctx)
            End If
        End If
    End Sub

    Private Sub ExecExamine(command As String, ctx As Context)
        Dim item = LCase(Trim(GetEverythingAfter(command, "examine ")))

        If item = "" Then
            PlayerErrorMessage(PlayerError.BadExamine, ctx)
            _badCmdBefore = "examine"
            Exit Sub
        End If

        Dim id = Disambiguate(item, _currentRoom & ";inventory", ctx)

        If id <= 0 Then
            If id <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
            _badCmdBefore = "examine"
            Exit Sub
        End If

        Dim o = _objs(id)

        ' Find "examine" action:
        For i = 1 To o.NumberActions
            If o.Actions(i).ActionName = "examine" Then
                ExecuteScript(o.Actions(i).Script, ctx, id)
                Exit Sub
            End If
        Next i

        ' Find "examine" property:
        For i = 1 To o.NumberProperties
            If o.Properties(i).PropertyName = "examine" Then
                Print(o.Properties(i).PropertyValue, ctx)
                Exit Sub
            End If
        Next i

        ' Find "examine" tag:
        For i = o.DefinitionSectionStart + 1 To _objs(id).DefinitionSectionEnd - 1
            If BeginsWith(_lines(i), "examine ") Then
                Dim action = Trim(GetEverythingAfter(_lines(i), "examine "))
                If Left(action, 1) = "<" Then
                    Print(GetParameter(_lines(i), ctx), ctx)
                Else
                    ExecuteScript(action, ctx, id)
                End If
                Exit Sub
            End If
        Next i

        DoLook(id, ctx, True)
    End Sub

    Private Sub ExecMoveThing(data As String, type As Thing, ctx As Context)
        Dim scp = InStr(data, ";")
        Dim name = Trim(Left(data, scp - 1))
        Dim place = Trim(Mid(data, scp + 1))
        MoveThing(name, place, type, ctx)
    End Sub

    Private Sub ExecProperty(data As String, ctx As Context)
        Dim id As Integer
        Dim found As Boolean
        Dim scp = InStr(data, ";")

        If scp = 0 Then
            LogASLError("No property data given in 'property <" & data & ">'", LogType.WarningError)
            Exit Sub
        End If

        Dim name = Trim(Left(data, scp - 1))
        Dim properties = Trim(Mid(data, scp + 1))

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(name) Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object in 'property <" & data & ">'", LogType.WarningError)
            Exit Sub
        End If

        AddToObjectProperties(properties, id, ctx)
    End Sub

    Private Sub ExecuteDo(procedureName As String, ctx As Context)
        Dim newCtx As Context = CopyContext(ctx)
        Dim numParameters = 0
        Dim useNewCtx As Boolean

        If _gameAslVersion >= 392 And Left(procedureName, 8) = "!intproc" Then
            ' If "do" procedure is run in a new context, context info is not passed to any nested
            ' script blocks in braces.

            useNewCtx = False
        Else
            useNewCtx = True
        End If

        If _gameAslVersion >= 284 Then
            Dim obp = InStr(procedureName, "(")
            Dim cbp As Integer
            If obp <> 0 Then
                cbp = InStr(obp + 1, procedureName, ")")
            End If

            If obp <> 0 And cbp <> 0 Then
                Dim parameters = Mid(procedureName, obp + 1, (cbp - obp) - 1)
                procedureName = Left(procedureName, obp - 1)

                parameters = parameters & ";"
                Dim pos = 1
                Do
                    numParameters = numParameters + 1
                    Dim scp = InStr(pos, parameters, ";")

                    newCtx.NumParameters = numParameters
                    ReDim Preserve newCtx.Parameters(numParameters)
                    newCtx.Parameters(numParameters) = Trim(Mid(parameters, pos, scp - pos))

                    pos = scp + 1
                Loop Until pos >= Len(parameters)
            End If
        End If

        Dim block = DefineBlockParam("procedure", procedureName)
        If block.StartLine = 0 And block.EndLine = 0 Then
            LogASLError("No such procedure " & procedureName, LogType.WarningError)
        Else
            For i = block.StartLine + 1 To block.EndLine - 1
                If Not useNewCtx Then
                    ExecuteScript(_lines(i), ctx)
                Else
                    ExecuteScript(_lines(i), newCtx)
                    ctx.DontProcessCommand = newCtx.DontProcessCommand
                End If
            Next i
        End If
    End Sub

    Private Sub ExecuteDoAction(data As String, ctx As Context)
        Dim id As Integer

        Dim scp = InStr(data, ";")
        If scp = 0 Then
            LogASLError("No action name specified in 'doaction <" & data & ">'")
            Exit Sub
        End If

        Dim objName = LCase(Trim(Left(data, scp - 1)))
        Dim actionName = Trim(Mid(data, scp + 1))
        Dim found = False

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = objName Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object '" & objName & "'")
            Exit Sub
        End If

        DoAction(id, actionName, ctx)
    End Sub

    Private Function ExecuteIfHere(obj As String, ctx As Context) As Boolean
        If _gameAslVersion <= 281 Then
            For i = 1 To _numberChars
                If _chars(i).ContainerRoom = _currentRoom And _chars(i).Exists Then
                    If LCase(obj) = LCase(_chars(i).ObjectName) Then
                        Return True
                    End If
                End If
            Next i
        End If

        For i = 1 To _numberObjs
            If LCase(_objs(i).ContainerRoom) = LCase(_currentRoom) And _objs(i).Exists Then
                If LCase(obj) = LCase(_objs(i).ObjectName) Then
                    Return True
                End If
            End If
        Next i

        Return False
    End Function

    Private Function ExecuteIfExists(obj As String, realOnly As Boolean) As Boolean
        Dim result As Boolean
        Dim errorReport = False
        Dim scp As Integer

        If InStr(obj, ";") <> 0 Then
            scp = InStr(obj, ";")
            If LCase(Trim(Mid(obj, scp + 1))) = "report" Then
                errorReport = True
            End If

            obj = Left(obj, scp - 1)
        End If

        Dim found = False

        If _gameAslVersion < 281 Then
            For i = 1 To _numberChars
                If LCase(obj) = LCase(_chars(i).ObjectName) Then
                    result = _chars(i).Exists
                    found = True
                    Exit For
                End If
            Next i
        End If

        If Not found Then
            For i = 1 To _numberObjs
                If LCase(obj) = LCase(_objs(i).ObjectName) Then
                    result = _objs(i).Exists
                    found = True
                    Exit For
                End If
            Next i
        End If

        If found = False And errorReport Then
            LogASLError("No such character/object '" & obj & "'.", LogType.UserError)
        End If

        If found = False Then result = False

        If realOnly Then
            Return found
        End If

        Return result
    End Function

    Private Function ExecuteIfProperty(data As String) As Boolean
        Dim id As Integer
        Dim scp = InStr(data, ";")

        If scp = 0 Then
            LogASLError("No property name given in condition 'property <" & data & ">' ...", LogType.WarningError)
            Return False
        End If

        Dim objName = Trim(Left(data, scp - 1))
        Dim propertyName = Trim(Mid(data, scp + 1))
        Dim found = False

        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(objName) Then
                found = True
                id = i
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such object '" & objName & "' in condition 'property <" & data & ">' ...", LogType.WarningError)
            Return False
        End If

        Return GetObjectProperty(propertyName, id, True) = "yes"
    End Function

    Private Sub ExecuteRepeat(data As String, ctx As Context)
        Dim repeatWhileTrue As Boolean
        Dim repeatScript As String = ""
        Dim bracketPos As Integer
        Dim afterBracket As String
        Dim foundScript = False

        If BeginsWith(data, "while ") Then
            repeatWhileTrue = True
            data = GetEverythingAfter(data, "while ")
        ElseIf BeginsWith(data, "until ") Then
            repeatWhileTrue = False
            data = GetEverythingAfter(data, "until ")
        Else
            LogASLError("Expected 'until' or 'while' in 'repeat " & ReportErrorLine(data) & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim pos = 1
        Do
            bracketPos = InStr(pos, data, ">")
            afterBracket = Trim(Mid(data, bracketPos + 1))
            If (Not BeginsWith(afterBracket, "and ")) And (Not BeginsWith(afterBracket, "or ")) Then
                repeatScript = afterBracket
                foundScript = True
            Else
                pos = bracketPos + 1
            End If
        Loop Until foundScript Or afterBracket = ""

        Dim conditions = Trim(Left(data, bracketPos))
        Dim finished = False

        Do
            If ExecuteConditions(conditions, ctx) = repeatWhileTrue Then
                ExecuteScript(repeatScript, ctx)
            Else
                finished = True
            End If
        Loop Until finished Or _gameFinished
    End Sub

    Private Sub ExecuteSetCollectable(param As String, ctx As Context)
        Dim val As Double
        Dim id As Integer
        Dim scp = InStr(param, ";")
        Dim name = Trim(Left(param, scp - 1))
        Dim newVal = Trim(Right(param, Len(param) - scp))
        Dim found = False

        For i = 1 To _numCollectables
            If _collectables(i).Name = name Then
                id = i
                found = True
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such collectable '" & param & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim op = Left(newVal, 1)
        Dim newValue = Trim(Right(newVal, Len(newVal) - 1))
        If IsNumeric(newValue) Then
            val = Conversion.Val(newValue)
        Else
            val = GetCollectableAmount(newValue)
        End If

        If op = "+" Then
            _collectables(id).Value = _collectables(id).Value + val
        ElseIf op = "-" Then
            _collectables(id).Value = _collectables(id).Value - val
        ElseIf op = "=" Then
            _collectables(id).Value = val
        End If

        CheckCollectable(id)
        UpdateItems(ctx)
    End Sub

    Private Sub ExecuteWait(waitLine As String, ctx As Context)
        If waitLine <> "" Then
            Print(GetParameter(waitLine, ctx), ctx)
        Else
            If _gameAslVersion >= 410 Then
                PlayerErrorMessage(PlayerError.DefaultWait, ctx)
            Else
                Print("|nPress a key to continue...", ctx)
            End If
        End If

        DoWait()
    End Sub

    Private Sub InitFileData(fileData As String)
        _fileData = fileData
        _fileDataPos = 1
    End Sub

    Private Function GetNextChunk() As String
        Dim nullPos = InStr(_fileDataPos, _fileData, Chr(0))
        Dim result = Mid(_fileData, _fileDataPos, nullPos - _fileDataPos)

        If nullPos < Len(_fileData) Then
            _fileDataPos = nullPos + 1
        End If

        Return result
    End Function

    Function GetFileDataChars(count As Integer) As String
        Dim result = Mid(_fileData, _fileDataPos, count)
        _fileDataPos = _fileDataPos + count
        Return result
    End Function

    Private Function GetObjectActions(actionInfo As String) As ActionType
        Dim name = LCase(GetParameter(actionInfo, _nullContext))
        Dim ep = InStr(actionInfo, ">")
        If ep = Len(actionInfo) Then
            LogASLError("No script given for '" & name & "' action data", LogType.WarningError)
            Return New ActionType
        End If

        Dim script = Trim(Mid(actionInfo, ep + 1))
        Dim result As ActionType = New ActionType
        result.ActionName = name
        result.Script = script
        Return result
    End Function

    Private Function GetObjectId(name As String, ctx As Context, Optional room As String = "") As Integer
        Dim id As Integer
        Dim found = False

        If BeginsWith(name, "the ") Then
            name = GetEverythingAfter(name, "the ")
        End If

        For i = 1 To _numberObjs
            If (LCase(_objs(i).ObjectName) = LCase(name) Or LCase(_objs(i).ObjectName) = "the " & LCase(name)) And (LCase(_objs(i).ContainerRoom) = LCase(room) Or room = "") And _objs(i).Exists = True Then
                id = i
                found = True
                Exit For
            End If
        Next i

        If Not found And _gameAslVersion >= 280 Then
            id = Disambiguate(name, room, ctx)
            If id > 0 Then found = True
        End If

        If found Then
            Return id
        End If

        Return -1
    End Function

    Private Function GetObjectIdNoAlias(name As String) As Integer
        For i = 1 To _numberObjs
            If LCase(_objs(i).ObjectName) = LCase(name) Then
                Return i
            End If
        Next i

        Return 0
    End Function

    Friend Function GetObjectProperty(name As String, id As Integer, Optional existsOnly As Boolean = False, Optional logError As Boolean = True) As String
        Dim result As String = ""
        Dim found = False
        Dim o = _objs(id)

        For i = 1 To o.NumberProperties
            If LCase(o.Properties(i).PropertyName) = LCase(name) Then
                found = True
                result = o.Properties(i).PropertyValue
                Exit For
            End If
        Next i

        If existsOnly Then
            If found Then
                Return "yes"
            End If
            Return "no"
        End If

        If found Then
            Return result
        End If

        If logError Then
            LogASLError("Object '" & _objs(id).ObjectName & "' has no property '" & name & "'", LogType.WarningError)
            Return "!"
        End If

        Return ""
    End Function

    Private Function GetPropertiesInType(type As String, Optional err As Boolean = True) As PropertiesActions
        Dim blockId As Integer
        Dim propertyList As New PropertiesActions
        Dim found = False

        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define type") Then
                If LCase(GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext)) = LCase(type) Then
                    blockId = i
                    found = True
                    Exit For
                End If
            End If
        Next i

        If Not found Then
            If err Then
                LogASLError("No such type '" & type & "'", LogType.WarningError)
            End If
            Return New PropertiesActions
        End If

        For i = _defineBlocks(blockId).StartLine + 1 To _defineBlocks(blockId).EndLine - 1
            If BeginsWith(_lines(i), "type ") Then
                Dim typeName = LCase(GetParameter(_lines(i), _nullContext))
                Dim newProperties = GetPropertiesInType(typeName)
                propertyList.Properties = propertyList.Properties & newProperties.Properties
                ReDim Preserve propertyList.Actions(propertyList.NumberActions + newProperties.NumberActions)
                For j = propertyList.NumberActions + 1 To propertyList.NumberActions + newProperties.NumberActions
                    propertyList.Actions(j) = New ActionType
                    propertyList.Actions(j).ActionName = newProperties.Actions(j - propertyList.NumberActions).ActionName
                    propertyList.Actions(j).Script = newProperties.Actions(j - propertyList.NumberActions).Script
                Next j
                propertyList.NumberActions = propertyList.NumberActions + newProperties.NumberActions

                ' Add this type name to the TypesIncluded list...
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + 1
                ReDim Preserve propertyList.TypesIncluded(propertyList.NumberTypesIncluded)
                propertyList.TypesIncluded(propertyList.NumberTypesIncluded) = typeName

                ' and add the names of the types included by it...

                ReDim Preserve propertyList.TypesIncluded(propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded)
                For j = propertyList.NumberTypesIncluded + 1 To propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded
                    propertyList.TypesIncluded(j) = newProperties.TypesIncluded(j - propertyList.NumberTypesIncluded)
                Next j
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded
            ElseIf BeginsWith(_lines(i), "action ") Then
                propertyList.NumberActions = propertyList.NumberActions + 1
                ReDim Preserve propertyList.Actions(propertyList.NumberActions)
                propertyList.Actions(propertyList.NumberActions) = GetObjectActions(GetEverythingAfter(_lines(i), "action "))
            ElseIf BeginsWith(_lines(i), "properties ") Then
                propertyList.Properties = propertyList.Properties & GetParameter(_lines(i), _nullContext) & ";"
            ElseIf Trim(_lines(i)) <> "" Then
                propertyList.Properties = propertyList.Properties & _lines(i) & ";"
            End If
        Next i

        Return propertyList
    End Function

    Friend Function GetRoomID(name As String, ctx As Context) As Integer
        If InStr(name, "[") > 0 Then
            Dim idx = GetArrayIndex(name, ctx)
            name = name & Trim(Str(idx))
        End If

        For i = 1 To _numberRooms
            If LCase(_rooms(i).RoomName) = LCase(name) Then
                Return i
            End If
        Next i

        Return 0
    End Function

    Private Function GetTextOrScript(textScript As String) As TextAction
        Dim result = New TextAction
        textScript = Trim(textScript)

        If Left(textScript, 1) = "<" Then
            result.Type = TextActionType.Text
            result.Data = GetParameter(textScript, _nullContext)
        Else
            result.Type = TextActionType.Script
            result.Data = textScript
        End If

        Return result
    End Function

    Private Function GetThingNumber(name As String, room As String, type As Thing) As Integer
        ' Returns the number in the Chars() or _objs() array of the specified char/obj

        If type = Thing.Character Then
            For i = 1 To _numberChars
                If (room <> "" And LCase(_chars(i).ObjectName) = LCase(name) And LCase(_chars(i).ContainerRoom) = LCase(room)) Or (room = "" And LCase(_chars(i).ObjectName) = LCase(name)) Then
                    Return i
                End If
            Next i
        ElseIf type = Thing.Object Then
            For i = 1 To _numberObjs
                If (room <> "" And LCase(_objs(i).ObjectName) = LCase(name) And LCase(_objs(i).ContainerRoom) = LCase(room)) Or (room = "" And LCase(_objs(i).ObjectName) = LCase(name)) Then
                    Return i
                End If
            Next i
        End If

        Return -1
    End Function

    Private Function GetThingBlock(name As String, room As String, type As Thing) As DefineBlock
        ' Returns position where specified char/obj is defined in ASL code

        Dim result = New DefineBlock

        If type = Thing.Character Then
            For i = 1 To _numberChars
                If LCase(_chars(i).ObjectName) = LCase(name) And LCase(_chars(i).ContainerRoom) = LCase(room) Then
                    result.StartLine = _chars(i).DefinitionSectionStart
                    result.EndLine = _chars(i).DefinitionSectionEnd
                    Return result
                End If
            Next i
        ElseIf type = Thing.Object Then
            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(name) And LCase(_objs(i).ContainerRoom) = LCase(room) Then
                    result.StartLine = _objs(i).DefinitionSectionStart
                    result.EndLine = _objs(i).DefinitionSectionEnd
                    Return result
                End If
            Next i
        End If

        result.StartLine = 0
        result.EndLine = 0
        Return result
    End Function

    Private Function MakeRestoreData() As String
        Dim data As New System.Text.StringBuilder
        Dim objectData(0) As ChangeType
        Dim roomData(0) As ChangeType
        Dim numObjectData As Integer
        Dim numRoomData As Integer

        ' <<< FILE HEADER DATA >>>

        data.Append("QUEST300" & Chr(0) & GetOriginalFilenameForQSG() & Chr(0))

        ' The start point for encrypted data is after the filename
        Dim start = data.Length + 1

        data.Append(_currentRoom & Chr(0))

        ' Organise Change Log

        For i = 1 To _gameChangeData.NumberChanges
            If BeginsWith(_gameChangeData.ChangeData(i).AppliesTo, "object ") Then
                numObjectData = numObjectData + 1
                ReDim Preserve objectData(numObjectData)
                objectData(numObjectData) = New ChangeType
                objectData(numObjectData) = _gameChangeData.ChangeData(i)
            ElseIf BeginsWith(_gameChangeData.ChangeData(i).AppliesTo, "room ") Then
                numRoomData = numRoomData + 1
                ReDim Preserve roomData(numRoomData)
                roomData(numRoomData) = New ChangeType
                roomData(numRoomData) = _gameChangeData.ChangeData(i)
            End If
        Next i

        ' <<< OBJECT CREATE/CHANGE DATA >>>

        data.Append(Trim(Str(numObjectData + _changeLogObjects.Changes.Count)) & Chr(0))

        For i = 1 To numObjectData
            data.Append(GetEverythingAfter(objectData(i).AppliesTo, "object ") & Chr(0) & objectData(i).Change & Chr(0))
        Next i

        For Each key As String In _changeLogObjects.Changes.Keys
            Dim appliesTo = Split(key, "#")(0)
            Dim changeData = _changeLogObjects.Changes.Item(key)

            data.Append(appliesTo & Chr(0) & changeData & Chr(0))
        Next

        ' <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>

        data.Append(Trim(Str(_numberObjs)) & Chr(0))

        For i = 1 To _numberObjs
            Dim exists As String
            Dim visible As String

            If _objs(i).Exists Then
                exists = Chr(1)
            Else
                exists = Chr(0)
            End If

            If _objs(i).Visible Then
                visible = Chr(1)
            Else
                visible = Chr(0)
            End If

            data.Append(_objs(i).ObjectName & Chr(0) & exists & visible & _objs(i).ContainerRoom & Chr(0))
        Next i

        ' <<< ROOM CREATE/CHANGE DATA >>>

        data.Append(Trim(Str(numRoomData)) & Chr(0))

        For i = 1 To numRoomData
            data.Append(GetEverythingAfter(roomData(i).AppliesTo, "room ") & Chr(0) & roomData(i).Change & Chr(0))
        Next i

        ' <<< TIMER STATE DATA >>>

        data.Append(Trim(Str(_numberTimers)) & Chr(0))

        For i = 1 To _numberTimers
            Dim t = _timers(i)
            data.Append(t.TimerName & Chr(0))

            If t.TimerActive Then
                data.Append(Chr(1))
            Else
                data.Append(Chr(0))
            End If

            data.Append(Trim(Str(t.TimerInterval)) & Chr(0))
            data.Append(Trim(Str(t.TimerTicks)) & Chr(0))
        Next i

        ' <<< STRING VARIABLE DATA >>>

        data.Append(Trim(Str(_numberStringVariables)) & Chr(0))

        For i = 1 To _numberStringVariables
            Dim s = _stringVariable(i)
            data.Append(s.VariableName & Chr(0) & Trim(Str(s.VariableUBound)) & Chr(0))

            For j = 0 To s.VariableUBound
                data.Append(s.VariableContents(j) & Chr(0))
            Next j
        Next i

        ' <<< NUMERIC VARIABLE DATA >>>

        data.Append(Trim(Str(_numberNumericVariables)) & Chr(0))

        For i = 1 To _numberNumericVariables
            Dim n = _numericVariable(i)
            data.Append(n.VariableName & Chr(0) & Trim(Str(n.VariableUBound)) & Chr(0))

            For j = 0 To n.VariableUBound
                data.Append(n.VariableContents(j) & Chr(0))
            Next j
        Next i

        ' Now encrypt data
        Dim dataString As String
        Dim newFileData As New System.Text.StringBuilder

        dataString = data.ToString()

        newFileData.Append(Left(dataString, start - 1))

        For i = start To Len(dataString)
            newFileData.Append(Chr(255 - Asc(Mid(dataString, i, 1))))
        Next i

        Return newFileData.ToString()
    End Function

    Private Sub MoveThing(name As String, room As String, type As Thing, ctx As Context)
        Dim oldRoom As String = ""

        Dim id = GetThingNumber(name, "", type)

        If InStr(room, "[") > 0 Then
            Dim idx = GetArrayIndex(room, ctx)
            room = room & Trim(Str(idx))
        End If

        If type = Thing.Character Then
            _chars(id).ContainerRoom = room
        ElseIf type = Thing.Object Then
            oldRoom = _objs(id).ContainerRoom
            _objs(id).ContainerRoom = room
        End If

        If _gameAslVersion >= 391 Then
            ' If this object contains other objects, move them too
            For i = 1 To _numberObjs
                If LCase(GetObjectProperty("parent", i, False, False)) = LCase(name) Then
                    MoveThing(_objs(i).ObjectName, room, type, ctx)
                End If
            Next i
        End If

        UpdateObjectList(ctx)

        If BeginsWith(LCase(room), "inventory") Or BeginsWith(LCase(oldRoom), "inventory") Then
            UpdateItems(ctx)
        End If
    End Sub

    Public Sub Pause(duration As Integer)
        _player.DoPause(duration)
        ChangeState(State.Waiting)

        SyncLock _waitLock
            System.Threading.Monitor.Wait(_waitLock)
        End SyncLock
    End Sub

    Private Function ConvertParameter(parameter As String, convertChar As String, action As ConvertType, ctx As Context) As String
        ' Returns a string with functions, string and
        ' numeric variables executed or converted as
        ' appropriate, read for display/etc.

        Dim result As String = ""
        Dim pos = 1
        Dim finished = False

        Do
            Dim varPos = InStr(pos, parameter, convertChar)
            If varPos = 0 Then
                varPos = Len(parameter) + 1
                finished = True
            End If

            Dim currentBit = Mid(parameter, pos, varPos - pos)
            result = result & currentBit

            If finished = False Then
                Dim nextPos = InStr(varPos + 1, parameter, convertChar)

                If nextPos = 0 Then
                    LogASLError("Line parameter <" & parameter & "> has missing " & convertChar, LogType.WarningError)
                    Return "<ERROR>"
                End If

                Dim varName = Mid(parameter, varPos + 1, (nextPos - 1) - varPos)

                If varName = "" Then
                    result = result & convertChar
                Else

                    If action = ConvertType.Strings Then
                        result = result & GetStringContents(varName, ctx)
                    ElseIf action = ConvertType.Functions Then
                        varName = EvaluateInlineExpressions(varName)
                        result = result & DoFunction(varName, ctx)
                    ElseIf action = ConvertType.Numeric Then
                        result = result & Trim(Str(GetNumericContents(varName, ctx)))
                    ElseIf action = ConvertType.Collectables Then
                        result = result & Trim(Str(GetCollectableAmount(varName)))
                    End If
                End If

                pos = nextPos + 1
            End If
        Loop Until finished

        Return result
    End Function

    Private Function DoFunction(data As String, ctx As Context) As String
        Dim name, parameter As String
        Dim intFuncResult As String = ""
        Dim intFuncExecuted = False
        Dim paramPos = InStr(data, "(")

        If paramPos <> 0 Then
            name = Trim(Left(data, paramPos - 1))
            Dim endParamPos = InStrRev(data, ")")
            If endParamPos = 0 Then
                LogASLError("Expected ) in $" & data & "$", LogType.WarningError)
                Return ""
            End If
            parameter = Mid(data, paramPos + 1, (endParamPos - paramPos) - 1)
        Else
            name = data
            parameter = ""
        End If

        Dim block As DefineBlock
        block = DefineBlockParam("function", name)

        If block.StartLine = 0 And block.EndLine = 0 Then
            'Function does not exist; try an internal function.
            intFuncResult = DoInternalFunction(name, parameter, ctx)
            If intFuncResult = "__NOTDEFINED" Then
                LogASLError("No such function '" & name & "'", LogType.WarningError)
                Return "[ERROR]"
            Else
                intFuncExecuted = True
            End If
        End If

        If intFuncExecuted Then
            Return intFuncResult
        Else
            Dim newCtx As Context = CopyContext(ctx)
            Dim numParameters = 0
            Dim pos = 1

            If parameter <> "" Then
                parameter = parameter & ";"
                Do
                    numParameters = numParameters + 1
                    Dim scp = InStr(pos, parameter, ";")

                    Dim parameterData = Trim(Mid(parameter, pos, scp - pos))
                    SetStringContents("quest.function.parameter." & Trim(Str(numParameters)), parameterData, ctx)

                    newCtx.NumParameters = numParameters
                    ReDim Preserve newCtx.Parameters(numParameters)
                    newCtx.Parameters(numParameters) = parameterData

                    pos = scp + 1
                Loop Until pos >= Len(parameter)
                SetStringContents("quest.function.numparameters", Trim(Str(numParameters)), ctx)
            Else
                SetStringContents("quest.function.numparameters", "0", ctx)
                newCtx.NumParameters = 0
            End If

            Dim result As String = ""

            For i = block.StartLine + 1 To block.EndLine - 1
                ExecuteScript(_lines(i), newCtx)
                result = newCtx.FunctionReturnValue
            Next i

            Return result
        End If
    End Function

    Private Function DoInternalFunction(name As String, parameter As String, ctx As Context) As String
        Dim parameters() As String
        Dim untrimmedParameters() As String
        Dim objId As Integer
        Dim numParameters = 0
        Dim pos = 1

        If parameter <> "" Then
            parameter = parameter & ";"
            Do
                numParameters = numParameters + 1
                Dim scp = InStr(pos, parameter, ";")
                ReDim Preserve parameters(numParameters)
                ReDim Preserve untrimmedParameters(numParameters)

                untrimmedParameters(numParameters) = Mid(parameter, pos, scp - pos)
                parameters(numParameters) = Trim(untrimmedParameters(numParameters))

                pos = scp + 1
            Loop Until pos >= Len(parameter)

            ' Remove final ";"
            parameter = Left(parameter, Len(parameter) - 1)
        Else
            numParameters = 1
            ReDim parameters(1)
            ReDim untrimmedParameters(1)
            parameters(1) = ""
            untrimmedParameters(1) = ""
        End If

        Dim param2 As String
        Dim param3 As String

        If name = "displayname" Then
            objId = GetObjectId(parameters(1), ctx)
            If objId = -1 Then
                LogASLError("Object '" & parameters(1) & "' does not exist", LogType.WarningError)
                Return "!"
            Else
                Return _objs(GetObjectId(parameters(1), ctx)).ObjectAlias
            End If
        ElseIf name = "numberparameters" Then
            Return Trim(Str(ctx.NumParameters))
        ElseIf name = "parameter" Then
            If numParameters = 0 Then
                LogASLError("No parameter number specified for $parameter$ function", LogType.WarningError)
                Return ""
            Else
                If Val(parameters(1)) > ctx.NumParameters Then
                    LogASLError("No parameter number " & parameters(1) & " sent to this function", LogType.WarningError)
                    Return ""
                Else
                    Return Trim(ctx.Parameters(CInt(parameters(1))))
                End If
            End If
        ElseIf name = "gettag" Then
            ' Deprecated
            Return FindStatement(DefineBlockParam("room", parameters(1)), parameters(2))
        ElseIf name = "objectname" Then
            Return _objs(ctx.CallingObjectId).ObjectName
        ElseIf name = "locationof" Then
            For i = 1 To _numberChars
                If LCase(_chars(i).ObjectName) = LCase(parameters(1)) Then
                    Return _chars(i).ContainerRoom
                End If
            Next i

            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(parameters(1)) Then
                    Return _objs(i).ContainerRoom
                End If
            Next i
        ElseIf name = "lengthof" Then
            Return Str(Len(untrimmedParameters(1)))
        ElseIf name = "left" Then
            If Val(parameters(2)) < 0 Then
                LogASLError("Invalid function call in '$Left$(" & parameters(1) & "; " & parameters(2) & ")$'", LogType.WarningError)
                Return "!"
            Else
                Return Left(parameters(1), CInt(parameters(2)))
            End If
        ElseIf name = "right" Then
            If Val(parameters(2)) < 0 Then
                LogASLError("Invalid function call in '$Right$(" & parameters(1) & "; " & parameters(2) & ")$'", LogType.WarningError)
                Return "!"
            Else
                Return Right(parameters(1), CInt(parameters(2)))
            End If
        ElseIf name = "mid" Then
            If numParameters = 3 Then
                If Val(parameters(2)) < 0 Then
                    LogASLError("Invalid function call in '$Mid$(" & parameters(1) & "; " & parameters(2) & "; " & parameters(3) & ")$'", LogType.WarningError)
                    Return "!"
                Else
                    Return Mid(parameters(1), CInt(parameters(2)), CInt(parameters(3)))
                End If
            ElseIf numParameters = 2 Then
                If Val(parameters(2)) < 0 Then
                    LogASLError("Invalid function call in '$Mid$(" & parameters(1) & "; " & parameters(2) & ")$'", LogType.WarningError)
                    Return "!"
                Else
                    Return Mid(parameters(1), CInt(parameters(2)))
                End If
            End If
            LogASLError("Invalid function call to '$Mid$(...)$'", LogType.WarningError)
            Return ""
        ElseIf name = "rand" Then
            Return Str(Int(_random.NextDouble() * (CDbl(parameters(2)) - CDbl(parameters(1)) + 1)) + CDbl(parameters(1)))
        ElseIf name = "instr" Then
            If numParameters = 3 Then
                param3 = ""
                If InStr(parameters(3), "_") <> 0 Then
                    For i = 1 To Len(parameters(3))
                        If Mid(parameters(3), i, 1) = "_" Then
                            param3 = param3 & " "
                        Else
                            param3 = param3 & Mid(parameters(3), i, 1)
                        End If
                    Next i
                Else
                    param3 = parameters(3)
                End If
                If Val(parameters(1)) <= 0 Then
                    LogASLError("Invalid function call in '$instr(" & parameters(1) & "; " & parameters(2) & "; " & parameters(3) & ")$'", LogType.WarningError)
                    Return "!"
                Else
                    Return Trim(Str(InStr(CInt(parameters(1)), parameters(2), param3)))
                End If
            ElseIf numParameters = 2 Then
                param2 = ""
                If InStr(parameters(2), "_") <> 0 Then
                    For i = 1 To Len(parameters(2))
                        If Mid(parameters(2), i, 1) = "_" Then
                            param2 = param2 & " "
                        Else
                            param2 = param2 & Mid(parameters(2), i, 1)
                        End If
                    Next i
                Else
                    param2 = parameters(2)
                End If
                Return Trim(Str(InStr(parameters(1), param2)))
            End If
            LogASLError("Invalid function call to '$Instr$(...)$'", LogType.WarningError)
            Return ""
        ElseIf name = "ucase" Then
            Return UCase(parameters(1))
        ElseIf name = "lcase" Then
            Return LCase(parameters(1))
        ElseIf name = "capfirst" Then
            Return UCase(Left(parameters(1), 1)) & Mid(parameters(1), 2)
        ElseIf name = "symbol" Then
            If parameters(1) = "lt" Then
                Return "<"
            ElseIf parameters(1) = "gt" Then
                Return ">"
            Else
                Return "!"
            End If
        ElseIf name = "loadmethod" Then
            Return _gameLoadMethod
        ElseIf name = "timerstate" Then
            For i = 1 To _numberTimers
                If LCase(_timers(i).TimerName) = LCase(parameters(1)) Then
                    If _timers(i).TimerActive Then
                        Return "1"
                    Else
                        Return "0"
                    End If
                End If
            Next i
            LogASLError("No such timer '" & parameters(1) & "'", LogType.WarningError)
            Return "!"
        ElseIf name = "timerinterval" Then
            For i = 1 To _numberTimers
                If LCase(_timers(i).TimerName) = LCase(parameters(1)) Then
                    Return Str(_timers(i).TimerInterval)
                End If
            Next i
            LogASLError("No such timer '" & parameters(1) & "'", LogType.WarningError)
            Return "!"
        ElseIf name = "ubound" Then
            For i = 1 To _numberNumericVariables
                If LCase(_numericVariable(i).VariableName) = LCase(parameters(1)) Then
                    Return Trim(Str(_numericVariable(i).VariableUBound))
                End If
            Next i

            For i = 1 To _numberStringVariables
                If LCase(_stringVariable(i).VariableName) = LCase(parameters(1)) Then
                    Return Trim(Str(_stringVariable(i).VariableUBound))
                End If
            Next i

            LogASLError("No such variable '" & parameters(1) & "'", LogType.WarningError)
            Return "!"
        ElseIf name = "objectproperty" Then
            Dim FoundObj = False
            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(parameters(1)) Then
                    FoundObj = True
                    objId = i
                    i = _numberObjs
                End If
            Next i

            If Not FoundObj Then
                LogASLError("No such object '" & parameters(1) & "'", LogType.WarningError)
                Return "!"
            Else
                Return GetObjectProperty(parameters(2), objId)
            End If
        ElseIf name = "getobjectname" Then
            If numParameters = 3 Then
                objId = Disambiguate(parameters(1), parameters(2) & ";" & parameters(3), ctx)
            ElseIf numParameters = 2 Then
                objId = Disambiguate(parameters(1), parameters(2), ctx)
            Else

                objId = Disambiguate(parameters(1), _currentRoom & ";inventory", ctx)
            End If

            If objId <= -1 Then
                LogASLError("No object found with display name '" & parameters(1) & "'", LogType.WarningError)
                Return "!"
            Else
                Return _objs(objId).ObjectName
            End If
        ElseIf name = "thisobject" Then
            Return _objs(ctx.CallingObjectId).ObjectName
        ElseIf name = "thisobjectname" Then
            Return _objs(ctx.CallingObjectId).ObjectAlias
        ElseIf name = "speechenabled" Then
            Return "1"
        ElseIf name = "removeformatting" Then
            Return RemoveFormatting(parameter)
        ElseIf name = "findexit" And _gameAslVersion >= 410 Then
            Dim e = FindExit(parameter)
            If e Is Nothing Then
                Return ""
            Else
                Return _objs(e.ObjID).ObjectName
            End If
        End If

        Return "__NOTDEFINED"
    End Function

    Private Sub ExecFor(line As String, ctx As Context)
        ' See if this is a "for each" loop:
        If BeginsWith(line, "each ") Then
            ExecForEach(GetEverythingAfter(line, "each "), ctx)
            Exit Sub
        End If

        ' Executes a for loop, of form:
        '   for <variable; startvalue; endvalue> script
        Dim endValue As Integer
        Dim stepValue As Integer
        Dim forData = GetParameter(line, ctx)

        ' Extract individual components:
        Dim scp1 = InStr(forData, ";")
        Dim scp2 = InStr(scp1 + 1, forData, ";")
        Dim scp3 = InStr(scp2 + 1, forData, ";")
        Dim counterVariable = Trim(Left(forData, scp1 - 1))
        Dim startValue = CInt(Mid(forData, scp1 + 1, (scp2 - 1) - scp1))

        If scp3 <> 0 Then
            endValue = CInt(Mid(forData, scp2 + 1, (scp3 - 1) - scp2))
            stepValue = CInt(Mid(forData, scp3 + 1))
        Else
            endValue = CInt(Mid(forData, scp2 + 1))
            stepValue = 1
        End If

        Dim loopScript = Trim(Mid(line, InStr(line, ">") + 1))

        For i As Double = startValue To endValue Step stepValue
            SetNumericVariableContents(counterVariable, i, ctx)
            ExecuteScript(loopScript, ctx)
            i = GetNumericContents(counterVariable, ctx)
        Next i
    End Sub

    Private Sub ExecSetVar(varInfo As String, ctx As Context)
        ' Sets variable contents from a script parameter.
        ' Eg <var1;7> sets numeric variable var1
        ' to 7

        Dim scp = InStr(varInfo, ";")
        Dim varName = Trim(Left(varInfo, scp - 1))
        Dim varCont = Trim(Mid(varInfo, scp + 1))
        Dim idx = GetArrayIndex(varName, ctx)

        If IsNumeric(varName) Then
            LogASLError("Invalid numeric variable name '" & varName & "' - variable names cannot be numeric", LogType.WarningError)
            Exit Sub
        End If

        Try
            If _gameAslVersion >= 391 Then
                Dim expResult = ExpressionHandler(varCont)
                If expResult.Success = ExpressionSuccess.OK Then
                    varCont = expResult.Result
                Else
                    varCont = "0"
                    LogASLError("Error setting numeric variable <" & varInfo & "> : " & expResult.Message, LogType.WarningError)
                End If
            Else
                Dim obscuredVarInfo = ObscureNumericExps(varCont)
                Dim opPos = InStr(obscuredVarInfo, "+")
                If opPos = 0 Then opPos = InStr(obscuredVarInfo, "*")
                If opPos = 0 Then opPos = InStr(obscuredVarInfo, "/")
                If opPos = 0 Then opPos = InStr(2, obscuredVarInfo, "-")

                If opPos <> 0 Then
                    Dim op = Mid(varCont, opPos, 1)
                    Dim num1 = Val(Left(varCont, opPos - 1))
                    Dim num2 = Val(Mid(varCont, opPos + 1))

                    Select Case op
                        Case "+"
                            varCont = Str(num1 + num2)
                        Case "-"
                            varCont = Str(num1 - num2)
                        Case "*"
                            varCont = Str(num1 * num2)
                        Case "/"
                            If num2 <> 0 Then
                                varCont = Str(num1 / num2)
                            Else
                                LogASLError("Division by zero - The result of this operation has been set to zero.", LogType.WarningError)
                                varCont = "0"
                            End If
                    End Select
                End If
            End If

            SetNumericVariableContents(varName, Val(varCont), ctx, idx)
        Catch
            LogASLError("Error setting variable '" & varName & "' to '" & varCont & "'", LogType.WarningError)
        End Try
    End Sub

    Private Function ExecuteIfAsk(question As String) As Boolean
        _player.ShowQuestion(question)
        ChangeState(State.Waiting)

        SyncLock _waitLock
            System.Threading.Monitor.Wait(_waitLock)
        End SyncLock

        Return _questionResponse
    End Function

    Private Sub SetQuestionResponse(response As Boolean) Implements IASL.SetQuestionResponse
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf SetQuestionResponseInNewThread))
        ChangeState(State.Working)
        runnerThread.Start(response)
        WaitForStateChange(State.Working)
    End Sub

    Private Sub SetQuestionResponseInNewThread(response As Object)
        _questionResponse = DirectCast(response, Boolean)

        SyncLock _waitLock
            System.Threading.Monitor.PulseAll(_waitLock)
        End SyncLock
    End Sub

    Private Function ExecuteIfGot(item As String) As Boolean
        If _gameAslVersion >= 280 Then
            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(item) Then
                    Return _objs(i).ContainerRoom = "inventory" And _objs(i).Exists
                End If
            Next i

            LogASLError("No object '" & item & "' defined.", LogType.WarningError)
            Return False
        End If

        For i = 1 To _numberItems
            If LCase(_items(i).Name) = LCase(item) Then
                Return _items(i).Got
            End If
        Next i

        LogASLError("Item '" & item & "' not defined.", LogType.WarningError)
        Return False
    End Function

    Private Function ExecuteIfHas(condition As String) As Boolean
        Dim checkValue As Double
        Dim colNum As Integer
        Dim scp = InStr(condition, ";")
        Dim name = Trim(Left(condition, scp - 1))
        Dim newVal = Trim(Right(condition, Len(condition) - scp))
        Dim found = False

        For i = 1 To _numCollectables
            If _collectables(i).Name = name Then
                colNum = i
                found = True
                Exit For
            End If
        Next i

        If Not found Then
            LogASLError("No such collectable in " & condition, LogType.WarningError)
            Exit Function
        End If

        Dim op = Left(newVal, 1)
        Dim newValue = Trim(Right(newVal, Len(newVal) - 1))
        If IsNumeric(newValue) Then
            checkValue = Val(newValue)
        Else
            checkValue = GetCollectableAmount(newValue)
        End If

        If op = "+" Then
            Return _collectables(colNum).Value > checkValue
        ElseIf op = "-" Then
            Return _collectables(colNum).Value < checkValue
        ElseIf op = "=" Then
            Return _collectables(colNum).Value = checkValue
        End If

        Return False
    End Function

    Private Function ExecuteIfIs(condition As String) As Boolean
        Dim value1, value2 As String
        Dim op As String
        Dim expectNumerics As Boolean
        Dim expResult As ExpressionResult

        Dim scp = InStr(condition, ";")
        If scp = 0 Then
            LogASLError("Expected second parameter in 'is " & condition & "'", LogType.WarningError)
            Return False
        End If

        Dim scp2 = InStr(scp + 1, condition, ";")
        If scp2 = 0 Then
            ' Only two parameters => standard "="
            op = "="
            value1 = Trim(Left(condition, scp - 1))
            value2 = Trim(Mid(condition, scp + 1))
        Else
            value1 = Trim(Left(condition, scp - 1))
            op = Trim(Mid(condition, scp + 1, (scp2 - scp) - 1))
            value2 = Trim(Mid(condition, scp2 + 1))
        End If

        If _gameAslVersion >= 391 Then
            ' Evaluate expressions in Value1 and Value2
            expResult = ExpressionHandler(value1)

            If expResult.Success = ExpressionSuccess.OK Then
                value1 = expResult.Result
            End If

            expResult = ExpressionHandler(value2)

            If expResult.Success = ExpressionSuccess.OK Then
                value2 = expResult.Result
            End If
        End If

        Dim result = False

        Select Case op
            Case "="
                If LCase(value1) = LCase(value2) Then
                    result = True
                End If
                expectNumerics = False
            Case "!="
                If LCase(value1) <> LCase(value2) Then
                    result = True
                End If
                expectNumerics = False
            Case "gt"
                If Val(value1) > Val(value2) Then
                    result = True
                End If
                expectNumerics = True
            Case "lt"
                If Val(value1) < Val(value2) Then
                    result = True
                End If
                expectNumerics = True
            Case "gt="
                If Val(value1) >= Val(value2) Then
                    result = True
                End If
                expectNumerics = True
            Case "lt="
                If Val(value1) <= Val(value2) Then
                    result = True
                End If
                expectNumerics = True
            Case Else
                LogASLError("Unrecognised comparison condition in 'is " & condition & "'", LogType.WarningError)
        End Select

        If expectNumerics Then
            If Not (IsNumeric(value1) And IsNumeric(value2)) Then
                LogASLError("Expected numeric comparison comparing '" & value1 & "' and '" & value2 & "'", LogType.WarningError)
            End If
        End If

        Return result
    End Function

    Private Function GetNumericContents(name As String, ctx As Context, Optional noError As Boolean = False) As Double
        Dim numNumber As Integer
        Dim arrayIndex As Integer
        Dim exists = False

        ' First, see if the variable already exists. If it
        ' does, get its contents. If not, generate an error.

        If InStr(name, "[") <> 0 And InStr(name, "]") <> 0 Then
            Dim op = InStr(name, "[")
            Dim cp = InStr(name, "]")
            Dim arrayIndexData = Mid(name, op + 1, (cp - op) - 1)
            If IsNumeric(arrayIndexData) Then
                arrayIndex = CInt(arrayIndexData)
            Else
                arrayIndex = CInt(GetNumericContents(arrayIndexData, ctx))
            End If
            name = Left(name, op - 1)
        Else
            arrayIndex = 0
        End If

        If _numberNumericVariables > 0 Then
            For i = 1 To _numberNumericVariables
                If LCase(_numericVariable(i).VariableName) = LCase(name) Then
                    numNumber = i
                    exists = True
                    Exit For
                End If
            Next i
        End If

        If Not exists Then
            If Not noError Then LogASLError("No numeric variable '" & name & "' defined.", LogType.WarningError)
            Return -32767
        End If

        If arrayIndex > _numericVariable(numNumber).VariableUBound Then
            If Not noError Then LogASLError("Array index of '" & name & "[" & Trim(Str(arrayIndex)) & "]' too big.", LogType.WarningError)
            Return -32766
        End If

        ' Now, set the contents
        Return Val(_numericVariable(numNumber).VariableContents(arrayIndex))
    End Function

    Friend Sub PlayerErrorMessage(e As PlayerError, ctx As Context)
        Print(GetErrorMessage(e, ctx), ctx)
    End Sub

    Private Sub PlayerErrorMessage_ExtendInfo(e As PlayerError, ctx As Context, extraInfo As String)
        Dim message = GetErrorMessage(e, ctx)

        If extraInfo <> "" Then
            If Right(message, 1) = "." Then
                message = Left(message, Len(message) - 1)
            End If

            message = message & " - " & extraInfo & "."
        End If

        Print(message, ctx)
    End Sub

    Private Function GetErrorMessage(e As PlayerError, ctx As Context) As String
        Return ConvertParameter(ConvertParameter(ConvertParameter(_playerErrorMessageString(e), "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx)
    End Function

    Private Sub PlayMedia(filename As String)
        PlayMedia(filename, False, False)
    End Sub

    Private Sub PlayMedia(filename As String, sync As Boolean, looped As Boolean)
        If filename.Length = 0 Then
            _player.StopSound()
        Else
            If looped And sync Then sync = False ' Can't loop and sync at the same time, that would just hang!

            _player.PlaySound(filename, sync, looped)

            If sync Then
                ChangeState(State.Waiting)
            End If

            If sync Then
                SyncLock (_waitLock)
                    System.Threading.Monitor.Wait(_waitLock)
                End SyncLock
            End If
        End If
    End Sub

    Private Sub PlayWav(parameter As String)
        Dim sync As Boolean = False
        Dim looped As Boolean = False
        Dim params As New List(Of String)(parameter.Split(";"c))

        params = New List(Of String)(params.Select(Function(p As String) Trim(p)))

        Dim filename = params(0)

        If params.Contains("loop") Then looped = True
        If params.Contains("sync") Then sync = True

        If filename.Length > 0 And InStr(filename, ".") = 0 Then
            filename = filename & ".wav"
        End If

        PlayMedia(filename, sync, looped)
    End Sub

    Private Sub RestoreGameData(fileData As String)
        Dim appliesTo As String
        Dim data As String = ""
        Dim objId, timerNum As Integer
        Dim varUbound As Integer
        Dim found As Boolean
        Dim numStoredData As Integer
        Dim storedData(0) As ChangeType
        Dim decryptedFile As New System.Text.StringBuilder

        ' Decrypt file
        For i = 1 To Len(fileData)
            decryptedFile.Append(Chr(255 - Asc(Mid(fileData, i, 1))))
        Next i

        _fileData = decryptedFile.ToString()
        _currentRoom = GetNextChunk()

        ' OBJECTS

        Dim numData = CInt(GetNextChunk())
        Dim createdObjects As New List(Of String)

        For i = 1 To numData
            appliesTo = GetNextChunk()
            data = GetNextChunk()

            ' As of Quest 4.0, properties and actions are put into StoredData while we load the file,
            ' and then processed later. This is so any created rooms pick up their properties - otherwise
            ' we try to set them before they've been created.

            If BeginsWith(data, "properties ") Or BeginsWith(data, "action ") Then
                numStoredData = numStoredData + 1
                ReDim Preserve storedData(numStoredData)
                storedData(numStoredData) = New ChangeType
                storedData(numStoredData).AppliesTo = appliesTo
                storedData(numStoredData).Change = data
            ElseIf BeginsWith(data, "create ") Then
                Dim createData As String = appliesTo & ";" & GetEverythingAfter(data, "create ")
                ' workaround bug where duplicate "create" entries appear in the restore data
                If Not createdObjects.Contains(createData) Then
                    ExecuteCreate("object <" & createData & ">", _nullContext)
                    createdObjects.Add(createData)
                End If
            Else
                LogASLError("QSG Error: Unrecognised item '" & appliesTo & "; " & data & "'", LogType.InternalError)
            End If
        Next i

        numData = CInt(GetNextChunk())
        For i = 1 To numData
            appliesTo = GetNextChunk()
            data = GetFileDataChars(2)
            objId = GetObjectIdNoAlias(appliesTo)

            If Left(data, 1) = Chr(1) Then
                _objs(objId).Exists = True
            Else
                _objs(objId).Exists = False
            End If

            If Right(data, 1) = Chr(1) Then
                _objs(objId).Visible = True
            Else
                _objs(objId).Visible = False
            End If

            _objs(objId).ContainerRoom = GetNextChunk()
        Next i

        ' ROOMS

        numData = CInt(GetNextChunk())

        For i = 1 To numData
            appliesTo = GetNextChunk()
            data = GetNextChunk()

            If BeginsWith(data, "exit ") Then
                ExecuteCreate(data, _nullContext)
            ElseIf data = "create" Then
                ExecuteCreate("room <" & appliesTo & ">", _nullContext)
            ElseIf BeginsWith(data, "destroy exit ") Then
                DestroyExit(appliesTo & "; " & GetEverythingAfter(data, "destroy exit "), _nullContext)
            End If
        Next i

        ' Now go through and apply object properties and actions

        For i = 1 To numStoredData
            Dim d = storedData(i)
            If BeginsWith(d.Change, "properties ") Then
                AddToObjectProperties(GetEverythingAfter(d.Change, "properties "), GetObjectIdNoAlias(d.AppliesTo), _nullContext)
            ElseIf BeginsWith(d.Change, "action ") Then
                AddToObjectActions(GetEverythingAfter(d.Change, "action "), GetObjectIdNoAlias(d.AppliesTo), _nullContext)
            End If
        Next i

        ' TIMERS

        numData = CInt(GetNextChunk())
        For i = 1 To numData
            found = False
            appliesTo = GetNextChunk()
            For j = 1 To _numberTimers
                If _timers(j).TimerName = appliesTo Then
                    timerNum = j
                    found = True
                    Exit For
                End If
            Next j

            If found Then
                Dim t = _timers(timerNum)
                Dim thisChar As String = GetFileDataChars(1)

                If thisChar = Chr(1) Then
                    t.TimerActive = True
                Else
                    t.TimerActive = False
                End If

                t.TimerInterval = CInt(GetNextChunk())
                t.TimerTicks = CInt(GetNextChunk())
            End If
        Next i

        ' STRING VARIABLES

        ' Set this flag so we don't run any status variable onchange scripts while restoring
        _gameIsRestoring = True

        numData = CInt(GetNextChunk())
        For i = 1 To numData
            appliesTo = GetNextChunk()
            varUbound = CInt(GetNextChunk())

            If varUbound = 0 Then
                data = GetNextChunk()
                SetStringContents(appliesTo, data, _nullContext)
            Else
                For j = 0 To varUbound
                    data = GetNextChunk()
                    SetStringContents(appliesTo, data, _nullContext, j)
                Next j
            End If
        Next i

        ' NUMERIC VARIABLES

        numData = CInt(GetNextChunk())
        For i = 1 To numData
            appliesTo = GetNextChunk()
            varUbound = CInt(GetNextChunk())

            If varUbound = 0 Then
                data = GetNextChunk()
                SetNumericVariableContents(appliesTo, Val(data), _nullContext)
            Else
                For j = 0 To varUbound
                    data = GetNextChunk()
                    SetNumericVariableContents(appliesTo, Val(data), _nullContext, j)
                Next j
            End If
        Next i

        _gameIsRestoring = False
    End Sub

    Private Sub SetBackground(col As String)
        _player.SetBackground("#" + GetHTMLColour(col, "white"))
    End Sub

    Private Sub SetForeground(col As String)
        _player.SetForeground("#" + GetHTMLColour(col, "black"))
    End Sub

    Private Sub SetDefaultPlayerErrorMessages()
        _playerErrorMessageString(PlayerError.BadCommand) = "I don't understand your command. Type HELP for a list of valid commands."
        _playerErrorMessageString(PlayerError.BadGo) = "I don't understand your use of 'GO' - you must either GO in some direction, or GO TO a place."
        _playerErrorMessageString(PlayerError.BadGive) = "You didn't say who you wanted to give that to."
        _playerErrorMessageString(PlayerError.BadCharacter) = "I can't see anybody of that name here."
        _playerErrorMessageString(PlayerError.NoItem) = "You don't have that."
        _playerErrorMessageString(PlayerError.ItemUnwanted) = "#quest.error.gender# doesn't want #quest.error.article#."
        _playerErrorMessageString(PlayerError.BadLook) = "You didn't say what you wanted to look at."
        _playerErrorMessageString(PlayerError.BadThing) = "I can't see that here."
        _playerErrorMessageString(PlayerError.DefaultLook) = "Nothing out of the ordinary."
        _playerErrorMessageString(PlayerError.DefaultSpeak) = "#quest.error.gender# says nothing."
        _playerErrorMessageString(PlayerError.BadItem) = "I can't see that anywhere."
        _playerErrorMessageString(PlayerError.DefaultTake) = "You pick #quest.error.article# up."
        _playerErrorMessageString(PlayerError.BadUse) = "You didn't say what you wanted to use that on."
        _playerErrorMessageString(PlayerError.DefaultUse) = "You can't use that here."
        _playerErrorMessageString(PlayerError.DefaultOut) = "There's nowhere you can go out to around here."
        _playerErrorMessageString(PlayerError.BadPlace) = "You can't go there."
        _playerErrorMessageString(PlayerError.DefaultExamine) = "Nothing out of the ordinary."
        _playerErrorMessageString(PlayerError.BadTake) = "You can't take #quest.error.article#."
        _playerErrorMessageString(PlayerError.CantDrop) = "You can't drop that here."
        _playerErrorMessageString(PlayerError.DefaultDrop) = "You drop #quest.error.article#."
        _playerErrorMessageString(PlayerError.BadDrop) = "You are not carrying such a thing."
        _playerErrorMessageString(PlayerError.BadPronoun) = "I don't know what '#quest.error.pronoun#' you are referring to."
        _playerErrorMessageString(PlayerError.BadExamine) = "You didn't say what you wanted to examine."
        _playerErrorMessageString(PlayerError.AlreadyOpen) = "It is already open."
        _playerErrorMessageString(PlayerError.AlreadyClosed) = "It is already closed."
        _playerErrorMessageString(PlayerError.CantOpen) = "You can't open that."
        _playerErrorMessageString(PlayerError.CantClose) = "You can't close that."
        _playerErrorMessageString(PlayerError.DefaultOpen) = "You open it."
        _playerErrorMessageString(PlayerError.DefaultClose) = "You close it."
        _playerErrorMessageString(PlayerError.BadPut) = "You didn't specify what you wanted to put #quest.error.article# on or in."
        _playerErrorMessageString(PlayerError.CantPut) = "You can't put that there."
        _playerErrorMessageString(PlayerError.DefaultPut) = "Done."
        _playerErrorMessageString(PlayerError.CantRemove) = "You can't remove that."
        _playerErrorMessageString(PlayerError.AlreadyPut) = "It is already there."
        _playerErrorMessageString(PlayerError.DefaultRemove) = "Done."
        _playerErrorMessageString(PlayerError.Locked) = "The exit is locked."
        _playerErrorMessageString(PlayerError.DefaultWait) = "Press a key to continue..."
        _playerErrorMessageString(PlayerError.AlreadyTaken) = "You already have that."
    End Sub

    Private Sub SetFont(name As String)
        If name = "" Then name = _defaultFontName
        _player.SetFont(name)
    End Sub

    Private Sub SetFontSize(size As Double)
        If size = 0 Then size = _defaultFontSize
        _player.SetFontSize(CStr(size))
    End Sub

    Private Sub SetNumericVariableContents(name As String, content As Double, ctx As Context, Optional arrayIndex As Integer = 0)
        Dim numNumber As Integer
        Dim exists = False

        If IsNumeric(name) Then
            LogASLError("Illegal numeric variable name '" & name & "' - check you didn't put % around the variable name in the ASL code", LogType.WarningError)
            Exit Sub
        End If

        ' First, see if variable already exists. If it does,
        ' modify it. If not, create it.

        If _numberNumericVariables > 0 Then
            For i = 1 To _numberNumericVariables
                If LCase(_numericVariable(i).VariableName) = LCase(name) Then
                    numNumber = i
                    exists = True
                    Exit For
                End If
            Next i
        End If

        If exists = False Then
            _numberNumericVariables = _numberNumericVariables + 1
            numNumber = _numberNumericVariables
            ReDim Preserve _numericVariable(numNumber)
            _numericVariable(numNumber) = New VariableType
            _numericVariable(numNumber).VariableUBound = arrayIndex
        End If

        If arrayIndex > _numericVariable(numNumber).VariableUBound Then
            ReDim Preserve _numericVariable(numNumber).VariableContents(arrayIndex)
            _numericVariable(numNumber).VariableUBound = arrayIndex
        End If

        ' Now, set the contents
        _numericVariable(numNumber).VariableName = name
        ReDim Preserve _numericVariable(numNumber).VariableContents(_numericVariable(numNumber).VariableUBound)
        _numericVariable(numNumber).VariableContents(arrayIndex) = CStr(content)

        If _numericVariable(numNumber).OnChangeScript <> "" And Not _gameIsRestoring Then
            Dim script = _numericVariable(numNumber).OnChangeScript
            ExecuteScript(script, ctx)
        End If

        If _numericVariable(numNumber).DisplayString <> "" Then
            UpdateStatusVars(ctx)
        End If
    End Sub

    Private Sub SetOpenClose(name As String, open As Boolean, ctx As Context)
        Dim cmd As String

        If open Then
            cmd = "open"
        Else
            cmd = "close"
        End If

        Dim id = GetObjectIdNoAlias(name)
        If id = 0 Then
            LogASLError("Invalid object name specified in '" & cmd & " <" & name & ">", LogType.WarningError)
            Exit Sub
        End If

        DoOpenClose(id, open, False, ctx)
    End Sub

    Private Sub SetTimerState(name As String, state As Boolean)
        For i = 1 To _numberTimers
            If LCase(name) = LCase(_timers(i).TimerName) Then
                _timers(i).TimerActive = state
                _timers(i).BypassThisTurn = True     ' don't trigger timer during the turn it was first enabled
                Return
            End If
        Next i

        LogASLError("No such timer '" & name & "'", LogType.WarningError)
    End Sub

    Private Function SetUnknownVariableType(variableData As String, ctx As Context) As SetResult
        Dim scp = InStr(variableData, ";")
        If scp = 0 Then
            Return SetResult.Error
        End If

        Dim name = Trim(Left(variableData, scp - 1))
        If InStr(name, "[") <> 0 And InStr(name, "]") <> 0 Then
            Dim pos = InStr(name, "[")
            name = Left(name, pos - 1)
        End If

        Dim contents = Trim(Mid(variableData, scp + 1))

        For i = 1 To _numberStringVariables
            If LCase(_stringVariable(i).VariableName) = LCase(name) Then
                ExecSetString(variableData, ctx)
                Return SetResult.Found
            End If
        Next i

        For i = 1 To _numberNumericVariables
            If LCase(_numericVariable(i).VariableName) = LCase(name) Then
                ExecSetVar(variableData, ctx)
                Return SetResult.Found
            End If
        Next i

        For i = 1 To _numCollectables
            If LCase(_collectables(i).Name) = LCase(name) Then
                ExecuteSetCollectable(variableData, ctx)
                Return SetResult.Found
            End If
        Next i

        Return SetResult.Unfound
    End Function

    Private Function SetUpChoiceForm(blockName As String, ctx As Context) As String
        ' Returns script to execute from choice block
        Dim block = DefineBlockParam("selection", blockName)
        Dim prompt = FindStatement(block, "info")

        Dim menuOptions As New Dictionary(Of String, String)
        Dim menuScript As New Dictionary(Of String, String)

        For i = block.StartLine + 1 To block.EndLine - 1
            If BeginsWith(_lines(i), "choice ") Then
                menuOptions.Add(CStr(i), GetParameter(_lines(i), ctx))
                menuScript.Add(CStr(i), Trim(Right(_lines(i), Len(_lines(i)) - InStr(_lines(i), ">"))))
            End If
        Next i

        Print("- |i" & prompt & "|xi", ctx)

        Dim mnu As New MenuData(prompt, menuOptions, False)
        Dim choice As String = ShowMenu(mnu)

        Print("- " & menuOptions(choice) & "|n", ctx)
        Return menuScript(choice)
    End Function

    Private Sub SetUpDefaultFonts()
        ' Sets up default fonts
        Dim gameblock = GetDefineBlock("game")

        _defaultFontName = "Arial"
        _defaultFontSize = 9

        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(_lines(i), "default fontname ") Then
                Dim name = GetParameter(_lines(i), _nullContext)
                If name <> "" Then
                    _defaultFontName = name
                End If
            ElseIf BeginsWith(_lines(i), "default fontsize ") Then
                Dim size = CInt(GetParameter(_lines(i), _nullContext))
                If size <> 0 Then
                    _defaultFontSize = size
                End If
            End If
        Next i
    End Sub

    Private Sub SetUpDisplayVariables()
        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(i), "define variable ") Then

                Dim variable = New VariableType
                ReDim variable.VariableContents(0)

                variable.VariableName = GetParameter(_lines(i), _nullContext)
                variable.DisplayString = ""
                variable.NoZeroDisplay = False
                variable.OnChangeScript = ""
                variable.VariableContents(0) = ""
                variable.VariableUBound = 0

                Dim type = "numeric"

                Do
                    i = i + 1

                    If BeginsWith(_lines(i), "type ") Then
                        type = GetEverythingAfter(_lines(i), "type ")
                        If type <> "string" And type <> "numeric" Then
                            LogASLError("Unrecognised variable type in variable '" & variable.VariableName & "' - type '" & type & "'", LogType.WarningError)
                            Exit Do
                        End If
                    ElseIf BeginsWith(_lines(i), "onchange ") Then
                        variable.OnChangeScript = GetEverythingAfter(_lines(i), "onchange ")
                    ElseIf BeginsWith(_lines(i), "display ") Then
                        Dim displayString = GetEverythingAfter(_lines(i), "display ")
                        If BeginsWith(displayString, "nozero ") Then
                            variable.NoZeroDisplay = True
                        End If
                        variable.DisplayString = GetParameter(_lines(i), _nullContext, False)
                    ElseIf BeginsWith(_lines(i), "value ") Then
                        variable.VariableContents(0) = GetParameter(_lines(i), _nullContext)
                    End If

                Loop Until Trim(_lines(i)) = "end define"

                If type = "string" Then
                    ' Create string variable
                    _numberStringVariables = _numberStringVariables + 1
                    Dim id = _numberStringVariables
                    ReDim Preserve _stringVariable(id)
                    _stringVariable(id) = variable
                    _numDisplayStrings = _numDisplayStrings + 1
                ElseIf type = "numeric" Then
                    If variable.VariableContents(0) = "" Then variable.VariableContents(0) = CStr(0)
                    _numberNumericVariables = _numberNumericVariables + 1
                    Dim iNumNumber = _numberNumericVariables
                    ReDim Preserve _numericVariable(iNumNumber)
                    _numericVariable(iNumNumber) = variable
                    _numDisplayNumerics = _numDisplayNumerics + 1
                End If
            End If
        Next i

    End Sub

    Private Sub SetUpGameObject()
        _numberObjs = 1
        ReDim _objs(1)
        _objs(1) = New ObjectType
        Dim o = _objs(1)
        o.ObjectName = "game"
        o.ObjectAlias = ""
        o.Visible = False
        o.Exists = True

        Dim nestBlock = 0
        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If nestBlock = 0 Then
                If BeginsWith(_lines(i), "define ") Then
                    nestBlock = nestBlock + 1
                ElseIf BeginsWith(_lines(i), "properties ") Then
                    AddToObjectProperties(GetParameter(_lines(i), _nullContext), _numberObjs, _nullContext)
                ElseIf BeginsWith(_lines(i), "type ") Then
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1
                    ReDim Preserve o.TypesIncluded(o.NumberTypesIncluded)
                    o.TypesIncluded(o.NumberTypesIncluded) = GetParameter(_lines(i), _nullContext)

                    Dim propertyData = GetPropertiesInType(GetParameter(_lines(i), _nullContext))
                    AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext)
                    For k = 1 To propertyData.NumberActions
                        AddObjectAction(_numberObjs, propertyData.Actions(k).ActionName, propertyData.Actions(k).Script)
                    Next k
                ElseIf BeginsWith(_lines(i), "action ") Then
                    AddToObjectActions(GetEverythingAfter(_lines(i), "action "), _numberObjs, _nullContext)
                End If
            Else
                If Trim(_lines(i)) = "end define" Then
                    nestBlock = nestBlock - 1
                End If
            End If
        Next i
    End Sub

    Private Sub SetUpMenus()
        Dim exists As Boolean = False
        Dim menuTitle As String = ""
        Dim menuOptions As New Dictionary(Of String, String)

        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define menu ") Then

                If exists Then
                    LogASLError("Can't load menu '" & GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext) & "' - only one menu can be added.", LogType.WarningError)
                Else
                    menuTitle = GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext)

                    For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                        If Trim(_lines(j)) <> "" Then
                            Dim scp = InStr(_lines(j), ":")
                            If scp = 0 And _lines(j) <> "-" Then
                                LogASLError("No menu command specified in menu '" & menuTitle & "', item '" & _lines(j), LogType.WarningError)
                            Else
                                If _lines(j) = "-" Then
                                    menuOptions.Add("k" & CStr(j), "-")
                                Else
                                    menuOptions.Add(Trim(Mid(_lines(j), scp + 1)), Trim(Left(_lines(j), scp - 1)))
                                End If

                            End If
                        End If
                    Next j

                    If menuOptions.Count > 0 Then
                        exists = True
                    End If
                End If
            End If
        Next i

        If exists Then
            Dim windowMenu As New MenuData(menuTitle, menuOptions, False)
            _player.SetWindowMenu(windowMenu)
        End If
    End Sub

    Private Sub SetUpOptions()
        Dim opt As String

        For i = GetDefineBlock("options").StartLine + 1 To GetDefineBlock("options").EndLine - 1
            If BeginsWith(_lines(i), "panes ") Then
                opt = LCase(Trim(GetEverythingAfter(_lines(i), "panes ")))
                _player.SetPanesVisible(opt)
            ElseIf BeginsWith(_lines(i), "abbreviations ") Then
                opt = LCase(Trim(GetEverythingAfter(_lines(i), "abbreviations ")))
                If opt = "off" Then _useAbbreviations = False Else _useAbbreviations = True
            End If
        Next i
    End Sub

    Private Sub SetUpRoomData()
        Dim defaultProperties As New PropertiesActions

        ' see if define type <defaultroom> exists:
        Dim defaultExists = False
        For i = 1 To _numberSections
            If Trim(_lines(_defineBlocks(i).StartLine)) = "define type <defaultroom>" Then
                defaultExists = True
                defaultProperties = GetPropertiesInType("defaultroom")
                Exit For
            End If
        Next i

        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define room ") Then
                _numberRooms = _numberRooms + 1
                ReDim Preserve _rooms(_numberRooms)
                _rooms(_numberRooms) = New RoomType

                _numberObjs = _numberObjs + 1
                ReDim Preserve _objs(_numberObjs)
                _objs(_numberObjs) = New ObjectType

                Dim r = _rooms(_numberRooms)

                r.RoomName = GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext)
                _objs(_numberObjs).ObjectName = r.RoomName
                _objs(_numberObjs).IsRoom = True
                _objs(_numberObjs).CorresRoom = r.RoomName
                _objs(_numberObjs).CorresRoomId = _numberRooms

                r.ObjId = _numberObjs

                If _gameAslVersion >= 410 Then
                    r.Exits = New RoomExits(Me)
                    r.Exits.ObjID = r.ObjId
                End If

                If defaultExists Then
                    AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext)
                    For k = 1 To defaultProperties.NumberActions
                        AddObjectAction(_numberObjs, defaultProperties.Actions(k).ActionName, defaultProperties.Actions(k).Script)
                    Next k
                End If

                For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                    If BeginsWith(_lines(j), "define ") Then
                        'skip nested blocks
                        Dim nestedBlock = 1
                        Do
                            j = j + 1
                            If BeginsWith(_lines(j), "define ") Then
                                nestedBlock = nestedBlock + 1
                            ElseIf Trim(_lines(j)) = "end define" Then
                                nestedBlock = nestedBlock - 1
                            End If
                        Loop Until nestedBlock = 0
                    End If

                    If _gameAslVersion >= 280 And BeginsWith(_lines(j), "alias ") Then
                        r.RoomAlias = GetParameter(_lines(j), _nullContext)
                        _objs(_numberObjs).ObjectAlias = r.RoomAlias
                        If _gameAslVersion >= 350 Then AddToObjectProperties("alias=" & r.RoomAlias, _numberObjs, _nullContext)
                    ElseIf _gameAslVersion >= 280 And BeginsWith(_lines(j), "description ") Then
                        r.Description = GetTextOrScript(GetEverythingAfter(_lines(j), "description "))
                        If _gameAslVersion >= 350 Then
                            If r.Description.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "description", r.Description.Data)
                            Else
                                AddToObjectProperties("description=" & r.Description.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "out ") Then
                        r.Out.Text = GetParameter(_lines(j), _nullContext)
                        r.Out.Script = Trim(Mid(_lines(j), InStr(_lines(j), ">") + 1))
                        If _gameAslVersion >= 350 Then
                            If r.Out.Script <> "" Then
                                AddObjectAction(_numberObjs, "out", r.Out.Script)
                            End If

                            AddToObjectProperties("out=" & r.Out.Text, _numberObjs, _nullContext)
                        End If
                    ElseIf BeginsWith(_lines(j), "east ") Then
                        r.East = GetTextOrScript(GetEverythingAfter(_lines(j), "east "))
                        If _gameAslVersion >= 350 Then
                            If r.East.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "east", r.East.Data)
                            Else
                                AddToObjectProperties("east=" & r.East.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "west ") Then
                        r.West = GetTextOrScript(GetEverythingAfter(_lines(j), "west "))
                        If _gameAslVersion >= 350 Then
                            If r.West.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "west", r.West.Data)
                            Else
                                AddToObjectProperties("west=" & r.West.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "north ") Then
                        r.North = GetTextOrScript(GetEverythingAfter(_lines(j), "north "))
                        If _gameAslVersion >= 350 Then
                            If r.North.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "north", r.North.Data)
                            Else
                                AddToObjectProperties("north=" & r.North.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "south ") Then
                        r.South = GetTextOrScript(GetEverythingAfter(_lines(j), "south "))
                        If _gameAslVersion >= 350 Then
                            If r.South.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "south", r.South.Data)
                            Else
                                AddToObjectProperties("south=" & r.South.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "northeast ") Then
                        r.NorthEast = GetTextOrScript(GetEverythingAfter(_lines(j), "northeast "))
                        If _gameAslVersion >= 350 Then
                            If r.NorthEast.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "northeast", r.NorthEast.Data)
                            Else
                                AddToObjectProperties("northeast=" & r.NorthEast.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "northwest ") Then
                        r.NorthWest = GetTextOrScript(GetEverythingAfter(_lines(j), "northwest "))
                        If _gameAslVersion >= 350 Then
                            If r.NorthWest.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "northwest", r.NorthWest.Data)
                            Else
                                AddToObjectProperties("northwest=" & r.NorthWest.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "southeast ") Then
                        r.SouthEast = GetTextOrScript(GetEverythingAfter(_lines(j), "southeast "))
                        If _gameAslVersion >= 350 Then
                            If r.SouthEast.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "southeast", r.SouthEast.Data)
                            Else
                                AddToObjectProperties("southeast=" & r.SouthEast.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "southwest ") Then
                        r.SouthWest = GetTextOrScript(GetEverythingAfter(_lines(j), "southwest "))
                        If _gameAslVersion >= 350 Then
                            If r.SouthWest.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "southwest", r.SouthWest.Data)
                            Else
                                AddToObjectProperties("southwest=" & r.SouthWest.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "up ") Then
                        r.Up = GetTextOrScript(GetEverythingAfter(_lines(j), "up "))
                        If _gameAslVersion >= 350 Then
                            If r.Up.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "up", r.Up.Data)
                            Else
                                AddToObjectProperties("up=" & r.Up.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf BeginsWith(_lines(j), "down ") Then
                        r.Down = GetTextOrScript(GetEverythingAfter(_lines(j), "down "))
                        If _gameAslVersion >= 350 Then
                            If r.Down.Type = TextActionType.Script Then
                                AddObjectAction(_numberObjs, "down", r.Down.Data)
                            Else
                                AddToObjectProperties("down=" & r.Down.Data, _numberObjs, _nullContext)
                            End If
                        End If
                    ElseIf _gameAslVersion >= 280 And BeginsWith(_lines(j), "indescription ") Then
                        r.InDescription = GetParameter(_lines(j), _nullContext)
                        If _gameAslVersion >= 350 Then AddToObjectProperties("indescription=" & r.InDescription, _numberObjs, _nullContext)
                    ElseIf _gameAslVersion >= 280 And BeginsWith(_lines(j), "look ") Then
                        r.Look = GetParameter(_lines(j), _nullContext)
                        If _gameAslVersion >= 350 Then AddToObjectProperties("look=" & r.Look, _numberObjs, _nullContext)
                    ElseIf BeginsWith(_lines(j), "prefix ") Then
                        r.Prefix = GetParameter(_lines(j), _nullContext)
                        If _gameAslVersion >= 350 Then AddToObjectProperties("prefix=" & r.Prefix, _numberObjs, _nullContext)
                    ElseIf BeginsWith(_lines(j), "script ") Then
                        r.Script = GetEverythingAfter(_lines(j), "script ")
                        AddObjectAction(_numberObjs, "script", r.Script)
                    ElseIf BeginsWith(_lines(j), "command ") Then
                        r.NumberCommands = r.NumberCommands + 1
                        ReDim Preserve r.Commands(r.NumberCommands)
                        r.Commands(r.NumberCommands) = New UserDefinedCommandType
                        r.Commands(r.NumberCommands).CommandText = GetParameter(_lines(j), _nullContext, False)
                        r.Commands(r.NumberCommands).CommandScript = Trim(Mid(_lines(j), InStr(_lines(j), ">") + 1))
                    ElseIf BeginsWith(_lines(j), "place ") Then
                        r.NumberPlaces = r.NumberPlaces + 1
                        ReDim Preserve r.Places(r.NumberPlaces)
                        r.Places(r.NumberPlaces) = New PlaceType
                        Dim placeData = GetParameter(_lines(j), _nullContext)
                        Dim scp = InStr(placeData, ";")
                        If scp = 0 Then
                            r.Places(r.NumberPlaces).PlaceName = placeData
                        Else
                            r.Places(r.NumberPlaces).PlaceName = Trim(Mid(placeData, scp + 1))
                            r.Places(r.NumberPlaces).Prefix = Trim(Left(placeData, scp - 1))
                        End If
                        r.Places(r.NumberPlaces).Script = Trim(Mid(_lines(j), InStr(_lines(j), ">") + 1))
                    ElseIf BeginsWith(_lines(j), "use ") Then
                        r.NumberUse = r.NumberUse + 1
                        ReDim Preserve r.Use(r.NumberUse)
                        r.Use(r.NumberUse) = New ScriptText
                        r.Use(r.NumberUse).Text = GetParameter(_lines(j), _nullContext)
                        r.Use(r.NumberUse).Script = Trim(Mid(_lines(j), InStr(_lines(j), ">") + 1))
                    ElseIf BeginsWith(_lines(j), "properties ") Then
                        AddToObjectProperties(GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                    ElseIf BeginsWith(_lines(j), "type ") Then
                        _objs(_numberObjs).NumberTypesIncluded = _objs(_numberObjs).NumberTypesIncluded + 1
                        ReDim Preserve _objs(_numberObjs).TypesIncluded(_objs(_numberObjs).NumberTypesIncluded)
                        _objs(_numberObjs).TypesIncluded(_objs(_numberObjs).NumberTypesIncluded) = GetParameter(_lines(j), _nullContext)

                        Dim propertyData = GetPropertiesInType(GetParameter(_lines(j), _nullContext))
                        AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext)
                        For k = 1 To propertyData.NumberActions
                            AddObjectAction(_numberObjs, propertyData.Actions(k).ActionName, propertyData.Actions(k).Script)
                        Next k
                    ElseIf BeginsWith(_lines(j), "action ") Then
                        AddToObjectActions(GetEverythingAfter(_lines(j), "action "), _numberObjs, _nullContext)
                    ElseIf BeginsWith(_lines(j), "beforeturn ") Then
                        r.BeforeTurnScript = r.BeforeTurnScript & GetEverythingAfter(_lines(j), "beforeturn ") & vbCrLf
                    ElseIf BeginsWith(_lines(j), "afterturn ") Then
                        r.AfterTurnScript = r.AfterTurnScript & GetEverythingAfter(_lines(j), "afterturn ") & vbCrLf
                    End If
                Next j
            End If
        Next i
    End Sub

    Private Sub SetUpSynonyms()
        Dim block = GetDefineBlock("synonyms")
        _numberSynonyms = 0

        If block.StartLine = 0 And block.EndLine = 0 Then
            Exit Sub
        End If

        For i = block.StartLine + 1 To block.EndLine - 1
            Dim eqp = InStr(_lines(i), "=")
            If eqp <> 0 Then
                Dim originalWordsList = Trim(Left(_lines(i), eqp - 1))
                Dim convertWord = Trim(Mid(_lines(i), eqp + 1))

                'Go through each word in OriginalWordsList (sep.
                'by ";"):

                originalWordsList = originalWordsList & ";"
                Dim pos = 1

                Do
                    Dim endOfWord = InStr(pos, originalWordsList, ";")
                    Dim thisWord = Trim(Mid(originalWordsList, pos, endOfWord - pos))

                    If InStr(" " & convertWord & " ", " " & thisWord & " ") > 0 Then
                        ' Recursive synonym
                        LogASLError("Recursive synonym detected: '" & thisWord & "' converting to '" & convertWord & "'", LogType.WarningError)
                    Else
                        _numberSynonyms = _numberSynonyms + 1
                        ReDim Preserve _synonyms(_numberSynonyms)
                        _synonyms(_numberSynonyms) = New SynonymType
                        _synonyms(_numberSynonyms).OriginalWord = thisWord
                        _synonyms(_numberSynonyms).ConvertTo = convertWord
                    End If
                    pos = endOfWord + 1
                Loop Until pos >= Len(originalWordsList)
            End If
        Next i
    End Sub

    Private Sub SetUpTimers()
        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define timer ") Then
                _numberTimers = _numberTimers + 1
                ReDim Preserve _timers(_numberTimers)
                _timers(_numberTimers) = New TimerType
                _timers(_numberTimers).TimerName = GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext)
                _timers(_numberTimers).TimerActive = False

                For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                    If BeginsWith(_lines(j), "interval ") Then
                        _timers(_numberTimers).TimerInterval = CInt(GetParameter(_lines(j), _nullContext))
                    ElseIf BeginsWith(_lines(j), "action ") Then
                        _timers(_numberTimers).TimerAction = GetEverythingAfter(_lines(j), "action ")
                    ElseIf Trim(LCase(_lines(j))) = "enabled" Then
                        _timers(_numberTimers).TimerActive = True
                    ElseIf Trim(LCase(_lines(j))) = "disabled" Then
                        _timers(_numberTimers).TimerActive = False
                    End If
                Next j
            End If
        Next i
    End Sub

    Private Sub SetUpTurnScript()
        Dim block = GetDefineBlock("game")

        _beforeTurnScript = ""
        _afterTurnScript = ""

        For i = block.StartLine + 1 To block.EndLine - 1
            If BeginsWith(_lines(i), "beforeturn ") Then
                _beforeTurnScript = _beforeTurnScript & GetEverythingAfter(Trim(_lines(i)), "beforeturn ") & vbCrLf
            ElseIf BeginsWith(_lines(i), "afterturn ") Then
                _afterTurnScript = _afterTurnScript & GetEverythingAfter(Trim(_lines(i)), "afterturn ") & vbCrLf
            End If
        Next i
    End Sub

    Private Sub SetUpUserDefinedPlayerErrors()
        ' goes through "define game" block and sets stored error
        ' messages accordingly

        Dim block = GetDefineBlock("game")
        Dim examineIsCustomised = False

        For i = block.StartLine + 1 To block.EndLine - 1
            If BeginsWith(_lines(i), "error ") Then
                Dim errorInfo = GetParameter(_lines(i), _nullContext, False)
                Dim scp = InStr(errorInfo, ";")
                Dim errorName = Left(errorInfo, scp - 1)
                Dim errorMsg = Trim(Mid(errorInfo, scp + 1))
                Dim currentError = 0

                Select Case errorName
                    Case "badcommand"
                        currentError = PlayerError.BadCommand
                    Case "badgo"
                        currentError = PlayerError.BadGo
                    Case "badgive"
                        currentError = PlayerError.BadGive
                    Case "badcharacter"
                        currentError = PlayerError.BadCharacter
                    Case "noitem"
                        currentError = PlayerError.NoItem
                    Case "itemunwanted"
                        currentError = PlayerError.ItemUnwanted
                    Case "badlook"
                        currentError = PlayerError.BadLook
                    Case "badthing"
                        currentError = PlayerError.BadThing
                    Case "defaultlook"
                        currentError = PlayerError.DefaultLook
                    Case "defaultspeak"
                        currentError = PlayerError.DefaultSpeak
                    Case "baditem"
                        currentError = PlayerError.BadItem
                    Case "baddrop"
                        currentError = PlayerError.BadDrop
                    Case "defaultake"
                        If _gameAslVersion <= 280 Then
                            currentError = PlayerError.BadTake
                        Else
                            currentError = PlayerError.DefaultTake
                        End If
                    Case "baduse"
                        currentError = PlayerError.BadUse
                    Case "defaultuse"
                        currentError = PlayerError.DefaultUse
                    Case "defaultout"
                        currentError = PlayerError.DefaultOut
                    Case "badplace"
                        currentError = PlayerError.BadPlace
                    Case "badexamine"
                        If _gameAslVersion >= 310 Then
                            currentError = PlayerError.BadExamine
                        End If
                    Case "defaultexamine"
                        currentError = PlayerError.DefaultExamine
                        examineIsCustomised = True
                    Case "badtake"
                        currentError = PlayerError.BadTake
                    Case "cantdrop"
                        currentError = PlayerError.CantDrop
                    Case "defaultdrop"
                        currentError = PlayerError.DefaultDrop
                    Case "badpronoun"
                        currentError = PlayerError.BadPronoun
                    Case "alreadyopen"
                        currentError = PlayerError.AlreadyOpen
                    Case "alreadyclosed"
                        currentError = PlayerError.AlreadyClosed
                    Case "cantopen"
                        currentError = PlayerError.CantOpen
                    Case "cantclose"
                        currentError = PlayerError.CantClose
                    Case "defaultopen"
                        currentError = PlayerError.DefaultOpen
                    Case "defaultclose"
                        currentError = PlayerError.DefaultClose
                    Case "badput"
                        currentError = PlayerError.BadPut
                    Case "cantput"
                        currentError = PlayerError.CantPut
                    Case "defaultput"
                        currentError = PlayerError.DefaultPut
                    Case "cantremove"
                        currentError = PlayerError.CantRemove
                    Case "alreadyput"
                        currentError = PlayerError.AlreadyPut
                    Case "defaultremove"
                        currentError = PlayerError.DefaultRemove
                    Case "locked"
                        currentError = PlayerError.Locked
                    Case "defaultwait"
                        currentError = PlayerError.DefaultWait
                    Case "alreadytaken"
                        currentError = PlayerError.AlreadyTaken
                End Select

                _playerErrorMessageString(currentError) = errorMsg
                If currentError = PlayerError.DefaultLook And Not examineIsCustomised Then
                    ' If we're setting the default look message, and we've not already customised the
                    ' default examine message, then set the default examine message to the same thing.
                    _playerErrorMessageString(PlayerError.DefaultExamine) = errorMsg
                End If
            End If
        Next i
    End Sub

    Private Sub SetVisibility(thing As String, type As Thing, visible As Boolean, ctx As Context)
        ' Sets visibilty of objects and characters        

        If _gameAslVersion >= 280 Then
            Dim found = False

            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(thing) Then
                    _objs(i).Visible = visible
                    If visible Then
                        AddToObjectProperties("not invisible", i, ctx)
                    Else
                        AddToObjectProperties("invisible", i, ctx)
                    End If

                    found = True
                    Exit For
                End If
            Next i

            If Not found Then
                LogASLError("Not found object '" & thing & "'", LogType.WarningError)
            End If
        Else
            ' split ThingString into character name and room
            ' (thingstring of form name@room)

            Dim atPos = InStr(thing, "@")
            Dim room, name As String

            ' If no room specified, current room presumed
            If atPos = 0 Then
                room = _currentRoom
                name = thing
            Else
                name = Trim(Left(thing, atPos - 1))
                room = Trim(Right(thing, Len(thing) - atPos))
            End If

            If type = LegacyGame.Thing.Character Then
                For i = 1 To _numberChars
                    If LCase(_chars(i).ContainerRoom) = LCase(room) And LCase(_chars(i).ObjectName) = LCase(name) Then
                        _chars(i).Visible = visible
                        Exit For
                    End If
                Next i
            ElseIf type = LegacyGame.Thing.Object Then
                For i = 1 To _numberObjs
                    If LCase(_objs(i).ContainerRoom) = LCase(room) And LCase(_objs(i).ObjectName) = LCase(name) Then
                        _objs(i).Visible = visible
                        Exit For
                    End If
                Next i
            End If
        End If

        UpdateObjectList(ctx)
    End Sub

    Private Sub ShowPictureInText(filename As String)
        If Not _useStaticFrameForPictures Then
            _player.ShowPicture(filename)
        Else
            ' Workaround for a particular game which expects pictures to be in a popup window -
            ' use the static picture frame feature so that image is not cleared
            _player.SetPanelContents("<img src=""" + _player.GetURL(filename) + """ onload=""setPanelHeight()""/>")
        End If
    End Sub

    Private Sub ShowRoomInfoV2(Room As String)
        ' ShowRoomInfo for Quest 2.x games

        Dim roomDisplayText As String = ""
        Dim descTagExist As Boolean
        Dim gameBlock As DefineBlock
        Dim charsViewable As String
        Dim charsFound As Integer
        Dim prefixAliasNoFormat, prefix, prefixAlias, inDesc As String
        Dim aliasName As String = ""
        Dim charList As String
        Dim foundLastComma, cp, ncp As Integer
        Dim noFormatObjsViewable As String
        Dim objsViewable As String = ""
        Dim objsFound As Integer
        Dim objListString, noFormatObjListString As String
        Dim possDir, nsew, doorways, places, place As String
        Dim aliasOut As String = ""
        Dim placeNoFormat As String
        Dim descLine As String = ""
        Dim lastComma, oldLastComma As Integer
        Dim defineBlock As Integer
        Dim lookString As String = ""

        gameBlock = GetDefineBlock("game")
        _currentRoom = Room

        'find the room
        Dim roomBlock As DefineBlock
        roomBlock = DefineBlockParam("room", Room)
        Dim finishedFindingCommas As Boolean

        charsViewable = ""
        charsFound = 0

        'see if room has an alias
        For i = roomBlock.StartLine + 1 To roomBlock.EndLine - 1
            If BeginsWith(_lines(i), "alias") Then
                aliasName = GetParameter(_lines(i), _nullContext)
                i = roomBlock.EndLine
            End If
        Next i
        If aliasName = "" Then aliasName = Room

        'see if room has a prefix
        prefix = FindStatement(roomBlock, "prefix")
        If prefix = "" Then
            prefixAlias = "|cr" & aliasName & "|cb"
            prefixAliasNoFormat = aliasName ' No formatting version, for label
        Else
            prefixAlias = prefix & " |cr" & aliasName & "|cb"
            prefixAliasNoFormat = prefix & " " & aliasName
        End If

        'print player's location
        'find indescription line:
        inDesc = "unfound"
        For i = roomBlock.StartLine + 1 To roomBlock.EndLine - 1
            If BeginsWith(_lines(i), "indescription") Then
                inDesc = Trim(GetParameter(_lines(i), _nullContext))
                i = roomBlock.EndLine
            End If
        Next i

        If inDesc <> "unfound" Then
            ' Print player's location according to indescription:
            If Right(inDesc, 1) = ":" Then
                ' if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText & Left(inDesc, Len(inDesc) - 1) & " " & prefixAlias & "." & vbCrLf
            Else
                ' otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText & inDesc & vbCrLf
            End If
        Else
            ' if no indescription line, print the default.
            roomDisplayText = roomDisplayText & "You are in " & prefixAlias & "." & vbCrLf
        End If

        _player.LocationUpdated(prefixAliasNoFormat)

        SetStringContents("quest.formatroom", prefixAliasNoFormat, _nullContext)

        'FIND CHARACTERS ===

        For i = 1 To _numberChars
            If _chars(i).ContainerRoom = Room And _chars(i).Exists And _chars(i).Visible Then
                charsViewable = charsViewable & _chars(i).Prefix & "|b" & _chars(i).ObjectName & "|xb" & _chars(i).Suffix & ", "
                charsFound = charsFound + 1
            End If
        Next i

        If charsFound = 0 Then
            charsViewable = "There is nobody here."
            SetStringContents("quest.characters", "", _nullContext)
        Else
            'chop off final comma and add full stop (.)
            charList = Left(charsViewable, Len(charsViewable) - 2)
            SetStringContents("quest.characters", charList, _nullContext)

            'if more than one character, add "and" before
            'last one:
            cp = InStr(charList, ",")
            If cp <> 0 Then
                foundLastComma = 0
                Do
                    ncp = InStr(cp + 1, charList, ",")
                    If ncp = 0 Then
                        foundLastComma = 1
                    Else
                        cp = ncp
                    End If
                Loop Until foundLastComma = 1

                charList = Trim(Left(charList, cp - 1)) & " and " & Trim(Mid(charList, cp + 1))
            End If

            charsViewable = "You can see " & charList & " here."
        End If

        roomDisplayText = roomDisplayText & charsViewable & vbCrLf

        'FIND OBJECTS

        noFormatObjsViewable = ""

        For i = 1 To _numberObjs
            If _objs(i).ContainerRoom = Room And _objs(i).Exists And _objs(i).Visible Then
                objsViewable = objsViewable & _objs(i).Prefix & "|b" & _objs(i).ObjectName & "|xb" & _objs(i).Suffix & ", "
                noFormatObjsViewable = noFormatObjsViewable & _objs(i).Prefix & _objs(i).ObjectName & ", "

                objsFound = objsFound + 1
            End If
        Next i

        Dim finishedLoop As Boolean
        If objsFound <> 0 Then
            objListString = Left(objsViewable, Len(objsViewable) - 2)
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2)

            cp = InStr(objListString, ",")
            If cp <> 0 Then
                Do
                    ncp = InStr(cp + 1, objListString, ",")
                    If ncp = 0 Then
                        finishedLoop = True
                    Else
                        cp = ncp
                    End If
                Loop Until finishedLoop

                objListString = Trim(Left(objListString, cp - 1) & " and " & Trim(Mid(objListString, cp + 1)))
            End If

            objsViewable = "There is " & objListString & " here."
            SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), _nullContext)
            SetStringContents("quest.formatobjects", objListString, _nullContext)
            roomDisplayText = roomDisplayText & objsViewable & vbCrLf
        Else
            SetStringContents("quest.objects", "", _nullContext)
            SetStringContents("quest.formatobjects", "", _nullContext)
        End If

        'FIND DOORWAYS
        doorways = ""
        nsew = ""
        places = ""
        possDir = ""

        For i = roomBlock.StartLine + 1 To roomBlock.EndLine - 1
            If BeginsWith(_lines(i), "out") Then
                doorways = GetParameter(_lines(i), _nullContext)
            End If

            If BeginsWith(_lines(i), "north ") Then
                nsew = nsew & "|bnorth|xb, "
                possDir = possDir & "n"
            ElseIf BeginsWith(_lines(i), "south ") Then
                nsew = nsew & "|bsouth|xb, "
                possDir = possDir & "s"
            ElseIf BeginsWith(_lines(i), "east ") Then
                nsew = nsew & "|beast|xb, "
                possDir = possDir & "e"
            ElseIf BeginsWith(_lines(i), "west ") Then
                nsew = nsew & "|bwest|xb, "
                possDir = possDir & "w"
            ElseIf BeginsWith(_lines(i), "northeast ") Then
                nsew = nsew & "|bnortheast|xb, "
                possDir = possDir & "a"
            ElseIf BeginsWith(_lines(i), "northwest ") Then
                nsew = nsew & "|bnorthwest|xb, "
                possDir = possDir & "b"
            ElseIf BeginsWith(_lines(i), "southeast ") Then
                nsew = nsew & "|bsoutheast|xb, "
                possDir = possDir & "c"
            ElseIf BeginsWith(_lines(i), "southwest ") Then
                nsew = nsew & "|bsouthwest|xb, "
                possDir = possDir & "d"
            End If

            If BeginsWith(_lines(i), "place") Then
                'remove any prefix semicolon from printed text
                place = GetParameter(_lines(i), _nullContext)
                placeNoFormat = place 'Used in object list - no formatting or prefix
                If InStr(place, ";") > 0 Then
                    placeNoFormat = Right(place, Len(place) - (InStr(place, ";") + 1))
                    place = Trim(Left(place, InStr(place, ";") - 1)) & " |b" & Right(place, Len(place) - (InStr(place, ";") + 1)) & "|xb"
                Else
                    place = "|b" & place & "|xb"
                End If
                places = places & place & ", "

            End If

        Next i

        Dim outside As DefineBlock
        If doorways <> "" Then
            'see if outside has an alias
            outside = DefineBlockParam("room", doorways)
            For i = outside.StartLine + 1 To outside.EndLine - 1
                If BeginsWith(_lines(i), "alias") Then
                    aliasOut = GetParameter(_lines(i), _nullContext)
                    i = outside.EndLine
                End If
            Next i
            If aliasOut = "" Then aliasOut = doorways

            roomDisplayText = roomDisplayText & "You can go out to " & aliasOut & "." & vbCrLf
            possDir = possDir & "o"
            SetStringContents("quest.doorways.out", aliasOut, _nullContext)
        Else
            SetStringContents("quest.doorways.out", "", _nullContext)
        End If

        Dim finished As Boolean
        If nsew <> "" Then
            'strip final comma
            nsew = Left(nsew, Len(nsew) - 2)
            cp = InStr(nsew, ",")
            If cp <> 0 Then
                finished = False
                Do
                    ncp = InStr(cp + 1, nsew, ",")
                    If ncp = 0 Then
                        finished = True
                    Else
                        cp = ncp
                    End If
                Loop Until finished

                nsew = Trim(Left(nsew, cp - 1)) & " or " & Trim(Mid(nsew, cp + 1))
            End If

            roomDisplayText = roomDisplayText & "You can go " & nsew & "." & vbCrLf
            SetStringContents("quest.doorways.dirs", nsew, _nullContext)
        Else
            SetStringContents("quest.doorways.dirs", "", _nullContext)
        End If

        UpdateDirButtons(possDir, _nullContext)

        If places <> "" Then
            'strip final comma
            places = Left(places, Len(places) - 2)

            'if there is still a comma here, there is more than
            'one place, so add the word "or" before the last one.
            If InStr(places, ",") > 0 Then
                lastComma = 0
                finishedFindingCommas = False
                Do
                    oldLastComma = lastComma
                    lastComma = InStr(lastComma + 1, places, ",")
                    If lastComma = 0 Then
                        finishedFindingCommas = True
                        lastComma = oldLastComma
                    End If
                Loop Until finishedFindingCommas

                places = Left(places, lastComma) & " or" & Right(places, Len(places) - lastComma)
            End If

            roomDisplayText = roomDisplayText & "You can go to " & places & "." & vbCrLf
            SetStringContents("quest.doorways.places", places, _nullContext)
        Else
            SetStringContents("quest.doorways.places", "", _nullContext)
        End If

        'Print RoomDisplayText if there is no "description" tag,
        'otherwise execute the description tag information:

        ' First, look in the "define room" block:
        descTagExist = False
        For i = roomBlock.StartLine + 1 To roomBlock.EndLine - 1
            If BeginsWith(_lines(i), "description ") Then
                descLine = _lines(i)
                descTagExist = True
                Exit For
            End If
        Next i

        If descTagExist = False Then
            'Look in the "define game" block:
            For i = gameBlock.StartLine + 1 To gameBlock.EndLine - 1
                If BeginsWith(_lines(i), "description ") Then
                    descLine = _lines(i)
                    descTagExist = True
                    Exit For
                End If
            Next i
        End If

        If descTagExist = False Then
            'Remove final newline:
            roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2)
            Print(roomDisplayText, _nullContext)
        Else
            'execute description tag:
            'If no script, just print the tag's parameter.
            'Otherwise, execute it as ASL script:

            descLine = GetEverythingAfter(Trim(descLine), "description ")
            If Left(descLine, 1) = "<" Then
                Print(GetParameter(descLine, _nullContext), _nullContext)
            Else
                ExecuteScript(descLine, _nullContext)
            End If
        End If

        UpdateObjectList(_nullContext)

        defineBlock = 0

        For i = roomBlock.StartLine + 1 To roomBlock.EndLine - 1
            ' don't get the 'look' statements in nested define blocks
            If BeginsWith(_lines(i), "define") Then defineBlock = defineBlock + 1
            If BeginsWith(_lines(i), "end define") Then defineBlock = defineBlock - 1
            If BeginsWith(_lines(i), "look") And defineBlock = 0 Then
                lookString = GetParameter(_lines(i), _nullContext)
                i = roomBlock.EndLine
            End If
        Next i

        If lookString <> "" Then Print(lookString, _nullContext)
    End Sub

    Private Sub Speak(text As String)
        _player.Speak(text)
    End Sub

    Private Sub AddToObjectList(objList As List(Of ListData), exitList As List(Of ListData), name As String, type As Thing)
        name = CapFirst(name)

        If type = Thing.Room Then
            objList.Add(New ListData(name, _listVerbs(ListType.ExitsList)))
            exitList.Add(New ListData(name, _listVerbs(ListType.ExitsList)))
        Else
            objList.Add(New ListData(name, _listVerbs(ListType.ObjectsList)))
        End If
    End Sub

    Private Sub ExecExec(scriptLine As String, ctx As Context)
        If ctx.CancelExec Then Exit Sub

        Dim execLine = GetParameter(scriptLine, ctx)
        Dim newCtx As Context = CopyContext(ctx)
        newCtx.StackCounter = newCtx.StackCounter + 1

        If newCtx.StackCounter > 500 Then
            LogASLError("Out of stack space running '" & scriptLine & "' - infinite loop?", LogType.WarningError)
            ctx.CancelExec = True
            Exit Sub
        End If

        If _gameAslVersion >= 310 Then
            newCtx.AllowRealNamesInCommand = True
        End If

        If InStr(execLine, ";") = 0 Then
            Try
                ExecCommand(execLine, newCtx, False)
            Catch
                LogASLError("Internal error " & Err.Number & " running '" & scriptLine & "'", LogType.WarningError)
                ctx.CancelExec = True
            End Try
        Else
            Dim scp = InStr(execLine, ";")
            Dim ex = Trim(Left(execLine, scp - 1))
            Dim r = Trim(Mid(execLine, scp + 1))
            If r = "normal" Then
                ExecCommand(ex, newCtx, False, False)
            Else
                LogASLError("Unrecognised post-command parameter in " & Trim(scriptLine), LogType.WarningError)
            End If
        End If
    End Sub

    Private Sub ExecSetString(info As String, ctx As Context)
        ' Sets string contents from a script parameter.
        ' Eg <string1;contents> sets string variable string1
        ' to "contents"

        Dim scp = InStr(info, ";")
        Dim name = Trim(Left(info, scp - 1))
        Dim value = Mid(info, scp + 1)

        If IsNumeric(name) Then
            LogASLError("Invalid string name '" & name & "' - string names cannot be numeric", LogType.WarningError)
            Exit Sub
        End If

        If _gameAslVersion >= 281 Then
            value = Trim(value)
            If Left(value, 1) = "[" And Right(value, 1) = "]" Then
                value = Mid(value, 2, Len(value) - 2)
            End If
        End If

        Dim idx = GetArrayIndex(name, ctx)
        SetStringContents(name, value, ctx, idx)
    End Sub

    Private Function ExecUserCommand(cmd As String, ctx As Context, Optional libCommands As Boolean = False) As Boolean
        'Executes a user-defined command. If unavailable, returns
        'false.
        Dim curCmd, commandList As String
        Dim script As String = ""
        Dim commandTag As String
        Dim commandLine As String = ""
        Dim foundCommand = False

        'First, check for a command in the current room block
        Dim roomId = GetRoomID(_currentRoom, ctx)

        ' RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        If roomId <> 0 Then
            Dim r = _rooms(roomId)
            For i = 1 To r.NumberCommands
                commandList = r.Commands(i).CommandText
                Dim ep As Integer
                Do
                    ep = InStr(commandList, ";")
                    If ep = 0 Then
                        curCmd = commandList
                    Else
                        curCmd = Trim(Left(commandList, ep - 1))
                        commandList = Trim(Mid(commandList, ep + 1))
                    End If

                    If IsCompatible(LCase(cmd), LCase(curCmd)) Then
                        commandLine = curCmd
                        script = r.Commands(i).CommandScript
                        foundCommand = True
                        ep = 0
                        Exit For
                    End If
                Loop Until ep = 0
            Next i
        End If

        If Not libCommands Then
            commandTag = "command"
        Else
            commandTag = "lib command"
        End If

        If Not foundCommand Then
            ' Check "define game" block
            Dim block = GetDefineBlock("game")
            For i = block.StartLine + 1 To block.EndLine - 1
                If BeginsWith(_lines(i), commandTag) Then

                    commandList = GetParameter(_lines(i), ctx, False)
                    Dim ep As Integer
                    Do
                        ep = InStr(commandList, ";")
                        If ep = 0 Then
                            curCmd = commandList
                        Else
                            curCmd = Trim(Left(commandList, ep - 1))
                            commandList = Trim(Mid(commandList, ep + 1))
                        End If

                        If IsCompatible(LCase(cmd), LCase(curCmd)) Then
                            commandLine = curCmd
                            Dim ScriptPos = InStr(_lines(i), ">") + 1
                            script = Trim(Mid(_lines(i), ScriptPos))
                            foundCommand = True
                            ep = 0
                            Exit For
                        End If
                    Loop Until ep = 0
                End If
            Next i
        End If

        If foundCommand Then
            If GetCommandParameters(cmd, commandLine, ctx) Then
                ExecuteScript(script, ctx)
            End If
        End If

        Return foundCommand
    End Function

    Private Sub ExecuteChoose(section As String, ctx As Context)
        ExecuteScript(SetUpChoiceForm(section, ctx), ctx)
    End Sub

    Private Function GetCommandParameters(test As String, required As String, ctx As Context) As Boolean
        'Gets parameters from line. For example, if 'required'
        'is "read #1#" and 'test' is "read sign", #1# returns
        '"sign".

        ' Returns FALSE if #@object# form used and object doesn't
        ' exist.

        Dim chunksBegin() As Integer
        Dim chunksEnd() As Integer
        Dim varName() As String
        Dim var2Pos As Integer

        ' Add dots before and after both strings. This fudge
        ' stops problems caused when variables are right at the
        ' beginning or end of a line.
        ' PostScript: well, it used to, I'm not sure if it's really
        ' required now though....
        ' As of Quest 4.0 we use the ¦ character rather than a dot.
        test = "¦" & Trim(test) & "¦"
        required = "¦" & required & "¦"

        'Go through RequiredLine in chunks going up to variables.
        Dim currentReqLinePos = 1
        Dim currentTestLinePos = 1
        Dim finished = False
        Dim numberChunks = 0
        Do
            Dim nextVarPos = InStr(currentReqLinePos, required, "#")
            Dim currentVariable = ""

            If nextVarPos = 0 Then
                finished = True
                nextVarPos = Len(required) + 1
            Else
                var2Pos = InStr(nextVarPos + 1, required, "#")
                currentVariable = Mid(required, nextVarPos + 1, (var2Pos - 1) - nextVarPos)
            End If

            Dim checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1))
            Dim chunkBegin = InStr(currentTestLinePos, LCase(test), LCase(checkChunk))
            Dim chunkEnd = chunkBegin + Len(checkChunk)

            numberChunks = numberChunks + 1
            ReDim Preserve chunksBegin(numberChunks)
            ReDim Preserve chunksEnd(numberChunks)
            ReDim Preserve varName(numberChunks)
            chunksBegin(numberChunks) = chunkBegin
            chunksEnd(numberChunks) = chunkEnd
            varName(numberChunks) = currentVariable

            'Get to end of variable name
            currentReqLinePos = var2Pos + 1

            currentTestLinePos = chunkEnd
        Loop Until finished

        Dim success = True

        'Return values to string variable
        For i = 1 To numberChunks - 1
            Dim arrayIndex As Integer
            ' If VarName contains array name, change to index number
            If InStr(varName(i), "[") > 0 Then
                arrayIndex = GetArrayIndex(varName(i), ctx)
            Else
                arrayIndex = 0
            End If

            Dim curChunk = Mid(test, chunksEnd(i), chunksBegin(i + 1) - chunksEnd(i))

            If BeginsWith(varName(i), "@") Then
                varName(i) = GetEverythingAfter(varName(i), "@")
                Dim id = Disambiguate(curChunk, _currentRoom & ";" & "inventory", ctx)

                If id = -1 Then
                    If _gameAslVersion >= 391 Then
                        PlayerErrorMessage(PlayerError.BadThing, ctx)
                    Else
                        PlayerErrorMessage(PlayerError.BadItem, ctx)
                    End If
                    ' The Mid$(...,2) and Left$(...,2) removes the initial/final "."
                    _badCmdBefore = Mid(Trim(Left(test, chunksEnd(i) - 1)), 2)
                    _badCmdAfter = Trim(Mid(test, chunksBegin(i + 1)))
                    _badCmdAfter = Left(_badCmdAfter, Len(_badCmdAfter) - 1)
                    success = False
                ElseIf id = -2 Then
                    _badCmdBefore = Trim(Left(test, chunksEnd(i) - 1))
                    _badCmdAfter = Trim(Mid(test, chunksBegin(i + 1)))
                    success = False
                Else
                    SetStringContents(varName(i), _objs(id).ObjectName, ctx, arrayIndex)
                End If
            Else
                SetStringContents(varName(i), curChunk, ctx, arrayIndex)
            End If
        Next i

        Return success
    End Function

    Private Function GetGender(character As String, capitalise As Boolean, ctx As Context) As String
        Dim result As String

        If _gameAslVersion >= 281 Then
            result = _objs(GetObjectIdNoAlias(character)).Gender
        Else
            Dim resultLine = RetrLine("character", character, "gender", ctx)

            If resultLine = "<unfound>" Then
                result = "it "
            Else
                result = GetParameter(resultLine, ctx) & " "
            End If
        End If

        If capitalise Then result = UCase(Left(result, 1)) & Right(result, Len(result) - 1)
        Return result
    End Function

    Private Function GetStringContents(name As String, ctx As Context) As String
        Dim returnAlias = False
        Dim arrayIndex = 0

        ' Check for property shortcut
        Dim cp = InStr(name, ":")
        If cp <> 0 Then
            Dim objName = Trim(Left(name, cp - 1))
            Dim propName = Trim(Mid(name, cp + 1))

            Dim obp = InStr(objName, "(")
            If obp <> 0 Then
                Dim cbp = InStr(obp, objName, ")")
                If cbp <> 0 Then
                    objName = GetStringContents(Mid(objName, obp + 1, (cbp - obp) - 1), ctx)
                End If
            End If

            Return GetObjectProperty(propName, GetObjectIdNoAlias(objName))
        End If

        If Left(name, 1) = "@" Then
            returnAlias = True
            name = Mid(name, 2)
        End If

        If InStr(name, "[") <> 0 And InStr(name, "]") <> 0 Then
            Dim bp = InStr(name, "[")
            Dim ep = InStr(name, "]")
            Dim arrayIndexData = Mid(name, bp + 1, (ep - bp) - 1)
            If IsNumeric(arrayIndexData) Then
                arrayIndex = CInt(arrayIndexData)
            Else
                arrayIndex = CInt(GetNumericContents(arrayIndexData, ctx))
                If arrayIndex = -32767 Then
                    LogASLError("Array index in '" & name & "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)", LogType.WarningError)
                    Return ""
                End If
            End If
            name = Left(name, bp - 1)
        End If

        ' First, see if the string already exists. If it does,
        ' get its contents. If not, generate an error.

        Dim exists = False
        Dim id As Integer

        If _numberStringVariables > 0 Then
            For i = 1 To _numberStringVariables
                If LCase(_stringVariable(i).VariableName) = LCase(name) Then
                    id = i
                    exists = True
                    Exit For
                End If
            Next i
        End If

        If Not exists Then
            LogASLError("No string variable '" & name & "' defined.", LogType.WarningError)
            Return ""
        End If

        If arrayIndex > _stringVariable(id).VariableUBound Then
            LogASLError("Array index of '" & name & "[" & Trim(Str(arrayIndex)) & "]' too big.", LogType.WarningError)
            Return ""
        End If

        ' Now, set the contents
        If Not returnAlias Then
            Return _stringVariable(id).VariableContents(arrayIndex)
        Else
            Return _objs(GetObjectIdNoAlias(_stringVariable(id).VariableContents(arrayIndex))).ObjectAlias
        End If
    End Function

    Private Function IsAvailable(thingName As String, type As Thing, ctx As Context) As Boolean
        ' Returns availability of object/character

        ' split ThingString into character name and room
        ' (thingstring of form name@room)

        Dim room, name As String

        Dim atPos = InStr(thingName, "@")

        ' If no room specified, current room presumed
        If atPos = 0 Then
            room = _currentRoom
            name = thingName
        Else
            name = Trim(Left(thingName, atPos - 1))
            room = Trim(Right(thingName, Len(thingName) - atPos))
        End If

        If type = Thing.Character Then
            For i = 1 To _numberChars
                If LCase(_chars(i).ContainerRoom) = LCase(room) And LCase(_chars(i).ObjectName) = LCase(name) Then
                    Return _chars(i).Exists
                End If
            Next i
        ElseIf type = Thing.Object Then
            For i = 1 To _numberObjs
                If LCase(_objs(i).ContainerRoom) = LCase(room) And LCase(_objs(i).ObjectName) = LCase(name) Then
                    Return _objs(i).Exists
                End If
            Next i
        End If
    End Function

    Private Function IsCompatible(test As String, required As String) As Boolean
        'Tests to see if 'test' "works" with 'required'.
        'For example, if 'required' = "read #text#", then the
        'tests of "read book" and "read sign" are compatible.
        Dim var2Pos As Integer

        ' This avoids "xxx123" being compatible with "xxx".
        test = "¦" & Trim(test) & "¦"
        required = "¦" & required & "¦"

        'Go through RequiredLine in chunks going up to variables.
        Dim currentReqLinePos = 1
        Dim currentTestLinePos = 1
        Dim finished = False
        Do
            Dim nextVarPos = InStr(currentReqLinePos, required, "#")
            If nextVarPos = 0 Then
                nextVarPos = Len(required) + 1
                finished = True
            Else
                var2Pos = InStr(nextVarPos + 1, required, "#")
            End If

            Dim checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1))

            If InStr(currentTestLinePos, test, checkChunk) <> 0 Then
                currentTestLinePos = InStr(currentTestLinePos, test, checkChunk) + Len(checkChunk)
            Else
                Return False
            End If

            'Skip to end of variable
            currentReqLinePos = var2Pos + 1
        Loop Until finished

        Return True
    End Function

    Private Function OpenGame(filename As String) As Boolean
        Dim cdatb, result As Boolean
        Dim visible As Boolean
        Dim room As String
        Dim fileData As String = ""
        Dim savedQsgVersion As String
        Dim data As String = ""
        Dim name As String
        Dim scp, cdat As Integer
        Dim scp2, scp3 As Integer
        Dim lines As String() = {}

        _gameLoadMethod = "loaded"

        Dim prevQsgVersion = False

        If _data Is Nothing Then
            fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252))
        Else
            fileData = System.Text.Encoding.GetEncoding(1252).GetString(_data.Data)
        End If

        ' Check version
        savedQsgVersion = Left(fileData, 10)

        If BeginsWith(savedQsgVersion, "QUEST200.1") Then
            prevQsgVersion = True
        ElseIf Not BeginsWith(savedQsgVersion, "QUEST300") Then
            Return False
        End If

        If prevQsgVersion Then
            lines = fileData.Split({vbCrLf, vbLf}, StringSplitOptions.None)
            _gameFileName = lines(1)
        Else
            InitFileData(fileData)
            GetNextChunk()

            If _data Is Nothing Then
                _gameFileName = GetNextChunk()
            Else
                GetNextChunk()
                _gameFileName = _data.SourceFile
            End If
        End If

        If _data Is Nothing And Not System.IO.File.Exists(_gameFileName) Then
            _gameFileName = _player.GetNewGameFile(_gameFileName, "*.asl;*.cas;*.zip")
            If _gameFileName = "" Then Exit Function
        End If

        result = InitialiseGame(_gameFileName, True)

        If result = False Then
            Return False
        End If

        If Not prevQsgVersion Then
            ' Open Quest 3.0 saved game file
            _gameLoading = True
            RestoreGameData(fileData)
            _gameLoading = False
        Else
            ' Open Quest 2.x saved game file

            _currentRoom = lines(3)

            ' Start at line 5 as line 4 is always "!c"
            Dim lineNumber As Integer = 5

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!i" Then
                    scp = InStr(data, ";")
                    name = Trim(Left(data, scp - 1))
                    cdat = CInt(Right(data, Len(data) - scp))

                    For i = 1 To _numCollectables
                        If _collectables(i).Name = name Then
                            _collectables(i).Value = cdat
                            i = _numCollectables
                        End If
                    Next i
                End If
            Loop Until data = "!i"

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!o" Then
                    scp = InStr(data, ";")
                    name = Trim(Left(data, scp - 1))
                    cdatb = IsYes(Right(data, Len(data) - scp))

                    For i = 1 To _numberItems
                        If _items(i).Name = name Then
                            _items(i).Got = cdatb
                            i = _numberItems
                        End If
                    Next i
                End If
            Loop Until data = "!o"

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!p" Then
                    scp = InStr(data, ";")
                    scp2 = InStr(scp + 1, data, ";")
                    scp3 = InStr(scp2 + 1, data, ";")

                    name = Trim(Left(data, scp - 1))
                    cdatb = IsYes(Mid(data, scp + 1, (scp2 - scp) - 1))
                    visible = IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1))
                    room = Trim(Mid(data, scp3 + 1))

                    For i = 1 To _numberObjs
                        If _objs(i).ObjectName = name And Not _objs(i).Loaded Then
                            _objs(i).Exists = cdatb
                            _objs(i).Visible = visible
                            _objs(i).ContainerRoom = room
                            _objs(i).Loaded = True
                            i = _numberObjs
                        End If
                    Next i
                End If
            Loop Until data = "!p"

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!s" Then
                    scp = InStr(data, ";")
                    scp2 = InStr(scp + 1, data, ";")
                    scp3 = InStr(scp2 + 1, data, ";")

                    name = Trim(Left(data, scp - 1))
                    cdatb = IsYes(Mid(data, scp + 1, (scp2 - scp) - 1))
                    visible = IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1))
                    room = Trim(Mid(data, scp3 + 1))

                    For i = 1 To _numberChars
                        If _chars(i).ObjectName = name Then
                            _chars(i).Exists = cdatb
                            _chars(i).Visible = visible
                            _chars(i).ContainerRoom = room
                            i = _numberChars
                        End If
                    Next i
                End If
            Loop Until data = "!s"

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!n" Then
                    scp = InStr(data, ";")
                    name = Trim(Left(data, scp - 1))
                    data = Right(data, Len(data) - scp)

                    SetStringContents(name, data, _nullContext)
                End If
            Loop Until data = "!n"

            Do
                data = lines(lineNumber)
                lineNumber += 1
                If data <> "!e" Then
                    scp = InStr(data, ";")
                    name = Trim(Left(data, scp - 1))
                    data = Right(data, Len(data) - scp)

                    SetNumericVariableContents(name, Val(data), _nullContext)
                End If
            Loop Until data = "!e"

        End If

        _saveGameFile = filename

        Return True
    End Function

    Private Function SaveGame(filename As String, Optional saveFile As Boolean = True) As Byte()
        Dim ctx As Context = New Context()
        Dim saveData As String

        If _gameAslVersion >= 391 Then ExecuteScript(_beforeSaveScript, ctx)

        If _gameAslVersion >= 280 Then
            saveData = MakeRestoreData()
        Else
            saveData = MakeRestoreDataV2()
        End If

        If saveFile Then
            System.IO.File.WriteAllText(filename, saveData, System.Text.Encoding.GetEncoding(1252))
        End If

        _saveGameFile = filename

        Return System.Text.Encoding.GetEncoding(1252).GetBytes(saveData)
    End Function

    Private Function MakeRestoreDataV2() As String
        Dim lines As New List(Of String)
        Dim i As Integer

        lines.Add("QUEST200.1")
        lines.Add(GetOriginalFilenameForQSG)
        lines.Add(_gameName)
        lines.Add(_currentRoom)

        lines.Add("!c")
        For i = 1 To _numCollectables
            lines.Add(_collectables(i).Name & ";" & Str(_collectables(i).Value))
        Next i

        lines.Add("!i")
        For i = 1 To _numberItems
            lines.Add(_items(i).Name & ";" & YesNo(_items(i).Got))
        Next i

        lines.Add("!o")
        For i = 1 To _numberObjs
            lines.Add(_objs(i).ObjectName & ";" & YesNo(_objs(i).Exists) & ";" & YesNo(_objs(i).Visible) & ";" & _objs(i).ContainerRoom)
        Next i

        lines.Add("!p")
        For i = 1 To _numberChars
            lines.Add(_chars(i).ObjectName & ";" & YesNo(_chars(i).Exists) & ";" & YesNo(_chars(i).Visible) & ";" & _chars(i).ContainerRoom)
        Next i

        lines.Add("!s")
        For i = 1 To _numberStringVariables
            lines.Add(_stringVariable(i).VariableName & ";" & _stringVariable(i).VariableContents(0))
        Next i

        lines.Add("!n")
        For i = 1 To _numberNumericVariables
            lines.Add(_numericVariable(i).VariableName & ";" & Str(CDbl(_numericVariable(i).VariableContents(0))))
        Next i

        lines.Add("!e")

        Return String.Join(vbCrLf, lines)
    End Function

    Private Sub SetAvailability(thingString As String, exists As Boolean, ctx As Context, Optional type As Thing = Thing.Object)
        ' Sets availability of objects (and characters in ASL<281)

        If _gameAslVersion >= 281 Then
            Dim found = False
            For i = 1 To _numberObjs
                If LCase(_objs(i).ObjectName) = LCase(thingString) Then
                    _objs(i).Exists = exists
                    If exists Then
                        AddToObjectProperties("not hidden", i, ctx)
                    Else
                        AddToObjectProperties("hidden", i, ctx)
                    End If
                    found = True
                    Exit For
                End If
            Next i

            If Not found Then
                LogASLError("Not found object '" & thingString & "'", LogType.WarningError)
            End If
        Else
            ' split ThingString into character name and room
            ' (thingstring of form name@room)

            Dim room, name As String

            Dim atPos = InStr(thingString, "@")
            ' If no room specified, currentroom presumed
            If atPos = 0 Then
                room = _currentRoom
                name = thingString
            Else
                name = Trim(Left(thingString, atPos - 1))
                room = Trim(Right(thingString, Len(thingString) - atPos))
            End If
            If type = Thing.Character Then
                For i = 1 To _numberChars
                    If LCase(_chars(i).ContainerRoom) = LCase(room) And LCase(_chars(i).ObjectName) = LCase(name) Then
                        _chars(i).Exists = exists
                        Exit For
                    End If
                Next i
            ElseIf type = Thing.Object Then
                For i = 1 To _numberObjs
                    If LCase(_objs(i).ContainerRoom) = LCase(room) And LCase(_objs(i).ObjectName) = LCase(name) Then
                        _objs(i).Exists = exists
                        Exit For
                    End If
                Next i
            End If
        End If

        UpdateItems(ctx)
        UpdateObjectList(ctx)
    End Sub

    Friend Sub SetStringContents(name As String, value As String, ctx As Context, Optional arrayIndex As Integer = 0)
        Dim id As Integer
        Dim exists = False

        If name = "" Then
            LogASLError("Internal error - tried to set empty string name to '" & value & "'", LogType.WarningError)
            Exit Sub
        End If

        If _gameAslVersion >= 281 Then
            Dim bp = InStr(name, "[")
            If bp <> 0 Then
                arrayIndex = GetArrayIndex(name, ctx)
                name = Left(name, bp - 1)
            End If
        End If

        If arrayIndex < 0 Then
            LogASLError("'" & name & "[" & Trim(Str(arrayIndex)) & "]' is invalid - did not assign to array", LogType.WarningError)
            Exit Sub
        End If

        ' First, see if the string already exists. If it does,
        ' modify it. If not, create it.

        If _numberStringVariables > 0 Then
            For i = 1 To _numberStringVariables
                If LCase(_stringVariable(i).VariableName) = LCase(name) Then
                    id = i
                    exists = True
                    Exit For
                End If
            Next i
        End If

        If Not exists Then
            _numberStringVariables = _numberStringVariables + 1
            id = _numberStringVariables
            ReDim Preserve _stringVariable(id)
            _stringVariable(id) = New VariableType
            _stringVariable(id).VariableUBound = arrayIndex
        End If

        If arrayIndex > _stringVariable(id).VariableUBound Then
            ReDim Preserve _stringVariable(id).VariableContents(arrayIndex)
            _stringVariable(id).VariableUBound = arrayIndex
        End If

        ' Now, set the contents
        _stringVariable(id).VariableName = name
        ReDim Preserve _stringVariable(id).VariableContents(_stringVariable(id).VariableUBound)
        _stringVariable(id).VariableContents(arrayIndex) = value

        If _stringVariable(id).OnChangeScript <> "" Then
            Dim script = _stringVariable(id).OnChangeScript
            ExecuteScript(script, ctx)
        End If

        If _stringVariable(id).DisplayString <> "" Then
            UpdateStatusVars(ctx)
        End If
    End Sub

    Private Sub SetUpCharObjectInfo()
        Dim defaultProperties As New PropertiesActions

        _numberChars = 0

        ' see if define type <default> exists:
        Dim defaultExists = False
        For i = 1 To _numberSections
            If Trim(_lines(_defineBlocks(i).StartLine)) = "define type <default>" Then
                defaultExists = True
                defaultProperties = GetPropertiesInType("default")
                Exit For
            End If
        Next i

        For i = 1 To _numberSections
            Dim block = _defineBlocks(i)
            If Not (BeginsWith(_lines(block.StartLine), "define room") Or BeginsWith(_lines(block.StartLine), "define game") Or BeginsWith(_lines(block.StartLine), "define object ")) Then
                Continue For
            End If

            Dim restOfLine As String
            Dim origContainerRoomName, containerRoomName As String

            If BeginsWith(_lines(block.StartLine), "define room") Then
                origContainerRoomName = GetParameter(_lines(block.StartLine), _nullContext)
            Else
                origContainerRoomName = ""
            End If

            Dim startLine As Integer = block.StartLine
            Dim endLine As Integer = block.EndLine

            If BeginsWith(_lines(block.StartLine), "define object ") Then
                startLine = startLine - 1
                endLine = endLine + 1
            End If

            For j = startLine + 1 To endLine - 1
                If BeginsWith(_lines(j), "define object") Then
                    containerRoomName = origContainerRoomName

                    _numberObjs = _numberObjs + 1
                    ReDim Preserve _objs(_numberObjs)
                    _objs(_numberObjs) = New ObjectType

                    Dim o = _objs(_numberObjs)

                    o.ObjectName = GetParameter(_lines(j), _nullContext)
                    o.ObjectAlias = o.ObjectName
                    o.DefinitionSectionStart = j
                    o.ContainerRoom = containerRoomName
                    o.Visible = True
                    o.Gender = "it"
                    o.Article = "it"

                    o.Take.Type = TextActionType.Nothing

                    If defaultExists Then
                        AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext)
                        For k = 1 To defaultProperties.NumberActions
                            AddObjectAction(_numberObjs, defaultProperties.Actions(k).ActionName, defaultProperties.Actions(k).Script)
                        Next k
                    End If

                    If _gameAslVersion >= 391 Then AddToObjectProperties("list", _numberObjs, _nullContext)

                    Dim hidden = False
                    Do
                        j = j + 1
                        If Trim(_lines(j)) = "hidden" Then
                            o.Exists = False
                            hidden = True
                            If _gameAslVersion >= 311 Then AddToObjectProperties("hidden", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "startin ") And containerRoomName = "__UNKNOWN" Then
                            containerRoomName = GetParameter(_lines(j), _nullContext)
                        ElseIf BeginsWith(_lines(j), "prefix ") Then
                            o.Prefix = GetParameter(_lines(j), _nullContext) & " "
                            If _gameAslVersion >= 311 Then AddToObjectProperties("prefix=" & o.Prefix, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "suffix ") Then
                            o.Suffix = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("suffix=" & o.Suffix, _numberObjs, _nullContext)
                        ElseIf Trim(_lines(j)) = "invisible" Then
                            o.Visible = False
                            If _gameAslVersion >= 311 Then AddToObjectProperties("invisible", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "alias ") Then
                            o.ObjectAlias = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("alias=" & o.ObjectAlias, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "alt ") Then
                            AddToObjectAltNames(GetParameter(_lines(j), _nullContext), _numberObjs)
                        ElseIf BeginsWith(_lines(j), "detail ") Then
                            o.Detail = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("detail=" & o.Detail, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "gender ") Then
                            o.Gender = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("gender=" & o.Gender, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "article ") Then
                            o.Article = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("article=" & o.Article, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "gain ") Then
                            o.GainScript = GetEverythingAfter(_lines(j), "gain ")
                            AddObjectAction(_numberObjs, "gain", o.GainScript)
                        ElseIf BeginsWith(_lines(j), "lose ") Then
                            o.LoseScript = GetEverythingAfter(_lines(j), "lose ")
                            AddObjectAction(_numberObjs, "lose", o.LoseScript)
                        ElseIf BeginsWith(_lines(j), "displaytype ") Then
                            o.DisplayType = GetParameter(_lines(j), _nullContext)
                            If _gameAslVersion >= 311 Then AddToObjectProperties("displaytype=" & o.DisplayType, _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "look ") Then
                            If _gameAslVersion >= 311 Then
                                restOfLine = GetEverythingAfter(_lines(j), "look ")
                                If Left(restOfLine, 1) = "<" Then
                                    AddToObjectProperties("look=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                                Else
                                    AddObjectAction(_numberObjs, "look", restOfLine)
                                End If
                            End If
                        ElseIf BeginsWith(_lines(j), "examine ") Then
                            If _gameAslVersion >= 311 Then
                                restOfLine = GetEverythingAfter(_lines(j), "examine ")
                                If Left(restOfLine, 1) = "<" Then
                                    AddToObjectProperties("examine=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                                Else
                                    AddObjectAction(_numberObjs, "examine", restOfLine)
                                End If
                            End If
                        ElseIf _gameAslVersion >= 311 And BeginsWith(_lines(j), "speak ") Then
                            restOfLine = GetEverythingAfter(_lines(j), "speak ")
                            If Left(restOfLine, 1) = "<" Then
                                AddToObjectProperties("speak=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                AddObjectAction(_numberObjs, "speak", restOfLine)
                            End If
                        ElseIf BeginsWith(_lines(j), "properties ") Then
                            AddToObjectProperties(GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "type ") Then
                            o.NumberTypesIncluded = o.NumberTypesIncluded + 1
                            ReDim Preserve o.TypesIncluded(o.NumberTypesIncluded)
                            o.TypesIncluded(o.NumberTypesIncluded) = GetParameter(_lines(j), _nullContext)

                            Dim PropertyData = GetPropertiesInType(GetParameter(_lines(j), _nullContext))
                            AddToObjectProperties(PropertyData.Properties, _numberObjs, _nullContext)
                            For k = 1 To PropertyData.NumberActions
                                AddObjectAction(_numberObjs, PropertyData.Actions(k).ActionName, PropertyData.Actions(k).Script)
                            Next k

                            ReDim Preserve o.TypesIncluded(o.NumberTypesIncluded + PropertyData.NumberTypesIncluded)
                            For k = 1 To PropertyData.NumberTypesIncluded
                                o.TypesIncluded(k + o.NumberTypesIncluded) = PropertyData.TypesIncluded(k)
                            Next k
                            o.NumberTypesIncluded = o.NumberTypesIncluded + PropertyData.NumberTypesIncluded
                        ElseIf BeginsWith(_lines(j), "action ") Then
                            AddToObjectActions(GetEverythingAfter(_lines(j), "action "), _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "use ") Then
                            AddToUseInfo(_numberObjs, GetEverythingAfter(_lines(j), "use "))
                        ElseIf BeginsWith(_lines(j), "give ") Then
                            AddToGiveInfo(_numberObjs, GetEverythingAfter(_lines(j), "give "))
                        ElseIf Trim(_lines(j)) = "take" Then
                            o.Take.Type = TextActionType.Default
                            AddToObjectProperties("take", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "take ") Then
                            If Left(GetEverythingAfter(_lines(j), "take "), 1) = "<" Then
                                o.Take.Type = TextActionType.Text
                                o.Take.Data = GetParameter(_lines(j), _nullContext)

                                AddToObjectProperties("take=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                o.Take.Type = TextActionType.Script
                                restOfLine = GetEverythingAfter(_lines(j), "take ")
                                o.Take.Data = restOfLine

                                AddObjectAction(_numberObjs, "take", restOfLine)
                            End If
                        ElseIf Trim(_lines(j)) = "container" Then
                            If _gameAslVersion >= 391 Then AddToObjectProperties("container", _numberObjs, _nullContext)
                        ElseIf Trim(_lines(j)) = "surface" Then
                            If _gameAslVersion >= 391 Then
                                AddToObjectProperties("container", _numberObjs, _nullContext)
                                AddToObjectProperties("surface", _numberObjs, _nullContext)
                            End If
                        ElseIf Trim(_lines(j)) = "opened" Then
                            If _gameAslVersion >= 391 Then AddToObjectProperties("opened", _numberObjs, _nullContext)
                        ElseIf Trim(_lines(j)) = "transparent" Then
                            If _gameAslVersion >= 391 Then AddToObjectProperties("transparent", _numberObjs, _nullContext)
                        ElseIf Trim(_lines(j)) = "open" Then
                            AddToObjectProperties("open", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "open ") Then
                            If Left(GetEverythingAfter(_lines(j), "open "), 1) = "<" Then
                                AddToObjectProperties("open=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                restOfLine = GetEverythingAfter(_lines(j), "open ")
                                AddObjectAction(_numberObjs, "open", restOfLine)
                            End If
                        ElseIf Trim(_lines(j)) = "close" Then
                            AddToObjectProperties("close", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "close ") Then
                            If Left(GetEverythingAfter(_lines(j), "close "), 1) = "<" Then
                                AddToObjectProperties("close=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                restOfLine = GetEverythingAfter(_lines(j), "close ")
                                AddObjectAction(_numberObjs, "close", restOfLine)
                            End If
                        ElseIf Trim(_lines(j)) = "add" Then
                            AddToObjectProperties("add", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "add ") Then
                            If Left(GetEverythingAfter(_lines(j), "add "), 1) = "<" Then
                                AddToObjectProperties("add=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                restOfLine = GetEverythingAfter(_lines(j), "add ")
                                AddObjectAction(_numberObjs, "add", restOfLine)
                            End If
                        ElseIf Trim(_lines(j)) = "remove" Then
                            AddToObjectProperties("remove", _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "remove ") Then
                            If Left(GetEverythingAfter(_lines(j), "remove "), 1) = "<" Then
                                AddToObjectProperties("remove=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                            Else
                                restOfLine = GetEverythingAfter(_lines(j), "remove ")
                                AddObjectAction(_numberObjs, "remove", restOfLine)
                            End If
                        ElseIf BeginsWith(_lines(j), "parent ") Then
                            AddToObjectProperties("parent=" & GetParameter(_lines(j), _nullContext), _numberObjs, _nullContext)
                        ElseIf BeginsWith(_lines(j), "list") Then
                            ProcessListInfo(_lines(j), _numberObjs)
                        End If

                    Loop Until Trim(_lines(j)) = "end define"

                    o.DefinitionSectionEnd = j
                    If Not hidden Then o.Exists = True
                ElseIf _gameAslVersion <= 280 And BeginsWith(_lines(j), "define character") Then
                    containerRoomName = origContainerRoomName
                    _numberChars = _numberChars + 1
                    ReDim Preserve _chars(_numberChars)
                    _chars(_numberChars) = New ObjectType
                    _chars(_numberChars).ObjectName = GetParameter(_lines(j), _nullContext)
                    _chars(_numberChars).DefinitionSectionStart = j
                    _chars(_numberChars).ContainerRoom = ""
                    _chars(_numberChars).Visible = True
                    Dim hidden = False
                    Do
                        j = j + 1
                        If Trim(_lines(j)) = "hidden" Then
                            _chars(_numberChars).Exists = False
                            hidden = True
                        ElseIf BeginsWith(_lines(j), "startin ") And containerRoomName = "__UNKNOWN" Then
                            containerRoomName = GetParameter(_lines(j), _nullContext)
                        ElseIf BeginsWith(_lines(j), "prefix ") Then
                            _chars(_numberChars).Prefix = GetParameter(_lines(j), _nullContext) & " "
                        ElseIf BeginsWith(_lines(j), "suffix ") Then
                            _chars(_numberChars).Suffix = " " & GetParameter(_lines(j), _nullContext)
                        ElseIf Trim(_lines(j)) = "invisible" Then
                            _chars(_numberChars).Visible = False
                        ElseIf BeginsWith(_lines(j), "alias ") Then
                            _chars(_numberChars).ObjectAlias = GetParameter(_lines(j), _nullContext)
                        ElseIf BeginsWith(_lines(j), "detail ") Then
                            _chars(_numberChars).Detail = GetParameter(_lines(j), _nullContext)
                        End If

                        _chars(_numberChars).ContainerRoom = containerRoomName

                    Loop Until Trim(_lines(j)) = "end define"

                    _chars(_numberChars).DefinitionSectionEnd = j
                    If Not hidden Then _chars(_numberChars).Exists = True
                End If
            Next j
        Next i

        UpdateVisibilityInContainers(_nullContext)
    End Sub

    Private Sub ShowGameAbout(ctx As Context)
        Dim version = FindStatement(GetDefineBlock("game"), "game version")
        Dim author = FindStatement(GetDefineBlock("game"), "game author")
        Dim copyright = FindStatement(GetDefineBlock("game"), "game copyright")
        Dim info = FindStatement(GetDefineBlock("game"), "game info")

        Print("|bGame name:|cl  " & _gameName & "|cb|xb", ctx)
        If version <> "" Then Print("|bVersion:|xb    " & version, ctx)
        If author <> "" Then Print("|bAuthor:|xb     " & author, ctx)
        If copyright <> "" Then Print("|bCopyright:|xb  " & copyright, ctx)

        If info <> "" Then
            Print("", ctx)
            Print(info, ctx)
        End If
    End Sub

    Private Sub ShowPicture(filename As String)
        ' In Quest 4.x this function would be used for showing a picture in a popup window, but
        ' this is no longer supported - ALL images are displayed in-line with the game text. Any
        ' image caption is displayed as text, and any image size specified is ignored.

        Dim caption As String = ""

        If InStr(filename, ";") <> 0 Then
            caption = Trim(Mid(filename, InStr(filename, ";") + 1))
            filename = Trim(Left(filename, InStr(filename, ";") - 1))
        End If

        If InStr(filename, "@") <> 0 Then
            ' size is ignored
            filename = Trim(Left(filename, InStr(filename, "@") - 1))
        End If

        If caption.Length > 0 Then Print(caption, _nullContext)

        ShowPictureInText(filename)
    End Sub

    Private Sub ShowRoomInfo(room As String, ctx As Context, Optional noPrint As Boolean = False)
        If _gameAslVersion < 280 Then
            ShowRoomInfoV2(room)
            Exit Sub
        End If

        Dim roomDisplayText As String = ""
        Dim descTagExist As Boolean
        Dim doorwayString, roomAlias As String
        Dim finishedFindingCommas As Boolean
        Dim prefix, roomDisplayName As String
        Dim roomDisplayNameNoFormat, inDescription As String
        Dim visibleObjects As String = ""
        Dim visibleObjectsNoFormat As String
        Dim placeList As String
        Dim lastComma, oldLastComma As Integer
        Dim descType As Integer
        Dim descLine As String = ""
        Dim showLookText As Boolean
        Dim lookDesc As String = ""
        Dim objLook As String
        Dim objSuffix As String

        Dim gameBlock = GetDefineBlock("game")

        _currentRoom = room
        Dim id = GetRoomID(_currentRoom, ctx)

        If id = 0 Then Exit Sub

        ' FIRST LINE - YOU ARE IN... ***********************************************

        roomAlias = _rooms(id).RoomAlias
        If roomAlias = "" Then roomAlias = _rooms(id).RoomName

        prefix = _rooms(id).Prefix

        If prefix = "" Then
            roomDisplayName = "|cr" & roomAlias & "|cb"
            roomDisplayNameNoFormat = roomAlias ' No formatting version, for label
        Else
            roomDisplayName = prefix & " |cr" & roomAlias & "|cb"
            roomDisplayNameNoFormat = prefix & " " & roomAlias
        End If

        inDescription = _rooms(id).InDescription

        If inDescription <> "" Then
            ' Print player's location according to indescription:
            If Right(inDescription, 1) = ":" Then
                ' if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText & Left(inDescription, Len(inDescription) - 1) & " " & roomDisplayName & "." & vbCrLf
            Else
                ' otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText & inDescription & vbCrLf
            End If
        Else
            ' if no indescription line, print the default.
            roomDisplayText = roomDisplayText & "You are in " & roomDisplayName & "." & vbCrLf
        End If

        _player.LocationUpdated(UCase(Left(roomAlias, 1)) & Mid(roomAlias, 2))

        SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx)

        ' SHOW OBJECTS *************************************************************

        visibleObjectsNoFormat = ""

        Dim visibleObjectsList As New List(Of Integer) ' of object IDs
        Dim count As Integer

        For i = 1 To _numberObjs
            If LCase(_objs(i).ContainerRoom) = LCase(room) And _objs(i).Exists And _objs(i).Visible And Not _objs(i).IsExit Then
                visibleObjectsList.Add(i)
            End If
        Next i

        For Each objId As Integer In visibleObjectsList
            objSuffix = _objs(objId).Suffix
            If objSuffix <> "" Then objSuffix = " " & objSuffix

            If _objs(objId).ObjectAlias = "" Then
                visibleObjects = visibleObjects & _objs(objId).Prefix & "|b" & _objs(objId).ObjectName & "|xb" & objSuffix
                visibleObjectsNoFormat = visibleObjectsNoFormat & _objs(objId).Prefix & _objs(objId).ObjectName
            Else
                visibleObjects = visibleObjects & _objs(objId).Prefix & "|b" & _objs(objId).ObjectAlias & "|xb" & objSuffix
                visibleObjectsNoFormat = visibleObjectsNoFormat & _objs(objId).Prefix & _objs(objId).ObjectAlias
            End If

            count = count + 1
            If count < visibleObjectsList.Count() - 1 Then
                visibleObjects = visibleObjects & ", "
                visibleObjectsNoFormat = visibleObjectsNoFormat & ", "
            ElseIf count = visibleObjectsList.Count() - 1 Then
                visibleObjects = visibleObjects & " and "
                visibleObjectsNoFormat = visibleObjectsNoFormat & ", "
            End If
        Next

        If visibleObjectsList.Count() > 0 Then
            SetStringContents("quest.formatobjects", visibleObjects, ctx)
            visibleObjects = "There is " & visibleObjects & " here."
            SetStringContents("quest.objects", visibleObjectsNoFormat, ctx)
            roomDisplayText = roomDisplayText & visibleObjects & vbCrLf
        Else
            SetStringContents("quest.objects", "", ctx)
            SetStringContents("quest.formatobjects", "", ctx)
        End If

        ' SHOW EXITS ***************************************************************

        doorwayString = UpdateDoorways(id, ctx)

        If _gameAslVersion < 410 Then
            placeList = GetGoToExits(id, ctx)

            If placeList <> "" Then
                'strip final comma
                placeList = Left(placeList, Len(placeList) - 2)

                'if there is still a comma here, there is more than
                'one place, so add the word "or" before the last one.
                If InStr(placeList, ",") > 0 Then
                    lastComma = 0
                    finishedFindingCommas = False
                    Do
                        oldLastComma = lastComma
                        lastComma = InStr(lastComma + 1, placeList, ",")
                        If lastComma = 0 Then
                            finishedFindingCommas = True
                            lastComma = oldLastComma
                        End If
                    Loop Until finishedFindingCommas

                    placeList = Left(placeList, lastComma - 1) & " or" & Right(placeList, Len(placeList) - lastComma)
                End If

                roomDisplayText = roomDisplayText & "You can go to " & placeList & "." & vbCrLf
                SetStringContents("quest.doorways.places", placeList, ctx)
            Else
                SetStringContents("quest.doorways.places", "", ctx)
            End If
        End If

        ' GET "LOOK" DESCRIPTION (but don't print it yet) **************************

        objLook = GetObjectProperty("look", _rooms(id).ObjId, , False)

        If objLook = "" Then
            If _rooms(id).Look <> "" Then
                lookDesc = _rooms(id).Look
            End If
        Else
            lookDesc = objLook
        End If

        SetStringContents("quest.lookdesc", lookDesc, ctx)


        ' FIND DESCRIPTION TAG, OR ACTION ******************************************

        ' In Quest versions prior to 3.1, with any custom description, the "look"
        ' text was always displayed after the "description" tag was printed/executed.
        ' In Quest 3.1 and later, it isn't - descriptions should print the look
        ' tag themselves when and where necessary.

        showLookText = True

        If _rooms(id).Description.Data <> "" Then
            descLine = _rooms(id).Description.Data
            descType = _rooms(id).Description.Type
            descTagExist = True
        Else
            descTagExist = False
        End If

        If descTagExist = False Then
            'Look in the "define game" block:
            For i = gameBlock.StartLine + 1 To gameBlock.EndLine - 1
                If BeginsWith(_lines(i), "description ") Then
                    descLine = GetEverythingAfter(_lines(i), "description ")
                    descTagExist = True
                    If Left(descLine, 1) = "<" Then
                        descLine = GetParameter(descLine, ctx)
                        descType = TextActionType.Text
                    Else
                        descType = TextActionType.Script
                    End If
                    i = gameBlock.EndLine
                End If
            Next i
        End If

        If descTagExist And _gameAslVersion >= 310 Then
            showLookText = False
        End If

        If Not noPrint Then
            If descTagExist = False Then
                'Remove final vbCrLf:
                roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2)
                Print(roomDisplayText, ctx)
                If doorwayString <> "" Then Print(doorwayString, ctx)
            Else
                'execute description tag:
                'If no script, just print the tag's parameter.
                'Otherwise, execute it as ASL script:

                If descType = TextActionType.Text Then
                    Print(descLine, ctx)
                Else
                    ExecuteScript(descLine, ctx)
                End If
            End If

            UpdateObjectList(ctx)

            ' SHOW "LOOK" DESCRIPTION **************************************************

            If showLookText Then
                If lookDesc <> "" Then
                    Print(lookDesc, ctx)
                End If
            End If
        End If

    End Sub

    Private Sub CheckCollectable(ColNum As Integer)
        ' Checks to see whether a collectable item has exceeded
        ' its range - if so, it resets the number to the nearest
        ' valid number. It's a handy quick way of making sure that
        ' a player's health doesn't reach 101%, for example.

        Dim maxn, n, minn As Double
        Dim M As Integer
        Dim T As String

        T = _collectables(ColNum).Type
        n = _collectables(ColNum).Value

        If T = "%" And n > 100 Then n = 100
        If (T = "%" Or T = "p") And n < 0 Then n = 0
        If InStr(T, "r") > 0 Then
            If InStr(T, "r") = 1 Then
                maxn = Val(Mid(T, Len(T) - 1))
                M = 1
            ElseIf InStr(T, "r") = Len(T) Then
                minn = Val(Left(T, Len(T) - 1))
                M = 2
            Else
                minn = Val(Left(T, InStr(T, "r") - 1))
                maxn = Val(Mid(T, InStr(T, "r") + 1))
                M = 3
            End If

            If (M = 1 Or M = 3) And n > maxn Then n = maxn
            If (M = 2 Or M = 3) And n < minn Then n = minn
        End If

        _collectables(ColNum).Value = n

    End Sub

    Private Function DisplayCollectableInfo(ColNum As Integer) As String
        Dim FirstBit, D, NextBit As String
        Dim ExcPos As Integer
        Dim FirstStarPos, SecondStarPos As Integer
        Dim AfterStar, BeforeStar, BetweenStar As String

        If _collectables(ColNum).Display = "<def>" Then
            D = "You have " & Trim(Str(_collectables(ColNum).Value)) & " " & _collectables(ColNum).Name
        ElseIf _collectables(ColNum).Display = "" Then
            D = "<null>"
        Else
            ExcPos = InStr(_collectables(ColNum).Display, "!")
            If ExcPos = 0 Then
                D = _collectables(ColNum).Display
            Else
                FirstBit = Left(_collectables(ColNum).Display, ExcPos - 1)
                NextBit = Right(_collectables(ColNum).Display, Len(_collectables(ColNum).Display) - ExcPos)
                D = FirstBit & Trim(Str(_collectables(ColNum).Value)) & NextBit
            End If

            If InStr(D, "*") > 0 Then
                FirstStarPos = InStr(D, "*")
                SecondStarPos = InStr(FirstStarPos + 1, D, "*")
                BeforeStar = Left(D, FirstStarPos - 1)
                AfterStar = Mid(D, SecondStarPos + 1)
                BetweenStar = Mid(D, FirstStarPos + 1, (SecondStarPos - FirstStarPos) - 1)

                If _collectables(ColNum).Value <> 1 Then
                    D = BeforeStar & BetweenStar & AfterStar
                Else
                    D = BeforeStar & AfterStar
                End If
            End If
        End If

        If _collectables(ColNum).Value = 0 And _collectables(ColNum).DisplayWhenZero = False Then
            D = "<null>"
        End If

        Return D
    End Function

    Private Sub DisplayTextSection(sectionname As String, ctx As Context, Optional OutputTo As String = "normal")
        Dim textblock As DefineBlock
        textblock = DefineBlockParam("text", sectionname)
        Dim i As Integer

        If textblock.StartLine <> 0 Then
            For i = textblock.StartLine + 1 To textblock.EndLine - 1
                If _gameAslVersion >= 392 Then
                    ' Convert string variables etc.
                    Print(GetParameter("<" & _lines(i) & ">", ctx), ctx, OutputTo)
                Else
                    Print(_lines(i), ctx, OutputTo)
                End If
            Next i

            Print("", ctx)
        End If
    End Sub

    ' Returns true if the system is ready to process a new command after completion - so it will be
    ' in most cases, except when ExecCommand just caused an "enter" script command to complete

    Private Function ExecCommand(thecommand As String, ctx As Context, Optional EchoCommand As Boolean = True, Optional RunUserCommand As Boolean = True, Optional DontSetIt As Boolean = False) As Boolean
        Dim cmd As String
        Dim EnteredHelpCommand As Boolean
        Dim RoomID As Integer
        Dim GlobalOverride As Boolean
        Dim OldBadCmdBefore As String
        Dim CP, i, n As Integer
        Dim SkipAfterTurn As Boolean
        Dim G, D, c, P, T As String
        Dim j As Integer

        SkipAfterTurn = False
        thecommand = RemoveFormatting(thecommand)

        OldBadCmdBefore = _badCmdBefore

        RoomID = GetRoomID(_currentRoom, ctx)
        EnteredHelpCommand = False

        If thecommand = "" Then Return True

        cmd = LCase(thecommand)

        SyncLock _commandLock
            If _commandOverrideModeOn Then
                ' Commands have been overridden for this command,
                ' so put input into previously specified variable
                ' and exit:

                SetStringContents(_commandOverrideVariable, thecommand, ctx)
                System.Threading.Monitor.PulseAll(_commandLock)
                Return False
            End If
        End SyncLock

        Dim UserCommandReturn As Boolean
        Dim newcommand As String

        If EchoCommand = True Then
            Print("> " & thecommand, ctx, , True)
        End If

        thecommand = LCase(thecommand)

        SetStringContents("quest.originalcommand", thecommand, ctx)

        newcommand = " " & thecommand & " "

        ' Convert synonyms:
        For i = 1 To _numberSynonyms
            CP = 1
            Do
                n = InStr(CP, newcommand, " " & _synonyms(i).OriginalWord & " ")
                If n <> 0 Then
                    newcommand = Left(newcommand, n - 1) & " " & _synonyms(i).ConvertTo & " " & Mid(newcommand, n + Len(_synonyms(i).OriginalWord) + 2)
                    CP = n + 1
                End If
            Loop Until n = 0
        Next i

        'strip starting and ending spaces
        thecommand = Mid(newcommand, 2, Len(newcommand) - 2)

        SetStringContents("quest.command", thecommand, ctx)

        ' Execute any "beforeturn" script:

        Dim NewThread As Context = CopyContext(ctx)
        GlobalOverride = False

        ' RoomID is 0 if there are no rooms in the game. Unlikely, but we get an RTE otherwise.
        If RoomID <> 0 Then
            If _rooms(RoomID).BeforeTurnScript <> "" Then
                If BeginsWith(_rooms(RoomID).BeforeTurnScript, "override") Then
                    ExecuteScript(GetEverythingAfter(_rooms(RoomID).BeforeTurnScript, "override"), NewThread)
                    GlobalOverride = True
                Else
                    ExecuteScript(_rooms(RoomID).BeforeTurnScript, NewThread)
                End If
            End If
        End If
        If _beforeTurnScript <> "" And GlobalOverride = False Then ExecuteScript(_beforeTurnScript, NewThread)

        ' In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        ' in which case we don't process the command.

        If Not NewThread.DontProcessCommand Then
            'Try to execute user defined command, if allowed:

            UserCommandReturn = False
            If RunUserCommand = True Then
                UserCommandReturn = ExecUserCommand(thecommand, ctx)

                If Not UserCommandReturn Then
                    UserCommandReturn = ExecVerb(thecommand, ctx)
                End If

                If Not UserCommandReturn Then
                    ' Try command defined by a library
                    UserCommandReturn = ExecUserCommand(thecommand, ctx, True)
                End If

                If Not UserCommandReturn Then
                    ' Try verb defined by a library
                    UserCommandReturn = ExecVerb(thecommand, ctx, True)
                End If
            End If

            thecommand = LCase(thecommand)
        Else
            ' Set the UserCommand flag to fudge not processing any more commands
            UserCommandReturn = True
        End If

        Dim InvList As String = "", InventoryPlace As String
        Dim LastComma As Integer
        Dim ThisComma, CurPos As Integer
        If UserCommandReturn = False Then

            If CmdStartsWith(thecommand, "speak to ") Then
                c = GetEverythingAfter(thecommand, "speak to ")
                ExecSpeak(c, ctx)
            ElseIf CmdStartsWith(thecommand, "talk to ") Then
                c = GetEverythingAfter(thecommand, "talk to ")
                ExecSpeak(c, ctx)
            ElseIf cmd = "exit" Or cmd = "out" Or cmd = "leave" Then
                LeaveRoom(ctx)
                _lastIt = 0
            ElseIf cmd = "north" Or cmd = "south" Or cmd = "east" Or cmd = "west" Then
                GoDirection(thecommand, ctx)
                _lastIt = 0
            ElseIf cmd = "n" Or cmd = "s" Or cmd = "w" Or cmd = "e" Then
                Select Case InStr("nswe", cmd)
                    Case 1
                        GoDirection("north", ctx)
                    Case 2
                        GoDirection("south", ctx)
                    Case 3
                        GoDirection("west", ctx)
                    Case 4
                        GoDirection("east", ctx)
                End Select
                _lastIt = 0
            ElseIf cmd = "ne" Or cmd = "northeast" Or cmd = "north-east" Or cmd = "north east" Or cmd = "go ne" Or cmd = "go northeast" Or cmd = "go north-east" Or cmd = "go north east" Then
                GoDirection("northeast", ctx)
                _lastIt = 0
            ElseIf cmd = "nw" Or cmd = "northwest" Or cmd = "north-west" Or cmd = "north west" Or cmd = "go nw" Or cmd = "go northwest" Or cmd = "go north-west" Or cmd = "go north west" Then
                GoDirection("northwest", ctx)
                _lastIt = 0
            ElseIf cmd = "se" Or cmd = "southeast" Or cmd = "south-east" Or cmd = "south east" Or cmd = "go se" Or cmd = "go southeast" Or cmd = "go south-east" Or cmd = "go south east" Then
                GoDirection("southeast", ctx)
                _lastIt = 0
            ElseIf cmd = "sw" Or cmd = "southwest" Or cmd = "south-west" Or cmd = "south west" Or cmd = "go sw" Or cmd = "go southwest" Or cmd = "go south-west" Or cmd = "go south west" Then
                GoDirection("southwest", ctx)
                _lastIt = 0
            ElseIf cmd = "up" Or cmd = "u" Then
                GoDirection("up", ctx)
                _lastIt = 0
            ElseIf cmd = "down" Or cmd = "d" Then
                GoDirection("down", ctx)
                _lastIt = 0
            ElseIf CmdStartsWith(thecommand, "go ") Then
                If _gameAslVersion >= 410 Then
                    _rooms(GetRoomID(_currentRoom, ctx)).Exits.ExecuteGo(thecommand, ctx)
                Else
                    D = GetEverythingAfter(thecommand, "go ")
                    If D = "out" Then
                        LeaveRoom(ctx)
                    ElseIf D = "north" Or D = "south" Or D = "east" Or D = "west" Or D = "up" Or D = "down" Then
                        GoDirection(D, ctx)
                    ElseIf BeginsWith(D, "to ") Then
                        P = GetEverythingAfter(D, "to ")
                        GoToPlace(P, ctx)
                    Else
                        PlayerErrorMessage(PlayerError.BadGo, ctx)
                    End If
                End If
                _lastIt = 0
            ElseIf CmdStartsWith(thecommand, "give ") Then
                G = GetEverythingAfter(thecommand, "give ")
                ExecGive(G, ctx)
            ElseIf CmdStartsWith(thecommand, "take ") Then
                T = GetEverythingAfter(thecommand, "take ")
                ExecTake(T, ctx)
            ElseIf CmdStartsWith(thecommand, "drop ") And _gameAslVersion >= 280 Then
                D = GetEverythingAfter(thecommand, "drop ")
                ExecDrop(D, ctx)
            ElseIf CmdStartsWith(thecommand, "get ") Then
                T = GetEverythingAfter(thecommand, "get ")
                ExecTake(T, ctx)
            ElseIf CmdStartsWith(thecommand, "pick up ") Then
                T = GetEverythingAfter(thecommand, "pick up ")
                ExecTake(T, ctx)
            ElseIf cmd = "pick it up" Or cmd = "pick them up" Or cmd = "pick this up" Or cmd = "pick that up" Or cmd = "pick these up" Or cmd = "pick those up" Or cmd = "pick him up" Or cmd = "pick her up" Then
                ExecTake(Mid(cmd, 6, InStr(7, cmd, " ") - 6), ctx)
            ElseIf CmdStartsWith(thecommand, "look ") Then
                ExecLook(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "l ") Then
                ExecLook("look " & GetEverythingAfter(thecommand, "l "), ctx)
            ElseIf CmdStartsWith(thecommand, "examine ") And _gameAslVersion >= 280 Then
                ExecExamine(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "x ") And _gameAslVersion >= 280 Then
                ExecExamine("examine " & GetEverythingAfter(thecommand, "x "), ctx)
            ElseIf cmd = "l" Or cmd = "look" Then
                ExecLook("look", ctx)
            ElseIf cmd = "x" Or cmd = "examine" Then
                ExecExamine("examine", ctx)
            ElseIf CmdStartsWith(thecommand, "use ") Then
                ExecUse(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "open ") And _gameAslVersion >= 391 Then
                ExecOpenClose(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "close ") And _gameAslVersion >= 391 Then
                ExecOpenClose(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "put ") And _gameAslVersion >= 391 Then
                ExecAddRemove(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "add ") And _gameAslVersion >= 391 Then
                ExecAddRemove(thecommand, ctx)
            ElseIf CmdStartsWith(thecommand, "remove ") And _gameAslVersion >= 391 Then
                ExecAddRemove(thecommand, ctx)
            ElseIf cmd = "save" Then
                _player.RequestSave(Nothing)
            ElseIf cmd = "quit" Then
                GameFinished()
            ElseIf BeginsWith(cmd, "help") Then
                ShowHelp(ctx)
                EnteredHelpCommand = True
            ElseIf cmd = "about" Then
                ShowGameAbout(ctx)
            ElseIf cmd = "clear" Then
                DoClear()
            ElseIf cmd = "debug" Then
                ' TO DO: This is temporary, would be better to have a log viewer built in to Player
                For Each logEntry As String In _log
                    Print(logEntry, ctx)
                Next
            ElseIf cmd = "inventory" Or cmd = "inv" Or cmd = "i" Then
                InventoryPlace = "inventory"

                If _gameAslVersion >= 280 Then
                    For i = 1 To _numberObjs
                        If _objs(i).ContainerRoom = InventoryPlace And _objs(i).Exists And _objs(i).Visible Then
                            InvList = InvList & _objs(i).Prefix

                            If _objs(i).ObjectAlias = "" Then
                                InvList = InvList & "|b" & _objs(i).ObjectName & "|xb"
                            Else
                                InvList = InvList & "|b" & _objs(i).ObjectAlias & "|xb"
                            End If

                            If _objs(i).Suffix <> "" Then
                                InvList = InvList & " " & _objs(i).Suffix
                            End If

                            InvList = InvList & ", "
                        End If
                    Next i
                Else
                    For j = 1 To _numberItems
                        If _items(j).Got = True Then
                            InvList = InvList & _items(j).Name & ", "
                        End If
                    Next j
                End If
                If InvList <> "" Then

                    InvList = Left(InvList, Len(InvList) - 2)
                    InvList = UCase(Left(InvList, 1)) & Mid(InvList, 2)

                    CurPos = 1
                    Do
                        ThisComma = InStr(CurPos, InvList, ",")
                        If ThisComma <> 0 Then
                            LastComma = ThisComma
                            CurPos = ThisComma + 1
                        End If
                    Loop Until ThisComma = 0
                    If LastComma <> 0 Then InvList = Left(InvList, LastComma - 1) & " and" & Mid(InvList, LastComma + 1)
                    Print("You are carrying:|n" & InvList & ".", ctx)
                Else
                    Print("You are not carrying anything.", ctx)
                End If
            ElseIf CmdStartsWith(thecommand, "oops ") Then
                ExecOops(GetEverythingAfter(thecommand, "oops "), ctx)
            ElseIf CmdStartsWith(thecommand, "the ") Then
                ExecOops(GetEverythingAfter(thecommand, "the "), ctx)
            Else
                PlayerErrorMessage(PlayerError.BadCommand, ctx)
            End If
        End If

        If Not SkipAfterTurn Then
            ' Execute any "afterturn" script:
            GlobalOverride = False

            If RoomID <> 0 Then
                If _rooms(RoomID).AfterTurnScript <> "" Then
                    If BeginsWith(_rooms(RoomID).AfterTurnScript, "override") Then
                        ExecuteScript(GetEverythingAfter(_rooms(RoomID).AfterTurnScript, "override"), ctx)
                        GlobalOverride = True
                    Else
                        ExecuteScript(_rooms(RoomID).AfterTurnScript, ctx)
                    End If
                End If
            End If

            ' was set to NullThread here for some reason
            If _afterTurnScript <> "" And GlobalOverride = False Then ExecuteScript(_afterTurnScript, ctx)
        End If

        Print("", ctx)

        If Not DontSetIt Then
            ' Use "DontSetIt" when we don't want "it" etc. to refer to the object used in this turn.
            ' This is used for e.g. auto-remove object from container when taking.
            _lastIt = _thisTurnIt
            _lastItMode = _thisTurnItMode
        End If
        If _badCmdBefore = OldBadCmdBefore Then _badCmdBefore = ""

        Return True
    End Function

    Private Function CmdStartsWith(sCommand As String, sCheck As String) As Boolean
        ' When we are checking user input in ExecCommand, we check things like whether
        ' the player entered something beginning with "put ". We need to trim what the player entered
        ' though, otherwise they might just type "put " and then we would try disambiguating an object
        ' called "".

        Return BeginsWith(Trim(sCommand), sCheck)
    End Function

    Private Sub ExecGive(GiveString As String, ctx As Context)
        Dim characterblock As DefineBlock
        Dim ObjArticle As String
        Dim ToLoc As Integer
        Dim ItemToGive, CharToGive As String
        Dim ObjectType As Thing
        Dim ItemID As Integer
        Dim InventoryPlace As String
        Dim NotGotItem As Boolean
        Dim GiveScript As String = ""
        Dim FoundGiveScript, FoundGiveToObject As Boolean
        Dim GiveToObjectID, i As Integer
        Dim ObjGender As String
        Dim RealName, ItemCheck As String
        Dim GiveLine As Integer

        ToLoc = InStr(GiveString, " to ")
        If ToLoc = 0 Then
            ToLoc = InStr(GiveString, " the ")
            If ToLoc = 0 Then
                PlayerErrorMessage(PlayerError.BadGive, ctx)
                Exit Sub
            Else
                ItemToGive = Trim(Mid(GiveString, ToLoc + 4, Len(GiveString) - (ToLoc + 2)))
                CharToGive = Trim(Mid(GiveString, 1, ToLoc))
            End If
        Else
            ItemToGive = Trim(Mid(GiveString, 1, ToLoc))
            CharToGive = Trim(Mid(GiveString, ToLoc + 3, Len(GiveString) - (ToLoc + 2)))
        End If

        If _gameAslVersion >= 281 Then
            ObjectType = Thing.Object
        Else
            ObjectType = Thing.Character
        End If

        ' First see if player has "ItemToGive":
        If _gameAslVersion >= 280 Then
            InventoryPlace = "inventory"

            ItemID = Disambiguate(ItemToGive, InventoryPlace, ctx)

            If ItemID = -1 Then
                PlayerErrorMessage(PlayerError.NoItem, ctx)
                _badCmdBefore = "give"
                _badCmdAfter = "to " & CharToGive
                Exit Sub
            ElseIf ItemID = -2 Then
                Exit Sub
            Else
                ObjArticle = _objs(ItemID).Article
            End If
        Else
            ' ASL2:
            NotGotItem = True

            For i = 1 To _numberItems
                If LCase(_items(i).Name) = LCase(ItemToGive) Then
                    If _items(i).Got = False Then
                        NotGotItem = True
                        i = _numberItems
                    Else
                        NotGotItem = False
                    End If
                End If
            Next i

            If NotGotItem = True Then
                PlayerErrorMessage(PlayerError.NoItem, ctx)
                Exit Sub
            Else
                ObjArticle = _objs(ItemID).Article
            End If
        End If

        If _gameAslVersion >= 281 Then
            FoundGiveScript = False
            FoundGiveToObject = False

            GiveToObjectID = Disambiguate(CharToGive, _currentRoom, ctx)
            If GiveToObjectID > 0 Then
                FoundGiveToObject = True
            End If

            If Not FoundGiveToObject Then
                If GiveToObjectID <> -2 Then PlayerErrorMessage(PlayerError.BadCharacter, ctx)
                _badCmdBefore = "give " & ItemToGive & " to"
                Exit Sub
            End If

            'Find appropriate give script ****
            'now, for "give a to b", we have
            'ItemID=a and GiveToObjectID=b

            Dim o = _objs(GiveToObjectID)

            For i = 1 To o.NumberGiveData
                If o.GiveData(i).GiveType = GiveType.GiveSomethingTo And LCase(o.GiveData(i).GiveObject) = LCase(_objs(ItemID).ObjectName) Then
                    FoundGiveScript = True
                    GiveScript = o.GiveData(i).GiveScript
                    i = o.NumberGiveData
                End If
            Next i

            If Not FoundGiveScript Then
                'check a for give to <b>:

                Dim g = _objs(ItemID)

                For i = 1 To g.NumberGiveData
                    If g.GiveData(i).GiveType = GiveType.GiveToSomething And LCase(g.GiveData(i).GiveObject) = LCase(_objs(GiveToObjectID).ObjectName) Then
                        FoundGiveScript = True
                        GiveScript = g.GiveData(i).GiveScript
                        i = g.NumberGiveData
                    End If
                Next i
            End If

            If Not FoundGiveScript Then
                'check b for give anything:
                GiveScript = _objs(GiveToObjectID).GiveAnything
                If GiveScript <> "" Then
                    FoundGiveScript = True
                    SetStringContents("quest.give.object.name", _objs(ItemID).ObjectName, ctx)
                End If
            End If

            If Not FoundGiveScript Then
                'check a for give to anything:
                GiveScript = _objs(ItemID).GiveToAnything
                If GiveScript <> "" Then
                    FoundGiveScript = True
                    SetStringContents("quest.give.object.name", _objs(GiveToObjectID).ObjectName, ctx)
                End If
            End If

            If FoundGiveScript Then
                ExecuteScript(GiveScript, ctx, ItemID)
            Else
                SetStringContents("quest.error.charactername", _objs(GiveToObjectID).ObjectName, ctx)

                ObjGender = Trim(_objs(GiveToObjectID).Gender)
                ObjGender = UCase(Left(ObjGender, 1)) & Mid(ObjGender, 2)
                SetStringContents("quest.error.gender", ObjGender, ctx)

                SetStringContents("quest.error.article", ObjArticle, ctx)
                PlayerErrorMessage(PlayerError.ItemUnwanted, ctx)
            End If
        Else
            ' ASL2:
            characterblock = GetThingBlock(CharToGive, _currentRoom, ObjectType)

            If (characterblock.StartLine = 0 And characterblock.EndLine = 0) Or IsAvailable(CharToGive & "@" & _currentRoom, ObjectType, ctx) = False Then
                PlayerErrorMessage(PlayerError.BadCharacter, ctx)
                Exit Sub
            End If

            RealName = _chars(GetThingNumber(CharToGive, _currentRoom, ObjectType)).ObjectName

            ' now, see if there's a give statement for this item in
            ' this characters definition:

            GiveLine = 0
            For i = characterblock.StartLine + 1 To characterblock.EndLine - 1
                If BeginsWith(_lines(i), "give") Then
                    ItemCheck = GetParameter(_lines(i), ctx)
                    If LCase(ItemCheck) = LCase(ItemToGive) Then
                        GiveLine = i
                    End If
                End If
            Next i

            If GiveLine = 0 Then
                If ObjArticle = "" Then ObjArticle = "it"
                SetStringContents("quest.error.charactername", RealName, ctx)
                SetStringContents("quest.error.gender", Trim(GetGender(CharToGive, True, ctx)), ctx)
                SetStringContents("quest.error.article", ObjArticle, ctx)
                PlayerErrorMessage(PlayerError.ItemUnwanted, ctx)
                Exit Sub
            End If

            ' now, execute the statement on GiveLine
            ExecuteScript(GetSecondChunk(_lines(GiveLine)), ctx)
        End If

    End Sub

    Private Sub ExecLook(LookLine As String, ctx As Context)
        Dim AtPos As Integer
        Dim LookStuff, LookItem, LookText As String

        Dim InventoryPlace As String
        Dim FoundObject As Boolean
        Dim ObjID As Integer
        If Trim(LookLine) = "look" Then
            ShowRoomInfo((_currentRoom), ctx)
        Else
            If _gameAslVersion < 391 Then
                AtPos = InStr(LookLine, " at ")

                If AtPos = 0 Then
                    LookItem = GetEverythingAfter(LookLine, "look ")
                Else
                    LookItem = Trim(Mid(LookLine, AtPos + 4))
                End If
            Else
                If BeginsWith(LookLine, "look at ") Then
                    LookItem = GetEverythingAfter(LookLine, "look at ")
                ElseIf BeginsWith(LookLine, "look in ") Then
                    LookItem = GetEverythingAfter(LookLine, "look in ")
                ElseIf BeginsWith(LookLine, "look on ") Then
                    LookItem = GetEverythingAfter(LookLine, "look on ")
                ElseIf BeginsWith(LookLine, "look inside ") Then
                    LookItem = GetEverythingAfter(LookLine, "look inside ")
                Else
                    LookItem = GetEverythingAfter(LookLine, "look ")
                End If
            End If

            If _gameAslVersion >= 280 Then
                FoundObject = False

                InventoryPlace = "inventory"

                ObjID = Disambiguate(LookItem, InventoryPlace & ";" & _currentRoom, ctx)
                If ObjID > 0 Then
                    FoundObject = True
                End If

                If Not FoundObject Then
                    If ObjID <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                    _badCmdBefore = "look at"
                    Exit Sub
                End If

                DoLook(ObjID, ctx)
            Else
                If BeginsWith(LookItem, "the ") Then
                    LookItem = GetEverythingAfter(LookItem, "the ")
                End If

                LookLine = RetrLine("object", LookItem, "look", ctx)

                If LookLine <> "<unfound>" Then
                    'Check for availability
                    If IsAvailable(LookItem, Thing.Object, ctx) = False Then
                        LookLine = "<unfound>"
                    End If
                End If

                If LookLine = "<unfound>" Then
                    LookLine = RetrLine("character", LookItem, "look", ctx)

                    If LookLine <> "<unfound>" Then
                        If IsAvailable(LookItem, Thing.Character, ctx) = False Then
                            LookLine = "<unfound>"
                        End If
                    End If

                    If LookLine = "<unfound>" Then
                        PlayerErrorMessage(PlayerError.BadThing, ctx)
                        Exit Sub
                    ElseIf LookLine = "<undefined>" Then
                        PlayerErrorMessage(PlayerError.DefaultLook, ctx)
                        Exit Sub
                    End If
                ElseIf LookLine = "<undefined>" Then
                    PlayerErrorMessage(PlayerError.DefaultLook, ctx)
                    Exit Sub
                End If

                LookStuff = Trim(GetEverythingAfter(Trim(LookLine), "look "))
                If Left(LookStuff, 1) = "<" Then
                    LookText = GetParameter(LookLine, ctx)
                    Print(LookText, ctx)
                Else
                    ExecuteScript(LookStuff, ctx, ObjID)
                End If
            End If
        End If

    End Sub

    Private Sub ExecSpeak(c As String, ctx As Context)
        Dim i As Integer
        Dim l, s As String

        If BeginsWith(c, "the ") Then
            c = GetEverythingAfter(c, "the ")
        End If

        Dim ObjectName As String
        ObjectName = c

        ' if the "speak" parameter of the character c$'s definition
        ' is just a parameter, say it - otherwise execute it as
        ' a script.

        Dim ObjectType As Thing

        Dim SpeakLine As String = ""
        Dim InventoryPlace As String
        Dim FoundSpeak As Boolean
        Dim SpeakText As String
        Dim FoundObject As Boolean
        Dim ObjID As Integer
        If _gameAslVersion >= 281 Then

            FoundObject = False

            InventoryPlace = "inventory"

            ObjID = Disambiguate(ObjectName, InventoryPlace & ";" & _currentRoom, ctx)
            If ObjID > 0 Then
                FoundObject = True
            End If

            If Not FoundObject Then
                If ObjID <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                _badCmdBefore = "speak to"
                Exit Sub
            End If

            FoundSpeak = False

            ' First look for action, then look
            ' for property, then check define
            ' section:

            Dim o = _objs(ObjID)

            For i = 1 To o.NumberActions
                If o.Actions(i).ActionName = "speak" Then
                    SpeakLine = "speak " & o.Actions(i).Script
                    FoundSpeak = True
                    i = o.NumberActions
                End If
            Next i

            If Not FoundSpeak Then
                For i = 1 To o.NumberProperties
                    If o.Properties(i).PropertyName = "speak" Then
                        SpeakLine = "speak <" & o.Properties(i).PropertyValue & ">"
                        FoundSpeak = True
                        i = o.NumberProperties
                    End If
                Next i
            End If

            If _gameAslVersion < 311 Then
                ' For some reason ASL3 < 311 looks for a "look" tag rather than
                ' having had this set up at initialisation.
                If Not FoundSpeak Then
                    For i = o.DefinitionSectionStart To o.DefinitionSectionEnd
                        If BeginsWith(_lines(i), "speak ") Then
                            SpeakLine = _lines(i)
                            FoundSpeak = True
                        End If
                    Next i
                End If
            End If

            If Not FoundSpeak Then
                SetStringContents("quest.error.gender", UCase(Left(_objs(ObjID).Gender, 1)) & Mid(_objs(ObjID).Gender, 2), ctx)
                PlayerErrorMessage(PlayerError.DefaultSpeak, ctx)
                Exit Sub
            End If

            SpeakLine = GetEverythingAfter(SpeakLine, "speak ")

            If BeginsWith(SpeakLine, "<") Then
                SpeakText = GetParameter(SpeakLine, ctx)
                If _gameAslVersion >= 350 Then
                    Print(SpeakText, ctx)
                Else
                    Print(Chr(34) & SpeakText & Chr(34), ctx)
                End If
            Else
                ExecuteScript(SpeakLine, ctx, ObjID)
            End If

        Else
            l = RetrLine("character", c, "speak", ctx)
            ObjectType = Thing.Character

            s = Trim(GetEverythingAfter(Trim(l), "speak "))

            If l <> "<unfound>" And l <> "<undefined>" Then
                ' Character exists; but is it available??
                If IsAvailable(c & "@" & _currentRoom, ObjectType, ctx) = False Then
                    l = "<undefined>"
                End If
            End If

            If l = "<undefined>" Then
                PlayerErrorMessage(PlayerError.BadCharacter, ctx)
            ElseIf l = "<unfound>" Then
                SetStringContents("quest.error.gender", Trim(GetGender(c, True, ctx)), ctx)
                SetStringContents("quest.error.charactername", c, ctx)
                PlayerErrorMessage(PlayerError.DefaultSpeak, ctx)
            ElseIf BeginsWith(s, "<") Then
                s = GetParameter(l, ctx)
                Print(Chr(34) & s & Chr(34), ctx)
            Else
                ExecuteScript(s, ctx)
            End If
        End If

    End Sub

    Private Sub ExecTake(TakeItem As String, ctx As Context)
        Dim i As Integer
        Dim FoundItem, FoundTake As Boolean
        Dim OriginalTakeItem As String
        Dim ObjID As Integer
        Dim ParentID As Integer
        Dim ParentDisplayName As String
        Dim ObjectIsInContainer As Boolean
        Dim ScriptLine As String
        Dim ContainerError As String = ""

        FoundItem = True
        FoundTake = False
        OriginalTakeItem = TakeItem

        ObjID = Disambiguate(TakeItem, _currentRoom, ctx)

        If ObjID < 0 Then
            FoundItem = False
        Else
            FoundItem = True
        End If

        If FoundItem = False Then
            If ObjID <> -2 Then
                If _gameAslVersion >= 410 Then
                    ObjID = Disambiguate(TakeItem, "inventory", ctx)
                    If ObjID >= 0 Then
                        ' Player already has this item
                        PlayerErrorMessage(PlayerError.AlreadyTaken, ctx)
                    Else
                        PlayerErrorMessage(PlayerError.BadThing, ctx)
                    End If
                ElseIf _gameAslVersion >= 391 Then
                    PlayerErrorMessage(PlayerError.BadThing, ctx)
                Else
                    PlayerErrorMessage(PlayerError.BadItem, ctx)
                End If
            End If
            _badCmdBefore = "take"
            Exit Sub
        Else
            SetStringContents("quest.error.article", _objs(ObjID).Article, ctx)
        End If

        ObjectIsInContainer = False

        If _gameAslVersion >= 391 Then

            If Not PlayerCanAccessObject(ObjID, ParentID, ContainerError) Then
                PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, ContainerError)
                Exit Sub
            End If

        End If

        If _gameAslVersion >= 280 Then
            Dim t = _objs(ObjID).Take

            If ObjectIsInContainer And (t.Type = TextActionType.Default Or t.Type = TextActionType.Text) Then
                ' So, we want to take an object that's in a container or surface. So first
                ' we have to remove the object from that container.

                If _objs(ParentID).ObjectAlias <> "" Then
                    ParentDisplayName = _objs(ParentID).ObjectAlias
                Else
                    ParentDisplayName = _objs(ParentID).ObjectName
                End If

                Print("(first removing " & _objs(ObjID).Article & " from " & ParentDisplayName & ")", ctx)

                ' Try to remove the object
                ctx.AllowRealNamesInCommand = True
                ExecCommand("remove " & _objs(ObjID).ObjectName, ctx, False, , True)

                If GetObjectProperty("parent", ObjID, False, False) <> "" Then
                    ' removing the object failed
                    Exit Sub
                End If
            End If

            If t.Type = TextActionType.Default Then
                PlayerErrorMessage(PlayerError.DefaultTake, ctx)
                PlayerItem(TakeItem, True, ctx, ObjID)
            ElseIf t.Type = TextActionType.Text Then
                Print(t.Data, ctx)
                PlayerItem(TakeItem, True, ctx, ObjID)
            ElseIf t.Type = TextActionType.Script Then
                ExecuteScript(t.Data, ctx, ObjID)
            Else
                PlayerErrorMessage(PlayerError.BadTake, ctx)
            End If
        Else

            ' find 'take' line
            For i = _objs(ObjID).DefinitionSectionStart + 1 To _objs(ObjID).DefinitionSectionEnd - 1
                If BeginsWith(_lines(i), "take") Then
                    ScriptLine = Trim(GetEverythingAfter(Trim(_lines(i)), "take"))
                    ExecuteScript(ScriptLine, ctx, ObjID)
                    FoundTake = True
                    i = _objs(ObjID).DefinitionSectionEnd
                End If
            Next i

            If FoundTake = False Then
                PlayerErrorMessage(PlayerError.BadTake, ctx)
            End If
        End If

    End Sub

    Private Sub ExecUse(tuseline As String, ctx As Context)
        Dim OnWithPos, i, EndOnWith As Integer
        Dim UseLine As String
        Dim UseDeclareLine As String = "", UseOn, UseItem, ScriptLine As String

        UseLine = Trim(GetEverythingAfter(tuseline, "use "))

        Dim RoomID As Integer
        RoomID = GetRoomID(_currentRoom, ctx)

        OnWithPos = InStr(UseLine, " on ")
        If OnWithPos = 0 Then
            OnWithPos = InStr(UseLine, " with ")
            EndOnWith = OnWithPos + 4
        Else
            EndOnWith = OnWithPos + 2
        End If


        If OnWithPos <> 0 Then
            UseOn = Trim(Right(UseLine, Len(UseLine) - EndOnWith))
            UseItem = Trim(Left(UseLine, OnWithPos - 1))
        Else
            UseOn = ""
            UseItem = UseLine
        End If

        ' see if player has this item:

        Dim FoundItem As Boolean
        Dim ItemID As Integer
        Dim InventoryPlace As String = ""
        Dim NotGotItem As Boolean
        If _gameAslVersion >= 280 Then
            InventoryPlace = "inventory"

            FoundItem = False

            ItemID = Disambiguate(UseItem, InventoryPlace, ctx)
            If ItemID > 0 Then FoundItem = True

            If Not FoundItem Then
                If ItemID <> -2 Then PlayerErrorMessage(PlayerError.NoItem, ctx)
                If UseOn = "" Then
                    _badCmdBefore = "use"
                Else
                    _badCmdBefore = "use"
                    _badCmdAfter = "on " & UseOn
                End If
                Exit Sub
            End If
        Else
            NotGotItem = True

            For i = 1 To _numberItems
                If LCase(_items(i).Name) = LCase(UseItem) Then
                    If _items(i).Got = False Then
                        NotGotItem = True
                        i = _numberItems
                    Else
                        NotGotItem = False
                    End If
                End If
            Next i

            If NotGotItem = True Then
                PlayerErrorMessage(PlayerError.NoItem, ctx)
                Exit Sub
            End If
        End If

        Dim UseScript As String = ""
        Dim FoundUseScript As Boolean
        Dim FoundUseOnObject As Boolean
        Dim UseOnObjectID As Integer
        Dim Found As Boolean
        If _gameAslVersion >= 280 Then
            FoundUseScript = False

            If UseOn = "" Then
                If _gameAslVersion < 410 Then
                    Dim r = _rooms(RoomID)
                    For i = 1 To r.NumberUse
                        If LCase(_objs(ItemID).ObjectName) = LCase(r.Use(i).Text) Then
                            FoundUseScript = True
                            UseScript = r.Use(i).Script
                            i = r.NumberUse
                        End If
                    Next i
                End If

                If Not FoundUseScript Then
                    UseScript = _objs(ItemID).Use
                    If UseScript <> "" Then FoundUseScript = True
                End If
            Else
                FoundUseOnObject = False

                UseOnObjectID = Disambiguate(UseOn, _currentRoom, ctx)
                If UseOnObjectID > 0 Then
                    FoundUseOnObject = True
                Else
                    UseOnObjectID = Disambiguate(UseOn, InventoryPlace, ctx)
                    If UseOnObjectID > 0 Then
                        FoundUseOnObject = True
                    End If
                End If

                If Not FoundUseOnObject Then
                    If UseOnObjectID <> -2 Then PlayerErrorMessage(PlayerError.BadThing, ctx)
                    _badCmdBefore = "use " & UseItem & " on"
                    Exit Sub
                End If

                'now, for "use a on b", we have
                'ItemID=a and UseOnObjectID=b

                'first check b for use <a>:

                Dim o = _objs(UseOnObjectID)

                For i = 1 To o.NumberUseData
                    If o.UseData(i).UseType = UseType.UseSomethingOn And LCase(o.UseData(i).UseObject) = LCase(_objs(ItemID).ObjectName) Then
                        FoundUseScript = True
                        UseScript = o.UseData(i).UseScript
                        i = o.NumberUseData
                    End If
                Next i

                If Not FoundUseScript Then
                    'check a for use on <b>:
                    Dim u = _objs(ItemID)
                    For i = 1 To u.NumberUseData
                        If u.UseData(i).UseType = UseType.UseOnSomething And LCase(u.UseData(i).UseObject) = LCase(_objs(UseOnObjectID).ObjectName) Then
                            FoundUseScript = True
                            UseScript = u.UseData(i).UseScript
                            i = u.NumberUseData
                        End If
                    Next i
                End If

                If Not FoundUseScript Then
                    'check b for use anything:
                    UseScript = _objs(UseOnObjectID).UseAnything
                    If UseScript <> "" Then
                        FoundUseScript = True
                        SetStringContents("quest.use.object.name", _objs(ItemID).ObjectName, ctx)
                    End If
                End If

                If Not FoundUseScript Then
                    'check a for use on anything:
                    UseScript = _objs(ItemID).UseOnAnything
                    If UseScript <> "" Then
                        FoundUseScript = True
                        SetStringContents("quest.use.object.name", _objs(UseOnObjectID).ObjectName, ctx)
                    End If
                End If
            End If

            If FoundUseScript Then
                ExecuteScript(UseScript, ctx, ItemID)
            Else
                PlayerErrorMessage(PlayerError.DefaultUse, ctx)
            End If
        Else
            If UseOn <> "" Then
                UseDeclareLine = RetrLineParam("object", UseOn, "use", UseItem, ctx)
            Else
                Found = False
                For i = 1 To _rooms(RoomID).NumberUse
                    If LCase(_rooms(RoomID).Use(i).Text) = LCase(UseItem) Then
                        UseDeclareLine = "use <> " & _rooms(RoomID).Use(i).Script
                        Found = True
                        i = _rooms(RoomID).NumberUse
                    End If
                Next i

                If Not Found Then
                    UseDeclareLine = FindLine(GetDefineBlock("game"), "use", UseItem)
                End If

                If Not Found And UseDeclareLine = "" Then
                    PlayerErrorMessage(PlayerError.DefaultUse, ctx)
                    Exit Sub
                End If
            End If

            If UseDeclareLine <> "<unfound>" And UseDeclareLine <> "<undefined>" And UseOn <> "" Then
                'Check for object availablity
                If IsAvailable(UseOn, Thing.Object, ctx) = False Then
                    UseDeclareLine = "<undefined>"
                End If
            End If

            If UseDeclareLine = "<undefined>" Then
                UseDeclareLine = RetrLineParam("character", UseOn, "use", UseItem, ctx)

                If UseDeclareLine <> "<undefined>" Then
                    'Check for character availability
                    If IsAvailable(UseOn, Thing.Character, ctx) = False Then
                        UseDeclareLine = "<undefined>"
                    End If
                End If

                If UseDeclareLine = "<undefined>" Then
                    PlayerErrorMessage(PlayerError.BadThing, ctx)
                    Exit Sub
                ElseIf UseDeclareLine = "<unfound>" Then
                    PlayerErrorMessage(PlayerError.DefaultUse, ctx)
                    Exit Sub
                End If
            ElseIf UseDeclareLine = "<unfound>" Then
                PlayerErrorMessage(PlayerError.DefaultUse, ctx)
                Exit Sub
            End If

            ScriptLine = Right(UseDeclareLine, Len(UseDeclareLine) - InStr(UseDeclareLine, ">"))
            ExecuteScript(ScriptLine, ctx)
        End If

    End Sub

    Private Sub ObjectActionUpdate(ObjID As Integer, ActionName As String, ActionScript As String, Optional NoUpdate As Boolean = False)
        Dim ObjectName As String
        Dim SP, EP As Integer
        ActionName = LCase(ActionName)

        If Not NoUpdate Then
            If ActionName = "take" Then
                _objs(ObjID).Take.Data = ActionScript
                _objs(ObjID).Take.Type = TextActionType.Script
            ElseIf ActionName = "use" Then
                AddToUseInfo(ObjID, ActionScript)
            ElseIf ActionName = "gain" Then
                _objs(ObjID).GainScript = ActionScript
            ElseIf ActionName = "lose" Then
                _objs(ObjID).LoseScript = ActionScript
            ElseIf BeginsWith(ActionName, "use ") Then
                ActionName = GetEverythingAfter(ActionName, "use ")
                If InStr(ActionName, "'") > 0 Then
                    SP = InStr(ActionName, "'")
                    EP = InStr(SP + 1, ActionName, "'")
                    If EP = 0 Then
                        LogASLError("Missing ' in 'action <use " & ActionName & "> " & ReportErrorLine(ActionScript))
                        Exit Sub
                    End If

                    ObjectName = Mid(ActionName, SP + 1, EP - SP - 1)

                    AddToUseInfo(ObjID, Trim(Left(ActionName, SP - 1)) & " <" & ObjectName & "> " & ActionScript)
                Else
                    AddToUseInfo(ObjID, ActionName & " " & ActionScript)
                End If
            ElseIf BeginsWith(ActionName, "give ") Then
                ActionName = GetEverythingAfter(ActionName, "give ")
                If InStr(ActionName, "'") > 0 Then

                    SP = InStr(ActionName, "'")
                    EP = InStr(SP + 1, ActionName, "'")
                    If EP = 0 Then
                        LogASLError("Missing ' in 'action <give " & ActionName & "> " & ReportErrorLine(ActionScript))
                        Exit Sub
                    End If

                    ObjectName = Mid(ActionName, SP + 1, EP - SP - 1)

                    AddToGiveInfo(ObjID, Trim(Left(ActionName, SP - 1)) & " <" & ObjectName & "> " & ActionScript)
                Else
                    AddToGiveInfo(ObjID, ActionName & " " & ActionScript)
                End If
            End If
        End If

        If _gameFullyLoaded Then
            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs(ObjID).ObjectName, ActionName, "action <" & ActionName & "> " & ActionScript)
        End If

    End Sub

    Private Sub ExecuteIf(ScriptLine As String, ctx As Context)
        Dim IfLine, Conditions, ObscuredLine As String
        Dim ElsePos, ThenPos, ThenEndPos As Integer
        Dim ThenScript As String, ElseScript As String = ""

        IfLine = Trim(GetEverythingAfter(Trim(ScriptLine), "if "))
        ObscuredLine = ObliterateParameters(IfLine)

        ThenPos = InStr(ObscuredLine, "then")

        Dim ErrMsg As String
        If ThenPos = 0 Then
            ErrMsg = "Expected 'then' missing from script statement '" & ReportErrorLine(ScriptLine) & "' - statement bypassed."
            LogASLError(ErrMsg, LogType.WarningError)
            Exit Sub
        End If

        Conditions = Trim(Left(IfLine, ThenPos - 1))

        ThenPos = ThenPos + 4
        ElsePos = InStr(ObscuredLine, "else")

        If ElsePos = 0 Then
            ThenEndPos = Len(ObscuredLine) + 1
        Else
            ThenEndPos = ElsePos - 1
        End If

        ThenScript = Trim(Mid(IfLine, ThenPos, ThenEndPos - ThenPos))

        If ElsePos <> 0 Then
            ElseScript = Trim(Right(IfLine, Len(IfLine) - (ThenEndPos + 4)))
        End If

        ' Remove braces from around "then" and "else" script
        ' commands, if present
        If Left(ThenScript, 1) = "{" And Right(ThenScript, 1) = "}" Then
            ThenScript = Mid(ThenScript, 2, Len(ThenScript) - 2)
        End If
        If Left(ElseScript, 1) = "{" And Right(ElseScript, 1) = "}" Then
            ElseScript = Mid(ElseScript, 2, Len(ElseScript) - 2)
        End If

        If ExecuteConditions(Conditions, ctx) Then
            ExecuteScript((ThenScript), ctx)
        Else
            If ElsePos <> 0 Then ExecuteScript((ElseScript), ctx)
        End If

    End Sub

    Private Sub ExecuteScript(ScriptLine As String, ctx As Context, Optional NewCallingObjectID As Integer = 0)
        Try
            If Trim(ScriptLine) = "" Then Exit Sub
            If _gameFinished Then Exit Sub

            Dim CurPos As Integer
            Dim bFinLoop As Boolean
            Dim CRLFPos As Integer
            Dim CurScriptLine As String
            If InStr(ScriptLine, vbCrLf) > 0 Then
                CurPos = 1
                bFinLoop = False
                Do
                    CRLFPos = InStr(CurPos, ScriptLine, vbCrLf)
                    If CRLFPos = 0 Then
                        bFinLoop = True
                        CRLFPos = Len(ScriptLine) + 1
                    End If

                    CurScriptLine = Trim(Mid(ScriptLine, CurPos, CRLFPos - CurPos))
                    If CurScriptLine <> vbCrLf Then
                        ExecuteScript(CurScriptLine, ctx)
                    End If
                    CurPos = CRLFPos + 2
                Loop Until bFinLoop
                Exit Sub
            End If

            If NewCallingObjectID <> 0 Then
                ctx.CallingObjectId = NewCallingObjectID
            End If

            If BeginsWith(ScriptLine, "if ") Then
                ExecuteIf(ScriptLine, ctx)
            ElseIf BeginsWith(ScriptLine, "select case ") Then
                ExecuteSelectCase(ScriptLine, ctx)
            ElseIf BeginsWith(ScriptLine, "choose ") Then
                ExecuteChoose(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "set ") Then
                ExecuteSet(GetEverythingAfter(ScriptLine, "set "), ctx)
            ElseIf BeginsWith(ScriptLine, "inc ") Or BeginsWith(ScriptLine, "dec ") Then
                ExecuteIncDec(ScriptLine, ctx)
            ElseIf BeginsWith(ScriptLine, "say ") Then
                Print(Chr(34) & GetParameter(ScriptLine, ctx) & Chr(34), ctx)
            ElseIf BeginsWith(ScriptLine, "do ") Then
                ExecuteDo(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "doaction ") Then
                ExecuteDoAction(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "give ") Then
                PlayerItem(GetParameter(ScriptLine, ctx), True, ctx)
            ElseIf BeginsWith(ScriptLine, "lose ") Or BeginsWith(ScriptLine, "drop ") Then
                PlayerItem(GetParameter(ScriptLine, ctx), False, ctx)
            ElseIf BeginsWith(ScriptLine, "msg nospeak ") Then
                Print(GetParameter(ScriptLine, ctx), ctx, , True)
            ElseIf BeginsWith(ScriptLine, "msg ") Then
                Print(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "speak ") Then
                Speak(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "helpmsg ") Then
                Print(GetParameter(ScriptLine, ctx), ctx, "help")
            ElseIf Trim(LCase(ScriptLine)) = "helpclose" Then
                ' This command does nothing in the Quest 5 player, as there is no separate help window
            ElseIf BeginsWith(ScriptLine, "goto ") Then
                PlayGame(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "playerwin") Then
                FinishGame(StopType.Win, ctx)
            ElseIf BeginsWith(ScriptLine, "playerlose") Then
                FinishGame(StopType.Lose, ctx)
            ElseIf Trim(LCase(ScriptLine)) = "stop" Then
                FinishGame(StopType.Null, ctx)
            ElseIf BeginsWith(ScriptLine, "playwav ") Then
                PlayWav(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "playmidi ") Then
                PlayMedia(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "playmp3 ") Then
                PlayMedia(GetParameter(ScriptLine, ctx))
            ElseIf Trim(LCase(ScriptLine)) = "picture close" Then
                ' This command does nothing in the Quest 5 player, as there is no separate picture window
            ElseIf (_gameAslVersion >= 390 And BeginsWith(ScriptLine, "picture popup ")) Or (_gameAslVersion >= 282 And _gameAslVersion < 390 And BeginsWith(ScriptLine, "picture ")) Or (_gameAslVersion < 282 And BeginsWith(ScriptLine, "show ")) Then
                ShowPicture(GetParameter(ScriptLine, ctx))
            ElseIf (_gameAslVersion >= 390 And BeginsWith(ScriptLine, "picture ")) Then
                ShowPictureInText(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "animate persist ") Then
                ShowPicture(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "animate ") Then
                ShowPicture(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "extract ") Then
                ExtractFile(GetParameter(ScriptLine, ctx))
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "hideobject ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), False, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "showobject ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), True, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "moveobject ") Then
                ExecMoveThing(GetParameter(ScriptLine, ctx), Thing.Object, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "hidechar ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), False, ctx, Thing.Character)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "showchar ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), True, ctx, Thing.Character)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "movechar ") Then
                ExecMoveThing(GetParameter(ScriptLine, ctx), Thing.Character, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "revealobject ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Object, True, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "concealobject ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Object, False, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "revealchar ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Character, True, ctx)
            ElseIf _gameAslVersion < 281 And BeginsWith(ScriptLine, "concealchar ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Character, False, ctx)
            ElseIf _gameAslVersion >= 281 And BeginsWith(ScriptLine, "hide ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), False, ctx)
            ElseIf _gameAslVersion >= 281 And BeginsWith(ScriptLine, "show ") Then
                SetAvailability(GetParameter(ScriptLine, ctx), True, ctx)
            ElseIf _gameAslVersion >= 281 And BeginsWith(ScriptLine, "move ") Then
                ExecMoveThing(GetParameter(ScriptLine, ctx), Thing.Object, ctx)
            ElseIf _gameAslVersion >= 281 And BeginsWith(ScriptLine, "reveal ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Object, True, ctx)
            ElseIf _gameAslVersion >= 281 And BeginsWith(ScriptLine, "conceal ") Then
                SetVisibility(GetParameter(ScriptLine, ctx), Thing.Object, False, ctx)
            ElseIf _gameAslVersion >= 391 And BeginsWith(ScriptLine, "open ") Then
                SetOpenClose(GetParameter(ScriptLine, ctx), True, ctx)
            ElseIf _gameAslVersion >= 391 And BeginsWith(ScriptLine, "close ") Then
                SetOpenClose(GetParameter(ScriptLine, ctx), False, ctx)
            ElseIf _gameAslVersion >= 391 And BeginsWith(ScriptLine, "add ") Then
                ExecAddRemoveScript(GetParameter(ScriptLine, ctx), True, ctx)
            ElseIf _gameAslVersion >= 391 And BeginsWith(ScriptLine, "remove ") Then
                ExecAddRemoveScript(GetParameter(ScriptLine, ctx), False, ctx)
            ElseIf BeginsWith(ScriptLine, "clone ") Then
                ExecClone(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "exec ") Then
                ExecExec(ScriptLine, ctx)
            ElseIf BeginsWith(ScriptLine, "setstring ") Then
                ExecSetString(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "setvar ") Then
                ExecSetVar(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "for ") Then
                ExecFor(GetEverythingAfter(ScriptLine, "for "), ctx)
            ElseIf BeginsWith(ScriptLine, "property ") Then
                ExecProperty(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "type ") Then
                ExecType(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "action ") Then
                ExecuteAction(GetEverythingAfter(ScriptLine, "action "), ctx)
            ElseIf BeginsWith(ScriptLine, "flag ") Then
                ExecuteFlag(GetEverythingAfter(ScriptLine, "flag "), ctx)
            ElseIf BeginsWith(ScriptLine, "create ") Then
                ExecuteCreate(GetEverythingAfter(ScriptLine, "create "), ctx)
            ElseIf BeginsWith(ScriptLine, "destroy exit ") Then
                DestroyExit(GetParameter(ScriptLine, ctx), ctx)
            ElseIf BeginsWith(ScriptLine, "repeat ") Then
                ExecuteRepeat(GetEverythingAfter(ScriptLine, "repeat "), ctx)
            ElseIf BeginsWith(ScriptLine, "enter ") Then
                ExecuteEnter(ScriptLine, ctx)
            ElseIf BeginsWith(ScriptLine, "displaytext ") Then
                DisplayTextSection(GetParameter(ScriptLine, ctx), ctx, "normal")
            ElseIf BeginsWith(ScriptLine, "helpdisplaytext ") Then
                DisplayTextSection(GetParameter(ScriptLine, ctx), ctx, "help")
            ElseIf BeginsWith(ScriptLine, "font ") Then
                SetFont(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "pause ") Then
                Pause(CInt(GetParameter(ScriptLine, ctx)))
            ElseIf Trim(LCase(ScriptLine)) = "clear" Then
                DoClear()
            ElseIf Trim(LCase(ScriptLine)) = "helpclear" Then
                ' This command does nothing in the Quest 5 player, as there is no separate help window
            ElseIf BeginsWith(ScriptLine, "background ") Then
                SetBackground(GetParameter(ScriptLine, ctx))
            ElseIf BeginsWith(ScriptLine, "foreground ") Then
                SetForeground(GetParameter(ScriptLine, ctx))
            ElseIf Trim(LCase(ScriptLine)) = "nointro" Then
                _autoIntro = False
            ElseIf BeginsWith(ScriptLine, "debug ") Then
                LogASLError(GetParameter(ScriptLine, ctx), LogType.Misc)
            ElseIf BeginsWith(ScriptLine, "mailto ") Then
                Dim emailAddress As String = GetParameter(ScriptLine, ctx)
                RaiseEvent PrintText("<a target=""_blank"" href=""mailto:" + emailAddress + """>" + emailAddress + "</a>")
            ElseIf BeginsWith(ScriptLine, "shell ") And _gameAslVersion < 410 Then
                LogASLError("'shell' is not supported in this version of Quest", LogType.WarningError)
            ElseIf BeginsWith(ScriptLine, "shellexe ") And _gameAslVersion < 410 Then
                LogASLError("'shellexe' is not supported in this version of Quest", LogType.WarningError)
            ElseIf BeginsWith(ScriptLine, "wait") Then
                ExecuteWait(Trim(GetEverythingAfter(Trim(ScriptLine), "wait")), ctx)
            ElseIf BeginsWith(ScriptLine, "timeron ") Then
                SetTimerState(GetParameter(ScriptLine, ctx), True)
            ElseIf BeginsWith(ScriptLine, "timeroff ") Then
                SetTimerState(GetParameter(ScriptLine, ctx), False)
            ElseIf Trim(LCase(ScriptLine)) = "outputon" Then
                _outPutOn = True
                UpdateObjectList(ctx)
                UpdateItems(ctx)
            ElseIf Trim(LCase(ScriptLine)) = "outputoff" Then
                _outPutOn = False
            ElseIf Trim(LCase(ScriptLine)) = "panes off" Then
                _player.SetPanesVisible("off")
            ElseIf Trim(LCase(ScriptLine)) = "panes on" Then
                _player.SetPanesVisible("on")
            ElseIf BeginsWith(ScriptLine, "lock ") Then
                ExecuteLock(GetParameter(ScriptLine, ctx), True)
            ElseIf BeginsWith(ScriptLine, "unlock ") Then
                ExecuteLock(GetParameter(ScriptLine, ctx), False)
            ElseIf BeginsWith(ScriptLine, "playmod ") And _gameAslVersion < 410 Then
                LogASLError("'playmod' is not supported in this version of Quest", LogType.WarningError)
            ElseIf BeginsWith(ScriptLine, "modvolume") And _gameAslVersion < 410 Then
                LogASLError("'modvolume' is not supported in this version of Quest", LogType.WarningError)
            ElseIf Trim(LCase(ScriptLine)) = "dontprocess" Then
                ctx.DontProcessCommand = True
            ElseIf BeginsWith(ScriptLine, "return ") Then
                ctx.FunctionReturnValue = GetParameter(ScriptLine, ctx)
            Else
                If BeginsWith(ScriptLine, "'") = False Then
                    LogASLError("Unrecognized keyword. Line reads: '" & Trim(ReportErrorLine(ScriptLine)) & "'", LogType.WarningError)
                End If
            End If
        Catch
            Print("[An internal error occurred]", ctx)
            LogASLError(Err.Number & " - '" & Err.Description & "' occurred processing script line '" & ScriptLine & "'", LogType.InternalError)
        End Try
    End Sub

    Private Sub ExecuteEnter(ScriptLine As String, ctx As Context)
        _commandOverrideModeOn = True
        _commandOverrideVariable = GetParameter(ScriptLine, ctx)

        ' Now, wait for CommandOverrideModeOn to be set
        ' to False by ExecCommand. Execution can then resume.

        ChangeState(State.Waiting, True)

        SyncLock _commandLock
            System.Threading.Monitor.Wait(_commandLock)
        End SyncLock

        _commandOverrideModeOn = False

        ' State will have been changed to Working when the user typed their response,
        ' and will be set back to Ready when the call to ExecCommand has finished
    End Sub

    Private Sub ExecuteSet(SetInstruction As String, ctx As Context)
        Dim i As Integer
        Dim TimerInterval As String
        Dim SCPos As Integer
        Dim TimerName As String
        Dim FoundTimer As Boolean

        If _gameAslVersion >= 280 Then
            If BeginsWith(SetInstruction, "interval ") Then
                TimerInterval = GetParameter(SetInstruction, ctx)
                SCPos = InStr(TimerInterval, ";")
                If SCPos = 0 Then
                    LogASLError("Too few parameters in 'set " & SetInstruction & "'", LogType.WarningError)
                    Exit Sub
                End If

                TimerName = Trim(Left(TimerInterval, SCPos - 1))
                TimerInterval = CStr(Val(Trim(Mid(TimerInterval, SCPos + 1))))

                For i = 1 To _numberTimers
                    If LCase(TimerName) = LCase(_timers(i).TimerName) Then
                        FoundTimer = True
                        _timers(i).TimerInterval = CInt(TimerInterval)
                        i = _numberTimers
                    End If
                Next i

                If Not FoundTimer Then
                    LogASLError("No such timer '" & TimerName & "'", LogType.WarningError)
                    Exit Sub
                End If
            ElseIf BeginsWith(SetInstruction, "string ") Then
                ExecSetString(GetParameter(SetInstruction, ctx), ctx)
            ElseIf BeginsWith(SetInstruction, "numeric ") Then
                ExecSetVar(GetParameter(SetInstruction, ctx), ctx)
            ElseIf BeginsWith(SetInstruction, "collectable ") Then
                ExecuteSetCollectable(GetParameter(SetInstruction, ctx), ctx)
            Else
                Dim Result = SetUnknownVariableType(GetParameter(SetInstruction, ctx), ctx)
                If Result = SetResult.Error Then
                    LogASLError("Error on setting 'set " & SetInstruction & "'", LogType.WarningError)
                ElseIf Result = SetResult.Unfound Then
                    LogASLError("Variable type not specified in 'set " & SetInstruction & "'", LogType.WarningError)
                End If
            End If
        Else
            ExecuteSetCollectable(GetParameter(SetInstruction, ctx), ctx)
        End If

    End Sub

    Private Function FindStatement(searchblock As DefineBlock, statement As String) As String
        Dim i As Integer

        ' Finds a statement within a given block of lines

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1

            ' Ignore sub-define blocks
            If BeginsWith(_lines(i), "define ") Then
                Do
                    i = i + 1
                Loop Until Trim(_lines(i)) = "end define"
            End If
            ' Check to see if the line matches the statement
            ' that is begin searched for
            If BeginsWith(_lines(i), statement) Then
                ' Return the parameters between < and > :
                Return GetParameter(_lines(i), _nullContext)
            End If
        Next i

        Return ""
    End Function

    Private Function FindLine(searchblock As DefineBlock, statement As String, statementparam As String) As String
        Dim i As Integer
        ' Finds a statement within a given block of lines

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1

            ' Ignore sub-define blocks
            If BeginsWith(_lines(i), "define ") Then
                Do
                    i = i + 1
                Loop Until Trim(_lines(i)) = "end define"
            End If
            ' Check to see if the line matches the statement
            ' that is begin searched for
            If BeginsWith(_lines(i), statement) Then
                If UCase(Trim(GetParameter(_lines(i), _nullContext))) = UCase(Trim(statementparam)) Then
                    Return Trim(_lines(i))
                End If
            End If
        Next i

        Return ""
    End Function

    Private Function GetCollectableAmount(colname As String) As Double
        Dim i As Integer

        For i = 1 To _numCollectables
            If _collectables(i).Name = colname Then
                Return _collectables(i).Value
            End If
        Next i

        Return 0
    End Function

    Private Function GetSecondChunk(l As String) As String
        Dim EndOfFirstBit, LengthOfKeyword As Integer
        Dim SecondChunk As String

        EndOfFirstBit = InStr(l, ">") + 1
        LengthOfKeyword = (Len(l) - EndOfFirstBit) + 1
        SecondChunk = Trim(Mid(l, EndOfFirstBit, LengthOfKeyword))

        Return SecondChunk
    End Function

    Private Sub GoDirection(Direction As String, ctx As Context)
        ' leaves the current room in direction specified by
        ' 'direction'

        Dim NewRoom As String
        Dim SCP As Integer

        Dim RoomID As Integer
        Dim DirData As New TextAction
        RoomID = GetRoomID(_currentRoom, ctx)

        If RoomID = 0 Then Exit Sub

        If _gameAslVersion >= 410 Then
            _rooms(RoomID).Exits.ExecuteGo(Direction, ctx)
            Exit Sub
        End If

        Dim r = _rooms(RoomID)

        If Direction = "north" Then
            DirData = r.North
        ElseIf Direction = "south" Then
            DirData = r.South
        ElseIf Direction = "west" Then
            DirData = r.West
        ElseIf Direction = "east" Then
            DirData = r.East
        ElseIf Direction = "northeast" Then
            DirData = r.NorthEast
        ElseIf Direction = "northwest" Then
            DirData = r.NorthWest
        ElseIf Direction = "southeast" Then
            DirData = r.SouthEast
        ElseIf Direction = "southwest" Then
            DirData = r.SouthWest
        ElseIf Direction = "up" Then
            DirData = r.Up
        ElseIf Direction = "down" Then
            DirData = r.Down
        ElseIf Direction = "out" Then
            If r.Out.Script = "" Then
                DirData.Data = r.Out.Text
                DirData.Type = TextActionType.Text
            Else
                DirData.Data = r.Out.Script
                DirData.Type = TextActionType.Script
            End If
        End If

        If DirData.Type = TextActionType.Script And DirData.Data <> "" Then
            ExecuteScript(DirData.Data, ctx)
        ElseIf DirData.Data <> "" Then
            NewRoom = DirData.Data
            SCP = InStr(NewRoom, ";")
            If SCP <> 0 Then
                NewRoom = Trim(Mid(NewRoom, SCP + 1))
            End If
            PlayGame(NewRoom, ctx)
        Else
            If Direction = "out" Then
                PlayerErrorMessage(PlayerError.DefaultOut, ctx)
            Else
                PlayerErrorMessage(PlayerError.BadPlace, ctx)
            End If
        End If

    End Sub

    Private Sub GoToPlace(placecheck As String, ctx As Context)
        ' leaves the current room in direction specified by
        ' 'direction'

        Dim DisallowedDirection As Boolean
        Dim NP As String, Destination As String = "", P, s As String
        DisallowedDirection = False

        P = PlaceExist(placecheck, ctx)

        If P <> "" Then
            Destination = P
        ElseIf BeginsWith(placecheck, "the ") Then
            NP = GetEverythingAfter(placecheck, "the ")
            P = PlaceExist(NP, ctx)
            If P <> "" Then
                Destination = P
            Else
                DisallowedDirection = True
            End If
        Else
            DisallowedDirection = True
        End If

        If Destination <> "" Then
            If InStr(Destination, ";") > 0 Then
                s = Trim(Right(Destination, Len(Destination) - InStr(Destination, ";")))
                ExecuteScript(s, ctx)
            Else
                PlayGame(Destination, ctx)
            End If
        End If

        If DisallowedDirection = True Then
            PlayerErrorMessage(PlayerError.BadPlace, ctx)
        End If

    End Sub

    Private Function InitialiseGame(afilename As String, Optional LoadedFromQSG As Boolean = False) As Boolean
        Dim GameType, ErrorString As String
        Dim i As Integer
        Dim ASLVersion As String

        _loadedFromQsg = LoadedFromQSG

        _changeLogRooms = New ChangeLog
        _changeLogObjects = New ChangeLog
        _changeLogRooms.AppliesToType = ChangeLog.AppliesTo.Room
        _changeLogObjects.AppliesToType = ChangeLog.AppliesTo.Object

        _outPutOn = True
        _useAbbreviations = True

        _gamePath = System.IO.Path.GetDirectoryName(afilename) + "\"

        Dim LogMsg As String
        LogMsg = "Opening file " & afilename & " on " & Date.Now.ToString()
        LogASLError(LogMsg, LogType.Init)

        ' Parse file and find where the 'define' blocks are:
        If ParseFile(afilename) = False Then
            LogASLError("Unable to open file", LogType.Init)
            ErrorString = "Unable to open " & afilename

            If _openErrorReport <> "" Then
                ' Strip last vbcrlf
                _openErrorReport = Left(_openErrorReport, Len(_openErrorReport) - 2)
                ErrorString = ErrorString & ":" & vbCrLf & vbCrLf & _openErrorReport
            End If

            Print("Error: " & ErrorString, _nullContext)

            Return False
        End If

        ' Check version
        Dim gameblock As DefineBlock
        gameblock = GetDefineBlock("game")

        ASLVersion = "//"
        GameType = ""
        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(_lines(i), "asl-version ") Then
                ASLVersion = GetParameter(_lines(i), _nullContext)
            ElseIf BeginsWith(_lines(i), "gametype ") Then
                GameType = GetEverythingAfter(_lines(i), "gametype ")
            End If
        Next i

        If ASLVersion = "//" Then
            LogASLError("File contains no version header.", LogType.WarningError)
        Else
            _gameAslVersion = CInt(ASLVersion)

            Const RecognisedVersions As String = "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/"

            If InStr(RecognisedVersions, "/" & ASLVersion & "/") = 0 Then
                LogASLError("Unrecognised ASL version number.", LogType.WarningError)
            End If
        End If

        _listVerbs.Add(ListType.ExitsList, New List(Of String)(New String() {"Go to"}))

        If _gameAslVersion >= 280 And _gameAslVersion < 390 Then
            _listVerbs.Add(ListType.ObjectsList, New List(Of String)(New String() {"Look at", "Examine", "Take", "Speak to"}))
            _listVerbs.Add(ListType.InventoryList, New List(Of String)(New String() {"Look at", "Examine", "Use", "Drop"}))
        Else
            _listVerbs.Add(ListType.ObjectsList, New List(Of String)(New String() {"Look at", "Take", "Speak to"}))
            _listVerbs.Add(ListType.InventoryList, New List(Of String)(New String() {"Look at", "Use", "Drop"}))
        End If

        ' Get the name of the game:
        _gameName = GetParameter(_lines(GetDefineBlock("game").StartLine), _nullContext)

        _player.UpdateGameName(_gameName)
        _player.Show("Panes")
        _player.Show("Location")
        _player.Show("Command")

        SetUpGameObject()
        SetUpOptions()

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(i), "beforesave ") Then
                _beforeSaveScript = GetEverythingAfter(_lines(i), "beforesave ")
            ElseIf BeginsWith(_lines(i), "onload ") Then
                _onLoadScript = GetEverythingAfter(_lines(i), "onload ")

            End If
        Next i

        SetDefaultPlayerErrorMessages()

        SetUpSynonyms()
        SetUpRoomData()

        If _gameAslVersion >= 410 Then
            SetUpExits()
        End If

        If _gameAslVersion < 280 Then
            ' Set up an array containing the names of all the items
            ' used in the game, based on the possitems statement
            ' of the 'define game' block.
            SetUpItemArrays()
        End If

        If _gameAslVersion < 280 Then
            ' Now, go through the 'startitems' statement and set up
            ' the items array so we start with those items mentioned.
            SetUpStartItems()
        End If

        ' Set up collectables.
        SetUpCollectables()

        SetUpDisplayVariables()

        ' Set up characters and objects.
        SetUpCharObjectInfo()
        SetUpUserDefinedPlayerErrors()
        SetUpDefaultFonts()
        SetUpTurnScript()
        SetUpTimers()
        SetUpMenus()

        _gameFileName = afilename

        LogASLError("Finished loading file.", LogType.Init)

        _defaultRoomProperties = GetPropertiesInType("defaultroom", False)
        _defaultProperties = GetPropertiesInType("default", False)

        Return True
    End Function

    Private Sub LeaveRoom(ctx As Context)
        ' leaves the current room
        GoDirection("out", ctx)
    End Sub

    Private Function PlaceExist(PlaceName As String, ctx As Context) As String
        ' Returns actual name of an available "place" exit, and if
        ' script is executed on going in that direction, that script
        ' is returned after a ";"

        Dim RoomID As Integer
        Dim CheckPlace As String
        Dim DestRoomID As Integer
        Dim CheckPlaceName As String
        Dim FoundPlace, ScriptPresent As Boolean
        Dim i As Integer

        RoomID = GetRoomID(_currentRoom, ctx)
        FoundPlace = False
        ScriptPresent = False

        ' check if place is available
        Dim r = _rooms(RoomID)

        For i = 1 To r.NumberPlaces
            CheckPlace = r.Places(i).PlaceName

            'remove any prefix and semicolon
            If InStr(CheckPlace, ";") > 0 Then
                CheckPlace = Trim(Right(CheckPlace, Len(CheckPlace) - (InStr(CheckPlace, ";") + 1)))
            End If

            CheckPlaceName = CheckPlace

            If _gameAslVersion >= 311 And r.Places(i).Script = "" Then
                DestRoomID = GetRoomID(CheckPlace, ctx)
                If DestRoomID <> 0 Then
                    If _rooms(DestRoomID).RoomAlias <> "" Then
                        CheckPlaceName = _rooms(DestRoomID).RoomAlias
                    End If
                End If
            End If

            If LCase(CheckPlaceName) = LCase(PlaceName) Then
                FoundPlace = True

                If r.Places(i).Script <> "" Then
                    Return CheckPlace & ";" & r.Places(i).Script
                Else
                    Return CheckPlace
                End If
            End If
        Next i

        Return ""
    End Function

    Private Sub PlayerItem(anitem As String, gotit As Boolean, ctx As Context, Optional ObjID As Integer = 0)
        ' Gives the player an item (if gotit=True) or takes an
        ' item away from the player (if gotit=False).

        ' If ASL>280, setting gotit=TRUE moves specified
        ' *object* to room "inventory"; setting gotit=FALSe
        ' drops object into current room.

        Dim FoundObjectName As Boolean
        FoundObjectName = False
        Dim OldRoom As String
        Dim i As Integer

        If _gameAslVersion >= 280 Then
            If ObjID = 0 Then
                For i = 1 To _numberObjs
                    If LCase(_objs(i).ObjectName) = LCase(anitem) Then
                        ObjID = i
                        i = _numberObjs
                    End If
                Next i
            End If

            If ObjID <> 0 Then
                OldRoom = LCase(_objs(ObjID).ContainerRoom)
                If gotit Then
                    If _gameAslVersion >= 391 Then
                        ' Unset parent information, if any
                        AddToObjectProperties("not parent", ObjID, ctx)
                    End If
                    MoveThing(_objs(ObjID).ObjectName, "inventory", Thing.Object, ctx)

                    If _objs(ObjID).GainScript <> "" Then
                        ExecuteScript(_objs(ObjID).GainScript, ctx)
                    End If
                Else
                    MoveThing(_objs(ObjID).ObjectName, _currentRoom, Thing.Object, ctx)

                    If _objs(ObjID).LoseScript <> "" Then
                        ExecuteScript(_objs(ObjID).LoseScript, ctx)
                    End If

                End If

                FoundObjectName = True
            End If

            If Not FoundObjectName Then
                LogASLError("No such object '" & anitem & "'", LogType.WarningError)
            Else
                UpdateItems(ctx)
                UpdateObjectList(ctx)
            End If
        Else
            For i = 1 To _numberItems
                If _items(i).Name = anitem Then
                    _items(i).Got = gotit
                    i = _numberItems
                End If
            Next i

            UpdateItems(ctx)

        End If
    End Sub

    Friend Sub PlayGame(Room As String, ctx As Context)
        'plays the specified room

        Dim RoomID As Integer
        Dim RoomScript As String
        RoomID = GetRoomID(Room, ctx)

        If RoomID = 0 Then
            LogASLError("No such room '" & Room & "'", LogType.WarningError)
            Exit Sub
        End If

        Dim LastRoom As String
        LastRoom = _currentRoom

        _currentRoom = Room

        SetStringContents("quest.currentroom", Room, ctx)

        If _gameAslVersion >= 391 And _gameAslVersion < 410 Then
            AddToObjectProperties("visited", _rooms(RoomID).ObjId, ctx)
        End If

        ShowRoomInfo((Room), ctx)

        UpdateItems(ctx)

        ' Find script lines and execute them.

        If _rooms(RoomID).Script <> "" Then
            RoomScript = _rooms(RoomID).Script
            ExecuteScript(RoomScript, ctx)
        End If

        If _gameAslVersion >= 410 Then
            AddToObjectProperties("visited", _rooms(RoomID).ObjId, ctx)
        End If
    End Sub

    Friend Sub Print(txt As String, ctx As Context, Optional OutputTo As String = "normal", Optional NoTalk As Boolean = False)
        Dim i As Integer
        Dim PrintString As String
        Dim PrintThis As Boolean

        PrintString = ""

        If txt = "" Then
            DoPrint(PrintString)
        Else
            For i = 1 To Len(txt)

                PrintThis = True

                If Mid(txt, i, 2) = "|w" Then
                    DoPrint(PrintString)
                    PrintString = ""
                    PrintThis = False
                    i = i + 1

                    ExecuteScript("wait <>", ctx)

                ElseIf Mid(txt, i, 2) = "|c" Then
                    Select Case Mid(txt, i, 3)
                        Case "|cb", "|cr", "|cl", "|cy", "|cg"
                            ' Do nothing - we don't want to remove the colour formatting codes.
                        Case Else
                            DoPrint(PrintString)
                            PrintString = ""
                            PrintThis = False
                            i = i + 1

                            ExecuteScript("clear", ctx)
                    End Select
                End If

                If PrintThis Then PrintString = PrintString & Mid(txt, i, 1)

            Next i

            If PrintString <> "" Then DoPrint(PrintString)
        End If
    End Sub

    Private Function RetrLine(BlockType As String, blockparam As String, lineret As String, ctx As Context) As String
        'retrieves the line lineret in the block of type blocktype
        'with parameter blockparam in the current room/game block

        Dim i As Integer
        Dim searchblock As DefineBlock

        If BlockType = "object" Then
            searchblock = GetThingBlock(blockparam, _currentRoom, Thing.Object)
        Else
            searchblock = GetThingBlock(blockparam, _currentRoom, Thing.Character)
        End If

        If searchblock.StartLine = 0 And searchblock.EndLine = 0 Then
            Return "<undefined>"
        End If

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1
            If BeginsWith(_lines(i), lineret) Then
                Return Trim(_lines(i))
            End If
        Next i

        Return "<unfound>"
    End Function

    Private Function RetrLineParam(BlockType As String, blockparam As String, lineret As String, lineparam As String, ctx As Context) As String
        'retrieves the line lineret with parameter lineparam
        'in the block of type blocktype with parameter blockparam
        'in the current room - of course.

        Dim i As Integer
        Dim searchblock As DefineBlock

        If BlockType = "object" Then
            searchblock = GetThingBlock(blockparam, _currentRoom, Thing.Object)
        Else
            searchblock = GetThingBlock(blockparam, _currentRoom, Thing.Character)
        End If

        If searchblock.StartLine = 0 And searchblock.EndLine = 0 Then
            Return "<undefined>"
        End If

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1
            If BeginsWith(_lines(i), lineret) AndAlso LCase(GetParameter(_lines(i), ctx)) = LCase(lineparam) Then
                Return Trim(_lines(i))
            End If
        Next i

        Return "<unfound>"
    End Function

    Private Sub SetUpCollectables()
        Dim LastItem As Boolean
        Dim CharPos, a, NextComma As Integer
        Dim PossItems As String
        Dim CInfo, T As String
        Dim SpacePos, EqualsPos, Space2Pos As Integer
        Dim i, b As String
        Dim Bpos1, BPos2 As Integer
        LastItem = False

        _numCollectables = 0

        ' Initialise collectables:
        ' First, find the collectables section within the define
        ' game block, and get its parameters:

        For a = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(a), "collectables ") Then
                PossItems = Trim(GetParameter(_lines(a), _nullContext, False))

                ' if collectables is a null string, there are no
                ' collectables. Otherwise, there is one more object than
                ' the number of commas. So, first check to see if we have
                ' no objects:

                If PossItems <> "" Then
                    _numCollectables = 1
                    CharPos = 1
                    Do
                        ReDim Preserve _collectables(_numCollectables)
                        _collectables(_numCollectables) = New Collectable
                        NextComma = InStr(CharPos + 1, PossItems, ",")
                        If NextComma = 0 Then
                            NextComma = InStr(CharPos + 1, PossItems, ";")
                        End If

                        'If there are no more commas, we want everything
                        'up to the end of the string, and then to exit
                        'the loop:
                        If NextComma = 0 Then
                            NextComma = Len(PossItems) + 1
                            LastItem = True
                        End If

                        'Get item info
                        CInfo = Trim(Mid(PossItems, CharPos, NextComma - CharPos))
                        _collectables(_numCollectables).Name = Trim(Left(CInfo, InStr(CInfo, " ")))

                        EqualsPos = InStr(CInfo, "=")
                        SpacePos = InStr(CInfo, " ")
                        Space2Pos = InStr(EqualsPos, CInfo, " ")
                        If Space2Pos = 0 Then Space2Pos = Len(CInfo) + 1
                        T = Trim(Mid(CInfo, SpacePos + 1, EqualsPos - SpacePos - 1))
                        i = Trim(Mid(CInfo, EqualsPos + 1, Space2Pos - EqualsPos - 1))

                        If Left(T, 1) = "d" Then
                            T = Mid(T, 2)
                            _collectables(_numCollectables).DisplayWhenZero = False
                        Else
                            _collectables(_numCollectables).DisplayWhenZero = True
                        End If

                        _collectables(_numCollectables).Type = T
                        _collectables(_numCollectables).Value = Val(i)

                        ' Get display string between square brackets
                        Bpos1 = InStr(CInfo, "[")
                        BPos2 = InStr(CInfo, "]")
                        If Bpos1 = 0 Then
                            _collectables(_numCollectables).Display = "<def>"
                        Else
                            b = Mid(CInfo, Bpos1 + 1, (BPos2 - 1) - Bpos1)
                            _collectables(_numCollectables).Display = Trim(b)
                        End If

                        CharPos = NextComma + 1
                        _numCollectables = _numCollectables + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                    _numCollectables = _numCollectables - 1
                End If
            End If
        Next a
    End Sub

    Private Sub SetUpItemArrays()
        Dim LastItem As Boolean
        Dim CharPos, a, NextComma As Integer
        Dim PossItems As String
        LastItem = False

        _numberItems = 0

        ' Initialise items:
        ' First, find the possitems section within the define game
        ' block, and get its parameters:
        For a = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(a), "possitems ") Or BeginsWith(_lines(a), "items ") Then
                PossItems = GetParameter(_lines(a), _nullContext)

                If PossItems <> "" Then
                    _numberItems = _numberItems + 1
                    CharPos = 1
                    Do
                        ReDim Preserve _items(_numberItems)
                        _items(_numberItems) = New ItemType
                        NextComma = InStr(CharPos + 1, PossItems, ",")
                        If NextComma = 0 Then
                            NextComma = InStr(CharPos + 1, PossItems, ";")
                        End If

                        'If there are no more commas, we want everything
                        'up to the end of the string, and then to exit
                        'the loop:
                        If NextComma = 0 Then
                            NextComma = Len(PossItems) + 1
                            LastItem = True
                        End If

                        'Get item name
                        _items(_numberItems).Name = Trim(Mid(PossItems, CharPos, NextComma - CharPos))
                        _items(_numberItems).Got = False

                        CharPos = NextComma + 1
                        _numberItems = _numberItems + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                    _numberItems = _numberItems - 1
                End If
            End If
        Next a
    End Sub

    Private Sub SetUpStartItems()
        Dim CharPos, a, NextComma As Integer
        Dim StartItems As String
        Dim LastItem As Boolean
        Dim TheItemName As String
        Dim i As Integer

        For a = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(a), "startitems ") Then
                StartItems = GetParameter(_lines(a), _nullContext)

                If StartItems <> "" Then
                    CharPos = 1
                    Do
                        NextComma = InStr(CharPos + 1, StartItems, ",")
                        If NextComma = 0 Then
                            NextComma = InStr(CharPos + 1, StartItems, ";")
                        End If

                        'If there are no more commas, we want everything
                        'up to the end of the string, and then to exit
                        'the loop:
                        If NextComma = 0 Then
                            NextComma = Len(StartItems) + 1
                            LastItem = True
                        End If

                        'Get item name
                        TheItemName = Trim(Mid(StartItems, CharPos, NextComma - CharPos))

                        'Find which item this is, and set it
                        For i = 1 To _numberItems
                            If _items(i).Name = TheItemName Then
                                _items(i).Got = True
                                i = _numberItems
                            End If
                        Next i

                        CharPos = NextComma + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                End If
            End If
        Next a
    End Sub

    Private Sub ShowHelp(ctx As Context)
        ' In Quest 4 and below, the help text displays in a separate window. In Quest 5, it displays
        ' in the same window as the game text.
        Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx, "help")
        Print("", ctx, "help")
        Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", ctx, "help")
        Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", ctx, "help")
        Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", ctx, "help")
        Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", ctx, "help")
        Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", ctx, "help")
        Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n", ctx, "help")
        Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", ctx, "help")
    End Sub

    Private Sub ReadCatalog(CatData As String)
        Dim i, Chr0Pos, ResourceStart As Integer

        Chr0Pos = InStr(CatData, Chr(0))
        _numResources = CInt(DecryptString(Left(CatData, Chr0Pos - 1)))
        ReDim Preserve _resources(_numResources)
        _resources(_numResources) = New ResourceType

        CatData = Mid(CatData, Chr0Pos + 1)

        ResourceStart = 0

        For i = 1 To _numResources
            Dim r = _resources(i)
            Chr0Pos = InStr(CatData, Chr(0))
            r.ResourceName = DecryptString(Left(CatData, Chr0Pos - 1))
            CatData = Mid(CatData, Chr0Pos + 1)

            Chr0Pos = InStr(CatData, Chr(0))
            r.ResourceLength = CInt(DecryptString(Left(CatData, Chr0Pos - 1)))
            CatData = Mid(CatData, Chr0Pos + 1)

            r.ResourceStart = ResourceStart
            ResourceStart = ResourceStart + r.ResourceLength

            r.Extracted = False
        Next i
    End Sub

    Private Sub UpdateDirButtons(AvailableDirs As String, ctx As Context)
        Dim compassExits As New List(Of ListData)

        If InStr(AvailableDirs, "n") > 0 Then
            AddCompassExit(compassExits, "north")
        End If

        If InStr(AvailableDirs, "s") > 0 Then
            AddCompassExit(compassExits, "south")
        End If

        If InStr(AvailableDirs, "w") > 0 Then
            AddCompassExit(compassExits, "west")
        End If

        If InStr(AvailableDirs, "e") > 0 Then
            AddCompassExit(compassExits, "east")
        End If

        If InStr(AvailableDirs, "o") > 0 Then
            AddCompassExit(compassExits, "out")
        End If

        If InStr(AvailableDirs, "a") > 0 Then
            AddCompassExit(compassExits, "northeast")
        End If

        If InStr(AvailableDirs, "b") > 0 Then
            AddCompassExit(compassExits, "northwest")
        End If

        If InStr(AvailableDirs, "c") > 0 Then
            AddCompassExit(compassExits, "southeast")
        End If

        If InStr(AvailableDirs, "d") > 0 Then
            AddCompassExit(compassExits, "southwest")
        End If

        If InStr(AvailableDirs, "u") > 0 Then
            AddCompassExit(compassExits, "up")
        End If

        If InStr(AvailableDirs, "f") > 0 Then
            AddCompassExit(compassExits, "down")
        End If

        _compassExits = compassExits
        UpdateExitsList()
    End Sub

    Private Sub AddCompassExit(exitList As List(Of ListData), name As String)
        exitList.Add(New ListData(name, _listVerbs(ListType.ExitsList)))
    End Sub

    Private Function UpdateDoorways(RoomID As Integer, ctx As Context) As String
        Dim RoomDisplayText As String = "", OutPlace As String = ""
        Dim SCP As Integer
        Dim Directions As String = "", NSEW As String = "", OutPlaceName As String = ""
        Dim OutPlaceAlias As String
        Dim CommaPos As Integer
        Dim bFinNSEW As Boolean
        Dim NewCommaPos As Integer
        Dim OutPlacePrefix As String = ""

        Dim e, n, s, W As String
        Dim SE, NE, NW, SW As String
        Dim D, U, O As String

        n = "north"
        s = "south"
        e = "east"
        W = "west"
        NE = "northeast"
        NW = "northwest"
        SE = "southeast"
        SW = "southwest"
        U = "up"
        D = "down"
        O = "out"

        If _gameAslVersion >= 410 Then
            _rooms(RoomID).Exits.GetAvailableDirectionsDescription(RoomDisplayText, Directions)
        Else

            If _rooms(RoomID).Out.Text <> "" Then
                OutPlace = _rooms(RoomID).Out.Text

                'remove any prefix semicolon from printed text
                SCP = InStr(OutPlace, ";")
                If SCP = 0 Then
                    OutPlaceName = OutPlace
                Else
                    OutPlaceName = Trim(Mid(OutPlace, SCP + 1))
                    OutPlacePrefix = Trim(Left(OutPlace, SCP - 1))
                    OutPlace = OutPlacePrefix & " " & OutPlaceName
                End If
            End If

            If _rooms(RoomID).North.Data <> "" Then
                NSEW = NSEW & "|b" & n & "|xb, "
                Directions = Directions & "n"
            End If
            If _rooms(RoomID).South.Data <> "" Then
                NSEW = NSEW & "|b" & s & "|xb, "
                Directions = Directions & "s"
            End If
            If _rooms(RoomID).East.Data <> "" Then
                NSEW = NSEW & "|b" & e & "|xb, "
                Directions = Directions & "e"
            End If
            If _rooms(RoomID).West.Data <> "" Then
                NSEW = NSEW & "|b" & W & "|xb, "
                Directions = Directions & "w"
            End If
            If _rooms(RoomID).NorthEast.Data <> "" Then
                NSEW = NSEW & "|b" & NE & "|xb, "
                Directions = Directions & "a"
            End If
            If _rooms(RoomID).NorthWest.Data <> "" Then
                NSEW = NSEW & "|b" & NW & "|xb, "
                Directions = Directions & "b"
            End If
            If _rooms(RoomID).SouthEast.Data <> "" Then
                NSEW = NSEW & "|b" & SE & "|xb, "
                Directions = Directions & "c"
            End If
            If _rooms(RoomID).SouthWest.Data <> "" Then
                NSEW = NSEW & "|b" & SW & "|xb, "
                Directions = Directions & "d"
            End If
            If _rooms(RoomID).Up.Data <> "" Then
                NSEW = NSEW & "|b" & U & "|xb, "
                Directions = Directions & "u"
            End If
            If _rooms(RoomID).Down.Data <> "" Then
                NSEW = NSEW & "|b" & D & "|xb, "
                Directions = Directions & "f"
            End If

            If OutPlace <> "" Then
                'see if outside has an alias

                OutPlaceAlias = _rooms(GetRoomID(OutPlaceName, ctx)).RoomAlias
                If OutPlaceAlias = "" Then
                    OutPlaceAlias = OutPlace
                Else
                    If _gameAslVersion >= 360 Then
                        If OutPlacePrefix <> "" Then
                            OutPlaceAlias = OutPlacePrefix & " " & OutPlaceAlias
                        End If
                    End If
                End If

                RoomDisplayText = RoomDisplayText & "You can go |bout|xb to " & OutPlaceAlias & "."
                If NSEW <> "" Then RoomDisplayText = RoomDisplayText & " "

                Directions = Directions & "o"
                If _gameAslVersion >= 280 Then
                    SetStringContents("quest.doorways.out", OutPlaceName, ctx)
                Else
                    SetStringContents("quest.doorways.out", OutPlaceAlias, ctx)
                End If
                SetStringContents("quest.doorways.out.display", OutPlaceAlias, ctx)
            Else
                SetStringContents("quest.doorways.out", "", ctx)
                SetStringContents("quest.doorways.out.display", "", ctx)
            End If

            If NSEW <> "" Then
                'strip final comma
                NSEW = Left(NSEW, Len(NSEW) - 2)
                CommaPos = InStr(NSEW, ",")
                If CommaPos <> 0 Then
                    bFinNSEW = False
                    Do
                        NewCommaPos = InStr(CommaPos + 1, NSEW, ",")
                        If NewCommaPos = 0 Then
                            bFinNSEW = True
                        Else
                            CommaPos = NewCommaPos
                        End If
                    Loop Until bFinNSEW

                    NSEW = Trim(Left(NSEW, CommaPos - 1)) & " or " & Trim(Mid(NSEW, CommaPos + 1))
                End If

                RoomDisplayText = RoomDisplayText & "You can go " & NSEW & "."
                SetStringContents("quest.doorways.dirs", NSEW, ctx)
            Else
                SetStringContents("quest.doorways.dirs", "", ctx)
            End If
        End If

        UpdateDirButtons(Directions, ctx)

        Return RoomDisplayText
    End Function

    Private Sub UpdateItems(ctx As Context)
        ' displays the items a player has
        Dim i, j As Integer
        Dim k As String
        Dim invList As New List(Of ListData)

        If Not _outPutOn Then Exit Sub

        Dim CurObjName As String

        If _gameAslVersion >= 280 Then
            For i = 1 To _numberObjs
                If _objs(i).ContainerRoom = "inventory" And _objs(i).Exists And _objs(i).Visible Then
                    If _objs(i).ObjectAlias = "" Then
                        CurObjName = _objs(i).ObjectName
                    Else
                        CurObjName = _objs(i).ObjectAlias
                    End If

                    invList.Add(New ListData(CapFirst(CurObjName), _listVerbs(ListType.InventoryList)))

                End If
            Next i
        Else
            For j = 1 To _numberItems
                If _items(j).Got = True Then
                    invList.Add(New ListData(CapFirst(_items(j).Name), _listVerbs(ListType.InventoryList)))
                End If
            Next j
        End If

        RaiseEvent UpdateList(ListType.InventoryList, invList)

        If _gameAslVersion >= 284 Then
            UpdateStatusVars(ctx)
        Else
            If _numCollectables > 0 Then

                Dim status As String = ""

                For j = 1 To _numCollectables
                    k = DisplayCollectableInfo(j)
                    If k <> "<null>" Then
                        If status.Length > 0 Then status += Environment.NewLine
                        status += k
                    End If
                Next j

                _player.SetStatusText(status)

            End If
        End If
    End Sub

    Private Sub FinishGame(wingame As StopType, ctx As Context)
        If wingame = StopType.Win Then
            DisplayTextSection("win", ctx)
        ElseIf wingame = StopType.Lose Then
            DisplayTextSection("lose", ctx)
        End If

        GameFinished()
    End Sub

    Private Sub UpdateObjectList(ctx As Context)
        ' Updates object list
        Dim i, PlaceID As Integer
        Dim ShownPlaceName As String
        Dim ObjSuffix As String, CharsViewable As String = ""
        Dim CharsFound As Integer
        Dim NoFormatObjsViewable, CharList As String, ObjsViewable As String = ""
        Dim ObjsFound As Integer
        Dim ObjListString, NFObjListString As String

        If Not _outPutOn Then Exit Sub

        Dim objList As New List(Of ListData)
        Dim exitList As New List(Of ListData)

        'find the room
        Dim roomblock As DefineBlock
        roomblock = DefineBlockParam("room", _currentRoom)

        'FIND CHARACTERS ===
        If _gameAslVersion < 281 Then
            ' go through Chars() array
            For i = 1 To _numberChars
                If _chars(i).ContainerRoom = _currentRoom And _chars(i).Exists And _chars(i).Visible Then
                    AddToObjectList(objList, exitList, _chars(i).ObjectName, Thing.Character)
                    CharsViewable = CharsViewable & _chars(i).Prefix & "|b" & _chars(i).ObjectName & "|xb" & _chars(i).Suffix & ", "
                    CharsFound = CharsFound + 1
                End If
            Next i

            If CharsFound = 0 Then
                SetStringContents("quest.characters", "", ctx)
            Else
                'chop off final comma and add full stop (.)
                CharList = Left(CharsViewable, Len(CharsViewable) - 2)
                SetStringContents("quest.characters", CharList, ctx)
            End If
        End If

        'FIND OBJECTS
        NoFormatObjsViewable = ""

        For i = 1 To _numberObjs
            If LCase(_objs(i).ContainerRoom) = LCase(_currentRoom) And _objs(i).Exists And _objs(i).Visible And Not _objs(i).IsExit Then
                ObjSuffix = _objs(i).Suffix
                If ObjSuffix <> "" Then ObjSuffix = " " & ObjSuffix
                If _objs(i).ObjectAlias = "" Then
                    AddToObjectList(objList, exitList, _objs(i).ObjectName, Thing.Object)
                    ObjsViewable = ObjsViewable & _objs(i).Prefix & "|b" & _objs(i).ObjectName & "|xb" & ObjSuffix & ", "
                    NoFormatObjsViewable = NoFormatObjsViewable & _objs(i).Prefix & _objs(i).ObjectName & ", "
                Else
                    AddToObjectList(objList, exitList, _objs(i).ObjectAlias, Thing.Object)
                    ObjsViewable = ObjsViewable & _objs(i).Prefix & "|b" & _objs(i).ObjectAlias & "|xb" & ObjSuffix & ", "
                    NoFormatObjsViewable = NoFormatObjsViewable & _objs(i).Prefix & _objs(i).ObjectAlias & ", "
                End If
                ObjsFound = ObjsFound + 1
            End If
        Next i

        If ObjsFound <> 0 Then
            ObjListString = Left(ObjsViewable, Len(ObjsViewable) - 2)
            NFObjListString = Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2)
            SetStringContents("quest.objects", Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2), ctx)
            SetStringContents("quest.formatobjects", ObjListString, ctx)
        Else
            SetStringContents("quest.objects", "", ctx)
            SetStringContents("quest.formatobjects", "", ctx)
        End If

        'FIND DOORWAYS
        Dim RoomID As Integer
        RoomID = GetRoomID(_currentRoom, ctx)

        Dim r = _rooms(RoomID)

        If _gameAslVersion >= 410 Then

            If RoomID > 0 Then
                For Each oExit As RoomExit In _rooms(RoomID).Exits.Places.Values
                    AddToObjectList(objList, exitList, oExit.DisplayName, Thing.Room)
                Next
            End If

        Else
            For i = 1 To r.NumberPlaces

                If _gameAslVersion >= 311 And _rooms(RoomID).Places(i).Script = "" Then
                    PlaceID = GetRoomID(_rooms(RoomID).Places(i).PlaceName, ctx)
                    If PlaceID = 0 Then
                        ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
                    Else
                        If _rooms(PlaceID).RoomAlias <> "" Then
                            ShownPlaceName = _rooms(PlaceID).RoomAlias
                        Else
                            ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
                        End If
                    End If
                Else
                    ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
                End If

                AddToObjectList(objList, exitList, ShownPlaceName, Thing.Room)
            Next i
        End If

        RaiseEvent UpdateList(ListType.ObjectsList, objList)
        _gotoExits = exitList
        UpdateExitsList()
    End Sub

    Private Sub UpdateExitsList()
        ' The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        ' ASL4 code produces these separately. So we keep track of them separately and then
        ' merge to send to the Player.

        Dim mergedList As New List(Of ListData)

        For Each listItem As ListData In _compassExits
            mergedList.Add(listItem)
        Next

        For Each listItem As ListData In _gotoExits
            mergedList.Add(listItem)
        Next

        RaiseEvent UpdateList(ListType.ExitsList, mergedList)
    End Sub

    Private Sub UpdateStatusVars(ctx As Context)
        Dim DisplayData As String
        Dim i As Integer
        Dim status As String = ""

        If _numDisplayStrings > 0 Then
            For i = 1 To _numDisplayStrings
                DisplayData = DisplayStatusVariableInfo(i, VarType.String, ctx)

                If DisplayData <> "" Then
                    If status.Length > 0 Then status += Environment.NewLine
                    status += DisplayData
                End If
            Next i
        End If

        If _numDisplayNumerics > 0 Then
            For i = 1 To _numDisplayNumerics
                DisplayData = DisplayStatusVariableInfo(i, VarType.Numeric, ctx)
                If DisplayData <> "" Then
                    If status.Length > 0 Then status += Environment.NewLine
                    status += DisplayData
                End If
            Next i
        End If

        _player.SetStatusText(status)
    End Sub

    Private Sub UpdateVisibilityInContainers(ctx As Context, Optional OnlyParent As String = "")
        ' Use OnlyParent to only update objects that are contained by a specific parent

        Dim i, ParentID As Integer
        Dim Parent As String
        Dim ParentIsTransparent, ParentIsOpen, ParentIsSeen As Boolean
        Dim ParentIsSurface As Boolean

        If _gameAslVersion < 391 Then Exit Sub

        If OnlyParent <> "" Then
            OnlyParent = LCase(OnlyParent)
            ParentID = GetObjectIdNoAlias(OnlyParent)

            ParentIsOpen = IsYes(GetObjectProperty("opened", ParentID, True, False))
            ParentIsTransparent = IsYes(GetObjectProperty("transparent", ParentID, True, False))
            ParentIsSeen = IsYes(GetObjectProperty("seen", ParentID, True, False))
            ParentIsSurface = IsYes(GetObjectProperty("surface", ParentID, True, False))
        End If

        For i = 1 To _numberObjs
            ' If object has a parent object
            Parent = GetObjectProperty("parent", i, False, False)

            If Parent <> "" Then

                ' Check if that parent is open, or transparent
                If OnlyParent = "" Then
                    ParentID = GetObjectIdNoAlias(Parent)
                    ParentIsOpen = IsYes(GetObjectProperty("opened", ParentID, True, False))
                    ParentIsTransparent = IsYes(GetObjectProperty("transparent", ParentID, True, False))
                    ParentIsSeen = IsYes(GetObjectProperty("seen", ParentID, True, False))
                    ParentIsSurface = IsYes(GetObjectProperty("surface", ParentID, True, False))
                End If

                If OnlyParent = "" Or (LCase(Parent) = OnlyParent) Then

                    If ParentIsSurface Or ((ParentIsOpen Or ParentIsTransparent) And ParentIsSeen) Then
                        ' If the parent is a surface, then the contents are always available.
                        ' Otherwise, only if the parent has been seen, AND is either open or transparent,
                        ' then the contents are available.

                        SetAvailability(_objs(i).ObjectName, True, ctx)
                    Else
                        SetAvailability(_objs(i).ObjectName, False, ctx)
                    End If

                End If
            End If
        Next i

    End Sub

    ' TODO: Fix - ErrorMsg was ByRef
    Private Function PlayerCanAccessObject(ObjID As Integer, Optional ParentID As Integer = 0, Optional ErrorMsg As String = "", Optional colObjects As List(Of Integer) = Nothing) As Boolean
        ' Called to see if a player can interact with an object (take it, open it etc.).
        ' For example, if the object is on a surface which is inside a closed container,
        ' the object cannot be accessed.

        Dim Parent As String
        Dim ParentDisplayName As String

        Dim sHierarchy As String = ""
        If IsYes(GetObjectProperty("parent", ObjID, True, False)) Then

            ' Object is in a container...

            Parent = GetObjectProperty("parent", ObjID, False, False)
            ParentID = GetObjectIdNoAlias(Parent)

            ' But if it's a surface then it's OK

            If Not IsYes(GetObjectProperty("surface", ParentID, True, False)) And Not IsYes(GetObjectProperty("opened", ParentID, True, False)) Then
                ' Parent has no "opened" property, so it's closed. Hence
                ' object can't be accessed

                If _objs(ParentID).ObjectAlias <> "" Then
                    ParentDisplayName = _objs(ParentID).ObjectAlias
                Else
                    ParentDisplayName = _objs(ParentID).ObjectName
                End If

                ErrorMsg = "inside closed " & ParentDisplayName

                Return False
            End If

            ' Is the parent itself accessible?
            If colObjects Is Nothing Then
                colObjects = New List(Of Integer)
            End If

            If colObjects.Contains(ParentID) Then
                ' We've already encountered this parent while recursively calling
                ' this function - we're in a loop of parents!
                For Each id As Integer In colObjects
                    sHierarchy = sHierarchy & _objs(id).ObjectName & " -> "
                Next
                sHierarchy = sHierarchy & _objs(ParentID).ObjectName
                LogASLError("Looped object parents detected: " & sHierarchy)
                Return False
            End If

            colObjects.Add(ParentID)

            If Not PlayerCanAccessObject(ParentID, , ErrorMsg, colObjects) Then
                Return False
            End If

            Return True
        End If

        Return True
    End Function

    Private Function GetGoToExits(RoomID As Integer, ctx As Context) As String
        Dim i As Integer
        Dim PlaceID As Integer
        Dim PlaceList As String = ""
        Dim ShownPrefix As String
        Dim ShownPlaceName As String

        For i = 1 To _rooms(RoomID).NumberPlaces
            If _gameAslVersion >= 311 And _rooms(RoomID).Places(i).Script = "" Then
                PlaceID = GetRoomID(_rooms(RoomID).Places(i).PlaceName, ctx)
                If PlaceID = 0 Then
                    LogASLError("No such room '" & _rooms(RoomID).Places(i).PlaceName & "'", LogType.WarningError)
                    ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
                Else
                    If _rooms(PlaceID).RoomAlias <> "" Then
                        ShownPlaceName = _rooms(PlaceID).RoomAlias
                    Else
                        ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
                    End If
                End If
            Else
                ShownPlaceName = _rooms(RoomID).Places(i).PlaceName
            End If

            ShownPrefix = _rooms(RoomID).Places(i).Prefix
            If ShownPrefix <> "" Then ShownPrefix = ShownPrefix & " "

            PlaceList = PlaceList & ShownPrefix & "|b" & ShownPlaceName & "|xb, "
        Next i

        Return PlaceList
    End Function

    Private Sub SetUpExits()
        ' Exits have to be set up after all the rooms have been initialised

        Dim i As Integer
        Dim j As Integer
        Dim sRoomName As String
        Dim lRoomID As Integer
        Dim NestedBlock As Integer

        For i = 1 To _numberSections
            If BeginsWith(_lines(_defineBlocks(i).StartLine), "define room ") Then
                sRoomName = GetParameter(_lines(_defineBlocks(i).StartLine), _nullContext)
                lRoomID = GetRoomID(sRoomName, _nullContext)

                For j = _defineBlocks(i).StartLine + 1 To _defineBlocks(i).EndLine - 1
                    If BeginsWith(_lines(j), "define ") Then
                        'skip nested blocks
                        NestedBlock = 1
                        Do
                            j = j + 1
                            If BeginsWith(_lines(j), "define ") Then
                                NestedBlock = NestedBlock + 1
                            ElseIf Trim(_lines(j)) = "end define" Then
                                NestedBlock = NestedBlock - 1
                            End If
                        Loop Until NestedBlock = 0
                    End If

                    _rooms(lRoomID).Exits.AddExitFromTag(_lines(j))
                Next j
            End If
        Next i

        Exit Sub

    End Sub

    Private Function FindExit(sTag As String) As RoomExit
        ' e.g. Takes a tag of the form "room; north" and return's the north exit of room.

        Dim sRoom As String
        Dim sExit As String
        Dim asParams() As String
        Dim lRoomID As Integer
        Dim lDir As Direction

        asParams = Split(sTag, ";")
        If UBound(asParams) < 1 Then
            LogASLError("No exit specified in '" & sTag & "'", LogType.WarningError)
            Return New RoomExit(Me)
        End If

        sRoom = Trim(asParams(0))
        sExit = Trim(asParams(1))

        lRoomID = GetRoomID(sRoom, _nullContext)

        If lRoomID = 0 Then
            LogASLError("Can't find room '" & sRoom & "'", LogType.WarningError)
            Return Nothing
        End If

        Dim exits = _rooms(lRoomID).Exits
        lDir = exits.GetDirectionEnum(sExit)
        If lDir = Direction.None Then
            If exits.Places.ContainsKey(sExit) Then
                Return exits.Places.Item(sExit)
            End If
        Else
            Return exits.GetDirectionExit(lDir)
        End If

        Return Nothing
    End Function

    Private Sub ExecuteLock(sTag As String, bLock As Boolean)
        Dim oExit As RoomExit

        oExit = FindExit(sTag)

        If oExit Is Nothing Then
            LogASLError("Can't find exit '" & sTag & "'", LogType.WarningError)
            Exit Sub
        End If

        oExit.IsLocked = bLock
    End Sub

    Public Sub Begin() Implements IASL.Begin
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf DoBegin))
        ChangeState(State.Working)
        runnerThread.Start()

        SyncLock _stateLock
            While _state = State.Working And Not _gameFinished
                System.Threading.Monitor.Wait(_stateLock)
            End While
        End SyncLock
    End Sub

    Private Sub DoBegin()
        Dim gameblock As DefineBlock = GetDefineBlock("game")
        Dim NewThread As New Context
        Dim i As Integer

        SetFont("")
        SetFontSize(0)

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(i), "background ") Then
                SetBackground(GetParameter(_lines(i), _nullContext))
            End If
        Next i

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(_lines(i), "foreground ") Then
                SetForeground(GetParameter(_lines(i), _nullContext))
            End If
        Next i

        ' Execute any startscript command that appears in the
        ' "define game" block:

        _autoIntro = True

        ' For ASL>=391, we only run startscripts if LoadMethod is normal (i.e. we haven't started
        ' from a saved QSG file)

        If _gameAslVersion < 391 Or (_gameAslVersion >= 391 And _gameLoadMethod = "normal") Then

            ' for GameASLVersion 311 and later, any library startscript is executed first:
            If _gameAslVersion >= 311 Then
                ' We go through the game block executing these in reverse order, as
                ' the statements which are included last should be executed first.
                For i = gameblock.EndLine - 1 To gameblock.StartLine + 1 Step -1
                    If BeginsWith(_lines(i), "lib startscript ") Then
                        NewThread = _nullContext
                        ExecuteScript(Trim(GetEverythingAfter(Trim(_lines(i)), "lib startscript ")), NewThread)
                    End If
                Next i
            End If

            For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
                If BeginsWith(_lines(i), "startscript ") Then
                    NewThread = _nullContext
                    ExecuteScript(Trim(GetEverythingAfter(Trim(_lines(i)), "startscript")), NewThread)
                ElseIf BeginsWith(_lines(i), "lib startscript ") And _gameAslVersion < 311 Then
                    NewThread = _nullContext
                    ExecuteScript(Trim(GetEverythingAfter(Trim(_lines(i)), "lib startscript ")), NewThread)
                End If
            Next i

        End If

        _gameFullyLoaded = True

        ' Display intro text
        If _autoIntro And _gameLoadMethod = "normal" Then DisplayTextSection("intro", _nullContext)

        ' Start game from room specified by "start" statement
        Dim StartRoom As String = ""
        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(_lines(i), "start ") Then
                StartRoom = GetParameter(_lines(i), _nullContext)
            End If
        Next i

        If Not _loadedFromQsg Then
            NewThread = _nullContext
            PlayGame(StartRoom, NewThread)
            Print("", _nullContext)
        Else
            UpdateItems(_nullContext)

            Print("Restored saved game", _nullContext)
            Print("", _nullContext)
            PlayGame(_currentRoom, _nullContext)
            Print("", _nullContext)

            If _gameAslVersion >= 391 Then
                ' For ASL>=391, OnLoad is now run for all games.
                NewThread = _nullContext
                ExecuteScript(_onLoadScript, NewThread)
            End If

        End If

        RaiseNextTimerTickRequest()

        ChangeState(State.Ready)
    End Sub

    Public ReadOnly Property Errors As System.Collections.Generic.List(Of String) Implements IASL.Errors
        Get
            Return New List(Of String)()
        End Get
    End Property

    Public ReadOnly Property Filename As String Implements IASL.Filename
        Get
            Return _filename
        End Get
    End Property

    Public Sub Finish() Implements IASL.Finish
        GameFinished()
    End Sub

    Public Event Finished() Implements IASL.Finished

    Public Event LogError(errorMessage As String) Implements IASL.LogError

    Public Event PrintText(text As String) Implements IASL.PrintText

    Public Sub Save(filename As String, html As String) Implements IASL.Save
        SaveGame(filename)
    End Sub

    Public Function Save(html As String) As Byte() Implements IASL.Save
        Return SaveGame(Filename, False)
    End Function

    Public ReadOnly Property SaveFilename As String Implements IASL.SaveFilename
        Get
            Return _saveGameFile
        End Get
    End Property

    Public Sub SendCommand(command As String) Implements IASL.SendCommand
        SendCommand(command, 0, Nothing)
    End Sub

    Public Sub SendCommand(command As String, metadata As IDictionary(Of String, String)) Implements IASL.SendCommand
        SendCommand(command, 0, metadata)
    End Sub

    Public Sub SendCommand(command As String, elapsedTime As Integer, metadata As IDictionary(Of String, String)) Implements IASLTimer.SendCommand
        ' The processing of commands is done in a separate thread, so things like the "enter" command can
        ' lock the thread while waiting for further input. After starting to process the command, we wait
        ' for something to happen before returning from the SendCommand call - either the command will have
        ' finished processing, or perhaps a prompt has been printed and now the game is waiting for further
        ' user input after hitting an "enter" script command.

        If Not _readyForCommand Then Exit Sub

        Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf ProcessCommandInNewThread))
        ChangeState(State.Working)
        runnerThread.Start(command)

        WaitForStateChange(State.Working)

        If elapsedTime > 0 Then
            Tick(elapsedTime)
        Else
            RaiseNextTimerTickRequest()
        End If

    End Sub

    Private Sub WaitForStateChange(changedFromState As State)
        SyncLock _stateLock
            While _state = changedFromState And Not _gameFinished
                System.Threading.Monitor.Wait(_stateLock)
            End While
        End SyncLock

    End Sub

    Private Sub ProcessCommandInNewThread(command As Object)
        ' Process command, and change state to Ready if the command finished processing

        Try
            If ExecCommand(DirectCast(command, String), New Context) Then
                ChangeState(State.Ready)
            End If
        Catch ex As Exception
            LogException(ex)
            ChangeState(State.Ready)
        End Try
    End Sub

    Public Sub SendEvent(eventName As String, param As String) Implements IASL.SendEvent

    End Sub

    Public Event UpdateList(listType As ListType, items As System.Collections.Generic.List(Of ListData)) Implements IASL.UpdateList

    Public Function Initialise(player As IPlayer) As Boolean Implements IASL.Initialise
        _player = player
        If LCase(Right(_filename, 4)) = ".qsg" Or _data IsNot Nothing Then
            Return OpenGame(_filename)
        Else
            Return InitialiseGame(_filename)
        End If
    End Function

    Private Sub GameFinished()
        _gameFinished = True
        RaiseEvent Finished()
        ChangeState(State.Finished)

        ' In case we're in the middle of processing an "enter" command, nudge the thread along
        SyncLock _commandLock
            System.Threading.Monitor.PulseAll(_commandLock)
        End SyncLock

        SyncLock _waitLock
            System.Threading.Monitor.PulseAll(_waitLock)
        End SyncLock

        SyncLock _stateLock
            System.Threading.Monitor.PulseAll(_stateLock)
        End SyncLock

        Cleanup()
    End Sub

    Private Function GetResourcePath(filename As String) As String Implements IASL.GetResourcePath
        If Not _resourceFile Is Nothing AndAlso _resourceFile.Length > 0 Then
            Dim extractResult As String = ExtractFile(filename)
            Return extractResult
        End If
        Return System.IO.Path.Combine(_gamePath, filename)
    End Function

    Private Sub Cleanup()
        DeleteDirectory(_tempFolder)
    End Sub

    Private Sub DeleteDirectory(dir As String)
        If System.IO.Directory.Exists(dir) Then
            Try
                System.IO.Directory.Delete(dir, True)
            Catch
            End Try
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Cleanup()
        MyBase.Finalize()
    End Sub

    Private Function GetLibraryLines(libName As String) As String()
        Dim libCode As Byte() = Nothing
        libName = LCase(libName)

        Select Case libName
            Case "stdverbs.lib"
                libCode = My.Resources.stdverbs
            Case "standard.lib"
                libCode = My.Resources.standard
            Case "q3ext.qlb"
                libCode = My.Resources.q3ext
            Case "typelib.qlb"
                libCode = My.Resources.Typelib
            Case "net.lib"
                libCode = My.Resources.net
        End Select

        If libCode Is Nothing Then Return Nothing

        Return GetResourceLines(libCode)
    End Function

    Public ReadOnly Property SaveExtension As String Implements IASL.SaveExtension
        Get
            Return "qsg"
        End Get
    End Property

    Public Sub Tick(elapsedTime As Integer) Implements IASLTimer.Tick
        Dim i As Integer
        Dim TimerScripts As New List(Of String)

        Debug.Print("Tick: " + elapsedTime.ToString)

        For i = 1 To _numberTimers
            If _timers(i).TimerActive Then
                If _timers(i).BypassThisTurn Then
                    ' don't trigger timer during the turn it was first enabled
                    _timers(i).BypassThisTurn = False
                Else
                    _timers(i).TimerTicks = _timers(i).TimerTicks + elapsedTime

                    If _timers(i).TimerTicks >= _timers(i).TimerInterval Then
                        _timers(i).TimerTicks = 0
                        TimerScripts.Add(_timers(i).TimerAction)
                    End If
                End If
            End If
        Next i

        If TimerScripts.Count > 0 Then
            Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf RunTimersInNewThread))

            ChangeState(State.Working)
            runnerThread.Start(TimerScripts)
            WaitForStateChange(State.Working)
        End If

        RaiseNextTimerTickRequest()
    End Sub

    Private Sub RunTimersInNewThread(scripts As Object)
        Dim scriptList As List(Of String) = DirectCast(scripts, List(Of String))

        For Each script As String In scriptList
            Try
                ExecuteScript(script, _nullContext)
            Catch ex As Exception
                LogException(ex)
            End Try
        Next

        ChangeState(State.Ready)
    End Sub

    Private Sub RaiseNextTimerTickRequest()
        Dim anyTimerActive As Boolean = False
        Dim nextTrigger As Integer = 60

        For i As Integer = 1 To _numberTimers
            If _timers(i).TimerActive Then
                anyTimerActive = True

                Dim thisNextTrigger As Integer = _timers(i).TimerInterval - _timers(i).TimerTicks
                If thisNextTrigger < nextTrigger Then
                    nextTrigger = thisNextTrigger
                End If
            End If
        Next i

        If Not anyTimerActive Then nextTrigger = 0
        If _gameFinished Then nextTrigger = 0

        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger.ToString)

        RaiseEvent RequestNextTimerTick(nextTrigger)
    End Sub

    Private Sub ChangeState(newState As State)
        Dim acceptCommands As Boolean = (newState = State.Ready)
        ChangeState(newState, acceptCommands)
    End Sub

    Private Sub ChangeState(newState As State, acceptCommands As Boolean)
        _readyForCommand = acceptCommands
        SyncLock _stateLock
            _state = newState
            System.Threading.Monitor.PulseAll(_stateLock)
        End SyncLock
    End Sub

    Public Sub FinishWait() Implements IASL.FinishWait
        If (_state <> State.Waiting) Then Exit Sub
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf FinishWaitInNewThread))
        ChangeState(State.Working)
        runnerThread.Start()
        WaitForStateChange(State.Working)
    End Sub

    Private Sub FinishWaitInNewThread()
        SyncLock _waitLock
            System.Threading.Monitor.PulseAll(_waitLock)
        End SyncLock
    End Sub

    Public Sub FinishPause() Implements IASL.FinishPause
        FinishWait()
    End Sub

    Private m_menuResponse As String

    Private Function ShowMenu(menuData As MenuData) As String
        _player.ShowMenu(menuData)
        ChangeState(State.Waiting)

        SyncLock _waitLock
            System.Threading.Monitor.Wait(_waitLock)
        End SyncLock

        Return m_menuResponse
    End Function

    Public Sub SetMenuResponse(response As String) Implements IASL.SetMenuResponse
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf SetMenuResponseInNewThread))
        ChangeState(State.Working)
        runnerThread.Start(response)
        WaitForStateChange(State.Working)
    End Sub

    Private Sub SetMenuResponseInNewThread(response As Object)
        m_menuResponse = DirectCast(response, String)

        SyncLock _waitLock
            System.Threading.Monitor.PulseAll(_waitLock)
        End SyncLock
    End Sub

    Private Sub LogException(ex As Exception)
        RaiseEvent LogError(ex.Message + Environment.NewLine + ex.StackTrace)
    End Sub

    Public Function GetExternalScripts() As IEnumerable(Of String) Implements IASL.GetExternalScripts
        Return Nothing
    End Function

    Public Function GetExternalStylesheets() As IEnumerable(Of String) Implements IASL.GetExternalStylesheets
        Return Nothing
    End Function

    Public Event RequestNextTimerTick(nextTick As Integer) Implements IASLTimer.RequestNextTimerTick

    Public ReadOnly Property OriginalFilename As String Implements IASL.OriginalFilename
        Get
            Return _originalFilename
        End Get
    End Property

    Private Function GetOriginalFilenameForQSG() As String
        If _originalFilename IsNot Nothing Then Return _originalFilename
        Return _gameFileName
    End Function

    Public Delegate Function UnzipFunctionDelegate(filename As String, <Runtime.InteropServices.Out()> ByRef tempDir As String) As String
    Private m_unzipFunction As UnzipFunctionDelegate

    Public Sub SetUnzipFunction(unzipFunction As UnzipFunctionDelegate)
        m_unzipFunction = unzipFunction
    End Sub

    Private Function GetUnzippedFile(filename As String) As String
        Dim tempDir As String = Nothing
        Dim result As String = m_unzipFunction.Invoke(filename, tempDir)
        _tempFolder = tempDir
        Return result
    End Function

    Public Property TempFolder As String Implements IASL.TempFolder
        Get
            Return _tempFolder
        End Get
        Set
            _tempFolder = Value
        End Set
    End Property

    Public ReadOnly Property ASLVersion As Integer
        Get
            Return _gameAslVersion
        End Get
    End Property

    Public Function GetResource(file As String) As System.IO.Stream Implements IASL.GetResource
        If file = "_game.cas" Then
            Return New IO.MemoryStream(GetResourcelessCAS())
        End If
        Dim path As String = GetResourcePath(file)
        If Not System.IO.File.Exists(path) Then Return Nothing
        Return New IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read)
    End Function

    Private m_gameId As String

    Public ReadOnly Property GameID() As String Implements IASL.GameID
        Get
            If String.IsNullOrEmpty(_gameFileName) Then Return Nothing
            If Config.ReadGameFileFromAzureBlob Then
                Return m_gameId
            End If
            Return TextAdventures.Utility.Utility.FileMD5Hash(_gameFileName)
        End Get
    End Property

    Public Iterator Function GetResources() As IEnumerable(Of String)
        For i As Integer = 1 To _numResources
            Yield _resources(i).ResourceName
        Next
        If _numResources > 0 Then
            Yield "_game.cas"
        End If
    End Function

    Private Function GetResourcelessCAS() As Byte()
        Dim FileData As String = System.IO.File.ReadAllText(_resourceFile, System.Text.Encoding.GetEncoding(1252))
        Return System.Text.Encoding.GetEncoding(1252).GetBytes(Left(FileData, _startCatPos - 1))
    End Function

End Class