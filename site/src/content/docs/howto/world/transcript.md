---
title: Transcripts
description: How players record a transcript of a session, add comments to it, and send it to you - and how to turn the feature off
---

A transcript is a recording of everything the player types and the game prints. It is the single most useful thing a beta-tester can send you: you see exactly what they did, in order, including the commands that didn't work.

The commands below are built into every game, so there is nothing to set up. Point your testers at this page, or paste the instructions into your game's introduction.

## Recording a transcript

To start recording, type any of these:

```
SCRIPT
TRANSCRIPT
SCRIPT ON
TRANSCRIPT ON
ENABLE SCRIPT
ENABLE TRANSCRIPT
```

The game asks for a name for the transcript - anything that will help you tell one session from another - and offers the game's title as a suggestion in the command box. Accept it or type your own, then press Enter. Recording starts with a header giving the game's title, author, version and version code, IFID, and the date and time:

```
Start of a transcript of:
TITLE: The Mansion
AUTHOR: A N Author
VERSION: 1.2 (7)
IFID: 4d1f2b70-2a6c-4f5e-9a23-6b3f1c5d2e88
DATE AND TIME: 20/09/2026, 09:00:30 GMT+1
```

Recording a second session under the same name appends to the same transcript, so give each session its own name if you want them kept apart.

If a transcript is already running, the game says "The transcript is already enabled."

To stop recording:

```
SCRIPT OFF
TRANSCRIPT OFF
DISABLE SCRIPT
DISABLE TRANSCRIPT
```

Anything the player types starting with `*` is not treated as a command. The game replies "Noted." and the line goes into the transcript, so a tester can leave notes as they play:

```
> * the hook is described as brass here but iron in the hall
* the hook is described as brass here but iron in the hall
Noted.
```

Starting every note with `*` also makes them easy to search for afterwards.

## Reading and sending a transcript

Type `VIEW TRANSCRIPT` (or `SHOW TRANSCRIPT`, or `DISPLAY TRANSCRIPT`) at any time. The game prints a link to a transcript viewer, which lists every transcript recorded for any game, with buttons to open, download or delete each one. "Download" saves it as a plain `.txt` file - that's what a tester should send you.

Where transcripts are kept depends on how the game is being played:

- **In a browser** - in that browser's local storage for the site the game is on. They survive closing the tab, but are lost if the player clears the site's browsing data, or if they were playing in a private window. Download anything worth keeping first.
- **In the [desktop app](/download/)** - as files in the app's own data folder, so clearing browser data doesn't touch them. The `VIEW TRANSCRIPT` link opens the same viewer.

Either way transcripts stay on the player's own machine. Nothing is uploaded, so a tester has to send you the downloaded file themselves.

## Turning the feature off

There is no editor control for this, but you can switch transcripts off entirely by setting a `notranscript` attribute on the `game` object to `true` - add it on the game's _Attributes_ tab, with type "Boolean". All the commands above then answer "This game has no transcript feature."

Most games should leave it on. It costs nothing, and it's what your testers will use.

## See also

- [Walkthroughs](/howto/scripting/using-walkthroughs) - replaying a fixed list of commands to check nothing has broken
- [Debugging your game](/howto/scripting/debugging-your-game) - inspecting objects and attributes while you play
