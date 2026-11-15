---
title: Your game's details
sidebar:
  order: 1
---

The _Setup_ tab of the game object holds everything that describes your game rather than being part of it: its title, who wrote it, what it looks like in a list of games.

Most of it can be left until you are ready to publish, but the game name and author are worth setting early — they are what the player sees before anything else, and what your saved game files are identified by.

## Title and author

**Game name** is the title. **Subtitle** is optional, and shown smaller underneath it.

**Author** is you, however you want to be credited.

**Display title and author when the game begins** prints all three, centred, before the first room description. Untick it if your game opens with something you would rather the player saw first — a quotation, a prologue, an image. Nothing is lost by turning it off; the title still appears everywhere else the game is listed.

## Version and Game ID

**Version** is a free-text label — "1.0", "2026-03-14", "Director's cut". It is for people, so use whatever scheme you like.

**Version code** is a number, for machines. Increase it every time you release an update, so that two builds can always be told apart in the right order even when their version labels cannot be compared. Both are shown together when the player types `VERSION`, and at the head of a [transcript](/howto/testing/transcripts).

**Game ID** uniquely identifies your game, and is generated for you when the game is created. It is also your game's IFID - the ID that interactive fiction catalogues and tools use to tell games apart, as defined by the [Treaty of Babel](https://babel.ifarchive.org/babel.html) - and it is shown by the `VERSION` command. You should never normally change it. The one time you should is if you made this game by copying an existing one: both would otherwise claim the same identity, and a player's saved games could end up attached to the wrong game. Generate a new ID in that case, and only that case.

## Category, year and cover art

**Category** describes the kind of game — Fantasy, Horror, Puzzle and so on. Pick from the list, or type your own. It is used to file your game when it is published.

**Year of release** is the year the game first came out. Leave it as it is when you publish an update; it records the original release, not the latest one.

Gamebooks also have a **Language** dropdown here, recording the language your game is written in. It is catalogue information — it does not change the language Quest Viva itself uses, which comes from the language library the game includes.

**Cover art** is the image shown alongside your game in a catalogue. The recommended format is a 512×512 PNG. Since it will often be shown small, a cover with one clear image and a legible title works far better than a detailed illustration.

**Description** is the blurb: a paragraph or two telling a potential player what your game is and why they might want to play it. It is shown on the game's listing page, not in the game itself, so write it for someone deciding whether to start rather than for someone already playing.

## Where these are used

Everything on this tab travels with the published game file, and a catalogue that lists your game reads its title, author, category, cover art and description from there.

When you [publish as HTML](/publishing/hosting#publish-as-html), the page gets your game's title and description too, so a link to it shared on Discord, a forum or social media shows a preview card with your game's name and blurb rather than Quest Viva's. The **Zip with player included** export also puts your cover art beside the page, for the preview to show.

The _Setup_ tab is only the description of the game, though — getting it in front of players is a separate step. See [Publishing your game](/publishing).
