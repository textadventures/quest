---
layout: index
title: Transcripts
---

As of Quest 5.8 there is a fully functional transcript feature. A transcript is a recording of everything the player types and the game prints, and can be very useful when beta-testing, for example.

To turn the transcript on, use any of these commands during play.

SCRIPT
TRANSCRIPT
SCRIPT ON
TRANSCRIPT ON
ENABLE SCRIPT
ENABLE TRANSCRIPT

If it is already enabled, Quest will print, "The transcript is already enabled."

To stop the transcript:

SCRIPT OFF
TRANSCRIPT OFF
DISABLE SCRIPT
DISABLE TRANSCRIPT

You can view the transcript at any time, using one of these comnmands. The transcript will appear in a pop-up window. You can print the trascript too.

SHOW SCRIPT
VIEW SCRIPT
SHOW TRANSCRIPT
VIEW TRANSCRIPT

If using the desktop player (as of the upcoming 5.8.0 release), transcripts will be saved to the directory "Documents\Quest Transcripts". This file will be in the HTML format. When double-clicked, the transcript should open in the user's default browser.

CSS is added to align everything to the left. The background is set to white and the color to black. Be aware that the transcript may have a few strange-looking areas, depending on what all HTML and CSS code you have in a game (we are fixing every issue we come across, but it is hard to foresee what code a Quest game may include!).

Note that during play you can type a * and then some text, and Quest will ignore it. This is useful for when you want to comment on something, such as a bug you have found. The comment will appear in the transcript (and can be searched for as it starts with a *), but you will not confuse the game with your weird command.