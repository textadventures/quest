---
title: Pictures, sound and video
sidebar:
  order: 3
---

A page doesn't have to be only text. The **Page type** drop-down at the top of the _Page_ tab is where most of this lives, and the _Action_ tab handles sound.

| Page type | What it does |
|---|---|
| **Text** | A paragraph and its links. The default. |
| **Picture** | The same, with a picture above the text. |
| **YouTube** | The same, with a video above the text. |
| **External link** | Sends the reader to another website instead of showing a page. |
| **Script** | Runs a script and shows nothing of its own. |
| **Script + Text** | Runs a script, then shows the text and links as usual. |

The last two are what [Keeping track](/tutorial/gamebook/keeping-track) is built on.

## A picture

Set a page's type to **Picture** and a "Picture" box appears above the description, with an **Upload…** button beside it. Click it, choose a file, and the picture is added to your game and to that page.

The box also drops down a list of every picture already in the game, so you only ever upload each one once - handy when the same establishing shot opens several branches.

Try it on The Lighthouse's `Start` page: find or make an image of a lighthouse at night, set the page type to Picture, and upload it.

JPEG suits photographs; PNG suits everything else, because it compresses without losing quality and can have transparent parts. Keep an eye on file sizes - every file in your game is packaged when you publish, and pictures are usually what makes a gamebook large. **Manage assets** on the toolbar lists every file in the game and lets you delete the ones you have stopped using.

## A video

Set the page type to **YouTube** and you get a "YouTube id" box. The id is the short code at the end of a YouTube share link: click Share under the video, and in a URL like `https://youtu.be/8jPyg2pK11M` the id is `8jPyg2pK11M`. Paste just that part - not the whole address.

The video appears above the text, and the reader can play it and then carry on down the page.

## Sound

Sound is on the **_Action_** tab, which applies to any page type. There is one box, "Play sound when the player enters this page", with its own **Upload…** button.

By default a sound stops when the reader turns the page. Tick "Continue to play sound on next page" if it should carry on - which is how you get a storm, or a piece of music, running across a run of pages rather than restarting at each one.

For The Lighthouse, a loop of rain and sea on the `Start` page with "continue" ticked, and something quieter once the reader is inside, does most of the work of setting a scene.

Two things worth knowing. Browsers will not play sound until the reader has clicked something, so a sound on the very first page may not play until they choose an option - put the sound on the second page if it matters. And a reader may well be somewhere they can't have sound on at all, so never make a sound the only way to learn something.

## Sending the reader elsewhere

**External link** is a page that isn't really a page: it takes the reader straight to a web address, and shows nothing of its own. It is there for a link to your own site or to something the story refers to.

Use it sparingly, and never as an ending - a reader who clicks it has left your game, and the back button is their only way home.

## Styling text

You don't need a special page type for bold and italic. The description box has a toolbar with **B**, *I* and underline buttons, which wrap whatever you have selected, and an **Insert** menu with the rest - including **Once** for text that only appears the first time a page is seen, and **Random text** for a line that varies.

Those come from the [text processor](/howto/text/text-processor), which runs on everything your game prints. The next chapter uses it in earnest.

[Next: Keeping track](/tutorial/gamebook/keeping-track)
