---
title: Hosting your game
description: Put your game on your own website, on itch.io, or anywhere else that serves files - and what each option needs from your host
---

Once you have created a Quest Viva game, you'll want to let other people play it. You have various options for hosting it.

If you are not sure which to pick, [Publishing your game](/publishing/publishing) has a decision table covering all of them.

## Upload to textadventures.co.uk

The simplest option is to upload your published `.quest` file to [textadventures.co.uk](https://textadventures.co.uk). You'll get a shareable link which you can send to anyone, which will let them play the game in their web browser.

## Publish as HTML

The simplest way to host a game yourself is to let the editor bundle it with the Quest player. From the editor's **File** menu, choose **Publish…**. Besides the `.quest` file, the dialog offers two HTML options:

- **Zip with player included** (~16 MB download, ~40 MB unzipped) - the full player plus your game (in `index.html`). Extract and upload the folder to any static web host. Nothing is fetched from anywhere else.
- **Small HTML file** - one small `.html` with your game embedded. Visitors load the player from a CDN when they play.
  - The CDN ([jsDelivr](https://www.jsdelivr.com/)) has to be reachable for your audience. If jsDelivr is blocked for them, the same package is also on [unpkg](https://unpkg.com/): open the exported `.html`, find the `<base href="https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@...">` line, and change it to the same path under `https://unpkg.com/@textadventures/quest-viva-wasmplayer@...`.
  - The small HTML file pins the exact Quest Viva version you exported with, rather than always using the latest. Re-export from the editor if you want to pick up a newer player release.

Either option will work on any web host, such as Netlify - you can try [Netlify Drop](https://app.netlify.com/drop) even without logging in (though you will need to log in if you want your site to stick around). Neither works if you open the file by double-clicking it on disk - it has to be on a website (or any simple local web host). You'll get a clear on-screen message if you try. For true offline play, see the [desktop app](/download/).

### Upload to itch.io

[itch.io](https://itch.io/) hosts browser-playable games, and the zip export is exactly what it expects.

- Publish a **Zip with player included** - itch.io needs `index.html` at the top level of the zip, which is where the export puts it
- Create a project, set **Kind of project** to HTML, and upload the zip, marking it as playable in the browser
- itch.io limits an uploaded zip to 500 MB of extracted content, 200 MB for any one file, and 1,000 files - the player accounts for about 40 MB and a hundred or so files, comfortably inside all three, so only your own images, audio and video are likely to get near them

The small HTML file works on itch.io too, but the zip is the better choice: a game that carries its own player does not stop working if the CDN becomes unreachable.

## Upload a .quest file to your own website

If you'd rather not host the player, you can upload just your `.quest` file to your own website and let the Quest Viva player site run it.

Once you've got a public URL to your `.quest` file, go to the [Quest Viva Player site](https://play.questviva.com/player/) and put it in the "Load from URL" box.

If that works, the game will load, and the browser address bar will change to a shareable link - `https://play.questviva.com/player/?url=<your game url>`.

Note that you will need to configure your website's CORS headers to allow `https://play.questviva.com` to access your URL. If you can't change them, publish as HTML instead - that needs no special headers.

## Host WasmPlayer yourself

The zip export above is the packaged version of this. Do it by hand if you want to serve several games from one copy of the player, or to customise the player's configuration.

- Download the latest `WasmPlayer.zip` file from the [Quest Viva Releases page](https://github.com/textadventures/quest/releases)
- Extract to a folder
- Add your `.quest` file to that folder
- Edit `quest-config.js` to point `defaultGameUrl` to your `.quest` file
- Upload the entire folder to your web host

## Host WebPlayer yourself

This option requires a bit more setup, and is only recommended if you require that end users don't download your `.quest` file. For example, some people have used this option for running online treasure hunts - the `.quest` file stays on the server, so it can't be examined. See the separate [WebPlayer](/publishing/webplayer/) guide.
