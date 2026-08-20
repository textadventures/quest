---
title: Creating a gamebook
sidebar:
  order: 16
---

## Creating a blank game

This tutorial guides you through creating your first gamebook game. If you want to create a text adventure instead, see [the main Quest tutorial](/tutorial/creating_a_simple_game).

Open the editor - either in your browser, or the desktop app - and you'll see a "Create new game" section. Ensure that "Gamebook" is selected as the game type, and enter a name like "Tutorial Game".

Click "Create local draft" (or "Save to folder..." if you'd rather store the game file yourself), and you'll see the main Editor screen.

On the left is a tree showing you the pages in the gamebook, and a place to set options about the game itself. "Game" is currently selected, so that's what we can see in the pane on the right.

Quest has created three example pages for us, and inside Page1 is the "player" object, which is where the game begins. You can test the game by clicking the "Preview" button towards the top right.

As you'll see, it's a pretty empty game at the moment. We can navigate to pages 2 and 3, but that's it.

You can go back to the Editor by closing the preview, or typing `QUIT`.

## Editing pages

To create your game, edit the text for Page1. Underneath the text, the "Options" list shows which pages a player can get to from here. You can add new pages directly from here, or create links to other pages which already exist.

## Page types

### Text

This is the standard page type. It simply shows a paragraph of text, followed by the list of options.

### Picture

This is the same as the Text type, but you can also choose a picture to display at the top of the screen.

### YouTube

This is the same as the Text type, but you can also choose a YouTube video to display at the top of the screen. You will need the YouTube id of the video - an easy way to get this for a YouTube video is to find the video you want and click Share. The id will be displayed at the end of a URL like `https://youtu.be/8jPyg2pK11M` where `8jPyg2pK11M` is the id you want.

### External link

This is a special page type which takes the player directly to another website. It doesn't display any text of its own.

## Playing sounds

You can play a sound when a player reaches a page. Go to the Action tab and browse for a sound file.

## Releasing your game

To publish your game, follow the same steps as listed in [Releasing your game](/tutorial/releasing_your_game) in the main text adventure tutorial.
