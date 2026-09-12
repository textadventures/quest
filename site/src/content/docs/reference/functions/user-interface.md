---
title: "User interface functions"
sidebar:
  order: 5
---

Functions that change what is displayed or how it is displayed or require the player to do something, rather than affecting the game world.

## AddPageLink
```quest
AddPageLink (object source, object destination, string text)
```

Adds an option to the `dialoguepage` **source** linking to **destination**, displayed as **text**. If a link to that destination already exists, it's replaced. See also [RemovePageLink](#removepagelink) and [ShowPage](#showpage). There's also an [AddPageLink](/reference/functions/gamebook#addpagelink) for gamebook pages, which works the same way.

## Ask
```quest
Ask (string question)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Asks the player the specified **question**, showing "Yes" and "No" as numbered links in the transcript, and returns a [boolean](/types#boolean) - **true** if they answer "Yes". The player can click a link or type its number. The script is suspended until they have answered, so the result can go straight into an `if`:

```quest
if (Ask ("Are you sure?")) {
  msg ("Yes, you are")
}
else {
  msg ("Changed your mind, then")
}
```

This replaces the [ask](/scripts#ask) script command, which is no longer offered when you add a script command.

### The callback form

There is a second form, which takes a script to run once the player has answered:

```quest
Ask (string question)  { script }
```

The script can read a [boolean](/types#boolean) variable **result**, **true** if the player answered "Yes".

```quest
Ask ("Are you sure?") {
  if (result){
    msg ("Yes, you are")
  } 
}
```

This form is still offered in the script editor. Both forms look the same to the player; the difference is that this one ends the turn before waiting, so the player can save, load and undo while choosing, whereas the plain `Ask (question)` form suspends the script mid-turn and save is unavailable until it is answered.

**Note:** The callback form is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts). Neither caveat applies to the plain `Ask (question)` form above, which simply returns a value.

## ClearFramePicture
```quest
ClearFramePicture
```

Clears the static frame picture. Use [SetFramePicture](#setframepicture) to set the frame picture.

Does not return a value.

## ClearScreen
```quest
ClearScreen
```

Clears the screen.

## DisplayList
```quest
DisplayList (list, boolean numbers)
```

Outputs the specified list using \<ol\> (ordered list, if numbers is true) or \<ul\> (unordered list, if numbers is false).

## DisplayMailtoLink
```quest
DisplayMailtoLink(string displaylink, string email)
```

Displays a maillink, which will open an external mailclient.

## EndPageDialogue
```quest
EndPageDialogue ()
```

Ends the current [Pages](#showpage) dialogue, hiding the current page's option links. Callable from a page script, or automatically run when the player cancels out of a dialogue that allows it.

## GetCurrentFontFamily
```quest
GetCurrentFontFamily ()
```

Returns the fonts currently in use - the [defaultwebfont](/attributes#defaultwebfont) and [defaultfont](/attributes#defaultfont).

## GetInput
```quest
GetInput()
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Waits for the user to enter some text at the command prompt. Instead of handling the input as a command, it is returned as the result of the function, as a [string](/types#string).

The script is suspended where it is, so the line after the `GetInput()` call does not run until the player has answered:

```quest
msg ("What is your name?")
player.alias = GetInput()
msg ("Hi, " + player.alias)
```

In the script editor, this is the "player's typed input" template on a "Set a variable or attribute" action. See [Asking a question](/howto/tasks/asking-a-question) for a fuller guide.

**Note:** Quest 5.2 deprecated this function, and Quest 5.4 to 5.8 rejected it outright, in favour of the [get input](/scripts#get-input) script command - blocking the game to wait for an answer tied up a real thread in those versions. Quest Viva suspends the script instead, so the function is available again in games marked as ASL version 600, and is now the better of the two. It still raises an error in a game whose version is 540 to 580; change the game's version to 600 or later to use it.

## GoToPage
```quest
GoToPage (object page)
```

Displays the given [dialoguepage](#showpage) object: prints its description, then its options as a numbered list of links (or ends the dialogue if it has none). Callable from a page's own script to redirect to a different page, the same way [ShowPage](#showpage) starts at one.

## HasSeenPage
```quest
HasSeenPage (object page)
```

Returns a [boolean](/types#boolean) - **true** if the given [dialoguepage](#showpage) has been visited before (its `visited` attribute), for conditional page text or options based on what the player has already seen.

## InitUserInterface
```quest
InitUserInterface ()
```

InitUserInterface is an empty method which can be imported and overwritten by the user. It is called after initializing the interface after starting or loading a game. It can be used to reload user generated stylesheet or gui elements.

An alternative approach is to add a user interface initialisation script on the _Advanced Features_ tab of the game object (turn on the tab on the _Features_ tab of the game object). This is effectively the same but can also be used on the web version.

## OutputText
```quest
OutputText (string text)
```

Prints the specified text.

This is called by the `msg` script command, and so does the same as that (for any game created in Quest 5.4 or later). Text is passed through the text processor before printing.

## OutputTextNoBr
```quest
OutputTextNoBr (string text)
```

Prints the specified text, without a line break at the end. The next text printed will appear on the same line.

The usual `OutputText` (or `msg`) adds an HTML "br" element to the end of the text, to indicate the end of the line; this function omits it.

## OutputTextRaw
```quest
OutputTextRaw (string text)
```

Prints the specified text, without passing the text through the text processor.

## OutputTextRawNoBr
```quest
OutputTextRawNoBr (string text)
```

Prints the specified text, without a line break at the end and without passing the text through the text processor. The next text printed will appear on the same line.

The usual `OutputTextRaw` adds an HTML "br" element to the end of the text, to indicate the end of the line; this function omits it.

## PrintCentered
```quest
PrintCentered(string text)
```

Prints the specified text the same way [msg](/scripts/#msg) does, just centered instead of left-aligned - the text can include HTML as usual.

## RemovePageLink
```quest
RemovePageLink (object source, object destination)
```

Removes the option (if any) on the `dialoguepage` **source** that links to **destination**. See also [AddPageLink](#addpagelink). There's also a [RemovePageLink](/reference/functions/gamebook#removepagelink) for gamebook pages, which works the same way.

## SetAlignment
```quest
SetAlignment(string alignment)
```

Sets the text alignment for all the text which follows. Valid alignment values are "left", "center" or "right".

## SetBackgroundColour
```quest
SetBackgroundColour(string colour)
```

Sets the colour of the background. [Here](http://www.html-color-names.com/color-chart.php) is a site which shows the HTML-colour-names.

## SetBackgroundImage
```quest
SetBackgroundImage (filename)
```

Sets the background image to the specified file.

## SetBackgroundOpacity
```quest
SetBackgroundOpacity(float value)
```

Sets the opacity of the background (how transparent it is). This should be a number from 0.0 (completely transparent) to 1.0 (completely opaque). Note that this function only changes the value stored by Quest Viva, and it is necessary to call `SetBackgroundColour` to get Quest Viva to update the UI. If you do not want to change the background colour, you can do this, for example:

```quest
SetBackgroundOpacity(0.5)
SetBackgroundColour(game.defaultbackground)
```

## SetFontName
```quest
SetFontName(string fontname)
```

Sets the font.

## SetFontSize
```quest
SetFontSize(int fontsize)
```

Sets the font size.

## SetForegroundColour
```quest
SetForegroundColour(string colour)
```

Sets the colour of the text. [Here](http://www.html-color-names.com/color-chart.php) is a site which shows the HTML-colour-names.

## SetFramePicture
```quest
SetFramePicture (string filename)
```

Sets the static frame picture to the specified file. Use [ClearFramePicture](#clearframepicture) to clear the frame.

Does not return a value.

## SetWebFontName
```quest
SetWebFontName(string fontname)
```

Sets the web font. Here you can see all available fonts: <https://fonts.google.com/>

## ShowMenu
```quest
ShowMenu (string caption, stringdictionary or stringlist options, boolean allow cancel)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Shows the specified options as a numbered list of links in the transcript and returns the player's choice, as a [string](/types#string). The player can click an option or type its number. If a dictionary of options is passed in, the values are displayed as options and the key is returned; if a list of options is passed in, the list item is returned. The script is suspended until the player has chosen, so the result can go straight into a variable:

```quest
colour = ShowMenu ("What is your favourite colour?", Split("Red;Green;Blue;Yellow", ";"), false)
msg ("You chose " + colour)
```

If the "allow cancel" parameter is set to **true**, entering any command other than one of the option numbers dismisses the menu, and `ShowMenu` returns an empty string (the dismissing command itself is discarded). If it is set to **false**, the player must choose one entry of the menu, and anything else they type is ignored.

The [Split](/reference/functions/string#split) function can be useful to quickly get a list of options, whilst [switch](/scripts#switch) can be useful for dealing with the result. Because one call simply follows another, asking several questions in a row needs no nesting:

```quest
colour = ShowMenu ("What is your favourite colour?", Split("Red;Green;Blue;Yellow", ";"), false)
animal = ShowMenu ("Okay, and what is your favourite animal?", Split("Dog;Turtle;Duck;Newt;Trout", ";"), false)
msg ("Really? A " + LCase(colour) + " " + LCase(animal) + " fan.")
```

This replaces the [show menu](/scripts#show-menu) script command, which is no longer offered when you add a script command.

### The callback form

There is a second form, which takes a script to run once the player has chosen:

```quest
ShowMenu (string caption, stringdictionary or list options, boolean allow ignore)  { script }
```

The script can read a [string](/types#string) variable **result** containing the player's choice. If a list of objects is passed in, **result** is the object's name, and an object with a link colour specified has that colour used for its link.

If the "allow ignore" parameter is set to **true**, the player can ignore the menu and interact with other objects. The menu is just closed then. If the "allow ignore" parameter is set to **false**, the player must choose one entry of the menu.

```quest
options = Split("Red;Green;Blue;Yellow", ";")
ShowMenu ("What is your favourite colour?", options, false) {
  switch (result) {
    case ("Red") {
      msg ("You must be very passionate. Or like a teamthat play in red.")
    }
    case ("Yellow") {
      msg ("What a bright, cheerful colour!.")
    }
    case ("Green", "Blue") {
      msg (result + "? Seriously?")
    }
  }
}
```

This form is still offered in the script editor. Both forms look the same to the player; the difference is that this one ends the turn before waiting, so the player can save, load and undo while choosing, whereas the plain `ShowMenu (...)` form suspends the script mid-turn and save is unavailable until it is answered.

The callback form will also take an object list, or a list of objects and strings. Note that `result` will always be a string - in the case of an object, it will be the object's name.

```quest
ShowMenu ("Select", ScopeInventory(), true) {
  obj = GetObject(result)
  RemoveObject(obj)
  msg ("You smash the " + obj.name + " to bits.")
}
```

**Note:** The callback form is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts). Neither caveat applies to the plain `ShowMenu (caption, options, allow cancel)` form above, which simply returns a value.

## ShowPage
```quest
ShowPage (object page, boolean allowCancel, boolean runTurnScripts)  { script }
```

Starts a branching dialogue at the given `dialoguepage` object, for building NPC conversations or other choice-driven text out of linked pages rather than [ShowMenu](#showmenu) callbacks. Each page has a description and a set of options (added with [AddPageLink](/reference/functions/gamebook#addpagelink)) linking to other pages; choosing an option is a normal command, so - unlike a ShowMenu-based dialogue - the game is fully idle between choices and save/load/undo work throughout.

- **allowCancel**: if true, entering any command other than a numbered option or option name ends the dialogue (via [EndPageDialogue](#endpagedialogue)) and then runs normally; if false, the player is told to choose one of the options.
- **runTurnScripts**: whether turn scripts should fire for each choice made during the dialogue. Off by default, since each choice is a real turn and most games don't want e.g. hunger daemons ticking mid-conversation.

See also [GoToPage](#gotopage) (jump to a different page from within a page's own script), [HasSeenPage](#hasseenpage), [EndPageDialogue](#endpagedialogue), and [AddPageLink](#addpagelink)/[RemovePageLink](#removepagelink) for building a page's options from a script instead of the editor's Options list.

## ShowYouTube
```quest
ShowYouTube(string id)
```

Plays a video from YouTube.

You will need the YouTube id of the video - an easy way to get this for a YouTube video is to find the video you want and click Share. The id will be displayed at the end of a URL like `https://youtu.be/qDlakzXcnro` where "qDlakzXcnro" is the id you want.

## TextFX_Typewriter
```quest
TextFX_Typewriter(text, int speed)
```

Displays the text one character at a time. The speed parameter specifies the length delay between characters, in milliseconds.

## TextFX_Unscramble
```quest
TextFX_Unscramble(text, int speed, int reveal)
```

Shows animated scrambled text which is gradually replaced with the text specified. The speed parameter specifies the time delay in milliseconds between updates, and the reveal parameter specifies how many characters of the original text to display on each update.

## UpdateStatusAttributes
```quest
UpdateStatusAttributes ()
```

Updates the status attributes box.

## WaitForKeyPress
```quest
WaitForKeyPress ()
```

Waits for a keypress. As with [GetInput](#getinput), the script is suspended where it is, so there is no nested block and the next line runs once the player has pressed a key:

```quest
msg ("First bit")
WaitForKeyPress
msg ("Second bit")
```

**Note:** Quest 5.1 deprecated this function in favour of the [wait](/scripts#wait) script command, for the same reason as [GetInput](#getinput) above. Quest Viva suspends the script rather than blocking a thread, so it is available again - and preferred - in games marked as ASL version 600. It still raises an error in a game whose version is 540 to 580.
