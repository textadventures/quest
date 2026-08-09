---
title: Hosting your game
sidebar:
  order: 2
---

Once you have created a Quest game, you'll want to let other people play it. You have various options for hosting it.

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

### Publish a single file (no download)

If you don't need a fully offline copy, there's a much smaller option: a single HTML file with your game embedded in it, which loads the Quest Viva Player itself from a CDN rather than from your own hosting. Uploading one small file is all that's needed - no separate download, extraction or folder upload.

In the editor, open the **File** menu and choose **Export as single file…**. This downloads one `.html` file - upload it to any web host (Netlify, GitHub Pages, your own site, etc.) and share the link.

A couple of things to know about this option:

- **It needs the CDN to be reachable.** The generated file loads the player from [jsDelivr](https://www.jsdelivr.com/) - if that's blocked on a visitor's network, the game won't load for them. Everything else (your game, its assets) is embedded directly in the file, so only the player itself depends on the CDN. If jsDelivr doesn't work for your audience, the same package is also published to [unpkg](https://unpkg.com/): open the exported `.html` file in a text editor, find the `<base href="https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@...">` line near the top, and change the URL to the same path under `https://unpkg.com/@textadventures/quest-viva-wasmplayer@...` instead.
- **It's pinned to the Quest Viva version you exported with.** Your saved `.quest` file's own script behaviour is always preserved, however the player is updated - but the player's look and interface can still change between releases, so the export deliberately links to one exact version rather than "always the latest," to make sure it keeps looking and working the same way in the future. Re-export from the editor if you want to pick up a newer player release.
- **This still doesn't enable double-click-from-disk play.** The file needs to be served over `http://`/`https://` (any host, or the CDN itself) - opening it directly from your computer's file system won't work, the same as every other WasmPlayer option on this page. You'll get a clear on-screen message if you try. If you need true offline play, see the [desktop app](/download/) instead.

## Host WebPlayer yourself

This option requires a bit more setup, and is only recommended if you require that end users don't download your `.quest` file. For example, some people have used this option for running online treasure hunts - the `.quest` file stays on the server, so it can't be examined. See the separate [WebPlayer](/guides/webplayer/) guide.