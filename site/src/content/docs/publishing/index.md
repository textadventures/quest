---
title: Publishing your game
description: The three things the Publish dialog can make, and which one to choose for textadventures.co.uk, your own website, itch.io or a competition
---

When your game is ready for other people, the editor packages it up for you. Open the **File** menu in the toolbar and choose **Publish…**.

The dialog offers three things it can make. All three contain the same game - what differs is who hosts the player that runs it, and therefore where you can put the result.

| You want | Choose | What you get |
|---|---|---|
| Your game listed on textadventures.co.uk | **.quest file** | One file to submit to the site |
| Your game on your own website | **Zip with player included** | A folder to upload: your game plus the Quest player |
| Your game on itch.io | **Zip with player included** | The same folder, uploaded as a browser-playable project |
| The smallest possible upload | **Small HTML file** | A single `.html` page; players fetch the Quest player from a CDN |
| A competition entry | **Zip with player included** | An entry judges can play with nothing installed - see [Competition entry](/publishing/competition-entry) |

If you are editing a game that is stored on textadventures.co.uk, the first option reads **Publish to textadventures.co.uk** instead. It uploads the game for you and then takes you to the site to fill in the description, category and who can see it, so you never handle the file yourself.

:::note[Publishing is not backing up]
What the Publish dialog makes is a build for players, not your source: the editor does not open a `.quest` file or an HTML export. If your game is a local draft in the browser, keep a copy of the source with **Backup…** in the **File** menu, which downloads a `.zip` of the game file and its assets that you can import again later. If your game lives in a folder, it is already on your disk.
:::

## .quest file

A `.quest` file is your whole game in one file: the game itself, every picture and sound it uses, and the library code it was built with. It is what textadventures.co.uk accepts, what the [Quest Viva player](https://play.questviva.com/player/) and the desktop app open, and the format to use if you want to hand the game to someone rather than host a web page.

It is not a web page, so it needs a player to run it. That is either a site that provides one (textadventures.co.uk, play.questviva.com) or one you host yourself - see [Hosting your game](/publishing/hosting).

## Small HTML file

One `.html` file containing your game, which loads the Quest player from a CDN when somebody plays it. It is the smallest upload by far, which makes it a good fit for a personal site, a shared-hosting account, or anywhere you would rather not upload tens of megabytes.

The trade-off is that the CDN has to be reachable for your players. The file also pins the exact Quest Viva version you exported with, so re-export if you want a newer player.

## Zip with player included

Your game plus a complete copy of the Quest player - about 16 MB to download and 40 MB unzipped. Unzip it and upload the folder to any static web host, and `index.html` is your game. Nothing is fetched from anywhere else, so it keeps working whatever happens to a CDN, and it is the right choice for a competition entry or anywhere the game has to be self-contained.

Neither HTML option works if you open the file by double-clicking it on disk - it has to be served by a website, or any simple local web server. You will get a clear on-screen message if you try. For genuine offline play, use the [desktop app](/download/).

Both HTML exports carry your game's IFID in a `<meta property="ifiction:ifid">` tag, as the [Treaty of Babel](https://babel.ifarchive.org/babel.html) asks for, so catalogues and tools can identify the game without unpacking it.

## The publish process

What goes into a published game? Broadly two things.

First, the game code: your game plus all the library code it uses, including the built-in libraries, assembled into one file. This is why an old game keeps working - if Quest Viva's built-in libraries change in a few years' time, your published game is unaffected, because it carries its own copy.

Second, your files. Every asset the editor knows about is included, whether your game uses it or not, along with a `metadata.iFiction` record of the title, author, category, description and cover art from the [_Setup_ tab](/publishing/game-details).

The **Publish** dialog tells you how many files it is including and their total size. **Show files** lists them, largest first. Where the files come from depends on how your game is stored:

- **A folder** (the desktop app, or "Save to folder…" in the browser): every file in that folder is included, and the dialog names it. If your game file sits in a folder with lots of unrelated things in it, such as your Downloads folder, move the game into a folder of its own.
- **A local draft** (stored in the browser): there is no folder to tidy. The files are exactly the ones listed under **Manage assets…**, which is also where you delete the ones you have stopped using.

## Size limitations

textadventures.co.uk has a 50 MB upload limit, and the **Publish** dialog warns you if your game is over it. As text that is an enormous amount - you would do well to write a game that reaches 1 MB - but images, audio and video add up fast. The HTML options have no hard limit, but you get the same warning, because everything in the file is downloaded before the player can start.

If your game is too large:

* Remove files your game does not use - **Show files** puts the biggest first, and that is usually where the problem is
* Use shorter clips, or lower quality ones
* Host large video, image or sound files on another web site
* Host the game yourself instead, where there is no limit - see [Hosting your game](/publishing/hosting)

## Before you release

It is tempting to release the moment the game works. Players will not thank you for it. Before you show it to anybody:

1. Play it through and fix everything you find.
2. Spell check it - see [Spell checking](#spell-checking) below.
3. Check that every room and object has an alias and a description, and that everything you mention in a description actually exists as an object, even if only as scenery.
4. Think about what a player might reasonably type and try: alternative names for objects, and verbs that at least say "you can't do that" rather than giving a default response.
5. Save the game. Quest Viva does extra error checking when it saves that it does not do at any other time, so this is a quick test of your code. Then load it again and check everything still works - loading is especially sensitive to errors in scripts.

And however tempting it is, do not release the game you built while working through the tutorial.

## Spell checking

Your browser's built-in spell checker will underline mistakes as you type into the editor's text fields, as long as you are using a browser that supports it.

You can also open the source code in a code editor that has a spell checker. Save and close the game in the editor first, back up the file, and set the code editor's language to XML - the source looks intimidating, and you need to correct only text the player will see, not code.

## Beta-testing

Beta-testing is getting other people to play your game so that bugs and typos can be found before release. It is vital: testers find spelling mistakes, objects you have not implemented, verbs you never thought of, and routes through the game you did not consider. If you do not know anyone who can do this, ask on the [textadventures Discord](https://textadventures.co.uk/community/discord) or the [intfiction.org forums](https://intfiction.org/c/authoring/8).

Work through [Before you release](#before-you-release) first - sending testers a game with problems you already knew about wastes their time and yours.

Then upload the game in the normal way, but keep it unlisted, with "Who can access this game?" set to "Only people I give the link to". To get a new build to testers, publish a fresh `.quest` file and use "Upload an updated game file" in the _Edit_ menu on your game's page. If you are hosting the game yourself instead, an unadvertised URL does the same job.

Assume several rounds of beta versions, each better than the last, and consider finding new testers for each. When it is ready, choose "Edit this listing" from the same _Edit_ menu and set who can access the game to "Everybody". Remember to thank your beta-testers - usually in an "about" command inside the game itself.

## Publishing on textadventures.co.uk

If the **Publish to textadventures.co.uk** option is not offered, publish a `.quest` file and upload it by hand: on textadventures.co.uk, click _Create_ at the top, then _Submit_ below that, and follow the instructions.

Either way the site asks "Who can access this game?". Choose **"Only people I give the link to"** while you are testing - that makes the game unlisted, so only people you send the link to can find it - and **"Everybody"** when you are ready to release. There is no separate "private" setting to worry about: unlisted is what keeps a work in progress out of public view while your testers can still reach it.

Your game then goes into a queue for moderation, which can take a few days. Very basic games are filed under "Sandpit", games with sexual content under "Adult", and everything else in the category you chose.

To release a new version later, publish a fresh `.quest` file and use "Upload an updated game file" in the _Edit_ menu on your game's page. Read [Updating a released game](/publishing/updating-a-released-game) first - it explains why players part-way through carry on playing the old version, and what that means for how you fix things.

## Announcing your game

Once it is live, tell people about it:

- the [textadventures Discord](https://textadventures.co.uk/community/discord), in the `#games` channel
- the [intfiction.org forums](https://intfiction.org/c/playing/project-announcements/50), in Project Announcements
- [IFDB](https://ifdb.org/), where your game's IFID identifies it

## See also

- [Your game's details](/publishing/game-details) - the title, author, cover art and IFID that travel with a published game
- [Hosting your game](/publishing/hosting) - where to put an HTML export, and other hosting options
- [Updating a released game](/publishing/updating-a-released-game) - releasing a new version without breaking players' saves
- [Competition entry](/publishing/competition-entry) - entering IFComp and other competitions
