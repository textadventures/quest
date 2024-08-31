---
layout: index
title: Releasing your game
---

There are five stages to releasing a Quest game.

1.  Before release testing
2.  Upload as an unlisted game
3.  Upload testing
4.  Public upload
5.  Announcement


Before releasing testing
------------------------

Before you even think about releasing your game, you need to thoroughly check to make sure it works properly, and that it has sensible responses for things that a player might reasonably type while playing it. To create a good game is a lot of hard work, and while it might be tempting to release your first efforts after a minimal amount of testing, your players won’t thank you for it.

And no matter how tempting it might be, do not even *think* about releasing the game you’ve created while working your way through this tutorial!

Here are some things to think about before unleashing your game on an unsuspecting public:

-   Think about all the objects a player might refer to – make sure everything you refer to in your descriptions is at least set up as a scenery object.
-   Think about all the different things a player might reasonably try to do with an object – set up verbs, even if they just tell the player that they can't do that.
-   Think about all the different ways a player might type a command, and make sure you have enough verb alternatives set up.
-   Think about the different ways a player might refer to the same thing, and set up alternative names.
-   Make sure you **test** your game thoroughly.
-   Spell check!




Upload as an unlisted game
--------------------------

This process is quite different, depending on whether you are using the web editor or the desktop version. By default your game will be unlisted; leave it like that for now.

### Web version

In the on-line editor, click the _Publish_ button at the top right, then just follow the instructions. If you later update your game, you can click _Publish_ again, and it will be updated.


### Desktop version

This is a little more complicated, but not much. In the editor, go to _Tools - Publish_, give it a name (the same as your main file by default, but with a .quest extension), and click okay. Quest will produce a file in a subfolder of your main game called "Output".

On the Text Adventurers web site, click on _Create_ at the top, then _Upload_ game below that. Then follow the instructions.

For more on the Publish tool, including the size limitations, see below.


Upload testing
--------------

Now play the uploaded version of your game. I would recommend saving, and ensuring a saved game can be loaded and looks okay, as saving and loading tend to be essentially sensitive to errors in scripts!

Now get some other people to test it – you'll be surprised at all the things they pick up that you would never have thought of. This is called beta-testing, and while it can be a pain, especially as you are keen to get your game out there fast, is well worth it in the long run. A couple of bugs in your game will quickly lead to bad reviews.


Public upload
---------------

Once all the bugs are sorted, upload your game again, just as before. Check the game listing text is fine, and set who can access the game to everyone. Congratulations, your game is now live!


Announcement
------------

Now all you have to do is tell people about it, so they will play it.

You can announce your game on:

-   the [textadventures.co.uk Game Announcements and Feedback forum](https://textadventures.co.uk/forum/games)
-   the [intfiction.org forums](http://www.intfiction.org/forum/viewforum.php?f=19)
-   [IFDB](http://ifdb.tads.org/)
-   [rec.games.int-fiction newsgroup](http://groups.google.com/group/rec.games.int-fiction)






A Note on The Publish Process
-------------------

The applies to the Publish tool on the desktop version - it is all done automatically for you on the web version.

What gets included in the .quest file, when you do _Tools - Publish_? Broadly two things.

Firstly the game code. This is all the code from all the libraries, including the built-in libraries, from whatever folders on your PC, assembled into one big file. This means that if, in a few years, Quest's built-in libraries get radically updated, your game will not be affected.

Secondly, any supporting files. This is any file Quest can find in your game folder with a certain name format, whether they are used in your game or not. Images and sounds that are not in this folder will not be included, images and sounds that are in it, but not used will be included. Note that when you select images and sounds through the Quest GUI, it will copy the file into the game folder, so in theory all these files should already be there.

Quest grabs any file with a name that matches one of these formats

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg

However, you can modify that by changing `game.publishfileextensions`; despite the name, it is not restricted to file extensions. If you have a text file you want included, but other you do not, you could set it like this:

> *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg;includeme.txt

The single code file plus all the supporting files are then compressed in a single archive file.


### Size Limitations

The Quest off-line editor has no limits, though eventually your computer will in theory grind to a halt. Given the capabilities of modern computers that is not going to happen.

The Text Adventurers web site, however, has a 20 Mb upload limit. This is the size of the published .quest file, and if your game is larger than that, the editor will give you a warning with you do _Tools - Publish_. In terms of game, that is a huge amount, and you will be doing well to build a game that is even 1 Mb. However, images, videos and sounds can seriously inflate the file size.

I would guess the on-line editor is also limited to 20 Mb.

If you do find your game in over 20 Mb:

* Remove files that are not used from the game folder

* Use smaller or lower quality clips (I have heard Pixillion Image Converter and Switch Sound Converter are useful here)

* Host larger video/image/sound files on another web site

* Host your Quest game on your own web server (this is not trivial, is not well-supported and may require spending money, so not to be undertaken lightly)


