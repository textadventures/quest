---
title: Privacy
description: What Quest Viva's desktop app, web app and websites send, store and share
editUrl: false
---

Quest Viva has no accounts, no advertising and no cookies, and doesn't track you. Games you make stay on your device unless you choose to publish them to textadventures.co.uk. This page covers the Quest Viva desktop app, wherever you installed it from, the web app at play.questviva.com and play-beta.questviva.com, and this website, questviva.com and beta.questviva.com.

Quest Viva is open source, so you can check everything here against [the code on GitHub](https://github.com/textadventures/quest).

## What stays on your device

- **Games you create or open** are read from and saved to files and folders you choose. In the web app, drafts you haven't saved to a folder are kept in your browser's storage.
- **Saved games** (progress in games you play) are kept in your browser's storage, or in the desktop app's own storage, unless you save one to a file.
- **Settings**, such as the light or dark theme, are kept in the same place.

None of this is sent anywhere. Clearing your browser's site data, or uninstalling the desktop app, deletes it.

## What is sent to textadventures.co.uk

Quest Viva is part of [textadventures.co.uk](https://textadventures.co.uk/), which lists and hosts the games in the Play tab. The app talks to it in three cases.

**Browsing the Play tab.** Showing, searching and opening games in the catalog fetches them from textadventures.co.uk. Each request says which app is asking: web or desktop, its version, and for the desktop app the operating system (Windows, Mac or Linux) and processor type. These are used to count how the catalog is used, and to offer the right download when a newer version is out.

**Playing a game from textadventures.co.uk.** The player downloads the game from textadventures.co.uk and reports how far it got starting the game: when the start screen appears, when the game starts, or that it couldn't be started and the error message. Each report has the same app details as above, how long each step of loading took, the game's size, which player interface was used, and a random number made up for that one visit, so the reports from one visit can be put together. That number isn't stored and is different every time. These reports are used to find games that fail to load, and to make loading faster. Games from anywhere else, including your own, send nothing.

**Publishing a game to textadventures.co.uk.** If you choose textadventures.co.uk in the editor's Publish window, the game's `.quest` file is uploaded there, along with its file name and the app details above. You then finish publishing on textadventures.co.uk, signed in to your account there. The other ways to publish, such as saving a `.quest` file or a web page, make the file on your device and upload nothing.

What textadventures.co.uk does with information it holds, such as your account there, is covered by its own [privacy policy](https://textadventures.co.uk/privacy).

## Other sites games can use

A game can use content from other websites. A game that uses a Google font loads it from Google Fonts, and a game's author can include pictures, sounds or other content from any site. Those sites receive your IP address when the content loads, as they would for any web page. Some pages of this website include YouTube videos, which use youtube-nocookie.com and set no cookies until you play the video.

## Hosting

questviva.com and play.questviva.com are hosted by Cloudflare, and textadventures.co.uk serves the catalog and games. Like any website, they receive your IP address and browser details with each request, and may keep them in their logs for a short time to run the service and protect it from abuse.

This documentation website counts its visits with [Cloudflare Web Analytics](https://www.cloudflare.com/web-analytics/), which uses no cookies and doesn't identify you or follow you to other sites. It shows us totals such as which pages are read most. The web app and the desktop app don't use it.

## Questions

If you have a question about privacy in Quest Viva, ask on [GitHub](https://github.com/textadventures/quest/issues).
