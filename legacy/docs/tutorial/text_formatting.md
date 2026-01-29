---
layout: index
title: Text formatting
---

Up until now, our Quest game has been entirely plain text. In this section, we'll look at jazzing things up a bit by using text formatting, and including pictures, sounds and video.

Setting the Default Font and Colours
------------------------------------

To change the default font and colours used throughout your game, select "game" from the tree and go to the Display tab.

Here you can set the initial font, foreground, background and hyperlink colours.

Changing Fonts and Colours during the Game
------------------------------------------

You can also change the font and colours while the game is in progress. You can do this using script commands. The commands are all in the "Output" category - you can change the foreground and background colour, and also the font and font size.

Bold, Italic, Underline
-----------------------

You can use HTML-style formatting to set bold, italic and underline. For example, when printing a message, you can type this:

     This text is <b>bold</b>. This text is <i>italic</i>. This text is <u>underlined</u>.

This will output:

This text is <b>bold</b>. This text is <i>italic</i>. This text is <u>underlined</u>.

Up to Quest 5.3, you can also temporarily override colour using a "color" tag, for example:

     This text is the current foreground colour, but <color color="red">this is red</color>.

The "color" tag is not supported as of Quest 5.4, as all output is now plain HTML. This means you can use "span" tags with in-line CSS styles to set the colour of a range of text instead, however, as of Quest 5.7, you can use text processor commands.

> here is some text with {colour:blue:this} written in blue and {back:red:that} with a red background.
