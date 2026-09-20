---
title: Pictures
description: Add picture files to your game, show them in the text or in a fixed frame above it, and control their size and position
---

A picture can set a scene faster than a paragraph can. Quest Viva can drop one into the game text, hold one in a fixed frame above the text, or hide one behind a link the player clicks.

| You want | Use |
|---|---|
| A picture as part of a script | The ["Show a picture"](#showing-a-picture) command |
| A picture inside a room or object description | [`{img:file.png}`](#a-picture-in-a-description) |
| A picture that stays put while the text scrolls | The [picture frame](#the-picture-frame) |
| A picture the player opens by clicking | [`{popup:...}`](#a-picture-the-player-can-open) |
| Control over position and size | [Your own `<img>` tag](#size-position-and-phone-screens) |
| A picture that changes as the game goes on | [Swap it, or draw it in code](#changing-a-picture-during-the-game) |

## Adding picture files

Anywhere the editor asks for a picture there's a box with the filename in it and an **Upload…** button beside it. Click **Upload…** and choose a file, and it's added to the game; the box also drops down a list of every picture already in the game, so you only upload each one once.

You can also manage the whole lot at once: **Manage assets** on the toolbar opens the **Assets** dialog, which lists every file in the game with its own **Upload…** button, and lets you delete the ones you no longer need.

The picture boxes offer **.jpg**, **.jpeg**, **.png** and **.gif**. Use JPEG for photographs and PNG for everything else - PNG compresses without losing quality and supports transparency, so the image can appear to be any shape you like. The player will happily display other formats too, such as WebP and SVG; upload those through the **Assets** dialog and type the filename into the box yourself.

Every file in the game is packaged when you publish, whether the game uses it or not, so delete anything you've stopped using. The **Publish** dialog tells you how many files it's including and how big they are, and warns you if the total goes over textadventures.co.uk's 50 MB limit - pictures are usually what gets you there. See [The publish process](/publishing/publishing#the-publish-process).

## Showing a picture

In any script, click **+ Add script**, choose the **Output** category and then **Show a picture**. Pick or upload the file, and that's it:

```quest
picture ("gravestone.png")
msg ("A large white room, empty but for a single gravestone.")
```

The picture appears in the game text at that point, on its own line, and scrolls away with everything else. If you clear the screen, it goes with the rest of the text.

The filename is an expression, so it doesn't have to be fixed:

```quest
picture (game.pov.parent.name + ".png")
```

If the file isn't in the game, the player shows a broken-image icon rather than an error, so check the spelling if nothing appears.

## A picture in a description

To put a picture inside some text rather than on a line of its own, use the [text processor](/howto/world/text-processor) directive `{img:...}`. It works anywhere text is printed - a room description, an object description, a message:

```
A rusty iron torch, still smoking. {img:torch.png}
```

That's usually the easiest way to give an object a picture: select it, go to its _Setup_ tab, and add the directive to **"Look at" object description**.

## The picture frame

A picture per room quickly becomes tedious if you have to script each one, and you probably don't want the picture scrolling away as the player reads. The picture frame is a fixed area above the text: the text scrolls underneath it, and the picture changes as the player moves.

Select the **game** object, go to the _Interface_ tab, and tick **Picture frame** at the bottom. Each room then gets a **Room picture** box at the top of its _Room_ tab - pick or upload a picture there, and it appears in the frame whenever the player is in that room.

**Clear picture panel if room has no picture** is ticked as well by default, so moving into a room with no picture empties the frame. Untick it and the last picture stays up until another room replaces it, which suits a game where the picture stands for a whole region rather than a single room.

Turning the frame on also adds two more script commands to the **Output** category, for changing the picture mid-room:

```quest
SetFramePicture ("storm.png")
ClearFramePicture
```

Moving to another room overrides either of these, since the room's own picture is applied on entry.

The frame is scaled to fit: never wider than the text, and never more than half the height of the player's window, so a tall picture is shrunk rather than pushing the text off the screen. Unlike a picture in the text, the frame survives a screen clear.

## Size, position and phone screens

A picture in the text is already capped at the width of the text column, on a phone as well as a desktop, so an oversized picture is scaled down instead of forcing the page sideways. Height isn't capped, though, so a very tall picture will push the text a long way down - crop it, or use the picture frame.

For anything more - floating the picture so text flows around it, centring it, making it smaller - write the `<img>` tag yourself and print it with `msg`. Use `GetFileURL` to turn the filename into an address that works whether the game is played online, offline or in the desktop app:

```quest
msg ("<img src='" + GetFileURL("gravestone.png") + "' style='float:left; padding-right:15px;' />")
msg ("A large white room, empty but for a single gravestone. Yours, if the inscription is to be believed.")
```

`style` takes [CSS](https://developer.mozilla.org/en-US/docs/Web/CSS), as a list of `name: value;` pairs. The useful ones here:

| To | Use |
|---|---|
| Flow text around the picture | `float:left;` or `float:right;` |
| Leave a gap beside it | `padding-right:15px;` |
| Centre it | `display:block; margin-left:auto; margin-right:auto;` |
| Make it smaller | `width:200px;` - set only one of width and height and the other follows |
| Fade it | `opacity:0.5;` |

Single quotes inside the tag save you escaping double quotes all the way through the string.

## A picture the player can open

To keep a big picture out of the way until it's wanted, put it in a popup. The `{popup:...}` directive shows a link, and the picture appears when the player clicks it, disappearing again on the next click:

```
The map is pinned above the desk. {popup:Study the map:{img:map.png}}
```

`JS.showPopup` does the same from a script, in a proper dialogue box with a title and an OK button:

```quest
JS.showPopup ("The map", "<img src='" + GetFileURL("map.png") + "' style='max-width:100%' />")
```

That box is a fixed width, so scale the picture down with `max-width:100%` as above, or use [`JS.showPopupCustomSize`](/js#showpopupcustomsize) to choose your own.

## Changing a picture during the game

Give a picture an `id` when you print it, and a later script can point it at a different file. The picture changes where it stands, without a new one appearing further down:

```quest
msg ("<img id='portrait' src='" + GetFileURL("knight-well.png") + "' />")
```

```quest
JS.eval ("document.getElementById('portrait').src = '" + GetFileURL("knight-wounded.png") + "';")
```

This is how you show a character's changing mood, a door opening, or a dial moving, without filling the screen with pictures. See [Customising the interface](/howto/ux/customising-the-ui) for more of what `JS.eval` can do.

### Drawing a picture in code

You don't always need a file. An `<svg>` element is a picture described in text, which the browser draws - so a script can build one from the state of the game. A health bar, for example:

```quest
width = 200 * game.pov.health / 100
s = "<svg width=\"200\" height=\"16\" role=\"img\" aria-label=\"Health " + game.pov.health + " per cent\">"
s = s + "<rect width=\"200\" height=\"16\" fill=\"#333\" />"
s = s + "<rect width=\"" + width + "\" height=\"16\" fill=\"crimson\" />"
s = s + "</svg>"
msg (s)
```

The same trick draws a thermometer, or a floor plan that fills in as the player explores. [MDN's SVG tutorial](https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorials/SVG_from_scratch) covers the shapes available. Give every generated picture an `aria-label` saying what it shows, so it isn't a blank to a player using a screen reader.

For a map of the rooms themselves, you don't need any of this - see [Showing a map](/howto/tasks/showing-a-map).

## Hosting pictures elsewhere

If your pictures push the game over the upload limit, you can leave them on a website and point at them instead - use the full address and skip `GetFileURL`:

```quest
msg ("<img src='https://example.com/images/gravestone.png' />")
```

The catch is that the game breaks if that site goes away or changes the address, and it won't work offline or in the desktop app at all. Prefer [hosting the game yourself](/publishing/hosting), which has no size limit, and keep the pictures inside it.

## See also

- [Sound and video](/howto/multimedia/adding-sounds)
- [The text processor](/howto/world/text-processor) - `{img:}`, `{popup:}` and the rest
- [Showing a map](/howto/tasks/showing-a-map)
- [Changing the look of your game](/howto/ux/ui-style) - themes, colours and the margin image
