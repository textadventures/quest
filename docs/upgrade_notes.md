---
layout: index
title: Upgrade Notes
---

Upgrading from Quest 5.4.1 to Quest 5.5
---------------------------------------

To prevent so many tabs from showing when editing a game, some of Quest's features can now be toggled on and off - on a per-game basis, and on a per-object basis. When upgrading, some features in the Editor may be hidden until you turn the feature on again.

Per-game features - set these by selecting "game" and then go to the Features tab. Turning these options on will cause the relevant tabs and script commands to be shown.

-   hyperlinks
-   map and custom drawing grid
-   static picture frame
-   score
-   health
-   inventory limits
-   light and dark
-   ask and tell
-   in-room descriptions

Per-object features - set these by selecting the object and then go to the Features tab. Turning these options on will cause the tab to be displayed for the object.

-   use and give
-   container
-   switch on/off
-   edible
-   player

Other changes:

-   the "Pause" request is no longer supported. Use the [SetTimeout](functions/corelibrary/settimeout.html) function instead.
-   the game title and author name are now displayed by default when a game begins. You can turn this option off from the game Setup tab.
-   in gamebook mode, the new default style is to not clear the screen between different pages. You can revert to the old behaviour by clicking game, then go to the Display tab and tick "Clear screen between each page".

For more details on new features, see the [Quest 5.5 beta blog post](http://blog.textadventures.co.uk/2013/12/19/quest-5-5-beta-is-now-available/).

Upgrading from Quest 5.4 to Quest 5.4.1
---------------------------------------

If you have any exits that run scripts instead of automatically moving the player, you will need to tick a new checkbox on the exit editor "Run a script (instead of moving the player automatically)".

Upgrading from Quest 5.3 to Quest 5.4
-------------------------------------

-   the old synchronous functions Ask, ShowMenu, GetInput and synchronous Wait request are now unsupported, and will raise an error if used in a game with ASL version set to 540 or above.
-   ASLX attribute type name changes:
    -   "list" becomes "simplestringlist"
    -   "stringdictionary" becomes "simplestringdictionary"
    -   "objectdictionary" becomes "simpleobjectdictionary"
    -   new "stringlist" format
    -   new "stringdictionary" format
    -   new "objectdictionary" format
    -   new "list" format
    -   new "dictionary" format
-   new functions [NewList](functions/newlist.html) and [NewScriptDictionary](functions/newscriptdictionary.html)
-   new [text processor](text_processor.html)
-   [GetTaggedName](functions/corelibrary/gettaggedname.html) is removed from Core.aslx
-   [GetDisplayNameLink](functions/corelibrary/getdisplaynamelink.html) no longer takes a verbs parameter
-   new function [SetAlignment](functions/corelibrary/setalignment.html)
-   the [insert](scripts/insert.html) script command is obsolete, as you can directly write HTML with [msg](scripts/msg.html)
-   the old [text formatting](tutorial/text_formatting.html) XML is no longer relevant - use HTML instead. The only breaking change is there is no "color" tag in HTML.
-   new "JS." calling method for JavaScript functions

Upgrading from Quest 5.2 to Quest 5.3
-------------------------------------

-   Display verbs are now automatically generated by default. If you want the old behaviour (manually specifying display verbs for each object), go to the game Room Descriptions tab and turn off "Automatically generate object display verbs list".
-   See also the [Quest 5.3 Beta release announcement](https://blog.textadventures.co.uk/2012/12/03/quest-5-3-beta-is-now-available/)

Upgrading from Quest 5.1 to Quest 5.2
-------------------------------------

See the [Quest 5.2 Beta release announcement](https://blog.textadventures.co.uk/2012/04/14/quest-5-2-beta-is-now-available/) and the [Quest 5.2 release announcement](https://blog.textadventures.co.uk/2012/05/12/quest-5-2-is-out-now/)

Upgrading from Quest 5.0 to Quest 5.1
-------------------------------------

You can open and edit your Quest 5.0 games in Quest 5.1 (but note that, once saved, you won't be able to edit your Quest 5.1 game in Quest 5.0).

Please note that a few changes have been made to enable Quest games to be converted to Javascript (so they can run as stand-alone apps on iPhone, Android etc.). This means that some functions have been deprecated - this means that you can still use them, but I would encourage you to use the suggested replacements instead, particularly if you plan to convert your game into an app:

-   the [ShowMenu](functions/showmenu.html) function is deprecated - use the new [show menu](scripts/show_menu.html) script command instead
-   the [WaitForKeyPress](functions/corelibrary/waitforkeypress.html) (and "Wait" [request](scripts/request.html)) is deprecated - use the new [wait](scripts/wait.html) script command instead
-   the [Ask](functions/ask.html) function is deprecated - use the new [ask](scripts/ask.html) script command instead
-   the + and - operators on lists are deprecated - use the new [ListCombine](functions/listcombine.html) and [ListExclude](functions/listexclude.html) functions instead

Some minor behaviour changes:

-   when playing videos with [ShowYouTube](functions/corelibrary/showyoutube.html) or [ShowVimeo](functions/corelibrary/showvimeo.html), the video now automatically starts playing
-   the static picture frame feature is no longer an optional extra - there is no need to set the [useframe](attributes/useframe.html) option.

New features:

-   there is a new "Simple Mode" in the Editor, which can be toggled on and off from the Tools menu. This hides away much of Quest's more advanced functionality, so is useful for beginners, or if you don't need to use the more advanced features.
-   you can now add, view and edit comments in the script editor
-   [walkthrough assertions](using_walkthroughs.html#Assertions) let you test the value of expressions when a walkthrough is running
-   new [while](scripts/while.html) script command
-   [for](scripts/for.html) has a new "step" parameter
-   new [ObjectLink](functions/corelibrary/objectlink.html) and [CommandLink](functions/corelibrary/commandlink.html) functions make it easier to output hyperlinks within a script

Other changes:

-   ["changedXXXX" script attributes](change_scripts.html) can now access "oldvalue" to get the previous value of the attribute they're watching
-   richer set of attributes for use...on... and give...to...: [useanything](attributes/useanything.html), [giveanything](attributes/giveanything.html), [selfuseon](attributes/selfuseon.html), [selfuseanything](attributes/selfuseanything.html), [giveto](attributes/giveto.html), [givetoanything](attributes/givetoanything.html)
-   new [onexit](attributes/onexit.html) script can be run when leaving a room
-   new [GetAllChildObjects](functions/getallchildobjects.html) function, and the related [GetDirectChildren](functions/getdirectchildren.html) function has been moved from Core.aslx to become a Quest function
-   new [IsDefined](functions/isdefined.html) function
-   the [create exit](scripts/create_exit.html) script command now lets you specify a name for the exit object
-   new [IsGameRunning](functions/isgamerunning.html) function
-   new [request](scripts/request.html) types RequestSave and SetPanelContents
-   new optional cacheID parameter for regex functions [IsRegexMatch](functions/isregexmatch.html), [Populate](functions/populate.html), [GetMatchStrength](functions/getmatchstrength.html)

Upgrading from Quest 5.0 Beta Versions
--------------------------------------

### Upgrading from 5.0 Beta 4

For Quest 5.0 (Release Candidate 1) and later, each compass exit has its own type. You will get some slightly odd behaviour if you don't update your exits - for example, if you have both north and northwest exits from a room, typing "n" will give you a disambiguation menu. To prevent this, go to each exit in your game and choose the direction from the "Type" dropdown, so it matches the Alias.

### Upgrading from 5.0 Beta 2

For Beta 3 and later, the "player" object has been moved out of Core.aslx. This means if you have a game created for Beta 1 or Beta 2, you will need to add the "player" object yourself. You just need to make sure the object name is "player" and that it inherits the "defaultplayer" type.

You can simply copy and paste this code block into your game file:

     <object name="player">
       <inherit name="defaultplayer" />
     </object>

Upgrading from Quest 4.x
------------------------

See separate article [Upgrading from Quest 4](upgrading_from_quest_4.html)

Upgrading to development versions
---------------------------------

Development versions are unsupported. This section of the documentation is to note changes which will be moved to a new section above when the development version is released properly.
