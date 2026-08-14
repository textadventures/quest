---
title: Functions
sidebar:
  order: 6
---

This is a list of most of the functions built in to Quest, grouped by type. For a list in alphabetical order, go [here](/functions/index_allfunctions).



- [Functions for Attributes](#attributes)

- [Functions for Variables](#variables)

- [Functions for Objects and Exits](#objects)

- [Timers and Turnscripts](#timers-and-turnscripts)

- [User Interface Functions](#ui)

- [List Functions](#list)

- [Scope Functions](#scope)

- [Dictionary Functions](#dictionary)

- [String Functions](#string)

- [Clothing Functions](#clothing)

- [Randomising Functions](#random)

- [General Functions](#general)

- [Core.aslx Functions](#core)

- [Internal Functions](#internal)

- [Mathematical Functions](#maths)



<a name="attributes"></a>Functions for Attributes
---------------------------------------

Functions for checking and setting attributes on objects.

* [DecreaseHealth](/functions/corelibrary/decreasehealth)
* [DecreaseMoney](/functions/corelibrary/decreasemoney)
* [DecreaseScore](/functions/corelibrary/decreasescore)
* [GetAttribute](/functions/getattribute)
* [GetAttributeNames](/functions/getattributenames)
* [GetBoolean](/functions/getboolean)
* [GetDouble](/functions/getdouble)
* [GetInt](/functions/getint)
* [GetString](/functions/getstring)
* [HasAttribute](/functions/hasattribute)
* [HasBoolean](/functions/hasboolean)
* [HasDelegateImplementation](/functions/hasdelegateimplementation)
* [HasDouble](/functions/hasdouble)
* [HasInt](/functions/hasint)
* [HasObject](/functions/hasobject)
* [HasScript](/functions/hasscript)
* [HasString](/functions/hasstring)
* [IncreaseHealth](/functions/corelibrary/increasehealth)
* [IncreaseMoney](/functions/corelibrary/increasemoney)
* [IncreaseScore](/functions/corelibrary/increasescore)
* [SetObjectFlagOff](/functions/corelibrary/setobjectflagoff)
* [SetObjectFlagOn](/functions/corelibrary/setobjectflagon)



<a name="variables"></a>Functions for Variables
---------------------------------------

Functions that check or change the values of variables (and can be used on attributes too).

* [Equal](/functions/equal)
* [IsDefined](/functions/isdefined)
* [IsDouble](/functions/isdouble)
* [IsInt](/functions/isint)
* [ToDouble](/functions/todouble)
* [ToInt](/functions/toint)
* [ToString](/functions/tostring)
* [TypeOf](/functions/typeof)



<a name="objects"></a>Functions for Objects and Exits
---------------------------------------

* [Clone](/functions/clone)
* [CloneObject](/functions/corelibrary/cloneobject)
* [CloneObjectAndMove](/functions/corelibrary/cloneobjectandmove)
* [CloneObjectAndMoveHere](/functions/corelibrary/cloneobjectandmovehere)
* [DoesInherit](/functions/doesinherit)
* [GetObject](/functions/getobject)
* [MakeObjectInvisible](/functions/corelibrary/makeobjectinvisible)
* [MakeObjectVisible](/functions/corelibrary/makeobjectvisible)
* [MoveObject](/functions/corelibrary/moveobject)
* [RemoveObject](/functions/corelibrary/removeobject)

* [CreateBiExits](/functions/createbiexits)
* [GetExitByLink](/functions/getexitbylink)
* [GetExitByName](/functions/getexitbyname)
* [LockExit](/functions/corelibrary/lockexit)
* [MakeExitInvisible](/functions/corelibrary/makeexitinvisible)
* [MakeExitVisible](/functions/corelibrary/makeexitvisible)
* [UnlockExit](/functions/corelibrary/unlockexit)



<a name="tandt"></a>Timers and Turnscripts
---------------------------------------

* [DisableTimer](/functions/corelibrary/disabletimer)
* [DisableTurnScript](/functions/corelibrary/disableturnscript)
* [GetTimer](/functions/gettimer)
* [SetTimeout](/functions/corelibrary/settimeout)
* [SetTimeoutID](/functions/corelibrary/settimeoutid)
* [SetTimerInterval](/functions/corelibrary/settimerinterval)
* [SetTimerScript](/functions/corelibrary/settimerscript)
* [SetTurnScript](/functions/corelibrary/setturnscript)
* [SetTurnTimeout](/functions/corelibrary/setturntimeout)
* [SetTurnTimeoutID](/functions/corelibrary/setturntimeoutid)
* [SuppressTurnscripts](/functions/suppressturnscripts)



<a name="ui"></a>User Interface Functions 
-----------------------------------------

Functions that change what is displayed or how it is displayed or require the player to do something, rather than affecting the game world.

* [Ask](/functions/ask)
* [ClearFramePicture](/functions/corelibrary/clearframepicture)
* [ClearScreen](/functions/corelibrary/clearscreen)
* [DisplayList](/functions/corelibrary/displaylist)
* [DisplayMailtoLink](/functions/corelibrary/displaymailtolink)
* [GetCurrentFontFamily](/functions/corelibrary/getcurrentfontfamily)
* [GetInput](/functions/getinput)
* [InitUserInterface](/functions/corelibrary/inituserinterface)
* [OutputText](/functions/outputtext)
* [OutputTextNoBr](/functions/outputtextnobr)
* [OutputTextRaw](/functions/outputtextraw)
* [OutputTextRawNoBr](/functions/outputtextrawnobr)
* [PrintCentered](/functions/corelibrary/printcentered)
* [SetAlignment](/functions/corelibrary/setalignment)
* [SetBackgroundColour](/functions/corelibrary/setbackgroundcolour)
* [SetBackgroundImage](/functions/corelibrary/setbackgroundimage)
* [SetBackgroundOpacity](/functions/corelibrary/setbackgroundopacity)
* [SetFontName](/functions/corelibrary/setfontname)
* [SetFontSize](/functions/corelibrary/setfontsize)
* [SetForegroundColour](/functions/corelibrary/setforegroundcolour)
* [SetFramePicture](/functions/corelibrary/setframepicture)
* [SetWebFontName](/functions/corelibrary/setwebfontname)
* [ShowMenu](/functions/showmenu)
* [ShowVimeo](/functions/corelibrary/showvimeo)
* [ShowYouTube](/functions/corelibrary/showyoutube)
* [TextFX_Typewriter](/functions/corelibrary/textfx_typewriter)
* [TextFX_Unscramble](/functions/corelibrary/textfx_unscramble)
* [UpdateStatusAttributes](/functions/corelibrary/updatestatusattributes)



<a name="list"></a>List Functions
---------------------------------

Functions for manipulating lists. For a discussion on how to use lists, see [here](/using_lists).

* [Contains](/functions/contains)
* [FilterByAttribute](/functions/filterbyattribute)
* [FilterByNotAttribute](/functions/filterbynotattribute)
* [FilterByType](/functions/filterbytype)
* [IndexOf](/functions/indexof)
* [ListCombine](/functions/listcombine)
* [ListContains](/functions/listcontains)
* [ListCompact](/functions/listcompact)
* [ListCount](/functions/listcount)
* [ListExclude](/functions/listexclude)
* [ListItem](/functions/listitem)
* [NewList](/functions/newlist)
* [NewObjectList](/functions/newobjectlist)
* [NewStringList](/functions/newstringlist)
* [ObjectListCompact](/functions/objectlistcompact)
* [ObjectListItem](/functions/objectlistitem)
* [ObjectListSort](/functions/objectlistsort)
* [ObjectListSortDescending](/functions/objectlistsortdescending)
* [RemoveSceneryObjects](/functions/corelibrary/removesceneryobjects)
* [StringListItem](/functions/stringlistitem)
* [StringListSort](/functions/stringlistsort)
* [StringListSortDescending](/functions/stringlistsortdescending)



<a name="scope"></a>Scope Functions
-----------------------------------

Functions that will return a list of objects (in the loosest sense). See more [here](/scopes).

* [AllCommands](/functions/allcommands)
* [AllExits](/functions/allexits)
* [AllObjects](/functions/allobjects)
* [AllRooms](/functions/allrooms)
* [AllTurnScripts](/functions/allturnscripts)
* [GetAllChildObjects](/functions/getallchildobjects)
* [GetDirectChildren](/functions/getdirectchildren)
* [ScopeAllExitsForRoom](/functions/corelibrary/scopeallexitsforroom)
* [ScopeCommands](/functions/corelibrary/scopecommands)
* [ScopeExits](/functions/corelibrary/scopeexits)
* [ScopeExitsAll](/functions/corelibrary/scopeexitsall)
* [ScopeExitsForRoom](/functions/corelibrary/scopeexitsforroom)
* [ScopeUnlockedExitsForRoom](/functions/corelibrary/scopeunlockedexitsforroom)
* [ScopeInventory](/functions/corelibrary/scopeinventory)
* [ScopeReachable](/functions/corelibrary/scopereachable)
* [ScopeReachableForRoom](/functions/corelibrary/scopereachableforroom)
* [ScopeReachableInventory](/functions/corelibrary/scopereachableinventory)
* [ScopeReachableNotHeld](/functions/corelibrary/scopereachablenotheld)
* [ScopeReachableNotHeldForRoom](/functions/corelibrary/scopereachablenotheldforroom)
* [ScopeVisible](/functions/corelibrary/scopevisible)
* [ScopeVisibleForRoom](/functions/corelibrary/scopevisibleforroom)
* [ScopeVisibleNotHeld](/functions/corelibrary/scopevisiblenotheld)
* [ScopeVisibleNotHeldForRoom](/functions/corelibrary/scopevisiblenotheldforroom)
* [ScopeVisibleNotHeldNotScenery](/functions/corelibrary/scopevisiblenotheldnotscenery)
* [ScopeVisibleNotHeldNotSceneryForRoom](/functions/corelibrary/scopevisiblenotheldnotsceneryforroom)
* [ScopeVisibleNotReachable](/functions/corelibrary/scopevisiblenotreachable)
* [ScopeVisibleNotReachableForRoom](/functions/corelibrary/scopevisiblenotreachableforroom)



<a name="dictionary"></a>Dictionary Functions
---------------------------------

Functions for manipulating dictionaries. For a discussion on how to use dictionaries, see [here](/using_dictionaries).

* [DictionaryAdd](/functions/dictionaryadd)
* [DictionaryContains](/functions/dictionarycontains)
* [DictionaryCount](/functions/dictionarycount)
* [DictionaryItem](/functions/dictionaryitem)
* [DictionaryRemove](/functions/dictionaryremove)
* [NewDictionary](/functions/newdictionary)
* [NewObjectDictionary](/functions/newobjectdictionary)
* [NewScriptDictionary](/functions/newscriptdictionary)
* [NewStringDictionary](/functions/newstringdictionary)
* [ObjectDictionaryItem](/functions/objectdictionaryitem)
* [QuickParams](/functions/quickparams)
* [ScriptDictionaryItem](/functions/scriptdictionaryitem)
* [StringDictionaryItem](/functions/stringdictionaryitem)



<a name="string"></a>String Functions
--------------------------------------

* [Asc](/functions/string/asc)
* [CapFirst](/functions/string/capfirst)
* [Chr](/functions/string/chr)
* [Conjugate](/functions/corelibrary/conjugate)
* [Decimalise](/functions/string/decimalise)
* [DisplayMoney](/functions/string/displaymoney)
* [DisplayNumber](/functions/string/displaynumber)
* [DynamicTemplate](/functions/dynamictemplate)
* [EndsWith](/functions/string/endswith)
* [FormatList](/functions/string/formatlist)
* [GetMatchStrength](/functions/getmatchstrength)
* [Instr](/functions/string/instr)
* [InstrRev](/functions/string/instrrev)
* [IsNumeric](/functions/string/isnumeric)
* [IsRegexMatch](/functions/isregexmatch)
* [Join](/functions/string/join)
* [LCase](/functions/string/lcase)
* [Left](/functions/string/left)
* [LengthOf](/functions/string/lengthof)
* [LTrim](/functions/string/ltrim)
* [Mid](/functions/string/mid)
* [PadString](/functions/string/padstring)
* [ProcessText](/functions/processtext)
* [Replace](/functions/string/replace)
* [ReverseDirection](/functions/string/reversedirection)
* [Right](/functions/string/right)
* [RTrim](/functions/string/rtrim)
* [SafeXML](/functions/safexml)
* [Spaces](/functions/string/spaces)
* [Split](/functions/string/split)
* [StartsWith](/functions/string/startswith)
* [Template](/functions/template)
* [ToRoman](/functions/string/toroman)
* [ToWords](/functions/string/towords)
* [Trim](/functions/string/trim)
* [UCase](/functions/string/ucase)
* [WriteVerb](/functions/corelibrary/writeverb)



<a name="clothing"></a>Clothing Functions
-----------------------------------------

* [WearGarment](/functions/weargarment)
* [WearGarment](/functions/weargarment)
* [GetOuter](/functions/getouter)
* [GetOuterFor](/functions/getouterfor)
* [RemoveGarment](/functions/removegarment)
* [WearGarment](/functions/weargarment)



<a name="random"></a>Randomising Functions
-------------------------------------

These functions all return a random value. See also [here](/random).

* [DiceRoll](/functions/corelibrary/diceroll)
* [GetRandomDouble](/functions/getrandomdouble)
* [GetRandomInt](/functions/getrandomint)
* [PickOneChild](/functions/pickonechild)
* [PickOneChildOfType](/functions/pickonechildoftype)
* [PickOneExit](/functions/pickoneexit)
* [PickOneObject](/functions/pickoneobject)
* [PickOneString](/functions/pickonestring)
* [PickOneUnlockedExit](/functions/pickoneunlockedexit)
* [RandomChance](/functions/corelibrary/randomchance)



<a name="general"></a>General Functions
---------------------------------------

* [Eval](/functions/eval)
* [GetFileData](/functions/getfiledata)
* [GetFileURL](/functions/getfileurl)
* [Log](/functions/corelibrary/log)
* [RunDelegateFunction](/functions/rundelegatefunction)



<a name="core"></a>Core.aslx Functions
--------------------------------------

Functions with very specific effects in the game world.

* [AddToInventory](/functions/corelibrary/addtoinventory)
* [CanReachThrough](/functions/corelibrary/canreachthrough)
* [CanSeeThrough](/functions/corelibrary/canseethrough)
* [ChangePOV](/functions/corelibrary/changepov)
* [CheckDarkness](/functions/corelibrary/checkdarkness)
* [FormatExitList](/functions/corelibrary/formatexitlist)
* [FormatObjectList](/functions/corelibrary/formatobjectlist)
* [GetBlockingObject](/functions/corelibrary/getblockingobject)
* [GetDefiniteName](/functions/getdefinitename)
* [GetDisplayAlias](/functions/corelibrary/getdisplayalias)
* [GetDisplayName](/functions/corelibrary/getdisplayname)
* [GetDisplayNameLink](/functions/corelibrary/getdisplaynamelink)
* [GetDisplayVerbs](/functions/corelibrary/getdisplayverbs)
* [GetListDisplayAlias](/functions/corelibrary/getlistdisplayalias)
* [GetNonTransparentParent](/functions/corelibrary/getnontransparentparent)
* [GetVolume](/functions/corelibrary/getvolume)
* [Got](/functions/corelibrary/got)
* [HelperCloseObject](/functions/corelibrary/helpercloseobject)
* [HelperOpenObject](/functions/corelibrary/helperopenobject)
* [IsSwitchedOn](/functions/corelibrary/isswitchedon)
* [ListParents](/functions/corelibrary/listparents)
* [SetDark](/functions/corelibrary/setdark)
* [SetExitLightstrength](/functions/corelibrary/setexitlightstrength)
* [SetLight](/functions/corelibrary/setlight)
* [SetObjectLightstrength](/functions/corelibrary/setobjectlightstrength)
* [ShowRoomDescription](/functions/corelibrary/showroomdescription)
* [SwitchOff](/functions/corelibrary/switchoff)
* [SwitchOn](/functions/corelibrary/switchon)



<a name="internal"></a>Internal Core.aslx Functions
---------------------------------------------------

Most games shouldn't need to call these directly.

* [AddExternalStylesheet](/functions/corelibrary/addexternalstylesheet)
* [AddStatusAttributesForElement](/functions/corelibrary/addstatusattributesforelement)
* [CloseObject](/functions/corelibrary/closeobject)
* [CommandLink](/functions/corelibrary/commandlink)
* [CompareNames](/functions/corelibrary/comparenames)
* [ContainsAccessible](/functions/corelibrary/containsaccessible)
* [ContainsReachable](/functions/corelibrary/containsreachable)
* [ContainsVisible](/functions/corelibrary/containsvisible)
* [DisplayHttpLink](/functions/corelibrary/displayhttplink)
* [DoAskTell](/functions/corelibrary/doasktell)
* [DoDrop](/functions/corelibrary/dodrop)
* [DoTake](/functions/corelibrary/dotake)
* [FormatStatusAttribute](/functions/corelibrary/formatstatusattribute)
* [GenerateMenuChoices](/functions/corelibrary/generatemenuchoices)
* [GetDefaultPrefix](/functions/corelibrary/getdefaultprefix)
* [GetKeywordsMatchStrength](/functions/corelibrary/getkeywordsmatchstrength)
* [GetPlacesObjectsList](/functions/corelibrary/getplacesobjectslist)
* [GetTaggedName](/functions/corelibrary/gettaggedname)
* [GetUniqueElementName](/functions/getuniqueelementname)
* [Grid_AddNewShapePoint](/functions/corelibrary/grid_addnewshapepoint)
* [Grid_ClearCustomLayer](/functions/corelibrary/grid_clearcustomlayer)
* [Grid_DrawArrow](/functions/corelibrary/grid_drawarrow)
* [Grid_DrawGridLines](/functions/corelibrary/grid_drawgridlines)
* [Grid_DrawImage](/functions/corelibrary/grid_drawimage)
* [Grid_DrawLine](/functions/corelibrary/grid_drawline)
* [Grid_DrawShape](/functions/corelibrary/grid_drawshape)
* [Grid_DrawSquare](/functions/corelibrary/grid_drawsquare)
* [Grid_DrawSvg](/functions/corelibrary/grid_drawsvg)
* [Grid_LoadSvg](/functions/corelibrary/grid_loadsvg)
* [Grid_SetCentre](/functions/corelibrary/grid_setcentre)
* [Grid_ShowCustomLayer](/functions/corelibrary/grid_showcustomlayer)
* [Grid_CalculateMapCoordinates](/functions/corelibrary/grid_calculatemapcoordinates)
* [Grid_DrawPlayerInRoom](/functions/corelibrary/grid_drawplayerinroom)
* [Grid_DrawRoom](/functions/corelibrary/grid_drawroom)
* [Grid_Redraw](/functions/corelibrary/grid_redraw)
* [Grid_SetScale](/functions/corelibrary/grid_setscale)
* [HandleCommand](/functions/corelibrary/handlecommand)
* [HandleSingleCommand](/functions/corelibrary/handlesinglecommand)
* [HandleSingleCommandPattern](/functions/corelibrary/handlesinglecommandpattern)
* [InitInterface](/functions/corelibrary/initinterface)
* [InitPOV](/functions/corelibrary/initpov)
* [InitVerbsList](/functions/corelibrary/initverbslist)
* [IsGameRunning](/functions/isgamerunning)
* [ListObjectContents](/functions/corelibrary/listobjectcontents)
* [OnEnterRoom](/functions/corelibrary/onenterroom)
* [ObjectLink](/functions/corelibrary/objectlink)
* [OpenObject](/functions/corelibrary/openobject)
* [Populate](/functions/populate)
* [ResolveName](/functions/corelibrary/resolvename)
* [ResolveNameInternal](/functions/corelibrary/resolvenameinternal)
* [ResolveNameList](/functions/corelibrary/resolvenamelist)
* [ResolveNameListItem](/functions/corelibrary/resolvenamelistitem)
* [RunTurnScripts](/functions/corelibrary/runturnscripts)
* [StartGame](/functions/corelibrary/startgame)
* [TryOpenClose](/functions/corelibrary/tryopenclose)



<a name="maths"></a>Mathematical Functions
---------------------------------------------------

These will not be relevant to many games at all, but are available as a consequence of the .NET framework Quest is built on. They are included here for completeness; if you need them, you will know what they do. There is no further documentation.

Quest has `e` and `pi` as built-in constants.

These all take a single floating point number, and return the corresponding floating point number. Note that the trigonometric functions use radians rather than degrees.

* Abs
* Acos
* Asin
* Atan
* Cos
* Exp
* Log
* Log10
* Sin
* Sinh
* Sqrt
* Tan
* Tanh

The following functions all take a floating point parameter and return an integer.

* Ceiling
* Floor
* Round
* Truncate
* Sign

These two functions take two parameters, and can be used with either floating point or integers, and return the same type.

* Max
* Min

