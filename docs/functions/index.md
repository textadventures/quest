---
layout: index
title: Functions
---

This is a list of most of the functions built in to Quest, grouped by type. For a list in alphabetical order, go [here](index_allfunctions.html).



- [Functions for Attributes](#attributes)

- [Functions for Variables](#variables)

- [Functions for Objects and Exits](#objects)

- [Timers and Turnscripts](#tanat)

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

* [DecreaseHealth](corelibrary/decreasehealth.html)
* [DecreaseMoney](corelibrary/decreasemoney.html)
* [DecreaseScore](corelibrary/decreasescore.html)
* [GetAttribute](getattribute.html)
* [GetAttributeNames](getattributenames.html)
* [GetBoolean](getboolean.html)
* [GetDouble](getdouble.html)
* [GetInt](getint.html)
* [GetString](getstring.html)
* [HasAttribute](hasattribute.html)
* [HasBoolean](hasboolean.html)
* [HasDelegateImplementation](hasdelegateimplementation.html)
* [HasDouble](hasdouble.html)
* [HasInt](hasint.html)
* [HasObject](hasobject.html)
* [HasScript](hasscript.html)
* [HasString](hasstring.html)
* [IncreaseHealth](corelibrary/increasehealth.html)
* [IncreaseMoney](corelibrary/increasemoney.html)
* [IncreaseScore](corelibrary/increasescore.html)
* [SetObjectFlagOff](corelibrary/setobjectflagoff.html)
* [SetObjectFlagOn](corelibrary/setobjectflagon.html)



<a name="variables"></a>Functions for Variables
---------------------------------------

Functions that check or change the values of variables (and can be used on attributes too).

* [Equal](equal.html)
* [IsDefined](isdefined.html)
* [IsDouble](isdouble.html)
* [IsInt](isint.html)
* [ToDouble](todouble.html)
* [ToInt](toint.html)
* [ToString](tostring.html)
* [TypeOf](typeof.html)



<a name="objects"></a>Functions for Objects and Exits
---------------------------------------

* [Clone](clone.html)
* [CloneObject](corelibrary/cloneobject.html)
* [CloneObjectAndMove](corelibrary/cloneobjectandmove.html)
* [CloneObjectAndMoveHere](corelibrary/cloneobjectandmovehere.html)
* [DoesInherit](doesinherit.html)
* [GetObject](getobject.html)
* [MakeObjectInvisible](corelibrary/makeobjectinvisible.html)
* [MakeObjectVisible](corelibrary/makeobjectvisible.html)
* [MoveObject](corelibrary/moveobject.html)
* [RemoveObject](corelibrary/removeobject.html)

* [CreateBiExits](createbiexits.html)
* [GetExitByLink](getexitbylink.html)
* [GetExitByName](getexitbyname.html)
* [LockExit](corelibrary/lockexit.html)
* [MakeExitInvisible](corelibrary/makeexitinvisible.html)
* [MakeExitVisible](corelibrary/makeexitvisible.html)
* [UnlockExit](corelibrary/unlockexit.html)



<a name="tandt"></a>Timers and Turnscripts
---------------------------------------

* [DisableTimer](corelibrary/disabletimer.html)
* [DisableTurnScript](corelibrary/disableturnscript.html)
* [GetTimer](gettimer.html)
* [SetTimeout](corelibrary/settimeout.html)
* [SetTimeoutID](corelibrary/settimeoutid.html)
* [SetTimerInterval](corelibrary/settimerinterval.html)
* [SetTimerScript](corelibrary/settimerscript.html)
* [SetTurnScript](corelibrary/setturnscript.html)
* [SetTurnTimeout](corelibrary/setturntimeout.html)
* [SetTurnTimeoutID](corelibrary/setturntimeoutid.html)
* [SuppressTurnscripts](suppressturnscripts.html)



<a name="ui"></a>User Interface Functions 
-----------------------------------------

Functions that change what is displayed or how it is displayed or require the player to do something, rather than affecting the game world.

* [Ask](ask.html)
* [ClearFramePicture](corelibrary/clearframepicture.html)
* [ClearScreen](corelibrary/clearscreen.html)
* [DisplayList](corelibrary/displaylist.html)
* [DisplayMailtoLink](corelibrary/displaymailtolink.html)
* [GetCurrentFontFamily](corelibrary/getcurrentfontfamily.html)
* [GetInput](getinput.html)
* [InitUserInterface](corelibrary/inituserinterface.html)
* [OutputText](outputtext.html)
* [OutputTextNoBr](outputtextnobr.html)
* [OutputTextRaw](outputtextraw.html)
* [OutputTextRawNoBr](outputtextrawnobr.html)
* [PrintCentered](corelibrary/printcentered.html)
* [SetAlignment](corelibrary/setalignment.html)
* [SetBackgroundColour](corelibrary/setbackgroundcolour.html)
* [SetBackgroundImage](corelibrary/setbackgroundimage.html)
* [SetBackgroundOpacity](corelibrary/setbackgroundopacity.html)
* [SetFontName](corelibrary/setfontname.html)
* [SetFontSize](corelibrary/setfontsize.html)
* [SetForegroundColour](corelibrary/setforegroundcolour.html)
* [SetFramePicture](corelibrary/setframepicture.html)
* [SetWebFontName](corelibrary/setwebfontname.html)
* [ShowMenu](showmenu.html)
* [ShowVimeo](corelibrary/showvimeo.html)
* [ShowYouTube](corelibrary/showyoutube.html)
* [TextFX_Typewriter](corelibrary/textfx_typewriter.html)
* [TextFX_Unscramble](corelibrary/textfx_unscramble.html)
* [UpdateStatusAttributes](corelibrary/updatestatusattributes.html)



<a name="list"></a>List Functions
---------------------------------

Functions for manipulating lists. For a discussion on how to use lists, see [here](../using_lists.html).

* [Contains](contains.html)
* [FilterByAttribute](filterbyattribute.html)
* [FilterByNotAttribute](filterbynotattribute.html)
* [FilterByType](filterbytype.html)
* [IndexOf](indexof.html)
* [ListCombine](listcombine.html)
* [ListContains](listcontains.html)
* [ListCompact](listcompact.html)
* [ListCount](listcount.html)
* [ListExclude](listexclude.html)
* [ListItem](listitem.html)
* [NewList](newlist.html)
* [NewObjectList](newobjectlist.html)
* [NewStringList](newstringlist.html)
* [ObjectListCompact](objectlistcompact.html)
* [ObjectListItem](objectlistitem.html)
* [ObjectListSort](objectlistsort.html)
* [ObjectListSortDescending](objectlistsortdescending.html)
* [RemoveSceneryObjects](corelibrary/removesceneryobjects.html)
* [StringListItem](stringlistitem.html)
* [StringListSort](stringlistsort.html)
* [StringListSortDescending](stringlistsortdescending.html)



<a name="scope"></a>Scope Functions
-----------------------------------

Functions that will return a list of objects (in the loosest sense). See more [here](../scopes.html).

* [AllCommands](allcommands.html)
* [AllExits](allexits.html)
* [AllObjects](allobjects.html)
* [AllRooms](allrooms.html)
* [AllTurnScripts](allturnscripts.html)
* [GetAllChildObjects](getallchildobjects.html)
* [GetDirectChildren](getdirectchildren.html)
* [ScopeAllExitsForRoom](corelibrary/scopeallexitsforroom.html)
* [ScopeCommands](corelibrary/scopecommands.html)
* [ScopeExits](corelibrary/scopeexits.html)
* [ScopeExitsAll](corelibrary/scopeexitsall.html)
* [ScopeExitsForRoom](corelibrary/scopeexitsforroom.html)
* [ScopeUnlockedExitsForRoom](corelibrary/scopeunlockedexitsforroom.html)
* [ScopeInventory](corelibrary/scopeinventory.html)
* [ScopeReachable](corelibrary/scopereachable.html)
* [ScopeReachableForRoom](corelibrary/scopereachableforroom.html)
* [ScopeReachableInventory](corelibrary/scopereachableinventory.html)
* [ScopeReachableNotHeld](corelibrary/scopereachablenotheld.html)
* [ScopeReachableNotHeldForRoom](corelibrary/scopereachablenotheldforroom.html)
* [ScopeVisible](corelibrary/scopevisible.html)
* [ScopeVisibleForRoom](corelibrary/scopevisibleforroom.html)
* [ScopeVisibleNotHeld](corelibrary/scopevisiblenotheld.html)
* [ScopeVisibleNotHeldForRoom](corelibrary/scopevisiblenotheldforroom.html)
* [ScopeVisibleNotHeldNotScenery](corelibrary/scopevisiblenotheldnotscenery.html)
* [ScopeVisibleNotHeldNotSceneryForRoom](corelibrary/scopevisiblenotheldnotsceneryforroom.html)
* [ScopeVisibleNotReachable](corelibrary/scopevisiblenotreachable.html)
* [ScopeVisibleNotReachableForRoom](corelibrary/scopevisiblenotreachableforroom.html)



<a name="dictionary"></a>Dictionary Functions
---------------------------------

Functions for manipulating dictionaries. For a discussion on how to use dictionaries, see [here](../using_dictionaries.html).

* [DictionaryAdd](dictionaryadd.html)
* [DictionaryContains](dictionarycontains.html)
* [DictionaryCount](dictionarycount.html)
* [DictionaryItem](dictionaryitem.html)
* [DictionaryRemove](dictionaryremove.html)
* [NewDictionary](newdictionary.html)
* [NewObjectDictionary](newobjectdictionary.html)
* [NewScriptDictionary](newscriptdictionary.html)
* [NewStringDictionary](newstringdictionary.html)
* [ObjectDictionaryItem](objectdictionaryitem.html)
* [QuickParams](quickparams.html)
* [ScriptDictionaryItem](scriptdictionaryitem.html)
* [StringDictionaryItem](stringdictionaryitem.html)



<a name="string"></a>String Functions
--------------------------------------

* [Asc](string/asc.html)
* [CapFirst](string/capfirst.html)
* [Chr](string/chr.html)
* [Conjugate](corelibrary/conjugate.html)
* [Decimalise](string/decimalise.html)
* [DisplayMoney](string/displaymoney.html)
* [DisplayNumber](string/displaynumber.html)
* [DynamicTemplate](dynamictemplate.html)
* [EndsWith](string/endswith.html)
* [FormatList](string/formatlist.html)
* [GetMatchStrength](getmatchstrength.html)
* [Instr](string/instr.html)
* [InstrRev](string/instrrev.html)
* [IsNumeric](string/isnumeric.html)
* [IsRegexMatch](isregexmatch.html)
* [Join](string/join.html)
* [LCase](string/lcase.html)
* [Left](string/left.html)
* [LengthOf](string/lengthof.html)
* [LTrim](string/ltrim.html)
* [Mid](string/mid.html)
* [PadString](string/padstring.html)
* [ProcessText](processtext.html)
* [Replace](string/replace.html)
* [ReverseDirection](string/reversedirection.html)
* [Right](string/right.html)
* [RTrim](string/rtrim.html)
* [SafeXML](safexml.html)
* [Spaces](string/spaces.html)
* [Split](string/split.html)
* [StartsWith](string/startswith.html)
* [Template](template.html)
* [ToRoman](string/toroman.html)
* [ToWords](string/towords.html)
* [Trim](string/trim.html)
* [UCase](string/ucase.html)
* [WriteVerb](corelibrary/writeverb.html)



<a name="clothing"></a>Clothing Functions
-----------------------------------------

* [WearGarment](weargarment.html)
* [WearGarment](weargarment.html)
* [GetOuter](getouter.html)
* [GetOuterFor](getouterfor.html)
* [RemoveGarment](removegarment.html)
* [WearGarment](weargarment.html)



<a name="random"></a>Randomising Functions
-------------------------------------

These functions all return a random value.

* [DiceRoll](corelibrary/diceroll.html)
* [GetRandomDouble](getrandomdouble.html)
* [GetRandomInt](getrandomint.html)
* [PickOneChild](pickonechild.html)
* [PickOneChildOfType](pickonechildoftype.html)
* [PickOneExit](pickoneexit.html)
* [PickOneObject](pickoneobject.html)
* [PickOneString](pickonestring.html)
* [PickOneUnlockedExit](pickoneunlockedexit.html)
* [RandomChance](corelibrary/randomchance.html)



<a name="general"></a>General Functions
---------------------------------------

* [Eval](eval.html)
* [GetFileData](getfiledata.html)
* [GetFileURL](getfileurl.html)
* [Log](corelibrary/log.html)
* [RunDelegateFunction](rundelegatefunction.html)



<a name="core"></a>Core.aslx Functions
--------------------------------------

Functions with very specific effects in the game world.

* [AddToInventory](corelibrary/addtoinventory.html)
* [CanReachThrough](corelibrary/canreachthrough.html)
* [CanSeeThrough](corelibrary/canseethrough.html)
* [ChangePOV](corelibrary/changepov.html)
* [CheckDarkness](corelibrary/checkdarkness.html)
* [FormatExitList](corelibrary/formatexitlist.html)
* [FormatObjectList](corelibrary/formatobjectlist.html)
* [GetBlockingObject](corelibrary/getblockingobject.html)
* [GetDefiniteName](getdefinitename.html)
* [GetDisplayAlias](corelibrary/getdisplayalias.html)
* [GetDisplayName](corelibrary/getdisplayname.html)
* [GetDisplayNameLink](corelibrary/getdisplaynamelink.html)
* [GetDisplayVerbs](corelibrary/getdisplayverbs.html)
* [GetListDisplayAlias](corelibrary/getlistdisplayalias.html)
* [GetNonTransparentParent](corelibrary/getnontransparentparent.html)
* [GetVolume](corelibrary/getvolume.html)
* [Got](corelibrary/got.html)
* [HelperCloseObject](corelibrary/helpercloseobject.html)
* [HelperOpenObject](corelibrary/helperopenobject.html)
* [IsSwitchedOn](corelibrary/isswitchedon.html)
* [ListParents](corelibrary/listparents.html)
* [SetDark](corelibrary/setdark.html)
* [SetExitLightstrength](corelibrary/setexitlightstrength.html)
* [SetLight](corelibrary/setlight.html)
* [SetObjectLightstrength](corelibrary/setobjectlightstrength.html)
* [ShowRoomDescription](corelibrary/showroomdescription.html)
* [SwitchOff](corelibrary/switchoff.html)
* [SwitchOn](corelibrary/switchon.html)



<a name="internal"></a>Internal Core.aslx Functions
---------------------------------------------------

Most games shouldn't need to call these directly.

* [AddExternalStylesheet](corelibrary/addexternalstylesheet.html)
* [AddStatusAttributesForElement](corelibrary/addstatusattributesforelement.html)
* [CloseObject](corelibrary/closeobject.html)
* [CommandLink](corelibrary/commandlink.html)
* [CompareNames](corelibrary/comparenames.html)
* [ContainsAccessible](corelibrary/containsaccessible.html)
* [ContainsReachable](corelibrary/containsreachable.html)
* [ContainsVisible](corelibrary/containsvisible.html)
* [DisplayHttpLink](corelibrary/displayhttplink.html)
* [DoAskTell](corelibrary/doasktell.html)
* [DoDrop](corelibrary/dodrop.html)
* [DoTake](corelibrary/dotake.html)
* [FormatStatusAttribute](corelibrary/formatstatusattribute.html)
* [GenerateMenuChoices](corelibrary/generatemenuchoices.html)
* [GetDefaultPrefix](corelibrary/getdefaultprefix.html)
* [GetKeywordsMatchStrength](corelibrary/getkeywordsmatchstrength.html)
* [GetPlacesObjectsList](corelibrary/getplacesobjectslist.html)
* [GetTaggedName](corelibrary/gettaggedname.html)
* [GetUniqueElementName](getuniqueelementname.html)
* [Grid_AddNewShapePoint](corelibrary/grid_addnewshapepoint.html)
* [Grid_ClearCustomLayer](corelibrary/grid_clearcustomlayer.html)
* [Grid_DrawArrow](corelibrary/grid_drawarrow.html)
* [Grid_DrawGridLines](corelibrary/grid_drawgridlines.html)
* [Grid_DrawImage](corelibrary/grid_drawimage.html)
* [Grid_DrawLine](corelibrary/grid_drawline.html)
* [Grid_DrawShape](corelibrary/grid_drawshape.html)
* [Grid_DrawSquare](corelibrary/grid_drawsquare.html)
* [Grid_DrawSvg](corelibrary/grid_drawsvg.html)
* [Grid_LoadSvg](corelibrary/grid_loadsvg.html)
* [Grid_SetCentre](corelibrary/grid_setcentre.html)
* [Grid_ShowCustomLayer](corelibrary/grid_showcustomlayer.html)
* [Grid_CalculateMapCoordinates](corelibrary/grid_calculatemapcoordinates.html)
* [Grid_DrawPlayerInRoom](corelibrary/grid_drawplayerinroom.html)
* [Grid_DrawRoom](corelibrary/grid_drawroom.html)
* [Grid_Redraw](corelibrary/grid_redraw.html)
* [Grid_SetScale](corelibrary/grid_setscale.html)
* [HandleCommand](corelibrary/handlecommand.html)
* [HandleSingleCommand](corelibrary/handlesinglecommand.html)
* [HandleSingleCommandPattern](corelibrary/handlesinglecommandpattern.html)
* [InitInterface](corelibrary/initinterface.html)
* [InitPOV](corelibrary/initpov.html)
* [InitVerbsList](corelibrary/initverbslist.html)
* [IsGameRunning](isgamerunning.html)
* [ListObjectContents](corelibrary/listobjectcontents.html)
* [OnEnterRoom](corelibrary/onenterroom.html)
* [ObjectLink](corelibrary/objectlink.html)
* [OpenObject](corelibrary/openobject.html)
* [Populate](populate.html)
* [ResolveName](corelibrary/resolvename.html)
* [ResolveNameInternal](corelibrary/resolvenameinternal.html)
* [ResolveNameList](corelibrary/resolvenamelist.html)
* [ResolveNameListItem](corelibrary/resolvenamelistitem.html)
* [RunTurnScripts](corelibrary/runturnscripts.html)
* [StartGame](corelibrary/startgame.html)
* [TryOpenClose](corelibrary/tryopenclose.html)



<a name="maths"></a>Mathematical Functions
---------------------------------------------------

These will not be relevant to many games at all, but are available as a consequence of the .NET framework Quest is built on. They are included here for completeness; if you need them, you will know what they do. There is no further documentation.

Quest has `e` and `pi` as built-in constants.

These all take a single foating point number, and return the corresponding floating point number. Note that the trigonometric functions use radians rather than degrees.

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

