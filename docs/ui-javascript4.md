---
layout: index
title: Customising the UI - Part 3
---

Testing
-------

When you are messing with the interface, it is easy to get things wrong - or try to do something that is not possible. You should test your game to make sure it works as you expect and looks as you expect. In particular, you should check that it still works and looks the same after the player has reloaded a save game, as this is when problems most often come to light, and it is easy to forget to check this.


Various Tricks
--------------

A collection of tricks using the techniques already discussed.

### The "Continue" link

You can change in the colour of hyperlinks on the Display tab of the game object, but it does not affect the "Continue" message when the game waits for the player to press a button, because that is actually part of the command line, not the output text. However, you can change it with JQuery, like this:

```
JS.setCss ("#txtCommandDiv a", "color:pink;")
```

Note that the first parameter is identifying an `a` element (an HTML anchor, which is used for hyperlinks) inside of the `#txtCommandDiv`.


### The "Saved" text

The message that says the game is saved is also odd, in that is has no ID so cannot be changed through JQuery/CSS.

The solution is to change the style of a container element, however, even that is problematic as they may not exist yet when 'InitUserInterface' fires, so I suggest setting style properties on the body element (this is not an id, so has no # before it.

```
  JS.setCss ("body", "color:orange;font-family:georgia,serif;")
```

### Changing the Ending

The `finish` script command terminates the game, and replaces the panes on the right with a message. You can change the default font using JQuery again, to make it consistent with your game:

```
JS.setCss ("#gamePanesFinished", "font-family:Berkshire Swash;")
```

You can also change what gets displayed, using the JQuery html method. In this example, I am modifying the text (using the `html` method of JQuery), and adding an image (and we have to use GetFileURL to do that). I am also building the string first, and then calling JS.eval.

```
s = "$('#gamePanesFinished').html('<h2>Game Over</h2>"
s = s + "<p>This game has finished and you are dead!".</p><img src=\""
s = s + GetFileURL("gravestone.png")
s = s + "\" />');"
JS.eval (s)
finish
```

### Changing the Arrows

The arrows in the compass rose and the triangles to the left of the panes are icons that are defined in JQuery. To change their color, you need to replace the image file (they are all in one file).

You an get an image file with the right colours, from here:
[http://download.jqueryui.com/themeroller/images/ui-icons_800080_256x240.png](http://download.jqueryui.com/themeroller/images/ui-icons_800080_256x240.png)

You can change the number 800080 to the RGB colour what you want (I guess the file server creates the images on the fly, and will accept any value, but that may not be the case), this is a dark purple I was trying. Save the file in your game folder.

Then you just need to do this to get the new icons in your game (again, modifying the number for your downloaded file):

```
JS.setCss (".ui-icon", "background-image', 'url(" + GetFileURL("ui-icons_800080_256x240.png") + ");")
```

Once you have the file, you could edit it to change the shape of the arrows too, or make them multicoloured (using the web editor, you will need to use the "Show picture" script element to upload the image; once uploaded, delete the "Show picture").


### Disable the panes

This will leave the panes there, but clicking on them will do nothing.

```
JS.setCss ("#gamePanesRunning", "pointer-events:none;")
```

To enable them again:

```
JS.setCss ("#gamePanesRunning", "pointer-events:inherit;")
```


### Moving the screen to the bottom

Sometimes when you display something on the screen, Quest fails to scroll down for. You can force that with this:

```
JS.scrollToEnd()
```

### Sticking the command bar to the bottom of the screen.

You can use this to keep the box where the player types pinned to the bottom of the screen. The first line sets its position to "fixed", this means it will stay in one place relative to the screen. The second line specifies where it will be fixed. The third line stops the game printing messages behind the input box.

```
JS.setCss("#txtCommandDiv", "position:fixed;bottom:10px")
JS.setCss("#gameContent", "margin-bottom:70px;")
```

