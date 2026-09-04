---
title: "Publishing"
sidebar:
  order: 16
---

To get your game playable on [textadventures.co.uk](https://textadventures.co.uk), you need to publish it.

Note that once you have published it, your game will go into a queue for moderation. Games may be assigned to the "Sandpit" category if they are very basic, or to "Adult" if they sexual content, otherwise they will be assigned to the appropriate category, and will appear on the web site.

Moderation can take a few days; please be patient.


## Publishing your game

In the editor, open the **File** menu in the toolbar and choose **Publish…**. This builds a `.quest` package (your game file plus its assets) and downloads it.

On textadventures.co.uk, click on _Create_ at the top, then _Submit_ below that. Then follow the instructions to upload the `.quest` file you just downloaded.


## The publish process

What gets included in the `.quest` file, when you publish?  Broadly two things.

Firstly the game code. This is all the code from all the libraries, including the built-in libraries, from whatever folders on your PC, assembled into one big file. This means that if, in a few years, Quest's built-in libraries get radically updated, your game will not be affected.

Secondly, any supporting files. This is any file Quest can find in your game folder with a certain name format, whether they are used in your game or not. Images and sounds that are not in this folder will not be included, images and sounds that are in it, but not used will be included. Note that when you select images and sounds through the Quest GUI, it will copy the file into the game folder, so in theory all these files should already be there.

Quest grabs any file with a name that matches one of these formats

    *.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg;*.ogg;*.ogv

However, you can modify that by changing `game.publishfileextensions`; despite the name, it is not restricted to file extensions. If you have a text file you want included, but others you do not, you could set it like this:

```quest
*.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg;*.ogg;*.ogv;includeme.txt
```

The single code file plus all the supporting files are then compressed in a single archive file.


## Size limitations

textadventures.co.uk has a 50 Mb upload limit. This is the size of the published `.quest` file, and if your game is larger than that, the editor will give you a warning when you try to publish. In terms of game, that is a huge amount, and you will be doing well to build a game that is even 1 Mb. However, images, videos and sounds can seriously inflate the file size.

If your game is too large, you can try:

* Remove files that are not used from the game folder

* Use smaller or lower quality clips

* Host larger video/image/sound files on another web site

* Host your game yourself instead - see [Hosting your game](/publishing/hosting) for several options, including one that's just a single file to upload


## Announcing your game

Once your game is live, tell people about it! You can post on:

-   the [textadventures Discord](https://textadventures.co.uk/community/discord) in the `#games` channel
-   the [intfiction.org forums](https://intfiction.org/c/playing/project-announcements/50) in Project Announcements
-   [IFDB](https://ifdb.org/)


## Spell checking

Your browser's built-in spell-checker will generally underline mistakes as you type into the editor's text fields, as long as you're using a browser that supports it.

Another technique is to open the source code in a text editor that has a spell-checker, such as _Notepad++_ (which can be downloaded for free). The source code can look intimidating, and you need to be careful only to correct text that will be seen, not code or XML. With Notepad++ you can set the language to XML, which will help.

Before doing this, it is best to save and close the game in the editor first, and to create a back-up of your file.


## Beta-testing

Beta-testing is getting other people to play your game so bugs and typos can be identified and corrected before release to the public. It is absolutely vital; with the best will in the world, testers are sure to find spelling mistakes, objects you have not implemented, verbs you have not thought of, and routes through the game you have not considered. Better these things are found during beta-testing than after release. If you do not know anyone who can do this for you, it is worth asking on the forum.

### Before beta-testing

It is tempting to get the game to testers fast, but you are really just wasting their time and yours if you know there are problems before sending it. So:

1. Play the game through and correct any mistakes you can find.
2. Spell check it - see [Spell checking](#spell-checking) above.
3. Some things you might want to check, depending on your game: every room and object has an alias and a description; everything mentioned in a description is actually implemented; the appropriate display and inventory verbs are there, and inappropriate ones are absent.
4. Play the game, then try to save it. When Quest saves it does some extra error checking it does not do any other time, so this is a quick test of your code - do not send testers a game that will not save. If it saved successfully, load it again and check the UI still looks the same and the various parts still work, since loading tends to be especially sensitive to errors in scripts.

### Running beta-testing

You can upload a game to Text Adventures in the normal way for beta-testing, but keep its visibility to private. There is an "Upload a new file" link on the _Edit_ page, so you can publish updates during the testing process by downloading a fresh `.quest` package and uploading it there.

You should assume you will be releasing a few beta versions, each improving on the previous, and it may be a good idea to get new testers at each round.

When it is ready for release, go to "View/Edit Game Listing", and change the visibility to public. Remember to thank your beta-testers - it's common to do this with an "about" command in the game itself.