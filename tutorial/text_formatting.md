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

     This text is &lt;b&gt;bold&lt;/b&gt;. This text is &lt;i&gt;italic&lt;/i&gt;. This text is &lt;u&gt;underlined&lt;/u&gt;.

This will output:

This text is <b>bold</b>. This text is <i>italic</i>. This text is <u>underlined</u>.

Up to Quest 5.3, you can also temporarily override colour using a "color" tag, for example:

     This text is the current foreground colour, but &lt;color color="red"&gt;this is red&lt;/color&gt;.

The "color" tag is not supported as of Quest 5.4, as all output is now plain HTML. This means you can use "span" tags with in-line CSS styles to set the colour of a range of text instead.

Inserting Pictures
------------------

You can show a picture using the "Show a picture" command. Let's make our "kitchen" room show a picture of a kitchen when we enter it.

First we need to find a picture of a kitchen.

-   You could use [Google Images](http://images.google.com), which will be fine for our demonstration – but bear in mind if you want to distribute images with your game, they must be images you've created yourself, or at least you must have permission from the copyright owner.
-   You could do an [<http://www.flickr.com/search/advanced/>? advanced Flickr search] to find images which are released under a Creative Commons licence - this often means you can include the image in your game as long as you credit the original author. Check which variant the author has used before including the picture in your game though.
-   A good website for royalty free images is [iStockphoto.com](http://www.istockphoto.com).
-   Or you could just go to your own kitchen with a digital camera.
-   Or, since this is just a demonstration which you won't be publishing anywhere, you could find a picture of anything else at all.

Now select the "kitchen" room from the tree. Click the Scripts tab, and go to the "After entering the room" script. Add a "Show picture" command, and browse for your kitchen photo. The picture will be automatically copied to the same folder as your game file.

Now launch the game and go to the kitchen to verify that the picture is displayed with the room description.

Static Picture Frame
--------------------

If you want to use a picture for each room, creating a script for each one is a pain, and more importantly, you probably don't want the picture to scroll in-line with the rest of the game text.

Instead, you can use the static picture frame option. This provides a static area at the top of the screen for your picture, while the text scrolls underneath. You can change the picture using a script command, and you can also give each room its own picture.

If you want to have a picture per room, but some of the rooms will have no picture, you should turn on the "Clear picture panel if room has no picture" option. Otherwise, if you go to a room with no picture, the previous picture will still be shown in the frame.

If you click on any room and go to the "Room" tab, you'll see a "Room picture" editor at the top. Here you can browse for a picture file, and it will be copied to your game folder. You can also choose existing picture files from the drop-down.

You can also change the frame picture using a script, with the "Set frame picture" command.

Sound and Video
---------------

You can play a sound in a script using the "Play sound" command. Quest supports both WAV and MP3 files, but I recommend you use MP3 files as much as possible because these are more widely supported by web browsers on different platforms.

You can choose "Wait for sound to finish before continuing" if you want to run the remaining script only after the sound has finished. This is useful for intro sequences, or letting some speech finish before moving the player to a different room, for example.

The "loop" option will cause the sound to continue playing until the "Stop sound" command is run. Note that Quest won't let you use both the "wait" and "loop" options at the same time - hopefully for obvious reasons.

You can also embed video in your game via the "Scripts" tab. The video will appear in-line with the game text. You can embed videos from [YouTube](http://www.youtube.com) or [Vimeo](http://www.vimeo.com). You will need to specify the video id - you can usually see this easily in the URL of a video on the YouTube or Vimeo sites. For example you can find Jason Scott, Director of GET LAMP, on [GoogleTechTalks Channel](http://youtu.be/LRhbcDzbGSU). the Youtube ID is "LRhbcDzbGSU."

You can also find script commands to print an email or web address hyperlink.
