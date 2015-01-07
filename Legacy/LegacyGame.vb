Option Strict On
Option Explicit On

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

    Private Structure DefineBlock
        Dim StartLine As Integer
        Dim EndLine As Integer
    End Structure

    Friend Structure ThreadData
        Dim CallingObjectID As Integer
        Dim NumParameters As Integer
        Dim Parameters() As String
        Dim FunctionReturnValue As String
        Dim AllowRealNamesInCommand As Boolean
        Dim DontProcessCommand As Boolean
        Dim CancelExec As Boolean
        Dim StackCounter As Integer
    End Structure

    Private OpenErrorReport As String
    Private CASkeywords(255) As String 'Tokenised CAS keywords
    Private Lines() As String 'Stores the lines of the ASL script/definitions
    Private DefineBlocks() As DefineBlock 'Stores the start and end lines of each 'define' section
    Private NumberSections As Integer 'Number of define sections
    Private GameName As String 'The name of the game
    Friend NullThread As New ThreadData

    Public Const LOGTYPE_MISC As Integer = 0
    Public Const LOGTYPE_FATALERROR As Integer = 1
    Public Const LOGTYPE_WARNINGERROR As Integer = 2
    Public Const LOGTYPE_INIT As Integer = 3
    Public Const LOGTYPE_LIBRARYWARNINGERROR As Integer = 4
    Public Const LOGTYPE_WARNING As Integer = 5
    Public Const LOGTYPE_USERERROR As Integer = 6
    Public Const LOGTYPE_INTERNALERROR As Integer = 7

    Private m_CurrentDirectory As String

    Private m_oDefineBlockParams As Dictionary(Of String, Dictionary(Of String, String))

    Friend Enum eDirection
        dirNone = -1
        dirOut = 0
        dirNorth = 1
        dirSouth = 2
        dirEast = 3
        dirWest = 4
        dirNorthWest = 5
        dirNorthEast = 6
        dirSouthWest = 7
        dirSouthEast = 8
        dirUp = 9
        dirDown = 10
    End Enum

    Private Structure ItemType
        Dim itemname As String
        Dim gotitem As Boolean
    End Structure

    Private Structure Collectable
        Dim collectablename As String
        Dim collectabletype As String
        Dim collectablenumber As Double
        Dim collectabledisplay As String
        Dim collectablemax As Double
        Dim DisplayWhenZero As Boolean
    End Structure

    Friend Structure PropertyType
        Dim PropertyName As String
        Dim PropertyValue As String
    End Structure

    Friend Structure ActionType
        Dim ActionName As String
        Dim Script As String
    End Structure

    Friend Structure UseDataType
        Dim UseObject As String
        Dim UseType As Integer
        Dim UseScript As String
    End Structure

    Friend Structure GiveDataType
        Dim GiveObject As String
        Dim GiveType As Integer
        Dim GiveScript As String
    End Structure

    Private Structure PropertiesActions
        Dim Properties As String
        Dim NumberActions As Integer
        Dim Actions() As ActionType
        Dim NumberTypesIncluded As Integer
        Dim TypesIncluded() As String
    End Structure

    Private Structure ObjectListType
        Dim ObjectName As String
        Dim ObjectType As Integer
        Dim DisplayObjectType As String
    End Structure

    Private Structure VariableType
        Dim VariableName As String
        Dim VariableContents() As String
        Dim VariableUBound As Integer
        Dim DisplayString As String
        Dim OnChangeScript As String
        Dim NoZeroDisplay As Boolean
        Dim Scope As Integer
    End Structure

    Private Structure SynonymType
        Dim OriginalWord As String
        Dim ConvertTo As String
    End Structure

    Private Structure TimerType
        Dim TimerName As String
        Dim TimerInterval As Integer
        Dim TimerActive As Boolean
        Dim TimerAction As String
        Dim TimerTicks As Integer
        Dim BypassThisTurn As Boolean
    End Structure

    Friend Structure UserDefinedCommandType
        Dim CommandText As String
        Dim CommandScript As String
    End Structure

    Friend Structure TextAction
        Dim Data As String
        Dim Type As Integer
    End Structure

    Private Structure InterfaceStringType
        Dim StringName As String
        Dim StringText As String
    End Structure

    Public Const TA_TEXT As Integer = 1
    Public Const TA_SCRIPT As Integer = 2
    Public Const TA_NOTHING As Integer = 0
    Public Const TA_DEFAULT As Integer = 3

    Friend Structure ScriptText
        Dim Text As String
        Dim Script As String
    End Structure

    Friend Structure PlaceType
        Dim PlaceName As String
        Dim Prefix As String
        Dim PlaceAlias As String
        Dim Script As String
    End Structure

    Friend Structure RoomType
        Dim RoomName As String
        Dim RoomAlias As String
        Dim Commands() As UserDefinedCommandType
        Dim NumberCommands As Integer
        Dim Description As TextAction
        Dim out As ScriptText
        Dim East As TextAction
        Dim West As TextAction
        Dim North As TextAction
        Dim South As TextAction
        Dim NorthEast As TextAction
        Dim NorthWest As TextAction
        Dim SouthEast As TextAction
        Dim SouthWest As TextAction
        Dim Up As TextAction
        Dim Down As TextAction
        Dim InDescription As String
        Dim Look As String
        Dim Places() As PlaceType
        Dim NumberPlaces As Integer
        Dim Prefix As String
        Dim Script As String
        Dim use() As ScriptText
        Dim NumberUse As Integer
        Dim ObjID As Integer
        Dim BeforeTurnScript As String
        Dim AfterTurnScript As String
        Dim Exits As RoomExits
    End Structure

    Friend Structure ObjectType
        Dim ObjectName As String
        Dim ObjectAlias As String
        Dim Detail As String
        Dim ContainerRoom As String
        Dim Exists As Boolean
        Dim IsGlobal As Boolean
        Dim Prefix As String
        Dim Suffix As String
        Dim Gender As String
        Dim Article As String
        Dim DefinitionSectionStart As Integer
        Dim DefinitionSectionEnd As Integer
        Dim Visible As Boolean
        Dim UnallocatedPlayer As Boolean
        Dim IsNetPlayer As Boolean
        Dim GainScript As String
        Dim LoseScript As String
        Dim NumberProperties As Integer
        Dim Properties() As PropertyType
        Dim Speak As TextAction
        Dim take As TextAction
        Dim IsRoom As Boolean
        Dim IsExit As Boolean
        Dim CorresRoom As String
        Dim CorresRoomID As Integer
        Dim Loaded As Boolean
        Dim NumberActions As Integer
        Dim Actions() As ActionType
        Dim NumberUseData As Integer
        Dim UseData() As UseDataType
        Dim UseAnything As String
        Dim UseOnAnything As String
        Dim use As String
        Dim NumberGiveData As Integer
        Dim GiveData() As GiveDataType
        Dim GiveAnything As String
        Dim GiveToAnything As String
        Dim DisplayType As String
        Dim NumberTypesIncluded As Integer
        Dim TypesIncluded() As String
        Dim NumberAltNames As Integer
        Dim AltNames() As String
        Dim AddScript As TextAction
        Dim RemoveScript As TextAction
        Dim OpenScript As TextAction
        Dim CloseScript As TextAction
    End Structure

    Private Structure ChangeType
        Dim AppliesTo As String
        Dim Change As String
    End Structure

    Private Structure GameChangeDataType
        Dim NumberChanges As Integer
        Dim ChangeData() As ChangeType
    End Structure

    Private Structure ResourceType
        Dim ResourceName As String
        Dim ResourceStart As Integer
        Dim ResourceLength As Integer
        Dim Extracted As Boolean
    End Structure

    Private Structure MenuItemType
        Dim Text As String
        Dim MenuCommand As String
    End Structure

    Private Structure MenuType
        Dim NumberItems As Integer
        Dim Items() As MenuItemType
        Dim MenuName As String
    End Structure

    Private Structure ExpressionResult
        Dim Result As String
        Dim success As Integer
        Dim Message As String
    End Structure

    Private m_oChangeLogRooms As ChangeLog
    Private m_oChangeLogObjects As ChangeLog
    Private m_oDefaultProperties As PropertiesActions
    Private m_oDefaultRoomProperties As PropertiesActions

    Friend Rooms() As RoomType
    Friend NumberRooms As Integer
    Private NumericVariable() As VariableType
    Private NumberNumericVariables As Integer
    Private StringVariable() As VariableType
    Private NumberStringVariables As Integer
    Private Synonyms() As SynonymType
    Private NumberSynonyms As Integer
    Private Items() As ItemType
    Private Chars() As ObjectType
    Friend Objs() As ObjectType
    Private NumberChars As Integer
    Friend NumberObjs As Integer
    Private NumberItems As Integer
    Friend CurrentRoom As String
    Private Collectables() As Collectable
    Private NumCollectables As Integer
    Private GamePath As String
    Private GameFileName As String
    Private optOutputFile As Boolean
    Private OutputFileName As String
    Private optDefaultFontName As String
    Private optDefaultFontSize As Double
    Private optEnableSpeech As Boolean
    Private optSpeakEverything As Boolean
    Private SaveGameFile As String
    Private OutputFileHandle As Integer
    Private ObjectList() As ObjectListType
    Private NumInObjList As Integer
    Private DefaultFontName As String
    Private DefaultFontSize As Double
    Private AutoIntro As Boolean
    Private CommandOverrideModeOn As Boolean
    Private CommandOverrideVariable As String
    Private AfterTurnScript As String
    Private BeforeTurnScript As String
    Private OutPutOn As Boolean
    Private GameASLVersion As Integer
    Private ChoiceNumber As Integer
    Private GameLoadMethod As String
    Private Timers() As TimerType
    Private NumberTimers As Integer
    Private CurFont As String
    Private CurFontSize As Double
    Private NumberScriptData As Integer
    Private DisplayStatus As Boolean
    Private DisplayStringIDs() As Integer
    Private NumDisplayStrings As Integer
    Private DisplayNumericIDs() As Integer
    Private NumDisplayNumerics As Integer
    Private GameFullyLoaded As Boolean
    Private GameChangeData As GameChangeDataType
    Private MidiIsLooping As Boolean
    Private CurForeground As Integer
    Private CurBackground As Integer
    Private LastIt, LastItMode As Integer
    Private ThisTurnIt, ThisTurnItMode As Integer
    Private BadCmdBefore, BadCmdAfter As String
    Private CurCol As Integer
    Private DefaultObject As ObjectType
    Private DefaultRoom As RoomType
    Private NumResources As Integer
    Private Resources() As ResourceType
    Private ResourceFile As String
    Private ResourceOffset As Integer
    Private StartCatPos As Integer
    Private goptAbbreviations As Boolean
    Private m_loadedFromQSG As Boolean
    Private BeforeSaveScript As String
    Private OnLoadScript As String
    Private NumSkipCheckFiles As Integer
    Private SkipCheckFile() As String
    Private m_compassExits As New List(Of ListData)
    Private m_gotoExits As New List(Of ListData)
    Private m_textFormatter As New TextFormatter
    Private m_log As New List(Of String)
    Private m_fileData As String
    Private m_commandLock As Object = New Object
    Private m_stateLock As Object = New Object
    Private m_state As State = State.Ready
    Private m_waitLock As Object = New Object
    Private m_readyForCommand As Boolean = True
    Private m_gameLoading As Boolean
    Private m_random As New Random()
    Private m_tempFolder As String

    Private Const NUMBER_PLAYER_ERROR_MESSAGES As Integer = 38
    Private PlayerErrorMessageString(NUMBER_PLAYER_ERROR_MESSAGES) As String

    Friend Const ERROR_BADCOMMAND As Integer = 1
    Friend Const ERROR_BADGO As Integer = 2
    Friend Const ERROR_BADGIVE As Integer = 3
    Friend Const ERROR_BADCHARACTER As Integer = 4
    Friend Const ERROR_NOITEM As Integer = 5
    Friend Const ERROR_ITEMUNWANTED As Integer = 6
    Friend Const ERROR_BADLOOK As Integer = 7
    Friend Const ERROR_BADTHING As Integer = 8
    Friend Const ERROR_DEFAULTLOOK As Integer = 9
    Friend Const ERROR_DEFAULTSPEAK As Integer = 10
    Friend Const ERROR_BADITEM As Integer = 11
    Friend Const ERROR_DEFAULTTAKE As Integer = 12
    Friend Const ERROR_BADUSE As Integer = 13
    Friend Const ERROR_DEFAULTUSE As Integer = 14
    Friend Const ERROR_DEFAULTOUT As Integer = 15
    Friend Const ERROR_BADPLACE As Integer = 16
    Friend Const ERROR_BADEXAMINE As Integer = 17
    Friend Const ERROR_DEFAULTEXAMINE As Integer = 18
    Friend Const ERROR_BADTAKE As Integer = 19
    Friend Const ERROR_CANTDROP As Integer = 20
    Friend Const ERROR_DEFAULTDROP As Integer = 21
    Friend Const ERROR_BADDROP As Integer = 22
    Friend Const ERROR_BADPRONOUN As Integer = 23
    Friend Const ERROR_ALREADYOPEN As Integer = 24
    Friend Const ERROR_ALREADYCLOSED As Integer = 25
    Friend Const ERROR_CANTOPEN As Integer = 26
    Friend Const ERROR_CANTCLOSE As Integer = 27
    Friend Const ERROR_DEFAULTOPEN As Integer = 28
    Friend Const ERROR_DEFAULTCLOSE As Integer = 29
    Friend Const ERROR_BADPUT As Integer = 30
    Friend Const ERROR_CANTPUT As Integer = 31
    Friend Const ERROR_DEFAULTPUT As Integer = 32
    Friend Const ERROR_CANTREMOVE As Integer = 33
    Friend Const ERROR_ALREADYPUT As Integer = 34
    Friend Const ERROR_DEFAULTREMOVE As Integer = 35
    Friend Const ERROR_LOCKED As Integer = 36
    Friend Const ERROR_DEFAULTWAIT As Integer = 37
    Friend Const ERROR_ALREADYTAKEN As Integer = 38

    Private Const IT_INANIMATE As Integer = 1
    Private Const IT_MALE As Integer = 2
    Private Const IT_FEMALE As Integer = 3

    Private Const SET_ERROR As Integer = 0
    Private Const SET_FOUND As Integer = 1
    Private Const SET_UNFOUND As Integer = 2

    Private QuestVersion As String
    Private QuestName As String
    Private Const QSGVersion As String = "QUEST300"
    Private Const ASLVersionNumber As String = "410"
    Private Const RecognisedVersions As String = "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/"
    Private Const QUEST_CHARACTER As Integer = 1
    Private Const QUEST_OBJECT As Integer = 2
    Private Const QUEST_ROOM As Integer = 3
    Private Const CONVERT_STRINGS As Integer = 1
    Private Const CONVERT_FUNCTIONS As Integer = 2
    Private Const CONVERT_NUMERIC As Integer = 3
    Private Const CONVERT_COLLECTABLES As Integer = 4
    Private Const USE_ON_SOMETHING As Integer = 1
    Private Const USE_SOMETHING_ON As Integer = 2
    Private Const GIVE_TO_SOMETHING As Integer = 1
    Private Const GIVE_SOMETHING_TO As Integer = 2
    Private Const VARTYPE_STRING As Integer = 1
    Private Const VARTYPE_NUMERIC As Integer = 2
    Private Const STOPGAME_WIN As Integer = 1
    Private Const STOPGAME_LOSE As Integer = 2
    Private Const STOPGAME_NULL As Integer = 0
    Private Const ANIMATION_NONE As Integer = 0
    Private Const ANIMATION_NORMAL As Integer = 1
    Private Const ANIMATION_PERSIST As Integer = 2
    Private Const CONTAINER_NONE As Integer = 0
    Private Const CONTAINER_NORMAL As Integer = 1
    Private Const CONTAINER_SURFACE As Integer = 2
    Private Const EXPRESSION_OK As Integer = 1
    Private Const EXPRESSION_FAIL As Integer = 2

    Private m_listVerbs As New Dictionary(Of ListType, List(Of String))
    Private m_filename As String
    Private m_originalFilename As String
    Private m_data As InitGameData
    Private m_player As IPlayer
    Private m_gameFinished As Boolean
    Private m_gameIsRestoring As Boolean
    Private m_useStaticFrameForPictures As Boolean

    Public Sub New(filename As String, originalFilename As String)
        QuestVersion = My.Application.Info.Version.ToString()
        m_tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath, "Quest", Guid.NewGuid().ToString())
        InitialiseQuest()
        GetQuestSettings()
        GameLoadMethod = "normal"
        m_filename = filename
        m_originalFilename = originalFilename

        ' Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
        ' for compatibility with games which have non-fatal errors in them.
        NumSkipCheckFiles = 3
        ReDim SkipCheckFile(3)
        SkipCheckFile(1) = "bargain.cas"
        SkipCheckFile(2) = "easymoney.asl"
        SkipCheckFile(3) = "musicvf1.cas"
    End Sub

    Public Class InitGameData
        Public Data As Byte()
        Public SourceFile As String
    End Class

    Public Sub New(data As InitGameData)
        Me.New(Nothing, Nothing)
        m_data = data
    End Sub

    Private Function StripCodes(InputString As String) As String
        Dim FCodeDat As String
        Dim FCodePos, FCodeLen As Integer

        Do
            FCodePos = InStr(InputString, "|")
            If FCodePos <> 0 Then
                FCodeDat = Mid(InputString, FCodePos + 1, 3)

                If Left(FCodeDat, 1) = "b" Then
                    FCodeLen = 1
                ElseIf Left(FCodeDat, 2) = "xb" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 1) = "u" Then
                    FCodeLen = 1
                ElseIf Left(FCodeDat, 2) = "xu" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 1) = "i" Then
                    FCodeLen = 1
                ElseIf Left(FCodeDat, 2) = "xi" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "cr" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "cb" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "cl" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "cy" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "cg" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 1) = "n" Then
                    FCodeLen = 1
                ElseIf Left(FCodeDat, 2) = "xn" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 1) = "s" Then
                    FCodeLen = 3
                ElseIf Left(FCodeDat, 2) = "jc" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "jl" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 2) = "jr" Then
                    FCodeLen = 2
                ElseIf Left(FCodeDat, 1) = "w" Then
                    FCodeLen = 1
                ElseIf Left(FCodeDat, 1) = "c" Then
                    FCodeLen = 1
                End If

                If FCodeLen = 0 Then
                    ' unknown code
                    FCodeLen = 1
                End If

                InputString = Left(InputString, FCodePos - 1) & Mid(InputString, FCodePos + FCodeLen + 1)
            End If

        Loop Until FCodePos = 0

        StripCodes = InputString

    End Function

    Private Function CheckSections() As Boolean
        Dim Defines, i, Braces As Integer
        Dim CheckLine As String = ""
        Dim BracePos As Integer
        Dim CurPos As Integer
        Dim ThisSection As String = ""
        Dim HasErrors As Boolean
        Dim bSkipBlock As Boolean
        OpenErrorReport = ""
        HasErrors = False
        Defines = 0
        Braces = 0

        For i = 1 To UBound(Lines)
            If Not BeginsWith(Lines(i), "#!qdk-note: ") Then
                If BeginsWith(Lines(i), "define ") Then
                    ThisSection = Lines(i)
                    Braces = 0
                    Defines = Defines + 1
                    bSkipBlock = BeginsWith(Lines(i), "define text") Or BeginsWith(Lines(i), "define synonyms")
                ElseIf Trim(Lines(i)) = "end define" Then
                    Defines = Defines - 1

                    If Defines < 0 Then
                        LogASLError("Extra 'end define' after block '" & ThisSection & "'", LOGTYPE_FATALERROR)
                        OpenErrorReport = OpenErrorReport & "Extra 'end define' after block '" & ThisSection & "'" & vbCrLf
                        HasErrors = True
                        Defines = 0
                    End If

                    If Braces > 0 Then
                        LogASLError("Missing } in block '" & ThisSection & "'", LOGTYPE_FATALERROR)
                        OpenErrorReport = OpenErrorReport & "Missing } in block '" & ThisSection & "'" & vbCrLf
                        HasErrors = True
                    ElseIf Braces < 0 Then
                        LogASLError("Too many } in block '" & ThisSection & "'", LOGTYPE_FATALERROR)
                        OpenErrorReport = OpenErrorReport & "Too many } in block '" & ThisSection & "'" & vbCrLf
                        HasErrors = True
                    End If
                End If

                If Left(Lines(i), 1) <> "'" And Not bSkipBlock Then
                    CheckLine = ObliterateParameters(Lines(i))
                    If BeginsWith(CheckLine, "'<ERROR;") Then
                        ' ObliterateParameters denotes a mismatched $, ( etc.
                        ' by prefixing line with '<ERROR;*; where * is the mismatched
                        ' character
                        LogASLError("Expected closing " & Mid(CheckLine, 9, 1) & " character in '" & ReportErrorLine(Lines(i)) & "'", LOGTYPE_FATALERROR)
                        OpenErrorReport = OpenErrorReport & "Expected closing " & Mid(CheckLine, 9, 1) & " character in '" & ReportErrorLine(Lines(i)) & "'." & vbCrLf
                        CheckSections = False
                        Exit Function
                    End If
                End If

                If Left(Trim(CheckLine), 1) <> "'" Then
                    ' Now check {
                    CurPos = 1
                    Do
                        BracePos = InStr(CurPos, CheckLine, "{")
                        If BracePos <> 0 Then
                            CurPos = BracePos + 1
                            Braces = Braces + 1
                        End If
                    Loop Until BracePos = 0 Or CurPos > Len(CheckLine)

                    ' Now check }
                    CurPos = 1
                    Do
                        BracePos = InStr(CurPos, CheckLine, "}")
                        If BracePos <> 0 Then
                            CurPos = BracePos + 1
                            Braces = Braces - 1
                        End If
                    Loop Until BracePos = 0 Or CurPos > Len(CheckLine)
                End If
            End If
        Next i

        CheckSections = True

        If Defines > 0 Then
            LogASLError("Missing 'end define'", LOGTYPE_FATALERROR)
            OpenErrorReport = OpenErrorReport & "Missing 'end define'." & vbCrLf
            HasErrors = True
        End If

        If HasErrors Then
            CheckSections = False
        Else
            CheckSections = True
        End If

    End Function

    Private Function ConvertFriendlyIfs() As Boolean
        ' Converts
        '   if (%something% < 3) then ...
        ' to
        '   if is <%something%;lt;3> then ...
        ' and also repeat until ...

        ' Returns False if successful

        Dim ConvPos, i, SymbPos As Integer
        Dim Symbol As String
        Dim EndParamPos, j As Integer
        Dim ParamData As String
        Dim StartParamPos As Integer
        Dim FirstData, SecondData As String
        Dim ObscureLine, NewParam, VarObscureLine As String
        Dim BracketCount As Integer

        For i = 1 To UBound(Lines)
            ObscureLine = ObliterateParameters(Lines(i))
            ConvPos = InStr(ObscureLine, "if (")
            If ConvPos = 0 Then
                ConvPos = InStr(ObscureLine, "until (")
            End If
            If ConvPos = 0 Then
                ConvPos = InStr(ObscureLine, "while (")
            End If
            If ConvPos = 0 Then
                ConvPos = InStr(ObscureLine, "not (")
            End If
            If ConvPos = 0 Then
                ConvPos = InStr(ObscureLine, "and (")
            End If
            If ConvPos = 0 Then
                ConvPos = InStr(ObscureLine, "or (")
            End If


            If ConvPos <> 0 Then
                VarObscureLine = ObliterateVariableNames(Lines(i))
                If BeginsWith(VarObscureLine, "'<ERROR;") Then
                    ' ObliterateVariableNames denotes a mismatched #, % or $
                    ' by prefixing line with '<ERROR;*; where * is the mismatched
                    ' character
                    LogASLError("Expected closing " & Mid(VarObscureLine, 9, 1) & " character in '" & ReportErrorLine(Lines(i)) & "'", LOGTYPE_FATALERROR)
                    ConvertFriendlyIfs = True
                    Exit Function
                End If
                StartParamPos = InStr(ConvPos, Lines(i), "(")

                EndParamPos = 0
                BracketCount = 1
                For j = StartParamPos + 1 To Len(Lines(i))
                    If Mid(Lines(i), j, 1) = "(" Then
                        BracketCount = BracketCount + 1
                    ElseIf Mid(Lines(i), j, 1) = ")" Then
                        BracketCount = BracketCount - 1
                    End If
                    If BracketCount = 0 Then
                        EndParamPos = j
                        Exit For
                    End If
                Next j

                'EndParamPos = InStr(ConvPos, VarObscureLine, ")")
                If EndParamPos = 0 Then
                    LogASLError("Expected ) in '" & ReportErrorLine(Lines(i)) & "'", LOGTYPE_FATALERROR)
                    ConvertFriendlyIfs = True
                    Exit Function
                End If

                ParamData = Mid(Lines(i), StartParamPos + 1, (EndParamPos - StartParamPos) - 1)

                SymbPos = InStr(ParamData, "!=")
                If SymbPos = 0 Then
                    SymbPos = InStr(ParamData, "<>")
                    If SymbPos = 0 Then
                        SymbPos = InStr(ParamData, "<=")
                        If SymbPos = 0 Then
                            SymbPos = InStr(ParamData, ">=")
                            If SymbPos = 0 Then
                                SymbPos = InStr(ParamData, "<")
                                If SymbPos = 0 Then
                                    SymbPos = InStr(ParamData, ">")
                                    If SymbPos = 0 Then
                                        SymbPos = InStr(ParamData, "=")
                                        If SymbPos = 0 Then
                                            LogASLError("Unrecognised 'if' condition in '" & ReportErrorLine(Lines(i)) & "'", LOGTYPE_FATALERROR)
                                            ConvertFriendlyIfs = True
                                            Exit Function
                                        Else
                                            Symbol = "="
                                        End If
                                    Else
                                        Symbol = ">"
                                    End If
                                Else
                                    Symbol = "<"
                                End If
                            Else
                                Symbol = ">="
                            End If
                        Else
                            Symbol = "<="
                        End If
                    Else
                        Symbol = "<>"
                    End If
                Else
                    Symbol = "<>"
                End If


                FirstData = Trim(Left(ParamData, SymbPos - 1))
                SecondData = Trim(Mid(ParamData, SymbPos + Len(Symbol)))

                If Symbol = "=" Then
                    NewParam = "is <" & FirstData & ";" & SecondData & ">"
                Else
                    NewParam = "is <" & FirstData & ";"
                    If Symbol = "<" Then
                        NewParam = NewParam & "lt"
                    ElseIf Symbol = ">" Then
                        NewParam = NewParam & "gt"
                    ElseIf Symbol = ">=" Then
                        NewParam = NewParam & "gt="
                    ElseIf Symbol = "<=" Then
                        NewParam = NewParam & "lt="
                    ElseIf Symbol = "<>" Then
                        NewParam = NewParam & "!="
                    End If
                    NewParam = NewParam & ";" & SecondData & ">"
                End If

                Lines(i) = Left(Lines(i), StartParamPos - 1) & NewParam & Mid(Lines(i), EndParamPos + 1)

                ' Repeat processing this line, in case there are
                ' further changes to be made.
                i = i - 1
            End If
        Next i

        ConvertFriendlyIfs = False
    End Function

    Private Sub ConvertMultiLineSections()

        Dim StartLine, BraceCount As Integer
        Dim ThisLine, LineToAdd As String
        Dim LastBrace As Integer
        Dim k, i, j, M As Integer
        Dim RestOfLine, ProcName As String
        Dim EndLineNum As Integer
        Dim AfterLastBrace, Z As String
        Dim StartOfOrig As String

        Dim TestLine As String
        Dim TestBraceCount As Integer
        Dim OBP, CBP As Integer
        Dim CurProc As Integer

        i = 1
        Do
            Z = Lines(DefineBlocks(i).StartLine)
            If ((Not BeginsWith(Z, "define text ")) And (Not BeginsWith(Z, "define menu ")) And Z <> "define synonyms") Then
                For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                    If InStr(Lines(j), "{") > 0 Then

                        AfterLastBrace = ""
                        ThisLine = Trim(Lines(j))

                        ProcName = "<!intproc" & Trim(Str(CurProc)) & ">"

                        ' see if this brace's corresponding closing
                        ' brace is on same line:

                        TestLine = Mid(Lines(j), InStr(Lines(j), "{") + 1)
                        TestBraceCount = 1
                        Do
                            OBP = InStr(TestLine, "{")
                            CBP = InStr(TestLine, "}")
                            If OBP = 0 Then OBP = Len(TestLine) + 1
                            If CBP = 0 Then CBP = Len(TestLine) + 1
                            If OBP < CBP Then
                                TestBraceCount = TestBraceCount + 1
                                TestLine = Mid(TestLine, OBP + 1)
                            ElseIf CBP < OBP Then
                                TestBraceCount = TestBraceCount - 1
                                TestLine = Mid(TestLine, CBP + 1)
                            End If
                        Loop Until OBP = CBP Or TestBraceCount = 0

                        If TestBraceCount <> 0 Then
                            AddLine("define procedure " & ProcName)
                            StartLine = UBound(Lines)
                            RestOfLine = Trim(Right(ThisLine, Len(ThisLine) - InStr(ThisLine, "{")))
                            BraceCount = 1
                            If RestOfLine <> "" Then AddLine(RestOfLine)

                            For M = 1 To Len(RestOfLine)
                                If Mid(RestOfLine, M, 1) = "{" Then
                                    BraceCount = BraceCount + 1
                                ElseIf Mid(RestOfLine, M, 1) = "}" Then
                                    BraceCount = BraceCount - 1
                                End If
                            Next M

                            If BraceCount <> 0 Then
                                k = j + 1
                                Do
                                    For M = 1 To Len(Lines(k))
                                        If Mid(Lines(k), M, 1) = "{" Then
                                            BraceCount = BraceCount + 1
                                        ElseIf Mid(Lines(k), M, 1) = "}" Then
                                            BraceCount = BraceCount - 1
                                        End If

                                        If BraceCount = 0 Then
                                            LastBrace = M
                                            Exit For
                                        End If
                                    Next M

                                    If BraceCount <> 0 Then
                                        'put Lines(k) into another variable, as
                                        'AddLine ReDims Lines, which it can't do if
                                        'passed Lines(x) as a parameter.
                                        LineToAdd = Lines(k)
                                        AddLine(LineToAdd)
                                    Else
                                        AddLine(Left(Lines(k), LastBrace - 1))
                                        AfterLastBrace = Trim(Mid(Lines(k), LastBrace + 1))
                                    End If

                                    'Clear original line
                                    Lines(k) = ""
                                    k = k + 1
                                Loop While BraceCount <> 0
                            End If

                            AddLine("end define")
                            EndLineNum = UBound(Lines)

                            NumberSections = NumberSections + 1
                            ReDim Preserve DefineBlocks(NumberSections)
                            DefineBlocks(NumberSections).StartLine = StartLine
                            DefineBlocks(NumberSections).EndLine = EndLineNum

                            'Change original line where the { section
                            'started to call the new procedure.
                            StartOfOrig = Trim(Left(ThisLine, InStr(ThisLine, "{") - 1))
                            Lines(j) = StartOfOrig & " do " & ProcName & " " & AfterLastBrace
                            CurProc = CurProc + 1

                            ' Process this line again in case there was stuff after the last brace that included
                            ' more braces. e.g. } else {
                            j = j - 1
                        End If
                    End If
                Next j
            End If
            i = i + 1
        Loop Until i > NumberSections

        ' Join next-line "else"s to corresponding "if"s

        For i = 1 To NumberSections
            Z = Lines(DefineBlocks(i).StartLine)
            If ((Not BeginsWith(Z, "define text ")) And (Not BeginsWith(Z, "define menu ")) And Z <> "define synonyms") Then
                For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                    If BeginsWith(Lines(j), "else ") Then

                        'Go upwards to find "if" statement that this
                        'belongs to

                        For k = j To DefineBlocks(i).StartLine + 1 Step -1
                            If BeginsWith(Lines(k), "if ") Or InStr(ObliterateParameters(Lines(k)), " if ") <> 0 Then
                                Lines(k) = Lines(k) & " " & Trim(Lines(j))
                                Lines(j) = ""
                                k = DefineBlocks(i).StartLine
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
        Dim iCurBegin, iCurEnd As Integer
        Dim bHasErrors As Boolean
        Dim iCurPos As Integer
        Dim iNumParamStart, iNumParamEnd As Integer
        Dim bFinLoop, bInText As Boolean
        Dim i As Integer

        bHasErrors = False
        bInText = False

        ' Checks for incorrect number of < and > :
        For i = 1 To UBound(Lines)
            iNumParamStart = 0
            iNumParamEnd = 0

            If BeginsWith(Lines(i), "define text ") Then
                bInText = True
            End If
            If bInText And Trim(LCase(Lines(i))) = "end define" Then
                bInText = False
            End If

            If Not bInText Then
                'Find number of <'s:
                iCurPos = 1
                bFinLoop = False
                Do
                    If InStr(iCurPos, Lines(i), "<") <> 0 Then
                        iNumParamStart = iNumParamStart + 1
                        iCurPos = InStr(iCurPos, Lines(i), "<") + 1
                    Else
                        bFinLoop = True
                    End If
                Loop Until bFinLoop

                'Find number of >'s:
                iCurPos = 1
                bFinLoop = False
                Do
                    If InStr(iCurPos, Lines(i), ">") <> 0 Then
                        iNumParamEnd = iNumParamEnd + 1
                        iCurPos = InStr(iCurPos, Lines(i), ">") + 1
                    Else
                        bFinLoop = True
                    End If
                Loop Until bFinLoop

                If iNumParamStart > iNumParamEnd Then
                    LogASLError("Expected > in " & ReportErrorLine(Lines(i)), LOGTYPE_FATALERROR)
                    bHasErrors = True
                ElseIf iNumParamStart < iNumParamEnd Then
                    LogASLError("Too many > in " & ReportErrorLine(Lines(i)), LOGTYPE_FATALERROR)
                    bHasErrors = True
                End If
            End If
        Next i

        'Exit if errors found
        If bHasErrors = True Then
            ErrorCheck = True
            Exit Function
        End If


        ' Checks that define sections have parameters:
        For i = 1 To NumberSections
            iCurBegin = DefineBlocks(i).StartLine
            iCurEnd = DefineBlocks(i).EndLine

            If BeginsWith(Lines(iCurBegin), "define game") Then
                If InStr(Lines(iCurBegin), "<") = 0 Then
                    LogASLError("'define game' has no parameter - game has no name", LOGTYPE_FATALERROR)
                    ErrorCheck = True
                    Exit Function
                End If
            Else
                If Not BeginsWith(Lines(iCurBegin), "define synonyms") And Not BeginsWith(Lines(iCurBegin), "define options") Then
                    If InStr(Lines(iCurBegin), "<") = 0 Then
                        LogASLError(Lines(iCurBegin) & " has no parameter", LOGTYPE_FATALERROR)
                        bHasErrors = True
                    End If
                End If
            End If
        Next i

        'Exit if errors found
        If bHasErrors = True Then
            ErrorCheck = True
            Exit Function
        End If

    End Function

    Private Function GetAfterParameter(ByRef InputLine As String) As String
        ' Returns everything after the end of the first parameter
        ' in a string, i.e. for "use <thing> do <myproc>" it
        ' returns "do <myproc>"
        Dim EOP As Integer
        EOP = InStr(InputLine, ">")

        If EOP = 0 Or EOP + 1 > Len(InputLine) Then
            GetAfterParameter = ""
        Else
            GetAfterParameter = Trim(Mid(InputLine, EOP + 1))
        End If

    End Function

    Private Function ObliterateParameters(ByRef InputLine As String) As String

        Dim bInParameter As Boolean
        Dim ExitCharacter As String = ""
        Dim CurChar As String
        Dim OutputLine As String = ""
        Dim ObscuringFunctionName As Boolean
        Dim i As Integer

        bInParameter = False

        For i = 1 To Len(InputLine)
            CurChar = Mid(InputLine, i, 1)

            If bInParameter Then
                If ExitCharacter = ")" Then
                    If InStr("$#%", CurChar) > 0 Then
                        ' We might be converting a line like:
                        '   if ( $rand(1;10)$ < 3 ) then {
                        ' and we don't want it to end up like this:
                        '   if (~~~~~~~~~~~)$ <~~~~~~~~~~~
                        ' which will cause all sorts of confustion. So,
                        ' we get rid of everything between the $ characters
                        ' in this case, and set a flag so we know what we're
                        ' doing.

                        ObscuringFunctionName = True
                        ExitCharacter = CurChar

                        ' Move along please

                        OutputLine = OutputLine & "~"
                        i = i + 1
                        CurChar = Mid(InputLine, i, 1)
                    End If
                End If
            End If

            If Not bInParameter Then
                OutputLine = OutputLine & CurChar
                If CurChar = "<" Then
                    bInParameter = True
                    ExitCharacter = ">"
                End If
                If CurChar = "(" Then
                    bInParameter = True
                    ExitCharacter = ")"
                End If
            Else
                If CurChar = ExitCharacter Then
                    If Not ObscuringFunctionName Then
                        bInParameter = False
                        OutputLine = OutputLine & CurChar
                    Else
                        ' We've finished obscuring the function name,
                        ' now let's find the next ) as we were before
                        ' we found this dastardly function
                        ObscuringFunctionName = False
                        ExitCharacter = ")"
                        OutputLine = OutputLine & "~"
                    End If
                Else
                    OutputLine = OutputLine & "~"
                End If
            End If
        Next i

        If bInParameter Then
            ObliterateParameters = "'<ERROR;" & ExitCharacter & ";" & OutputLine
        Else
            ObliterateParameters = OutputLine
        End If

    End Function

    Private Function ObliterateVariableNames(ByRef InputLine As String) As String
        Dim bInParameter As Boolean
        Dim ExitCharacter As String = ""
        Dim OutputLine As String = ""
        Dim CurChar As String
        Dim i As Integer

        bInParameter = False

        For i = 1 To Len(InputLine)
            CurChar = Mid(InputLine, i, 1)
            If Not bInParameter Then
                OutputLine = OutputLine & CurChar
                If CurChar = "$" Then
                    bInParameter = True
                    ExitCharacter = "$"
                End If
                If CurChar = "#" Then
                    bInParameter = True
                    ExitCharacter = "#"
                End If
                If CurChar = "%" Then
                    bInParameter = True
                    ExitCharacter = "%"
                End If
                ' The ~ was for collectables, and this syntax only
                ' exists in Quest 2.x. The ~ was only finally
                ' allowed to be present on its own in ASL 320.
                If CurChar = "~" And GameASLVersion < 320 Then
                    bInParameter = True
                    ExitCharacter = "~"
                End If
            Else
                If CurChar = ExitCharacter Then
                    bInParameter = False
                    OutputLine = OutputLine & CurChar
                Else
                    OutputLine = OutputLine & "X"
                End If
            End If
        Next i

        If bInParameter Then
            OutputLine = "'<ERROR;" & ExitCharacter & ";" & OutputLine
        End If

        ObliterateVariableNames = OutputLine

    End Function

    Private Sub RemoveComments()
        Dim i, AposPos As Integer
        Dim InTextBlock As Boolean
        Dim InSynonymsBlock As Boolean
        Dim OblitLine As String

        ' If in a synonyms block, we want to remove lines which are comments, but
        ' we don't want to remove synonyms that contain apostrophes, so we only
        ' get rid of lines with an "'" at the beginning or with " '" in them

        For i = 1 To UBound(Lines)

            If BeginsWith(Lines(i), "'!qdk-note:") Then
                Lines(i) = "#!qdk-note:" & GetEverythingAfter(Lines(i), "'!qdk-note:")
            Else
                If BeginsWith(Lines(i), "define text ") Then
                    InTextBlock = True
                ElseIf Trim(Lines(i)) = "define synonyms" Then
                    InSynonymsBlock = True
                ElseIf BeginsWith(Lines(i), "define type ") Then
                    InSynonymsBlock = True
                ElseIf Trim(Lines(i)) = "end define" Then
                    InTextBlock = False
                    InSynonymsBlock = False
                End If

                If Not InTextBlock And Not InSynonymsBlock Then
                    If InStr(Lines(i), "'") > 0 Then
                        OblitLine = ObliterateParameters(Lines(i))
                        If Not BeginsWith(OblitLine, "'<ERROR;") Then
                            AposPos = InStr(OblitLine, "'")

                            If AposPos <> 0 Then
                                Lines(i) = Trim(Left(Lines(i), AposPos - 1))
                            End If
                        End If
                    End If
                ElseIf InSynonymsBlock Then
                    If Left(Trim(Lines(i)), 1) = "'" Then
                        Lines(i) = ""
                    Else
                        ' we look for " '", not "'" in synonyms lines
                        AposPos = InStr(ObliterateParameters(Lines(i)), " '")
                        If AposPos <> 0 Then
                            Lines(i) = Trim(Left(Lines(i), AposPos - 1))
                        End If
                    End If
                End If
            End If

        Next i
    End Sub

    Private Function ReportErrorLine(ByRef InputLine As String) As String
        ' We don't want to see the "!intproc" in logged error reports lines.
        ' This function replaces these "do" lines with a nicer-looking "..." for error reporting.

        Dim ReplaceFrom As Integer
        Dim OutputLine As String

        ReplaceFrom = InStr(InputLine, "do <!intproc")
        If ReplaceFrom <> 0 Then
            OutputLine = Left(InputLine, ReplaceFrom - 1) & "..."
        Else
            OutputLine = InputLine
        End If

        ReportErrorLine = OutputLine
    End Function

    Private Function YesNo(ByRef yn As Boolean) As String
        If yn = True Then YesNo = "Yes" Else YesNo = "No"
    End Function

    Private Function OneZero(ByRef theValue As Boolean) As Integer
        ' Used for check boxes where 1=true, 0=false
        If theValue = True Then OneZero = 1 Else OneZero = 0
    End Function

    Private Function IsOne(ByRef theValue As Integer) As Boolean
        ' Used for checkboxes
        If theValue = 1 Then IsOne = True Else IsOne = False
    End Function

    Private Function IsYes(ByRef yn As String) As Boolean
        If LCase(yn) = "yes" Then IsYes = True Else IsYes = False
    End Function

    Friend Function BeginsWith(ByRef s As String, ByRef T As String) As Boolean
        ' Compares the beginning of the line with a given
        ' string. Case insensitive.

        ' Example: beginswith("Hello there","HeLlO")=TRUE

        Dim TT As Integer

        TT = Len(T)

        If Left(LTrim(LCase(s)), TT) = LCase(T) Then
            BeginsWith = True
        Else
            BeginsWith = False
        End If

    End Function

    Private Function ConvertCASKeyword(ByRef CASchar As String) As String

        Dim c As Byte = System.Text.Encoding.GetEncoding(1252).GetBytes(CASchar)(0)
        Dim CK As String = CASkeywords(c)

        If CK = "!cr" Then
            CK = vbCrLf
        End If

        ConvertCASKeyword = CK

    End Function

    Private Sub ConvertMultiLines()
        'Goes through each section capable of containing
        'script commands and puts any multiple-line script commands
        'into separate procedures. Also joins multiple-line "if"
        'statements.

        'This calls RemoveComments after joining lines, so that lines
        'with "'" as part of a multi-line parameter are not destroyed,
        'before looking for braces.

        Dim i As Integer

        For i = UBound(Lines) To 1 Step -1
            If Right(Lines(i), 2) = "__" Then
                Lines(i) = Left(Lines(i), Len(Lines(i)) - 2) & LTrim(Lines(i + 1))
                Lines(i + 1) = ""
                'Recalculate this line again
                i = i + 1
            ElseIf Right(Lines(i), 1) = "_" Then
                Lines(i) = Left(Lines(i), Len(Lines(i)) - 1) & LTrim(Lines(i + 1))
                Lines(i + 1) = ""
                'Recalculate this line again
                i = i + 1
            End If
        Next i

        RemoveComments()

    End Sub

    Private Function GetDefineBlock(ByRef blockname As String) As DefineBlock

        ' Returns the start and end points of a named block.
        ' Returns 0 if block not found.

        Dim i As Integer
        Dim l, BlockType As String

        GetDefineBlock.StartLine = 0
        GetDefineBlock.EndLine = 0

        For i = 1 To NumberSections
            ' Get the first line of the define section:
            l = Lines(DefineBlocks(i).StartLine)

            ' Now, starting from the first word after 'define',
            ' retrieve the next word and compare it to blockname:

            ' Add a space for define blocks with no parameter
            If InStr(8, l, " ") = 0 Then l = l & " "

            BlockType = Mid(l, 8, InStr(8, l, " ") - 8)

            If BlockType = blockname Then
                ' Return the start and end points
                GetDefineBlock.StartLine = DefineBlocks(i).StartLine
                GetDefineBlock.EndLine = DefineBlocks(i).EndLine
                i = NumberSections
            End If
        Next i

    End Function

    Private Function DefineBlockParam(blockname As String, Param As String) As DefineBlock

        ' Returns the start and end points of a named block

        Dim i As Integer
        Dim sBlockType As String
        Dim SP As Integer
        Dim oCache As Dictionary(Of String, String)
        Dim sBlockName As String
        Dim asBlock() As String

        Param = "k" & Param ' protect against numeric block names

        If Not m_oDefineBlockParams.ContainsKey(blockname) Then
            ' Lazily create cache of define block params

            oCache = New Dictionary(Of String, String)
            m_oDefineBlockParams.Add(blockname, oCache)

            For i = 1 To NumberSections
                ' get the word after "define", e.g. "procedure"
                sBlockType = GetEverythingAfter(Lines(DefineBlocks(i).StartLine), "define ")
                SP = InStr(sBlockType, " ")
                If SP <> 0 Then
                    sBlockType = Trim(Left(sBlockType, SP - 1))
                End If

                If sBlockType = blockname Then
                    sBlockName = RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread, False)

                    sBlockName = "k" & sBlockName

                    If Not oCache.ContainsKey(sBlockName) Then
                        oCache.Add(sBlockName, DefineBlocks(i).StartLine & "," & DefineBlocks(i).EndLine)
                    Else
                        ' silently ignore duplicates
                    End If
                End If
            Next i
        Else
            oCache = m_oDefineBlockParams.Item(blockname)
        End If

        If oCache.ContainsKey(Param) Then
            asBlock = Split(oCache.Item(Param), ",")
            DefineBlockParam.StartLine = CInt(asBlock(0))
            DefineBlockParam.EndLine = CInt(asBlock(1))
        End If

    End Function

    Friend Function GetEverythingAfter(ByRef TheString As String, ByRef thetext As String) As String

        Dim l As Integer

        If Len(thetext) > Len(TheString) Then
            GetEverythingAfter = ""
            Exit Function
        End If
        l = Len(thetext)
        GetEverythingAfter = Right(TheString, Len(TheString) - l)
    End Function

    Private Function Keyword2CAS(ByRef KWord As String) As String

        Dim k As String
        Dim i As Integer

        If KWord = "" Then
            Keyword2CAS = ""
            Exit Function
        End If
        k = ""

        For i = 0 To 255
            If LCase(KWord) = LCase(CASkeywords(i)) Then
                k = Chr(i)
                i = 255
            End If
        Next i

        If k = "" Then
            Keyword2CAS = Keyword2CAS("!unknown") & KWord & Keyword2CAS("!unknown")
        Else
            Keyword2CAS = k
        End If

    End Function

    Private Sub LoadCASKeywords()

        'Loads data required for conversion of CAS files

        Dim SemiColonPos, FH, KNum As Integer
        Dim KWord As String

        FH = FreeFile()

        Dim QuestDatLines As String() = GetResourceLines(My.Resources.QuestDAT)

        For Each line As String In QuestDatLines
            If Left(line, 1) <> "#" Then
                'Lines isn't a comment - so parse it.
                SemiColonPos = InStr(line, ";")
                KWord = Trim(Left(line, SemiColonPos - 1))
                KNum = CInt(Right(line, Len(line) - SemiColonPos))
                CASkeywords(KNum) = KWord
            End If
        Next

        Exit Sub

    End Sub

    Private Function GetResourceLines(res As Byte()) As String()
        Dim enc As New System.Text.UTF8Encoding()
        Dim resFile As String = enc.GetString(res)
        Return Split(resFile, Chr(13) + Chr(10))
    End Function

    Private Function ParseFile(ByRef Filename As String) As Boolean
        'Returns FALSE if failed.

        Dim FileNum As Integer
        Dim bHasErrors As Boolean
        Dim bResult As Boolean
        Dim LibCode(0) As String
        Dim LibLines As Integer
        Dim IgnoreMode, SkipCheck As Boolean
        Dim l, c, i, D, j As Integer
        Dim LibFileHandle As Integer
        Dim LibResourceLines As String()
        Dim LibFile As String
        Dim LibLine As String
        Dim InDefGameBlock, GameLine As Integer
        Dim InDefSynBlock, SynLine As Integer
        Dim LibFoundThisSweep As Boolean
        Dim LibFileName As String
        Dim LibraryList(0) As String
        Dim NumLibraries As Integer
        Dim LibraryAlreadyIncluded As Boolean
        Dim InDefTypeBlock As Integer
        Dim TypeBlockName As String
        Dim TypeLine As Integer
        Dim DefineCount, CurLine As Integer

        m_oDefineBlockParams = New Dictionary(Of String, Dictionary(Of String, String))

        bResult = True

        FileNum = FreeFile()
        ' Parses file and returns the positions of each main
        ' 'define' block. Supports nested defines.

        If LCase(Right(Filename, 4)) = ".zip" Then
            m_originalFilename = Filename
            Filename = GetUnzippedFile(Filename)
            GamePath = System.IO.Path.GetDirectoryName(Filename)
        End If

        If LCase(Right(Filename, 4)) = ".asl" Or LCase(Right(Filename, 4)) = ".txt" Then
            'Read file into Lines array
            Dim fileData As String

            If Config.ReadGameFileFromAzureBlob Then
                Using client As New WebClient
                    fileData = client.DownloadString(Filename)

                    Dim fileBytes As Byte() = client.DownloadData(Filename)
                    m_gameId = TextAdventures.Utility.Utility.CalculateMD5Hash(fileBytes)
                End Using
            Else
                fileData = System.IO.File.ReadAllText(Filename)
            End If

            Dim aslLines As String() = fileData.Split(Chr(13))
            ReDim Lines(aslLines.Length)
            Lines(0) = ""

            For l = 1 To aslLines.Length
                Lines(l) = aslLines(l - 1)
                RemoveTabs(Lines(l))
                Lines(l) = Lines(l).Trim(" "c, Chr(10), Chr(13))
            Next

            l = aslLines.Length

        ElseIf LCase(Right(Filename, 4)) = ".cas" Then
            LogASLError("Loading CAS")
            If LoadCASFile(Filename) = False Then
                OpenErrorReport = OpenErrorReport & "Unable to open CAS file." & vbCrLf
                ParseFile = False
                Exit Function
            End If
            l = UBound(Lines)

        Else
            Throw New InvalidOperationException("Unrecognized file extension")
        End If

        Dim LibVer As Integer

        ' Add libraries to end of code:

        NumLibraries = 0

        Do
            LibFoundThisSweep = False
            For i = l To 1 Step -1
                ' We search for includes backwards as a game might include
                ' some-general.lib and then something-specific.lib which needs
                ' something-general; if we include something-specific first,
                ' then something-general afterwards, something-general's startscript
                ' gets executed before something-specific's, as we execute the
                ' lib startscripts backwards as well
                If BeginsWith(Lines(i), "!include ") Then
                    LibFileName = RetrieveParameter(Lines(i), NullThread)
                    'Clear !include statement
                    Lines(i) = ""
                    LibraryAlreadyIncluded = False
                    LogASLError("Including library '" & LibFileName & "'...", LOGTYPE_INIT)

                    For j = 1 To NumLibraries
                        If LCase(LibFileName) = LCase(LibraryList(j)) Then
                            LibraryAlreadyIncluded = True
                            j = NumLibraries
                        End If
                    Next j

                    If LibraryAlreadyIncluded Then
                        LogASLError("     - Library already included.", LOGTYPE_INIT)
                    Else
                        NumLibraries = NumLibraries + 1
                        ReDim Preserve LibraryList(NumLibraries)
                        LibraryList(NumLibraries) = LibFileName

                        LibFoundThisSweep = True
                        LibResourceLines = Nothing

                        LibFile = GamePath & LibFileName
                        LogASLError(" - Searching for " & LibFile & " (game path)", LOGTYPE_INIT)
                        LibFileHandle = FreeFile()

                        If System.IO.File.Exists(LibFile) Then
                            FileOpen(LibFileHandle, LibFile, OpenMode.Input)
                        Else
                            ' File was not found; try standard Quest libraries (stored here as resources)

                            LogASLError("     - Library not found in game path.", LOGTYPE_INIT)
                            LogASLError(" - Searching for " & LibFile & " (standard libraries)", LOGTYPE_INIT)
                            LibResourceLines = GetLibraryLines(LibFileName)

                            If LibResourceLines Is Nothing Then
                                LogASLError("Library not found.", LOGTYPE_FATALERROR)
                                OpenErrorReport = OpenErrorReport & "Library '" & LibraryList(NumLibraries) & "' not found." & vbCrLf
                                ParseFile = False
                                Exit Function
                            End If
                        End If

                        LogASLError("     - Found library, opening...", LOGTYPE_INIT)

                        On Error GoTo 0

                        LibLines = 0

                        If LibResourceLines Is Nothing Then
                            Do Until EOF(LibFileHandle)
                                LibLines = LibLines + 1
                                LibLine = LineInput(LibFileHandle)
                                RemoveTabs(LibLine)
                                ReDim Preserve LibCode(LibLines)
                                LibCode(LibLines) = Trim(LibLine)
                            Loop
                            FileClose(LibFileHandle)
                        Else
                            For Each ResLibLine As String In LibResourceLines
                                LibLines = LibLines + 1
                                ReDim Preserve LibCode(LibLines)
                                LibLine = ResLibLine
                                RemoveTabs(LibLine)
                                LibCode(LibLines) = Trim(LibLine)
                            Next
                        End If

                        LibVer = -1

                        If LibCode(1) = "!library" Then
                            For c = 1 To LibLines
                                If BeginsWith(LibCode(c), "!asl-version ") Then
                                    LibVer = CInt(RetrieveParameter(LibCode(c), NullThread))
                                    c = LibLines
                                End If
                            Next c
                        Else
                            'Old library
                            LibVer = 100
                        End If

                        If LibVer = -1 Then
                            LogASLError(" - Library has no asl-version information.", LOGTYPE_LIBRARYWARNINGERROR)
                            LibVer = 200
                        End If

                        IgnoreMode = False
                        For c = 1 To LibLines
                            If BeginsWith(LibCode(c), "!include ") Then
                                ' Quest only honours !include in a library for asl-version
                                ' 311 or later, as it ignored them in versions < 3.11
                                If LibVer >= 311 Then
                                    AddLine(LibCode(c))
                                    l = l + 1
                                End If
                            ElseIf Left(LibCode(c), 1) <> "!" And Left(LibCode(c), 1) <> "'" And Not IgnoreMode Then
                                AddLine(LibCode(c))
                                l = l + 1
                            Else
                                If LibCode(c) = "!addto game" Then
                                    InDefGameBlock = 0
                                    For D = 1 To UBound(Lines)
                                        If BeginsWith(Lines(D), "define game ") Then
                                            InDefGameBlock = 1
                                        ElseIf BeginsWith(Lines(D), "define ") Then
                                            If InDefGameBlock <> 0 Then
                                                InDefGameBlock = InDefGameBlock + 1
                                            End If
                                        ElseIf Lines(D) = "end define" And InDefGameBlock = 1 Then
                                            GameLine = D
                                            D = UBound(Lines)
                                        ElseIf Lines(D) = "end define" Then
                                            If InDefGameBlock <> 0 Then
                                                InDefGameBlock = InDefGameBlock - 1
                                            End If
                                        End If
                                    Next D

                                    Do
                                        c = c + 1
                                        If Not BeginsWith(LibCode(c), "!end") Then
                                            ReDim Preserve Lines(UBound(Lines) + 1)
                                            For D = UBound(Lines) To GameLine + 1 Step -1
                                                Lines(D) = Lines(D - 1)
                                            Next D

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

                                            If LibVer >= 311 And BeginsWith(LibCode(c), "startscript ") Then
                                                Lines(GameLine) = "lib " & LibCode(c)
                                            ElseIf LibVer >= 392 And (BeginsWith(LibCode(c), "command ") Or BeginsWith(LibCode(c), "verb ")) Then
                                                Lines(GameLine) = "lib " & LibCode(c)
                                            Else
                                                Lines(GameLine) = LibCode(c)
                                            End If

                                            l = l + 1
                                            GameLine = GameLine + 1
                                        End If
                                    Loop Until BeginsWith(LibCode(c), "!end")
                                ElseIf LibCode(c) = "!addto synonyms" Then
                                    InDefSynBlock = 0
                                    For D = 1 To UBound(Lines)
                                        If Lines(D) = "define synonyms" Then
                                            InDefSynBlock = 1
                                        ElseIf Lines(D) = "end define" And InDefSynBlock = 1 Then
                                            SynLine = D
                                            D = UBound(Lines)
                                        End If
                                    Next D

                                    If InDefSynBlock = 0 Then
                                        'No "define synonyms" block in game - so add it
                                        AddLine("define synonyms")
                                        AddLine("end define")
                                        SynLine = UBound(Lines)
                                    End If

                                    Do
                                        c = c + 1
                                        If Not BeginsWith(LibCode(c), "!end") Then
                                            ReDim Preserve Lines(UBound(Lines) + 1)
                                            For D = UBound(Lines) To SynLine + 1 Step -1
                                                Lines(D) = Lines(D - 1)
                                            Next D
                                            Lines(SynLine) = LibCode(c)
                                            l = l + 1
                                            SynLine = SynLine + 1
                                        End If
                                    Loop Until BeginsWith(LibCode(c), "!end")
                                ElseIf BeginsWith(LibCode(c), "!addto type ") Then
                                    InDefTypeBlock = 0
                                    TypeBlockName = LCase(RetrieveParameter(LibCode(c), NullThread))
                                    For D = 1 To UBound(Lines)
                                        If LCase(Lines(D)) = "define type <" & TypeBlockName & ">" Then
                                            InDefTypeBlock = 1
                                        ElseIf Lines(D) = "end define" And InDefTypeBlock = 1 Then
                                            TypeLine = D
                                            D = UBound(Lines)
                                        End If
                                    Next D

                                    If InDefTypeBlock = 0 Then
                                        'No "define type (whatever)" block in game - so add it
                                        AddLine("define type <" & TypeBlockName & ">")
                                        AddLine("end define")
                                        TypeLine = UBound(Lines)
                                    End If

                                    Do
                                        c = c + 1
                                        If c > LibLines Then Exit Do
                                        If Not BeginsWith(LibCode(c), "!end") Then
                                            ReDim Preserve Lines(UBound(Lines) + 1)
                                            For D = UBound(Lines) To TypeLine + 1 Step -1
                                                Lines(D) = Lines(D - 1)
                                            Next D
                                            Lines(TypeLine) = LibCode(c)
                                            l = l + 1
                                            TypeLine = TypeLine + 1
                                        End If
                                    Loop Until BeginsWith(LibCode(c), "!end")


                                ElseIf LibCode(c) = "!library" Then
                                    'ignore
                                ElseIf BeginsWith(LibCode(c), "!asl-version ") Then
                                    'ignore
                                ElseIf BeginsWith(LibCode(c), "'") Then
                                    'ignore
                                ElseIf BeginsWith(LibCode(c), "!QDK") Then
                                    IgnoreMode = True
                                ElseIf BeginsWith(LibCode(c), "!end") Then
                                    IgnoreMode = False
                                End If
                            End If
                        Next c
                    End If
                End If
            Next i
        Loop Until LibFoundThisSweep = False

        SkipCheck = False

        Dim FilenameNoPath As String
        Dim LastSlashPos, SlashPos As Integer
        Dim CurPos As Integer
        CurPos = 1
        Do
            SlashPos = InStr(CurPos, Filename, "\")
            If SlashPos <> 0 Then LastSlashPos = SlashPos
            CurPos = SlashPos + 1
        Loop Until SlashPos = 0
        FilenameNoPath = LCase(Mid(Filename, LastSlashPos + 1))

        For i = 1 To NumSkipCheckFiles
            If FilenameNoPath = SkipCheckFile(i) Then
                SkipCheck = True
                i = NumSkipCheckFiles
            End If
        Next i

        If FilenameNoPath = "musicvf1.cas" Then
            m_useStaticFrameForPictures = True
        End If

        'RemoveComments called within ConvertMultiLines
        ConvertMultiLines()

        If Not SkipCheck Then
            If Not CheckSections() Then
                ParseFile = False
                Exit Function
            End If
        End If

        NumberSections = 1

        For i = 1 To l
            ' find section beginning with 'define'
            If BeginsWith(Lines(i), "define") = True Then
                ' Now, go through until we reach an 'end define'. However, if we
                ' encounter another 'define' there is a nested define. So, if we
                ' encounter 'define' we increment the definecount. When we find an
                ' 'end define' we decrement it. When definecount is zero, we have
                ' found the end of the section.

                DefineCount = 1

                ' Don't count the current line - we know it begins with 'define'...
                CurLine = i + 1
                Do
                    If BeginsWith(Lines(CurLine), "define") = True Then
                        DefineCount = DefineCount + 1
                    ElseIf BeginsWith(Lines(CurLine), "end define") = True Then
                        DefineCount = DefineCount - 1
                    End If
                    CurLine = CurLine + 1
                Loop Until DefineCount = 0
                CurLine = CurLine - 1

                ' Now, we know that the define section begins at i and ends at
                ' curline. Remember where the section begins and ends:

                ReDim Preserve DefineBlocks(NumberSections)
                DefineBlocks(NumberSections).StartLine = i
                DefineBlocks(NumberSections).EndLine = CurLine

                NumberSections = NumberSections + 1
                i = CurLine
            End If
        Next i

        NumberSections = NumberSections - 1

        Dim GotGameBlock As Boolean
        GotGameBlock = False
        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define game ") Then
                GotGameBlock = True
                i = NumberSections
            End If
        Next i

        If Not GotGameBlock Then
            OpenErrorReport = OpenErrorReport & "No 'define game' block." & vbCrLf
            ParseFile = False
            Exit Function
        End If

        ConvertMultiLineSections()

        bHasErrors = ConvertFriendlyIfs()
        If Not bHasErrors Then bHasErrors = ErrorCheck()

        If bHasErrors Then
            Throw New InvalidOperationException("Errors found in game file.")
            bResult = False
        End If

        SaveGameFile = ""

        ParseFile = bResult
        Exit Function

ErrorHandler:
        If Err.Number = 53 Then
            OpenErrorReport = OpenErrorReport & "File not found." & vbCrLf
            ParseFile = False
            Exit Function

        ElseIf Err.Number = 76 Then
            OpenErrorReport = OpenErrorReport & "Path not found." & vbCrLf
            ParseFile = False
            Exit Function

        ElseIf Err.Number = 71 Then
            OpenErrorReport = OpenErrorReport & "Unable to open file." & vbCrLf
            ParseFile = False
            Exit Function
        Else
            Err.Raise(Err.Number)
        End If

    End Function

    Friend Sub LogASLError(TheError As String, Optional ByRef MessageType As Integer = LOGTYPE_MISC)

        ' TO DO: This should raise an event so we can access logs from Player

        If MessageType = LOGTYPE_FATALERROR Then
            TheError = "FATAL ERROR: " & TheError
        ElseIf MessageType = LOGTYPE_WARNINGERROR Then
            TheError = "ERROR: " & TheError
        ElseIf MessageType = LOGTYPE_LIBRARYWARNINGERROR Then
            TheError = "WARNING ERROR (LIBRARY): " & TheError
        ElseIf MessageType = LOGTYPE_INIT Then
            TheError = "INIT: " & TheError
        ElseIf MessageType = LOGTYPE_WARNING Then
            TheError = "WARNING: " & TheError
        ElseIf MessageType = LOGTYPE_USERERROR Then
            TheError = "ERROR (REQUESTED): " & TheError
        ElseIf MessageType = LOGTYPE_INTERNALERROR Then
            TheError = "INTERNAL ERROR: " & TheError
        End If

        m_log.Add(TheError)

    End Sub

    Friend Function RetrieveParameter(ByRef InputString As String, ByRef Thread As ThreadData, Optional ByRef bConvertStringVariables As Boolean = True) As String

        ' Returns the parameters between < and > in a string
        Dim RetrParam As String
        Dim NewParam As String
        Dim StartPos As Integer
        Dim EndPos As Integer

        StartPos = InStr(InputString, "<")
        EndPos = InStr(InputString, ">")

        If StartPos = 0 Or EndPos = 0 Then
            LogASLError("Expected parameter in '" & ReportErrorLine(InputString) & "'", LOGTYPE_WARNINGERROR)
            Return ""
        End If

        RetrParam = Mid(InputString, StartPos + 1, (EndPos - StartPos) - 1)

        If bConvertStringVariables Then
            If GameASLVersion >= 320 Then
                NewParam = ConvertParameter(ConvertParameter(ConvertParameter(RetrParam, "#", CONVERT_STRINGS, Thread), "%", CONVERT_NUMERIC, Thread), "$", CONVERT_FUNCTIONS, Thread)
            Else
                If Not Left(RetrParam, 9) = "~Internal" Then
                    NewParam = ConvertParameter(ConvertParameter(ConvertParameter(ConvertParameter(RetrParam, "#", CONVERT_STRINGS, Thread), "%", CONVERT_NUMERIC, Thread), "~", CONVERT_COLLECTABLES, Thread), "$", CONVERT_FUNCTIONS, Thread)
                Else
                    NewParam = RetrParam
                End If
            End If
        Else
            NewParam = RetrParam
        End If

        NewParam = EvaluateInlineExpressions(NewParam)

        RetrieveParameter = NewParam

    End Function

    Private Sub AddLine(ByRef theline As String)
        'Adds a line to the game script
        Dim NumLines As Integer

        NumLines = UBound(Lines) + 1
        ReDim Preserve Lines(NumLines)
        Lines(NumLines) = theline
    End Sub

    Private Function LoadCASFile(ByRef thefilename As String) As Boolean
        Dim EndLineReached, ExitTheLoop As Boolean
        Dim TextMode, FinTheLoop As Boolean
        Dim CasVersion As Integer
        Dim StartCat As String = ""
        Dim EndCatPos As Integer
        Dim FileData As String = ""
        Dim ChkVer As String
        Dim j As Integer
        Dim CurLin, TextData As String
        Dim CPos, NextLinePos As Integer
        Dim c, TL, CKW, D As String

        ReDim Lines(0)

        If Config.ReadGameFileFromAzureBlob Then
            Using client As New WebClient
                Dim url As String = Filename
                Dim baseAddress As Uri = New Uri(url)
                Dim directory As Uri = New Uri(baseAddress, ".")

                Try
                    FileData = client.DownloadString(url)
                Catch ex As WebException
                    url = directory.OriginalString + "_game.cas"
                    FileData = client.DownloadString(url)
                End Try

                Dim parts As String() = directory.OriginalString.Split("/"c)
                m_gameId = parts(parts.Length - 2)
            End Using
        Else
            FileData = System.IO.File.ReadAllText(thefilename, System.Text.Encoding.GetEncoding(1252))
        End If

        ChkVer = Left(FileData, 7)
        If ChkVer = "QCGF001" Then
            CasVersion = 1
        ElseIf ChkVer = "QCGF002" Then
            CasVersion = 2
        ElseIf ChkVer = "QCGF003" Then
            CasVersion = 3
        Else
            Throw New InvalidOperationException("Invalid or corrupted CAS file.")
        End If

        If CasVersion = 3 Then
            StartCat = Keyword2CAS("!startcat")
        End If

        For i As Integer = 9 To Len(FileData)
            If CasVersion = 3 And Mid(FileData, i, 1) = StartCat Then
                ' Read catalog
                StartCatPos = i
                EndCatPos = InStr(j, FileData, Keyword2CAS("!endcat"))
                ReadCatalog(Mid(FileData, j + 1, EndCatPos - j - 1))
                ResourceFile = thefilename
                ResourceOffset = EndCatPos + 1
                i = Len(FileData)
                m_fileData = FileData
            Else

                CurLin = ""
                EndLineReached = False
                If TextMode = True Then
                    TextData = Mid(FileData, i, InStr(i, FileData, Chr(253)) - (i - 1))
                    TextData = Left(TextData, Len(TextData) - 1)
                    CPos = 1
                    FinTheLoop = False

                    If TextData <> "" Then

                        Do
                            NextLinePos = InStr(CPos, TextData, Chr(0))
                            If NextLinePos = 0 Then
                                NextLinePos = Len(TextData) + 1
                                FinTheLoop = True
                            End If
                            TL = DecryptString(Mid(TextData, CPos, NextLinePos - CPos))
                            AddLine(TL)
                            CPos = NextLinePos + 1
                        Loop Until FinTheLoop

                    End If

                    TextMode = False
                    i = InStr(i, FileData, Chr(253))
                End If

                j = i
                Do
                    CKW = Mid(FileData, j, 1)
                    c = ConvertCASKeyword(CKW)

                    If c = vbCrLf Then
                        EndLineReached = True
                    Else
                        If Left(c, 1) <> "!" Then
                            CurLin = CurLin & c & " "
                        Else
                            If c = "!quote" Then
                                ExitTheLoop = False
                                CurLin = CurLin & "<"
                                Do
                                    j = j + 1
                                    D = Mid(FileData, j, 1)
                                    If D <> Chr(0) Then
                                        CurLin = CurLin & DecryptString(D)
                                    Else
                                        CurLin = CurLin & "> "
                                        ExitTheLoop = True
                                    End If
                                Loop Until ExitTheLoop
                            ElseIf c = "!unknown" Then
                                ExitTheLoop = False
                                Do
                                    j = j + 1
                                    D = Mid(FileData, j, 1)
                                    If D <> Chr(254) Then
                                        CurLin = CurLin & D
                                    Else
                                        ExitTheLoop = True
                                    End If
                                Loop Until ExitTheLoop
                                CurLin = CurLin & " "
                            End If
                        End If
                    End If

                    j = j + 1
                Loop Until EndLineReached
                AddLine(Trim(CurLin))
                If BeginsWith(CurLin, "define text") Or (CasVersion >= 2 And (BeginsWith(CurLin, "define synonyms") Or BeginsWith(CurLin, "define type") Or BeginsWith(CurLin, "define menu"))) Then
                    TextMode = True
                End If
                'j is already at correct place, but i will be
                'incremented - so put j back one or we will miss a
                'character.
                i = j - 1
            End If
        Next i

        LoadCASFile = True

    End Function

    Private Function DecryptString(ByRef sString As String) As String
        Dim OS As String
        Dim Z As Integer

        OS = ""
        For Z = 1 To Len(sString)
            Dim v As Byte = System.Text.Encoding.GetEncoding(1252).GetBytes(Mid(sString, Z, 1))(0)
            OS = OS & Chr(v Xor 255)
        Next Z

        DecryptString = OS
    End Function

    Private Sub RemoveTabs(ByRef ConvertLine As String)
        Dim foundalltabs As Boolean
        Dim CPos, TabChar As Integer

        If InStr(ConvertLine, Chr(9)) > 0 Then
            'Remove tab characters and change them into
            'spaces; otherwise they bugger up the Trim
            'commands.
            CPos = 1
            foundalltabs = False
            Do
                TabChar = InStr(CPos, ConvertLine, Chr(9))
                If TabChar <> 0 Then
                    ConvertLine = Left(ConvertLine, TabChar - 1) & Space(4) & Mid(ConvertLine, TabChar + 1)
                    CPos = TabChar + 1
                Else
                    foundalltabs = True
                End If
            Loop Until foundalltabs
        End If
    End Sub

    Public Sub CreatePath(ByRef sPath As String)
        System.IO.Directory.CreateDirectory(sPath)
    End Sub

    Private Sub DoAddRemove(ByRef ChildObjID As Integer, ByRef ParentObjID As Integer, ByRef DoAdd As Boolean, ByRef Thread As ThreadData)

        If DoAdd Then
            AddToObjectProperties("parent=" & Objs(ParentObjID).ObjectName, ChildObjID, Thread)
            Objs(ChildObjID).ContainerRoom = Objs(ParentObjID).ContainerRoom
        Else
            AddToObjectProperties("not parent", ChildObjID, Thread)
        End If

        If GameASLVersion >= 410 Then
            ' Putting something in a container implicitly makes that
            ' container "seen". Otherwise we could try to "look at" the
            ' object we just put in the container and have disambigution fail!
            AddToObjectProperties("seen", ParentObjID, Thread)
        End If

        UpdateVisibilityInContainers(Thread, Objs(ParentObjID).ObjectName)

    End Sub

    Private Sub DoLook(ByRef ObjID As Integer, ByRef Thread As ThreadData, Optional ByRef ShowExamineError As Boolean = False, Optional ByRef ShowDefaultDescription As Boolean = True)

        Dim FoundLook As Boolean
        Dim i, ErrMsgID As Integer
        Dim ObjectContents As String

        FoundLook = False

        ' First, set the "seen" property, and for ASL >= 391, update visibility for any
        ' object that is contained by this object.

        If GameASLVersion >= 391 Then
            AddToObjectProperties("seen", ObjID, Thread)
            UpdateVisibilityInContainers(Thread, Objs(ObjID).ObjectName)
        End If

        ' First look for action, then look
        ' for property, then check define
        ' section:

        Dim sLookLine As String
        With Objs(ObjID)

            For i = 1 To .NumberActions
                If .Actions(i).ActionName = "look" Then
                    FoundLook = True
                    ExecuteScript(.Actions(i).Script, Thread)
                    Exit For
                End If
            Next i

            If Not FoundLook Then
                For i = 1 To .NumberProperties
                    If .Properties(i).PropertyName = "look" Then
                        ' do this odd RetrieveParameter stuff to convert any variables
                        Print(RetrieveParameter("<" & .Properties(i).PropertyValue & ">", Thread), Thread)
                        FoundLook = True
                        Exit For
                    End If
                Next i
            End If

            If Not FoundLook Then
                For i = .DefinitionSectionStart To .DefinitionSectionEnd
                    If BeginsWith(Lines(i), "look ") Then

                        sLookLine = Trim(GetEverythingAfter(Lines(i), "look "))

                        If Left(sLookLine, 1) = "<" Then
                            Print(RetrieveParameter(Lines(i), Thread), Thread)
                        Else
                            ExecuteScript(sLookLine, Thread, ObjID)
                        End If

                        FoundLook = True
                    End If
                Next i
            End If

        End With

        If GameASLVersion >= 391 Then
            ObjectContents = ListContents(ObjID, Thread)
        Else
            ObjectContents = ""
        End If

        If Not FoundLook And ShowDefaultDescription Then

            If ShowExamineError Then
                ErrMsgID = ERROR_DEFAULTEXAMINE
            Else
                ErrMsgID = ERROR_DEFAULTLOOK
            End If

            ' Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            ' any contents.

            If ObjectContents = "" Then PlayerErrorMessage(ErrMsgID, Thread)
        End If

        If ObjectContents <> "" And ObjectContents <> "<script>" Then Print(ObjectContents, Thread)

    End Sub

    Private Sub DoOpenClose(ByRef ObjID As Integer, ByRef DoOpen As Boolean, ByRef ShowLook As Boolean, ByRef Thread As ThreadData)

        If DoOpen Then
            AddToObjectProperties("opened", ObjID, Thread)
            If ShowLook Then DoLook(ObjID, Thread, , False)
        Else
            AddToObjectProperties("not opened", ObjID, Thread)
        End If

        UpdateVisibilityInContainers(Thread, Objs(ObjID).ObjectName)

    End Sub

    Private Function EvaluateInlineExpressions(ByRef InputLine As String) As String

        ' Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>

        If GameASLVersion < 391 Then
            EvaluateInlineExpressions = InputLine
            Exit Function
        End If

        Dim EndBracePos, BracePos, CurPos As Integer
        Dim ExpResult As ExpressionResult
        Dim Expression, ResultLine As String

        CurPos = 1
        ResultLine = ""

        Do
            BracePos = InStr(CurPos, InputLine, "{")

            If BracePos <> 0 Then

                ResultLine = ResultLine & Mid(InputLine, CurPos, BracePos - CurPos)

                If Mid(InputLine, BracePos, 2) = "{{" Then
                    ' {{ = {
                    CurPos = BracePos + 2
                    ResultLine = ResultLine & "{"
                Else
                    EndBracePos = InStr(BracePos + 1, InputLine, "}")
                    If EndBracePos = 0 Then
                        LogASLError("Expected } in '" & InputLine & "'", LOGTYPE_WARNINGERROR)
                        EvaluateInlineExpressions = "<ERROR>"
                        Exit Function
                    Else
                        Expression = Mid(InputLine, BracePos + 1, EndBracePos - BracePos - 1)
                        ExpResult = ExpressionHandler(Expression)
                        If ExpResult.success <> EXPRESSION_OK Then
                            LogASLError("Error evaluating expression in <" & InputLine & "> - " & ExpResult.Message)
                            EvaluateInlineExpressions = "<ERROR>"
                            Exit Function
                        End If

                        ResultLine = ResultLine & ExpResult.Result
                        CurPos = EndBracePos + 1
                    End If
                End If
            Else
                ResultLine = ResultLine & Mid(InputLine, CurPos)
            End If
        Loop Until BracePos = 0 Or CurPos > Len(InputLine)

        ' Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        CurPos = 1
        Do
            BracePos = InStr(CurPos, ResultLine, "}}")
            If BracePos <> 0 Then
                ResultLine = Left(ResultLine, BracePos) & Mid(ResultLine, BracePos + 2)
                CurPos = BracePos + 1
            End If
        Loop Until BracePos = 0 Or CurPos > Len(ResultLine)

        EvaluateInlineExpressions = ResultLine

    End Function

    Private Sub ExecAddRemove(ByRef CommandLine As String, ByRef Thread As ThreadData)
        Dim ChildObjID As Integer
        Dim ChildObjectName As String
        Dim DoAdd As Boolean
        Dim SepPos, ParentObjID, SepLen As Integer
        Dim ParentObjectName As String
        Dim NoParentSpecified As Boolean
        Dim Verb As String = ""
        Dim i As Integer
        Dim Action As String
        Dim FoundAction As Boolean
        Dim ActionScript As String = ""
        Dim PropertyExists As Boolean
        Dim TextToPrint As String
        Dim IsContainer As Boolean
        Dim InventoryPlace As String
        Dim ErrorMsg As String = ""
        Dim GotObject As Boolean
        Dim ChildLength As Integer

        ' handles e.g. PUT CLOCK ON MANTLEPIECE
        '                  ^ child  ^ parent

        InventoryPlace = "inventory"

        NoParentSpecified = False

        If BeginsWith(CommandLine, "put ") Then
            Verb = "put"
            DoAdd = True
            SepPos = InStr(CommandLine, " on ")
            SepLen = 4
            If SepPos = 0 Then
                SepPos = InStr(CommandLine, " in ")
                SepLen = 4
            End If
            If SepPos = 0 Then
                SepPos = InStr(CommandLine, " onto ")
                SepLen = 6
            End If
        ElseIf BeginsWith(CommandLine, "add ") Then
            Verb = "add"
            DoAdd = True
            SepPos = InStr(CommandLine, " to ")
            SepLen = 4
        ElseIf BeginsWith(CommandLine, "remove ") Then
            Verb = "remove"
            DoAdd = False
            SepPos = InStr(CommandLine, " from ")
            SepLen = 6
        End If

        If SepPos = 0 Then
            NoParentSpecified = True
            SepPos = Len(CommandLine) + 1
        End If

        ChildLength = SepPos - (Len(Verb) + 2)

        If ChildLength < 0 Then
            PlayerErrorMessage(ERROR_BADCOMMAND, Thread)
            BadCmdBefore = Verb
            Exit Sub
        End If

        ChildObjectName = Trim(Mid(CommandLine, Len(Verb) + 2, ChildLength))

        GotObject = False

        If GameASLVersion >= 392 And DoAdd Then
            ChildObjID = Disambiguate(ChildObjectName, CurrentRoom & ";" & InventoryPlace, Thread)

            If ChildObjID > 0 Then
                If Objs(ChildObjID).ContainerRoom = InventoryPlace Then
                    GotObject = True
                Else
                    ' Player is not carrying the object they referred to. So, first take the object.
                    Print("(first taking " & Objs(ChildObjID).Article & ")", Thread)
                    ' Try to take the object
                    Thread.AllowRealNamesInCommand = True
                    ExecCommand("take " & Objs(ChildObjID).ObjectName, Thread, False, , True)

                    If Objs(ChildObjID).ContainerRoom = InventoryPlace Then GotObject = True
                End If

                If Not GotObject Then
                    BadCmdBefore = Verb
                    Exit Sub
                End If
            Else
                If ChildObjID <> -2 Then PlayerErrorMessage(ERROR_NOITEM, Thread)
                BadCmdBefore = Verb
                Exit Sub
            End If

        Else
            ChildObjID = Disambiguate(ChildObjectName, InventoryPlace & ";" & CurrentRoom, Thread)

            If ChildObjID <= 0 Then
                If ChildObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                BadCmdBefore = Verb
                Exit Sub
            End If
        End If

        If NoParentSpecified And DoAdd Then
            SetStringContents("quest.error.article", Objs(ChildObjID).Article, Thread)
            PlayerErrorMessage(ERROR_BADPUT, Thread)
            Exit Sub
        End If

        If DoAdd Then
            Action = "add"
        Else
            Action = "remove"
        End If

        If Not NoParentSpecified Then
            ParentObjectName = Trim(Mid(CommandLine, SepPos + SepLen))

            ParentObjID = Disambiguate(ParentObjectName, CurrentRoom & ";" & InventoryPlace, Thread)

            If ParentObjID <= 0 Then
                If ParentObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                BadCmdBefore = Left(CommandLine, SepPos + SepLen)
                Exit Sub
            End If
        Else
            ' Assume the player was referring to the parent that the object is already in,
            ' if it is even in an object already

            If Not IsYes(GetObjectProperty("parent", ChildObjID, True, False)) Then
                PlayerErrorMessage(ERROR_CANTREMOVE, Thread)
                Exit Sub
            End If

            ParentObjID = GetObjectIDNoAlias(GetObjectProperty("parent", ChildObjID, False, True))
        End If

        ' Check if parent is a container

        IsContainer = IsYes(GetObjectProperty("container", ParentObjID, True, False))

        If Not IsContainer Then
            If DoAdd Then
                PlayerErrorMessage(ERROR_CANTPUT, Thread)
            Else
                PlayerErrorMessage(ERROR_CANTREMOVE, Thread)
            End If
            Exit Sub
        End If

        ' Check object is already held by that parent

        If IsYes(GetObjectProperty("parent", ChildObjID, True, False)) Then
            If DoAdd And LCase(GetObjectProperty("parent", ChildObjID, False, False)) = LCase(Objs(ParentObjID).ObjectName) Then
                PlayerErrorMessage(ERROR_ALREADYPUT, Thread)
            End If
        End If

        ' NEW: Check parent and child are accessible to player
        If Not PlayerCanAccessObject(ChildObjID, , , ErrorMsg) Then
            If DoAdd Then
                PlayerErrorMessage_ExtendInfo(ERROR_CANTPUT, Thread, ErrorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(ERROR_CANTREMOVE, Thread, ErrorMsg)
            End If
            Exit Sub
        End If
        If Not PlayerCanAccessObject(ParentObjID, , , ErrorMsg) Then
            If DoAdd Then
                PlayerErrorMessage_ExtendInfo(ERROR_CANTPUT, Thread, ErrorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(ERROR_CANTREMOVE, Thread, ErrorMsg)
            End If
            Exit Sub
        End If

        ' Check if parent is a closed container

        If Not IsYes(GetObjectProperty("surface", ParentObjID, True, False)) And Not IsYes(GetObjectProperty("opened", ParentObjID, True, False)) Then
            ' Not a surface and not open, so can't add to this closed container.
            If DoAdd Then
                PlayerErrorMessage(ERROR_CANTPUT, Thread)
            Else
                PlayerErrorMessage(ERROR_CANTREMOVE, Thread)
            End If
            Exit Sub
        End If

        ' Now check if it can be added to (or removed from)

        ' First check for an action
        With Objs(ParentObjID)
            For i = 1 To .NumberActions
                If LCase(.Actions(i).ActionName) = Action Then
                    FoundAction = True
                    ActionScript = .Actions(i).Script
                    Exit For
                End If
            Next i
        End With

        If FoundAction Then
            SetStringContents("quest." & LCase(Action) & ".object.name", Objs(ChildObjID).ObjectName, Thread)
            ExecuteScript(ActionScript, Thread, ParentObjID)
        Else
            ' Now check for a property
            PropertyExists = IsYes(GetObjectProperty(Action, ParentObjID, True, False))

            If Not PropertyExists Then
                ' Show error message
                If DoAdd Then
                    PlayerErrorMessage(ERROR_CANTPUT, Thread)
                Else
                    PlayerErrorMessage(ERROR_CANTREMOVE, Thread)
                End If
            Else
                TextToPrint = GetObjectProperty(Action, ParentObjID, False, False)
                If TextToPrint = "" Then
                    ' Show default message
                    If DoAdd Then
                        PlayerErrorMessage(ERROR_DEFAULTPUT, Thread)
                    Else
                        PlayerErrorMessage(ERROR_DEFAULTREMOVE, Thread)
                    End If
                Else
                    Print(TextToPrint, Thread)
                End If

                DoAddRemove(ChildObjID, ParentObjID, DoAdd, Thread)

            End If
        End If

    End Sub

    Private Sub ExecAddRemoveScript(ByRef Parameter As String, ByRef DoAdd As Boolean, ByRef Thread As ThreadData)

        Dim ChildObjID, ParentObjID As Integer
        Dim CommandName As String
        Dim ChildName As String
        Dim ParentName As String = ""
        Dim SCP As Integer

        If DoAdd Then
            CommandName = "add"
        Else
            CommandName = "remove"
        End If

        SCP = InStr(Parameter, ";")
        If SCP = 0 And DoAdd Then
            LogASLError("No parent specified in '" & CommandName & " <" & Parameter & ">", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        If SCP <> 0 Then
            ChildName = LCase(Trim(Left(Parameter, SCP - 1)))
            ParentName = LCase(Trim(Mid(Parameter, SCP + 1)))
        Else
            ChildName = LCase(Trim(Parameter))
        End If

        ChildObjID = GetObjectIDNoAlias(ChildName)
        If ChildObjID = 0 Then
            LogASLError("Invalid child object name specified in '" & CommandName & " <" & Parameter & ">", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        If SCP <> 0 Then
            ParentObjID = GetObjectIDNoAlias(ParentName)
            If ParentObjID = 0 Then
                LogASLError("Invalid parent object name specified in '" & CommandName & " <" & Parameter & ">", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If

            DoAddRemove(ChildObjID, ParentObjID, DoAdd, Thread)
        Else
            AddToObjectProperties("not parent", ChildObjID, Thread)
            UpdateVisibilityInContainers(Thread, Objs(ParentObjID).ObjectName)
        End If

    End Sub

    Private Sub ExecOpenClose(ByRef CommandLine As String, ByRef Thread As ThreadData)
        Dim ObjID As Integer
        Dim ObjectName As String
        Dim DoOpen As Boolean
        Dim IsOpen, FoundAction As Boolean
        Dim i As Integer
        Dim Action As String = ""
        Dim ActionScript As String = ""
        Dim PropertyExists As Boolean
        Dim TextToPrint As String
        Dim IsContainer As Boolean
        Dim InventoryPlace As String
        Dim ErrorMsg As String = ""

        InventoryPlace = "inventory"

        If BeginsWith(CommandLine, "open ") Then
            Action = "open"
            DoOpen = True
        ElseIf BeginsWith(CommandLine, "close ") Then
            Action = "close"
            DoOpen = False
        End If

        ObjectName = GetEverythingAfter(CommandLine, Action & " ")

        ObjID = Disambiguate(ObjectName, CurrentRoom & ";" & InventoryPlace, Thread)

        If ObjID <= 0 Then
            If ObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
            BadCmdBefore = Action
            Exit Sub
        End If

        ' Check if it's even a container

        IsContainer = IsYes(GetObjectProperty("container", ObjID, True, False))

        If Not IsContainer Then
            If DoOpen Then
                PlayerErrorMessage(ERROR_CANTOPEN, Thread)
            Else
                PlayerErrorMessage(ERROR_CANTCLOSE, Thread)
            End If
            Exit Sub
        End If

        ' Check if it's already open (or closed)

        IsOpen = IsYes(GetObjectProperty("opened", ObjID, True, False))

        If DoOpen And IsOpen Then
            ' Object is already open
            PlayerErrorMessage(ERROR_ALREADYOPEN, Thread)
            Exit Sub
        ElseIf Not DoOpen And Not IsOpen Then
            ' Object is already closed
            PlayerErrorMessage(ERROR_ALREADYCLOSED, Thread)
            Exit Sub
        End If

        ' NEW: Check if it's accessible, i.e. check it's not itself inside another closed container

        If Not PlayerCanAccessObject(ObjID, , , ErrorMsg) Then
            If DoOpen Then
                PlayerErrorMessage_ExtendInfo(ERROR_CANTOPEN, Thread, ErrorMsg)
            Else
                PlayerErrorMessage_ExtendInfo(ERROR_CANTCLOSE, Thread, ErrorMsg)
            End If
            Exit Sub
        End If

        ' Now check if it can be opened (or closed)

        ' First check for an action
        With Objs(ObjID)
            For i = 1 To .NumberActions
                If LCase(.Actions(i).ActionName) = Action Then
                    FoundAction = True
                    ActionScript = .Actions(i).Script
                    Exit For
                End If
            Next i
        End With

        If FoundAction Then
            ExecuteScript(ActionScript, Thread, ObjID)
        Else
            ' Now check for a property
            PropertyExists = IsYes(GetObjectProperty(Action, ObjID, True, False))

            If Not PropertyExists Then
                ' Show error message
                If DoOpen Then
                    PlayerErrorMessage(ERROR_CANTOPEN, Thread)
                Else
                    PlayerErrorMessage(ERROR_CANTCLOSE, Thread)
                End If
            Else
                TextToPrint = GetObjectProperty(Action, ObjID, False, False)
                If TextToPrint = "" Then
                    ' Show default message
                    If DoOpen Then
                        PlayerErrorMessage(ERROR_DEFAULTOPEN, Thread)
                    Else
                        PlayerErrorMessage(ERROR_DEFAULTCLOSE, Thread)
                    End If
                Else
                    Print(TextToPrint, Thread)
                End If

                DoOpenClose(ObjID, DoOpen, True, Thread)

            End If
        End If

    End Sub

    Private Sub ExecuteSelectCase(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        Dim CaseBlockName As String
        Dim i As Integer
        Dim CaseBlock As DefineBlock
        Dim AfterLine, ThisCase As String
        Dim SCP As Integer
        Dim CaseMatch As Boolean
        Dim FinLoop As Boolean
        Dim ThisCondition, CheckValue As String
        Dim CaseScript As String = ""

        ' ScriptLine passed will look like this:
        '   select case <whatever> do <!intprocX>
        ' with all the case statements in the intproc.

        AfterLine = GetAfterParameter(ScriptLine)

        If Not BeginsWith(AfterLine, "do <!intproc") Then
            LogASLError("No case block specified for '" & ScriptLine & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        CaseBlockName = RetrieveParameter(AfterLine, Thread)
        CaseBlock = DefineBlockParam("procedure", CaseBlockName)
        CheckValue = RetrieveParameter(ScriptLine, Thread)
        CaseMatch = False

        For i = CaseBlock.StartLine + 1 To CaseBlock.EndLine - 1
            ' Go through all the cases until we find the one that matches

            If Lines(i) <> "" Then
                If Not BeginsWith(Lines(i), "case ") Then
                    LogASLError("Invalid line in 'select case' block: '" & Lines(i) & "'", LOGTYPE_WARNINGERROR)
                Else

                    If BeginsWith(Lines(i), "case else ") Then
                        CaseMatch = True
                        CaseScript = GetEverythingAfter(Lines(i), "case else ")
                    Else
                        ThisCase = RetrieveParameter(Lines(i), Thread)

                        FinLoop = False

                        Do
                            SCP = InStr(ThisCase, ";")
                            If SCP = 0 Then
                                SCP = Len(ThisCase) + 1
                                FinLoop = True
                            End If

                            ThisCondition = Trim(Left(ThisCase, SCP - 1))
                            If ThisCondition = CheckValue Then
                                CaseScript = GetAfterParameter(Lines(i))
                                CaseMatch = True
                                FinLoop = True
                            Else
                                ThisCase = Mid(ThisCase, SCP + 1)
                            End If
                        Loop Until FinLoop
                    End If

                    If CaseMatch Then
                        ExecuteScript(CaseScript, Thread)
                        Exit For
                    End If
                End If
            End If
        Next i

    End Sub

    Private Function ExecVerb(ByRef CommandString As String, ByRef Thread As ThreadData, Optional ByRef LibCommands As Boolean = False) As Boolean
        Dim gameblock As DefineBlock
        Dim FoundVerb As Boolean
        Dim VerbProperty As String = ""
        Dim Script As String = ""
        Dim VerbsList As String
        Dim ThisVerb As String = ""
        Dim SCP, ColonPos As Integer
        Dim ObjID, i As Integer
        Dim FoundItem, FoundAction As Boolean
        Dim VerbObject As String = ""
        Dim InventoryPlace, VerbTag As String
        Dim ThisScript As String = ""

        FoundVerb = False
        FoundAction = False

        If Not LibCommands Then
            VerbTag = "verb "
        Else
            VerbTag = "lib verb "
        End If

        gameblock = GetDefineBlock("game")
        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), VerbTag) Then
                VerbsList = RetrieveParameter(Lines(i), Thread)

                ' The property or action the verb uses is either after a colon,
                ' or it's the first (or only) verb on the line.

                ColonPos = InStr(VerbsList, ":")
                If ColonPos <> 0 Then
                    VerbProperty = LCase(Trim(Mid(VerbsList, ColonPos + 1)))
                    VerbsList = Trim(Left(VerbsList, ColonPos - 1))
                Else
                    SCP = InStr(VerbsList, ";")
                    If SCP = 0 Then
                        VerbProperty = LCase(VerbsList)
                    Else
                        VerbProperty = LCase(Trim(Left(VerbsList, SCP - 1)))
                    End If
                End If

                ' Now let's see if this matches:

                Do
                    SCP = InStr(VerbsList, ";")
                    If SCP = 0 Then
                        ThisVerb = LCase(VerbsList)
                    Else
                        ThisVerb = LCase(Trim(Left(VerbsList, SCP - 1)))
                    End If

                    If BeginsWith(CommandString, ThisVerb & " ") Then
                        FoundVerb = True
                        VerbObject = GetEverythingAfter(CommandString, ThisVerb & " ")
                        Script = Trim(Mid(Lines(i), InStr(Lines(i), ">") + 1))
                    End If

                    If SCP <> 0 Then
                        VerbsList = Trim(Mid(VerbsList, SCP + 1))
                    End If
                Loop Until SCP = 0 Or Trim(VerbsList) = "" Or FoundVerb

                If FoundVerb Then Exit For

            End If
        Next i

        If FoundVerb Then

            InventoryPlace = "inventory"

            ObjID = Disambiguate(VerbObject, InventoryPlace & ";" & CurrentRoom, Thread)

            If ObjID < 0 Then
                FoundItem = False
            Else
                FoundItem = True
            End If

            If FoundItem = False Then
                If ObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                BadCmdBefore = ThisVerb
            Else
                SetStringContents("quest.error.article", Objs(ObjID).Article, Thread)

                ' Now see if this object has the relevant action or property
                With Objs(ObjID)
                    For i = 1 To .NumberActions
                        If LCase(.Actions(i).ActionName) = VerbProperty Then
                            FoundAction = True
                            ThisScript = .Actions(i).Script
                            Exit For
                        End If
                    Next i
                End With

                If ThisScript <> "" Then
                    ' Avoid an RTE "this array is fixed or temporarily locked"
                    ExecuteScript(ThisScript, Thread, ObjID)
                End If

                With Objs(ObjID)
                    If Not FoundAction Then
                        ' Check properties for a message
                        For i = 1 To .NumberProperties
                            If LCase(.Properties(i).PropertyName) = VerbProperty Then
                                FoundAction = True
                                Print(.Properties(i).PropertyValue, Thread)
                                Exit For
                            End If
                        Next i
                    End If

                    If Not FoundAction Then
                        ' Execute the default script from the verb definition
                        ExecuteScript(Script, Thread)
                    End If
                End With
            End If
        End If

        ExecVerb = FoundVerb

    End Function

    Private Function ExpressionHandler(Expression As String) As ExpressionResult

        Dim ObsExp As String
        Dim i As Integer
        Dim Elements() As String
        Dim NumElements As Integer
        Dim Operators(0) As String
        Dim NumOperators As Integer
        Dim OpNum As Integer
        Dim Val2, Val1, Result As Double
        Dim BracketCount, OpenBracketPos, EndBracketPos As Integer
        Dim res As New ExpressionResult
        Dim NestedResult As ExpressionResult
        Dim NewElement As Boolean

        ' Find brackets, recursively call ExpressionHandler
        Do
            OpenBracketPos = InStr(Expression, "(")
            If OpenBracketPos <> 0 Then
                ' Find equivalent closing bracket
                BracketCount = 1
                EndBracketPos = 0
                For i = OpenBracketPos + 1 To Len(Expression)
                    If Mid(Expression, i, 1) = "(" Then
                        BracketCount = BracketCount + 1
                    ElseIf Mid(Expression, i, 1) = ")" Then
                        BracketCount = BracketCount - 1
                    End If

                    If BracketCount = 0 Then
                        EndBracketPos = i
                        Exit For
                    End If
                Next i

                If EndBracketPos <> 0 Then
                    NestedResult = ExpressionHandler(Mid(Expression, OpenBracketPos + 1, EndBracketPos - OpenBracketPos - 1))
                    If NestedResult.success <> EXPRESSION_OK Then
                        res.success = NestedResult.success
                        res.Message = NestedResult.Message
                        Return res
                    End If

                    Expression = Left(Expression, OpenBracketPos - 1) & " " & NestedResult.Result & " " & Mid(Expression, EndBracketPos + 1)

                Else
                    res.Message = "Missing closing bracket"
                    res.success = EXPRESSION_FAIL
                    Return res

                End If
            End If
        Loop Until OpenBracketPos = 0


        NumElements = 1
        ReDim Elements(1)
        NumOperators = 0

        ' Split expression into elements, e.g.:
        '       2 + 3 * 578.2 / 36
        '       E O E O EEEEE O EE      where E=Element, O=Operator

        ' Populate Elements() and Operators()

        ObsExp = ObscureNumericExps(Expression)

        For i = 1 To Len(Expression)
            Select Case Mid(ObsExp, i, 1)
                Case "+", "*", "/"
                    NewElement = True
                Case "-"
                    ' A minus often means subtraction, so it's a new element. But sometimes
                    ' it just denotes a negative number. In this case, the current element will
                    ' be empty.

                    If Trim(Elements(NumElements)) = "" Then
                        NewElement = False
                    Else
                        NewElement = True
                    End If
                Case Else
                    NewElement = False
            End Select

            If NewElement Then
                NumElements = NumElements + 1
                ReDim Preserve Elements(NumElements)

                NumOperators = NumOperators + 1
                ReDim Preserve Operators(NumOperators)
                Operators(NumOperators) = Mid(Expression, i, 1)
            Else
                Elements(NumElements) = Elements(NumElements) & Mid(Expression, i, 1)
            End If
        Next i

        ' Check Elements are numeric, and trim spaces
        For i = 1 To NumElements

            Elements(i) = Trim(Elements(i))

            If Not IsNumeric(Elements(i)) Then
                res.Message = "Syntax error evaluating expression - non-numeric element '" & Elements(i) & "'"
                res.success = EXPRESSION_FAIL
                Return res
            End If
        Next i

        Do
            ' Go through the Operators array to find next calculation to perform

            OpNum = 0

            For i = 1 To NumOperators
                If Operators(i) = "/" Or Operators(i) = "*" Then
                    OpNum = i
                    Exit For
                End If
            Next i

            If OpNum = 0 Then
                For i = 1 To NumOperators
                    If Operators(i) = "+" Or Operators(i) = "-" Then
                        OpNum = i
                        Exit For
                    End If
                Next i
            End If

            ' If OpNum is still 0, there are no calculations left to do.

            If OpNum <> 0 Then

                Val1 = CDbl(Elements(OpNum))
                Val2 = CDbl(Elements(OpNum + 1))

                Select Case Operators(OpNum)
                    Case "/"
                        If Val2 = 0 Then
                            res.Message = "Division by zero"
                            res.success = EXPRESSION_FAIL
                            Return res
                        End If
                        Result = Val1 / Val2
                    Case "*"
                        Result = Val1 * Val2
                    Case "+"
                        Result = Val1 + Val2
                    Case "-"
                        Result = Val1 - Val2
                End Select

                Elements(OpNum) = CStr(Result)

                ' Remove this operator, and Elements(OpNum+1) from the arrays
                For i = OpNum To NumOperators - 1
                    Operators(i) = Operators(i + 1)
                Next i
                For i = OpNum + 1 To NumElements - 1
                    Elements(i) = Elements(i + 1)
                Next i
                NumOperators = NumOperators - 1
                NumElements = NumElements - 1
                ReDim Preserve Operators(NumOperators)
                ReDim Preserve Elements(NumElements)

            End If
        Loop Until OpNum = 0 Or NumOperators = 0

        res.success = EXPRESSION_OK
        res.Result = Elements(1)
        Return res

    End Function

    Private Sub InitialiseQuest()
        ' Initialise variables
        LoadCASKeywords()
        SaveGameFile = ""
        OutputFileHandle = -1
        NumberStringVariables = 0
    End Sub

    Private Function ListContents(ByRef ObjID As Integer, ByRef Thread As ThreadData) As String

        ' Returns a formatted list of the contents of a container.
        ' If the list action causes a script to be run instead, ListContents
        ' returns "<script>"

        Dim Contents As String
        Dim i, NumContents As Integer
        Dim ContentsIDs(0) As Integer
        Dim ListString As String
        Dim DisplayList As Boolean

        If Not IsYes(GetObjectProperty("container", ObjID, True, False)) Then
            ListContents = ""
            Exit Function
        End If

        If Not IsYes(GetObjectProperty("opened", ObjID, True, False)) And Not IsYes(GetObjectProperty("transparent", ObjID, True, False)) And Not IsYes(GetObjectProperty("surface", ObjID, True, False)) Then
            ' Container is closed, so return "list closed" property if there is one.

            If DoAction(ObjID, "list closed", Thread, False) Then
                ListContents = "<script>"
            Else
                ListContents = GetObjectProperty("list closed", ObjID, False, False)
            End If
        Else

            ' populate contents string

            NumContents = 0

            For i = 1 To NumberObjs
                If Objs(i).Exists And Objs(i).Visible Then
                    If LCase(GetObjectProperty("parent", i, False, False)) = LCase(Objs(ObjID).ObjectName) Then
                        NumContents = NumContents + 1
                        ReDim Preserve ContentsIDs(NumContents)
                        ContentsIDs(NumContents) = i
                    End If
                End If
            Next i

            Contents = ""

            If NumContents > 0 Then

                ' Check if list property is set.

                If DoAction(ObjID, "list", Thread, False) Then
                    ListContents = "<script>"
                Else

                    If IsYes(GetObjectProperty("list", ObjID, True, False)) Then
                        ' Read header, if any
                        ListString = GetObjectProperty("list", ObjID, False, False)

                        DisplayList = True

                        If ListString <> "" Then
                            If Right(ListString, 1) = ":" Then
                                Contents = Left(ListString, Len(ListString) - 1) & " "
                            Else
                                ' If header doesn't end in a colon, then the header is the only text to print
                                Contents = ListString
                                DisplayList = False
                            End If
                        Else
                            Contents = UCase(Left(Objs(ObjID).Article, 1)) & Mid(Objs(ObjID).Article, 2) & " contains "
                        End If

                        If DisplayList Then
                            For i = 1 To NumContents
                                If i > 1 Then
                                    If i < NumContents Then
                                        Contents = Contents & ", "
                                    Else
                                        Contents = Contents & " and "
                                    End If
                                End If

                                With Objs(ContentsIDs(i))
                                    If .Prefix <> "" Then Contents = Contents & .Prefix
                                    If .ObjectAlias <> "" Then
                                        Contents = Contents & "|b" & .ObjectAlias & "|xb"
                                    Else
                                        Contents = Contents & "|b" & .ObjectName & "|xb"
                                    End If
                                    If .Suffix <> "" Then Contents = Contents & " " & .Suffix
                                End With
                            Next i
                        End If

                        ListContents = Contents & "."
                    Else
                        ' The "list" property is not set, so do not list contents.
                        ListContents = ""
                    End If
                End If
            Else
                ' Container is empty, so return "list empty" property if there is one.

                If DoAction(ObjID, "list empty", Thread, False) Then
                    ListContents = "<script>"
                Else
                    ListContents = GetObjectProperty("list empty", ObjID, False, False)
                End If

            End If
        End If

    End Function

    Private Function ObscureNumericExps(ByRef InputString As String) As String

        ' Obscures + or - next to E in Double-type variables with exponents
        '   e.g. 2.345E+20 becomes 2.345EX20
        ' This stops huge numbers breaking parsing of maths functions

        Dim EPos, CurPos As Integer
        Dim OutputString As String
        OutputString = InputString

        CurPos = 1
        Do
            EPos = InStr(CurPos, OutputString, "E")
            If EPos <> 0 Then
                OutputString = Left(OutputString, EPos) & "X" & Mid(OutputString, EPos + 2)
                CurPos = EPos + 2
            End If
        Loop Until EPos = 0

        ObscureNumericExps = OutputString
    End Function

    Private Sub ProcessListInfo(ByRef ListLine As String, ByRef ObjID As Integer)
        Dim ListInfo As New TextAction
        Dim PropName As String = ""

        If BeginsWith(ListLine, "list closed <") Then
            ListInfo.Type = TA_TEXT
            ListInfo.Data = RetrieveParameter(ListLine, NullThread)
            PropName = "list closed"
        ElseIf Trim(ListLine) = "list closed off" Then
            ' default for list closed is off anyway
            Exit Sub
        ElseIf BeginsWith(ListLine, "list closed") Then
            ListInfo.Type = TA_SCRIPT
            ListInfo.Data = GetEverythingAfter(ListLine, "list closed")
            PropName = "list closed"


        ElseIf BeginsWith(ListLine, "list empty <") Then
            ListInfo.Type = TA_TEXT
            ListInfo.Data = RetrieveParameter(ListLine, NullThread)
            PropName = "list empty"
        ElseIf Trim(ListLine) = "list empty off" Then
            ' default for list empty is off anyway
            Exit Sub
        ElseIf BeginsWith(ListLine, "list empty") Then
            ListInfo.Type = TA_SCRIPT
            ListInfo.Data = GetEverythingAfter(ListLine, "list empty")
            PropName = "list empty"


        ElseIf Trim(ListLine) = "list off" Then
            AddToObjectProperties("not list", ObjID, NullThread)
            Exit Sub
        ElseIf BeginsWith(ListLine, "list <") Then
            ListInfo.Type = TA_TEXT
            ListInfo.Data = RetrieveParameter(ListLine, NullThread)
            PropName = "list"
        ElseIf BeginsWith(ListLine, "list ") Then
            ListInfo.Type = TA_SCRIPT
            ListInfo.Data = GetEverythingAfter(ListLine, "list ")
            PropName = "list"
        End If

        If PropName <> "" Then
            If ListInfo.Type = TA_TEXT Then
                AddToObjectProperties(PropName & "=" & ListInfo.Data, ObjID, NullThread)
            Else
                AddToObjectActions("<" & PropName & "> " & ListInfo.Data, ObjID, NullThread)
            End If
        End If
    End Sub

    Private Function GetHTMLColour(ByRef Colour As String, ByRef DefaultColour As String) As String

        ' Converts a Quest foreground or background colour setting into an HTML colour

        Colour = LCase(Colour)

        If Colour = "" Or Colour = "0" Then Colour = DefaultColour

        Select Case Colour
            Case "white"
                GetHTMLColour = "FFFFFF"
            Case "black"
                GetHTMLColour = "000000"
            Case "blue"
                GetHTMLColour = "0000FF"
            Case "yellow"
                GetHTMLColour = "FFFF00"
            Case "red"
                GetHTMLColour = "FF0000"
            Case "green"
                GetHTMLColour = "00FF00"
            Case Else
                GetHTMLColour = Colour
        End Select

    End Function

    Private Sub DoPrint(ByRef OutputText As String)
        RaiseEvent PrintText(m_textFormatter.OutputHTML(OutputText))
    End Sub

    Private Sub DestroyExit(ByRef ExitData As String, ByRef Thread As ThreadData)
        Dim SCP As Integer
        Dim FoundID As Boolean
        Dim FromRoom As String = ""
        Dim ToRoom As String = ""
        Dim RoomID, i, ExitID As Integer
        Dim FoundExit As Boolean

        SCP = InStr(ExitData, ";")
        If SCP = 0 Then
            LogASLError("No exit name specified in 'destroy exit <" & ExitData & ">'")
            Exit Sub
        End If

        Dim oExit As RoomExit
        If GameASLVersion >= 410 Then
            oExit = FindExit(ExitData)
            If oExit Is Nothing Then
                LogASLError("Can't find exit in 'destroy exit <" & ExitData & ">'")
                Exit Sub
            End If

            oExit.Parent.RemoveExit(oExit)

        Else

            FromRoom = LCase(Trim(Left(ExitData, SCP - 1)))
            ToRoom = Trim(Mid(ExitData, SCP + 1))

            ' Find From Room:
            FoundID = False

            For i = 1 To NumberRooms
                If LCase(Rooms(i).RoomName) = FromRoom Then
                    FoundID = True
                    RoomID = i
                    i = NumberRooms
                End If
            Next i

            If Not FoundID Then
                LogASLError("No such room '" & FromRoom & "'")
                Exit Sub
            End If

            FoundExit = False
            With Rooms(RoomID)
                For i = 1 To .NumberPlaces
                    If .Places(i).PlaceName = ToRoom Then
                        ExitID = i
                        FoundExit = True
                        i = .NumberPlaces
                    End If
                Next i

                If FoundExit Then
                    For i = ExitID To .NumberPlaces - 1
                        .Places(i) = .Places(i + 1)
                    Next i
                    ReDim Preserve .Places(.NumberPlaces - 1)
                    .NumberPlaces = .NumberPlaces - 1
                End If
            End With
        End If

        ' Update quest.* vars and obj list
        ShowRoomInfo(CurrentRoom, Thread, True)
        UpdateObjectList(Thread)

        AddToChangeLog("room " & FromRoom, "destroy exit " & ToRoom)

    End Sub

    Private Sub DoClear()
        m_player.ClearScreen()
    End Sub

    Private Sub DoWait()

        m_player.DoWait()
        ChangeState(State.Waiting)

        SyncLock m_waitLock
            System.Threading.Monitor.Wait(m_waitLock)
        End SyncLock

    End Sub

    Private Sub ExecuteFlag(ByRef InputLine As String, ByRef Thread As ThreadData)
        Dim PropertyString As String = ""

        If BeginsWith(InputLine, "on ") Then
            PropertyString = RetrieveParameter(InputLine, Thread)
        ElseIf BeginsWith(InputLine, "off ") Then
            PropertyString = "not " & RetrieveParameter(InputLine, Thread)
        End If

        ' Game object always has ObjID 1
        AddToObjectProperties(PropertyString, 1, Thread)

    End Sub

    Private Function ExecuteIfFlag(ByRef flag As String) As Boolean

        ' Game ObjID is set to 1

        If GetObjectProperty(flag, 1, True) = "yes" Then
            ExecuteIfFlag = True
        Else
            ExecuteIfFlag = False
        End If

    End Function

    Private Sub ExecuteIncDec(ByRef InputLine As String, ByRef Thread As ThreadData)
        Dim SC As Integer
        Dim Param, Var As String
        Dim Change As Double
        Dim Value As Double
        Dim ArrayIndex As Integer
        Param = RetrieveParameter(InputLine, Thread)

        SC = InStr(Param, ";")
        If SC = 0 Then
            Change = 1
            Var = Param
        Else
            Change = Val(Mid(Param, SC + 1))
            Var = Trim(Left(Param, SC - 1))
        End If

        Value = GetNumericContents(Var, Thread, True)
        If Value <= -32766 Then Value = 0

        If BeginsWith(InputLine, "inc ") Then
            Value = Value + Change
        ElseIf BeginsWith(InputLine, "dec ") Then
            Value = Value - Change
        End If

        ArrayIndex = GetArrayIndex(Var, Thread)

        SetNumericVariableContents(Var, Value, Thread, ArrayIndex)

    End Sub

    Private Function ExtractFile(ByRef FileToExtract As String) As String
        Dim Length, StartPos, i As Integer
        Dim FoundRes As Boolean
        Dim FileData As String
        Dim Extracted As Boolean
        Dim ResID As Integer
        Dim sFileName As String

        If ResourceFile = "" Then Return ""

        ' Find file in catalog

        FoundRes = False

        For i = 1 To NumResources
            If LCase(FileToExtract) = LCase(Resources(i).ResourceName) Then
                FoundRes = True
                StartPos = Resources(i).ResourceStart + ResourceOffset
                Length = Resources(i).ResourceLength
                Extracted = Resources(i).Extracted
                ResID = i
                Exit For
            End If
        Next i

        If Not FoundRes Then
            LogASLError("Unable to extract '" & FileToExtract & "' - not present in resources.", LOGTYPE_WARNINGERROR)
            ExtractFile = CStr(False)
            Exit Function
        End If

        sFileName = System.IO.Path.Combine(m_tempFolder, FileToExtract)
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(sFileName))

        If Not Extracted Then
            ' Extract file from cached CAS data
            FileData = Mid(m_fileData, StartPos, Length)

            ' Write file to temp dir
            System.IO.File.WriteAllText(sFileName, FileData, System.Text.Encoding.GetEncoding(1252))

            Resources(ResID).Extracted = True
        End If

        ExtractFile = sFileName

    End Function

    Private Sub AddObjectAction(ByRef ObjID As Integer, ByRef Actions As ActionType, Optional ByRef NoUpdate As Boolean = False)

        ' Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        ' AddToGiveInfo again leading to a big loop

        Dim FoundExisting As Boolean
        Dim ActionNum, i As Integer
        FoundExisting = False

        With Objs(ObjID)

            For i = 1 To .NumberActions
                If .Actions(i).ActionName = Actions.ActionName Then
                    FoundExisting = True
                    ActionNum = i
                    i = .NumberActions
                End If
            Next i

            If Not FoundExisting Then
                .NumberActions = .NumberActions + 1
                ReDim Preserve .Actions(.NumberActions)
                ActionNum = .NumberActions
            End If

            .Actions(ActionNum) = Actions

            ObjectActionUpdate(ObjID, Actions.ActionName, Actions.Script, NoUpdate)

        End With

    End Sub

    Private Sub AddToChangeLog(ByRef AppliesTo As String, ByRef ChangeData As String)
        With GameChangeData
            .NumberChanges = .NumberChanges + 1
            ReDim Preserve .ChangeData(.NumberChanges)
            .ChangeData(.NumberChanges).AppliesTo = AppliesTo
            .ChangeData(.NumberChanges).Change = ChangeData
        End With
    End Sub

    Private Sub AddToOOChangeLog(ByRef lAppliesToType As ChangeLog.eAppliesToType, ByRef sAppliesTo As String, ByRef sElement As String, ByRef sChangeData As String)

        Dim oChangeLog As ChangeLog

        ' NOTE: We're only actually ever using the object changelog at the moment.
        ' Rooms only get logged for creating rooms and creating/destroying exits, so we don't
        ' need the refactored ChangeLog component for those - although at some point the
        ' QSG code should be refactored so we don't have two different types of change logs.

        Select Case lAppliesToType
            Case ChangeLog.eAppliesToType.atObject
                oChangeLog = m_oChangeLogObjects
            Case ChangeLog.eAppliesToType.atRoom
                oChangeLog = m_oChangeLogRooms
            Case Else
                Throw New ArgumentOutOfRangeException()
        End Select

        oChangeLog.AddItem(sAppliesTo, sElement, sChangeData)

        Exit Sub

    End Sub

    Private Sub AddToGiveInfo(ByRef ObjID As Integer, ByRef GiveData As String)
        Dim ObjectName As String
        Dim DataID As Integer
        Dim Found As Boolean
        Dim EP As Integer
        Dim CurGiveType, i As Integer
        Dim ThisAction As ActionType

        With Objs(ObjID)

            If BeginsWith(GiveData, "to ") Then
                GiveData = GetEverythingAfter(GiveData, "to ")
                If BeginsWith(GiveData, "anything ") Then
                    .GiveToAnything = GetEverythingAfter(GiveData, "anything ")

                    ThisAction.ActionName = "give to anything"
                    ThisAction.Script = .GiveToAnything
                    AddObjectAction(ObjID, ThisAction, True)
                    Exit Sub
                Else
                    CurGiveType = GIVE_TO_SOMETHING

                    ThisAction.ActionName = "give to "
                End If
            Else
                If BeginsWith(GiveData, "anything ") Then
                    .GiveAnything = GetEverythingAfter(GiveData, "anything ")

                    ThisAction.ActionName = "give anything"
                    ThisAction.Script = .GiveAnything
                    AddObjectAction(ObjID, ThisAction, True)
                    Exit Sub
                Else
                    CurGiveType = GIVE_SOMETHING_TO
                    ThisAction.ActionName = "give "
                End If
            End If

            If Left(Trim(GiveData), 1) = "<" Then
                ObjectName = RetrieveParameter(GiveData, NullThread)

                ThisAction.ActionName = ThisAction.ActionName & "'" & ObjectName & "'"

                Found = False
                For i = 1 To .NumberGiveData
                    If .GiveData(i).GiveType = CurGiveType And LCase(.GiveData(i).GiveObject) = LCase(ObjectName) Then
                        DataID = i
                        i = .NumberGiveData
                        Found = True
                    End If
                Next i

                If Not Found Then
                    .NumberGiveData = .NumberGiveData + 1
                    ReDim Preserve .GiveData(.NumberGiveData)
                    DataID = .NumberGiveData
                End If

                EP = InStr(GiveData, ">")
                .GiveData(DataID).GiveType = CurGiveType
                .GiveData(DataID).GiveObject = ObjectName
                .GiveData(DataID).GiveScript = Mid(GiveData, EP + 2)

                ThisAction.Script = .GiveData(DataID).GiveScript
                AddObjectAction(ObjID, ThisAction, True)

            End If

        End With

    End Sub

    Friend Sub AddToObjectActions(ByRef ActionInfo As String, ByRef ObjID As Integer, ByRef Thread As ThreadData)
        Dim FoundExisting As Boolean
        Dim ActionName As String
        Dim ActionScript As String
        Dim EP As Integer
        Dim ActionNum, i As Integer
        FoundExisting = False

        ActionName = LCase(RetrieveParameter(ActionInfo, Thread))
        EP = InStr(ActionInfo, ">")
        If EP = Len(ActionInfo) Then
            LogASLError("No script given for '" & ActionName & "' action data", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ActionScript = Trim(Mid(ActionInfo, EP + 1))

        With Objs(ObjID)

            For i = 1 To .NumberActions
                If .Actions(i).ActionName = ActionName Then
                    FoundExisting = True
                    ActionNum = i
                    i = .NumberActions
                End If
            Next i

            If Not FoundExisting Then
                .NumberActions = .NumberActions + 1
                ReDim Preserve .Actions(.NumberActions)
                ActionNum = .NumberActions
            End If

            .Actions(ActionNum).ActionName = ActionName
            .Actions(ActionNum).Script = ActionScript

            ObjectActionUpdate(ObjID, ActionName, ActionScript)

        End With
    End Sub

    Private Sub AddToObjectAltNames(AltNames As String, ByRef ObjID As Integer)
        Dim EndPos As Integer
        Dim CurName As String

        With Objs(ObjID)

            Do
                EndPos = InStr(AltNames, ";")
                If EndPos = 0 Then EndPos = Len(AltNames) + 1
                CurName = Trim(Left(AltNames, EndPos - 1))

                If CurName <> "" Then
                    .NumberAltNames = .NumberAltNames + 1
                    ReDim Preserve .AltNames(.NumberAltNames)
                    .AltNames(.NumberAltNames) = CurName
                End If

                AltNames = Mid(AltNames, EndPos + 1)
            Loop Until Trim(AltNames) = ""

        End With
    End Sub

    Friend Sub AddToObjectProperties(PropertyInfo As String, ByRef ObjID As Integer, ByRef Thread As ThreadData)
        Dim CurInfo As String
        Dim SCP, EP As Integer
        Dim CurName, CurValue As String
        Dim CurNum As Integer
        Dim Found As Boolean
        Dim FalseProperty As Boolean
        Dim i As Integer

        ' this block was from old changelog, we use "new" changelog now for properties and actions
        'If GameFullyLoaded Then
        '    AddToChangeLog "object " & Objs(ObjID).ItsName, "properties " & PropertyInfo
        'End If

        If Right(PropertyInfo, 1) <> ";" Then
            PropertyInfo = PropertyInfo & ";"
        End If

        Do
            SCP = InStr(PropertyInfo, ";")
            CurInfo = Left(PropertyInfo, SCP - 1)
            PropertyInfo = Trim(Mid(PropertyInfo, SCP + 1))

            If CurInfo = "" Then Exit Do

            EP = InStr(CurInfo, "=")
            If EP <> 0 Then
                CurName = Trim(Left(CurInfo, EP - 1))
                CurValue = Trim(Mid(CurInfo, EP + 1))
            Else
                CurName = CurInfo
                CurValue = ""
            End If

            FalseProperty = False
            If BeginsWith(CurName, "not ") And CurValue = "" Then
                FalseProperty = True
                CurName = GetEverythingAfter(CurName, "not ")
            End If

            With Objs(ObjID)
                Found = False
                For i = 1 To .NumberProperties
                    If LCase(.Properties(i).PropertyName) = LCase(CurName) Then
                        Found = True
                        CurNum = i
                        i = .NumberProperties
                    End If
                Next i

                If Not Found Then
                    .NumberProperties = .NumberProperties + 1
                    ReDim Preserve .Properties(.NumberProperties)
                    CurNum = .NumberProperties
                End If

                If FalseProperty Then
                    .Properties(CurNum).PropertyName = ""
                Else
                    .Properties(CurNum).PropertyName = CurName
                    .Properties(CurNum).PropertyValue = CurValue
                End If

                AddToOOChangeLog(ChangeLog.eAppliesToType.atObject, Objs(ObjID).ObjectName, CurName, "properties " & CurInfo)

                Select Case CurName
                    Case "alias"
                        If .IsRoom Then
                            Rooms(.CorresRoomID).RoomAlias = CurValue
                        Else
                            .ObjectAlias = CurValue
                        End If
                        If GameFullyLoaded Then
                            UpdateObjectList(Thread)
                            UpdateItems(Thread)
                        End If
                    Case "prefix"
                        If .IsRoom Then
                            Rooms(.CorresRoomID).Prefix = CurValue
                        Else
                            If CurValue <> "" Then
                                .Prefix = CurValue & " "
                            Else
                                .Prefix = ""
                            End If
                        End If
                    Case "indescription"
                        If .IsRoom Then Rooms(.CorresRoomID).InDescription = CurValue
                    Case "description"
                        If .IsRoom Then
                            Rooms(.CorresRoomID).Description.Data = CurValue
                            Rooms(.CorresRoomID).Description.Type = TA_TEXT
                        End If
                    Case "look"
                        If .IsRoom Then
                            Rooms(.CorresRoomID).Look = CurValue
                        End If
                    Case "suffix"
                        .Suffix = CurValue
                    Case "displaytype"
                        .DisplayType = CurValue
                        If GameFullyLoaded Then UpdateObjectList(Thread)
                    Case "gender"
                        .Gender = CurValue
                    Case "article"
                        .Article = CurValue
                    Case "detail"
                        .Detail = CurValue
                    Case "hidden"
                        If FalseProperty Then
                            .Exists = True
                        Else
                            .Exists = False
                        End If

                        If GameFullyLoaded Then UpdateObjectList(Thread)
                    Case "invisible"
                        If FalseProperty Then
                            .Visible = True
                        Else
                            .Visible = False
                        End If

                        If GameFullyLoaded Then UpdateObjectList(Thread)
                    Case "take"
                        If GameASLVersion >= 392 Then
                            If FalseProperty Then
                                .take.Type = TA_NOTHING
                            Else
                                If CurValue = "" Then
                                    .take.Type = TA_DEFAULT
                                Else
                                    .take.Type = TA_TEXT
                                    .take.Data = CurValue
                                End If
                            End If
                        End If
                End Select
            End With
        Loop Until Len(Trim(PropertyInfo)) = 0

    End Sub

    Private Sub AddToUseInfo(ByRef ObjID As Integer, ByRef UseData As String)
        Dim ObjectName As String
        Dim DataID As Integer
        Dim Found As Boolean
        Dim EP As Integer
        Dim CurUseType, i As Integer

        With Objs(ObjID)

            If BeginsWith(UseData, "on ") Then
                UseData = GetEverythingAfter(UseData, "on ")
                If BeginsWith(UseData, "anything ") Then
                    .UseOnAnything = GetEverythingAfter(UseData, "anything ")
                    Exit Sub
                Else
                    CurUseType = USE_ON_SOMETHING
                End If
            Else
                If BeginsWith(UseData, "anything ") Then
                    .UseAnything = GetEverythingAfter(UseData, "anything ")
                    Exit Sub
                Else
                    CurUseType = USE_SOMETHING_ON
                End If
            End If

            If Left(Trim(UseData), 1) = "<" Then
                ObjectName = RetrieveParameter(UseData, NullThread)
                Found = False
                For i = 1 To .NumberUseData
                    If .UseData(i).UseType = CurUseType And LCase(.UseData(i).UseObject) = LCase(ObjectName) Then
                        DataID = i
                        i = .NumberUseData
                        Found = True
                    End If
                Next i

                If Not Found Then
                    .NumberUseData = .NumberUseData + 1
                    ReDim Preserve .UseData(.NumberUseData)
                    DataID = .NumberUseData
                End If

                EP = InStr(UseData, ">")
                .UseData(DataID).UseType = CurUseType
                .UseData(DataID).UseObject = ObjectName
                .UseData(DataID).UseScript = Mid(UseData, EP + 2)
            Else
                .use = Trim(UseData)
            End If

        End With

    End Sub

    Private Function CapFirst(ByRef InputString As String) As String
        CapFirst = UCase(Left(InputString, 1)) & Mid(InputString, 2)
    End Function

    Private Function ConvertVarsIn(ByRef InputString As String, ByRef Thread As ThreadData) As String
        ConvertVarsIn = RetrieveParameter("<" & InputString & ">", Thread)
    End Function

    Private Function DisambObjHere(ByRef Thread As ThreadData, ByRef ObjID As Integer, FirstPlace As String, Optional TwoPlaces As Boolean = False, Optional SecondPlace As String = "", Optional bExit As Boolean = False) As Boolean

        Dim OnlySeen, ObjIsSeen As Boolean
        Dim RoomObjID As Integer
        Dim InventoryPlace As String
        OnlySeen = False

        If FirstPlace = "game" Then
            FirstPlace = ""
            If SecondPlace = "seen" Then
                TwoPlaces = False
                SecondPlace = ""
                OnlySeen = True
                RoomObjID = Rooms(GetRoomID(Objs(ObjID).ContainerRoom, Thread)).ObjID

                InventoryPlace = "inventory"

                If Objs(ObjID).ContainerRoom = InventoryPlace Then
                    ObjIsSeen = True
                Else
                    If IsYes(GetObjectProperty("visited", RoomObjID, True, False)) Then
                        ObjIsSeen = True
                    Else
                        If IsYes(GetObjectProperty("seen", ObjID, True, False)) Then
                            ObjIsSeen = True
                        End If
                    End If
                End If

            End If
        End If

        If ((TwoPlaces = False And (LCase(Objs(ObjID).ContainerRoom) = LCase(FirstPlace) Or FirstPlace = "")) Or (TwoPlaces = True And (LCase(Objs(ObjID).ContainerRoom) = LCase(FirstPlace) Or LCase(Objs(ObjID).ContainerRoom) = LCase(SecondPlace)))) And Objs(ObjID).Exists = True And Objs(ObjID).IsExit = bExit Then
            If Not OnlySeen Then
                DisambObjHere = True
            Else
                If ObjIsSeen Then DisambObjHere = True Else DisambObjHere = False
            End If
        Else
            DisambObjHere = False
        End If
    End Function

    Private Sub ExecClone(ByRef CloneString As String, ByRef Thread As ThreadData)
        Dim SC2, SC, ObjID As Integer
        Dim NewObjName, ObjToClone, CloneTo As String

        SC = InStr(CloneString, ";")
        If SC = 0 Then
            LogASLError("No new object name specified in 'clone <" & CloneString & ">", LOGTYPE_WARNINGERROR)
            Exit Sub
        Else
            ObjToClone = Trim(Left(CloneString, SC - 1))
            ObjID = GetObjectIDNoAlias(ObjToClone)

            SC2 = InStr(SC + 1, CloneString, ";")
            If SC2 = 0 Then
                CloneTo = Objs(ObjID).ContainerRoom
                NewObjName = Trim(Mid(CloneString, SC + 1))
            Else
                CloneTo = Trim(Mid(CloneString, SC2 + 1))
                NewObjName = Trim(Mid(CloneString, SC + 1, (SC2 - SC) - 1))
            End If
        End If

        NumberObjs = NumberObjs + 1
        ReDim Preserve Objs(NumberObjs)
        Objs(NumberObjs) = Objs(ObjID)
        Objs(NumberObjs).ContainerRoom = CloneTo
        Objs(NumberObjs).ObjectName = NewObjName

        If Objs(ObjID).IsRoom Then
            ' This is a room so create the corresponding room as well

            NumberRooms = NumberRooms + 1
            ReDim Preserve Rooms(NumberRooms)
            Rooms(NumberRooms) = Rooms(Objs(ObjID).CorresRoomID)
            Rooms(NumberRooms).RoomName = NewObjName
            Rooms(NumberRooms).ObjID = NumberObjs

            Objs(NumberObjs).CorresRoom = NewObjName
            Objs(NumberObjs).CorresRoomID = NumberRooms

            AddToChangeLog("room " & NewObjName, "create")
        Else
            AddToChangeLog("object " & NewObjName, "create " & Objs(NumberObjs).ContainerRoom)
        End If

        UpdateObjectList(Thread)


    End Sub

    Private Sub ExecOops(ByRef Correction As String, ByRef Thread As ThreadData)

        If BadCmdBefore <> "" Then
            If BadCmdAfter = "" Then
                ExecCommand(BadCmdBefore & " " & Correction, Thread, False)
            Else
                ExecCommand(BadCmdBefore & " " & Correction & " " & BadCmdAfter, Thread, False)
            End If
        End If

    End Sub

    Private Sub ExecType(ByRef TypeData As String, ByRef Thread As ThreadData)
        Dim SCP As Integer
        Dim ObjName, TypeName As String
        Dim ObjID As Integer
        Dim Found As Boolean
        Dim PropertyData As PropertiesActions
        Dim i As Integer
        SCP = InStr(TypeData, ";")

        If SCP = 0 Then
            LogASLError("No type name given in 'type <" & TypeData & ">'")
            Exit Sub
        End If

        ObjName = Trim(Left(TypeData, SCP - 1))
        TypeName = Trim(Mid(TypeData, SCP + 1))

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                Found = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not Found Then
            LogASLError("No such object in 'type <" & TypeData & ">'")
            Exit Sub
        End If

        With Objs(ObjID)
            .NumberTypesIncluded = .NumberTypesIncluded + 1
            ReDim Preserve .TypesIncluded(.NumberTypesIncluded)
            .TypesIncluded(.NumberTypesIncluded) = TypeName

            PropertyData = GetPropertiesInType(TypeName)
            AddToObjectProperties(PropertyData.Properties, ObjID, Thread)
            For i = 1 To PropertyData.NumberActions
                AddObjectAction(ObjID, PropertyData.Actions(i))
            Next i

            ' New as of Quest 4.0. Fixes bug that "if type" would fail for any
            ' parent types included by the "type" command.
            For i = 1 To PropertyData.NumberTypesIncluded
                .NumberTypesIncluded = .NumberTypesIncluded + 1
                ReDim Preserve .TypesIncluded(.NumberTypesIncluded)
                .TypesIncluded(.NumberTypesIncluded) = PropertyData.TypesIncluded(i)
            Next i
        End With
    End Sub

    Private Function ExecuteIfAction(ByRef ActionData As String) As Boolean
        Dim SCP, ObjID As Integer
        Dim ObjName As String
        Dim ActionName As String
        Dim FoundObj As Boolean
        Dim Result As Boolean
        Dim i As Integer

        SCP = InStr(ActionData, ";")

        If SCP = 0 Then
            LogASLError("No action name given in condition 'action <" & ActionData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfAction = False
            Exit Function
        End If

        ObjName = Trim(Left(ActionData, SCP - 1))
        ActionName = Trim(Mid(ActionData, SCP + 1))

        FoundObj = False

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                FoundObj = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not FoundObj Then
            LogASLError("No such object '" & ObjName & "' in condition 'action <" & ActionData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfAction = False
            Exit Function
        End If

        Result = False

        With Objs(ObjID)
            For i = 1 To .NumberActions
                If LCase(.Actions(i).ActionName) = LCase(ActionName) Then
                    i = .NumberActions
                    Result = True
                End If
            Next i
        End With

        ExecuteIfAction = Result

    End Function

    Private Function ExecuteIfType(ByRef TypeData As String) As Boolean
        Dim SCP, ObjID As Integer
        Dim ObjName As String
        Dim TypeName As String
        Dim FoundObj As Boolean
        Dim Result As Boolean
        Dim i As Integer

        SCP = InStr(TypeData, ";")

        If SCP = 0 Then
            LogASLError("No type name given in condition 'type <" & TypeData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfType = False
            Exit Function
        End If

        ObjName = Trim(Left(TypeData, SCP - 1))
        TypeName = Trim(Mid(TypeData, SCP + 1))

        FoundObj = False

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                FoundObj = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not FoundObj Then
            LogASLError("No such object '" & ObjName & "' in condition 'type <" & TypeData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfType = False
            Exit Function
        End If

        Result = False

        With Objs(ObjID)
            For i = 1 To .NumberTypesIncluded
                If LCase(.TypesIncluded(i)) = LCase(TypeName) Then
                    i = .NumberTypesIncluded
                    Result = True
                End If
            Next i
        End With

        ExecuteIfType = Result

    End Function

    Private Function GetArrayIndex(ByRef sVarName As String, ByRef Thread As ThreadData) As Integer
        Dim BeginPos, EndPos As Integer
        Dim ArrayIndexData As String
        Dim ArrayIndex As Integer

        If InStr(sVarName, "[") <> 0 And InStr(sVarName, "]") <> 0 Then
            BeginPos = InStr(sVarName, "[")
            EndPos = InStr(sVarName, "]")
            ArrayIndexData = Mid(sVarName, BeginPos + 1, (EndPos - BeginPos) - 1)
            If IsNumeric(ArrayIndexData) Then
                ArrayIndex = CInt(ArrayIndexData)
            Else
                ArrayIndex = CInt(GetNumericContents(ArrayIndexData, Thread))
            End If
            sVarName = Left(sVarName, BeginPos - 1)
        End If

        GetArrayIndex = ArrayIndex

    End Function

    Private Function CurrentRoomOf(ByRef ThingName As String, ByRef ThingType As Integer) As String
        ' Returns current room of global character/object
        ' specified

        Dim i As Integer

        If ThingType = QUEST_CHARACTER Then
            For i = 1 To NumberChars
                If LCase(Chars(i).ObjectName) = LCase(ThingName) And Chars(i).IsGlobal = True Then
                    Return Chars(i).ContainerRoom
                End If
            Next i
        ElseIf ThingType = QUEST_OBJECT Then
            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(ThingName) And Objs(i).IsGlobal = True Then
                    Return Objs(i).ContainerRoom
                End If
            Next i
        End If

        Throw New ArgumentOutOfRangeException()

    End Function

    Friend Function Disambiguate(ObjectName As String, ByRef ContainedIn As String, ByRef Thread As ThreadData, Optional ByRef bExit As Boolean = False) As Integer
        ' Returns object ID being referred to by player.
        ' Returns -1 if object doesn't exist, calling function
        '   then expected to print relevant error.
        ' Returns -2 if "it" meaningless, prints own error.
        ' If it returns an object ID, it also sets quest.lastobject to the name
        ' of the object referred to.
        ' If Thread.AllowRealNamesInCommand is True, will allow an object's real
        ' name to be used even when the object has an alias - this is used when
        ' Disambiguate has been called after an "exec" command to prevent the
        ' player having to choose an object from the disambiguation menu twice

        Dim OrigBeginsWithThe As Boolean
        OrigBeginsWithThe = False
        Dim NumberCorresIDs As Integer
        NumberCorresIDs = 0
        Dim IDNumbers(0) As Integer
        Dim FoundItem As Boolean
        FoundItem = False
        Dim FirstPlace As String
        Dim SecondPlace As String = ""
        Dim TwoPlaces As Boolean
        Dim DescriptionText() As String
        Dim ValidNames() As String
        Dim NumValidNames As Integer
        Dim i, j As Integer
        Dim ThisName As String

        ObjectName = Trim(ObjectName)

        SetStringContents("quest.lastobject", "", Thread)

        Dim SCP As Integer
        If InStr(ContainedIn, ";") <> 0 Then
            SCP = InStr(ContainedIn, ";")
            TwoPlaces = True
            FirstPlace = Trim(Left(ContainedIn, SCP - 1))
            SecondPlace = Trim(Mid(ContainedIn, SCP + 1))
        Else
            TwoPlaces = False
            FirstPlace = ContainedIn
        End If

        If Thread.AllowRealNamesInCommand Then
            For i = 1 To NumberObjs
                If DisambObjHere(Thread, i, FirstPlace, TwoPlaces, SecondPlace) Then
                    If LCase(Objs(i).ObjectName) = LCase(ObjectName) Then
                        FoundItem = True
                        Disambiguate = i
                        SetStringContents("quest.lastobject", Objs(i).ObjectName, Thread)
                        Exit For
                    End If
                End If
            Next i

            If FoundItem Then Exit Function
        End If

        ' If player uses "it", "them" etc. as name:
        If ObjectName = "it" Or ObjectName = "them" Or ObjectName = "this" Or ObjectName = "those" Or ObjectName = "these" Or ObjectName = "that" Then
            SetStringContents("quest.error.pronoun", ObjectName, Thread)
            If LastIt <> 0 And LastItMode = IT_INANIMATE And DisambObjHere(Thread, LastIt, FirstPlace, TwoPlaces, SecondPlace) Then
                FoundItem = True
                Disambiguate = LastIt
                SetStringContents("quest.lastobject", Objs(LastIt).ObjectName, Thread)
                Exit Function
            Else
                PlayerErrorMessage(ERROR_BADPRONOUN, Thread)
                Disambiguate = -2
                Exit Function
            End If
        ElseIf ObjectName = "him" Then
            SetStringContents("quest.error.pronoun", ObjectName, Thread)
            If LastIt <> 0 And LastItMode = IT_MALE And DisambObjHere(Thread, LastIt, FirstPlace, TwoPlaces, SecondPlace) Then
                FoundItem = True
                Disambiguate = LastIt
                SetStringContents("quest.lastobject", Objs(LastIt).ObjectName, Thread)
                Exit Function
            Else
                PlayerErrorMessage(ERROR_BADPRONOUN, Thread)
                Disambiguate = -2
                Exit Function
            End If
        ElseIf ObjectName = "her" Then
            SetStringContents("quest.error.pronoun", ObjectName, Thread)
            If LastIt <> 0 And LastItMode = IT_FEMALE And DisambObjHere(Thread, LastIt, FirstPlace, TwoPlaces, SecondPlace) Then
                FoundItem = True
                Disambiguate = LastIt
                SetStringContents("quest.lastobject", Objs(LastIt).ObjectName, Thread)
                Exit Function
            Else
                PlayerErrorMessage(ERROR_BADPRONOUN, Thread)
                Disambiguate = -2
                Exit Function
            End If
        End If

        ThisTurnIt = 0

        If BeginsWith(ObjectName, "the ") Then
            ObjectName = GetEverythingAfter(ObjectName, "the ")
        End If

        For i = 1 To NumberObjs

            If DisambObjHere(Thread, i, FirstPlace, TwoPlaces, SecondPlace, bExit) Then

                NumValidNames = Objs(i).NumberAltNames + 1
                ReDim ValidNames(NumValidNames)
                ValidNames(1) = Objs(i).ObjectAlias
                For j = 1 To Objs(i).NumberAltNames
                    ValidNames(j + 1) = Objs(i).AltNames(j)
                Next j

                For j = 1 To NumValidNames
                    If ((LCase(ValidNames(j)) = LCase(ObjectName)) Or ("the " & LCase(ObjectName) = LCase(ValidNames(j)))) Then
                        NumberCorresIDs = NumberCorresIDs + 1
                        ReDim Preserve IDNumbers(NumberCorresIDs)
                        IDNumbers(NumberCorresIDs) = i

                        j = NumValidNames
                    End If
                Next j
            End If
        Next i

        If GameASLVersion >= 391 And NumberCorresIDs = 0 And goptAbbreviations And Len(ObjectName) > 0 Then
            ' Check for abbreviated object names

            For i = 1 To NumberObjs
                If DisambObjHere(Thread, i, FirstPlace, TwoPlaces, SecondPlace, bExit) Then
                    If Objs(i).ObjectAlias <> "" Then ThisName = LCase(Objs(i).ObjectAlias) Else ThisName = LCase(Objs(i).ObjectName)
                    If GameASLVersion >= 410 Then
                        If Objs(i).Prefix <> "" Then ThisName = Trim(LCase(Objs(i).Prefix)) & " " & ThisName
                        If Objs(i).Suffix <> "" Then ThisName = ThisName & " " & Trim(LCase(Objs(i).Suffix))
                    End If
                    If InStr(" " & ThisName, " " & LCase(ObjectName)) <> 0 Then
                        NumberCorresIDs = NumberCorresIDs + 1
                        ReDim Preserve IDNumbers(NumberCorresIDs)
                        IDNumbers(NumberCorresIDs) = i
                    End If
                End If
            Next i
        End If

        Dim Question As String
        If NumberCorresIDs = 1 Then
            FoundItem = True
            SetStringContents("quest.lastobject", Objs(IDNumbers(1)).ObjectName, Thread)
            Disambiguate = IDNumbers(1)
            ThisTurnIt = IDNumbers(1)

            Select Case Objs(IDNumbers(1)).Article
                Case "him"
                    ThisTurnItMode = IT_MALE
                Case "her"
                    ThisTurnItMode = IT_FEMALE
                Case Else
                    ThisTurnItMode = IT_INANIMATE
            End Select

            Exit Function
        ElseIf NumberCorresIDs > 1 Then
            ReDim DescriptionText(NumberCorresIDs)
            FoundItem = True

            Question = "Please select which " & ObjectName & " you mean:"
            Print("- |i" & Question & "|xi", Thread)

            Dim menuItems As New Dictionary(Of String, String)

            For i = 1 To NumberCorresIDs
                DescriptionText(i) = Objs(IDNumbers(i)).Detail
                If DescriptionText(i) = "" Then
                    If Objs(IDNumbers(i)).Prefix = "" Then
                        DescriptionText(i) = Objs(IDNumbers(i)).ObjectAlias
                    Else
                        DescriptionText(i) = Objs(IDNumbers(i)).Prefix & Objs(IDNumbers(i)).ObjectAlias
                    End If
                End If

                menuItems.Add(CStr(i), DescriptionText(i))

            Next i

            Dim mnu As New MenuData(Question, menuItems, False)
            Dim response As String = ShowMenu(mnu)

            ChoiceNumber = CInt(response)

            SetStringContents("quest.lastobject", Objs(IDNumbers(ChoiceNumber)).ObjectName, Thread)

            Disambiguate = IDNumbers(ChoiceNumber)
            ThisTurnIt = IDNumbers(ChoiceNumber)

            Select Case Objs(IDNumbers(ChoiceNumber)).Article
                Case "him"
                    ThisTurnItMode = IT_MALE
                Case "her"
                    ThisTurnItMode = IT_FEMALE
                Case Else
                    ThisTurnItMode = IT_INANIMATE
            End Select

            Print("- " & DescriptionText(ChoiceNumber) & "|n", Thread)

        End If

        If Not FoundItem Then
            Disambiguate = -1
            ThisTurnIt = LastIt
            SetStringContents("quest.error.object", ObjectName, Thread)
        End If

    End Function

    Private Function DisplayStatusVariableInfo(ByRef VarNum As Integer, ByRef VariableType As Integer, ByRef Thread As ThreadData) As String
        Dim DisplayData As String = ""
        Dim ExcPos As Integer
        Dim FirstStar, SecondStar As Integer
        Dim BeforeStar, AfterStar As String
        Dim BetweenStar As String
        Dim ArrayIndex As Integer

        ArrayIndex = 0

        If VariableType = VARTYPE_STRING Then
            DisplayData = ConvertVarsIn(StringVariable(VarNum).DisplayString, Thread)
            ExcPos = InStr(DisplayData, "!")

            If ExcPos <> 0 Then
                DisplayData = Left(DisplayData, ExcPos - 1) & StringVariable(VarNum).VariableContents(ArrayIndex) & Mid(DisplayData, ExcPos + 1)
            End If
        ElseIf VariableType = VARTYPE_NUMERIC Then
            If NumericVariable(VarNum).NoZeroDisplay And Val(NumericVariable(VarNum).VariableContents(ArrayIndex)) = 0 Then
                Return ""
            End If
            DisplayData = ConvertVarsIn(NumericVariable(VarNum).DisplayString, Thread)
            ExcPos = InStr(DisplayData, "!")

            If ExcPos <> 0 Then
                DisplayData = Left(DisplayData, ExcPos - 1) & NumericVariable(VarNum).VariableContents(ArrayIndex) & Mid(DisplayData, ExcPos + 1)
            End If

            If InStr(DisplayData, "*") > 0 Then
                FirstStar = InStr(DisplayData, "*")
                SecondStar = InStr(FirstStar + 1, DisplayData, "*")
                BeforeStar = Left(DisplayData, FirstStar - 1)
                AfterStar = Mid(DisplayData, SecondStar + 1)
                BetweenStar = Mid(DisplayData, FirstStar + 1, (SecondStar - FirstStar) - 1)

                If CDbl(NumericVariable(VarNum).VariableContents(ArrayIndex)) <> 1 Then
                    DisplayData = BeforeStar & BetweenStar & AfterStar
                Else
                    DisplayData = BeforeStar & AfterStar
                End If
            End If
        End If

        DisplayStatusVariableInfo = DisplayData

    End Function

    Friend Function DoAction(ByRef ObjID As Integer, ByRef ActionName As String, ByRef Thread As ThreadData, Optional ByRef LogError As Boolean = True) As Boolean

        Dim FoundAction As Boolean
        Dim ActionScript As String = ""
        Dim i As Integer

        With Objs(ObjID)

            For i = 1 To .NumberActions
                If .Actions(i).ActionName = LCase(ActionName) Then
                    FoundAction = True
                    ActionScript = .Actions(i).Script
                    Exit For
                End If
            Next i

            If Not FoundAction Then
                If LogError Then LogASLError("No such action '" & ActionName & "' defined for object '" & .ObjectName & "'")
                DoAction = False
                Exit Function
            End If

        End With

        Dim NewThread As ThreadData
        NewThread = Thread
        NewThread.CallingObjectID = ObjID

        ExecuteScript(ActionScript, NewThread, ObjID)

        DoAction = True

    End Function

    Public Function HasAction(ByRef ObjID As Integer, ByRef ActionName As String) As Boolean
        Dim i As Integer

        With Objs(ObjID)
            For i = 1 To .NumberActions
                If .Actions(i).ActionName = LCase(ActionName) Then
                    HasAction = True
                    Exit Function
                End If
            Next i
        End With
    End Function

    Private Sub ExecForEach(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        Dim InLocation, ScriptToExecute As String
        Dim i, BracketPos As Integer
        Dim bExit As Boolean
        Dim bRoom As Boolean

        If BeginsWith(ScriptLine, "object ") Then
            ScriptLine = GetEverythingAfter(ScriptLine, "object ")
            If Not BeginsWith(ScriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each object " & ReportErrorLine(ScriptLine) & "'", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If
        ElseIf BeginsWith(ScriptLine, "exit ") Then
            ScriptLine = GetEverythingAfter(ScriptLine, "exit ")
            If Not BeginsWith(ScriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each exit " & ReportErrorLine(ScriptLine) & "'", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If
            bExit = True
        ElseIf BeginsWith(ScriptLine, "room ") Then
            ScriptLine = GetEverythingAfter(ScriptLine, "room ")
            If Not BeginsWith(ScriptLine, "in ") Then
                LogASLError("Expected 'in' in 'for each room " & ReportErrorLine(ScriptLine) & "'", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If
            bRoom = True
        Else
            LogASLError("Unknown type in 'for each " & ReportErrorLine(ScriptLine) & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ScriptLine = GetEverythingAfter(ScriptLine, "in ")

        If BeginsWith(ScriptLine, "game ") Then
            InLocation = ""
            ScriptToExecute = GetEverythingAfter(ScriptLine, "game ")
        Else
            InLocation = LCase(RetrieveParameter(ScriptLine, Thread))
            BracketPos = InStr(ScriptLine, ">")
            ScriptToExecute = Trim(Mid(ScriptLine, BracketPos + 1))
        End If

        For i = 1 To NumberObjs
            If InLocation = "" Or LCase(Objs(i).ContainerRoom) = InLocation Then
                If Objs(i).IsRoom = bRoom And Objs(i).IsExit = bExit Then
                    SetStringContents("quest.thing", Objs(i).ObjectName, Thread)
                    ExecuteScript(ScriptToExecute, Thread)
                End If
            End If
        Next i
    End Sub

    Private Sub ExecuteAction(ByRef ActionData As String, ByRef Thread As ThreadData)
        Dim FoundExisting As Boolean
        Dim ActionName As String
        Dim ActionScript As String
        Dim EP, SCP As Integer
        Dim ActionNum As Integer
        Dim ActionParam As String
        Dim ObjName As String
        Dim FoundObject As Boolean
        Dim ObjID, i As Integer

        FoundExisting = False
        FoundObject = False

        ActionParam = RetrieveParameter(ActionData, Thread)
        SCP = InStr(ActionParam, ";")
        If SCP = 0 Then
            LogASLError("No action name specified in 'action " & ActionData & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ObjName = Trim(Left(ActionParam, SCP - 1))
        ActionName = Trim(Mid(ActionParam, SCP + 1))

        EP = InStr(ActionData, ">")
        If EP = Len(Trim(ActionData)) Then
            ActionScript = ""
        Else
            ActionScript = Trim(Mid(ActionData, EP + 1))
        End If

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                FoundObject = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not FoundObject Then
            LogASLError("No such object '" & ObjName & "' in 'action " & ActionData & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        With Objs(ObjID)

            For i = 1 To .NumberActions
                If .Actions(i).ActionName = ActionName Then
                    FoundExisting = True
                    ActionNum = i
                    i = .NumberActions
                End If
            Next i

            If Not FoundExisting Then
                .NumberActions = .NumberActions + 1
                ReDim Preserve .Actions(.NumberActions)
                ActionNum = .NumberActions
            End If

            .Actions(ActionNum).ActionName = ActionName
            .Actions(ActionNum).Script = ActionScript

            ObjectActionUpdate(ObjID, ActionName, ActionScript)

        End With

    End Sub

    Private Function ExecuteCondition(Condition As String, ByRef Thread As ThreadData) As Boolean
        Dim bThisResult, bThisNot As Boolean

        If BeginsWith(Condition, "not ") Then
            bThisNot = True
            Condition = GetEverythingAfter(Condition, "not ")
        Else
            bThisNot = False
        End If

        If BeginsWith(Condition, "got ") Then
            bThisResult = ExecuteIfGot(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "has ") Then
            bThisResult = ExecuteIfHas(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "ask ") Then
            bThisResult = ExecuteIfAsk(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "is ") Then
            bThisResult = ExecuteIfIs(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "here ") Then
            bThisResult = ExecuteIfHere(RetrieveParameter(Condition, Thread), Thread)
        ElseIf BeginsWith(Condition, "exists ") Then
            bThisResult = ExecuteIfExists(RetrieveParameter(Condition, Thread), False)
        ElseIf BeginsWith(Condition, "real ") Then
            bThisResult = ExecuteIfExists(RetrieveParameter(Condition, Thread), True)
        ElseIf BeginsWith(Condition, "property ") Then
            bThisResult = ExecuteIfProperty(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "action ") Then
            bThisResult = ExecuteIfAction(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "type ") Then
            bThisResult = ExecuteIfType(RetrieveParameter(Condition, Thread))
        ElseIf BeginsWith(Condition, "flag ") Then
            bThisResult = ExecuteIfFlag(RetrieveParameter(Condition, Thread))
        End If

        If bThisNot Then bThisResult = Not bThisResult

        ExecuteCondition = bThisResult
    End Function

    Private Function ExecuteConditions(ByRef ConditionList As String, ByRef Thread As ThreadData) As Boolean
        Dim Conditions() As String
        Dim iNumConditions As Integer
        Dim Operations() As String
        Dim bThisResult As Boolean
        Dim bConditionResult As Boolean
        Dim iCurLinePos As Integer
        Dim bFinalCondition As Boolean
        Dim ObscuredConditionList, NextCondition As String
        Dim NextConditionPos As Integer
        Dim ThisCondition As String
        Dim i As Integer

        ObscuredConditionList = ObliterateParameters(ConditionList)

        iCurLinePos = 1 : bFinalCondition = False
        iNumConditions = 0

        Do
            iNumConditions = iNumConditions + 1
            ReDim Preserve Conditions(iNumConditions)
            ReDim Preserve Operations(iNumConditions)

            NextCondition = "AND"
            NextConditionPos = InStr(iCurLinePos, ObscuredConditionList, "and ")
            If NextConditionPos = 0 Then
                NextConditionPos = InStr(iCurLinePos, ObscuredConditionList, "or ")
                NextCondition = "OR"
            End If

            If NextConditionPos = 0 Then
                NextConditionPos = Len(ObscuredConditionList) + 2
                bFinalCondition = True
                NextCondition = "FINAL"
            End If

            ThisCondition = Trim(Mid(ConditionList, iCurLinePos, NextConditionPos - iCurLinePos - 1))

            Conditions(iNumConditions) = ThisCondition
            Operations(iNumConditions) = NextCondition

            ' next condition starts from space after and/or
            iCurLinePos = InStr(NextConditionPos, ObscuredConditionList, " ")
        Loop Until bFinalCondition

        Operations(0) = "AND"
        bConditionResult = True

        For i = 1 To iNumConditions
            bThisResult = ExecuteCondition(Conditions(i), Thread)

            If Operations(i - 1) = "AND" Then
                bConditionResult = bThisResult And bConditionResult
            ElseIf Operations(i - 1) = "OR" Then
                bConditionResult = bThisResult Or bConditionResult
            End If
        Next i

        ExecuteConditions = bConditionResult

    End Function

    Private Sub ExecuteCreate(ByRef CreateData As String, ByRef Thread As ThreadData)
        Dim NewName As String
        Dim SCP As Integer
        Dim ParamData As String
        Dim j As Integer

        Dim ContainerRoom As String
        If BeginsWith(CreateData, "room ") Then
            NewName = RetrieveParameter(CreateData, Thread)
            NumberRooms = NumberRooms + 1
            ReDim Preserve Rooms(NumberRooms)
            Rooms(NumberRooms).RoomName = NewName

            NumberObjs = NumberObjs + 1
            ReDim Preserve Objs(NumberObjs)
            Objs(NumberObjs).ObjectName = NewName
            Objs(NumberObjs).IsRoom = True
            Objs(NumberObjs).CorresRoom = NewName
            Objs(NumberObjs).CorresRoomID = NumberRooms

            Rooms(NumberRooms).ObjID = NumberObjs

            AddToChangeLog("room " & NewName, "create")

            If GameASLVersion >= 410 Then
                AddToObjectProperties(m_oDefaultRoomProperties.Properties, NumberObjs, Thread)
                For j = 1 To m_oDefaultRoomProperties.NumberActions
                    AddObjectAction(NumberObjs, m_oDefaultRoomProperties.Actions(j))
                Next j

                Rooms(NumberRooms).Exits = New RoomExits(Me)
                Rooms(NumberRooms).Exits.ObjID = Rooms(NumberRooms).ObjID
            End If

        ElseIf BeginsWith(CreateData, "object ") Then
            ParamData = RetrieveParameter(CreateData, Thread)
            SCP = InStr(ParamData, ";")
            If SCP = 0 Then
                NewName = ParamData
                ContainerRoom = ""
            Else
                NewName = Trim(Left(ParamData, SCP - 1))
                ContainerRoom = Trim(Mid(ParamData, SCP + 1))
            End If

            NumberObjs = NumberObjs + 1
            ReDim Preserve Objs(NumberObjs)

            With Objs(NumberObjs)
                .ObjectName = NewName
                .ObjectAlias = NewName
                .ContainerRoom = ContainerRoom
                .Exists = True
                .Visible = True
                .Gender = "it"
                .Article = "it"
            End With

            AddToChangeLog("object " & NewName, "create " & Objs(NumberObjs).ContainerRoom)

            If GameASLVersion >= 410 Then
                AddToObjectProperties(m_oDefaultProperties.Properties, NumberObjs, Thread)
                For j = 1 To m_oDefaultProperties.NumberActions
                    AddObjectAction(NumberObjs, m_oDefaultProperties.Actions(j))
                Next j
            End If

            If Not m_gameLoading Then UpdateObjectList(Thread)

        ElseIf BeginsWith(CreateData, "exit ") Then
            ExecuteCreateExit(CreateData, Thread)
        End If
    End Sub

    Private Sub ExecuteCreateExit(ByRef CreateData As String, ByRef Thread As ThreadData)
        Dim ExitData As String
        Dim SrcRoom As String
        Dim DestRoom As String = ""
        Dim SrcID, DestID As Integer
        Dim NewName As String
        Dim SCP As Integer
        Dim ExitExists As Boolean
        Dim i As Integer
        Dim ParamPos As Integer
        Dim SaveData As String

        ExitData = GetEverythingAfter(CreateData, "exit ")
        NewName = RetrieveParameter(CreateData, Thread)
        SCP = InStr(NewName, ";")
        If GameASLVersion < 410 Then
            If SCP = 0 Then
                LogASLError("No exit destination given in 'create exit " & ExitData & "'", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If
        End If

        If SCP = 0 Then
            SrcRoom = Trim(NewName)
        Else
            SrcRoom = Trim(Left(NewName, SCP - 1))
        End If
        SrcID = GetRoomID(SrcRoom, Thread)

        If SrcID = 0 Then
            LogASLError("No such room '" & SrcRoom & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        If GameASLVersion < 410 Then
            ' only do destination room check for ASL <410, as can now have scripts on dynamically
            ' created exits, so the destination doesn't necessarily have to exist.

            DestRoom = Trim(Mid(NewName, SCP + 1))
            If DestRoom <> "" Then
                DestID = GetRoomID(DestRoom, Thread)

                If DestID = 0 Then
                    LogASLError("No such room '" & DestRoom & "'", LOGTYPE_WARNINGERROR)
                    Exit Sub
                End If
            End If
        End If

        ' If it's a "go to" exit, check if it already exists:
        ExitExists = False
        If BeginsWith(ExitData, "<") Then

            If GameASLVersion >= 410 Then
                ExitExists = Rooms(SrcID).Exits.Places.ContainsKey(DestRoom)
            Else
                For i = 1 To Rooms(SrcID).NumberPlaces
                    If LCase(Rooms(SrcID).Places(i).PlaceName) = LCase(DestRoom) Then
                        ExitExists = True
                        i = Rooms(SrcID).NumberPlaces
                    End If
                Next i
            End If

            If ExitExists Then
                LogASLError("Exit from '" & SrcRoom & "' to '" & DestRoom & "' already exists", LOGTYPE_WARNINGERROR)
                Exit Sub
            End If
        End If

        ParamPos = InStr(ExitData, "<")
        If ParamPos = 0 Then
            SaveData = ExitData
        Else
            SaveData = Left(ExitData, ParamPos - 1)
            ' We do this so the changelog doesn't contain unconverted variable names
            SaveData = SaveData & "<" & RetrieveParameter(ExitData, Thread) & ">"
        End If
        AddToChangeLog("room " & Rooms(SrcID).RoomName, "exit " & SaveData)

        With Rooms(SrcID)

            If GameASLVersion >= 410 Then
                .Exits.AddExitFromCreateScript(ExitData, Thread)
            Else
                If BeginsWith(ExitData, "north ") Then
                    .North.Data = DestRoom
                    .North.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "south ") Then
                    .South.Data = DestRoom
                    .South.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "east ") Then
                    .East.Data = DestRoom
                    .East.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "west ") Then
                    .West.Data = DestRoom
                    .West.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "northeast ") Then
                    .NorthEast.Data = DestRoom
                    .NorthEast.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "northwest ") Then
                    .NorthWest.Data = DestRoom
                    .NorthWest.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "southeast ") Then
                    .SouthEast.Data = DestRoom
                    .SouthEast.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "southwest ") Then
                    .SouthWest.Data = DestRoom
                    .SouthWest.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "up ") Then
                    .Up.Data = DestRoom
                    .Up.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "down ") Then
                    .Down.Data = DestRoom
                    .Down.Type = TA_TEXT
                ElseIf BeginsWith(ExitData, "out ") Then
                    .out.Text = DestRoom
                ElseIf BeginsWith(ExitData, "<") Then
                    .NumberPlaces = .NumberPlaces + 1
                    ReDim Preserve .Places(.NumberPlaces)
                    .Places(.NumberPlaces).PlaceName = DestRoom
                Else
                    LogASLError("Invalid direction in 'create exit " & ExitData & "'", LOGTYPE_WARNINGERROR)
                End If
            End If
        End With

        If Not m_gameLoading Then
            ' Update quest.doorways variables
            ShowRoomInfo(CurrentRoom, Thread, True)

            UpdateObjectList(Thread)

            If GameASLVersion < 410 Then
                If CurrentRoom = Rooms(SrcID).RoomName Then
                    UpdateDoorways(SrcID, Thread)
                ElseIf CurrentRoom = Rooms(DestID).RoomName Then
                    UpdateDoorways(DestID, Thread)
                End If
            Else
                ' Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                ' for current room anyway.
                UpdateDoorways(GetRoomID(CurrentRoom, Thread), Thread)
            End If
        End If
    End Sub

    Private Sub ExecDrop(ByRef DropItem As String, ByRef Thread As ThreadData)
        Dim FoundItem, ObjectIsInContainer As Boolean
        Dim ParentID, ObjectID, i As Integer
        Dim Parent As String
        Dim ParentDisplayName As String

        ObjectID = Disambiguate(DropItem, "inventory", Thread)

        If ObjectID > 0 Then
            FoundItem = True
        Else
            FoundItem = False
        End If

        If Not FoundItem Then
            If ObjectID <> -2 Then
                If GameASLVersion >= 391 Then
                    PlayerErrorMessage(ERROR_NOITEM, Thread)
                Else
                    PlayerErrorMessage(ERROR_BADDROP, Thread)
                End If
            End If
            BadCmdBefore = "drop"
            Exit Sub
        End If

        ' If object is inside a container, it must be removed before it can be dropped.
        ObjectIsInContainer = False
        If GameASLVersion >= 391 Then
            If IsYes(GetObjectProperty("parent", ObjectID, True, False)) Then
                ObjectIsInContainer = True
                Parent = GetObjectProperty("parent", ObjectID, False, False)
                ParentID = GetObjectIDNoAlias(Parent)
            End If
        End If

        Dim DropFound As Boolean
        Dim DropStatement As String = ""
        DropFound = False

        For i = Objs(ObjectID).DefinitionSectionStart To Objs(ObjectID).DefinitionSectionEnd
            If BeginsWith(Lines(i), "drop ") Then
                DropStatement = GetEverythingAfter(Lines(i), "drop ")
                DropFound = True
                i = Objs(ObjectID).DefinitionSectionEnd
            End If
        Next i

        SetStringContents("quest.error.article", Objs(ObjectID).Article, Thread)

        If Not DropFound Or BeginsWith(DropStatement, "everywhere") Then
            If ObjectIsInContainer Then
                ' So, we want to drop an object that's in a container or surface. So first
                ' we have to remove the object from that container.

                If Objs(ParentID).ObjectAlias <> "" Then
                    ParentDisplayName = Objs(ParentID).ObjectAlias
                Else
                    ParentDisplayName = Objs(ParentID).ObjectName
                End If

                Print("(first removing " & Objs(ObjectID).Article & " from " & ParentDisplayName & ")", Thread)

                ' Try to remove the object
                Thread.AllowRealNamesInCommand = True
                ExecCommand("remove " & Objs(ObjectID).ObjectName, Thread, False, , True)

                If GetObjectProperty("parent", ObjectID, False, False) <> "" Then
                    ' removing the object failed
                    Exit Sub
                End If
            End If
        End If

        If Not DropFound Then
            PlayerErrorMessage(ERROR_DEFAULTDROP, Thread)
            PlayerItem(Objs(ObjectID).ObjectName, False, Thread)
        Else
            If BeginsWith(DropStatement, "everywhere") Then
                PlayerItem(Objs(ObjectID).ObjectName, False, Thread)
                If InStr(DropStatement, "<") <> 0 Then
                    Print(RetrieveParameter(InputString:=DropStatement, Thread:=Thread), Thread)
                Else
                    PlayerErrorMessage(ERROR_DEFAULTDROP, Thread)
                End If
            ElseIf BeginsWith(DropStatement, "nowhere") Then
                If InStr(DropStatement, "<") <> 0 Then
                    Print(RetrieveParameter(InputString:=DropStatement, Thread:=Thread), Thread)
                Else
                    PlayerErrorMessage(ERROR_CANTDROP, Thread)
                End If
            Else
                ExecuteScript(DropStatement, Thread)
            End If
        End If

    End Sub

    Private Sub ExecExamine(ByRef CommandInfo As String, ByRef Thread As ThreadData)
        Dim ExamineItem, ExamineAction As String
        Dim FoundItem As Boolean
        Dim FoundExamineAction As Boolean
        Dim i, ObjID, j As Integer
        Dim InventoryPlace As String

        FoundExamineAction = False
        FoundItem = False

        InventoryPlace = "inventory"

        ExamineItem = LCase(Trim(GetEverythingAfter(CommandInfo, "examine ")))

        If ExamineItem = "" Then
            PlayerErrorMessage(ERROR_BADEXAMINE, Thread)
            BadCmdBefore = "examine"
            Exit Sub
        End If

        ObjID = Disambiguate(ExamineItem, CurrentRoom & ";" & InventoryPlace, Thread)
        If ObjID > 0 Then
            FoundItem = True
        Else
            FoundItem = False
        End If

        If FoundItem Then
            With Objs(ObjID)

                ' Find "examine" action:
                For i = 1 To .NumberActions
                    If .Actions(i).ActionName = "examine" Then
                        ExecuteScript(.Actions(i).Script, Thread, ObjID)
                        FoundExamineAction = True
                        i = .NumberActions
                    End If
                Next i

                ' Find "examine" property:
                If Not FoundExamineAction Then
                    For i = 1 To .NumberProperties
                        If .Properties(i).PropertyName = "examine" Then
                            Print(.Properties(i).PropertyValue, Thread)
                            FoundExamineAction = True
                            i = .NumberProperties
                        End If
                    Next i
                End If

                ' Find "examine" tag:
                If Not FoundExamineAction Then
                    For j = .DefinitionSectionStart + 1 To Objs(ObjID).DefinitionSectionEnd - 1
                        If BeginsWith(Lines(j), "examine ") Then
                            ExamineAction = Trim(GetEverythingAfter(Lines(j), "examine "))
                            If Left(ExamineAction, 1) = "<" Then
                                Print(RetrieveParameter(Lines(j), Thread), Thread)
                            Else
                                ExecuteScript(ExamineAction, Thread, ObjID)
                            End If
                            FoundExamineAction = True
                        End If
                    Next j
                End If

                If Not FoundExamineAction Then
                    DoLook(ObjID, Thread, True)
                End If

            End With
        End If

        If Not FoundItem Then
            If ObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
            BadCmdBefore = "examine"
        End If

    End Sub

    Private Sub ExecMoveThing(ByRef sThingData As String, ByRef sThingType As Integer, ByRef Thread As ThreadData)
        Dim SemiColonPos As Integer
        Dim ThingName, MoveToPlace As String

        SemiColonPos = InStr(sThingData, ";")

        ThingName = Trim(Left(sThingData, SemiColonPos - 1))
        MoveToPlace = Trim(Mid(sThingData, SemiColonPos + 1))

        MoveThing(ThingName, MoveToPlace, sThingType, Thread)

    End Sub

    Private Sub ExecProperty(ByRef PropertyData As String, ByRef Thread As ThreadData)
        Dim SCP As Integer
        Dim ObjName, Properties As String
        Dim ObjID, i As Integer
        Dim Found As Boolean
        SCP = InStr(PropertyData, ";")

        If SCP = 0 Then
            LogASLError("No property data given in 'property <" & PropertyData & ">'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ObjName = Trim(Left(PropertyData, SCP - 1))
        Properties = Trim(Mid(PropertyData, SCP + 1))

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                Found = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not Found Then
            LogASLError("No such object in 'property <" & PropertyData & ">'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        AddToObjectProperties(Properties, ObjID, Thread)

    End Sub

    Private Sub ExecuteDo(ByRef ProcedureName As String, ByRef Thread As ThreadData)
        Dim ProcedureBlock As DefineBlock
        Dim i, BracketPos As Integer
        Dim CloseBracketPos As Integer
        Dim ParameterData As String
        Dim NewThread As ThreadData
        Dim iNumParameters As Integer
        Dim RunInNewThread As Boolean
        Dim iCurPos, SCP As Integer

        NewThread = Thread

        If GameASLVersion >= 392 And Left(ProcedureName, 8) = "!intproc" Then
            ' If "do" procedure is run in a new thread, thread info is not passed to any nested
            ' script blocks in braces.

            RunInNewThread = False
        Else
            RunInNewThread = True
        End If

        If GameASLVersion >= 284 Then
            BracketPos = InStr(ProcedureName, "(")
            If BracketPos <> 0 Then
                CloseBracketPos = InStr(BracketPos + 1, ProcedureName, ")")
            End If

            If BracketPos <> 0 And CloseBracketPos <> 0 Then
                ParameterData = Mid(ProcedureName, BracketPos + 1, (CloseBracketPos - BracketPos) - 1)
                ProcedureName = Left(ProcedureName, BracketPos - 1)

                ParameterData = ParameterData & ";"
                iCurPos = 1
                Do
                    iNumParameters = iNumParameters + 1
                    SCP = InStr(iCurPos, ParameterData, ";")

                    NewThread.NumParameters = iNumParameters
                    ReDim Preserve NewThread.Parameters(iNumParameters)
                    NewThread.Parameters(iNumParameters) = Trim(Mid(ParameterData, iCurPos, SCP - iCurPos))

                    iCurPos = SCP + 1
                Loop Until iCurPos >= Len(ParameterData)
            End If
        End If

        ProcedureBlock = DefineBlockParam("procedure", ProcedureName)
        If ProcedureBlock.StartLine = 0 And ProcedureBlock.EndLine = 0 Then
            LogASLError("No such procedure " & ProcedureName, LOGTYPE_WARNINGERROR)
        Else
            For i = ProcedureBlock.StartLine + 1 To ProcedureBlock.EndLine - 1
                If Not RunInNewThread Then
                    ExecuteScript((Lines(i)), Thread)
                Else
                    ExecuteScript((Lines(i)), NewThread)
                    Thread.DontProcessCommand = NewThread.DontProcessCommand
                End If
            Next i
        End If
    End Sub

    Private Sub ExecuteDoAction(ByRef ActionData As String, ByRef Thread As ThreadData)
        Dim SCP As Integer
        Dim ObjName, ActionName As String
        Dim ObjID, i As Integer
        Dim FoundID As Boolean

        SCP = InStr(ActionData, ";")
        If SCP = 0 Then
            LogASLError("No action name specified in 'doaction <" & ActionData & ">'")
            Exit Sub
        End If

        ObjName = LCase(Trim(Left(ActionData, SCP - 1)))
        ActionName = Trim(Mid(ActionData, SCP + 1))

        FoundID = False

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = ObjName Then
                FoundID = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not FoundID Then
            LogASLError("No such object '" & ObjName & "'")
            Exit Sub
        End If

        DoAction(ObjID, ActionName, Thread)

    End Sub

    Private Function ExecuteIfHere(ByRef HereThing As String, ByRef Thread As ThreadData) As Boolean
        Dim bResult, bFound As Boolean
        Dim i As Integer

        bFound = False
        bResult = False

        If GameASLVersion <= 281 Then
            For i = 1 To NumberChars
                If Chars(i).ContainerRoom = CurrentRoom And Chars(i).Exists Then
                    If LCase(HereThing) = LCase(Chars(i).ObjectName) Then
                        bResult = True
                        bFound = True
                        i = NumberChars
                    End If
                End If
            Next i
        End If

        If Not bFound Then
            For i = 1 To NumberObjs
                If LCase(Objs(i).ContainerRoom) = LCase(CurrentRoom) And Objs(i).Exists Then
                    If LCase(HereThing) = LCase(Objs(i).ObjectName) Then
                        bResult = True
                        bFound = True
                        i = NumberObjs
                    End If
                End If
            Next i
        End If

        If bFound = False Then
            bResult = False
        End If

        ExecuteIfHere = bResult

    End Function

    Private Function ExecuteIfExists(ByRef ExistsThing As String, ByRef RealCheckOnly As Boolean) As Boolean
        Dim bResult, bFound As Boolean
        Dim bErrorReport As Boolean
        Dim i As Integer
        bErrorReport = False

        Dim SCP As Integer
        If InStr(ExistsThing, ";") <> 0 Then
            SCP = InStr(ExistsThing, ";")
            If LCase(Trim(Mid(ExistsThing, SCP + 1))) = "report" Then
                bErrorReport = True
            End If

            ExistsThing = Left(ExistsThing, SCP - 1)
        End If

        bFound = False

        If GameASLVersion < 281 Then
            For i = 1 To NumberChars
                If LCase(ExistsThing) = LCase(Chars(i).ObjectName) Then
                    If Chars(i).Exists Then
                        bResult = True
                    Else
                        bResult = False
                    End If

                    bFound = True
                    i = NumberChars
                End If
            Next i
        End If

        If Not bFound Then
            For i = 1 To NumberObjs
                If LCase(ExistsThing) = LCase(Objs(i).ObjectName) Then
                    If Objs(i).Exists Then
                        bResult = True
                    Else
                        bResult = False
                    End If

                    bFound = True
                    i = NumberObjs
                End If
            Next i
        End If

        If bFound = False And bErrorReport Then
            LogASLError("No such character/object '" & ExistsThing & "'.", LOGTYPE_USERERROR)
        End If

        If bFound = False Then bResult = False

        If RealCheckOnly Then
            ExecuteIfExists = bFound
        Else
            ExecuteIfExists = bResult
        End If

    End Function

    Private Function ExecuteIfProperty(ByRef PropertyData As String) As Boolean
        Dim SCP, ObjID As Integer
        Dim ObjName As String
        Dim PropertyName As String
        Dim FoundObj As Boolean
        Dim i As Integer

        SCP = InStr(PropertyData, ";")

        If SCP = 0 Then
            LogASLError("No property name given in condition 'property <" & PropertyData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfProperty = False
            Exit Function
        End If

        ObjName = Trim(Left(PropertyData, SCP - 1))
        PropertyName = Trim(Mid(PropertyData, SCP + 1))

        FoundObj = False

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjName) Then
                FoundObj = True
                ObjID = i
                i = NumberObjs
            End If
        Next i

        If Not FoundObj Then
            LogASLError("No such object '" & ObjName & "' in condition 'property <" & PropertyData & ">' ...", LOGTYPE_WARNINGERROR)
            ExecuteIfProperty = False
            Exit Function
        End If

        If GetObjectProperty(PropertyName, ObjID, True) = "yes" Then
            ExecuteIfProperty = True
        Else
            ExecuteIfProperty = False
        End If
    End Function

    Private Sub ExecuteRepeat(ByRef RepeatData As String, ByRef Thread As ThreadData)
        Dim RepeatWhileTrue As Boolean
        Dim RepeatScript As String = ""
        Dim BracketPos As Integer
        Dim Conditions As String
        Dim FinishedLoop As Boolean
        Dim CurPos As Integer
        Dim AfterBracket As String
        Dim FoundScript As Boolean
        FoundScript = False

        If BeginsWith(RepeatData, "while ") Then
            RepeatWhileTrue = True
            RepeatData = GetEverythingAfter(RepeatData, "while ")
        ElseIf BeginsWith(RepeatData, "until ") Then
            RepeatWhileTrue = False
            RepeatData = GetEverythingAfter(RepeatData, "until ")
        Else
            LogASLError("Expected 'until' or 'while' in 'repeat " & ReportErrorLine(RepeatData) & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        CurPos = 1
        Do
            BracketPos = InStr(CurPos, RepeatData, ">")
            AfterBracket = Trim(Mid(RepeatData, BracketPos + 1))
            If (Not BeginsWith(AfterBracket, "and ")) And (Not BeginsWith(AfterBracket, "or ")) Then
                RepeatScript = AfterBracket
                FoundScript = True
            Else
                CurPos = BracketPos + 1
            End If
        Loop Until FoundScript Or AfterBracket = ""

        Conditions = Trim(Left(RepeatData, BracketPos))

        FinishedLoop = False

        Do
            If ExecuteConditions(Conditions, Thread) = RepeatWhileTrue Then
                ExecuteScript(RepeatScript, Thread)
            Else
                FinishedLoop = True
            End If
        Loop Until FinishedLoop Or m_gameFinished
    End Sub

    Private Sub ExecuteSetCollectable(ByRef setparam As String, ByRef Thread As ThreadData)
        Dim NewVal As Double
        Dim SemiColonPos As Integer
        Dim FoundCollectable As Boolean
        Dim CName, TheNewVal As String
        Dim i As Integer
        Dim ColNum As Integer
        Dim OP, TheNewValue As String

        SemiColonPos = InStr(setparam, ";")
        CName = Trim(Left(setparam, SemiColonPos - 1))
        TheNewVal = Trim(Right(setparam, Len(setparam) - SemiColonPos))

        FoundCollectable = False

        For i = 1 To NumCollectables
            If Collectables(i).collectablename = CName Then
                ColNum = i
                i = NumCollectables
                FoundCollectable = True
            End If
        Next i

        If Not FoundCollectable Then
            LogASLError("No such collectable '" & setparam & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        OP = Left(TheNewVal, 1)
        TheNewValue = Trim(Right(TheNewVal, Len(TheNewVal) - 1))
        If IsNumeric(TheNewValue) Then
            NewVal = Val(TheNewValue)
        Else
            NewVal = GetCollectableAmount(TheNewValue)
        End If

        If OP = "+" Then
            Collectables(ColNum).collectablenumber = Collectables(ColNum).collectablenumber + NewVal
        ElseIf OP = "-" Then
            Collectables(ColNum).collectablenumber = Collectables(ColNum).collectablenumber - NewVal
        ElseIf OP = "=" Then
            Collectables(ColNum).collectablenumber = NewVal
        End If

        CheckCollectable(ColNum)

        UpdateItems(Thread)
    End Sub

    Private Sub ExecuteWait(ByRef WaitLine As String, ByRef Thread As ThreadData)

        If WaitLine <> "" Then
            Print(RetrieveParameter(WaitLine, Thread), Thread)
        Else

            If GameASLVersion >= 410 Then
                PlayerErrorMessage(ERROR_DEFAULTWAIT, Thread)
            Else
                Print("|nPress a key to continue...", Thread)
            End If
        End If

        DoWait()
    End Sub

    Private m_sFileData As String
    Private m_lIndex As Integer

    Private Sub InitFileData(FileData As String)
        m_sFileData = FileData
        m_lIndex = 1
    End Sub

    Private Function GetNextChunk() As String
        Dim NullPos As Integer
        NullPos = InStr(m_lIndex, m_sFileData, Chr(0))

        GetNextChunk = Mid(m_sFileData, m_lIndex, NullPos - m_lIndex)

        If NullPos < Len(m_sFileData) Then
            m_lIndex = NullPos + 1
        End If
    End Function

    Function GetFileDataChars(count As Integer) As String
        GetFileDataChars = Mid(m_sFileData, m_lIndex, count)
        m_lIndex = m_lIndex + count
    End Function

    Private Function GetObjectActions(ByRef ActionInfo As String) As ActionType
        Dim ActionName As String
        Dim ActionScript As String
        Dim EP As Integer

        ActionName = LCase(RetrieveParameter(ActionInfo, NullThread))
        EP = InStr(ActionInfo, ">")
        If EP = Len(ActionInfo) Then
            LogASLError("No script given for '" & ActionName & "' action data", LOGTYPE_WARNINGERROR)
            Return New ActionType
        End If

        ActionScript = Trim(Mid(ActionInfo, EP + 1))

        GetObjectActions.ActionName = ActionName
        GetObjectActions.Script = ActionScript

    End Function

    Private Function GetObjectID(ObjectName As String, ByRef Thread As ThreadData, Optional ByRef ObjectRoom As String = "") As Integer

        Dim CurID, i As Integer
        Dim FoundItem As Boolean
        FoundItem = False

        If BeginsWith(ObjectName, "the ") Then
            ObjectName = GetEverythingAfter(ObjectName, "the ")
        End If

        For i = 1 To NumberObjs
            If (LCase(Objs(i).ObjectName) = LCase(ObjectName) Or LCase(Objs(i).ObjectName) = "the " & LCase(ObjectName)) And (LCase(Objs(i).ContainerRoom) = LCase(ObjectRoom) Or ObjectRoom = "") And Objs(i).Exists = True Then
                CurID = i
                FoundItem = True
                i = NumberObjs
            End If
        Next i

        If Not FoundItem And GameASLVersion >= 280 Then
            CurID = Disambiguate(ObjectName, ObjectRoom, Thread)
            If CurID > 0 Then FoundItem = True
        End If

        If FoundItem Then
            GetObjectID = CurID
        Else
            GetObjectID = -1
        End If

    End Function

    Private Function GetObjectIDNoAlias(ByRef ObjectName As String) As Integer
        Dim i, ID As Integer
        Dim Found As Boolean

        Found = False

        For i = 1 To NumberObjs
            If LCase(Objs(i).ObjectName) = LCase(ObjectName) Then
                ID = i
                Found = True
                i = NumberObjs
            End If
        Next i

        If Not Found Then
            ID = 0
        End If

        GetObjectIDNoAlias = ID

    End Function

    Friend Function GetObjectProperty(ByRef PropertyName As String, ByRef ObjID As Integer, Optional ByRef ReturnExistsOnly As Boolean = False, Optional ByRef LogError As Boolean = True) As String
        Dim bFound As Boolean
        Dim sResult As String = ""
        Dim i As Integer
        bFound = False

        With Objs(ObjID)
            For i = 1 To .NumberProperties
                If LCase(.Properties(i).PropertyName) = LCase(PropertyName) Then
                    bFound = True
                    sResult = .Properties(i).PropertyValue
                    i = .NumberProperties
                End If
            Next i

            If ReturnExistsOnly Then
                If bFound Then
                    GetObjectProperty = "yes"
                Else
                    GetObjectProperty = "no"
                End If
            Else
                If bFound Then
                    GetObjectProperty = sResult
                Else
                    If LogError Then
                        LogASLError("Object '" & Objs(ObjID).ObjectName & "' has no property '" & PropertyName & "'", LOGTYPE_WARNINGERROR)
                        GetObjectProperty = "!"
                    Else
                        GetObjectProperty = ""
                    End If
                End If
            End If

        End With

    End Function

    Private Function GetPropertiesInType(ByRef TypeName As String, Optional ByRef bError As Boolean = True) As PropertiesActions
        Dim Found As Boolean
        Dim SecID As Integer
        Dim PropertyList As New PropertiesActions
        Dim NewProperties As PropertiesActions
        Dim IncTypeName As String
        Dim i, j As Integer
        Found = False

        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define type") Then
                If LCase(RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread)) = LCase(TypeName) Then
                    SecID = i
                    i = NumberSections
                    Found = True
                End If
            End If
        Next i

        If Not Found Then
            If bError Then
                LogASLError("No such type '" & TypeName & "'", LOGTYPE_WARNINGERROR)
            End If
            Return New PropertiesActions
        End If

        For i = DefineBlocks(SecID).StartLine + 1 To DefineBlocks(SecID).EndLine - 1
            If BeginsWith(Lines(i), "type ") Then
                IncTypeName = LCase(RetrieveParameter(Lines(i), NullThread))
                NewProperties = GetPropertiesInType(IncTypeName)
                With PropertyList
                    .Properties = .Properties & NewProperties.Properties
                    ReDim Preserve .Actions(.NumberActions + NewProperties.NumberActions)
                    For j = .NumberActions + 1 To .NumberActions + NewProperties.NumberActions
                        .Actions(j) = NewProperties.Actions(j - .NumberActions)
                    Next j
                    .NumberActions = .NumberActions + NewProperties.NumberActions

                    ' Add this type name to the TypesIncluded list...
                    .NumberTypesIncluded = .NumberTypesIncluded + 1
                    ReDim Preserve .TypesIncluded(.NumberTypesIncluded)
                    .TypesIncluded(.NumberTypesIncluded) = IncTypeName

                    ' and add the names of the types included by it...

                    ReDim Preserve .TypesIncluded(.NumberTypesIncluded + NewProperties.NumberTypesIncluded)
                    For j = .NumberTypesIncluded + 1 To .NumberTypesIncluded + NewProperties.NumberTypesIncluded
                        .TypesIncluded(j) = NewProperties.TypesIncluded(j - .NumberTypesIncluded)
                    Next j
                    .NumberTypesIncluded = .NumberTypesIncluded + NewProperties.NumberTypesIncluded
                End With
            ElseIf BeginsWith(Lines(i), "action ") Then
                With PropertyList
                    .NumberActions = .NumberActions + 1
                    ReDim Preserve .Actions(.NumberActions)
                    .Actions(.NumberActions) = GetObjectActions(GetEverythingAfter(Lines(i), "action "))
                End With
            ElseIf BeginsWith(Lines(i), "properties ") Then
                PropertyList.Properties = PropertyList.Properties & RetrieveParameter(Lines(i), NullThread) & ";"
            ElseIf Trim(Lines(i)) <> "" Then
                PropertyList.Properties = PropertyList.Properties & Lines(i) & ";"
            End If
        Next i

        GetPropertiesInType = PropertyList
    End Function

    Friend Function GetRoomID(RoomName As String, ByRef Thread As ThreadData) As Integer
        Dim Found As Boolean
        Dim ArrayIndex, i As Integer
        Found = False

        If InStr(RoomName, "[") > 0 Then
            ArrayIndex = GetArrayIndex(RoomName, Thread)
            RoomName = RoomName & Trim(Str(ArrayIndex))
        End If

        For i = 1 To NumberRooms
            If LCase(Rooms(i).RoomName) = LCase(RoomName) Then
                Found = True
                GetRoomID = i
                i = NumberRooms
            End If
        Next i

        If Not Found Then GetRoomID = 0

    End Function

    Private Function GetTextOrScript(ByRef TextScript As String) As TextAction
        TextScript = Trim(TextScript)

        If Left(TextScript, 1) = "<" Then
            GetTextOrScript.Type = TA_TEXT
            GetTextOrScript.Data = RetrieveParameter(TextScript, NullThread)
        Else
            GetTextOrScript.Type = TA_SCRIPT
            GetTextOrScript.Data = TextScript
        End If

    End Function

    Private Function GetThingNumber(ByRef ThingName As String, ByRef ThingRoom As String, ByRef ThingType As Integer) As Integer
        ' Returns the number in the Chars() or Objs() array
        ' of the specified char/obj

        Dim f, i As Integer

        f = 0

        If ThingType = QUEST_CHARACTER Then
            For i = 1 To NumberChars
                If (ThingRoom <> "" And LCase(Chars(i).ObjectName) = LCase(ThingName) And LCase(Chars(i).ContainerRoom) = LCase(ThingRoom)) Or (ThingRoom = "" And LCase(Chars(i).ObjectName) = LCase(ThingName)) Then
                    GetThingNumber = i
                    i = NumberChars
                    f = 1
                End If
            Next i
        ElseIf ThingType = QUEST_OBJECT Then
            For i = 1 To NumberObjs
                If (ThingRoom <> "" And LCase(Objs(i).ObjectName) = LCase(ThingName) And LCase(Objs(i).ContainerRoom) = LCase(ThingRoom)) Or (ThingRoom = "" And LCase(Objs(i).ObjectName) = LCase(ThingName)) Then
                    GetThingNumber = i
                    i = NumberObjs
                    f = 1
                End If
            Next i
        End If

        If f = 0 Then
            GetThingNumber = -1
        End If

    End Function

    Private Function GetThingBlock(ByRef ThingName As String, ByRef ThingRoom As String, ByRef ThingType As Integer) As DefineBlock
        ' Returns position where specified char/obj is defined
        ' in ASL code

        Dim f, i As Integer

        f = 0

        If ThingType = QUEST_CHARACTER Then
            For i = 1 To NumberChars
                If LCase(Chars(i).ObjectName) = LCase(ThingName) And LCase(Chars(i).ContainerRoom) = LCase(ThingRoom) Then
                    GetThingBlock.StartLine = Chars(i).DefinitionSectionStart
                    GetThingBlock.EndLine = Chars(i).DefinitionSectionEnd
                    i = NumberChars
                    f = 1
                End If
            Next i
        ElseIf ThingType = QUEST_OBJECT Then
            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(ThingName) And LCase(Objs(i).ContainerRoom) = LCase(ThingRoom) Then
                    GetThingBlock.StartLine = Objs(i).DefinitionSectionStart
                    GetThingBlock.EndLine = Objs(i).DefinitionSectionEnd
                    i = NumberObjs
                    f = 1
                End If
            Next i
        End If

        If f = 0 Then
            GetThingBlock.StartLine = 0
            GetThingBlock.EndLine = 0
        End If

    End Function

    Private Function MakeRestoreData() As String
        Dim FileData As New System.Text.StringBuilder
        Dim ObjectData(0) As ChangeType
        Dim RoomData(0) As ChangeType
        Dim NumObjectData As Integer
        Dim NumRoomData As Integer
        Dim i As Integer
        Dim j As Integer
        Dim ObjExists As String
        Dim ObjVisible As String
        Dim sAppliesTo As String
        Dim sChangeData As String
        Dim StartPoint As Integer

        ' <<< FILE HEADER DATA >>>

        FileData.Append(QSGVersion & Chr(0) & GetOriginalFilenameForQSG() & Chr(0))

        ' The start point for encrypted data is after the filename
        StartPoint = FileData.Length + 1

        FileData.Append(CurrentRoom & Chr(0))

        ' Organise Change Log

        With GameChangeData
            For i = 1 To .NumberChanges

                If BeginsWith(.ChangeData(i).AppliesTo, "object ") Then
                    NumObjectData = NumObjectData + 1
                    ReDim Preserve ObjectData(NumObjectData)
                    ObjectData(NumObjectData) = .ChangeData(i)
                ElseIf BeginsWith(.ChangeData(i).AppliesTo, "room ") Then
                    NumRoomData = NumRoomData + 1
                    ReDim Preserve RoomData(NumRoomData)
                    RoomData(NumRoomData) = .ChangeData(i)
                End If
            Next i
        End With

        ' <<< OBJECT CREATE/CHANGE DATA >>>

        FileData.Append(Trim(Str(NumObjectData + m_oChangeLogObjects.Changes.Count)) & Chr(0))

        For i = 1 To NumObjectData
            FileData.Append(GetEverythingAfter(ObjectData(i).AppliesTo, "object ") & Chr(0) & ObjectData(i).Change & Chr(0))
        Next i

        For Each key As String In m_oChangeLogObjects.Changes.Keys
            sAppliesTo = Split(key, "#")(0)
            sChangeData = m_oChangeLogObjects.Changes.Item(key)

            FileData.Append(sAppliesTo & Chr(0) & sChangeData & Chr(0))
        Next

        ' <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>

        FileData.Append(Trim(Str(NumberObjs)) & Chr(0))

        For i = 1 To NumberObjs
            If Objs(i).Exists Then
                ObjExists = Chr(1)
            Else
                ObjExists = Chr(0)
            End If

            If Objs(i).Visible Then
                ObjVisible = Chr(1)
            Else
                ObjVisible = Chr(0)
            End If

            FileData.Append(Objs(i).ObjectName & Chr(0) & ObjExists & ObjVisible & Objs(i).ContainerRoom & Chr(0))
        Next i

        ' <<< ROOM CREATE/CHANGE DATA >>>

        FileData.Append(Trim(Str(NumRoomData)) & Chr(0))

        For i = 1 To NumRoomData
            FileData.Append(GetEverythingAfter(RoomData(i).AppliesTo, "room ") & Chr(0) & RoomData(i).Change & Chr(0))
        Next i

        ' <<< TIMER STATE DATA >>>

        FileData.Append(Trim(Str(NumberTimers)) & Chr(0))

        For i = 1 To NumberTimers
            With Timers(i)
                FileData.Append(.TimerName & Chr(0))

                If .TimerActive Then
                    FileData.Append(Chr(1))
                Else
                    FileData.Append(Chr(0))
                End If

                FileData.Append(Trim(Str(.TimerInterval)) & Chr(0))
                FileData.Append(Trim(Str(.TimerTicks)) & Chr(0))
            End With
        Next i

        ' <<< STRING VARIABLE DATA >>>

        FileData.Append(Trim(Str(NumberStringVariables)) & Chr(0))

        For i = 1 To NumberStringVariables
            With StringVariable(i)
                FileData.Append(.VariableName & Chr(0) & Trim(Str(.VariableUBound)) & Chr(0))

                For j = 0 To .VariableUBound
                    FileData.Append(.VariableContents(j) & Chr(0))
                Next j
            End With
        Next i

        ' <<< NUMERIC VARIABLE DATA >>>

        FileData.Append(Trim(Str(NumberNumericVariables)) & Chr(0))

        For i = 1 To NumberNumericVariables
            With NumericVariable(i)
                FileData.Append(.VariableName & Chr(0) & Trim(Str(.VariableUBound)) & Chr(0))

                For j = 0 To .VariableUBound
                    FileData.Append(.VariableContents(j) & Chr(0))
                Next j
            End With
        Next i

        ' Now encrypt data
        Dim sFileData As String
        Dim NewFileData As New System.Text.StringBuilder

        sFileData = FileData.ToString()

        NewFileData.Append(Left(sFileData, StartPoint - 1))

        For i = StartPoint To Len(sFileData)
            NewFileData.Append(Chr(255 - Asc(Mid(sFileData, i, 1))))
        Next i

        MakeRestoreData = NewFileData.ToString()

    End Function

    Private Sub MoveThing(ByRef sThingName As String, ByRef sThingRoom As String, ByRef iThingType As Integer, ByRef Thread As ThreadData)
        Dim i, iThingNum, ArrayIndex As Integer
        Dim OldRoom As String = ""

        iThingNum = GetThingNumber(sThingName, "", iThingType)

        If InStr(sThingRoom, "[") > 0 Then
            ArrayIndex = GetArrayIndex(sThingRoom, Thread)
            sThingRoom = sThingRoom & Trim(Str(ArrayIndex))
        End If

        If iThingType = QUEST_CHARACTER Then
            Chars(iThingNum).ContainerRoom = sThingRoom
        ElseIf iThingType = QUEST_OBJECT Then
            OldRoom = Objs(iThingNum).ContainerRoom
            Objs(iThingNum).ContainerRoom = sThingRoom
        End If

        If GameASLVersion >= 391 Then
            ' If this object contains other objects, move them too
            For i = 1 To NumberObjs
                If LCase(GetObjectProperty("parent", i, False, False)) = LCase(sThingName) Then
                    MoveThing(Objs(i).ObjectName, sThingRoom, iThingType, Thread)
                End If
            Next i
        End If

        UpdateObjectList(Thread)

        If BeginsWith(LCase(sThingRoom), "inventory") Or BeginsWith(LCase(OldRoom), "inventory") Then
            UpdateItems(Thread)
        End If

    End Sub

    Public Sub Pause(ByRef Duration As Integer)
        m_player.DoPause(Duration)
        ChangeState(State.Waiting)

        SyncLock m_waitLock
            System.Threading.Monitor.Wait(m_waitLock)
        End SyncLock
    End Sub

    Private Function ConvertParameter(ByRef sParameter As String, ByRef sConvertChar As String, ByRef ConvertAction As Integer, ByRef Thread As ThreadData) As String
        ' Returns a string with functions, string and
        ' numeric variables executed or converted as
        ' appropriate, read for display/etc.

        Dim iCurStringPos As Integer
        Dim iVarPos As Integer
        Dim NewParam As String = ""
        Dim bFinishLoop As Boolean
        Dim sCurStringBit As String
        Dim sVarName As String
        Dim iNextPos As Integer

        iCurStringPos = 1
        bFinishLoop = False
        Do
            iVarPos = InStr(iCurStringPos, sParameter, sConvertChar)
            If iVarPos = 0 Then
                iVarPos = Len(sParameter) + 1
                bFinishLoop = True
            End If

            sCurStringBit = Mid(sParameter, iCurStringPos, iVarPos - iCurStringPos)
            NewParam = NewParam & sCurStringBit

            If bFinishLoop = False Then
                iNextPos = InStr(iVarPos + 1, sParameter, sConvertChar)

                If iNextPos = 0 Then
                    LogASLError("Line parameter <" & sParameter & "> has missing " & sConvertChar, LOGTYPE_WARNINGERROR)
                    ConvertParameter = "<ERROR>"
                    Exit Function
                End If

                sVarName = Mid(sParameter, iVarPos + 1, (iNextPos - 1) - iVarPos)

                If sVarName = "" Then
                    NewParam = NewParam & sConvertChar
                Else

                    If ConvertAction = CONVERT_STRINGS Then
                        NewParam = NewParam & GetStringContents(sVarName, Thread)
                    ElseIf ConvertAction = CONVERT_FUNCTIONS Then
                        sVarName = EvaluateInlineExpressions(sVarName)
                        NewParam = NewParam & DoFunction(sVarName, Thread)
                    ElseIf ConvertAction = CONVERT_NUMERIC Then
                        NewParam = NewParam & Trim(Str(GetNumericContents(sVarName, Thread)))
                    ElseIf ConvertAction = CONVERT_COLLECTABLES Then
                        NewParam = NewParam & Trim(Str(GetCollectableAmount(sVarName)))
                    End If
                End If

                iCurStringPos = iNextPos + 1
            End If
        Loop Until bFinishLoop

        ConvertParameter = NewParam

    End Function

    Private Function DoFunction(ByRef FunctionData As String, ByRef Thread As ThreadData) As String
        Dim FunctionName, FunctionParameter As String
        Dim sIntFuncResult As String = ""
        Dim bIntFuncExecuted As Boolean
        Dim NewThread As ThreadData
        Dim ParameterData As String
        Dim ParamPos, EndParamPos As Integer
        Dim SCP, i As Integer

        bIntFuncExecuted = False

        ParamPos = InStr(FunctionData, "(")
        If ParamPos <> 0 Then
            FunctionName = Trim(Left(FunctionData, ParamPos - 1))
            EndParamPos = InStrRev(FunctionData, ")")
            If EndParamPos = 0 Then
                LogASLError("Expected ) in $" & FunctionData & "$", LOGTYPE_WARNINGERROR)
                Return ""
            End If
            FunctionParameter = Mid(FunctionData, ParamPos + 1, (EndParamPos - ParamPos) - 1)
        Else
            FunctionName = FunctionData
            FunctionParameter = ""
        End If

        Dim procblock As DefineBlock
        procblock = DefineBlockParam("function", FunctionName)

        If procblock.StartLine = 0 And procblock.EndLine = 0 Then
            'Function does not exist; try an internal function.
            sIntFuncResult = DoInternalFunction(FunctionName, FunctionParameter, Thread)
            If sIntFuncResult = "__NOTDEFINED" Then
                LogASLError("No such function '" & FunctionName & "'", LOGTYPE_WARNINGERROR)
                Return "[ERROR]"
            Else
                bIntFuncExecuted = True
            End If
        End If

        Dim iNumParameters, iCurPos As Integer
        If bIntFuncExecuted Then
            Return sIntFuncResult
        Else
            NewThread = Thread

            iNumParameters = 0 : iCurPos = 1

            If FunctionParameter <> "" Then
                FunctionParameter = FunctionParameter & ";"
                Do
                    iNumParameters = iNumParameters + 1
                    SCP = InStr(iCurPos, FunctionParameter, ";")

                    ParameterData = Trim(Mid(FunctionParameter, iCurPos, SCP - iCurPos))
                    SetStringContents("quest.function.parameter." & Trim(Str(iNumParameters)), ParameterData, Thread)

                    NewThread.NumParameters = iNumParameters
                    ReDim Preserve NewThread.Parameters(iNumParameters)
                    NewThread.Parameters(iNumParameters) = ParameterData

                    iCurPos = SCP + 1
                Loop Until iCurPos >= Len(FunctionParameter)
                SetStringContents("quest.function.numparameters", Trim(Str(iNumParameters)), Thread)
            Else
                SetStringContents("quest.function.numparameters", "0", Thread)
                NewThread.NumParameters = 0
            End If

            Dim result As String = ""

            For i = procblock.StartLine + 1 To procblock.EndLine - 1
                ExecuteScript(Lines(i), NewThread)
                result = NewThread.FunctionReturnValue
            Next i

            Return result
        End If

    End Function

    Private Function DoInternalFunction(ByRef FunctionName As String, ByRef FunctionParameter As String, ByRef Thread As ThreadData) As String
        ' Split FunctionParameter into individual parameters
        Dim iNumParameters, iCurPos As Integer
        Dim Parameter() As String
        Dim UntrimmedParameter() As String
        Dim SCP, ObjID, i As Integer
        Dim FoundObj As Boolean

        iNumParameters = 0 : iCurPos = 1

        If FunctionParameter <> "" Then
            FunctionParameter = FunctionParameter & ";"
            Do
                iNumParameters = iNumParameters + 1
                SCP = InStr(iCurPos, FunctionParameter, ";")
                ReDim Preserve Parameter(iNumParameters)
                ReDim Preserve UntrimmedParameter(iNumParameters)

                UntrimmedParameter(iNumParameters) = Mid(FunctionParameter, iCurPos, SCP - iCurPos)
                Parameter(iNumParameters) = Trim(UntrimmedParameter(iNumParameters))

                iCurPos = SCP + 1
            Loop Until iCurPos >= Len(FunctionParameter)

            ' Remove final ";"
            FunctionParameter = Left(FunctionParameter, Len(FunctionParameter) - 1)
        Else
            iNumParameters = 1
            ReDim Parameter(1)
            ReDim UntrimmedParameter(1)
            Parameter(1) = ""
            UntrimmedParameter(1) = ""
        End If

        Dim Param3 As String
        Dim Param2 As String
        Dim oExit As RoomExit
        If FunctionName = "displayname" Then
            ObjID = GetObjectID(Parameter(1), Thread)
            If ObjID = -1 Then
                LogASLError("Object '" & Parameter(1) & "' does not exist", LOGTYPE_WARNINGERROR)
                Return "!"
            Else
                Return Objs(GetObjectID(Parameter(1), Thread)).ObjectAlias
            End If
        ElseIf FunctionName = "numberparameters" Then
            Return Trim(Str(Thread.NumParameters))
        ElseIf FunctionName = "parameter" Then
            If iNumParameters = 0 Then
                LogASLError("No parameter number specified for $parameter$ function", LOGTYPE_WARNINGERROR)
                Return ""
            Else
                If Val(Parameter(1)) > Thread.NumParameters Then
                    LogASLError("No parameter number " & Parameter(1) & " sent to this function", LOGTYPE_WARNINGERROR)
                    Return ""
                Else
                    Return Trim(Thread.Parameters(CInt(Parameter(1))))
                End If
            End If
        ElseIf FunctionName = "gettag" Then
            ' Deprecated
            Return FindStatement(DefineBlockParam("room", Parameter(1)), Parameter(2))
        ElseIf FunctionName = "objectname" Then
            Return Objs(Thread.CallingObjectID).ObjectName
        ElseIf FunctionName = "locationof" Then
            For i = 1 To NumberChars
                If LCase(Chars(i).ObjectName) = LCase(Parameter(1)) Then
                    Return Chars(i).ContainerRoom
                End If
            Next i

            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(Parameter(1)) Then
                    Return Objs(i).ContainerRoom
                End If
            Next i
        ElseIf FunctionName = "lengthof" Then
            Return Str(Len(UntrimmedParameter(1)))
        ElseIf FunctionName = "left" Then
            If Val(Parameter(2)) < 0 Then
                LogASLError("Invalid function call in '$Left$(" & Parameter(1) & "; " & Parameter(2) & ")$'", LOGTYPE_WARNINGERROR)
                Return "!"
            Else
                Return Left(Parameter(1), CInt(Parameter(2)))
            End If
        ElseIf FunctionName = "right" Then
            If Val(Parameter(2)) < 0 Then
                LogASLError("Invalid function call in '$Right$(" & Parameter(1) & "; " & Parameter(2) & ")$'", LOGTYPE_WARNINGERROR)
                Return "!"
            Else
                Return Right(Parameter(1), CInt(Parameter(2)))
            End If
        ElseIf FunctionName = "mid" Then
            If iNumParameters = 3 Then
                If Val(Parameter(2)) < 0 Then
                    LogASLError("Invalid function call in '$Mid$(" & Parameter(1) & "; " & Parameter(2) & "; " & Parameter(3) & ")$'", LOGTYPE_WARNINGERROR)
                    Return "!"
                Else
                    Return Mid(Parameter(1), CInt(Parameter(2)), CInt(Parameter(3)))
                End If
            ElseIf iNumParameters = 2 Then
                If Val(Parameter(2)) < 0 Then
                    LogASLError("Invalid function call in '$Mid$(" & Parameter(1) & "; " & Parameter(2) & ")$'", LOGTYPE_WARNINGERROR)
                    Return "!"
                Else
                    Return Mid(Parameter(1), CInt(Parameter(2)))
                End If
            End If
            LogASLError("Invalid function call to '$Mid$(...)$'", LOGTYPE_WARNINGERROR)
            Return ""
        ElseIf FunctionName = "rand" Then
            Return Str(Int(m_random.NextDouble() * (CDbl(Parameter(2)) - CDbl(Parameter(1)) + 1)) + CDbl(Parameter(1)))
        ElseIf FunctionName = "instr" Then
            If iNumParameters = 3 Then
                Param3 = ""
                If InStr(Parameter(3), "_") <> 0 Then
                    For i = 1 To Len(Parameter(3))
                        If Mid(Parameter(3), i, 1) = "_" Then
                            Param3 = Param3 & " "
                        Else
                            Param3 = Param3 & Mid(Parameter(3), i, 1)
                        End If
                    Next i
                Else
                    Param3 = Parameter(3)
                End If
                If Val(Parameter(1)) <= 0 Then
                    LogASLError("Invalid function call in '$instr(" & Parameter(1) & "; " & Parameter(2) & "; " & Parameter(3) & ")$'", LOGTYPE_WARNINGERROR)
                    Return "!"
                Else
                    Return Trim(Str(InStr(CInt(Parameter(1)), Parameter(2), Param3)))
                End If
            ElseIf iNumParameters = 2 Then
                Param2 = ""
                If InStr(Parameter(2), "_") <> 0 Then
                    For i = 1 To Len(Parameter(2))
                        If Mid(Parameter(2), i, 1) = "_" Then
                            Param2 = Param2 & " "
                        Else
                            Param2 = Param2 & Mid(Parameter(2), i, 1)
                        End If
                    Next i
                Else
                    Param2 = Parameter(2)
                End If
                Return Trim(Str(InStr(Parameter(1), Param2)))
            End If
            LogASLError("Invalid function call to '$Instr$(...)$'", LOGTYPE_WARNINGERROR)
            Return ""
        ElseIf FunctionName = "ucase" Then
            Return UCase(Parameter(1))
        ElseIf FunctionName = "lcase" Then
            Return LCase(Parameter(1))
        ElseIf FunctionName = "capfirst" Then
            Return UCase(Left(Parameter(1), 1)) & Mid(Parameter(1), 2)
        ElseIf FunctionName = "symbol" Then
            If Parameter(1) = "lt" Then
                Return "<"
            ElseIf Parameter(1) = "gt" Then
                Return ">"
            Else
                Return "!"
            End If
        ElseIf FunctionName = "loadmethod" Then
            Return GameLoadMethod
        ElseIf FunctionName = "timerstate" Then
            For i = 1 To NumberTimers
                If LCase(Timers(i).TimerName) = LCase(Parameter(1)) Then
                    If Timers(i).TimerActive Then
                        Return "1"
                    Else
                        Return "0"
                    End If
                End If
            Next i
            LogASLError("No such timer '" & Parameter(1) & "'", LOGTYPE_WARNINGERROR)
            Return "!"
        ElseIf FunctionName = "timerinterval" Then
            For i = 1 To NumberTimers
                If LCase(Timers(i).TimerName) = LCase(Parameter(1)) Then
                    Return Str(Timers(i).TimerInterval)
                End If
            Next i
            LogASLError("No such timer '" & Parameter(1) & "'", LOGTYPE_WARNINGERROR)
            Return "!"
        ElseIf FunctionName = "ubound" Then
            For i = 1 To NumberNumericVariables
                If LCase(NumericVariable(i).VariableName) = LCase(Parameter(1)) Then
                    Return Trim(Str(NumericVariable(i).VariableUBound))
                End If
            Next i

            For i = 1 To NumberStringVariables
                If LCase(StringVariable(i).VariableName) = LCase(Parameter(1)) Then
                    Return Trim(Str(StringVariable(i).VariableUBound))
                End If
            Next i

            LogASLError("No such variable '" & Parameter(1) & "'", LOGTYPE_WARNINGERROR)
            Return "!"
        ElseIf FunctionName = "objectproperty" Then
            FoundObj = False
            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(Parameter(1)) Then
                    FoundObj = True
                    ObjID = i
                    i = NumberObjs
                End If
            Next i

            If Not FoundObj Then
                LogASLError("No such object '" & Parameter(1) & "'", LOGTYPE_WARNINGERROR)
                Return "!"
            Else
                Return GetObjectProperty(Parameter(2), ObjID)
            End If
        ElseIf FunctionName = "getobjectname" Then
            If iNumParameters = 3 Then
                ObjID = Disambiguate(Parameter(1), Parameter(2) & ";" & Parameter(3), Thread)
            ElseIf iNumParameters = 2 Then
                ObjID = Disambiguate(Parameter(1), Parameter(2), Thread)
            Else

                ObjID = Disambiguate(Parameter(1), CurrentRoom & ";inventory", Thread)
            End If

            If ObjID <= -1 Then
                LogASLError("No object found with display name '" & Parameter(1) & "'", LOGTYPE_WARNINGERROR)
                Return "!"
            Else
                Return Objs(ObjID).ObjectName
            End If
        ElseIf FunctionName = "thisobject" Then
            Return Objs(Thread.CallingObjectID).ObjectName
        ElseIf FunctionName = "thisobjectname" Then
            Return Objs(Thread.CallingObjectID).ObjectAlias
        ElseIf FunctionName = "speechenabled" Then
            If optEnableSpeech Then
                Return "1"
            Else
                Return "0"
            End If
        ElseIf FunctionName = "removeformatting" Then
            Return StripCodes(FunctionParameter)
        ElseIf FunctionName = "findexit" And GameASLVersion >= 410 Then
            oExit = FindExit(FunctionParameter)
            If oExit Is Nothing Then
                Return ""
            Else
                Return Objs(oExit.ObjID).ObjectName
            End If
        End If

        Return "__NOTDEFINED"

    End Function

    Private Sub ExecFor(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        ' See if this is a "for each" loop:
        If BeginsWith(ScriptLine, "each ") Then
            ExecForEach(GetEverythingAfter(ScriptLine, "each "), Thread)
            Exit Sub
        End If

        ' Executes a for loop, of form:
        '   for <variable; startvalue; endvalue> script
        Dim CounterVariable As String
        Dim StartValue As Integer
        Dim EndValue As Integer
        Dim LoopScript As String
        Dim StepValue As Integer
        Dim ForData As String
        Dim SCPos2, SCPos1, SCPos3 As Integer
        Dim i As Double

        ForData = RetrieveParameter(ScriptLine, Thread)

        ' Extract individual components:
        SCPos1 = InStr(ForData, ";")
        SCPos2 = InStr(SCPos1 + 1, ForData, ";")
        SCPos3 = InStr(SCPos2 + 1, ForData, ";")

        CounterVariable = Trim(Left(ForData, SCPos1 - 1))
        StartValue = CInt(Mid(ForData, SCPos1 + 1, (SCPos2 - 1) - SCPos1))

        If SCPos3 <> 0 Then
            EndValue = CInt(Mid(ForData, SCPos2 + 1, (SCPos3 - 1) - SCPos2))
            StepValue = CInt(Mid(ForData, SCPos3 + 1))
        Else
            EndValue = CInt(Mid(ForData, SCPos2 + 1))
            StepValue = 1
        End If

        LoopScript = Trim(Mid(ScriptLine, InStr(ScriptLine, ">") + 1))

        For i = StartValue To EndValue Step StepValue
            SetNumericVariableContents(CounterVariable, i, Thread)
            ExecuteScript(LoopScript, Thread)
            i = GetNumericContents(CounterVariable, Thread)
        Next i

    End Sub

    Private Sub ExecSetVar(ByRef VarInfo As String, ByRef Thread As ThreadData)
        ' Sets variable contents from a script parameter.
        ' Eg <var1;7> sets numeric variable var1
        ' to 7

        Dim iSemiColonPos As Integer
        Dim sVarName As String
        Dim iVarCont As String
        Dim ArrayIndex As Integer
        Dim FirstNum, SecondNum As Double
        Dim ObscuredVarInfo As String
        Dim ExpResult As ExpressionResult
        Dim OpPos As Integer
        Dim OP As String

        iSemiColonPos = InStr(VarInfo, ";")
        sVarName = Trim(Left(VarInfo, iSemiColonPos - 1))
        iVarCont = Trim(Mid(VarInfo, iSemiColonPos + 1))

        ArrayIndex = GetArrayIndex(sVarName, Thread)

        If IsNumeric(sVarName) Then
            LogASLError("Invalid numeric variable name '" & sVarName & "' - variable names cannot be numeric", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        On Error GoTo errhandle

        If GameASLVersion >= 391 Then
            ExpResult = ExpressionHandler(iVarCont)
            If ExpResult.success = EXPRESSION_OK Then
                iVarCont = ExpResult.Result
            Else
                iVarCont = "0"
                LogASLError("Error setting numeric variable <" & VarInfo & "> : " & ExpResult.Message, LOGTYPE_WARNINGERROR)
            End If
        Else
            ObscuredVarInfo = ObscureNumericExps(iVarCont)

            OpPos = InStr(ObscuredVarInfo, "+")
            If OpPos = 0 Then OpPos = InStr(ObscuredVarInfo, "*")
            If OpPos = 0 Then OpPos = InStr(ObscuredVarInfo, "/")
            If OpPos = 0 Then OpPos = InStr(2, ObscuredVarInfo, "-")

            If OpPos <> 0 Then
                OP = Mid(iVarCont, OpPos, 1)
                FirstNum = Val(Left(iVarCont, OpPos - 1))
                SecondNum = Val(Mid(iVarCont, OpPos + 1))

                Select Case OP
                    Case "+"
                        iVarCont = Str(FirstNum + SecondNum)
                    Case "-"
                        iVarCont = Str(FirstNum - SecondNum)
                    Case "*"
                        iVarCont = Str(FirstNum * SecondNum)
                    Case "/"
                        If SecondNum <> 0 Then
                            iVarCont = Str(FirstNum / SecondNum)
                        Else
                            LogASLError("Division by zero - The result of this operation has been set to zero.", LOGTYPE_WARNINGERROR)
                            iVarCont = "0"
                        End If
                End Select
            End If
        End If


        SetNumericVariableContents(sVarName, Val(iVarCont), Thread, ArrayIndex)

        Exit Sub

errhandle:
        LogASLError("Error " & Trim(CStr(Err.Number)) & " (" & Err.Description & ") setting variable '" & sVarName & "' to '" & iVarCont & "'", LOGTYPE_WARNINGERROR)

    End Sub

    Private m_questionResponse As Boolean

    Private Function ExecuteIfAsk(ByRef askq As String) As Boolean
        m_player.ShowQuestion(askq)
        ChangeState(State.Waiting)

        SyncLock m_waitLock
            System.Threading.Monitor.Wait(m_waitLock)
        End SyncLock

        Return m_questionResponse
    End Function

    Private Sub SetQuestionResponse(response As Boolean) Implements IASL.SetQuestionResponse
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf SetQuestionResponseInNewThread))
        ChangeState(State.Working)
        runnerThread.Start(response)
        WaitForStateChange(State.Working)
    End Sub

    Private Sub SetQuestionResponseInNewThread(response As Object)
        m_questionResponse = DirectCast(response, Boolean)

        SyncLock m_waitLock
            System.Threading.Monitor.PulseAll(m_waitLock)
        End SyncLock
    End Sub

    Private Function ExecuteIfGot(ByRef theitem As String) As Boolean

        Dim i As Integer

        Dim FoundObject As Boolean
        Dim Result As Boolean
        Dim InventoryPlace As String
        Dim bResult As Boolean
        Dim iValidItemFlag As Integer
        If GameASLVersion >= 280 Then
            FoundObject = False
            Result = False
            InventoryPlace = "inventory"

            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(theitem) Then
                    FoundObject = True
                    If Objs(i).ContainerRoom = InventoryPlace And Objs(i).Exists Then
                        Result = True
                    Else
                        Result = False
                    End If
                End If
            Next i

            If Not FoundObject Then
                Result = False
                LogASLError("No object '" & theitem & "' defined.", LOGTYPE_WARNINGERROR)
            End If

            ExecuteIfGot = Result

        Else

            iValidItemFlag = 0

            For i = 1 To NumberItems
                If LCase(Items(i).itemname) = LCase(theitem) Then
                    bResult = Items(i).gotitem
                    i = NumberItems
                    iValidItemFlag = 1
                End If
            Next i

            If iValidItemFlag = 0 Then
                LogASLError("Item '" & theitem & "' not defined.", LOGTYPE_WARNINGERROR)
                bResult = False
            End If

            ExecuteIfGot = bResult
        End If

    End Function

    Private Function ExecuteIfHas(ByRef hascond As String) As Boolean

        Dim checkval As Double
        Dim condresult As Boolean
        Dim SemiColonPos As Integer
        Dim TheNewVal, CName, OP As String
        Dim i, ColNum As Integer
        Dim TheNewValue As String

        SemiColonPos = InStr(hascond, ";")
        CName = Trim(Left(hascond, SemiColonPos - 1))
        TheNewVal = Trim(Right(hascond, Len(hascond) - SemiColonPos))

        i = -1

        For i = 1 To NumCollectables
            If Collectables(i).collectablename = CName Then
                ColNum = i
                i = NumCollectables
            End If
        Next i

        If i = -1 Then
            LogASLError("No such collectable in " & hascond, LOGTYPE_WARNINGERROR)
            Exit Function
        End If

        OP = Left(TheNewVal, 1)
        TheNewValue = Trim(Right(TheNewVal, Len(TheNewVal) - 1))
        If IsNumeric(TheNewValue) Then
            checkval = Val(TheNewValue)
        Else
            checkval = GetCollectableAmount(TheNewValue)
        End If

        If OP = "+" Then
            If Collectables(ColNum).collectablenumber > checkval Then condresult = True Else condresult = False
        ElseIf OP = "-" Then
            If Collectables(ColNum).collectablenumber < checkval Then condresult = True Else condresult = False
        ElseIf OP = "=" Then
            If Collectables(ColNum).collectablenumber = checkval Then condresult = True Else condresult = False
        End If

        ExecuteIfHas = condresult

    End Function

    Private Function ExecuteIfIs(ByRef IsCondition As String) As Boolean
        Dim SCPos, SC2Pos As Integer
        Dim Value1, Value2 As String
        Dim Condition As String
        Dim Satisfied As Boolean
        Dim ExpectNumerics As Boolean
        Dim ExpResult As ExpressionResult

        SCPos = InStr(IsCondition, ";")
        If SCPos = 0 Then
            LogASLError("Expected second parameter in 'is " & IsCondition & "'", LOGTYPE_WARNINGERROR)
            ExecuteIfIs = False
            Exit Function
        End If

        SC2Pos = InStr(SCPos + 1, IsCondition, ";")
        If SC2Pos = 0 Then
            ' Only two parameters => standard "="
            Condition = "="
            Value1 = Trim(Left(IsCondition, SCPos - 1))
            Value2 = Trim(Mid(IsCondition, SCPos + 1))
        Else
            Value1 = Trim(Left(IsCondition, SCPos - 1))
            Condition = Trim(Mid(IsCondition, SCPos + 1, (SC2Pos - SCPos) - 1))
            Value2 = Trim(Mid(IsCondition, SC2Pos + 1))
        End If

        If GameASLVersion >= 391 Then
            ' Evaluate expressions in Value1 and Value2
            ExpResult = ExpressionHandler(Value1)

            If ExpResult.success = EXPRESSION_OK Then
                Value1 = ExpResult.Result
            End If

            ExpResult = ExpressionHandler(Value2)

            If ExpResult.success = EXPRESSION_OK Then
                Value2 = ExpResult.Result
            End If
        End If

        Satisfied = False

        Select Case Condition
            Case "="
                If LCase(Value1) = LCase(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = False
            Case "!="
                If LCase(Value1) <> LCase(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = False
            Case "gt"
                If Val(Value1) > Val(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = True
            Case "lt"
                If Val(Value1) < Val(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = True
            Case "gt="
                If Val(Value1) >= Val(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = True
            Case "lt="
                If Val(Value1) <= Val(Value2) Then
                    Satisfied = True
                End If
                ExpectNumerics = True
            Case Else
                LogASLError("Unrecognised comparison condition in 'is " & IsCondition & "'", LOGTYPE_WARNINGERROR)
        End Select

        If ExpectNumerics Then
            If Not (IsNumeric(Value1) And IsNumeric(Value2)) Then
                LogASLError("Expected numeric comparison comparing '" & Value1 & "' and '" & Value2 & "'", LOGTYPE_WARNINGERROR)
            End If
        End If

        ExecuteIfIs = Satisfied
    End Function

    Private Function GetNumericContents(NumericName As String, ByRef Thread As ThreadData, Optional ByRef NOERROR As Boolean = False) As Double
        Dim bNumExists As Boolean
        Dim iNumNumber As Integer
        Dim ArrayIndex, i As Integer
        Dim ArrayIndexData As String
        bNumExists = False

        ' First, see if the variable already exists. If it
        ' does, get its contents. If not, generate an error.

        Dim OpenPos, ClosePos As Integer
        If InStr(NumericName, "[") <> 0 And InStr(NumericName, "]") <> 0 Then
            OpenPos = InStr(NumericName, "[")
            ClosePos = InStr(NumericName, "]")
            ArrayIndexData = Mid(NumericName, OpenPos + 1, (ClosePos - OpenPos) - 1)
            If IsNumeric(ArrayIndexData) Then
                ArrayIndex = CInt(ArrayIndexData)
            Else
                ArrayIndex = CInt(GetNumericContents(ArrayIndexData, Thread))
            End If
            NumericName = Left(NumericName, OpenPos - 1)
        Else
            ArrayIndex = 0
        End If

        If NumberNumericVariables > 0 Then
            For i = 1 To NumberNumericVariables
                If LCase(NumericVariable(i).VariableName) = LCase(NumericName) Then
                    iNumNumber = i
                    bNumExists = True
                    i = NumberNumericVariables
                End If
            Next i
        End If

        If bNumExists = False Then
            If Not NOERROR Then LogASLError("No numeric variable '" & NumericName & "' defined.", LOGTYPE_WARNINGERROR)
            GetNumericContents = -32767
            Exit Function
        End If

        If ArrayIndex > NumericVariable(iNumNumber).VariableUBound Then
            If Not NOERROR Then LogASLError("Array index of '" & NumericName & "[" & Trim(Str(ArrayIndex)) & "]' too big.", LOGTYPE_WARNINGERROR)
            GetNumericContents = -32766
            Exit Function
        End If

        ' Now, set the contents
        GetNumericContents = Val(NumericVariable(iNumNumber).VariableContents(ArrayIndex))
    End Function

    Friend Sub PlayerErrorMessage(ByRef iErrorNumber As Integer, ByRef Thread As ThreadData)
        Print(GetErrorMessage(iErrorNumber, Thread), Thread)
    End Sub

    Private Sub PlayerErrorMessage_ExtendInfo(ByRef iErrorNumber As Integer, ByRef Thread As ThreadData, ByRef sExtraInfo As String)
        Dim sErrorMessage As String

        sErrorMessage = GetErrorMessage(iErrorNumber, Thread)

        If sExtraInfo <> "" Then
            If Right(sErrorMessage, 1) = "." Then
                sErrorMessage = Left(sErrorMessage, Len(sErrorMessage) - 1)
            End If

            sErrorMessage = sErrorMessage & " - " & sExtraInfo & "."
        End If

        Print(sErrorMessage, Thread)
    End Sub

    Private Function GetErrorMessage(ByRef iErrorNumber As Integer, ByRef Thread As ThreadData) As String
        GetErrorMessage = ConvertParameter(ConvertParameter(ConvertParameter(PlayerErrorMessageString(iErrorNumber), "%", CONVERT_NUMERIC, Thread), "$", CONVERT_FUNCTIONS, Thread), "#", CONVERT_STRINGS, Thread)
    End Function

    Private Sub PlayMedia(filename As String)
        PlayMedia(filename, False, False)
    End Sub

    Private Sub PlayMedia(filename As String, sync As Boolean, looped As Boolean)
        If filename.Length = 0 Then
            m_player.StopSound()
        Else
            If looped And sync Then sync = False ' Can't loop and sync at the same time, that would just hang!

            m_player.PlaySound(filename, sync, looped)

            If sync Then
                ChangeState(State.Waiting)
            End If

            If sync Then
                SyncLock (m_waitLock)
                    System.Threading.Monitor.Wait(m_waitLock)
                End SyncLock
            End If

        End If
    End Sub

    Private Sub PlayMidi(ByRef MidiFileName As String)
        PlayMedia(MidiFileName)
    End Sub

    Private Sub PlayMP3(ByRef MP3File As String)
        PlayMedia(MP3File)
    End Sub

    Private Sub PlayWav(ByRef parameter As String)
        Dim sync As Boolean = False
        Dim looped As Boolean = False
        Dim filename As String

        Dim params As New List(Of String)(parameter.Split(";"c))

        params = New List(Of String)(params.Select(Function(p As String) Trim(p)))

        filename = params(0)

        If params.Contains("loop") Then looped = True
        If params.Contains("sync") Then sync = True

        If filename.Length > 0 And InStr(filename, ".") = 0 Then
            filename = filename & ".wav"
        End If

        PlayMedia(filename, sync, looped)
    End Sub

    Private Function RestoreGameData(ByRef InputFileData As String) As Boolean
        ' Return true if successful
        Dim i, NumData, j As Integer
        Dim AppliesTo As String
        Dim data As String = ""
        Dim ObjID, TimerNum As Integer
        Dim VarUBound As Integer
        Dim Found As Boolean
        Dim NumStoredData As Integer
        Dim StoredData(0) As ChangeType
        Dim DecryptedFile As New System.Text.StringBuilder

        ' Decrypt file
        For i = 1 To Len(InputFileData)
            DecryptedFile.Append(Chr(255 - Asc(Mid(InputFileData, i, 1))))
        Next i

        m_sFileData = DecryptedFile.ToString()

        CurrentRoom = GetNextChunk()

        ' OBJECTS

        NumData = CInt(GetNextChunk())

        Dim createdObjects As New List(Of String)

        For i = 1 To NumData
            AppliesTo = GetNextChunk()
            data = GetNextChunk()

            ' As of Quest 4.0, properties and actions are put into StoredData while we load the file,
            ' and then processed later. This is so any created rooms pick up their properties - otherwise
            ' we try to set them before they've been created.

            If BeginsWith(data, "properties ") Or BeginsWith(data, "action ") Then
                NumStoredData = NumStoredData + 1
                ReDim Preserve StoredData(NumStoredData)
                StoredData(NumStoredData).AppliesTo = AppliesTo
                StoredData(NumStoredData).Change = data
            ElseIf BeginsWith(data, "create ") Then
                Dim createData As String = AppliesTo & ";" & GetEverythingAfter(data, "create ")
                ' workaround bug where duplicate "create" entries appear in the restore data
                If Not createdObjects.Contains(createData) Then
                    ExecuteCreate("object <" & createData & ">", NullThread)
                    createdObjects.Add(createData)
                End If
            Else
                LogASLError("QSG Error: Unrecognised item '" & AppliesTo & "; " & data & "'", LOGTYPE_INTERNALERROR)
            End If
        Next i

        NumData = CInt(GetNextChunk())
        For i = 1 To NumData
            AppliesTo = GetNextChunk()
            data = GetFileDataChars(2)

            ObjID = GetObjectIDNoAlias(AppliesTo)

            If Left(data, 1) = Chr(1) Then
                Objs(ObjID).Exists = True
            Else
                Objs(ObjID).Exists = False
            End If

            If Right(data, 1) = Chr(1) Then
                Objs(ObjID).Visible = True
            Else
                Objs(ObjID).Visible = False
            End If

            Objs(ObjID).ContainerRoom = GetNextChunk()
        Next i

        ' ROOMS

        NumData = CInt(GetNextChunk())

        For i = 1 To NumData
            AppliesTo = GetNextChunk()
            data = GetNextChunk()

            If BeginsWith(data, "exit ") Then
                ExecuteCreate(data, NullThread)
            ElseIf data = "create" Then
                ExecuteCreate("room <" & AppliesTo & ">", NullThread)
            ElseIf BeginsWith(data, "destroy exit ") Then
                DestroyExit(AppliesTo & "; " & GetEverythingAfter(data, "destroy exit "), NullThread)
            End If
        Next i

        ' Now go through and apply object properties and actions

        For i = 1 To NumStoredData
            With StoredData(i)
                If BeginsWith(.Change, "properties ") Then
                    AddToObjectProperties(GetEverythingAfter(.Change, "properties "), GetObjectIDNoAlias(.AppliesTo), NullThread)
                ElseIf BeginsWith(.Change, "action ") Then
                    AddToObjectActions(GetEverythingAfter(.Change, "action "), GetObjectIDNoAlias(.AppliesTo), NullThread)
                End If
            End With
        Next i

        ' TIMERS

        NumData = CInt(GetNextChunk())
        For i = 1 To NumData
            Found = False
            AppliesTo = GetNextChunk()
            For j = 1 To NumberTimers
                If Timers(j).TimerName = AppliesTo Then
                    TimerNum = j
                    j = NumberTimers
                    Found = True
                End If
            Next j

            If Found Then
                With Timers(TimerNum)

                    Dim thisChar As String = GetFileDataChars(1)

                    If thisChar = Chr(1) Then
                        .TimerActive = True
                    Else
                        .TimerActive = False
                    End If

                    .TimerInterval = CInt(GetNextChunk())
                    .TimerTicks = CInt(GetNextChunk())
                End With
            End If
        Next i

        ' STRING VARIABLES

        ' Set this flag so we don't run any status variable onchange scripts while restoring
        m_gameIsRestoring = True

        NumData = CInt(GetNextChunk())
        For i = 1 To NumData
            AppliesTo = GetNextChunk()
            VarUBound = CInt(GetNextChunk())

            If VarUBound = 0 Then
                data = GetNextChunk()
                SetStringContents(AppliesTo, data, NullThread)
            Else
                For j = 0 To VarUBound
                    data = GetNextChunk()
                    SetStringContents(AppliesTo, data, NullThread, j)
                Next j
            End If
        Next i

        ' NUMERIC VARIABLES

        NumData = CInt(GetNextChunk())
        For i = 1 To NumData
            AppliesTo = GetNextChunk()
            VarUBound = CInt(GetNextChunk())

            If VarUBound = 0 Then
                data = GetNextChunk()
                SetNumericVariableContents(AppliesTo, Val(data), NullThread)
            Else
                For j = 0 To VarUBound
                    data = GetNextChunk()
                    SetNumericVariableContents(AppliesTo, Val(data), NullThread, j)
                Next j
            End If
        Next i

        m_gameIsRestoring = False
        RestoreGameData = True
    End Function

    Private Sub SetBackground(ByRef Colour As String)
        m_player.SetBackground("#" + GetHTMLColour(Colour, "white"))
    End Sub

    Private Sub SetForeground(ByRef Colour As String)
        m_player.SetForeground("#" + GetHTMLColour(Colour, "black"))
    End Sub

    Private Sub SetDefaultPlayerErrorMessages()
        PlayerErrorMessageString(ERROR_BADCOMMAND) = "I don't understand your command. Type HELP for a list of valid commands."
        PlayerErrorMessageString(ERROR_BADGO) = "I don't understand your use of 'GO' - you must either GO in some direction, or GO TO a place."
        PlayerErrorMessageString(ERROR_BADGIVE) = "You didn't say who you wanted to give that to."
        PlayerErrorMessageString(ERROR_BADCHARACTER) = "I can't see anybody of that name here."
        PlayerErrorMessageString(ERROR_NOITEM) = "You don't have that."
        PlayerErrorMessageString(ERROR_ITEMUNWANTED) = "#quest.error.gender# doesn't want #quest.error.article#."
        PlayerErrorMessageString(ERROR_BADLOOK) = "You didn't say what you wanted to look at."
        PlayerErrorMessageString(ERROR_BADTHING) = "I can't see that here."
        PlayerErrorMessageString(ERROR_DEFAULTLOOK) = "Nothing out of the ordinary."
        PlayerErrorMessageString(ERROR_DEFAULTSPEAK) = "#quest.error.gender# says nothing."
        PlayerErrorMessageString(ERROR_BADITEM) = "I can't see that anywhere."
        PlayerErrorMessageString(ERROR_DEFAULTTAKE) = "You pick #quest.error.article# up."
        PlayerErrorMessageString(ERROR_BADUSE) = "You didn't say what you wanted to use that on."
        PlayerErrorMessageString(ERROR_DEFAULTUSE) = "You can't use that here."
        PlayerErrorMessageString(ERROR_DEFAULTOUT) = "There's nowhere you can go out to around here."
        PlayerErrorMessageString(ERROR_BADPLACE) = "You can't go there."
        PlayerErrorMessageString(ERROR_DEFAULTEXAMINE) = "Nothing out of the ordinary."
        PlayerErrorMessageString(ERROR_BADTAKE) = "You can't take #quest.error.article#."
        PlayerErrorMessageString(ERROR_CANTDROP) = "You can't drop that here."
        PlayerErrorMessageString(ERROR_DEFAULTDROP) = "You drop #quest.error.article#."
        PlayerErrorMessageString(ERROR_BADDROP) = "You are not carrying such a thing."
        PlayerErrorMessageString(ERROR_BADPRONOUN) = "I don't know what '#quest.error.pronoun#' you are referring to."
        PlayerErrorMessageString(ERROR_BADEXAMINE) = "You didn't say what you wanted to examine."
        PlayerErrorMessageString(ERROR_ALREADYOPEN) = "It is already open."
        PlayerErrorMessageString(ERROR_ALREADYCLOSED) = "It is already closed."
        PlayerErrorMessageString(ERROR_CANTOPEN) = "You can't open that."
        PlayerErrorMessageString(ERROR_CANTCLOSE) = "You can't close that."
        PlayerErrorMessageString(ERROR_DEFAULTOPEN) = "You open it."
        PlayerErrorMessageString(ERROR_DEFAULTCLOSE) = "You close it."
        PlayerErrorMessageString(ERROR_BADPUT) = "You didn't specify what you wanted to put #quest.error.article# on or in."
        PlayerErrorMessageString(ERROR_CANTPUT) = "You can't put that there."
        PlayerErrorMessageString(ERROR_DEFAULTPUT) = "Done."
        PlayerErrorMessageString(ERROR_CANTREMOVE) = "You can't remove that."
        PlayerErrorMessageString(ERROR_ALREADYPUT) = "It is already there."
        PlayerErrorMessageString(ERROR_DEFAULTREMOVE) = "Done."
        PlayerErrorMessageString(ERROR_LOCKED) = "The exit is locked."
        PlayerErrorMessageString(ERROR_DEFAULTWAIT) = "Press a key to continue..."
        PlayerErrorMessageString(ERROR_ALREADYTAKEN) = "You already have that."
    End Sub

    Private Sub SetFont(ByRef FontName As String, ByRef Thread As ThreadData, Optional ByRef OutputTo As String = "normal")
        If FontName = "" Then FontName = DefaultFontName
        m_player.SetFont(FontName)
    End Sub

    Private Sub SetFontSize(ByRef FontSize As Double, ByRef Thread As ThreadData, Optional ByRef OutputTo As String = "normal")
        If FontSize = 0 Then FontSize = DefaultFontSize
        m_player.SetFontSize(CStr(FontSize))
    End Sub

    Private Sub SetNumericVariableContents(ByRef NumName As String, ByRef NumContent As Double, ByRef Thread As ThreadData, Optional ByRef ArrayIndex As Integer = 0)
        Dim bNumExists As Boolean
        Dim iNumNumber As Integer
        Dim NumTitle, OnChangeScript As String
        Dim i As Integer
        bNumExists = False

        If IsNumeric(NumName) Then
            LogASLError("Illegal numeric variable name '" & NumName & "' - check you didn't put % around the variable name in the ASL code", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ' First, see if variable already exists. If it does,
        ' modify it. If not, create it.

        If NumberNumericVariables > 0 Then
            For i = 1 To NumberNumericVariables
                If LCase(NumericVariable(i).VariableName) = LCase(NumName) Then
                    iNumNumber = i
                    bNumExists = True
                    i = NumberNumericVariables
                End If
            Next i
        End If

        If bNumExists = False Then
            NumberNumericVariables = NumberNumericVariables + 1
            iNumNumber = NumberNumericVariables
            ReDim Preserve NumericVariable(iNumNumber)

            For i = 0 To ArrayIndex
                NumTitle = NumName
                If ArrayIndex <> 0 Then NumTitle = NumTitle & "[" & Trim(Str(i)) & "]"
            Next i
            NumericVariable(iNumNumber).VariableUBound = ArrayIndex
        End If

        If ArrayIndex > NumericVariable(iNumNumber).VariableUBound Then
            ReDim Preserve NumericVariable(iNumNumber).VariableContents(ArrayIndex)
            For i = NumericVariable(iNumNumber).VariableUBound + 1 To ArrayIndex
                NumTitle = NumName
                If ArrayIndex <> 0 Then NumTitle = NumTitle & "[" & Trim(Str(i)) & "]"
            Next i
            NumericVariable(iNumNumber).VariableUBound = ArrayIndex

        End If

        ' Now, set the contents
        NumericVariable(iNumNumber).VariableName = NumName
        ReDim Preserve NumericVariable(iNumNumber).VariableContents(NumericVariable(iNumNumber).VariableUBound)
        NumericVariable(iNumNumber).VariableContents(ArrayIndex) = CStr(NumContent)

        If NumericVariable(iNumNumber).OnChangeScript <> "" And Not m_gameIsRestoring Then
            OnChangeScript = NumericVariable(iNumNumber).OnChangeScript
            ExecuteScript(OnChangeScript, Thread)
        End If

        If NumericVariable(iNumNumber).DisplayString <> "" Then
            UpdateStatusVars(Thread)
        End If

    End Sub

    Private Sub SetOpenClose(ByRef ObjectName As String, ByRef DoOpen As Boolean, ByRef Thread As ThreadData)
        Dim ObjID As Integer
        Dim CommandName As String

        If DoOpen Then
            CommandName = "open"
        Else
            CommandName = "close"
        End If

        ObjID = GetObjectIDNoAlias(ObjectName)
        If ObjID = 0 Then
            LogASLError("Invalid object name specified in '" & CommandName & " <" & ObjectName & ">", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        DoOpenClose(ObjID, DoOpen, False, Thread)

    End Sub

    Private Sub SetTimerState(ByRef TimerName As String, ByRef TimerState As Boolean)
        Dim FoundTimer As Boolean
        Dim i As Integer

        For i = 1 To NumberTimers
            If LCase(TimerName) = LCase(Timers(i).TimerName) Then
                FoundTimer = True
                Timers(i).TimerActive = TimerState
                Timers(i).BypassThisTurn = True     ' don't trigger timer during the turn it was first enabled
                i = NumberTimers
            End If
        Next i

        If Not FoundTimer Then
            LogASLError("No such timer '" & TimerName & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

    End Sub

    Private Function SetUnknownVariableType(ByRef VariableData As String, ByRef Thread As ThreadData) As Integer
        Dim SCP As Integer
        Dim VariableName As String
        Dim VariableContents As String
        Dim FoundVariable As Boolean
        Dim i As Integer
        FoundVariable = False

        SCP = InStr(VariableData, ";")
        If SCP = 0 Then
            SetUnknownVariableType = SET_ERROR
            Exit Function
        End If

        VariableName = Trim(Left(VariableData, SCP - 1))
        Dim BeginPos As Integer
        If InStr(VariableName, "[") <> 0 And InStr(VariableName, "]") <> 0 Then
            BeginPos = InStr(VariableName, "[")
            VariableName = Left(VariableName, BeginPos - 1)
        End If

        VariableContents = Trim(Mid(VariableData, SCP + 1))

        For i = 1 To NumberStringVariables
            If LCase(StringVariable(i).VariableName) = LCase(VariableName) Then
                ExecSetString(VariableData, Thread)
                i = NumberStringVariables
                FoundVariable = True
            End If
        Next i

        If FoundVariable Then
            SetUnknownVariableType = SET_FOUND
            Exit Function
        End If

        For i = 1 To NumberNumericVariables
            If LCase(NumericVariable(i).VariableName) = LCase(VariableName) Then
                ExecSetVar(VariableData, Thread)
                i = NumberNumericVariables
                FoundVariable = True
            End If
        Next i

        If FoundVariable Then
            SetUnknownVariableType = SET_FOUND
            Exit Function
        End If

        For i = 1 To NumCollectables
            If LCase(Collectables(i).collectablename) = LCase(VariableName) Then
                ExecuteSetCollectable(VariableData, Thread)
                i = NumCollectables
                FoundVariable = True
            End If
        Next i

        If FoundVariable Then
            SetUnknownVariableType = SET_FOUND
            Exit Function
        End If

        SetUnknownVariableType = SET_UNFOUND

    End Function

    Private Function SetUpChoiceForm(ByRef choicesection As String, ByRef Thread As ThreadData) As String
        ' Returns script to execute from choice block
        Dim choiceblock As DefineBlock
        Dim P As String
        Dim i As Integer

        choiceblock = DefineBlockParam("selection", choicesection)
        P = FindStatement(choiceblock, "info")

        Dim menuOptions As New Dictionary(Of String, String)
        Dim menuScript As New Dictionary(Of String, String)

        For i = choiceblock.StartLine + 1 To choiceblock.EndLine - 1
            If BeginsWith(Lines(i), "choice ") Then
                menuOptions.Add(CStr(i), RetrieveParameter(Lines(i), Thread))
                menuScript.Add(CStr(i), Trim(Right(Lines(i), Len(Lines(i)) - InStr(Lines(i), ">"))))
            End If
        Next i

        Print("- |i" & P & "|xi", Thread)

        Dim mnu As New MenuData(P, menuOptions, False)
        Dim choice As String = ShowMenu(mnu)

        Print("- " & menuOptions(choice) & "|n", Thread)
        SetUpChoiceForm = menuScript(choice)

    End Function

    Private Sub SetUpDefaultFonts()
        ' Sets up default fonts
        Dim ThisFontName As String
        Dim ThisFontSize, i As Integer

        Dim gameblock As DefineBlock
        gameblock = GetDefineBlock("game")

        DefaultFontName = optDefaultFontName
        DefaultFontSize = optDefaultFontSize

        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), "default fontname ") Then
                ThisFontName = RetrieveParameter(Lines(i), NullThread)
                If ThisFontName <> "" Then
                    DefaultFontName = ThisFontName
                End If
            ElseIf BeginsWith(Lines(i), "default fontsize ") Then
                ThisFontSize = CInt(RetrieveParameter(Lines(i), NullThread))
                If ThisFontSize <> 0 Then
                    DefaultFontSize = ThisFontSize
                End If
            End If
        Next i

    End Sub

    Private Sub SetUpDisplayVariables()
        Dim i As Integer
        Dim iStringNumber As Integer
        Dim ThisVariable As VariableType
        Dim ThisType, DisplayString As String
        Dim iNumNumber As Integer

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(i), "define variable ") Then
                DisplayStatus = True

                ReDim ThisVariable.VariableContents(0)

                With ThisVariable
                    .VariableName = RetrieveParameter(Lines(i), NullThread)
                    .DisplayString = ""
                    .NoZeroDisplay = False
                    .OnChangeScript = ""
                    .VariableContents(0) = ""
                    .VariableUBound = 0
                End With

                ThisType = "numeric"

                Do
                    i = i + 1

                    If BeginsWith(Lines(i), "type ") Then
                        ThisType = GetEverythingAfter(Lines(i), "type ")
                        If ThisType <> "string" And ThisType <> "numeric" Then
                            LogASLError("Unrecognised variable type in variable '" & ThisVariable.VariableName & "' - type '" & ThisType & "'", LOGTYPE_WARNINGERROR)
                            Exit Do
                        End If
                    ElseIf BeginsWith(Lines(i), "onchange ") Then
                        ThisVariable.OnChangeScript = GetEverythingAfter(Lines(i), "onchange ")
                    ElseIf BeginsWith(Lines(i), "display ") Then
                        DisplayString = GetEverythingAfter(Lines(i), "display ")
                        If BeginsWith(DisplayString, "nozero ") Then
                            ThisVariable.NoZeroDisplay = True
                            DisplayString = GetEverythingAfter(DisplayString, "nozero ")
                        End If
                        ThisVariable.DisplayString = RetrieveParameter(Lines(i), NullThread, False)
                    ElseIf BeginsWith(Lines(i), "value ") Then
                        ThisVariable.VariableContents(0) = RetrieveParameter(Lines(i), NullThread)
                    End If

                Loop Until Trim(Lines(i)) = "end define"

                If ThisType = "string" Then
                    ' Create string variable
                    NumberStringVariables = NumberStringVariables + 1
                    iStringNumber = NumberStringVariables
                    ReDim Preserve StringVariable(iStringNumber)

                    StringVariable(iStringNumber).VariableName = ThisVariable.VariableName

                    StringVariable(iStringNumber).VariableUBound = 0
                    ReDim StringVariable(iStringNumber).VariableContents(0)

                    StringVariable(iStringNumber) = ThisVariable

                    NumDisplayStrings = NumDisplayStrings + 1
                    ReDim Preserve DisplayStringIDs(NumDisplayStrings)
                    DisplayStringIDs(NumDisplayStrings) = iStringNumber

                ElseIf ThisType = "numeric" Then
                    If ThisVariable.VariableContents(0) = "" Then ThisVariable.VariableContents(0) = CStr(0)
                    NumberNumericVariables = NumberNumericVariables + 1
                    iNumNumber = NumberNumericVariables
                    ReDim Preserve NumericVariable(iNumNumber)
                    ReDim NumericVariable(iNumNumber).VariableContents(0)
                    NumericVariable(iNumNumber).VariableUBound = 0

                    NumericVariable(iNumNumber).VariableName = ThisVariable.VariableName
                    NumericVariable(iNumNumber) = ThisVariable

                    NumDisplayNumerics = NumDisplayNumerics + 1
                    ReDim Preserve DisplayNumericIDs(NumDisplayNumerics)
                    DisplayNumericIDs(NumDisplayNumerics) = iNumNumber

                End If
            End If
        Next i

    End Sub

    Private Sub SetUpGameObject()
        Dim i As Integer
        Dim PropertyData As PropertiesActions
        Dim NestBlock, k As Integer

        NumberObjs = 1
        ReDim Objs(1)
        With Objs(1)
            .ObjectName = "game"
            .ObjectAlias = ""
            .Visible = False
            .Exists = True

            NestBlock = 0
            For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
                If NestBlock = 0 Then
                    If BeginsWith(Lines(i), "define ") Then
                        NestBlock = NestBlock + 1
                    ElseIf BeginsWith(Lines(i), "properties ") Then
                        AddToObjectProperties(RetrieveParameter(Lines(i), NullThread), NumberObjs, NullThread)
                    ElseIf BeginsWith(Lines(i), "type ") Then
                        .NumberTypesIncluded = .NumberTypesIncluded + 1
                        ReDim Preserve .TypesIncluded(.NumberTypesIncluded)
                        .TypesIncluded(.NumberTypesIncluded) = RetrieveParameter(Lines(i), NullThread)

                        PropertyData = GetPropertiesInType(RetrieveParameter(Lines(i), NullThread))
                        AddToObjectProperties(PropertyData.Properties, NumberObjs, NullThread)
                        For k = 1 To PropertyData.NumberActions
                            AddObjectAction(NumberObjs, PropertyData.Actions(k))
                        Next k
                    ElseIf BeginsWith(Lines(i), "action ") Then
                        AddToObjectActions(GetEverythingAfter(Lines(i), "action "), NumberObjs, NullThread)
                    End If
                Else
                    If Trim(Lines(i)) = "end define" Then
                        NestBlock = NestBlock - 1
                    End If
                End If
            Next i

        End With

    End Sub

    Private Sub SetUpMenus()

        Dim j, i, SCP As Integer
        Dim MenuExists As Boolean = False

        Dim menuTitle As String = ""
        Dim menuOptions As New Dictionary(Of String, String)

        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define menu ") Then

                If MenuExists Then
                    LogASLError("Can't load menu '" & RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread) & "' - only one menu can be added.", LOGTYPE_WARNINGERROR)
                Else
                    menuTitle = RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread)

                    For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                        If Trim(Lines(j)) <> "" Then
                            SCP = InStr(Lines(j), ":")
                            If SCP = 0 And Lines(j) <> "-" Then
                                LogASLError("No menu command specified in menu '" & menuTitle & "', item '" & Lines(j), LOGTYPE_WARNINGERROR)
                            Else
                                If Lines(j) = "-" Then
                                    menuOptions.Add("k" & CStr(j), "-")
                                Else
                                    menuOptions.Add(Trim(Mid(Lines(j), SCP + 1)), Trim(Left(Lines(j), SCP - 1)))
                                End If

                            End If
                        End If
                    Next j

                    If menuOptions.Count > 0 Then
                        MenuExists = True
                    End If
                End If
            End If
        Next i

        If MenuExists Then
            Dim windowMenu As New MenuData(menuTitle, menuOptions, False)
            m_player.SetWindowMenu(windowMenu)
        End If

    End Sub

    Private Sub SetUpOptions()

        Dim i As Integer
        Dim CurOpt As String

        For i = GetDefineBlock("options").StartLine + 1 To GetDefineBlock("options").EndLine - 1

            If BeginsWith(Lines(i), "panes ") Then
                CurOpt = LCase(Trim(GetEverythingAfter(Lines(i), "panes ")))
                m_player.SetPanesVisible(CurOpt)
            ElseIf BeginsWith(Lines(i), "abbreviations ") Then
                CurOpt = LCase(Trim(GetEverythingAfter(Lines(i), "abbreviations ")))
                If CurOpt = "off" Then goptAbbreviations = False Else goptAbbreviations = True
            End If
        Next i

    End Sub

    Private Sub SetUpRoomData()
        ' Sets up room data
        Dim NestedBlock As Integer
        Dim PropertyData As PropertiesActions
        Dim DefaultProperties As New PropertiesActions
        Dim DefaultExists As Boolean
        Dim ThisAction As ActionType
        Dim k, i, j As Integer

        ' see if define type <defaultroom> exists:
        DefaultExists = False
        For i = 1 To NumberSections
            If Trim(Lines(DefineBlocks(i).StartLine)) = "define type <defaultroom>" Then
                DefaultExists = True
                DefaultProperties = GetPropertiesInType("defaultroom")
                i = NumberSections
            End If
        Next i

        Dim PlaceData As String
        Dim SCP As Integer
        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define room ") Then
                NumberRooms = NumberRooms + 1
                ReDim Preserve Rooms(NumberRooms)

                NumberObjs = NumberObjs + 1
                ReDim Preserve Objs(NumberObjs)

                With Rooms(NumberRooms)
                    .RoomName = RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread)
                    Objs(NumberObjs).ObjectName = .RoomName
                    Objs(NumberObjs).IsRoom = True
                    Objs(NumberObjs).CorresRoom = .RoomName
                    Objs(NumberObjs).CorresRoomID = NumberRooms

                    .ObjID = NumberObjs

                    If GameASLVersion >= 410 Then
                        .Exits = New RoomExits(Me)
                        .Exits.ObjID = .ObjID
                    End If

                    ' *******************************************************************************
                    ' IF FURTHER CHANGES ARE MADE HERE, A NEW CREATEROOM SUB SHOULD BE CREATED, WHICH
                    ' WE CAN THEN CALL FROM EXECUTECREATE ALSO.
                    ' *******************************************************************************

                    NestedBlock = 0

                    If DefaultExists Then
                        AddToObjectProperties(DefaultProperties.Properties, NumberObjs, NullThread)
                        For k = 1 To DefaultProperties.NumberActions
                            AddObjectAction(NumberObjs, DefaultProperties.Actions(k))
                        Next k
                    End If

                    For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                        If BeginsWith(Lines(j), "define ") Then
                            'skip nested blocks
                            NestedBlock = 1
                            Do
                                j = j + 1
                                If BeginsWith(Lines(j), "define ") Then
                                    NestedBlock = NestedBlock + 1
                                ElseIf Trim(Lines(j)) = "end define" Then
                                    NestedBlock = NestedBlock - 1
                                End If
                            Loop Until NestedBlock = 0
                        End If

                        If GameASLVersion >= 280 And BeginsWith(Lines(j), "alias ") Then
                            .RoomAlias = RetrieveParameter(Lines(j), NullThread)
                            Objs(NumberObjs).ObjectAlias = .RoomAlias
                            If GameASLVersion >= 350 Then AddToObjectProperties("alias=" & .RoomAlias, NumberObjs, NullThread)
                        ElseIf GameASLVersion >= 280 And BeginsWith(Lines(j), "description ") Then
                            .Description = GetTextOrScript(GetEverythingAfter(Lines(j), "description "))
                            If GameASLVersion >= 350 Then
                                If .Description.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "description"
                                    ThisAction.Script = .Description.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("description=" & .Description.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "out ") Then
                            .out.Text = RetrieveParameter(Lines(j), NullThread)
                            .out.Script = Trim(Mid(Lines(j), InStr(Lines(j), ">") + 1))
                            If GameASLVersion >= 350 Then
                                If .out.Script <> "" Then
                                    ThisAction.ActionName = "out"
                                    ThisAction.Script = .out.Script
                                    AddObjectAction(NumberObjs, ThisAction)
                                End If

                                AddToObjectProperties("out=" & .out.Text, NumberObjs, NullThread)
                            End If
                        ElseIf BeginsWith(Lines(j), "east ") Then
                            .East = GetTextOrScript(GetEverythingAfter(Lines(j), "east "))
                            If GameASLVersion >= 350 Then
                                If .East.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "east"
                                    ThisAction.Script = .East.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("east=" & .East.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "west ") Then
                            .West = GetTextOrScript(GetEverythingAfter(Lines(j), "west "))
                            If GameASLVersion >= 350 Then
                                If .West.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "west"
                                    ThisAction.Script = .West.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("west=" & .West.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "north ") Then
                            .North = GetTextOrScript(GetEverythingAfter(Lines(j), "north "))
                            If GameASLVersion >= 350 Then
                                If .North.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "north"
                                    ThisAction.Script = .North.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("north=" & .North.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "south ") Then
                            .South = GetTextOrScript(GetEverythingAfter(Lines(j), "south "))
                            If GameASLVersion >= 350 Then
                                If .South.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "south"
                                    ThisAction.Script = .South.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("south=" & .South.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "northeast ") Then
                            .NorthEast = GetTextOrScript(GetEverythingAfter(Lines(j), "northeast "))
                            If GameASLVersion >= 350 Then
                                If .NorthEast.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "northeast"
                                    ThisAction.Script = .NorthEast.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("northeast=" & .NorthEast.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "northwest ") Then
                            .NorthWest = GetTextOrScript(GetEverythingAfter(Lines(j), "northwest "))
                            If GameASLVersion >= 350 Then
                                If .NorthWest.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "northwest"
                                    ThisAction.Script = .NorthWest.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("northwest=" & .NorthWest.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "southeast ") Then
                            .SouthEast = GetTextOrScript(GetEverythingAfter(Lines(j), "southeast "))
                            If GameASLVersion >= 350 Then
                                If .SouthEast.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "southeast"
                                    ThisAction.Script = .SouthEast.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("southeast=" & .SouthEast.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "southwest ") Then
                            .SouthWest = GetTextOrScript(GetEverythingAfter(Lines(j), "southwest "))
                            If GameASLVersion >= 350 Then
                                If .SouthWest.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "southwest"
                                    ThisAction.Script = .SouthWest.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("southwest=" & .SouthWest.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "up ") Then
                            .Up = GetTextOrScript(GetEverythingAfter(Lines(j), "up "))
                            If GameASLVersion >= 350 Then
                                If .Up.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "up"
                                    ThisAction.Script = .Up.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("up=" & .Up.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf BeginsWith(Lines(j), "down ") Then
                            .Down = GetTextOrScript(GetEverythingAfter(Lines(j), "down "))
                            If GameASLVersion >= 350 Then
                                If .Down.Type = TA_SCRIPT Then
                                    ThisAction.ActionName = "down"
                                    ThisAction.Script = .Down.Data
                                    AddObjectAction(NumberObjs, ThisAction)
                                Else
                                    AddToObjectProperties("down=" & .Down.Data, NumberObjs, NullThread)
                                End If
                            End If
                        ElseIf GameASLVersion >= 280 And BeginsWith(Lines(j), "indescription ") Then
                            .InDescription = RetrieveParameter(Lines(j), NullThread)
                            If GameASLVersion >= 350 Then AddToObjectProperties("indescription=" & .InDescription, NumberObjs, NullThread)
                        ElseIf GameASLVersion >= 280 And BeginsWith(Lines(j), "look ") Then
                            .Look = RetrieveParameter(Lines(j), NullThread)
                            If GameASLVersion >= 350 Then AddToObjectProperties("look=" & .Look, NumberObjs, NullThread)
                        ElseIf BeginsWith(Lines(j), "prefix ") Then
                            .Prefix = RetrieveParameter(Lines(j), NullThread)
                            If GameASLVersion >= 350 Then AddToObjectProperties("prefix=" & .Prefix, NumberObjs, NullThread)
                        ElseIf BeginsWith(Lines(j), "script ") Then
                            .Script = GetEverythingAfter(Lines(j), "script ")
                            ThisAction.ActionName = "script"
                            ThisAction.Script = .Script
                            AddObjectAction(NumberObjs, ThisAction)
                        ElseIf BeginsWith(Lines(j), "command ") Then
                            .NumberCommands = .NumberCommands + 1
                            ReDim Preserve .Commands(.NumberCommands)
                            .Commands(.NumberCommands).CommandText = RetrieveParameter(Lines(j), NullThread, False)
                            .Commands(.NumberCommands).CommandScript = Trim(Mid(Lines(j), InStr(Lines(j), ">") + 1))
                        ElseIf BeginsWith(Lines(j), "place ") Then
                            .NumberPlaces = .NumberPlaces + 1
                            ReDim Preserve .Places(.NumberPlaces)
                            PlaceData = RetrieveParameter(Lines(j), NullThread)
                            SCP = InStr(PlaceData, ";")
                            If SCP = 0 Then
                                .Places(.NumberPlaces).PlaceName = PlaceData
                            Else
                                .Places(.NumberPlaces).PlaceName = Trim(Mid(PlaceData, SCP + 1))
                                .Places(.NumberPlaces).Prefix = Trim(Left(PlaceData, SCP - 1))
                            End If
                            .Places(.NumberPlaces).Script = Trim(Mid(Lines(j), InStr(Lines(j), ">") + 1))
                        ElseIf BeginsWith(Lines(j), "use ") Then
                            .NumberUse = .NumberUse + 1
                            ReDim Preserve .use(.NumberUse)
                            .use(.NumberUse).Text = RetrieveParameter(Lines(j), NullThread)
                            .use(.NumberUse).Script = Trim(Mid(Lines(j), InStr(Lines(j), ">") + 1))
                        ElseIf BeginsWith(Lines(j), "properties ") Then
                            AddToObjectProperties(RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                        ElseIf BeginsWith(Lines(j), "type ") Then
                            Objs(NumberObjs).NumberTypesIncluded = Objs(NumberObjs).NumberTypesIncluded + 1
                            ReDim Preserve Objs(NumberObjs).TypesIncluded(Objs(NumberObjs).NumberTypesIncluded)
                            Objs(NumberObjs).TypesIncluded(Objs(NumberObjs).NumberTypesIncluded) = RetrieveParameter(Lines(j), NullThread)

                            PropertyData = GetPropertiesInType(RetrieveParameter(Lines(j), NullThread))
                            AddToObjectProperties(PropertyData.Properties, NumberObjs, NullThread)
                            For k = 1 To PropertyData.NumberActions
                                AddObjectAction(NumberObjs, PropertyData.Actions(k))
                            Next k
                        ElseIf BeginsWith(Lines(j), "action ") Then
                            AddToObjectActions(GetEverythingAfter(Lines(j), "action "), NumberObjs, NullThread)
                        ElseIf BeginsWith(Lines(j), "beforeturn ") Then
                            .BeforeTurnScript = .BeforeTurnScript & GetEverythingAfter(Lines(j), "beforeturn ") & vbCrLf
                        ElseIf BeginsWith(Lines(j), "afterturn ") Then
                            .AfterTurnScript = .AfterTurnScript & GetEverythingAfter(Lines(j), "afterturn ") & vbCrLf
                        End If
                    Next j
                End With
            End If
        Next i
    End Sub

    Private Sub SetUpSynonyms()
        ' Sets up synonyms when game is initialised

        Dim SynonymBlock As DefineBlock
        Dim EqualsSignPos As Integer
        Dim OriginalWordsList, ConvertWord As String
        Dim i, iCurPos, EndOfWord As Integer
        Dim ThisWord As String

        SynonymBlock = GetDefineBlock("synonyms")

        NumberSynonyms = 0

        If SynonymBlock.StartLine = 0 And SynonymBlock.EndLine = 0 Then
            Exit Sub
        End If

        For i = SynonymBlock.StartLine + 1 To SynonymBlock.EndLine - 1
            EqualsSignPos = InStr(Lines(i), "=")
            If EqualsSignPos <> 0 Then
                OriginalWordsList = Trim(Left(Lines(i), EqualsSignPos - 1))
                ConvertWord = Trim(Mid(Lines(i), EqualsSignPos + 1))

                'Go through each word in OriginalWordsList (sep.
                'by ";"):

                OriginalWordsList = OriginalWordsList & ";"
                iCurPos = 1

                Do
                    EndOfWord = InStr(iCurPos, OriginalWordsList, ";")
                    ThisWord = Trim(Mid(OriginalWordsList, iCurPos, EndOfWord - iCurPos))

                    If InStr(" " & ConvertWord & " ", " " & ThisWord & " ") > 0 Then
                        ' Recursive synonym
                        LogASLError("Recursive synonym detected: '" & ThisWord & "' converting to '" & ConvertWord & "'", LOGTYPE_WARNINGERROR)
                    Else
                        NumberSynonyms = NumberSynonyms + 1
                        ReDim Preserve Synonyms(NumberSynonyms)
                        Synonyms(NumberSynonyms).OriginalWord = ThisWord
                        Synonyms(NumberSynonyms).ConvertTo = ConvertWord
                    End If
                    iCurPos = EndOfWord + 1
                Loop Until iCurPos >= Len(OriginalWordsList)
            End If

        Next i

    End Sub

    Private Sub SetUpTimers()
        Dim i, j As Integer

        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define timer ") Then
                NumberTimers = NumberTimers + 1
                ReDim Preserve Timers(NumberTimers)
                Timers(NumberTimers).TimerName = RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread)
                Timers(NumberTimers).TimerActive = False

                For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                    If BeginsWith(Lines(j), "interval ") Then
                        Timers(NumberTimers).TimerInterval = CInt(RetrieveParameter(Lines(j), NullThread))
                    ElseIf BeginsWith(Lines(j), "action ") Then
                        Timers(NumberTimers).TimerAction = GetEverythingAfter(Lines(j), "action ")
                    ElseIf Trim(LCase(Lines(j))) = "enabled" Then
                        Timers(NumberTimers).TimerActive = True
                    ElseIf Trim(LCase(Lines(j))) = "disabled" Then
                        Timers(NumberTimers).TimerActive = False
                    End If
                Next j
            End If
        Next i

    End Sub

    Private Sub SetUpTurnScript()
        Dim gameblock As DefineBlock
        Dim i As Integer
        gameblock = GetDefineBlock("game")

        BeforeTurnScript = ""
        AfterTurnScript = ""

        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), "beforeturn ") Then
                BeforeTurnScript = BeforeTurnScript & GetEverythingAfter(Trim(Lines(i)), "beforeturn ") & vbCrLf
            ElseIf BeginsWith(Lines(i), "afterturn ") Then
                AfterTurnScript = AfterTurnScript & GetEverythingAfter(Trim(Lines(i)), "afterturn ") & vbCrLf
            End If
        Next i
    End Sub

    Private Sub SetUpUserDefinedPlayerErrors()
        ' goes through "define game" block and sets stored error
        ' messages accordingly

        Dim gameblock As DefineBlock
        Dim sErrorName, sErrorMsg As String
        Dim iCurrentError, i As Integer
        Dim ExamineIsCustomised As Boolean
        Dim ErrorInfo As String
        Dim SemiColonPos As Integer

        gameblock = GetDefineBlock("game")

        ExamineIsCustomised = False

        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), "error ") Then
                ErrorInfo = RetrieveParameter(Lines(i), NullThread, False)
                SemiColonPos = InStr(ErrorInfo, ";")

                sErrorName = Left(ErrorInfo, SemiColonPos - 1)
                sErrorMsg = Trim(Mid(ErrorInfo, SemiColonPos + 1))

                iCurrentError = 0
                Select Case sErrorName
                    Case "badcommand"
                        iCurrentError = ERROR_BADCOMMAND
                    Case "badgo"
                        iCurrentError = ERROR_BADGO
                    Case "badgive"
                        iCurrentError = ERROR_BADGIVE
                    Case "badcharacter"
                        iCurrentError = ERROR_BADCHARACTER
                    Case "noitem"
                        iCurrentError = ERROR_NOITEM
                    Case "itemunwanted"
                        iCurrentError = ERROR_ITEMUNWANTED
                    Case "badlook"
                        iCurrentError = ERROR_BADLOOK
                    Case "badthing"
                        iCurrentError = ERROR_BADTHING
                    Case "defaultlook"
                        iCurrentError = ERROR_DEFAULTLOOK
                    Case "defaultspeak"
                        iCurrentError = ERROR_DEFAULTSPEAK
                    Case "baditem"
                        iCurrentError = ERROR_BADITEM
                    Case "baddrop"
                        iCurrentError = ERROR_BADDROP
                    Case "defaultake"
                        If GameASLVersion <= 280 Then
                            iCurrentError = ERROR_BADTAKE
                        Else
                            iCurrentError = ERROR_DEFAULTTAKE
                        End If
                    Case "baduse"
                        iCurrentError = ERROR_BADUSE
                    Case "defaultuse"
                        iCurrentError = ERROR_DEFAULTUSE
                    Case "defaultout"
                        iCurrentError = ERROR_DEFAULTOUT
                    Case "badplace"
                        iCurrentError = ERROR_BADPLACE
                    Case "badexamine"
                        If GameASLVersion >= 310 Then
                            iCurrentError = ERROR_BADEXAMINE
                        End If
                    Case "defaultexamine"
                        iCurrentError = ERROR_DEFAULTEXAMINE
                        ExamineIsCustomised = True
                    Case "badtake"
                        iCurrentError = ERROR_BADTAKE
                    Case "cantdrop"
                        iCurrentError = ERROR_CANTDROP
                    Case "defaultdrop"
                        iCurrentError = ERROR_DEFAULTDROP
                    Case "badpronoun"
                        iCurrentError = ERROR_BADPRONOUN
                    Case "alreadyopen"
                        iCurrentError = ERROR_ALREADYOPEN
                    Case "alreadyclosed"
                        iCurrentError = ERROR_ALREADYCLOSED
                    Case "cantopen"
                        iCurrentError = ERROR_CANTOPEN
                    Case "cantclose"
                        iCurrentError = ERROR_CANTCLOSE
                    Case "defaultopen"
                        iCurrentError = ERROR_DEFAULTOPEN
                    Case "defaultclose"
                        iCurrentError = ERROR_DEFAULTCLOSE
                    Case "badput"
                        iCurrentError = ERROR_BADPUT
                    Case "cantput"
                        iCurrentError = ERROR_CANTPUT
                    Case "defaultput"
                        iCurrentError = ERROR_DEFAULTPUT
                    Case "cantremove"
                        iCurrentError = ERROR_CANTREMOVE
                    Case "alreadyput"
                        iCurrentError = ERROR_ALREADYPUT
                    Case "defaultremove"
                        iCurrentError = ERROR_DEFAULTREMOVE
                    Case "locked"
                        iCurrentError = ERROR_LOCKED
                    Case "defaultwait"
                        iCurrentError = ERROR_DEFAULTWAIT
                    Case "alreadytaken"
                        iCurrentError = ERROR_ALREADYTAKEN
                End Select

                PlayerErrorMessageString(iCurrentError) = sErrorMsg
                If iCurrentError = ERROR_DEFAULTLOOK And Not ExamineIsCustomised Then
                    ' If we're setting the default look message, and we've not already customised the
                    ' default examine message, then set the default examine message to the same thing.
                    PlayerErrorMessageString(ERROR_DEFAULTEXAMINE) = sErrorMsg
                End If
            End If
        Next i

    End Sub

    Private Sub SetVisibility(ByRef ThingString As String, ByRef ThingType As Integer, ByRef ThingVisible As Boolean, ByRef Thread As ThreadData)
        ' Sets visibilty of objects and characters

        Dim i, AtPos As Integer
        Dim CRoom, CName As String

        Dim FoundObject As Boolean
        If GameASLVersion >= 280 Then
            FoundObject = False

            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(ThingString) Then
                    Objs(i).Visible = ThingVisible
                    If ThingVisible Then
                        AddToObjectProperties("not invisible", i, Thread)
                    Else
                        AddToObjectProperties("invisible", i, Thread)
                    End If

                    i = NumberObjs + 1
                    FoundObject = True
                End If
            Next i

            If Not FoundObject Then
                LogASLError("Not found object '" & ThingString & "'", LOGTYPE_WARNINGERROR)
            End If
        Else
            ' split ThingString into character name and room
            ' (thingstring of form name@room)

            AtPos = InStr(ThingString, "@")

            ' If no room specified, current room presumed
            If AtPos = 0 Then
                CRoom = CurrentRoom
                CName = ThingString
            Else
                CName = Trim(Left(ThingString, AtPos - 1))
                CRoom = Trim(Right(ThingString, Len(ThingString) - AtPos))
            End If

            If ThingType = QUEST_CHARACTER Then
                For i = 1 To NumberChars
                    If LCase(Chars(i).ContainerRoom) = LCase(CRoom) And LCase(Chars(i).ObjectName) = LCase(CName) Then
                        Chars(i).Visible = ThingVisible
                        i = NumberChars + 1
                    End If
                Next i
            ElseIf ThingType = QUEST_OBJECT Then
                For i = 1 To NumberObjs
                    If LCase(Objs(i).ContainerRoom) = LCase(CRoom) And LCase(Objs(i).ObjectName) = LCase(CName) Then
                        Objs(i).Visible = ThingVisible
                        i = NumberObjs + 1
                    End If
                Next i
            End If
        End If

        UpdateObjectList(Thread)
    End Sub

    Private Sub ShowPictureInText(ByRef sFileName As String)
        If Not m_useStaticFrameForPictures Then
            m_player.ShowPicture(sFileName)
        Else
            ' Workaround for a particular game which expects pictures to be in a popup window -
            ' use the static picture frame feature so that image is not cleared
            m_player.SetPanelContents("<img src=""" + m_player.GetURL(sFileName) + """ onload=""setPanelHeight()""/>")
        End If
    End Sub

    Private Sub ShowRoomInfoV2(ByRef Room As String)
        ' ShowRoomInfo for Quest 2.x games
        Dim i As Integer

        Dim RoomDisplayText As String = ""
        Dim DescTagExist As Boolean
        Dim gameblock As DefineBlock
        Dim CharsViewable As String
        Dim CharsFound As Integer
        Dim PANF, Prefix, PA, InDesc As String
        Dim AliasName As String = ""
        Dim CharList As String
        Dim FoundLastComma, CommaPos, NewCommaPos As Integer
        Dim NoFormatObjsViewable As String
        Dim ObjsViewable As String = ""
        Dim ObjsFound As Integer
        Dim ObjListString, NFObjListString As String
        Dim PossDir, NSEW, Doorways, Places, PL As String
        Dim AliasOut As String = ""
        Dim PLNF As String
        Dim DescLine As String = ""
        Dim LastComma, OldLastComma As Integer
        Dim DefBlk As Integer
        Dim LookString As String = ""

        gameblock = GetDefineBlock("game")
        CurrentRoom = Room

        'find the room
        Dim roomblock As DefineBlock
        roomblock = DefineBlockParam("room", Room)
        Dim FinishedFindingCommas As Boolean

        CharsViewable = ""
        CharsFound = 0

        'see if room has an alias
        For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
            If BeginsWith(Lines(i), "alias") Then
                AliasName = RetrieveParameter(Lines(i), NullThread)
                i = roomblock.EndLine
            End If
        Next i
        If AliasName = "" Then AliasName = Room

        'see if room has a prefix
        Prefix = FindStatement(roomblock, "prefix")
        If Prefix = "" Then
            PA = "|cr" & AliasName & "|cb"
            PANF = AliasName ' No formatting version, for label
        Else
            PA = Prefix & " |cr" & AliasName & "|cb"
            PANF = Prefix & " " & AliasName
        End If

        'print player's location
        'find indescription line:
        InDesc = "unfound"
        For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
            If BeginsWith(Lines(i), "indescription") Then
                InDesc = Trim(RetrieveParameter(Lines(i), NullThread))
                i = roomblock.EndLine
            End If
        Next i

        If InDesc <> "unfound" Then
            ' Print player's location according to indescription:
            If Right(InDesc, 1) = ":" Then
                ' if line ends with a colon, add place name:
                RoomDisplayText = RoomDisplayText & Left(InDesc, Len(InDesc) - 1) & " " & PA & "." & vbCrLf
            Else
                ' otherwise, just print the indescription line:
                RoomDisplayText = RoomDisplayText & InDesc & vbCrLf
            End If
        Else
            ' if no indescription line, print the default.
            RoomDisplayText = RoomDisplayText & "You are in " & PA & "." & vbCrLf
        End If

        m_player.LocationUpdated(PANF)

        SetStringContents("quest.formatroom", PANF, NullThread)

        'FIND CHARACTERS ===

        ' go through Chars() array
        For i = 1 To NumberChars
            If Chars(i).ContainerRoom = Room And Chars(i).Exists And Chars(i).Visible Then
                CharsViewable = CharsViewable & Chars(i).Prefix & "|b" & Chars(i).ObjectName & "|xb" & Chars(i).Suffix & ", "
                CharsFound = CharsFound + 1
            End If
        Next i

        If CharsFound = 0 Then
            CharsViewable = "There is nobody here."
            SetStringContents("quest.characters", "", NullThread)
        Else
            'chop off final comma and add full stop (.)
            CharList = Left(CharsViewable, Len(CharsViewable) - 2)
            SetStringContents("quest.characters", CharList, NullThread)

            'if more than one character, add "and" before
            'last one:
            CommaPos = InStr(CharList, ",")
            If CommaPos <> 0 Then
                FoundLastComma = 0
                Do
                    NewCommaPos = InStr(CommaPos + 1, CharList, ",")
                    If NewCommaPos = 0 Then
                        FoundLastComma = 1
                    Else
                        CommaPos = NewCommaPos
                    End If
                Loop Until FoundLastComma = 1

                CharList = Trim(Left(CharList, CommaPos - 1)) & " and " & Trim(Mid(CharList, CommaPos + 1))
            End If

            CharsViewable = "You can see " & CharList & " here."
        End If

        RoomDisplayText = RoomDisplayText & CharsViewable & vbCrLf

        'FIND OBJECTS

        NoFormatObjsViewable = ""

        For i = 1 To NumberObjs
            If Objs(i).ContainerRoom = Room And Objs(i).Exists And Objs(i).Visible Then
                ObjsViewable = ObjsViewable & Objs(i).Prefix & "|b" & Objs(i).ObjectName & "|xb" & Objs(i).Suffix & ", "
                NoFormatObjsViewable = NoFormatObjsViewable & Objs(i).Prefix & Objs(i).ObjectName & ", "

                ObjsFound = ObjsFound + 1
            End If
        Next i

        Dim bFinLoop As Boolean
        If ObjsFound <> 0 Then
            ObjListString = Left(ObjsViewable, Len(ObjsViewable) - 2)
            NFObjListString = Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2)

            CommaPos = InStr(ObjListString, ",")
            If CommaPos <> 0 Then
                Do
                    NewCommaPos = InStr(CommaPos + 1, ObjListString, ",")
                    If NewCommaPos = 0 Then
                        bFinLoop = True
                    Else
                        CommaPos = NewCommaPos
                    End If
                Loop Until bFinLoop

                ObjListString = Trim(Left(ObjListString, CommaPos - 1) & " and " & Trim(Mid(ObjListString, CommaPos + 1)))
            End If

            ObjsViewable = "There is " & ObjListString & " here."
            SetStringContents("quest.objects", Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2), NullThread)
            SetStringContents("quest.formatobjects", ObjListString, NullThread)
            RoomDisplayText = RoomDisplayText & ObjsViewable & vbCrLf
        Else
            SetStringContents("quest.objects", "", NullThread)
            SetStringContents("quest.formatobjects", "", NullThread)
        End If

        'FIND DOORWAYS
        Doorways = ""
        NSEW = ""
        Places = ""
        PossDir = ""

        For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
            If BeginsWith(Lines(i), "out") Then
                Doorways = RetrieveParameter(Lines(i), NullThread)
            End If

            If BeginsWith(Lines(i), "north ") Then
                NSEW = NSEW & "|bnorth|xb, "
                PossDir = PossDir & "n"
            ElseIf BeginsWith(Lines(i), "south ") Then
                NSEW = NSEW & "|bsouth|xb, "
                PossDir = PossDir & "s"
            ElseIf BeginsWith(Lines(i), "east ") Then
                NSEW = NSEW & "|beast|xb, "
                PossDir = PossDir & "e"
            ElseIf BeginsWith(Lines(i), "west ") Then
                NSEW = NSEW & "|bwest|xb, "
                PossDir = PossDir & "w"
            ElseIf BeginsWith(Lines(i), "northeast ") Then
                NSEW = NSEW & "|bnortheast|xb, "
                PossDir = PossDir & "a"
            ElseIf BeginsWith(Lines(i), "northwest ") Then
                NSEW = NSEW & "|bnorthwest|xb, "
                PossDir = PossDir & "b"
            ElseIf BeginsWith(Lines(i), "southeast ") Then
                NSEW = NSEW & "|bsoutheast|xb, "
                PossDir = PossDir & "c"
            ElseIf BeginsWith(Lines(i), "southwest ") Then
                NSEW = NSEW & "|bsouthwest|xb, "
                PossDir = PossDir & "d"
            End If

            If BeginsWith(Lines(i), "place") Then
                'remove any prefix semicolon from printed text
                PL = RetrieveParameter(Lines(i), NullThread)
                PLNF = PL 'Used in object list - no formatting or prefix
                If InStr(PL, ";") > 0 Then
                    PLNF = Right(PL, Len(PL) - (InStr(PL, ";") + 1))
                    PL = Trim(Left(PL, InStr(PL, ";") - 1)) & " |b" & Right(PL, Len(PL) - (InStr(PL, ";") + 1)) & "|xb"
                Else
                    PL = "|b" & PL & "|xb"
                End If
                Places = Places & PL & ", "

            End If

        Next i

        Dim outside As DefineBlock
        If Doorways <> "" Then
            'see if outside has an alias
            outside = DefineBlockParam("room", Doorways)
            For i = outside.StartLine + 1 To outside.EndLine - 1
                If BeginsWith(Lines(i), "alias") Then
                    AliasOut = RetrieveParameter(Lines(i), NullThread)
                    i = outside.EndLine
                End If
            Next i
            If AliasOut = "" Then AliasOut = Doorways

            RoomDisplayText = RoomDisplayText & "You can go out to " & AliasOut & "." & vbCrLf
            PossDir = PossDir & "o"
            SetStringContents("quest.doorways.out", AliasOut, NullThread)
        Else
            SetStringContents("quest.doorways.out", "", NullThread)
        End If

        Dim bFinNSEW As Boolean
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

            RoomDisplayText = RoomDisplayText & "You can go " & NSEW & "." & vbCrLf
            SetStringContents("quest.doorways.dirs", NSEW, NullThread)
        Else
            SetStringContents("quest.doorways.dirs", "", NullThread)
        End If

        UpdateDirButtons(PossDir, NullThread)

        If Places <> "" Then
            'strip final comma
            Places = Left(Places, Len(Places) - 2)

            'if there is still a comma here, there is more than
            'one place, so add the word "or" before the last one.
            If InStr(Places, ",") > 0 Then
                LastComma = 0
                FinishedFindingCommas = False
                Do
                    OldLastComma = LastComma
                    LastComma = InStr(LastComma + 1, Places, ",")
                    If LastComma = 0 Then
                        FinishedFindingCommas = True
                        LastComma = OldLastComma
                    End If
                Loop Until FinishedFindingCommas

                Places = Left(Places, LastComma) & " or" & Right(Places, Len(Places) - LastComma)
            End If

            RoomDisplayText = RoomDisplayText & "You can go to " & Places & "." & vbCrLf
            SetStringContents("quest.doorways.places", Places, NullThread)
        Else
            SetStringContents("quest.doorways.places", "", NullThread)
        End If

        'Print RoomDisplayText if there is no "description" tag,
        'otherwise execute the description tag information:

        ' First, look in the "define room" block:
        DescTagExist = False
        For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
            If BeginsWith(Lines(i), "description ") Then
                DescLine = Lines(i)
                DescTagExist = True
                i = roomblock.EndLine
            End If
        Next i

        If DescTagExist = False Then
            'Look in the "define game" block:
            For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
                If BeginsWith(Lines(i), "description ") Then
                    DescLine = Lines(i)
                    DescTagExist = True
                    i = gameblock.EndLine
                End If
            Next i
        End If

        If DescTagExist = False Then
            'Remove final vbCrLf:
            RoomDisplayText = Left(RoomDisplayText, Len(RoomDisplayText) - 2)
            Print(RoomDisplayText, NullThread)
        Else
            'execute description tag:
            'If no script, just print the tag's parameter.
            'Otherwise, execute it as ASL script:

            DescLine = GetEverythingAfter(Trim(DescLine), "description ")
            If Left(DescLine, 1) = "<" Then
                Print(RetrieveParameter(DescLine, NullThread), NullThread)
            Else
                ExecuteScript(DescLine, NullThread)
            End If
        End If

        UpdateObjectList(NullThread)

        DefBlk = 0

        For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
            ' don't get the 'look' statements in nested define blocks
            If BeginsWith(Lines(i), "define") Then DefBlk = DefBlk + 1
            If BeginsWith(Lines(i), "end define") Then DefBlk = DefBlk - 1
            If BeginsWith(Lines(i), "look") And DefBlk = 0 Then
                LookString = RetrieveParameter(Lines(i), NullThread)
                i = roomblock.EndLine
            End If
        Next i

        If LookString <> "" Then Print(LookString, NullThread)

    End Sub

    Private Sub Speak(ByRef Text As String)
        m_player.Speak(Text)
    End Sub

    Private Sub AddToObjectList(objList As List(Of ListData), exitList As List(Of ListData), ObjName As String, ByRef ObjType As Integer, Optional ByRef DisplayType As String = "")
        ' Adds object, characters and places to the list on the
        ' main Quest window.

        NumInObjList = NumInObjList + 1
        ReDim Preserve ObjectList(NumInObjList)

        ObjName = CapFirst(ObjName)

        ObjectList(NumInObjList).ObjectName = ObjName
        ObjectList(NumInObjList).ObjectType = ObjType

        If DisplayType = "" Then
            DisplayType = ConvertObjectType(ObjType)
        End If

        ObjectList(NumInObjList).DisplayObjectType = DisplayType

        If ObjType = QUEST_ROOM Then
            objList.Add(New ListData(ObjName, m_listVerbs(ListType.ExitsList)))
            exitList.Add(New ListData(ObjName, m_listVerbs(ListType.ExitsList)))
        Else
            ' TO DO: DisplayType is not supported, should it be...?
            objList.Add(New ListData(ObjName, m_listVerbs(ListType.ObjectsList)))
        End If

    End Sub

    Private Function ConvertObjectType(ByRef ObjectType As Integer) As String

        Select Case ObjectType
            Case QUEST_OBJECT
                ConvertObjectType = "object"
            Case QUEST_CHARACTER
                ConvertObjectType = "character"
            Case QUEST_ROOM
                ConvertObjectType = "place"
        End Select

        Return ""

    End Function

    Private Sub ExecExec(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        Dim EX, ExecLine, R As String
        Dim SemiColonPos As Integer

        If Thread.CancelExec Then Exit Sub

        ExecLine = RetrieveParameter(ScriptLine, Thread)

        Dim NewThread As ThreadData
        NewThread = Thread

        NewThread.StackCounter = NewThread.StackCounter + 1

        If NewThread.StackCounter > 500 Then
            LogASLError("Out of stack space running '" & ScriptLine & "' - infinite loop?", LOGTYPE_WARNINGERROR)
            Thread.CancelExec = True
            Exit Sub
        End If

        If GameASLVersion >= 310 Then
            NewThread.AllowRealNamesInCommand = True
        End If

        If InStr(ExecLine, ";") = 0 Then
            On Error GoTo errhandle
            ExecCommand(ExecLine, NewThread, False)
            On Error GoTo 0
        Else
            SemiColonPos = InStr(ExecLine, ";")
            EX = Trim(Left(ExecLine, SemiColonPos - 1))
            R = Trim(Mid(ExecLine, SemiColonPos + 1))
            If R = "normal" Then
                ExecCommand(EX, NewThread, False, False)
            Else
                LogASLError("Unrecognised post-command parameter in " & Trim(ScriptLine), LOGTYPE_WARNINGERROR)
            End If
        End If

        Exit Sub

errhandle:
        If Err.Number = 28 Then
            LogASLError("Out of stack space running '" & ScriptLine & "' - infinite loop?", LOGTYPE_WARNINGERROR)
        Else
            LogASLError("Internal error " & Err.Number & " running '" & ScriptLine & "'", LOGTYPE_WARNINGERROR)
        End If

        Thread.CancelExec = True

    End Sub

    Private Sub ExecSetString(ByRef StringInfo As String, ByRef Thread As ThreadData)
        ' Sets string contents from a script parameter.
        ' Eg <string1;contents> sets string variable string1
        ' to "contents"

        Dim iSemiColonPos As Integer
        Dim sVarName As String
        Dim sVarCont As String
        Dim ArrayIndex As Integer

        iSemiColonPos = InStr(StringInfo, ";")
        sVarName = Trim(Left(StringInfo, iSemiColonPos - 1))
        sVarCont = Mid(StringInfo, iSemiColonPos + 1)

        If IsNumeric(sVarName) Then
            LogASLError("Invalid string name '" & sVarName & "' - string names cannot be numeric", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        If GameASLVersion >= 281 Then
            sVarCont = Trim(sVarCont)
            If Left(sVarCont, 1) = "[" And Right(sVarCont, 1) = "]" Then
                sVarCont = Mid(sVarCont, 2, Len(sVarCont) - 2)
            End If
        End If

        ArrayIndex = GetArrayIndex(sVarName, Thread)

        SetStringContents(sVarName, sVarCont, Thread, ArrayIndex)

    End Sub

    Private Function ExecUserCommand(ByRef thecommand As String, ByRef Thread As ThreadData, Optional ByRef LibCommands As Boolean = False) As Boolean
        'Executes a user-defined command. If unavailable, returns
        'false.
        Dim FoundCommand As Boolean
        Dim RoomID As Integer
        Dim roomblock As DefineBlock
        Dim CurCmd, CommandList As String
        Dim ScriptToExecute As String = ""
        Dim EndPos, i As Integer
        Dim CommandTag As String
        Dim CommandLine As String = ""
        Dim ScriptPos As Integer
        FoundCommand = False

        'First, check for a command in the current room block
        RoomID = GetRoomID(CurrentRoom, Thread)

        ' RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        If RoomID <> 0 Then
            With Rooms(RoomID)
                For i = 1 To .NumberCommands
                    CommandList = .Commands(i).CommandText
                    Do
                        EndPos = InStr(CommandList, ";")
                        If EndPos = 0 Then
                            CurCmd = CommandList
                        Else
                            CurCmd = Trim(Left(CommandList, EndPos - 1))
                            CommandList = Trim(Mid(CommandList, EndPos + 1))
                        End If

                        If IsCompatible(LCase(thecommand), LCase(CurCmd)) Then
                            CommandLine = CurCmd
                            ScriptToExecute = .Commands(i).CommandScript
                            FoundCommand = True
                            i = .NumberCommands
                            Exit Do
                        End If
                    Loop Until EndPos = 0
                Next i
            End With
        End If

        If Not LibCommands Then
            CommandTag = "command"
        Else
            CommandTag = "lib command"
        End If

        If Not FoundCommand Then
            ' Check "define game" block
            roomblock = GetDefineBlock("game")
            For i = roomblock.StartLine + 1 To roomblock.EndLine - 1
                If BeginsWith(Lines(i), CommandTag) Then

                    CommandList = RetrieveParameter(Lines(i), Thread, False)
                    Do
                        EndPos = InStr(CommandList, ";")
                        If EndPos = 0 Then
                            CurCmd = CommandList
                        Else
                            CurCmd = Trim(Left(CommandList, EndPos - 1))
                            CommandList = Trim(Mid(CommandList, EndPos + 1))
                        End If

                        If IsCompatible(LCase(thecommand), LCase(CurCmd)) Then
                            CommandLine = CurCmd
                            ScriptPos = InStr(Lines(i), ">") + 1
                            ScriptToExecute = Trim(Mid(Lines(i), ScriptPos))
                            FoundCommand = True
                            i = roomblock.EndLine
                            Exit Do
                        End If
                    Loop Until EndPos = 0
                End If
            Next i
        End If

        If FoundCommand Then
            If GetCommandParameters(thecommand, CommandLine, Thread) Then
                ExecuteScript(ScriptToExecute, Thread)
            End If
        End If

        ExecUserCommand = FoundCommand
    End Function

    Private Sub ExecuteChoose(ByRef choicesection As String, ByRef Thread As ThreadData)
        ExecuteScript(SetUpChoiceForm(choicesection, Thread), Thread)
    End Sub

    Private Function GetCommandParameters(TestLine As String, RequiredLine As String, ByRef Thread As ThreadData) As Boolean
        'Gets parameters from line. For example, if RequiredLine
        'is "read #1#" and TestLine is "read sign", #1# returns
        '"sign".

        ' Returns FALSE if #@object# form used and object doesn't
        ' exist.

        Dim CurrentReqLinePos, NextVarPos As Integer
        Dim CheckChunk As String
        Dim CurrentTestLinePos As Integer
        Dim FinishedProcessing As Boolean
        Dim CurrentVariable As String = ""
        Dim ChunkBegin, ChunkEnd As Integer
        Dim ChunksBegin() As Integer
        Dim ChunksEnd() As Integer
        Dim NumberChunks As Integer
        Dim VarName() As String
        Dim Var2Pos, ArrayIndex As Integer
        Dim CurChunk As String
        Dim ObjID As Integer
        Dim success As Boolean
        Dim i As Integer

        ' Add dots before and after both strings. This fudge
        ' stops problems caused when variables are right at the
        ' beginning or end of a line.
        ' PostScript: well, it used to, I'm not sure if it's really
        ' required now though....
        ' As of Quest 4.0 we use the ¦ character rather than a dot.
        TestLine = "¦" & Trim(TestLine) & "¦"
        RequiredLine = "¦" & RequiredLine & "¦"

        'Go through RequiredLine in chunks going up to variables.
        CurrentReqLinePos = 1
        CurrentTestLinePos = 1
        FinishedProcessing = False
        ChunkEnd = 0
        NumberChunks = 0
        Do
            NextVarPos = InStr(CurrentReqLinePos, RequiredLine, "#")
            If NextVarPos = 0 Then
                FinishedProcessing = True
                NextVarPos = Len(RequiredLine) + 1
            Else
                Var2Pos = InStr(NextVarPos + 1, RequiredLine, "#")
                CurrentVariable = Mid(RequiredLine, NextVarPos + 1, (Var2Pos - 1) - NextVarPos)
            End If

            CheckChunk = Mid(RequiredLine, CurrentReqLinePos, (NextVarPos - 1) - (CurrentReqLinePos - 1))
            ChunkBegin = InStr(CurrentTestLinePos, LCase(TestLine), LCase(CheckChunk))
            ChunkEnd = ChunkBegin + Len(CheckChunk)

            NumberChunks = NumberChunks + 1
            ReDim Preserve ChunksBegin(NumberChunks)
            ReDim Preserve ChunksEnd(NumberChunks)
            ReDim Preserve VarName(NumberChunks)
            ChunksBegin(NumberChunks) = ChunkBegin
            ChunksEnd(NumberChunks) = ChunkEnd
            VarName(NumberChunks) = CurrentVariable

            'Get to end of variable name
            CurrentReqLinePos = Var2Pos + 1

            CurrentTestLinePos = ChunkEnd
        Loop Until FinishedProcessing

        success = True

        'Return values to string variable
        For i = 1 To NumberChunks - 1
            ' If VarName contains array name, change to index number
            If InStr(VarName(i), "[") > 0 Then
                ArrayIndex = GetArrayIndex(VarName(i), Thread)
            Else
                ArrayIndex = 0
            End If

            CurChunk = Mid(TestLine, ChunksEnd(i), ChunksBegin(i + 1) - ChunksEnd(i))

            If BeginsWith(VarName(i), "@") Then
                VarName(i) = GetEverythingAfter(VarName(i), "@")
                ObjID = Disambiguate(CurChunk, CurrentRoom & ";" & "inventory", Thread)

                If ObjID = -1 Then
                    If GameASLVersion >= 391 Then
                        PlayerErrorMessage(ERROR_BADTHING, Thread)
                    Else
                        PlayerErrorMessage(ERROR_BADITEM, Thread)
                    End If
                    ' The Mid$(...,2) and Left$(...,2) removes the initial/final "."
                    BadCmdBefore = Mid(Trim(Left(TestLine, ChunksEnd(i) - 1)), 2)
                    BadCmdAfter = Trim(Mid(TestLine, ChunksBegin(i + 1)))
                    BadCmdAfter = Left(BadCmdAfter, Len(BadCmdAfter) - 1)
                    success = False
                ElseIf ObjID = -2 Then
                    BadCmdBefore = Trim(Left(TestLine, ChunksEnd(i) - 1))
                    BadCmdAfter = Trim(Mid(TestLine, ChunksBegin(i + 1)))
                    success = False
                Else
                    SetStringContents(VarName(i), Objs(ObjID).ObjectName, Thread, ArrayIndex)
                End If
            Else
                SetStringContents(VarName(i), CurChunk, Thread, ArrayIndex)
            End If
        Next i

        GetCommandParameters = success

    End Function

    Private Function GetGender(ByRef CharacterName As String, ByRef Capitalise As Boolean, ByRef Thread As ThreadData) As String
        Dim G, GL As String

        If GameASLVersion >= 281 Then
            G = Objs(GetObjectIDNoAlias(CharacterName)).Gender
        Else
            GL = RetrLine("character", CharacterName, "gender", Thread)

            If GL = "<unfound>" Then
                G = "it "
            Else
                G = RetrieveParameter(GL, Thread) & " "
            End If
        End If

        If Capitalise = True Then G = UCase(Left(G, 1)) & Right(G, Len(G) - 1)
        GetGender = G

    End Function

    Private Function GetStringContents(ByRef StringName As String, ByRef Thread As ThreadData) As String

        Dim bStringExists As Boolean
        Dim iStringNumber As Integer
        bStringExists = False
        Dim BeginPos, ArrayIndex, EndPos As Integer
        Dim ArrayIndexData As String
        Dim ColonPos As Integer
        Dim B1, B2 As Integer
        Dim ObjName As String
        Dim PropName As String
        Dim ReturnAlias As Boolean
        Dim i As Integer
        ReturnAlias = False
        ArrayIndex = 0

        ' Check for property shortcut
        ColonPos = InStr(StringName, ":")
        If ColonPos <> 0 Then
            ObjName = Trim(Left(StringName, ColonPos - 1))
            PropName = Trim(Mid(StringName, ColonPos + 1))

            B1 = InStr(ObjName, "(")
            If B1 <> 0 Then
                B2 = InStr(B1, ObjName, ")")
                If B2 <> 0 Then
                    ObjName = GetStringContents(Mid(ObjName, B1 + 1, (B2 - B1) - 1), Thread)
                End If
            End If

            GetStringContents = GetObjectProperty(PropName, GetObjectIDNoAlias(ObjName))
        Else
            If Left(StringName, 1) = "@" Then
                ReturnAlias = True
                StringName = Mid(StringName, 2)
            End If

            If InStr(StringName, "[") <> 0 And InStr(StringName, "]") <> 0 Then
                BeginPos = InStr(StringName, "[")
                EndPos = InStr(StringName, "]")
                ArrayIndexData = Mid(StringName, BeginPos + 1, (EndPos - BeginPos) - 1)
                If IsNumeric(ArrayIndexData) Then
                    ArrayIndex = CInt(ArrayIndexData)
                Else
                    ArrayIndex = CInt(GetNumericContents(ArrayIndexData, Thread))
                    If ArrayIndex = -32767 Then
                        GetStringContents = ""
                        LogASLError("Array index in '" & StringName & "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)", LOGTYPE_WARNINGERROR)
                        Exit Function
                    End If
                End If
                StringName = Left(StringName, BeginPos - 1)
            End If

            ' First, see if the string already exists. If it does,
            ' get its contents. If not, generate an error.

            If NumberStringVariables > 0 Then
                For i = 1 To NumberStringVariables
                    If LCase(StringVariable(i).VariableName) = LCase(StringName) Then
                        iStringNumber = i
                        bStringExists = True
                        i = NumberStringVariables
                    End If
                Next i
            End If

            If bStringExists = False Then
                LogASLError("No string variable '" & StringName & "' defined.", LOGTYPE_WARNINGERROR)
                GetStringContents = ""
                Exit Function
            End If

            If ArrayIndex > StringVariable(iStringNumber).VariableUBound Then
                LogASLError("Array index of '" & StringName & "[" & Trim(Str(ArrayIndex)) & "]' too big.", LOGTYPE_WARNINGERROR)
                GetStringContents = ""
                Exit Function
            End If

            ' Now, set the contents
            If Not ReturnAlias Then
                GetStringContents = StringVariable(iStringNumber).VariableContents(ArrayIndex)
            Else
                GetStringContents = Objs(GetObjectIDNoAlias(StringVariable(iStringNumber).VariableContents(ArrayIndex))).ObjectAlias
            End If
        End If
    End Function

    Private Function IsAvailable(ByRef ThingString As String, ByRef ThingType As Integer, ByRef Thread As ThreadData) As Boolean
        ' Returns availability of object/character

        ' split ThingString into character name and room
        ' (thingstring of form name@room)

        Dim i, AtPos As Integer
        Dim CRoom, CName As String

        AtPos = InStr(ThingString, "@")

        ' If no room specified, current room presumed
        If AtPos = 0 Then
            CRoom = CurrentRoom
            CName = ThingString
        Else
            CName = Trim(Left(ThingString, AtPos - 1))
            CRoom = Trim(Right(ThingString, Len(ThingString) - AtPos))
        End If

        If ThingType = QUEST_CHARACTER Then
            For i = 1 To NumberChars
                If LCase(Chars(i).ContainerRoom) = LCase(CRoom) And LCase(Chars(i).ObjectName) = LCase(CName) Then
                    IsAvailable = Chars(i).Exists
                    i = NumberChars
                End If
            Next i
        ElseIf ThingType = QUEST_OBJECT Then
            For i = 1 To NumberObjs
                If LCase(Objs(i).ContainerRoom) = LCase(CRoom) And LCase(Objs(i).ObjectName) = LCase(CName) Then
                    IsAvailable = Objs(i).Exists
                    i = NumberObjs
                End If
            Next i
        End If

    End Function

    Private Function IsCompatible(TestLine As String, ByRef RequiredLine As String) As Boolean
        'Tests to see if TestLine "works" with RequiredLine.
        'For example, if RequiredLine = "read #text#", then the
        'TestLines of "read book" and "read sign" are compatible.
        Dim CurrentReqLinePos, NextVarPos As Integer
        Dim CheckChunk As String
        Dim CurrentTestLinePos As Integer
        Dim FinishedProcessing As Boolean
        Dim Var2Pos As Integer

        ' This avoids "xxx123" being compatible with "xxx".
        TestLine = "¦" & Trim(TestLine) & "¦"
        RequiredLine = "¦" & RequiredLine & "¦"

        'Go through RequiredLine in chunks going up to variables.
        CurrentReqLinePos = 1
        CurrentTestLinePos = 1
        FinishedProcessing = False
        Do
            NextVarPos = InStr(CurrentReqLinePos, RequiredLine, "#")
            If NextVarPos = 0 Then
                NextVarPos = Len(RequiredLine) + 1
                FinishedProcessing = True
            Else
                Var2Pos = InStr(NextVarPos + 1, RequiredLine, "#")
            End If

            CheckChunk = Mid(RequiredLine, CurrentReqLinePos, (NextVarPos - 1) - (CurrentReqLinePos - 1))

            If InStr(CurrentTestLinePos, TestLine, CheckChunk) <> 0 Then
                CurrentTestLinePos = InStr(CurrentTestLinePos, TestLine, CheckChunk) + Len(CheckChunk)
            Else
                IsCompatible = False
                Exit Function
            End If

            'Skip to end of variable
            CurrentReqLinePos = Var2Pos + 1
        Loop Until FinishedProcessing

        IsCompatible = True
    End Function

    Private Function OpenGame(theGameFileName As String) As Boolean

        Dim cdatb, bResult As Boolean
        Dim CurObjVisible As Boolean
        Dim CurObjRoom As String
        Dim PrevQSGVersion As Boolean
        Dim FileData As String = ""
        Dim SavedQSGVersion As String
        Dim i As Integer
        Dim NullData As String = ""
        Dim CData As String = ""
        Dim CName As String
        Dim SemiColonPos, CDat As Integer
        Dim SC2Pos, SC3Pos As Integer
        Dim lines As String() = {}

        GameLoadMethod = "loaded"

        PrevQSGVersion = False

        If m_data Is Nothing Then
            FileData = System.IO.File.ReadAllText(theGameFileName, System.Text.Encoding.GetEncoding(1252))
        Else
            FileData = System.Text.Encoding.GetEncoding(1252).GetString(m_data.Data)
        End If

        ' Check version
        SavedQSGVersion = Left(FileData, 10)

        If BeginsWith(SavedQSGVersion, "QUEST200.1") Then
            PrevQSGVersion = True
        ElseIf Not BeginsWith(SavedQSGVersion, QSGVersion) Then
            Return False
        End If

        If PrevQSGVersion Then
            lines = FileData.Split({vbCrLf, vbLf}, StringSplitOptions.None)
            GameFileName = lines(1)
        Else
            InitFileData(FileData)
            NullData = GetNextChunk()

            If m_data Is Nothing Then
                GameFileName = GetNextChunk()
            Else
                GetNextChunk()
                GameFileName = m_data.SourceFile
            End If
        End If

        If m_data Is Nothing And Not System.IO.File.Exists(GameFileName) Then
            GameFileName = m_player.GetNewGameFile(GameFileName, "*.asl;*.cas;*.zip")
            If GameFileName = "" Then Exit Function
        End If

        bResult = InitialiseGame(GameFileName, True)

        If bResult = False Then
            Return False
        End If

        If Not PrevQSGVersion Then
            ' Open Quest 3.0 saved game file
            m_gameLoading = True
            Dim result As Boolean = RestoreGameData(FileData)
            m_gameLoading = False
            If Not result Then Return False
        Else
            ' Open Quest 2.x saved game file

            CurrentRoom = lines(3)

            ' Start at line 5 as line 4 is always "!c"
            Dim lineNumber As Integer = 5

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!i" Then
                    SemiColonPos = InStr(CData, ";")
                    CName = Trim(Left(CData, SemiColonPos - 1))
                    CDat = CInt(Right(CData, Len(CData) - SemiColonPos))

                    For i = 1 To NumCollectables
                        If Collectables(i).collectablename = CName Then
                            Collectables(i).collectablenumber = CDat
                            i = NumCollectables
                        End If
                    Next i
                End If
            Loop Until CData = "!i"

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!o" Then
                    SemiColonPos = InStr(CData, ";")
                    CName = Trim(Left(CData, SemiColonPos - 1))
                    cdatb = IsYes(Right(CData, Len(CData) - SemiColonPos))

                    For i = 1 To NumberItems
                        If Items(i).itemname = CName Then
                            Items(i).gotitem = cdatb
                            i = NumberItems
                        End If
                    Next i
                End If
            Loop Until CData = "!o"

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!p" Then
                    SemiColonPos = InStr(CData, ";")
                    SC2Pos = InStr(SemiColonPos + 1, CData, ";")
                    SC3Pos = InStr(SC2Pos + 1, CData, ";")

                    CName = Trim(Left(CData, SemiColonPos - 1))
                    cdatb = IsYes(Mid(CData, SemiColonPos + 1, (SC2Pos - SemiColonPos) - 1))
                    CurObjVisible = IsYes(Mid(CData, SC2Pos + 1, (SC3Pos - SC2Pos) - 1))
                    CurObjRoom = Trim(Mid(CData, SC3Pos + 1))

                    For i = 1 To NumberObjs
                        If Objs(i).ObjectName = CName And Not Objs(i).Loaded Then
                            Objs(i).Exists = cdatb
                            Objs(i).Visible = CurObjVisible
                            Objs(i).ContainerRoom = CurObjRoom
                            Objs(i).Loaded = True
                            i = NumberObjs
                        End If
                    Next i
                End If
            Loop Until CData = "!p"

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!s" Then
                    SemiColonPos = InStr(CData, ";")
                    SC2Pos = InStr(SemiColonPos + 1, CData, ";")
                    SC3Pos = InStr(SC2Pos + 1, CData, ";")

                    CName = Trim(Left(CData, SemiColonPos - 1))
                    cdatb = IsYes(Mid(CData, SemiColonPos + 1, (SC2Pos - SemiColonPos) - 1))
                    CurObjVisible = IsYes(Mid(CData, SC2Pos + 1, (SC3Pos - SC2Pos) - 1))
                    CurObjRoom = Trim(Mid(CData, SC3Pos + 1))

                    For i = 1 To NumberChars
                        If Chars(i).ObjectName = CName Then
                            Chars(i).Exists = cdatb
                            Chars(i).Visible = CurObjVisible
                            Chars(i).ContainerRoom = CurObjRoom
                            i = NumberChars
                        End If
                    Next i
                End If
            Loop Until CData = "!s"

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!n" Then
                    SemiColonPos = InStr(CData, ";")
                    CName = Trim(Left(CData, SemiColonPos - 1))
                    CData = Right(CData, Len(CData) - SemiColonPos)

                    SetStringContents(CName, CData, NullThread)
                End If
            Loop Until CData = "!n"

            Do
                CData = lines(lineNumber)
                lineNumber += 1
                If CData <> "!e" Then
                    SemiColonPos = InStr(CData, ";")
                    CName = Trim(Left(CData, SemiColonPos - 1))
                    CData = Right(CData, Len(CData) - SemiColonPos)

                    SetNumericVariableContents(CName, Val(CData), NullThread)
                End If
            Loop Until CData = "!e"

        End If

        SaveGameFile = theGameFileName

        Return True

    End Function

    Private Sub GetQuestSettings()

        ' TO DO: Implement options

        optDefaultFontName = "Arial"
        optDefaultFontSize = 9

        optEnableSpeech = True
        optSpeakEverything = False

    End Sub

    Private Function SaveGame(ByRef theGameFileName As String, Optional saveFile As Boolean = True) As Byte()
        Dim NewThread As ThreadData
        Dim saveData As String

        NewThread = NullThread

        If GameASLVersion >= 391 Then ExecuteScript(BeforeSaveScript, NewThread)

        If GameASLVersion >= 280 Then
            saveData = MakeRestoreData()
        Else
            saveData = MakeRestoreDataV2()
        End If

        If saveFile Then
            System.IO.File.WriteAllText(theGameFileName, saveData, System.Text.Encoding.GetEncoding(1252))
        End If

        SaveGameFile = theGameFileName

        Return System.Text.Encoding.GetEncoding(1252).GetBytes(saveData)

    End Function

    Private Function MakeRestoreDataV2() As String
        Dim lines As New List(Of String)
        Dim i As Integer

        lines.Add("QUEST200.1")
        lines.Add(GetOriginalFilenameForQSG)
        lines.Add(GameName)
        lines.Add(CurrentRoom)

        lines.Add("!c")
        For i = 1 To NumCollectables
            lines.Add(Collectables(i).collectablename & ";" & Str(Collectables(i).collectablenumber))
        Next i

        lines.Add("!i")
        For i = 1 To NumberItems
            lines.Add(Items(i).itemname & ";" & YesNo(Items(i).gotitem))
        Next i

        lines.Add("!o")
        For i = 1 To NumberObjs
            lines.Add(Objs(i).ObjectName & ";" & YesNo(Objs(i).Exists) & ";" & YesNo(Objs(i).Visible) & ";" & Objs(i).ContainerRoom)
        Next i

        lines.Add("!p")
        For i = 1 To NumberChars
            lines.Add(Chars(i).ObjectName & ";" & YesNo(Chars(i).Exists) & ";" & YesNo(Chars(i).Visible) & ";" & Chars(i).ContainerRoom)
        Next i

        lines.Add("!s")
        For i = 1 To NumberStringVariables
            lines.Add(StringVariable(i).VariableName & ";" & StringVariable(i).VariableContents(0))
        Next i

        lines.Add("!n")
        For i = 1 To NumberNumericVariables
            lines.Add(NumericVariable(i).VariableName & ";" & Str(CDbl(NumericVariable(i).VariableContents(0))))
        Next i

        lines.Add("!e")

        Return String.Join(vbCrLf, lines)
    End Function

    Private Sub SetAvailability(ByRef ThingString As String, ByRef ThingExist As Boolean, ByRef Thread As ThreadData, Optional ByRef ThingType As Integer = QUEST_OBJECT)
        ' Sets availability of objects (and characters in ASL<281)

        Dim i, ObjID, AtPos As Integer
        Dim CRoom, CName As String

        Dim FoundObject As Boolean
        If GameASLVersion >= 281 Then
            FoundObject = False
            For i = 1 To NumberObjs
                If LCase(Objs(i).ObjectName) = LCase(ThingString) Then
                    Objs(i).Exists = ThingExist
                    If ThingExist Then
                        AddToObjectProperties("not hidden", i, Thread)
                    Else
                        AddToObjectProperties("hidden", i, Thread)
                    End If
                    ObjID = i
                    FoundObject = True
                    i = NumberObjs
                End If
            Next i

            If Not FoundObject Then
                LogASLError("Not found object '" & ThingString & "'", LOGTYPE_WARNINGERROR)
            End If
        Else
            ' split ThingString into character name and room
            ' (thingstring of form name@room)

            AtPos = InStr(ThingString, "@")
            ' If no room specified, currentroom presumed
            If AtPos = 0 Then
                CRoom = CurrentRoom
                CName = ThingString
            Else
                CName = Trim(Left(ThingString, AtPos - 1))
                CRoom = Trim(Right(ThingString, Len(ThingString) - AtPos))
            End If
            If ThingType = QUEST_CHARACTER Then
                For i = 1 To NumberChars
                    If LCase(Chars(i).ContainerRoom) = LCase(CRoom) And LCase(Chars(i).ObjectName) = LCase(CName) Then
                        Chars(i).Exists = ThingExist
                        i = NumberChars + 1
                    End If
                Next i
            ElseIf ThingType = QUEST_OBJECT Then
                For i = 1 To NumberObjs
                    If LCase(Objs(i).ContainerRoom) = LCase(CRoom) And LCase(Objs(i).ObjectName) = LCase(CName) Then
                        Objs(i).Exists = ThingExist
                        i = NumberObjs + 1
                    End If
                Next i
            End If
        End If

        UpdateItems(Thread)

        UpdateObjectList(Thread)

    End Sub

    Friend Sub SetStringContents(ByRef StringName As String, ByRef StringContents As String, ByRef Thread As ThreadData, Optional ByRef ArrayIndex As Integer = 0)
        Dim bStringExists As Boolean
        Dim iStringNumber, i As Integer
        Dim StringTitle, OnChangeScript As String
        Dim BracketPos As Integer
        bStringExists = False

        If StringName = "" Then
            LogASLError("Internal error - tried to set empty string name to '" & StringContents & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        If GameASLVersion >= 281 Then
            BracketPos = InStr(StringName, "[")
            If BracketPos <> 0 Then
                ArrayIndex = GetArrayIndex(StringName, Thread)
                StringName = Left(StringName, BracketPos - 1)
            End If
        End If

        If ArrayIndex < 0 Then
            LogASLError("'" & StringName & "[" & Trim(Str(ArrayIndex)) & "]' is invalid - did not assign to array", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        ' First, see if the string already exists. If it does,
        ' modify it. If not, create it.

        If NumberStringVariables > 0 Then
            For i = 1 To NumberStringVariables
                If LCase(StringVariable(i).VariableName) = LCase(StringName) Then
                    iStringNumber = i
                    bStringExists = True
                    i = NumberStringVariables
                End If
            Next i
        End If

        If bStringExists = False Then
            NumberStringVariables = NumberStringVariables + 1
            iStringNumber = NumberStringVariables
            ReDim Preserve StringVariable(iStringNumber)

            For i = 0 To ArrayIndex
                StringTitle = StringName
                If ArrayIndex <> 0 Then StringTitle = StringTitle & "[" & Trim(Str(i)) & "]"
            Next i
            StringVariable(iStringNumber).VariableUBound = ArrayIndex
        End If

        If ArrayIndex > StringVariable(iStringNumber).VariableUBound Then
            ReDim Preserve StringVariable(iStringNumber).VariableContents(ArrayIndex)
            For i = StringVariable(iStringNumber).VariableUBound + 1 To ArrayIndex
                StringTitle = StringName
                If ArrayIndex <> 0 Then StringTitle = StringTitle & "[" & Trim(Str(i)) & "]"
            Next i
            StringVariable(iStringNumber).VariableUBound = ArrayIndex
        End If

        ' Now, set the contents
        StringVariable(iStringNumber).VariableName = StringName
        ReDim Preserve StringVariable(iStringNumber).VariableContents(StringVariable(iStringNumber).VariableUBound)
        StringVariable(iStringNumber).VariableContents(ArrayIndex) = StringContents

        If StringVariable(iStringNumber).OnChangeScript <> "" Then
            OnChangeScript = StringVariable(iStringNumber).OnChangeScript
            ExecuteScript(OnChangeScript, Thread)
        End If

        If StringVariable(iStringNumber).DisplayString <> "" Then
            UpdateStatusVars(Thread)
        End If
    End Sub

    Private Sub SetUpCharObjectInfo()
        Dim cdf As DefineBlock
        Dim DefaultExists As Boolean
        Dim PropertyData As PropertiesActions
        Dim DefaultProperties As New PropertiesActions
        Dim ThisAction As ActionType
        Dim RestOfLine As String
        Dim i As Integer
        Dim OrigCRoomName, CRoomName As String
        Dim k, j, e As Integer

        NumberChars = 0

        ' see if define type <default> exists:
        DefaultExists = False
        For i = 1 To NumberSections
            If Trim(Lines(DefineBlocks(i).StartLine)) = "define type <default>" Then
                DefaultExists = True
                DefaultProperties = GetPropertiesInType("default")
                i = NumberSections
            End If
        Next i

        For i = 1 To NumberSections
            cdf = DefineBlocks(i)
            If BeginsWith(Lines(cdf.StartLine), "define room") Or BeginsWith(Lines(cdf.StartLine), "define game") Or BeginsWith(Lines(cdf.StartLine), "define object ") Then
                If BeginsWith(Lines(cdf.StartLine), "define room") Then
                    OrigCRoomName = RetrieveParameter(Lines(cdf.StartLine), NullThread)
                Else
                    OrigCRoomName = ""
                End If

                If BeginsWith(Lines(cdf.StartLine), "define object ") Then
                    cdf.StartLine = cdf.StartLine - 1
                    cdf.EndLine = cdf.EndLine + 1
                End If

                For j = cdf.StartLine + 1 To cdf.EndLine - 1
                    If BeginsWith(Lines(j), "define object") Then
                        CRoomName = OrigCRoomName

                        NumberObjs = NumberObjs + 1
                        ReDim Preserve Objs(NumberObjs)

                        With Objs(NumberObjs)
                            .ObjectName = RetrieveParameter(Lines(j), NullThread)
                            .ObjectAlias = .ObjectName
                            .DefinitionSectionStart = j
                            .ContainerRoom = CRoomName
                            .Visible = True
                            .Gender = "it"
                            .Article = "it"

                            .take.Type = TA_NOTHING

                            If DefaultExists Then
                                AddToObjectProperties(DefaultProperties.Properties, NumberObjs, NullThread)
                                For k = 1 To DefaultProperties.NumberActions
                                    AddObjectAction(NumberObjs, DefaultProperties.Actions(k))
                                Next k
                            End If

                            If GameASLVersion >= 391 Then AddToObjectProperties("list", NumberObjs, NullThread)

                            e = 0
                            Do
                                j = j + 1
                                If Trim(Lines(j)) = "hidden" Then
                                    .Exists = False
                                    e = 1
                                    If GameASLVersion >= 311 Then AddToObjectProperties("hidden", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "startin ") And CRoomName = "__UNKNOWN" Then
                                    CRoomName = RetrieveParameter(Lines(j), NullThread)
                                ElseIf BeginsWith(Lines(j), "prefix ") Then
                                    .Prefix = RetrieveParameter(Lines(j), NullThread) & " "
                                    If GameASLVersion >= 311 Then AddToObjectProperties("prefix=" & .Prefix, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "suffix ") Then
                                    .Suffix = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("suffix=" & .Suffix, NumberObjs, NullThread)
                                ElseIf Trim(Lines(j)) = "invisible" Then
                                    .Visible = False
                                    If GameASLVersion >= 311 Then AddToObjectProperties("invisible", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "alias ") Then
                                    .ObjectAlias = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("alias=" & .ObjectAlias, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "alt ") Then
                                    AddToObjectAltNames(RetrieveParameter(Lines(j), NullThread), NumberObjs)
                                ElseIf BeginsWith(Lines(j), "detail ") Then
                                    .Detail = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("detail=" & .Detail, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "gender ") Then
                                    .Gender = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("gender=" & .Gender, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "article ") Then
                                    .Article = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("article=" & .Article, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "gain ") Then
                                    .GainScript = GetEverythingAfter(Lines(j), "gain ")
                                    ThisAction.ActionName = "gain"
                                    ThisAction.Script = .GainScript
                                    AddObjectAction(NumberObjs, ThisAction)
                                ElseIf BeginsWith(Lines(j), "lose ") Then
                                    .LoseScript = GetEverythingAfter(Lines(j), "lose ")
                                    ThisAction.ActionName = "lose"
                                    ThisAction.Script = .LoseScript
                                    AddObjectAction(NumberObjs, ThisAction)
                                ElseIf BeginsWith(Lines(j), "displaytype ") Then
                                    .DisplayType = RetrieveParameter(Lines(j), NullThread)
                                    If GameASLVersion >= 311 Then AddToObjectProperties("displaytype=" & .DisplayType, NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "look ") Then
                                    If GameASLVersion >= 311 Then
                                        RestOfLine = GetEverythingAfter(Lines(j), "look ")
                                        If Left(RestOfLine, 1) = "<" Then
                                            AddToObjectProperties("look=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                        Else
                                            ThisAction.ActionName = "look"
                                            ThisAction.Script = RestOfLine
                                            AddObjectAction(NumberObjs, ThisAction)
                                        End If
                                    End If
                                ElseIf BeginsWith(Lines(j), "examine ") Then
                                    If GameASLVersion >= 311 Then
                                        RestOfLine = GetEverythingAfter(Lines(j), "examine ")
                                        If Left(RestOfLine, 1) = "<" Then
                                            AddToObjectProperties("examine=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                        Else
                                            ThisAction.ActionName = "examine"
                                            ThisAction.Script = RestOfLine
                                            AddObjectAction(NumberObjs, ThisAction)
                                        End If
                                    End If
                                ElseIf GameASLVersion >= 311 And BeginsWith(Lines(j), "speak ") Then
                                    RestOfLine = GetEverythingAfter(Lines(j), "speak ")
                                    If Left(RestOfLine, 1) = "<" Then
                                        AddToObjectProperties("speak=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        ThisAction.ActionName = "speak"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf BeginsWith(Lines(j), "properties ") Then
                                    AddToObjectProperties(RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "type ") Then
                                    .NumberTypesIncluded = .NumberTypesIncluded + 1
                                    ReDim Preserve .TypesIncluded(.NumberTypesIncluded)
                                    .TypesIncluded(.NumberTypesIncluded) = RetrieveParameter(Lines(j), NullThread)

                                    PropertyData = GetPropertiesInType(RetrieveParameter(Lines(j), NullThread))
                                    AddToObjectProperties(PropertyData.Properties, NumberObjs, NullThread)
                                    For k = 1 To PropertyData.NumberActions
                                        AddObjectAction(NumberObjs, PropertyData.Actions(k))
                                    Next k

                                    ReDim Preserve .TypesIncluded(.NumberTypesIncluded + PropertyData.NumberTypesIncluded)
                                    For k = 1 To PropertyData.NumberTypesIncluded
                                        .TypesIncluded(k + .NumberTypesIncluded) = PropertyData.TypesIncluded(k)
                                    Next k
                                    .NumberTypesIncluded = .NumberTypesIncluded + PropertyData.NumberTypesIncluded
                                ElseIf BeginsWith(Lines(j), "action ") Then
                                    AddToObjectActions(GetEverythingAfter(Lines(j), "action "), NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "use ") Then
                                    AddToUseInfo(NumberObjs, GetEverythingAfter(Lines(j), "use "))
                                ElseIf BeginsWith(Lines(j), "give ") Then
                                    AddToGiveInfo(NumberObjs, GetEverythingAfter(Lines(j), "give "))
                                ElseIf Trim(Lines(j)) = "take" Then
                                    .take.Type = TA_DEFAULT
                                    AddToObjectProperties("take", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "take ") Then
                                    If Left(GetEverythingAfter(Lines(j), "take "), 1) = "<" Then
                                        .take.Type = TA_TEXT
                                        .take.Data = RetrieveParameter(Lines(j), NullThread)

                                        AddToObjectProperties("take=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        .take.Type = TA_SCRIPT
                                        RestOfLine = GetEverythingAfter(Lines(j), "take ")
                                        .take.Data = RestOfLine

                                        ThisAction.ActionName = "take"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf Trim(Lines(j)) = "container" Then
                                    If GameASLVersion >= 391 Then AddToObjectProperties("container", NumberObjs, NullThread)
                                ElseIf Trim(Lines(j)) = "surface" Then
                                    If GameASLVersion >= 391 Then
                                        AddToObjectProperties("container", NumberObjs, NullThread)
                                        AddToObjectProperties("surface", NumberObjs, NullThread)
                                    End If
                                ElseIf Trim(Lines(j)) = "opened" Then
                                    If GameASLVersion >= 391 Then AddToObjectProperties("opened", NumberObjs, NullThread)
                                ElseIf Trim(Lines(j)) = "transparent" Then
                                    If GameASLVersion >= 391 Then AddToObjectProperties("transparent", NumberObjs, NullThread)
                                ElseIf Trim(Lines(j)) = "open" Then
                                    AddToObjectProperties("open", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "open ") Then
                                    If Left(GetEverythingAfter(Lines(j), "open "), 1) = "<" Then
                                        AddToObjectProperties("open=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        RestOfLine = GetEverythingAfter(Lines(j), "open ")

                                        ThisAction.ActionName = "open"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf Trim(Lines(j)) = "close" Then
                                    AddToObjectProperties("close", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "close ") Then
                                    If Left(GetEverythingAfter(Lines(j), "close "), 1) = "<" Then
                                        AddToObjectProperties("close=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        RestOfLine = GetEverythingAfter(Lines(j), "close ")

                                        ThisAction.ActionName = "close"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf Trim(Lines(j)) = "add" Then
                                    AddToObjectProperties("add", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "add ") Then
                                    If Left(GetEverythingAfter(Lines(j), "add "), 1) = "<" Then
                                        AddToObjectProperties("add=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        RestOfLine = GetEverythingAfter(Lines(j), "add ")

                                        ThisAction.ActionName = "add"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf Trim(Lines(j)) = "remove" Then
                                    AddToObjectProperties("remove", NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "remove ") Then
                                    If Left(GetEverythingAfter(Lines(j), "remove "), 1) = "<" Then
                                        AddToObjectProperties("remove=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                    Else
                                        RestOfLine = GetEverythingAfter(Lines(j), "remove ")

                                        ThisAction.ActionName = "remove"
                                        ThisAction.Script = RestOfLine
                                        AddObjectAction(NumberObjs, ThisAction)
                                    End If
                                ElseIf BeginsWith(Lines(j), "parent ") Then
                                    AddToObjectProperties("parent=" & RetrieveParameter(Lines(j), NullThread), NumberObjs, NullThread)
                                ElseIf BeginsWith(Lines(j), "list") Then
                                    ProcessListInfo(Lines(j), NumberObjs)
                                End If

                            Loop Until Trim(Lines(j)) = "end define"

                            .DefinitionSectionEnd = j
                            If e = 0 Then .Exists = True
                        End With
                    ElseIf GameASLVersion <= 280 And BeginsWith(Lines(j), "define character") Then
                        CRoomName = OrigCRoomName
                        NumberChars = NumberChars + 1
                        ReDim Preserve Chars(NumberChars)
                        Chars(NumberChars).ObjectName = RetrieveParameter(Lines(j), NullThread)
                        Chars(NumberChars).DefinitionSectionStart = j
                        Chars(NumberChars).ContainerRoom = ""
                        Chars(NumberChars).Visible = True
                        e = 0
                        Do
                            j = j + 1
                            If Trim(Lines(j)) = "hidden" Then
                                Chars(NumberChars).Exists = False
                                e = 1
                            ElseIf BeginsWith(Lines(j), "startin ") And CRoomName = "__UNKNOWN" Then
                                CRoomName = RetrieveParameter(Lines(j), NullThread)
                            ElseIf BeginsWith(Lines(j), "prefix ") Then
                                Chars(NumberChars).Prefix = RetrieveParameter(Lines(j), NullThread) & " "
                            ElseIf BeginsWith(Lines(j), "suffix ") Then
                                Chars(NumberChars).Suffix = " " & RetrieveParameter(Lines(j), NullThread)
                            ElseIf Trim(Lines(j)) = "invisible" Then
                                Chars(NumberChars).Visible = False
                            ElseIf BeginsWith(Lines(j), "alias ") Then
                                Chars(NumberChars).ObjectAlias = RetrieveParameter(Lines(j), NullThread)
                            ElseIf BeginsWith(Lines(j), "detail ") Then
                                Chars(NumberChars).Detail = RetrieveParameter(Lines(j), NullThread)
                            End If

                            Chars(NumberChars).ContainerRoom = CRoomName

                        Loop Until Trim(Lines(j)) = "end define"

                        Chars(NumberChars).DefinitionSectionEnd = j
                        If e = 0 Then Chars(NumberChars).Exists = True
                    End If
                Next j
            End If
        Next i

        UpdateVisibilityInContainers(NullThread)

    End Sub

    Private Sub ShowGameAbout(ByRef Thread As ThreadData)
        Dim GameAuthor, GameVersion, GameCopy As String
        Dim GameInfo As String

        GameVersion = FindStatement(GetDefineBlock("game"), "game version")
        GameAuthor = FindStatement(GetDefineBlock("game"), "game author")
        GameCopy = FindStatement(GetDefineBlock("game"), "game copyright")
        GameInfo = FindStatement(GetDefineBlock("game"), "game info")

        Print("|bGame name:|cl  " & GameName & "|cb|xb", Thread)
        If GameVersion <> "" Then Print("|bVersion:|xb    " & GameVersion, Thread)
        If GameAuthor <> "" Then Print("|bAuthor:|xb     " & GameAuthor, Thread)
        If GameCopy <> "" Then Print("|bCopyright:|xb  " & GameCopy, Thread)

        If GameInfo <> "" Then
            Print("", Thread)
            Print(GameInfo, Thread)
        End If

    End Sub

    Private Sub ShowPicture(ByRef sFileName As String, Optional ByRef Animated As Integer = ANIMATION_NONE)

        ' In Quest 4.x this function would be used for showing a picture in a popup window, but
        ' this is no longer supported - ALL images are displayed in-line with the game text. Any
        ' image caption is displayed as text, and any image size specified is ignored.

        Dim SizeString As String
        Dim Caption As String = ""

        If InStr(sFileName, ";") <> 0 Then
            Caption = Trim(Mid(sFileName, InStr(sFileName, ";") + 1))
            sFileName = Trim(Left(sFileName, InStr(sFileName, ";") - 1))
        End If

        If InStr(sFileName, "@") <> 0 Then
            SizeString = Trim(Mid(sFileName, InStr(sFileName, "@") + 1))
            sFileName = Trim(Left(sFileName, InStr(sFileName, "@") - 1))
        End If

        If Caption.Length > 0 Then Print(Caption, NullThread)

        ShowPictureInText(sFileName)

    End Sub

    Private Sub ShowRoomInfo(ByRef Room As String, ByRef Thread As ThreadData, Optional ByRef NoPrint As Boolean = False)
        If GameASLVersion < 280 Then
            ShowRoomInfoV2(Room)
            Exit Sub
        End If

        Dim RoomDisplayText As String = ""
        Dim DescTagExist As Boolean
        Dim gameblock As DefineBlock
        Dim DoorwayString, RoomAlias As String
        Dim RoomID As Integer
        Dim FinishedFindingCommas As Boolean
        Dim Prefix, RoomDisplayName As String
        Dim RoomDisplayNameNoFormat, InDescription As String
        Dim VisibleObjects As String = ""
        Dim VisibleObjectsNoFormat As String
        Dim i As Integer
        Dim PlaceList As String
        Dim LastComma, OldLastComma As Integer
        Dim DescType As Integer
        Dim DescLine As String = ""
        Dim ShowLookText As Boolean
        Dim LookDesc As String = ""
        Dim ObjLook As String
        Dim ObjSuffix As String

        gameblock = GetDefineBlock("game")

        CurrentRoom = Room
        RoomID = GetRoomID(CurrentRoom, Thread)

        If RoomID = 0 Then Exit Sub

        ' FIRST LINE - YOU ARE IN... ***********************************************

        RoomAlias = Rooms(RoomID).RoomAlias
        If RoomAlias = "" Then RoomAlias = Rooms(RoomID).RoomName

        Prefix = Rooms(RoomID).Prefix

        If Prefix = "" Then
            RoomDisplayName = "|cr" & RoomAlias & "|cb"
            RoomDisplayNameNoFormat = RoomAlias ' No formatting version, for label
        Else
            RoomDisplayName = Prefix & " |cr" & RoomAlias & "|cb"
            RoomDisplayNameNoFormat = Prefix & " " & RoomAlias
        End If

        InDescription = Rooms(RoomID).InDescription

        If InDescription <> "" Then
            ' Print player's location according to indescription:
            If Right(InDescription, 1) = ":" Then
                ' if line ends with a colon, add place name:
                RoomDisplayText = RoomDisplayText & Left(InDescription, Len(InDescription) - 1) & " " & RoomDisplayName & "." & vbCrLf
            Else
                ' otherwise, just print the indescription line:
                RoomDisplayText = RoomDisplayText & InDescription & vbCrLf
            End If
        Else
            ' if no indescription line, print the default.
            RoomDisplayText = RoomDisplayText & "You are in " & RoomDisplayName & "." & vbCrLf
        End If

        m_player.LocationUpdated(UCase(Left(RoomAlias, 1)) & Mid(RoomAlias, 2))

        SetStringContents("quest.formatroom", RoomDisplayNameNoFormat, Thread)

        ' SHOW OBJECTS *************************************************************

        VisibleObjectsNoFormat = ""

        Dim colVisibleObjects As New List(Of Integer) ' of object IDs
        Dim lCount As Integer

        For i = 1 To NumberObjs
            If LCase(Objs(i).ContainerRoom) = LCase(Room) And Objs(i).Exists And Objs(i).Visible And Not Objs(i).IsExit Then
                colVisibleObjects.Add(i)
            End If
        Next i

        For Each objId As Integer In colVisibleObjects
            ObjSuffix = Objs(objId).Suffix
            If ObjSuffix <> "" Then ObjSuffix = " " & ObjSuffix

            If Objs(objId).ObjectAlias = "" Then
                VisibleObjects = VisibleObjects & Objs(objId).Prefix & "|b" & Objs(objId).ObjectName & "|xb" & ObjSuffix
                VisibleObjectsNoFormat = VisibleObjectsNoFormat & Objs(objId).Prefix & Objs(objId).ObjectName
            Else
                VisibleObjects = VisibleObjects & Objs(objId).Prefix & "|b" & Objs(objId).ObjectAlias & "|xb" & ObjSuffix
                VisibleObjectsNoFormat = VisibleObjectsNoFormat & Objs(objId).Prefix & Objs(objId).ObjectAlias
            End If

            lCount = lCount + 1
            If lCount < colVisibleObjects.Count() - 1 Then
                VisibleObjects = VisibleObjects & ", "
                VisibleObjectsNoFormat = VisibleObjectsNoFormat & ", "
            ElseIf lCount = colVisibleObjects.Count() - 1 Then
                VisibleObjects = VisibleObjects & " and "
                VisibleObjectsNoFormat = VisibleObjectsNoFormat & ", "
            End If
        Next

        If colVisibleObjects.Count() > 0 Then

            SetStringContents("quest.formatobjects", VisibleObjects, Thread)

            VisibleObjects = "There is " & VisibleObjects & " here."

            SetStringContents("quest.objects", VisibleObjectsNoFormat, Thread)

            RoomDisplayText = RoomDisplayText & VisibleObjects & vbCrLf
        Else
            SetStringContents("quest.objects", "", Thread)
            SetStringContents("quest.formatobjects", "", Thread)
        End If

        ' SHOW EXITS ***************************************************************

        DoorwayString = UpdateDoorways(RoomID, Thread)

        If GameASLVersion < 410 Then
            PlaceList = GetGoToExits(RoomID, Thread)

            If PlaceList <> "" Then
                'strip final comma
                PlaceList = Left(PlaceList, Len(PlaceList) - 2)

                'if there is still a comma here, there is more than
                'one place, so add the word "or" before the last one.
                If InStr(PlaceList, ",") > 0 Then
                    LastComma = 0
                    FinishedFindingCommas = False
                    Do
                        OldLastComma = LastComma
                        LastComma = InStr(LastComma + 1, PlaceList, ",")
                        If LastComma = 0 Then
                            FinishedFindingCommas = True
                            LastComma = OldLastComma
                        End If
                    Loop Until FinishedFindingCommas

                    PlaceList = Left(PlaceList, LastComma - 1) & " or" & Right(PlaceList, Len(PlaceList) - LastComma)
                End If

                RoomDisplayText = RoomDisplayText & "You can go to " & PlaceList & "." & vbCrLf
                SetStringContents("quest.doorways.places", PlaceList, Thread)
            Else
                SetStringContents("quest.doorways.places", "", Thread)
            End If
        End If

        ' GET "LOOK" DESCRIPTION (but don't print it yet) **************************

        ObjLook = GetObjectProperty("look", Rooms(RoomID).ObjID, , False)

        If ObjLook = "" Then
            If Rooms(RoomID).Look <> "" Then
                LookDesc = Rooms(RoomID).Look
            End If
        Else
            LookDesc = ObjLook
        End If

        SetStringContents("quest.lookdesc", LookDesc, Thread)


        ' FIND DESCRIPTION TAG, OR ACTION ******************************************

        ' In Quest versions prior to 3.1, with any custom description, the "look"
        ' text was always displayed after the "description" tag was printed/executed.
        ' In Quest 3.1 and later, it isn't - descriptions should print the look
        ' tag themselves when and where necessary.

        ShowLookText = True

        If Rooms(RoomID).Description.Data <> "" Then
            DescLine = Rooms(RoomID).Description.Data
            DescType = Rooms(RoomID).Description.Type
            DescTagExist = True
        Else
            DescTagExist = False
        End If

        If DescTagExist = False Then
            'Look in the "define game" block:
            For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
                If BeginsWith(Lines(i), "description ") Then
                    DescLine = GetEverythingAfter(Lines(i), "description ")
                    DescTagExist = True
                    If Left(DescLine, 1) = "<" Then
                        DescLine = RetrieveParameter(DescLine, Thread)
                        DescType = TA_TEXT
                    Else
                        DescType = TA_SCRIPT
                    End If
                    i = gameblock.EndLine
                End If
            Next i
        End If

        If DescTagExist And GameASLVersion >= 310 Then
            ShowLookText = False
        End If

        If Not NoPrint Then
            If DescTagExist = False Then
                'Remove final vbCrLf:
                RoomDisplayText = Left(RoomDisplayText, Len(RoomDisplayText) - 2)
                Print(RoomDisplayText, Thread)
                If DoorwayString <> "" Then Print(DoorwayString, Thread)
            Else
                'execute description tag:
                'If no script, just print the tag's parameter.
                'Otherwise, execute it as ASL script:

                If DescType = TA_TEXT Then
                    Print(DescLine, Thread)
                Else
                    ExecuteScript(DescLine, Thread)
                End If
            End If

            UpdateObjectList(Thread)

            ' SHOW "LOOK" DESCRIPTION **************************************************

            If ShowLookText Then
                If LookDesc <> "" Then
                    Print(LookDesc, Thread)
                End If
            End If
        End If

    End Sub

    Private Sub CheckCollectable(ByRef ColNum As Integer)
        ' Checks to see whether a collectable item has exceeded
        ' its range - if so, it resets the number to the nearest
        ' valid number. It's a handy quick way of making sure that
        ' a player's health doesn't reach 101%, for example.

        Dim maxn, n, minn As Double
        Dim M As Integer
        Dim T As String

        T = Collectables(ColNum).collectabletype
        n = Collectables(ColNum).collectablenumber

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

        Collectables(ColNum).collectablenumber = n

    End Sub

    Private Function DisplayCollectableInfo(ByRef ColNum As Integer) As String

        Dim FirstBit, D, NextBit As String
        Dim ExcPos As Integer
        Dim FirstStarPos, SecondStarPos As Integer
        Dim AfterStar, BeforeStar, BetweenStar As String

        If Collectables(ColNum).collectabledisplay = "<def>" Then
            D = "You have " & Trim(Str(Collectables(ColNum).collectablenumber)) & " " & Collectables(ColNum).collectablename
        ElseIf Collectables(ColNum).collectabledisplay = "" Then
            D = "<null>"
        Else
            ExcPos = InStr(Collectables(ColNum).collectabledisplay, "!")
            If ExcPos = 0 Then
                D = Collectables(ColNum).collectabledisplay
            Else
                FirstBit = Left(Collectables(ColNum).collectabledisplay, ExcPos - 1)
                NextBit = Right(Collectables(ColNum).collectabledisplay, Len(Collectables(ColNum).collectabledisplay) - ExcPos)
                D = FirstBit & Trim(Str(Collectables(ColNum).collectablenumber)) & NextBit
            End If

            If InStr(D, "*") > 0 Then
                FirstStarPos = InStr(D, "*")
                SecondStarPos = InStr(FirstStarPos + 1, D, "*")
                BeforeStar = Left(D, FirstStarPos - 1)
                AfterStar = Mid(D, SecondStarPos + 1)
                BetweenStar = Mid(D, FirstStarPos + 1, (SecondStarPos - FirstStarPos) - 1)

                If Collectables(ColNum).collectablenumber <> 1 Then
                    D = BeforeStar & BetweenStar & AfterStar
                Else
                    D = BeforeStar & AfterStar
                End If
            End If
        End If

        If Collectables(ColNum).collectablenumber = 0 And Collectables(ColNum).DisplayWhenZero = False Then
            D = "<null>"
        End If

        DisplayCollectableInfo = D

    End Function

    Private Sub DisplayTextSection(ByRef sectionname As String, ByRef Thread As ThreadData, Optional ByRef OutputTo As String = "normal")
        Dim textblock As DefineBlock
        textblock = DefineBlockParam("text", sectionname)
        Dim i As Integer

        If textblock.StartLine <> 0 Then
            For i = textblock.StartLine + 1 To textblock.EndLine - 1
                If GameASLVersion >= 392 Then
                    ' Convert string variables etc.
                    Print(RetrieveParameter("<" & Lines(i) & ">", Thread), Thread, OutputTo)
                Else
                    Print(Lines(i), Thread, OutputTo)
                End If
            Next i

            Print("", Thread)
        End If

    End Sub

    ' Returns true if the system is ready to process a new command after completion - so it will be
    ' in most cases, except when ExecCommand just caused an "enter" script command to complete

    Private Function ExecCommand(ByRef thecommand As String, ByRef Thread As ThreadData, Optional ByRef EchoCommand As Boolean = True, Optional ByRef RunUserCommand As Boolean = True, Optional ByRef DontSetIt As Boolean = False) As Boolean
        Dim cmd As String
        Dim EnteredHelpCommand As Boolean
        Dim RoomID As Integer
        Dim GlobalOverride As Boolean
        Dim OldBadCmdBefore As String
        Dim NewThread As ThreadData
        Dim CP, i, n As Integer
        Dim SkipAfterTurn As Boolean
        Dim G, D, c, P, T As String
        Dim j As Integer

        SkipAfterTurn = False
        thecommand = StripCodes(thecommand)

        OldBadCmdBefore = BadCmdBefore

        RoomID = GetRoomID(CurrentRoom, Thread)
        EnteredHelpCommand = False

        If thecommand = "" Then Return True

        cmd = LCase(thecommand)

        SyncLock m_commandLock
            If CommandOverrideModeOn Then
                ' Commands have been overridden for this command,
                ' so put input into previously specified variable
                ' and exit:

                SetStringContents(CommandOverrideVariable, thecommand, Thread)
                System.Threading.Monitor.PulseAll(m_commandLock)
                Return False
            End If
        End SyncLock

        Dim UserCommandReturn As Boolean
        Dim newcommand As String

        If EchoCommand = True Then
            Print("> " & thecommand, Thread, , True)

            If optSpeakEverything Then Speak(thecommand)
        End If

        thecommand = LCase(thecommand)

        SetStringContents("quest.originalcommand", thecommand, Thread)

        newcommand = " " & thecommand & " "

        ' Convert synonyms:
        For i = 1 To NumberSynonyms
            CP = 1
            Do
                n = InStr(CP, newcommand, " " & Synonyms(i).OriginalWord & " ")
                If n <> 0 Then
                    newcommand = Left(newcommand, n - 1) & " " & Synonyms(i).ConvertTo & " " & Mid(newcommand, n + Len(Synonyms(i).OriginalWord) + 2)
                    CP = n + 1
                End If
            Loop Until n = 0
        Next i

        'strip starting and ending spaces
        thecommand = Mid(newcommand, 2, Len(newcommand) - 2)

        SetStringContents("quest.command", thecommand, Thread)

        ' Execute any "beforeturn" script:

        NewThread = Thread
        GlobalOverride = False

        ' RoomID is 0 if there are no rooms in the game. Unlikely, but we get an RTE otherwise.
        If RoomID <> 0 Then
            If Rooms(RoomID).BeforeTurnScript <> "" Then
                If BeginsWith(Rooms(RoomID).BeforeTurnScript, "override") Then
                    ExecuteScript(GetEverythingAfter(Rooms(RoomID).BeforeTurnScript, "override"), NewThread)
                    GlobalOverride = True
                Else
                    ExecuteScript(Rooms(RoomID).BeforeTurnScript, NewThread)
                End If
            End If
        End If
        If BeforeTurnScript <> "" And GlobalOverride = False Then ExecuteScript(BeforeTurnScript, NewThread)

        ' In executing BeforeTurn script, "dontprocess" sets Thread.DontProcessCommand,
        ' in which case we don't process the command.

        If Not NewThread.DontProcessCommand Then
            'Try to execute user defined command, if allowed:

            UserCommandReturn = False
            If RunUserCommand = True Then
                UserCommandReturn = ExecUserCommand(thecommand, Thread)

                If Not UserCommandReturn Then
                    UserCommandReturn = ExecVerb(thecommand, Thread)
                End If

                If Not UserCommandReturn Then
                    ' Try command defined by a library
                    UserCommandReturn = ExecUserCommand(thecommand, Thread, True)
                End If

                If Not UserCommandReturn Then
                    ' Try verb defined by a library
                    UserCommandReturn = ExecVerb(thecommand, Thread, True)
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
                ExecSpeak(c, Thread)
            ElseIf CmdStartsWith(thecommand, "talk to ") Then
                c = GetEverythingAfter(thecommand, "talk to ")
                ExecSpeak(c, Thread)
            ElseIf cmd = "exit" Or cmd = "out" Or cmd = "leave" Then
                LeaveRoom(Thread)
                LastIt = 0
            ElseIf cmd = "north" Or cmd = "south" Or cmd = "east" Or cmd = "west" Then
                GoDirection(thecommand, Thread)
                LastIt = 0
            ElseIf cmd = "n" Or cmd = "s" Or cmd = "w" Or cmd = "e" Then
                Select Case InStr("nswe", cmd)
                    Case 1
                        GoDirection("north", Thread)
                    Case 2
                        GoDirection("south", Thread)
                    Case 3
                        GoDirection("west", Thread)
                    Case 4
                        GoDirection("east", Thread)
                End Select
                LastIt = 0
            ElseIf cmd = "ne" Or cmd = "northeast" Or cmd = "north-east" Or cmd = "north east" Or cmd = "go ne" Or cmd = "go northeast" Or cmd = "go north-east" Or cmd = "go north east" Then
                GoDirection("northeast", Thread)
                LastIt = 0
            ElseIf cmd = "nw" Or cmd = "northwest" Or cmd = "north-west" Or cmd = "north west" Or cmd = "go nw" Or cmd = "go northwest" Or cmd = "go north-west" Or cmd = "go north west" Then
                GoDirection("northwest", Thread)
                LastIt = 0
            ElseIf cmd = "se" Or cmd = "southeast" Or cmd = "south-east" Or cmd = "south east" Or cmd = "go se" Or cmd = "go southeast" Or cmd = "go south-east" Or cmd = "go south east" Then
                GoDirection("southeast", Thread)
                LastIt = 0
            ElseIf cmd = "sw" Or cmd = "southwest" Or cmd = "south-west" Or cmd = "south west" Or cmd = "go sw" Or cmd = "go southwest" Or cmd = "go south-west" Or cmd = "go south west" Then
                GoDirection("southwest", Thread)
                LastIt = 0
            ElseIf cmd = "up" Or cmd = "u" Then
                GoDirection("up", Thread)
                LastIt = 0
            ElseIf cmd = "down" Or cmd = "d" Then
                GoDirection("down", Thread)
                LastIt = 0
            ElseIf CmdStartsWith(thecommand, "go ") Then
                If GameASLVersion >= 410 Then
                    Rooms(GetRoomID(CurrentRoom, Thread)).Exits.ExecuteGo(thecommand, Thread)
                Else
                    D = GetEverythingAfter(thecommand, "go ")
                    If D = "out" Then
                        LeaveRoom(Thread)
                    ElseIf D = "north" Or D = "south" Or D = "east" Or D = "west" Or D = "up" Or D = "down" Then
                        GoDirection(D, Thread)
                    ElseIf BeginsWith(D, "to ") Then
                        P = GetEverythingAfter(D, "to ")
                        GoToPlace(P, Thread)
                    Else
                        PlayerErrorMessage(ERROR_BADGO, Thread)
                    End If
                End If
                LastIt = 0
            ElseIf CmdStartsWith(thecommand, "give ") Then
                G = GetEverythingAfter(thecommand, "give ")
                ExecGive(G, Thread)
            ElseIf CmdStartsWith(thecommand, "take ") Then
                T = GetEverythingAfter(thecommand, "take ")
                ExecTake(T, Thread)
            ElseIf CmdStartsWith(thecommand, "drop ") And GameASLVersion >= 280 Then
                D = GetEverythingAfter(thecommand, "drop ")
                ExecDrop(D, Thread)
            ElseIf CmdStartsWith(thecommand, "get ") Then
                T = GetEverythingAfter(thecommand, "get ")
                ExecTake(T, Thread)
            ElseIf CmdStartsWith(thecommand, "pick up ") Then
                T = GetEverythingAfter(thecommand, "pick up ")
                ExecTake(T, Thread)
            ElseIf cmd = "pick it up" Or cmd = "pick them up" Or cmd = "pick this up" Or cmd = "pick that up" Or cmd = "pick these up" Or cmd = "pick those up" Or cmd = "pick him up" Or cmd = "pick her up" Then
                ExecTake(Mid(cmd, 6, InStr(7, cmd, " ") - 6), Thread)
            ElseIf CmdStartsWith(thecommand, "look ") Then
                ExecLook(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "l ") Then
                ExecLook("look " & GetEverythingAfter(thecommand, "l "), Thread)
            ElseIf CmdStartsWith(thecommand, "examine ") And GameASLVersion >= 280 Then
                ExecExamine(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "x ") And GameASLVersion >= 280 Then
                ExecExamine("examine " & GetEverythingAfter(thecommand, "x "), Thread)
            ElseIf cmd = "l" Or cmd = "look" Then
                ExecLook("look", Thread)
            ElseIf cmd = "x" Or cmd = "examine" Then
                ExecExamine("examine", Thread)
            ElseIf CmdStartsWith(thecommand, "use ") Then
                ExecUse(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "open ") And GameASLVersion >= 391 Then
                ExecOpenClose(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "close ") And GameASLVersion >= 391 Then
                ExecOpenClose(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "put ") And GameASLVersion >= 391 Then
                ExecAddRemove(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "add ") And GameASLVersion >= 391 Then
                ExecAddRemove(thecommand, Thread)
            ElseIf CmdStartsWith(thecommand, "remove ") And GameASLVersion >= 391 Then
                ExecAddRemove(thecommand, Thread)
            ElseIf cmd = "save" Then
                m_player.RequestSave(Nothing)
            ElseIf cmd = "quit" Then
                GameFinished()
            ElseIf BeginsWith(cmd, "help") Then
                ShowHelp(Thread)
                EnteredHelpCommand = True
            ElseIf cmd = "about" Then
                ShowGameAbout(Thread)
            ElseIf cmd = "clear" Then
                DoClear()
            ElseIf cmd = "ver" Then
                Print("Quest ASL4 Interpreter " & QuestVersion, Thread)
                Print("Highest supported ASL version: " & ASLVersionNumber, Thread)
                Print("Current file uses ASL version: " & Trim(Str(GameASLVersion)), Thread)
            ElseIf cmd = "debug" Then
                ' TO DO: This is temporary, would be better to have a log viewer built in to Player
                For Each logEntry As String In m_log
                    Print(logEntry, Thread)
                Next
            ElseIf cmd = "inventory" Or cmd = "inv" Or cmd = "i" Then
                InventoryPlace = "inventory"

                If GameASLVersion >= 280 Then
                    For i = 1 To NumberObjs
                        If Objs(i).ContainerRoom = InventoryPlace And Objs(i).Exists And Objs(i).Visible Then
                            InvList = InvList & Objs(i).Prefix

                            If Objs(i).ObjectAlias = "" Then
                                InvList = InvList & "|b" & Objs(i).ObjectName & "|xb"
                            Else
                                InvList = InvList & "|b" & Objs(i).ObjectAlias & "|xb"
                            End If

                            If Objs(i).Suffix <> "" Then
                                InvList = InvList & " " & Objs(i).Suffix
                            End If

                            InvList = InvList & ", "
                        End If
                    Next i
                Else
                    For j = 1 To NumberItems
                        If Items(j).gotitem = True Then
                            InvList = InvList & Items(j).itemname & ", "
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
                    Print("You are carrying:|n" & InvList & ".", Thread)
                Else
                    Print("You are not carrying anything.", Thread)
                End If
            ElseIf CmdStartsWith(thecommand, "oops ") Then
                ExecOops(GetEverythingAfter(thecommand, "oops "), Thread)
            ElseIf CmdStartsWith(thecommand, "the ") Then
                ExecOops(GetEverythingAfter(thecommand, "the "), Thread)
            Else
                PlayerErrorMessage(ERROR_BADCOMMAND, Thread)
            End If
        End If

        If Not SkipAfterTurn Then
            ' Execute any "afterturn" script:
            GlobalOverride = False

            If RoomID <> 0 Then
                If Rooms(RoomID).AfterTurnScript <> "" Then
                    If BeginsWith(Rooms(RoomID).AfterTurnScript, "override") Then
                        ExecuteScript(GetEverythingAfter(Rooms(RoomID).AfterTurnScript, "override"), Thread)
                        GlobalOverride = True
                    Else
                        ExecuteScript(Rooms(RoomID).AfterTurnScript, Thread)
                    End If
                End If
            End If

            ' was set to NullThread here for some reason
            If AfterTurnScript <> "" And GlobalOverride = False Then ExecuteScript(AfterTurnScript, Thread)
        End If

        Print("", Thread)

        If Not DontSetIt Then
            ' Use "DontSetIt" when we don't want "it" etc. to refer to the object used in this turn.
            ' This is used for e.g. auto-remove object from container when taking.
            LastIt = ThisTurnIt
            LastItMode = ThisTurnItMode
        End If
        If BadCmdBefore = OldBadCmdBefore Then BadCmdBefore = ""

        Return True
    End Function

    Private Function CmdStartsWith(sCommand As String, sCheck As String) As Boolean
        ' When we are checking user input in ExecCommand, we check things like whether
        ' the player entered something beginning with "put ". We need to trim what the player entered
        ' though, otherwise they might just type "put " and then we would try disambiguating an object
        ' called "".

        CmdStartsWith = BeginsWith(Trim(sCommand), sCheck)
    End Function

    Private Sub ExecGive(ByRef GiveString As String, ByRef Thread As ThreadData)
        Dim characterblock As DefineBlock
        Dim ObjArticle As String
        Dim ToLoc As Integer
        Dim ItemToGive, CharToGive As String
        Dim ObjectType As Integer
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
                PlayerErrorMessage(ERROR_BADGIVE, Thread)
                Exit Sub
            Else
                ItemToGive = Trim(Mid(GiveString, ToLoc + 4, Len(GiveString) - (ToLoc + 2)))
                CharToGive = Trim(Mid(GiveString, 1, ToLoc))
            End If
        Else
            ItemToGive = Trim(Mid(GiveString, 1, ToLoc))
            CharToGive = Trim(Mid(GiveString, ToLoc + 3, Len(GiveString) - (ToLoc + 2)))
        End If

        If GameASLVersion >= 281 Then
            ObjectType = QUEST_OBJECT
        Else
            ObjectType = QUEST_CHARACTER
        End If

        ' First see if player has "ItemToGive":
        If GameASLVersion >= 280 Then
            InventoryPlace = "inventory"

            ItemID = Disambiguate(ItemToGive, InventoryPlace, Thread)

            If ItemID = -1 Then
                PlayerErrorMessage(ERROR_NOITEM, Thread)
                BadCmdBefore = "give"
                BadCmdAfter = "to " & CharToGive
                Exit Sub
            ElseIf ItemID = -2 Then
                Exit Sub
            Else
                ObjArticle = Objs(ItemID).Article
            End If
        Else
            ' ASL2:
            NotGotItem = True

            For i = 1 To NumberItems
                If LCase(Items(i).itemname) = LCase(ItemToGive) Then
                    If Items(i).gotitem = False Then
                        NotGotItem = True
                        i = NumberItems
                    Else
                        NotGotItem = False
                    End If
                End If
            Next i

            If NotGotItem = True Then
                PlayerErrorMessage(ERROR_NOITEM, Thread)
                Exit Sub
            Else
                ObjArticle = Objs(ItemID).Article
            End If
        End If

        If GameASLVersion >= 281 Then
            FoundGiveScript = False
            FoundGiveToObject = False

            GiveToObjectID = Disambiguate(CharToGive, CurrentRoom, Thread)
            If GiveToObjectID > 0 Then
                FoundGiveToObject = True
            End If

            If Not FoundGiveToObject Then
                If GiveToObjectID <> -2 Then PlayerErrorMessage(ERROR_BADCHARACTER, Thread)
                BadCmdBefore = "give " & ItemToGive & " to"
                Exit Sub
            End If

            'Find appropriate give script ****
            'now, for "give a to b", we have
            'ItemID=a and GiveToObjectID=b

            With Objs(GiveToObjectID)
                For i = 1 To .NumberGiveData
                    If .GiveData(i).GiveType = GIVE_SOMETHING_TO And LCase(.GiveData(i).GiveObject) = LCase(Objs(ItemID).ObjectName) Then
                        FoundGiveScript = True
                        GiveScript = .GiveData(i).GiveScript
                        i = .NumberGiveData
                    End If
                Next i
            End With

            If Not FoundGiveScript Then
                'check a for give to <b>:
                With Objs(ItemID)
                    For i = 1 To .NumberGiveData
                        If .GiveData(i).GiveType = GIVE_TO_SOMETHING And LCase(.GiveData(i).GiveObject) = LCase(Objs(GiveToObjectID).ObjectName) Then
                            FoundGiveScript = True
                            GiveScript = .GiveData(i).GiveScript
                            i = .NumberGiveData
                        End If
                    Next i
                End With
            End If

            If Not FoundGiveScript Then
                'check b for give anything:
                GiveScript = Objs(GiveToObjectID).GiveAnything
                If GiveScript <> "" Then
                    FoundGiveScript = True
                    SetStringContents("quest.give.object.name", Objs(ItemID).ObjectName, Thread)
                End If
            End If

            If Not FoundGiveScript Then
                'check a for give to anything:
                GiveScript = Objs(ItemID).GiveToAnything
                If GiveScript <> "" Then
                    FoundGiveScript = True
                    SetStringContents("quest.give.object.name", Objs(GiveToObjectID).ObjectName, Thread)
                End If
            End If

            If FoundGiveScript Then
                ExecuteScript(GiveScript, Thread, ItemID)
            Else
                SetStringContents("quest.error.charactername", Objs(GiveToObjectID).ObjectName, Thread)

                ObjGender = Trim(Objs(GiveToObjectID).Gender)
                ObjGender = UCase(Left(ObjGender, 1)) & Mid(ObjGender, 2)
                SetStringContents("quest.error.gender", ObjGender, Thread)

                SetStringContents("quest.error.article", ObjArticle, Thread)
                PlayerErrorMessage(ERROR_ITEMUNWANTED, Thread)
            End If
        Else
            ' ASL2:
            characterblock = GetThingBlock(CharToGive, CurrentRoom, ObjectType)

            If (characterblock.StartLine = 0 And characterblock.EndLine = 0) Or IsAvailable(CharToGive & "@" & CurrentRoom, ObjectType, Thread) = False Then
                PlayerErrorMessage(ERROR_BADCHARACTER, Thread)
                Exit Sub
            End If

            RealName = Chars(GetThingNumber(CharToGive, CurrentRoom, ObjectType)).ObjectName

            ' now, see if there's a give statement for this item in
            ' this characters definition:

            GiveLine = 0
            For i = characterblock.StartLine + 1 To characterblock.EndLine - 1
                If BeginsWith(Lines(i), "give") Then
                    ItemCheck = RetrieveParameter(Lines(i), Thread)
                    If LCase(ItemCheck) = LCase(ItemToGive) Then
                        GiveLine = i
                    End If
                End If
            Next i

            If GiveLine = 0 Then
                If ObjArticle = "" Then ObjArticle = "it"
                SetStringContents("quest.error.charactername", RealName, Thread)
                SetStringContents("quest.error.gender", Trim(GetGender(CharToGive, True, Thread)), Thread)
                SetStringContents("quest.error.article", ObjArticle, Thread)
                PlayerErrorMessage(ERROR_ITEMUNWANTED, Thread)
                Exit Sub
            End If

            ' now, execute the statement on GiveLine
            ExecuteScript(GetSecondChunk(Lines(GiveLine)), Thread)
        End If

    End Sub

    Private Sub ExecLook(ByRef LookLine As String, ByRef Thread As ThreadData)

        Dim AtPos As Integer
        Dim LookStuff, LookItem, LookText As String

        Dim InventoryPlace As String
        Dim FoundObject As Boolean
        Dim ObjID As Integer
        If Trim(LookLine) = "look" Then
            ShowRoomInfo((CurrentRoom), Thread)
        Else
            If GameASLVersion < 391 Then
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

            If GameASLVersion >= 280 Then
                FoundObject = False

                InventoryPlace = "inventory"

                ObjID = Disambiguate(LookItem, InventoryPlace & ";" & CurrentRoom, Thread)
                If ObjID > 0 Then
                    FoundObject = True
                End If

                If Not FoundObject Then
                    If ObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                    BadCmdBefore = "look at"
                    Exit Sub
                End If

                If Objs(ObjID).IsNetPlayer Then
                    Print(Objs(ObjID).ObjectAlias & " is another player.", Thread)
                Else
                    DoLook(ObjID, Thread)
                End If
            Else
                If BeginsWith(LookItem, "the ") Then
                    LookItem = GetEverythingAfter(LookItem, "the ")
                End If

                LookLine = RetrLine("object", LookItem, "look", Thread)

                If LookLine <> "<unfound>" Then
                    'Check for availability
                    If IsAvailable(LookItem, QUEST_OBJECT, Thread) = False Then
                        LookLine = "<unfound>"
                    End If
                End If

                If LookLine = "<unfound>" Then
                    LookLine = RetrLine("character", LookItem, "look", Thread)

                    If LookLine <> "<unfound>" Then
                        If IsAvailable(LookItem, QUEST_CHARACTER, Thread) = False Then
                            LookLine = "<unfound>"
                        End If
                    End If

                    If LookLine = "<unfound>" Then
                        PlayerErrorMessage(ERROR_BADTHING, Thread)
                        Exit Sub
                    ElseIf LookLine = "<undefined>" Then
                        PlayerErrorMessage(ERROR_DEFAULTLOOK, Thread)
                        Exit Sub
                    End If
                ElseIf LookLine = "<undefined>" Then
                    PlayerErrorMessage(ERROR_DEFAULTLOOK, Thread)
                    Exit Sub
                End If

                LookStuff = Trim(GetEverythingAfter(Trim(LookLine), "look "))
                If Left(LookStuff, 1) = "<" Then
                    LookText = RetrieveParameter(LookLine, Thread)
                    Print(LookText, Thread)
                Else
                    ExecuteScript(LookStuff, Thread, ObjID)
                End If
            End If
        End If

    End Sub

    Private Sub ExecSpeak(ByRef c As String, ByRef Thread As ThreadData)
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

        Dim ObjectType As Integer

        Dim SpeakLine As String = ""
        Dim InventoryPlace As String
        Dim FoundSpeak As Boolean
        Dim SpeakText As String
        Dim FoundObject As Boolean
        Dim ObjID As Integer
        If GameASLVersion >= 281 Then

            FoundObject = False

            InventoryPlace = "inventory"

            ObjID = Disambiguate(ObjectName, InventoryPlace & ";" & CurrentRoom, Thread)
            If ObjID > 0 Then
                FoundObject = True
            End If

            If Not FoundObject Then
                If ObjID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                BadCmdBefore = "speak to"
                Exit Sub
            End If

            FoundSpeak = False

            ' First look for action, then look
            ' for property, then check define
            ' section:

            With Objs(ObjID)

                For i = 1 To .NumberActions
                    If .Actions(i).ActionName = "speak" Then
                        SpeakLine = "speak " & .Actions(i).Script
                        FoundSpeak = True
                        i = .NumberActions
                    End If
                Next i

                If Not FoundSpeak Then
                    For i = 1 To .NumberProperties
                        If .Properties(i).PropertyName = "speak" Then
                            SpeakLine = "speak <" & .Properties(i).PropertyValue & ">"
                            FoundSpeak = True
                            i = .NumberProperties
                        End If
                    Next i
                End If

                If GameASLVersion < 311 Then
                    ' For some reason ASL3 < 311 looks for a "look" tag rather than
                    ' having had this set up at initialisation.
                    If Not FoundSpeak Then
                        For i = .DefinitionSectionStart To .DefinitionSectionEnd
                            If BeginsWith(Lines(i), "speak ") Then
                                SpeakLine = Lines(i)
                                FoundSpeak = True
                            End If
                        Next i
                    End If
                End If

            End With

            If Not FoundSpeak Then
                SetStringContents("quest.error.gender", UCase(Left(Objs(ObjID).Gender, 1)) & Mid(Objs(ObjID).Gender, 2), Thread)
                PlayerErrorMessage(ERROR_DEFAULTSPEAK, Thread)
                Exit Sub
            End If

            SpeakLine = GetEverythingAfter(SpeakLine, "speak ")

            If BeginsWith(SpeakLine, "<") Then
                SpeakText = RetrieveParameter(SpeakLine, Thread)
                If GameASLVersion >= 350 Then
                    Print(SpeakText, Thread)
                Else
                    Print(Chr(34) & SpeakText & Chr(34), Thread)
                End If
            Else
                ExecuteScript(SpeakLine, Thread, ObjID)
            End If

        Else
            l = RetrLine("character", c, "speak", Thread)
            ObjectType = QUEST_CHARACTER

            s = Trim(GetEverythingAfter(Trim(l), "speak "))

            If l <> "<unfound>" And l <> "<undefined>" Then
                ' Character exists; but is it available??
                If IsAvailable(c & "@" & CurrentRoom, ObjectType, Thread) = False Then
                    l = "<undefined>"
                End If
            End If

            If l = "<undefined>" Then
                PlayerErrorMessage(ERROR_BADCHARACTER, Thread)
            ElseIf l = "<unfound>" Then
                SetStringContents("quest.error.gender", Trim(GetGender(c, True, Thread)), Thread)
                SetStringContents("quest.error.charactername", c, Thread)
                PlayerErrorMessage(ERROR_DEFAULTSPEAK, Thread)
            ElseIf BeginsWith(s, "<") Then
                s = RetrieveParameter(l, Thread)
                Print(Chr(34) & s & Chr(34), Thread)
            Else
                ExecuteScript(s, Thread)
            End If
        End If

    End Sub

    Private Sub ExecTake(ByRef TakeItem As String, ByRef Thread As ThreadData)
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

        ObjID = Disambiguate(TakeItem, CurrentRoom, Thread)

        If ObjID < 0 Then
            FoundItem = False
        Else
            FoundItem = True
        End If

        If FoundItem = False Then
            If ObjID <> -2 Then
                If GameASLVersion >= 410 Then
                    ObjID = Disambiguate(TakeItem, "inventory", Thread)
                    If ObjID >= 0 Then
                        ' Player already has this item
                        PlayerErrorMessage(ERROR_ALREADYTAKEN, Thread)
                    Else
                        PlayerErrorMessage(ERROR_BADTHING, Thread)
                    End If
                ElseIf GameASLVersion >= 391 Then
                    PlayerErrorMessage(ERROR_BADTHING, Thread)
                Else
                    PlayerErrorMessage(ERROR_BADITEM, Thread)
                End If
            End If
            BadCmdBefore = "take"
            Exit Sub
        Else
            SetStringContents("quest.error.article", Objs(ObjID).Article, Thread)
        End If

        ObjectIsInContainer = False

        If GameASLVersion >= 391 Then

            If Not PlayerCanAccessObject(ObjID, ObjectIsInContainer, ParentID, ContainerError) Then
                PlayerErrorMessage_ExtendInfo(ERROR_BADTAKE, Thread, ContainerError)
                Exit Sub
            End If

        End If

        If GameASLVersion >= 280 Then
            With Objs(ObjID).take

                If ObjectIsInContainer And (.Type = TA_DEFAULT Or .Type = TA_TEXT) Then
                    ' So, we want to take an object that's in a container or surface. So first
                    ' we have to remove the object from that container.

                    If Objs(ParentID).ObjectAlias <> "" Then
                        ParentDisplayName = Objs(ParentID).ObjectAlias
                    Else
                        ParentDisplayName = Objs(ParentID).ObjectName
                    End If

                    Print("(first removing " & Objs(ObjID).Article & " from " & ParentDisplayName & ")", Thread)

                    ' Try to remove the object
                    Thread.AllowRealNamesInCommand = True
                    ExecCommand("remove " & Objs(ObjID).ObjectName, Thread, False, , True)

                    If GetObjectProperty("parent", ObjID, False, False) <> "" Then
                        ' removing the object failed
                        Exit Sub
                    End If
                End If

                If .Type = TA_DEFAULT Then
                    PlayerErrorMessage(ERROR_DEFAULTTAKE, Thread)
                    PlayerItem(TakeItem, True, Thread, ObjID)
                ElseIf .Type = TA_TEXT Then
                    Print(.Data, Thread)
                    PlayerItem(TakeItem, True, Thread, ObjID)
                ElseIf .Type = TA_SCRIPT Then
                    ExecuteScript(.Data, Thread, ObjID)
                Else
                    PlayerErrorMessage(ERROR_BADTAKE, Thread)
                End If
            End With
        Else

            ' find 'take' line
            For i = Objs(ObjID).DefinitionSectionStart + 1 To Objs(ObjID).DefinitionSectionEnd - 1
                If BeginsWith(Lines(i), "take") Then
                    ScriptLine = Trim(GetEverythingAfter(Trim(Lines(i)), "take"))
                    ExecuteScript(ScriptLine, Thread, ObjID)
                    FoundTake = True
                    i = Objs(ObjID).DefinitionSectionEnd
                End If
            Next i

            If FoundTake = False Then
                PlayerErrorMessage(ERROR_BADTAKE, Thread)
            End If
        End If

    End Sub

    Private Sub ExecUse(ByRef tuseline As String, ByRef Thread As ThreadData)
        Dim OnWithPos, i, EndOnWith As Integer
        Dim UseLine As String
        Dim UseDeclareLine As String = "", UseOn, UseItem, ScriptLine As String

        UseLine = Trim(GetEverythingAfter(tuseline, "use "))

        Dim RoomID As Integer
        RoomID = GetRoomID(CurrentRoom, Thread)

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
        If GameASLVersion >= 280 Then
            InventoryPlace = "inventory"

            FoundItem = False

            ItemID = Disambiguate(UseItem, InventoryPlace, Thread)
            If ItemID > 0 Then FoundItem = True

            If Not FoundItem Then
                If ItemID <> -2 Then PlayerErrorMessage(ERROR_NOITEM, Thread)
                If UseOn = "" Then
                    BadCmdBefore = "use"
                Else
                    BadCmdBefore = "use"
                    BadCmdAfter = "on " & UseOn
                End If
                Exit Sub
            End If
        Else
            NotGotItem = True

            For i = 1 To NumberItems
                If LCase(Items(i).itemname) = LCase(UseItem) Then
                    If Items(i).gotitem = False Then
                        NotGotItem = True
                        i = NumberItems
                    Else
                        NotGotItem = False
                    End If
                End If
            Next i

            If NotGotItem = True Then
                PlayerErrorMessage(ERROR_NOITEM, Thread)
                Exit Sub
            End If
        End If

        Dim UseScript As String = ""
        Dim FoundUseScript As Boolean
        Dim FoundUseOnObject As Boolean
        Dim UseOnObjectID As Integer
        Dim Found As Boolean
        If GameASLVersion >= 280 Then
            FoundUseScript = False

            If UseOn = "" Then
                If GameASLVersion < 410 Then
                    With Rooms(RoomID)
                        For i = 1 To .NumberUse
                            If LCase(Objs(ItemID).ObjectName) = LCase(.use(i).Text) Then
                                FoundUseScript = True
                                UseScript = .use(i).Script
                                i = .NumberUse
                            End If
                        Next i
                    End With
                End If

                If Not FoundUseScript Then
                    UseScript = Objs(ItemID).use
                    If UseScript <> "" Then FoundUseScript = True
                End If
            Else
                FoundUseOnObject = False

                UseOnObjectID = Disambiguate(UseOn, CurrentRoom, Thread)
                If UseOnObjectID > 0 Then
                    FoundUseOnObject = True
                Else
                    UseOnObjectID = Disambiguate(UseOn, InventoryPlace, Thread)
                    If UseOnObjectID > 0 Then
                        FoundUseOnObject = True
                    End If
                End If

                If Not FoundUseOnObject Then
                    If UseOnObjectID <> -2 Then PlayerErrorMessage(ERROR_BADTHING, Thread)
                    BadCmdBefore = "use " & UseItem & " on"
                    Exit Sub
                End If

                'now, for "use a on b", we have
                'ItemID=a and UseOnObjectID=b

                'first check b for use <a>:
                With Objs(UseOnObjectID)
                    For i = 1 To .NumberUseData
                        If .UseData(i).UseType = USE_SOMETHING_ON And LCase(.UseData(i).UseObject) = LCase(Objs(ItemID).ObjectName) Then
                            FoundUseScript = True
                            UseScript = .UseData(i).UseScript
                            i = .NumberUseData
                        End If
                    Next i
                End With

                If Not FoundUseScript Then
                    'check a for use on <b>:
                    With Objs(ItemID)
                        For i = 1 To .NumberUseData
                            If .UseData(i).UseType = USE_ON_SOMETHING And LCase(.UseData(i).UseObject) = LCase(Objs(UseOnObjectID).ObjectName) Then
                                FoundUseScript = True
                                UseScript = .UseData(i).UseScript
                                i = .NumberUseData
                            End If
                        Next i
                    End With
                End If

                If Not FoundUseScript Then
                    'check b for use anything:
                    UseScript = Objs(UseOnObjectID).UseAnything
                    If UseScript <> "" Then
                        FoundUseScript = True
                        SetStringContents("quest.use.object.name", Objs(ItemID).ObjectName, Thread)
                    End If
                End If

                If Not FoundUseScript Then
                    'check a for use on anything:
                    UseScript = Objs(ItemID).UseOnAnything
                    If UseScript <> "" Then
                        FoundUseScript = True
                        SetStringContents("quest.use.object.name", Objs(UseOnObjectID).ObjectName, Thread)
                    End If
                End If
            End If

            If FoundUseScript Then
                ExecuteScript(UseScript, Thread, ItemID)
            Else
                PlayerErrorMessage(ERROR_DEFAULTUSE, Thread)
            End If
        Else
            If UseOn <> "" Then
                UseDeclareLine = RetrLineParam("object", UseOn, "use", UseItem, Thread)
            Else
                Found = False
                For i = 1 To Rooms(RoomID).NumberUse
                    If LCase(Rooms(RoomID).use(i).Text) = LCase(UseItem) Then
                        UseDeclareLine = "use <> " & Rooms(RoomID).use(i).Script
                        Found = True
                        i = Rooms(RoomID).NumberUse
                    End If
                Next i

                If Not Found Then
                    UseDeclareLine = FindLine(GetDefineBlock("game"), "use", UseItem)
                End If

                If Not Found And UseDeclareLine = "" Then
                    PlayerErrorMessage(ERROR_DEFAULTUSE, Thread)
                    Exit Sub
                End If
            End If

            If UseDeclareLine <> "<unfound>" And UseDeclareLine <> "<undefined>" And UseOn <> "" Then
                'Check for object availablity
                If IsAvailable(UseOn, QUEST_OBJECT, Thread) = False Then
                    UseDeclareLine = "<undefined>"
                End If
            End If

            If UseDeclareLine = "<undefined>" Then
                UseDeclareLine = RetrLineParam("character", UseOn, "use", UseItem, Thread)

                If UseDeclareLine <> "<undefined>" Then
                    'Check for character availability
                    If IsAvailable(UseOn, QUEST_CHARACTER, Thread) = False Then
                        UseDeclareLine = "<undefined>"
                    End If
                End If

                If UseDeclareLine = "<undefined>" Then
                    PlayerErrorMessage(ERROR_BADTHING, Thread)
                    Exit Sub
                ElseIf UseDeclareLine = "<unfound>" Then
                    PlayerErrorMessage(ERROR_DEFAULTUSE, Thread)
                    Exit Sub
                End If
            ElseIf UseDeclareLine = "<unfound>" Then
                PlayerErrorMessage(ERROR_DEFAULTUSE, Thread)
                Exit Sub
            End If

            ScriptLine = Right(UseDeclareLine, Len(UseDeclareLine) - InStr(UseDeclareLine, ">"))
            ExecuteScript(ScriptLine, Thread)
        End If

    End Sub

    Private Sub ObjectActionUpdate(ByRef ObjID As Integer, ByRef ActionName As String, ByRef ActionScript As String, Optional ByRef NoUpdate As Boolean = False)
        Dim ObjectName As String
        Dim SP, EP As Integer
        ActionName = LCase(ActionName)

        If Not NoUpdate Then
            If ActionName = "take" Then
                Objs(ObjID).take.Data = ActionScript
                Objs(ObjID).take.Type = TA_SCRIPT
            ElseIf ActionName = "use" Then
                AddToUseInfo(ObjID, ActionScript)
            ElseIf ActionName = "gain" Then
                Objs(ObjID).GainScript = ActionScript
            ElseIf ActionName = "lose" Then
                Objs(ObjID).LoseScript = ActionScript
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

        If GameFullyLoaded Then
            AddToOOChangeLog(ChangeLog.eAppliesToType.atObject, Objs(ObjID).ObjectName, ActionName, "action <" & ActionName & "> " & ActionScript)
        End If

    End Sub

    Private Sub ExecuteIf(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        Dim IfLine, Conditions, ObscuredLine As String
        Dim ElsePos, ThenPos, ThenEndPos As Integer
        Dim ThenScript As String, ElseScript As String = ""

        IfLine = Trim(GetEverythingAfter(Trim(ScriptLine), "if "))
        ObscuredLine = ObliterateParameters(IfLine)

        ThenPos = InStr(ObscuredLine, "then")

        Dim ErrMsg As String
        If ThenPos = 0 Then
            ErrMsg = "Expected 'then' missing from script statement '" & ReportErrorLine(ScriptLine) & "' - statement bypassed."
            LogASLError(ErrMsg, LOGTYPE_WARNINGERROR)
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

        If ExecuteConditions(Conditions, Thread) Then
            ExecuteScript((ThenScript), Thread)
        Else
            If ElsePos <> 0 Then ExecuteScript((ElseScript), Thread)
        End If

    End Sub

    Private Sub ExecuteScript(ByRef ScriptLine As String, ByRef Thread As ThreadData, Optional ByRef NewCallingObjectID As Integer = 0)

        On Error GoTo ErrorHandler

        Debug.Print(ScriptLine)

        Dim TranscriptLine As String
        Dim i As Integer

        If Trim(ScriptLine) = "" Then Exit Sub
        If m_gameFinished Then Exit Sub

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
                    ExecuteScript(CurScriptLine, Thread)
                End If
                CurPos = CRLFPos + 2
            Loop Until bFinLoop
            Exit Sub
        End If

        If NewCallingObjectID <> 0 Then
            Thread.CallingObjectID = NewCallingObjectID
        End If

        Dim procblock As DefineBlock

        Dim ModVol As Integer
        If BeginsWith(ScriptLine, "if ") Then
            ExecuteIf(ScriptLine, Thread)
        ElseIf BeginsWith(ScriptLine, "select case ") Then
            ExecuteSelectCase(ScriptLine, Thread)
        ElseIf BeginsWith(ScriptLine, "choose ") Then
            ExecuteChoose(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "set ") Then
            ExecuteSet(GetEverythingAfter(ScriptLine, "set "), Thread)
        ElseIf BeginsWith(ScriptLine, "inc ") Or BeginsWith(ScriptLine, "dec ") Then
            ExecuteIncDec(ScriptLine, Thread)
        ElseIf BeginsWith(ScriptLine, "say ") Then
            Print(Chr(34) & RetrieveParameter(ScriptLine, Thread) & Chr(34), Thread)
        ElseIf BeginsWith(ScriptLine, "do ") Then
            ExecuteDo(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "doaction ") Then
            ExecuteDoAction(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "give ") Then
            PlayerItem(RetrieveParameter(ScriptLine, Thread), True, Thread)
        ElseIf BeginsWith(ScriptLine, "lose ") Or BeginsWith(ScriptLine, "drop ") Then
            PlayerItem(RetrieveParameter(ScriptLine, Thread), False, Thread)
        ElseIf BeginsWith(ScriptLine, "msg nospeak ") Then
            Print(RetrieveParameter(ScriptLine, Thread), Thread, , True)
        ElseIf BeginsWith(ScriptLine, "msg ") Then
            Print(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "speak ") Then
            Speak(RetrieveParameter(ScriptLine, Thread))
        ElseIf BeginsWith(ScriptLine, "helpmsg ") Then
            Print(RetrieveParameter(ScriptLine, Thread), Thread, "help")
        ElseIf Trim(LCase(ScriptLine)) = "helpclose" Then
            ' This command does nothing in the Quest 5 player, as there is no separate help window
        ElseIf BeginsWith(ScriptLine, "goto ") Then
            PlayGame(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "playerwin") Then
            FinishGame(STOPGAME_WIN, Thread)
        ElseIf BeginsWith(ScriptLine, "playerlose") Then
            FinishGame(STOPGAME_LOSE, Thread)
        ElseIf Trim(LCase(ScriptLine)) = "stop" Then
            FinishGame(STOPGAME_NULL, Thread)
        ElseIf BeginsWith(ScriptLine, "playwav ") Then
            PlayWav(RetrieveParameter(ScriptLine, Thread))
        ElseIf BeginsWith(ScriptLine, "playmidi ") Then
            PlayMidi(RetrieveParameter(ScriptLine, Thread))
        ElseIf BeginsWith(ScriptLine, "playmp3 ") Then
            PlayMP3(RetrieveParameter(ScriptLine, Thread))
        ElseIf Trim(LCase(ScriptLine)) = "picture close" Then
            ' This command does nothing in the Quest 5 player, as there is no separate picture window
        ElseIf (GameASLVersion >= 390 And BeginsWith(ScriptLine, "picture popup ")) Or (GameASLVersion >= 282 And GameASLVersion < 390 And BeginsWith(ScriptLine, "picture ")) Or (GameASLVersion < 282 And BeginsWith(ScriptLine, "show ")) Then
            ShowPicture(RetrieveParameter(ScriptLine, Thread))
        ElseIf (GameASLVersion >= 390 And BeginsWith(ScriptLine, "picture ")) Then
            ShowPictureInText(RetrieveParameter(ScriptLine, Thread))
        ElseIf BeginsWith(ScriptLine, "animate persist ") Then
            ShowPicture(RetrieveParameter(ScriptLine, Thread), ANIMATION_PERSIST)
        ElseIf BeginsWith(ScriptLine, "animate ") Then
            ShowPicture(RetrieveParameter(ScriptLine, Thread), ANIMATION_NORMAL)
        ElseIf BeginsWith(ScriptLine, "extract ") Then
            ExtractFile(RetrieveParameter(ScriptLine, Thread))
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "hideobject ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), False, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "showobject ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), True, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "moveobject ") Then
            ExecMoveThing(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "hidechar ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), False, Thread, QUEST_CHARACTER)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "showchar ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), True, Thread, QUEST_CHARACTER)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "movechar ") Then
            ExecMoveThing(RetrieveParameter(ScriptLine, Thread), QUEST_CHARACTER, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "revealobject ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, True, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "concealobject ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, False, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "revealchar ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_CHARACTER, True, Thread)
        ElseIf GameASLVersion < 281 And BeginsWith(ScriptLine, "concealchar ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_CHARACTER, False, Thread)
        ElseIf GameASLVersion >= 281 And BeginsWith(ScriptLine, "hide ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), False, Thread)
        ElseIf GameASLVersion >= 281 And BeginsWith(ScriptLine, "show ") Then
            SetAvailability(RetrieveParameter(ScriptLine, Thread), True, Thread)
        ElseIf GameASLVersion >= 281 And BeginsWith(ScriptLine, "move ") Then
            ExecMoveThing(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, Thread)
        ElseIf GameASLVersion >= 281 And BeginsWith(ScriptLine, "reveal ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, True, Thread)
        ElseIf GameASLVersion >= 281 And BeginsWith(ScriptLine, "conceal ") Then
            SetVisibility(RetrieveParameter(ScriptLine, Thread), QUEST_OBJECT, False, Thread)
        ElseIf GameASLVersion >= 391 And BeginsWith(ScriptLine, "open ") Then
            SetOpenClose(RetrieveParameter(ScriptLine, Thread), True, Thread)
        ElseIf GameASLVersion >= 391 And BeginsWith(ScriptLine, "close ") Then
            SetOpenClose(RetrieveParameter(ScriptLine, Thread), False, Thread)
        ElseIf GameASLVersion >= 391 And BeginsWith(ScriptLine, "add ") Then
            ExecAddRemoveScript(RetrieveParameter(ScriptLine, Thread), True, Thread)
        ElseIf GameASLVersion >= 391 And BeginsWith(ScriptLine, "remove ") Then
            ExecAddRemoveScript(RetrieveParameter(ScriptLine, Thread), False, Thread)
        ElseIf BeginsWith(ScriptLine, "clone ") Then
            ExecClone(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "exec ") Then
            ExecExec(ScriptLine, Thread)
        ElseIf BeginsWith(ScriptLine, "setstring ") Then
            ExecSetString(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "setvar ") Then
            ExecSetVar(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "for ") Then
            ExecFor(GetEverythingAfter(ScriptLine, "for "), Thread)
        ElseIf BeginsWith(ScriptLine, "property ") Then
            ExecProperty(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "type ") Then
            ExecType(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "action ") Then
            ExecuteAction(GetEverythingAfter(ScriptLine, "action "), Thread)
        ElseIf BeginsWith(ScriptLine, "flag ") Then
            ExecuteFlag(GetEverythingAfter(ScriptLine, "flag "), Thread)
        ElseIf BeginsWith(ScriptLine, "create ") Then
            ExecuteCreate(GetEverythingAfter(ScriptLine, "create "), Thread)
        ElseIf BeginsWith(ScriptLine, "destroy exit ") Then
            DestroyExit(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "repeat ") Then
            ExecuteRepeat(GetEverythingAfter(ScriptLine, "repeat "), Thread)
        ElseIf BeginsWith(ScriptLine, "enter ") Then
            ExecuteEnter(ScriptLine, Thread)
        ElseIf BeginsWith(ScriptLine, "displaytext ") Then
            DisplayTextSection(RetrieveParameter(ScriptLine, Thread), Thread, "normal")
        ElseIf BeginsWith(ScriptLine, "helpdisplaytext ") Then
            DisplayTextSection(RetrieveParameter(ScriptLine, Thread), Thread, "help")
        ElseIf BeginsWith(ScriptLine, "font ") Then
            SetFont(RetrieveParameter(ScriptLine, Thread), Thread)
        ElseIf BeginsWith(ScriptLine, "pause ") Then
            Pause(CInt(RetrieveParameter(ScriptLine, Thread)))
        ElseIf Trim(LCase(ScriptLine)) = "clear" Then
            DoClear()
        ElseIf Trim(LCase(ScriptLine)) = "helpclear" Then
            ' This command does nothing in the Quest 5 player, as there is no separate help window
        ElseIf BeginsWith(ScriptLine, "background ") Then
            SetBackground(RetrieveParameter(ScriptLine, Thread))
        ElseIf BeginsWith(ScriptLine, "foreground ") Then
            SetForeground(RetrieveParameter(ScriptLine, Thread))
        ElseIf Trim(LCase(ScriptLine)) = "nointro" Then
            AutoIntro = False
        ElseIf BeginsWith(ScriptLine, "debug ") Then
            LogASLError(RetrieveParameter(ScriptLine, Thread), LOGTYPE_MISC)
        ElseIf BeginsWith(ScriptLine, "mailto ") Then
            Dim emailAddress As String = RetrieveParameter(ScriptLine, Thread)
            RaiseEvent PrintText("<a target=""_blank"" href=""mailto:" + emailAddress + """>" + emailAddress + "</a>")
        ElseIf BeginsWith(ScriptLine, "shell ") And GameASLVersion < 410 Then
            LogASLError("'shell' is not supported in this version of Quest", LOGTYPE_WARNINGERROR)
        ElseIf BeginsWith(ScriptLine, "shellexe ") And GameASLVersion < 410 Then
            LogASLError("'shellexe' is not supported in this version of Quest", LOGTYPE_WARNINGERROR)
        ElseIf BeginsWith(ScriptLine, "wait") Then
            ExecuteWait(Trim(GetEverythingAfter(Trim(ScriptLine), "wait")), Thread)
        ElseIf BeginsWith(ScriptLine, "timeron ") Then
            SetTimerState(RetrieveParameter(ScriptLine, Thread), True)
        ElseIf BeginsWith(ScriptLine, "timeroff ") Then
            SetTimerState(RetrieveParameter(ScriptLine, Thread), False)
        ElseIf Trim(LCase(ScriptLine)) = "outputon" Then
            OutPutOn = True
            UpdateObjectList(Thread)
            UpdateItems(Thread)
        ElseIf Trim(LCase(ScriptLine)) = "outputoff" Then
            OutPutOn = False
        ElseIf Trim(LCase(ScriptLine)) = "panes off" Then
            m_player.SetPanesVisible("off")
        ElseIf Trim(LCase(ScriptLine)) = "panes on" Then
            m_player.SetPanesVisible("on")
        ElseIf BeginsWith(ScriptLine, "lock ") Then
            ExecuteLock(RetrieveParameter(ScriptLine, Thread), True)
        ElseIf BeginsWith(ScriptLine, "unlock ") Then
            ExecuteLock(RetrieveParameter(ScriptLine, Thread), False)
        ElseIf BeginsWith(ScriptLine, "playmod ") And GameASLVersion < 410 Then
            LogASLError("'playmod' is not supported in this version of Quest", LOGTYPE_WARNINGERROR)
        ElseIf BeginsWith(ScriptLine, "modvolume") And GameASLVersion < 410 Then
            LogASLError("'modvolume' is not supported in this version of Quest", LOGTYPE_WARNINGERROR)
        ElseIf Trim(LCase(ScriptLine)) = "dontprocess" Then
            Thread.DontProcessCommand = True
        ElseIf BeginsWith(ScriptLine, "return ") Then
            Thread.FunctionReturnValue = RetrieveParameter(ScriptLine, Thread)
        Else
            If BeginsWith(ScriptLine, "'") = False Then
                LogASLError("Unrecognized keyword. Line reads: '" & Trim(ReportErrorLine(ScriptLine)) & "'", LOGTYPE_WARNINGERROR)
            End If
        End If

        Exit Sub

ErrorHandler:
        Print("[An internal error occurred]", Thread)
        LogASLError(Err.Number & " - '" & Err.Description & "' occurred processing script line '" & ScriptLine & "'", LOGTYPE_INTERNALERROR)

    End Sub

    Private Sub ExecuteEnter(ByRef ScriptLine As String, ByRef Thread As ThreadData)
        CommandOverrideModeOn = True
        CommandOverrideVariable = RetrieveParameter(ScriptLine, Thread)

        ' Now, wait for CommandOverrideModeOn to be set
        ' to False by ExecCommand. Execution can then resume.

        ChangeState(State.Waiting, True)

        SyncLock m_commandLock
            System.Threading.Monitor.Wait(m_commandLock)
        End SyncLock

        CommandOverrideModeOn = False

        ' State will have been changed to Working when the user typed their response,
        ' and will be set back to Ready when the call to ExecCommand has finished

    End Sub

    Private Sub ExecuteSet(ByRef SetInstruction As String, ByRef Thread As ThreadData)

        Dim i As Integer

        Dim TimerInterval As String
        Dim SCPos As Integer
        Dim TimerName As String
        Dim FoundTimer As Boolean
        Dim Result As Integer
        If GameASLVersion >= 280 Then
            If BeginsWith(SetInstruction, "interval ") Then
                TimerInterval = RetrieveParameter(SetInstruction, Thread)
                SCPos = InStr(TimerInterval, ";")
                If SCPos = 0 Then
                    LogASLError("Too few parameters in 'set " & SetInstruction & "'", LOGTYPE_WARNINGERROR)
                    Exit Sub
                End If

                TimerName = Trim(Left(TimerInterval, SCPos - 1))
                TimerInterval = CStr(Val(Trim(Mid(TimerInterval, SCPos + 1))))

                For i = 1 To NumberTimers
                    If LCase(TimerName) = LCase(Timers(i).TimerName) Then
                        FoundTimer = True
                        Timers(i).TimerInterval = CInt(TimerInterval)
                        i = NumberTimers
                    End If
                Next i

                If Not FoundTimer Then
                    LogASLError("No such timer '" & TimerName & "'", LOGTYPE_WARNINGERROR)
                    Exit Sub
                End If
            ElseIf BeginsWith(SetInstruction, "string ") Then
                ExecSetString(RetrieveParameter(SetInstruction, Thread), Thread)
            ElseIf BeginsWith(SetInstruction, "numeric ") Then
                ExecSetVar(RetrieveParameter(SetInstruction, Thread), Thread)
            ElseIf BeginsWith(SetInstruction, "collectable ") Then
                ExecuteSetCollectable(RetrieveParameter(SetInstruction, Thread), Thread)
            Else
                Result = SetUnknownVariableType(RetrieveParameter(SetInstruction, Thread), Thread)
                If Result = SET_ERROR Then
                    LogASLError("Error on setting 'set " & SetInstruction & "'", LOGTYPE_WARNINGERROR)
                ElseIf Result = SET_UNFOUND Then
                    LogASLError("Variable type not specified in 'set " & SetInstruction & "'", LOGTYPE_WARNINGERROR)
                End If
            End If
        Else
            ExecuteSetCollectable(RetrieveParameter(SetInstruction, Thread), Thread)
        End If

    End Sub

    Private Function FindStatement(ByRef searchblock As DefineBlock, ByRef statement As String) As String
        Dim i As Integer

        ' Finds a statement within a given block of lines

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1

            ' Ignore sub-define blocks
            If BeginsWith(Lines(i), "define ") Then
                Do
                    i = i + 1
                Loop Until Trim(Lines(i)) = "end define"
            End If
            ' Check to see if the line matches the statement
            ' that is begin searched for
            If BeginsWith(Lines(i), statement) Then
                ' Return the parameters between < and > :
                Return RetrieveParameter(Lines(i), NullThread)
            End If
        Next i

        Return ""

    End Function

    Private Function FindLine(ByRef searchblock As DefineBlock, ByRef statement As String, ByRef statementparam As String) As String
        Dim i As Integer
        ' Finds a statement within a given block of lines

        For i = searchblock.StartLine + 1 To searchblock.EndLine - 1

            ' Ignore sub-define blocks
            If BeginsWith(Lines(i), "define ") Then
                Do
                    i = i + 1
                Loop Until Trim(Lines(i)) = "end define"
            End If
            ' Check to see if the line matches the statement
            ' that is begin searched for
            If BeginsWith(Lines(i), statement) Then
                If UCase(Trim(RetrieveParameter(Lines(i), NullThread))) = UCase(Trim(statementparam)) Then
                    Return Trim(Lines(i))
                End If
            End If
        Next i

        Return ""

    End Function

    Private Function GetCollectableAmount(ByRef colname As String) As Double
        Dim n As Double
        Dim i As Integer
        Dim foundval As Boolean
        foundval = False

        For i = 1 To NumCollectables
            If Collectables(i).collectablename = colname Then
                n = Collectables(i).collectablenumber
                foundval = True
                i = NumCollectables
            End If
        Next i

        GetCollectableAmount = n

    End Function

    Private Function GetSecondChunk(ByRef l As String) As String
        Dim EndOfFirstBit, LengthOfKeyword As Integer
        Dim SecondChunk As String

        EndOfFirstBit = InStr(l, ">") + 1
        LengthOfKeyword = (Len(l) - EndOfFirstBit) + 1
        SecondChunk = Trim(Mid(l, EndOfFirstBit, LengthOfKeyword))

        GetSecondChunk = SecondChunk
    End Function

    Private Sub GoDirection(ByRef Direction As String, ByRef Thread As ThreadData)
        ' leaves the current room in direction specified by
        ' 'direction'

        Dim NewRoom As String
        Dim SCP As Integer

        Dim RoomID As Integer
        Dim DirData As New TextAction
        RoomID = GetRoomID(CurrentRoom, Thread)

        If RoomID = 0 Then Exit Sub

        If GameASLVersion >= 410 Then
            Rooms(RoomID).Exits.ExecuteGo(Direction, Thread)
            Exit Sub
        End If

        With Rooms(RoomID)
            If Direction = "north" Then
                DirData = .North
            ElseIf Direction = "south" Then
                DirData = .South
            ElseIf Direction = "west" Then
                DirData = .West
            ElseIf Direction = "east" Then
                DirData = .East
            ElseIf Direction = "northeast" Then
                DirData = .NorthEast
            ElseIf Direction = "northwest" Then
                DirData = .NorthWest
            ElseIf Direction = "southeast" Then
                DirData = .SouthEast
            ElseIf Direction = "southwest" Then
                DirData = .SouthWest
            ElseIf Direction = "up" Then
                DirData = .Up
            ElseIf Direction = "down" Then
                DirData = .Down
            ElseIf Direction = "out" Then
                If .out.Script = "" Then
                    DirData.Data = .out.Text
                    DirData.Type = TA_TEXT
                Else
                    DirData.Data = .out.Script
                    DirData.Type = TA_SCRIPT
                End If
            End If
        End With

        If DirData.Type = TA_SCRIPT And DirData.Data <> "" Then
            ExecuteScript(DirData.Data, Thread)
        ElseIf DirData.Data <> "" Then
            NewRoom = DirData.Data
            SCP = InStr(NewRoom, ";")
            If SCP <> 0 Then
                NewRoom = Trim(Mid(NewRoom, SCP + 1))
            End If
            PlayGame(NewRoom, Thread)
        Else
            If Direction = "out" Then
                PlayerErrorMessage(ERROR_DEFAULTOUT, Thread)
            Else
                PlayerErrorMessage(ERROR_BADPLACE, Thread)
            End If
        End If

    End Sub

    Private Sub GoToPlace(ByRef placecheck As String, ByRef Thread As ThreadData)
        ' leaves the current room in direction specified by
        ' 'direction'

        Dim DisallowedDirection As Boolean
        Dim NP As String, Destination As String = "", P, s As String
        DisallowedDirection = False

        P = PlaceExist(placecheck, Thread)

        If P <> "" Then
            Destination = P
        ElseIf BeginsWith(placecheck, "the ") Then
            NP = GetEverythingAfter(placecheck, "the ")
            P = PlaceExist(NP, Thread)
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
                ExecuteScript(s, Thread)
            Else
                PlayGame(Destination, Thread)
            End If
        End If

        If DisallowedDirection = True Then
            PlayerErrorMessage(ERROR_BADPLACE, Thread)
        End If

    End Sub

    Private Function InitialiseGame(ByRef afilename As String, Optional ByRef LoadedFromQSG As Boolean = False) As Boolean
        Dim GameType, ErrorString As String
        Dim i As Integer
        Dim ASLVersion As String

        m_loadedFromQSG = LoadedFromQSG

        m_oChangeLogRooms = New ChangeLog
        m_oChangeLogObjects = New ChangeLog
        m_oChangeLogRooms.AppliesToType = ChangeLog.eAppliesToType.atRoom
        m_oChangeLogObjects.AppliesToType = ChangeLog.eAppliesToType.atObject

        OutPutOn = True
        goptAbbreviations = True

        GamePath = System.IO.Path.GetDirectoryName(afilename) + "\"

        LogASLError("Quest ASL4 Interpreter " & QuestVersion, LOGTYPE_INIT)

        Dim LogMsg As String
        LogMsg = "Opening file " & afilename & " on " & Date.Now.ToString()
        LogASLError(LogMsg, LOGTYPE_INIT)

        ' Parse file and find where the 'define' blocks are:
        If ParseFile(afilename) = False Then
            LogASLError("Unable to open file", LOGTYPE_INIT)
            ErrorString = "Unable to open " & afilename

            If OpenErrorReport <> "" Then
                ' Strip last vbcrlf
                OpenErrorReport = Left(OpenErrorReport, Len(OpenErrorReport) - 2)
                ErrorString = ErrorString & ":" & vbCrLf & vbCrLf & OpenErrorReport
            End If

            Print("Error: " & ErrorString, NullThread)

            InitialiseGame = False

            Exit Function
        End If

        ' Check version
        Dim gameblock As DefineBlock
        gameblock = GetDefineBlock("game")

        ASLVersion = "//"
        GameType = ""
        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), "asl-version ") Then
                ASLVersion = RetrieveParameter(Lines(i), NullThread)
            ElseIf BeginsWith(Lines(i), "gametype ") Then
                GameType = GetEverythingAfter(Lines(i), "gametype ")
            End If
        Next i

        If ASLVersion = "//" Then
            LogASLError("File contains no version header.", LOGTYPE_WARNINGERROR)
        Else
            GameASLVersion = CInt(ASLVersion)

            If InStr(RecognisedVersions, "/" & ASLVersion & "/") = 0 Then
                LogASLError("Unrecognised ASL version number.", LOGTYPE_WARNINGERROR)
            End If
        End If

        m_listVerbs.Add(ListType.ExitsList, New List(Of String)(New String() {"Go to"}))

        If GameASLVersion >= 280 And GameASLVersion < 390 Then
            m_listVerbs.Add(ListType.ObjectsList, New List(Of String)(New String() {"Look at", "Examine", "Take", "Speak to"}))
            m_listVerbs.Add(ListType.InventoryList, New List(Of String)(New String() {"Look at", "Examine", "Use", "Drop"}))
        Else
            m_listVerbs.Add(ListType.ObjectsList, New List(Of String)(New String() {"Look at", "Take", "Speak to"}))
            m_listVerbs.Add(ListType.InventoryList, New List(Of String)(New String() {"Look at", "Use", "Drop"}))
        End If

        ' Get the name of the game:
        GameName = RetrieveParameter(Lines(GetDefineBlock("game").StartLine), NullThread)

        m_player.UpdateGameName(GameName)
        m_player.Show("Panes")
        m_player.Show("Location")
        m_player.Show("Command")

        SetUpGameObject()
        SetUpOptions()

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(i), "beforesave ") Then
                BeforeSaveScript = GetEverythingAfter(Lines(i), "beforesave ")
            ElseIf BeginsWith(Lines(i), "onload ") Then
                OnLoadScript = GetEverythingAfter(Lines(i), "onload ")

            End If
        Next i

        SetDefaultPlayerErrorMessages()

        SetUpSynonyms()
        SetUpRoomData()

        If GameASLVersion >= 410 Then
            SetUpExits()
        End If

        If GameASLVersion < 280 Then
            ' Set up an array containing the names of all the items
            ' used in the game, based on the possitems statement
            ' of the 'define game' block.
            SetUpItemArrays()
        End If

        If GameASLVersion < 280 Then
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

        GameFileName = afilename

        LogASLError("Finished loading file.", LOGTYPE_INIT)

        m_oDefaultRoomProperties = GetPropertiesInType("defaultroom", False)
        m_oDefaultProperties = GetPropertiesInType("default", False)

        InitialiseGame = True

    End Function

    Private Sub LeaveRoom(ByRef Thread As ThreadData)
        ' leaves the current room
        GoDirection("out", Thread)
    End Sub

    Private Function PlaceExist(ByRef PlaceName As String, ByRef Thread As ThreadData) As String

        ' Returns actual name of an available "place" exit, and if
        ' script is executed on going in that direction, that script
        ' is returned after a ";"

        Dim RoomID As Integer
        Dim CheckPlace As String
        Dim DestRoomID As Integer
        Dim CheckPlaceName As String
        Dim FoundPlace, ScriptPresent As Boolean
        Dim i As Integer

        RoomID = GetRoomID(CurrentRoom, Thread)
        FoundPlace = False
        ScriptPresent = False

        ' check if place is available
        With Rooms(RoomID)

            For i = 1 To .NumberPlaces
                CheckPlace = .Places(i).PlaceName

                'remove any prefix and semicolon
                If InStr(CheckPlace, ";") > 0 Then
                    CheckPlace = Trim(Right(CheckPlace, Len(CheckPlace) - (InStr(CheckPlace, ";") + 1)))
                End If

                CheckPlaceName = CheckPlace

                If GameASLVersion >= 311 And .Places(i).Script = "" Then
                    DestRoomID = GetRoomID(CheckPlace, Thread)
                    If DestRoomID <> 0 Then
                        If Rooms(DestRoomID).RoomAlias <> "" Then
                            CheckPlaceName = Rooms(DestRoomID).RoomAlias
                        End If
                    End If
                End If

                If LCase(CheckPlaceName) = LCase(PlaceName) Then
                    FoundPlace = True

                    If .Places(i).Script <> "" Then
                        Return CheckPlace & ";" & .Places(i).Script
                    Else
                        Return CheckPlace
                    End If
                End If
            Next i

        End With

        Return ""

    End Function

    Private Sub PlayerItem(ByRef anitem As String, ByRef gotit As Boolean, ByRef Thread As ThreadData, Optional ByRef ObjID As Integer = 0)
        ' Gives the player an item (if gotit=True) or takes an
        ' item away from the player (if gotit=False).

        ' If ASL>280, setting gotit=TRUE moves specified
        ' *object* to room "inventory"; setting gotit=FALSe
        ' drops object into current room.

        Dim FoundObjectName As Boolean
        FoundObjectName = False
        Dim OldRoom As String
        Dim i As Integer

        If GameASLVersion >= 280 Then
            If ObjID = 0 Then
                For i = 1 To NumberObjs
                    If LCase(Objs(i).ObjectName) = LCase(anitem) Then
                        ObjID = i
                        i = NumberObjs
                    End If
                Next i
            End If

            If ObjID <> 0 Then
                OldRoom = LCase(Objs(ObjID).ContainerRoom)
                If gotit Then
                    If GameASLVersion >= 391 Then
                        ' Unset parent information, if any
                        AddToObjectProperties("not parent", ObjID, Thread)
                    End If
                    MoveThing(Objs(ObjID).ObjectName, "inventory", QUEST_OBJECT, Thread)

                    If Objs(ObjID).GainScript <> "" Then
                        ExecuteScript(Objs(ObjID).GainScript, Thread)
                    End If
                Else
                    MoveThing(Objs(ObjID).ObjectName, CurrentRoom, QUEST_OBJECT, Thread)

                    If Objs(ObjID).LoseScript <> "" Then
                        ExecuteScript(Objs(ObjID).LoseScript, Thread)
                    End If

                End If

                FoundObjectName = True
            End If

            If Not FoundObjectName Then
                LogASLError("No such object '" & anitem & "'", LOGTYPE_WARNINGERROR)
            Else
                UpdateItems(Thread)
                UpdateObjectList(Thread)
            End If
        Else
            For i = 1 To NumberItems
                If Items(i).itemname = anitem Then
                    Items(i).gotitem = gotit
                    i = NumberItems
                End If
            Next i

            UpdateItems(Thread)

        End If

    End Sub

    Friend Function PlayGame(ByRef Room As String, ByRef Thread As ThreadData) As Boolean
        'plays the specified room

        Dim RoomID As Integer
        Dim RoomScript As String
        RoomID = GetRoomID(Room, Thread)

        If RoomID = 0 Then
            LogASLError("No such room '" & Room & "'", LOGTYPE_WARNINGERROR)
            Exit Function
        End If

        Dim LastRoom As String
        LastRoom = CurrentRoom

        CurrentRoom = Room

        SetStringContents("quest.currentroom", Room, Thread)

        If GameASLVersion >= 391 And GameASLVersion < 410 Then
            AddToObjectProperties("visited", Rooms(RoomID).ObjID, Thread)
        End If

        ShowRoomInfo((Room), Thread)

        UpdateItems(Thread)

        ' Find script lines and execute them.

        If Rooms(RoomID).Script <> "" Then
            RoomScript = Rooms(RoomID).Script
            ExecuteScript(RoomScript, Thread)
        End If

        If GameASLVersion >= 410 Then
            AddToObjectProperties("visited", Rooms(RoomID).ObjID, Thread)
        End If

    End Function

    Friend Sub Print(ByRef txt As String, ByRef Thread As ThreadData, Optional ByRef OutputTo As String = "normal", Optional ByRef NoTalk As Boolean = False)

        ' Where we have |w (wait) and |c (clear screen) codes, we don't
        ' send these to the client. Instead, we send the text without these codes. After
        ' printing we then run the "wait <>" and "clear" script commands as appropriate.
        ' This function removes the codes from InputString, passed ByRef. The output
        ' of the function is a string e.g. "wc" means to run a wait command and then
        ' clear the screen.

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
                    'CodesList = CodesList & "w"
                    'InputString = Left$(InputString, i - 1) & Mid$(InputString, i + 2)
                    DoPrint(PrintString)
                    PrintString = ""
                    PrintThis = False
                    i = i + 1

                    ExecuteScript("wait <>", Thread)

                ElseIf Mid(txt, i, 2) = "|c" Then
                    Select Case Mid(txt, i, 3)
                        Case "|cb", "|cr", "|cl", "|cy", "|cg"
                            ' Do nothing - we don't want to remove the colour formatting codes.
                        Case Else
                            'CodesList = CodesList & "c"
                            'InputString = Left$(InputString, i - 1) & Mid$(InputString, i + 2)
                            DoPrint(PrintString)
                            PrintString = ""
                            PrintThis = False
                            i = i + 1

                            ExecuteScript("clear", Thread)
                    End Select
                End If

                If PrintThis Then PrintString = PrintString & Mid(txt, i, 1)

            Next i

            If PrintString <> "" Then DoPrint(PrintString)
        End If
    End Sub

    Private Function RetrLine(ByRef BlockType As String, ByRef blockparam As String, ByRef lineret As String, ByRef Thread As ThreadData) As String
        'retrieves the line lineret in the block of type blocktype
        'with parameter blockparam in the current room/game block

        Dim roomblock As DefineBlock
        Dim i As Integer
        roomblock = DefineBlockParam("room", CurrentRoom)
        Dim nonefound As Boolean
        Dim searchblock As DefineBlock
        Dim NothingSaid As Boolean
        Dim DefLine As String

        nonefound = True
        NothingSaid = False

        RetrLine = "<unfound>"

        DefLine = "define " & BlockType

        If BlockType = "object" Then
            searchblock = GetThingBlock(blockparam, CurrentRoom, QUEST_OBJECT)
        Else
            searchblock = GetThingBlock(blockparam, CurrentRoom, QUEST_CHARACTER)
        End If

        If searchblock.StartLine <> 0 And searchblock.EndLine <> 0 Then
            For i = searchblock.StartLine + 1 To searchblock.EndLine - 1
                If BeginsWith(Lines(i), lineret) Then
                    RetrLine = Trim(Lines(i))
                End If
            Next i
        End If

        If searchblock.StartLine = 0 And searchblock.EndLine = 0 Then
            RetrLine = "<undefined>"
        End If


    End Function

    Private Function RetrLineParam(ByRef BlockType As String, ByRef blockparam As String, ByRef lineret As String, ByRef lineparam As String, ByRef Thread As ThreadData) As String
        'retrieves the line lineret with parameter lineparam
        'in the block of type blocktype with parameter blockparam
        'in the current room - of course.

        Dim roomblock, searchblock As DefineBlock
        Dim i As Integer
        roomblock = DefineBlockParam("room", CurrentRoom)
        Dim nonefound, FinishLoop, NothingSaid As Boolean
        Dim DefLine As String

        RetrLineParam = ""
        nonefound = True
        NothingSaid = False

        DefLine = "define " & BlockType

        If BlockType = "object" Then
            searchblock = GetThingBlock(blockparam, CurrentRoom, QUEST_OBJECT)
        Else
            searchblock = GetThingBlock(blockparam, CurrentRoom, QUEST_CHARACTER)
        End If

        If searchblock.StartLine <> 0 And searchblock.EndLine <> 0 Then
            For i = searchblock.StartLine + 1 To searchblock.EndLine - 1
                If BeginsWith(LCase(Lines(i)), LCase(lineret)) Then
                    If LCase(RetrieveParameter(Lines(i), Thread)) = LCase(lineparam) Then
                        RetrLineParam = Trim(Lines(i))
                        FinishLoop = True
                        nonefound = False
                    End If
                End If
            Next i
        End If

        If searchblock.StartLine = 0 And searchblock.EndLine = 0 Then
            RetrLineParam = "<undefined>"
        ElseIf nonefound = True Then
            RetrLineParam = "<unfound>"
        End If

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

        NumCollectables = 0

        ' Initialise collectables:
        ' First, find the collectables section within the define
        ' game block, and get its parameters:

        For a = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(a), "collectables ") Then
                DisplayStatus = True
                PossItems = Trim(RetrieveParameter(Lines(a), NullThread, False))

                ' if collectables is a null string, there are no
                ' collectables. Otherwise, there is one more object than
                ' the number of commas. So, first check to see if we have
                ' no objects:

                If PossItems <> "" Then
                    NumCollectables = 1
                    CharPos = 1
                    Do
                        ReDim Preserve Collectables(NumCollectables)
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
                        Collectables(NumCollectables).collectablename = Trim(Left(CInfo, InStr(CInfo, " ")))

                        EqualsPos = InStr(CInfo, "=")
                        SpacePos = InStr(CInfo, " ")
                        Space2Pos = InStr(EqualsPos, CInfo, " ")
                        If Space2Pos = 0 Then Space2Pos = Len(CInfo) + 1
                        T = Trim(Mid(CInfo, SpacePos + 1, EqualsPos - SpacePos - 1))
                        i = Trim(Mid(CInfo, EqualsPos + 1, Space2Pos - EqualsPos - 1))

                        If Left(T, 1) = "d" Then
                            T = Mid(T, 2)
                            Collectables(NumCollectables).DisplayWhenZero = False
                        Else
                            Collectables(NumCollectables).DisplayWhenZero = True
                        End If

                        Collectables(NumCollectables).collectabletype = T
                        Collectables(NumCollectables).collectablenumber = Val(i)

                        ' Get display string between square brackets
                        Bpos1 = InStr(CInfo, "[")
                        BPos2 = InStr(CInfo, "]")
                        If Bpos1 = 0 Then
                            Collectables(NumCollectables).collectabledisplay = "<def>"
                        Else
                            b = Mid(CInfo, Bpos1 + 1, (BPos2 - 1) - Bpos1)
                            Collectables(NumCollectables).collectabledisplay = Trim(b)
                        End If

                        CharPos = NextComma + 1
                        NumCollectables = NumCollectables + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                    NumCollectables = NumCollectables - 1
                End If
            End If
        Next a
    End Sub

    Private Sub SetUpItemArrays()

        Dim LastItem As Boolean
        Dim CharPos, a, NextComma As Integer
        Dim PossItems As String
        LastItem = False

        NumberItems = 0

        ' Initialise items:
        ' First, find the possitems section within the define game
        ' block, and get its parameters:
        For a = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(a), "possitems ") Or BeginsWith(Lines(a), "items ") Then
                PossItems = RetrieveParameter(Lines(a), NullThread)

                If PossItems <> "" Then
                    NumberItems = NumberItems + 1
                    CharPos = 1
                    Do
                        ReDim Preserve Items(NumberItems)
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
                        Items(NumberItems).itemname = Trim(Mid(PossItems, CharPos, NextComma - CharPos))
                        Items(NumberItems).gotitem = False

                        CharPos = NextComma + 1
                        NumberItems = NumberItems + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                    NumberItems = NumberItems - 1
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
            If BeginsWith(Lines(a), "startitems ") Then
                StartItems = RetrieveParameter(Lines(a), NullThread)

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
                        For i = 1 To NumberItems
                            If Items(i).itemname = TheItemName Then
                                Items(i).gotitem = True
                                i = NumberItems
                            End If
                        Next i

                        CharPos = NextComma + 1

                        'lastitem set when nextcomma=0, above.
                    Loop Until LastItem = True
                End If
            End If
        Next a
    End Sub

    Private Sub ShowHelp(ByRef Thread As ThreadData)
        ' In Quest 4 and below, the help text displays in a separate window. In Quest 5, it displays
        ' in the same window as the game text.
        Print("|b|cl|s14Quest Quick Help|xb|cb|s00", Thread, "help")
        Print("", Thread, "help")
        Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", Thread, "help")
        Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", Thread, "help")
        Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", Thread, "help")
        Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", Thread, "help")
        Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", Thread, "help")
        Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n", Thread, "help")
        Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", Thread, "help")
    End Sub

    Private Sub ReadCatalog(ByRef CatData As String)
        Dim i, Chr0Pos, ResourceStart As Integer

        Chr0Pos = InStr(CatData, Chr(0))
        NumResources = CInt(DecryptString(Left(CatData, Chr0Pos - 1)))
        ReDim Preserve Resources(NumResources)

        CatData = Mid(CatData, Chr0Pos + 1)

        ResourceStart = 0

        For i = 1 To NumResources
            With Resources(i)
                Chr0Pos = InStr(CatData, Chr(0))
                .ResourceName = DecryptString(Left(CatData, Chr0Pos - 1))
                CatData = Mid(CatData, Chr0Pos + 1)

                Chr0Pos = InStr(CatData, Chr(0))
                .ResourceLength = CInt(DecryptString(Left(CatData, Chr0Pos - 1)))
                CatData = Mid(CatData, Chr0Pos + 1)

                .ResourceStart = ResourceStart
                ResourceStart = ResourceStart + .ResourceLength

                .Extracted = False
            End With
        Next i

    End Sub

    Private Sub UpdateDirButtons(ByRef AvailableDirs As String, ByRef Thread As ThreadData)

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

        m_compassExits = compassExits
        UpdateExitsList()

    End Sub

    Private Sub AddCompassExit(exitList As List(Of ListData), name As String)
        exitList.Add(New ListData(name, m_listVerbs(ListType.ExitsList)))
    End Sub

    Private Function UpdateDoorways(ByRef RoomID As Integer, ByRef Thread As ThreadData) As String
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

        If GameASLVersion >= 410 Then
            Rooms(RoomID).Exits.GetAvailableDirectionsDescription(RoomDisplayText, Directions)
        Else

            If Rooms(RoomID).out.Text <> "" Then
                OutPlace = Rooms(RoomID).out.Text

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

            If Rooms(RoomID).North.Data <> "" Then
                NSEW = NSEW & "|b" & n & "|xb, "
                Directions = Directions & "n"
            End If
            If Rooms(RoomID).South.Data <> "" Then
                NSEW = NSEW & "|b" & s & "|xb, "
                Directions = Directions & "s"
            End If
            If Rooms(RoomID).East.Data <> "" Then
                NSEW = NSEW & "|b" & e & "|xb, "
                Directions = Directions & "e"
            End If
            If Rooms(RoomID).West.Data <> "" Then
                NSEW = NSEW & "|b" & W & "|xb, "
                Directions = Directions & "w"
            End If
            If Rooms(RoomID).NorthEast.Data <> "" Then
                NSEW = NSEW & "|b" & NE & "|xb, "
                Directions = Directions & "a"
            End If
            If Rooms(RoomID).NorthWest.Data <> "" Then
                NSEW = NSEW & "|b" & NW & "|xb, "
                Directions = Directions & "b"
            End If
            If Rooms(RoomID).SouthEast.Data <> "" Then
                NSEW = NSEW & "|b" & SE & "|xb, "
                Directions = Directions & "c"
            End If
            If Rooms(RoomID).SouthWest.Data <> "" Then
                NSEW = NSEW & "|b" & SW & "|xb, "
                Directions = Directions & "d"
            End If
            If Rooms(RoomID).Up.Data <> "" Then
                NSEW = NSEW & "|b" & U & "|xb, "
                Directions = Directions & "u"
            End If
            If Rooms(RoomID).Down.Data <> "" Then
                NSEW = NSEW & "|b" & D & "|xb, "
                Directions = Directions & "f"
            End If

            If OutPlace <> "" Then
                'see if outside has an alias

                OutPlaceAlias = Rooms(GetRoomID(OutPlaceName, Thread)).RoomAlias
                If OutPlaceAlias = "" Then
                    OutPlaceAlias = OutPlace
                Else
                    If GameASLVersion >= 360 Then
                        If OutPlacePrefix <> "" Then
                            OutPlaceAlias = OutPlacePrefix & " " & OutPlaceAlias
                        End If
                    End If
                End If

                RoomDisplayText = RoomDisplayText & "You can go |bout|xb to " & OutPlaceAlias & "."
                If NSEW <> "" Then RoomDisplayText = RoomDisplayText & " "

                Directions = Directions & "o"
                If GameASLVersion >= 280 Then
                    SetStringContents("quest.doorways.out", OutPlaceName, Thread)
                Else
                    SetStringContents("quest.doorways.out", OutPlaceAlias, Thread)
                End If
                SetStringContents("quest.doorways.out.display", OutPlaceAlias, Thread)
            Else
                SetStringContents("quest.doorways.out", "", Thread)
                SetStringContents("quest.doorways.out.display", "", Thread)
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
                SetStringContents("quest.doorways.dirs", NSEW, Thread)
            Else
                SetStringContents("quest.doorways.dirs", "", Thread)
            End If
        End If

        UpdateDirButtons(Directions, Thread)

        UpdateDoorways = RoomDisplayText

    End Function

    Private Sub UpdateItems(ByRef Thread As ThreadData)

        ' displays the items a player has
        Dim i, j As Integer
        Dim k As String
        Dim invList As New List(Of ListData)

        If Not OutPutOn Then Exit Sub

        Dim CurObjName As String

        If GameASLVersion >= 280 Then
            For i = 1 To NumberObjs
                If Objs(i).ContainerRoom = "inventory" And Objs(i).Exists And Objs(i).Visible Then
                    If Objs(i).ObjectAlias = "" Then
                        CurObjName = Objs(i).ObjectName
                    Else
                        CurObjName = Objs(i).ObjectAlias
                    End If

                    invList.Add(New ListData(CapFirst(CurObjName), m_listVerbs(ListType.InventoryList)))

                End If
            Next i
        Else
            For j = 1 To NumberItems
                If Items(j).gotitem = True Then
                    invList.Add(New ListData(CapFirst(Items(j).itemname), m_listVerbs(ListType.InventoryList)))
                End If
            Next j
        End If

        RaiseEvent UpdateList(ListType.InventoryList, invList)

        If GameASLVersion >= 284 Then
            UpdateStatusVars(Thread)
        Else
            If NumCollectables > 0 Then

                Dim status As String = ""

                For j = 1 To NumCollectables
                    k = DisplayCollectableInfo(j)
                    If k <> "<null>" Then
                        If status.Length > 0 Then status += Environment.NewLine
                        status += k
                    End If
                Next j

                m_player.SetStatusText(status)

            End If
        End If

    End Sub

    Private Sub FinishGame(ByRef wingame As Integer, ByRef Thread As ThreadData)

        If wingame = STOPGAME_WIN Then
            DisplayTextSection("win", Thread)
        ElseIf wingame = STOPGAME_LOSE Then
            DisplayTextSection("lose", Thread)
        End If

        GameFinished()

    End Sub

    Private Sub UpdateObjectList(ByRef Thread As ThreadData)

        ' Updates object list
        Dim i, PlaceID As Integer
        Dim ShownPlaceName As String
        Dim ObjSuffix As String, CharsViewable As String = ""
        Dim CharsFound As Integer
        Dim NoFormatObjsViewable, CharList As String, ObjsViewable As String = ""
        Dim ObjsFound As Integer
        Dim ObjListString, NFObjListString As String

        If Not OutPutOn Then Exit Sub

        Dim objList As New List(Of ListData)
        Dim exitList As New List(Of ListData)

        'find the room
        Dim roomblock As DefineBlock
        roomblock = DefineBlockParam("room", CurrentRoom)

        'FIND CHARACTERS ===
        If GameASLVersion < 281 Then
            ' go through Chars() array
            For i = 1 To NumberChars
                If Chars(i).ContainerRoom = CurrentRoom And Chars(i).Exists And Chars(i).Visible Then
                    AddToObjectList(objList, exitList, Chars(i).ObjectName, QUEST_CHARACTER)
                    CharsViewable = CharsViewable & Chars(i).Prefix & "|b" & Chars(i).ObjectName & "|xb" & Chars(i).Suffix & ", "
                    CharsFound = CharsFound + 1
                End If
            Next i

            If CharsFound = 0 Then
                SetStringContents("quest.characters", "", Thread)
            Else
                'chop off final comma and add full stop (.)
                CharList = Left(CharsViewable, Len(CharsViewable) - 2)
                SetStringContents("quest.characters", CharList, Thread)
            End If
        End If

        'FIND OBJECTS
        NoFormatObjsViewable = ""
        Dim DisplayType As String

        For i = 1 To NumberObjs
            If LCase(Objs(i).ContainerRoom) = LCase(CurrentRoom) And Objs(i).Exists And Objs(i).Visible And Not Objs(i).IsExit Then
                ObjSuffix = Objs(i).Suffix
                If ObjSuffix <> "" Then ObjSuffix = " " & ObjSuffix
                DisplayType = Objs(i).DisplayType
                If Objs(i).ObjectAlias = "" Then
                    AddToObjectList(objList, exitList, Objs(i).ObjectName, QUEST_OBJECT, DisplayType)
                    ObjsViewable = ObjsViewable & Objs(i).Prefix & "|b" & Objs(i).ObjectName & "|xb" & ObjSuffix & ", "
                    NoFormatObjsViewable = NoFormatObjsViewable & Objs(i).Prefix & Objs(i).ObjectName & ", "
                Else
                    AddToObjectList(objList, exitList, Objs(i).ObjectAlias, QUEST_OBJECT, DisplayType)
                    ObjsViewable = ObjsViewable & Objs(i).Prefix & "|b" & Objs(i).ObjectAlias & "|xb" & ObjSuffix & ", "
                    NoFormatObjsViewable = NoFormatObjsViewable & Objs(i).Prefix & Objs(i).ObjectAlias & ", "
                End If
                ObjsFound = ObjsFound + 1
            End If
        Next i

        If ObjsFound <> 0 Then
            ObjListString = Left(ObjsViewable, Len(ObjsViewable) - 2)
            NFObjListString = Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2)
            ObjsViewable = "There is " & ObjListString & "."
            SetStringContents("quest.objects", Left(NoFormatObjsViewable, Len(NoFormatObjsViewable) - 2), Thread)
            SetStringContents("quest.formatobjects", ObjListString, Thread)
        Else
            SetStringContents("quest.objects", "", Thread)
            SetStringContents("quest.formatobjects", "", Thread)
        End If

        'FIND DOORWAYS
        Dim RoomID As Integer
        RoomID = GetRoomID(CurrentRoom, Thread)

        With Rooms(RoomID)

            If GameASLVersion >= 410 Then

                If RoomID > 0 Then
                    For Each oExit As RoomExit In Rooms(RoomID).Exits.Places.Values
                        AddToObjectList(objList, exitList, oExit.DisplayName, QUEST_ROOM)
                    Next
                End If

            Else
                For i = 1 To .NumberPlaces

                    If GameASLVersion >= 311 And Rooms(RoomID).Places(i).Script = "" Then
                        PlaceID = GetRoomID(Rooms(RoomID).Places(i).PlaceName, Thread)
                        If PlaceID = 0 Then
                            ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
                        Else
                            If Rooms(PlaceID).RoomAlias <> "" Then
                                ShownPlaceName = Rooms(PlaceID).RoomAlias
                            Else
                                ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
                            End If
                        End If
                    Else
                        ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
                    End If

                    AddToObjectList(objList, exitList, ShownPlaceName, QUEST_ROOM)
                Next i
            End If

        End With

        RaiseEvent UpdateList(ListType.ObjectsList, objList)
        m_gotoExits = exitList
        UpdateExitsList()

    End Sub

    Private Sub UpdateExitsList()
        ' The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        ' ASL4 code produces these separately. So we keep track of them separately and then
        ' merge to send to the Player.

        Dim mergedList As New List(Of ListData)

        For Each listItem As ListData In m_compassExits
            mergedList.Add(listItem)
        Next

        For Each listItem As ListData In m_gotoExits
            mergedList.Add(listItem)
        Next

        RaiseEvent UpdateList(ListType.ExitsList, mergedList)
    End Sub

    Private Sub UpdateStatusVars(ByRef Thread As ThreadData)
        Dim DisplayData As String
        Dim i As Integer
        Dim status As String = ""

        If NumDisplayStrings > 0 Then
            For i = 1 To NumDisplayStrings
                DisplayData = DisplayStatusVariableInfo(i, VARTYPE_STRING, Thread)

                If DisplayData <> "" Then
                    If status.Length > 0 Then status += Environment.NewLine
                    status += DisplayData
                End If
            Next i
        End If

        If NumDisplayNumerics > 0 Then
            For i = 1 To NumDisplayNumerics
                DisplayData = DisplayStatusVariableInfo(i, VARTYPE_NUMERIC, Thread)
                If DisplayData <> "" Then
                    If status.Length > 0 Then status += Environment.NewLine
                    status += DisplayData
                End If
            Next i
        End If

        m_player.SetStatusText(status)

    End Sub


    Private Sub UpdateVisibilityInContainers(ByRef Thread As ThreadData, Optional OnlyParent As String = "")
        ' Use OnlyParent to only update objects that are contained by a specific parent

        Dim i, ParentID As Integer
        Dim Parent As String
        Dim ParentIsTransparent, ParentIsOpen, ParentIsSeen As Boolean
        Dim ParentIsSurface As Boolean

        If GameASLVersion < 391 Then Exit Sub

        If OnlyParent <> "" Then
            OnlyParent = LCase(OnlyParent)
            ParentID = GetObjectIDNoAlias(OnlyParent)

            ParentIsOpen = IsYes(GetObjectProperty("opened", ParentID, True, False))
            ParentIsTransparent = IsYes(GetObjectProperty("transparent", ParentID, True, False))
            ParentIsSeen = IsYes(GetObjectProperty("seen", ParentID, True, False))
            ParentIsSurface = IsYes(GetObjectProperty("surface", ParentID, True, False))
        End If

        For i = 1 To NumberObjs
            ' If object has a parent object
            Parent = GetObjectProperty("parent", i, False, False)

            If Parent <> "" Then

                ' Check if that parent is open, or transparent
                If OnlyParent = "" Then
                    ParentID = GetObjectIDNoAlias(Parent)
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

                        SetAvailability(Objs(i).ObjectName, True, Thread)
                    Else
                        SetAvailability(Objs(i).ObjectName, False, Thread)
                    End If

                End If
            End If
        Next i

    End Sub

    Private Function PlayerCanAccessObject(ByRef ObjID As Integer, Optional ByRef ObjectIsInContainer As Boolean = False, Optional ByRef ParentID As Integer = 0, Optional ByRef ErrorMsg As String = "", Optional ByRef colObjects As List(Of Integer) = Nothing) As Boolean
        ' Called to see if a player can interact with an object (take it, open it etc.).
        ' For example, if the object is on a surface which is inside a closed container,
        ' the object cannot be accessed.

        Dim Parent As String
        Dim ParentDisplayName As String

        Dim sHierarchy As String = ""
        If IsYes(GetObjectProperty("parent", ObjID, True, False)) Then

            ' Object is in a container...

            Parent = GetObjectProperty("parent", ObjID, False, False)
            ParentID = GetObjectIDNoAlias(Parent)

            ' But if it's a surface then it's OK

            If Not IsYes(GetObjectProperty("surface", ParentID, True, False)) And Not IsYes(GetObjectProperty("opened", ParentID, True, False)) Then
                ' Parent has no "opened" property, so it's closed. Hence
                ' object can't be accessed
                PlayerCanAccessObject = False

                If Objs(ParentID).ObjectAlias <> "" Then
                    ParentDisplayName = Objs(ParentID).ObjectAlias
                Else
                    ParentDisplayName = Objs(ParentID).ObjectName
                End If

                ErrorMsg = "inside closed " & ParentDisplayName

                Exit Function
            End If

            ' Is the parent itself accessible?
            If colObjects Is Nothing Then
                colObjects = New List(Of Integer)
            End If

            If colObjects.Contains(ParentID) Then
                ' We've already encountered this parent while recursively calling
                ' this function - we're in a loop of parents!
                For Each id As Integer In colObjects
                    sHierarchy = sHierarchy & Objs(id).ObjectName & " -> "
                Next
                sHierarchy = sHierarchy & Objs(ParentID).ObjectName
                LogASLError("Looped object parents detected: " & sHierarchy)
                PlayerCanAccessObject = False
                Exit Function
            End If

            colObjects.Add(ParentID)

            If Not PlayerCanAccessObject(ParentID, , , ErrorMsg, colObjects) Then
                PlayerCanAccessObject = False
                Exit Function
            End If

            PlayerCanAccessObject = True
            ObjectIsInContainer = True
        Else
            PlayerCanAccessObject = True
        End If

    End Function

    Private Function GetGoToExits(ByRef RoomID As Integer, ByRef Thread As ThreadData) As String

        Dim i As Integer
        Dim PlaceID As Integer
        Dim PlaceList As String = ""
        Dim ShownPrefix As String
        Dim ShownPlaceName As String

        For i = 1 To Rooms(RoomID).NumberPlaces
            If GameASLVersion >= 311 And Rooms(RoomID).Places(i).Script = "" Then
                PlaceID = GetRoomID(Rooms(RoomID).Places(i).PlaceName, Thread)
                If PlaceID = 0 Then
                    LogASLError("No such room '" & Rooms(RoomID).Places(i).PlaceName & "'", LOGTYPE_WARNINGERROR)
                    ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
                Else
                    If Rooms(PlaceID).RoomAlias <> "" Then
                        ShownPlaceName = Rooms(PlaceID).RoomAlias
                    Else
                        ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
                    End If
                End If
            Else
                ShownPlaceName = Rooms(RoomID).Places(i).PlaceName
            End If

            ShownPrefix = Rooms(RoomID).Places(i).Prefix
            If ShownPrefix <> "" Then ShownPrefix = ShownPrefix & " "

            PlaceList = PlaceList & ShownPrefix & "|b" & ShownPlaceName & "|xb, "
        Next i

        GetGoToExits = PlaceList

        Exit Function

    End Function

    Private Sub SetUpExits()
        ' Exits have to be set up after all the rooms have been initialised

        Dim i As Integer
        Dim j As Integer
        Dim sRoomName As String
        Dim lRoomID As Integer
        Dim NestedBlock As Integer

        For i = 1 To NumberSections
            If BeginsWith(Lines(DefineBlocks(i).StartLine), "define room ") Then
                sRoomName = RetrieveParameter(Lines(DefineBlocks(i).StartLine), NullThread)
                lRoomID = GetRoomID(sRoomName, NullThread)

                For j = DefineBlocks(i).StartLine + 1 To DefineBlocks(i).EndLine - 1
                    If BeginsWith(Lines(j), "define ") Then
                        'skip nested blocks
                        NestedBlock = 1
                        Do
                            j = j + 1
                            If BeginsWith(Lines(j), "define ") Then
                                NestedBlock = NestedBlock + 1
                            ElseIf Trim(Lines(j)) = "end define" Then
                                NestedBlock = NestedBlock - 1
                            End If
                        Loop Until NestedBlock = 0
                    End If

                    Rooms(lRoomID).Exits.AddExitFromTag(Lines(j))
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
        Dim lDir As eDirection

        asParams = Split(sTag, ";")
        If UBound(asParams) < 1 Then
            LogASLError("No exit specified in '" & sTag & "'", LOGTYPE_WARNINGERROR)
            Return New RoomExit(Me)
        End If

        sRoom = Trim(asParams(0))
        sExit = Trim(asParams(1))

        lRoomID = GetRoomID(sRoom, NullThread)

        If lRoomID = 0 Then
            LogASLError("Can't find room '" & sRoom & "'", LOGTYPE_WARNINGERROR)
            Return Nothing
        End If

        With Rooms(lRoomID).Exits
            lDir = .GetDirectionEnum(sExit)
            If lDir = eDirection.dirNone Then
                If .Places.ContainsKey(sExit) Then
                    Return .Places.Item(sExit)
                End If
            Else
                Return .GetDirectionExit(lDir)
            End If
        End With

        Return Nothing

    End Function

    Private Sub ExecuteLock(sTag As String, ByRef bLock As Boolean)
        Dim oExit As RoomExit

        oExit = FindExit(sTag)

        If oExit Is Nothing Then
            LogASLError("Can't find exit '" & sTag & "'", LOGTYPE_WARNINGERROR)
            Exit Sub
        End If

        oExit.IsLocked = bLock

        Exit Sub

    End Sub

    Public Sub Begin() Implements IASL.Begin
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf DoBegin))
        ChangeState(State.Working)
        runnerThread.Start()

        SyncLock m_stateLock
            While m_state = State.Working And Not m_gameFinished
                System.Threading.Monitor.Wait(m_stateLock)
            End While
        End SyncLock
    End Sub

    Private Sub DoBegin()
        Dim gameblock As DefineBlock = GetDefineBlock("game")
        Dim NewThread As New ThreadData
        Dim i As Integer

        SetFont("", NullThread)
        SetFontSize(0, NullThread)

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(i), "background ") Then
                SetBackground(RetrieveParameter(Lines(i), NullThread))
            End If
        Next i

        For i = GetDefineBlock("game").StartLine + 1 To GetDefineBlock("game").EndLine - 1
            If BeginsWith(Lines(i), "foreground ") Then
                SetForeground(RetrieveParameter(Lines(i), NullThread))
            End If
        Next i

        ' Execute any startscript command that appears in the
        ' "define game" block:

        AutoIntro = True

        ' For ASL>=391, we only run startscripts if LoadMethod is normal (i.e. we haven't started
        ' from a saved QSG file)

        If GameASLVersion < 391 Or (GameASLVersion >= 391 And GameLoadMethod = "normal") Then

            ' for GameASLVersion 311 and later, any library startscript is executed first:
            If GameASLVersion >= 311 Then
                ' We go through the game block executing these in reverse order, as
                ' the statements which are included last should be executed first.
                For i = gameblock.EndLine - 1 To gameblock.StartLine + 1 Step -1
                    If BeginsWith(Lines(i), "lib startscript ") Then
                        NewThread = NullThread
                        ExecuteScript(Trim(GetEverythingAfter(Trim(Lines(i)), "lib startscript ")), NewThread)
                    End If
                Next i
            End If

            For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
                If BeginsWith(Lines(i), "startscript ") Then
                    NewThread = NullThread
                    ExecuteScript(Trim(GetEverythingAfter(Trim(Lines(i)), "startscript")), NewThread)
                ElseIf BeginsWith(Lines(i), "lib startscript ") And GameASLVersion < 311 Then
                    NewThread = NullThread
                    ExecuteScript(Trim(GetEverythingAfter(Trim(Lines(i)), "lib startscript ")), NewThread)
                End If
            Next i

        End If

        GameFullyLoaded = True

        ' Display intro text
        If AutoIntro And GameLoadMethod = "normal" Then DisplayTextSection("intro", NullThread)

        ' Start game from room specified by "start" statement
        Dim StartRoom As String = ""
        For i = gameblock.StartLine + 1 To gameblock.EndLine - 1
            If BeginsWith(Lines(i), "start ") Then
                StartRoom = RetrieveParameter(Lines(i), NullThread)
            End If
        Next i

        If Not m_loadedFromQSG Then
            NewThread = NullThread
            PlayGame(StartRoom, NewThread)
            Print("", NullThread)
        Else
            UpdateItems(NullThread)

            Print("Restored saved game", NullThread)
            Print("", NullThread)
            PlayGame(CurrentRoom, NullThread)
            Print("", NullThread)

            If GameASLVersion >= 391 Then
                ' For ASL>=391, OnLoad is now run for all games.
                NewThread = NullThread
                ExecuteScript(OnLoadScript, NewThread)
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
            Return m_filename
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
            Return SaveGameFile
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

        If Not m_readyForCommand Then Exit Sub

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
        SyncLock m_stateLock
            While m_state = changedFromState And Not m_gameFinished
                System.Threading.Monitor.Wait(m_stateLock)
            End While
        End SyncLock

    End Sub

    Private Sub ProcessCommandInNewThread(command As Object)
        ' Process command, and change state to Ready if the command finished processing

        Try
            If ExecCommand(DirectCast(command, String), New ThreadData) Then
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
        m_player = player
        If LCase(Right(m_filename, 4)) = ".qsg" Or m_data IsNot Nothing Then
            Return OpenGame(m_filename)
        Else
            Return InitialiseGame(m_filename)
        End If
    End Function

    Private Sub GameFinished()
        m_gameFinished = True
        RaiseEvent Finished()
        ChangeState(State.Finished)

        ' In case we're in the middle of processing an "enter" command, nudge the thread along
        SyncLock m_commandLock
            System.Threading.Monitor.PulseAll(m_commandLock)
        End SyncLock

        SyncLock m_waitLock
            System.Threading.Monitor.PulseAll(m_waitLock)
        End SyncLock

        SyncLock m_stateLock
            System.Threading.Monitor.PulseAll(m_stateLock)
        End SyncLock

        Cleanup()
    End Sub

    Private Function GetResourcePath(filename As String) As String Implements IASL.GetResourcePath
        If Not ResourceFile Is Nothing AndAlso ResourceFile.Length > 0 Then
            Dim extractResult As String = ExtractFile(filename)
            Return extractResult
        End If
        Return System.IO.Path.Combine(GamePath, filename)
    End Function

    Private Sub Cleanup()
        DeleteDirectory(m_tempFolder)
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

        For i = 1 To NumberTimers
            If Timers(i).TimerActive Then
                If Timers(i).BypassThisTurn Then
                    ' don't trigger timer during the turn it was first enabled
                    Timers(i).BypassThisTurn = False
                Else
                    Timers(i).TimerTicks = Timers(i).TimerTicks + elapsedTime

                    If Timers(i).TimerTicks >= Timers(i).TimerInterval Then
                        Timers(i).TimerTicks = 0
                        TimerScripts.Add(Timers(i).TimerAction)
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
                ExecuteScript(script, NullThread)
            Catch ex As Exception
                LogException(ex)
            End Try
        Next

        ChangeState(State.Ready)
    End Sub

    Private Sub RaiseNextTimerTickRequest()

        Dim anyTimerActive As Boolean = False
        Dim nextTrigger As Integer = 60

        For i As Integer = 1 To NumberTimers
            If Timers(i).TimerActive Then
                anyTimerActive = True

                Dim thisNextTrigger As Integer = Timers(i).TimerInterval - Timers(i).TimerTicks
                If thisNextTrigger < nextTrigger Then
                    nextTrigger = thisNextTrigger
                End If
            End If
        Next i

        If Not anyTimerActive Then nextTrigger = 0
        If m_gameFinished Then nextTrigger = 0

        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger.ToString)

        RaiseEvent RequestNextTimerTick(nextTrigger)

    End Sub

    Private Sub ChangeState(newState As State)
        Dim acceptCommands As Boolean = (newState = State.Ready)
        ChangeState(newState, acceptCommands)
    End Sub

    Private Sub ChangeState(newState As State, acceptCommands As Boolean)
        m_readyForCommand = acceptCommands
        SyncLock m_stateLock
            m_state = newState
            System.Threading.Monitor.PulseAll(m_stateLock)
        End SyncLock
    End Sub

    Public Sub FinishWait() Implements IASL.FinishWait
        If (m_state <> State.Waiting) Then Exit Sub
        Dim runnerThread As New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf FinishWaitInNewThread))
        ChangeState(State.Working)
        runnerThread.Start()
        WaitForStateChange(State.Working)
    End Sub

    Private Sub FinishWaitInNewThread()
        SyncLock m_waitLock
            System.Threading.Monitor.PulseAll(m_waitLock)
        End SyncLock
    End Sub

    Public Sub FinishPause() Implements IASL.FinishPause
        FinishWait()
    End Sub

    Private m_menuResponse As String

    Private Function ShowMenu(menuData As MenuData) As String
        m_player.ShowMenu(menuData)
        ChangeState(State.Waiting)

        SyncLock m_waitLock
            System.Threading.Monitor.Wait(m_waitLock)
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

        SyncLock m_waitLock
            System.Threading.Monitor.PulseAll(m_waitLock)
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
            Return m_originalFilename
        End Get
    End Property

    Private Function GetOriginalFilenameForQSG() As String
        If m_originalFilename IsNot Nothing Then Return m_originalFilename
        Return GameFileName
    End Function

    Public Delegate Function UnzipFunctionDelegate(filename As String, <Runtime.InteropServices.Out()> ByRef tempDir As String) As String
    Private m_unzipFunction As UnzipFunctionDelegate

    Public Sub SetUnzipFunction(unzipFunction As UnzipFunctionDelegate)
        m_unzipFunction = unzipFunction
    End Sub

    Private Function GetUnzippedFile(filename As String) As String
        Dim tempDir As String = Nothing
        Dim result As String = m_unzipFunction.Invoke(filename, tempDir)
        m_tempFolder = tempDir
        Return result
    End Function

    Public Property TempFolder As String Implements IASL.TempFolder
        Get
            Return m_tempFolder
        End Get
        Set
            m_tempFolder = Value
        End Set
    End Property

    Public ReadOnly Property ASLVersion As Integer
        Get
            Return GameASLVersion
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
            If String.IsNullOrEmpty(GameFileName) Then Return Nothing
            If Config.ReadGameFileFromAzureBlob Then
                Return m_gameId
            End If
            Return TextAdventures.Utility.Utility.FileMD5Hash(GameFileName)
        End Get
    End Property

    Public Iterator Function GetResources() As IEnumerable(Of String)
        For i As Integer = 1 To NumResources
            Yield Resources(i).ResourceName
        Next
        If NumResources > 0 Then
            Yield "_game.cas"
        End If
    End Function

    Private Function GetResourcelessCAS() As Byte()
        Dim FileData As String = System.IO.File.ReadAllText(ResourceFile, System.Text.Encoding.GetEncoding(1252))
        Return System.Text.Encoding.GetEncoding(1252).GetBytes(Left(FileData, StartCatPos - 1))
    End Function

End Class