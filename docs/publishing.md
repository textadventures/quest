---
layout: index
title: Publishing Quest games (inc. size limitations)
---

To get your game playable on the Text Adventurers web site, you need to publish it.

Note that once you have published it, your game will go into a queue for moderation. Games may be assigned to "sandbox" if they are very basic, or to "adult" if they include explicit sex, etc., otherwise they will be assigned to the appropriate category, and will appear on the web site.

Moderation can take a few days; please be patient.


Publish On-line
---------------

In the on-line editor, click the _Publish_ button at the top right, then just follow the instructions. If you later update your game, you can click _Publish update_, and it will be updated.


Publish Off-line
----------------

This is a little more complicated, but not much. In the editor, go to _Tools - Publish_, give it a name (the same as your main file by default, but with a .quest extension), and click okay. Quest will produce a file in a subfolder of your main game called "Output".

On the Text Adventurers web site, click on _Create_ at the top, then _Upload_ game below that. Then follow the instructions.


The Publish Process
--------------------

What gets included in the .quest file, when you do _Tools - Publish_? Broadly two things.

Firstly the game code. This is all the code from all the libraries, including the built-in libraries, from whatever folders on your PC, assembled into one big file. This means that if, in a few years, Quest's built-in libraries get radically updated, your game will not be affected.

Secondly, any supporting files. This is any file Quest can find in your game folder with a certain name format, whether they are used in your game or not. Images and sounds that are not in this folder will not be included, images and sounds that are in it, but not used will be included. Note that when you select images and sounds through the Quest GUI, it will copy the file into the game folder, so in theory all these files should already be there.

Quest grabs any file with a name that matches one of these formats

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg

However, you can modify that by changing `game.publishfileextensions`; despite the name, it is not restricted to file extensions. If you have a text file you want included, but other you do not, you could set it like this:

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg;includeme.txt

The single code file plus all the supporting files are then compressed in a single archive file.


Size Limitations
----------------

The Quest off-line editor has no limits, though eventually your computer will grind to a halt. Given the capabilities of modern computers that is really not going to happen.

The Text Adventurers web site, however, has a 20 Mb upload limit. This is the size of the published .quest file, and if your game is larger than that, the editor will give you a warning when you do _Tools - Publish_. In terms of game, that is a huge amount, and you will be doing well to build a game that is even 1 Mb. However, images, videos and sounds can seriously inflate the file size.

I would guess the on-line editor is also limited to 20 Mb.

If you do find your game in over 20 Mb:

* Remove files that are not used from the game folder

* Use smaller or lower quality clips (I have heard Pixillion Image Converter and Switch Sound Converter are useful here)

* Host larger video/image/sound files on another web site

* Host your Quest game on your own web server (this is not trivial, is not well-supported and may require spending money, so not to be undertaken lightly)


Beta-testing
------------

Beta-testing is getting other people to play your game so bugs and typos can be identified and corrected before release to the public. If you do not know anyone who can do this for you, it is worth asking on the forum.

You can upload a game to Text Adventures in the normal way for beta-testing, but keep its visibility to private. If working on-line, there is a "Publish Update" button in your list of games, allowing you to publish updates during the testing process. If working off-line, there is an "Upload a new file" link on the _Edit_ page.

One thing you _must_check is saving the game. For some reason when you save, Quest does a particularly thorough check of the game code, and the save will fail if it finds an error (on-line it will not tell you it has failed, by the way, it will just not do anything). Do not release your game until it will successfully save.

You should also ensure the UI looks the same when you load a saved game.

When it is ready for release, go to "View/Edit Game Listing", and change the visibility to public. Remember to thank your beta-testers.


Spell checking
--------------

For once, the on-line editor wins when it comes to spell-checking, as long as you use a browser that supports it.

If off-line, one technique is to open the source code in a text editor that has a spell-checker, such as _Notepad++_ (which can be downloaded for free). The source code can look intimidating, and you need to be careful only to correct text that will be seen, not code or XML. With Notepad++ you can set the language to XML, which will help.

Before doing this, it is best to quit Quest altogether, and then create a back-up of your file.