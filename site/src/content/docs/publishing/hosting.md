---
title: Hosting your game
sidebar:
  order: 2
---

Once you have created a Quest Viva game, you'll want to let other people play it. You have various options for hosting it.

## Upload to textadventures.co.uk

The simplest option is to upload your published `.quest` file to [textadventures.co.uk](https://textadventures.co.uk). You'll get a shareable link which you can send to anyone, which will let them play the game in their web browser.

## Upload to your own website

If you want a bit more control, or you want to upload a type of game that is not accepted on textadventures.co.uk, you can upload your `.quest` file to your own website.

Once you've got a public URL to your `.quest` file, go to the [Quest Viva Player site](https://play.questviva.com/player/) and put it in the "Load from URL" box.

If that works, the game will load, and the browser address bar will change to a shareable link - `https://play.questviva.com/player/?url=<your game url>`.

Note that you will need to configure your website's CORS headers to allow `https://play.questviva.com` to access your URL.

## Host WasmPlayer yourself

For even more control, you can host the Quest Viva Player on your own site too. If you can't change your website's CORS headers, this option should still work because it doesn't require any custom headers to be set.

This option should work on any web host, such as Netlify - you can try [Netlify Drop](https://app.netlify.com/drop) even without logging in (though you will need to log in if you want your site to stick around).

- Download the latest `WasmPlayer.zip` file from the [Quest Viva Releases page](https://github.com/textadventures/quest/releases)
- Extract to a folder
- Add your `.quest` file to that folder
- Edit `quest-config.js` to point `defaultGameUrl` to your `.quest` file
- Upload the entire folder to your web host

### Export as HTML

From the editor's **File** menu, choose **Export as HTML…**. Playing needs the Quest player; the dialog asks how you want to handle that:

- **Small HTML file** — one small `.html` with your game embedded. Visitors load the player from a CDN when they play.
  - The CDN ([jsDelivr](https://www.jsdelivr.com/)) has to be reachable for the "small HTML file" option to load. If jsDeliver is blocked for your audience, the same package is also on [unpkg](https://unpkg.com/): open the exported `.html`, find the `<base href="https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@...">` line, and change it to the same path under `https://unpkg.com/@textadventures/quest-viva-wasmplayer@...`.
  - The small HTML file pins the exact Quest Viva version you exported with, rather than always using the latest. Re-export from the editor if you want to pick up a newer player release.
- **Zip with player included** (~16 MB download, ~40 MB unzipped) — the full player plus your game (in `index.html`). Extract and upload the folder to any static web host. No CDN is required when using the zip.

Neither option works if you open the file by double-clicking it on disk — it has to be on a website (or any simple local web host). You'll get a clear on-screen message if you try. For true offline play, see the [desktop app](/download/).

## Host WebPlayer yourself

This option requires a bit more setup, and is only recommended if you require that end users don't download your `.quest` file. For example, some people have used this option for running online treasure hunts - the `.quest` file stays on the server, so it can't be examined. See the separate [WebPlayer](/publishing/webplayer/) guide.