---
layout: index
title: The Publish Tool
---

<div class="alert alert-info">
Note: The Publish tool is only applicable to the Windows desktop version of Quest (the web version is much easier).

</div>

What gets included in the .quest file, when you do _Tools - Publish_? Broadly two things.

Firstly the game code. This is all the code from all the libraries, including the built-in libraries, from whatever folders on your PC, assembled into one big file. This means that if, in a few years, Quest's built-in libraries get radically updated, your game will not be affected.

Secondly, any supporting files. This is any file Quest can find in your game folder with a certain name format, whether they are used in your game or not. Images and sounds that are not in this folder will not be included, images and sounds that are in it, but not used will be included. Note that when you select images and sounds through the Quest GUI, it will copy the file into the game folder, so in theory all these files should already be there.

Quest grabs any file with a name that matches one of these formats

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg

However, you can modify that by changing `game.publishfileextensions`; despite the name, it is not restricted to file extensions. If you have a text file you want included, but other you do not, you could set it like this:

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg;includeme.txt

The single code file plus all the supporting files are then compressed in a single archive file.


## Size Limitations

The Quest off-line editor has no limits, though eventually your computer will grind to a halt. Given the capabilities of modern computers that is not going to happen.

The Text Adventurers web site, however, has a 20 Mb upload limit. This is the size of the published .quest file, and if your game is larger than that, the editor will give you a warning with you do _Tools - Publish_. In terms of game, that is a huge amount, and you will be doing well to build a game that is even 1 Mb. However, images, videos and sounds can seriously inflate the file size.

I would guess the on-line editor is also limited to 20 Mb.

If you do find your game in over 20 Mb:

* Remove files that are not used from the game folder

* Use smaller or lower quality clips (I have heard Pixillion Image Converter and Switch Sound Converter are useful here)

* Host larger video/image/sound files on another web site

* Host your Quest game on your own web server (this is not trivial, is not well-supported and may require spending money, so not to be undertaken lightly)
