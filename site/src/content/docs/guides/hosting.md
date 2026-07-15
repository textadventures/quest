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

## Host WebPlayer yourself

This option requires a bit more setup, and is only recommended if you require that end users don't download your `.quest` file. For example, some people have used this option for running online treasure hunts - the `.quest` file stays on the server, so it can't be examined. See the separate [WebPlayer](/guides/webplayer/) guide.