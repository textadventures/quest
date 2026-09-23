---
title: Publishing your gamebook
sidebar:
  order: 5
---

The Lighthouse works. Before anyone else reads it, there are two things worth doing: making it look like yours, and checking it.

## The look of it

Select "game" at the top of the tree and open the _Display_ tab. Everything here applies to the whole game.

**Fonts.** "Base font" is a list of the fonts every device has; "Web font" is a much longer list that the player loads as it needs them. A web font wins on character and loses on speed, and either way the thing that matters is that it stays readable in a long paragraph. Set the size a little larger than you think you need - people read gamebooks on phones.

**Colours.** Background, foreground and link colour. Pick the link colour deliberately: it is the only signal the reader gets about what they can click, so it needs to stand out clearly against the text. Any box takes a colour name or a hex value beginning with `#`.

**Clear the screen between pages** decides whether each page replaces the last one or adds to a scroll. Clearing gives you a book, one page at a time. Not clearing gives you a transcript the reader can scroll back through, which is kinder when a page refers to something a few choices ago.

**Layout** - the border, a custom width, and padding. A custom width is worth setting: full-screen text on a wide monitor is a very long line to read. Somewhere around 700 pixels is comfortable.

Set the cover art and description on the _Setup_ tab while you are there. They are what people see before they have read a word - the description is the blurb, so write it like one.

## Checking it

A gamebook's failure mode is a branch nobody ever walks. Before you publish:

- **Click every option, on every page.** There is no substitute. Work through the tree from the top and tick pages off.
- **Look for pages nothing links to.** A page you renamed or replaced can end up orphaned, and the reader will never see it.
- **Check every ending reads like one.** A page with no options simply stops, so an ending that trails off looks like a bug.
- **Read it on a phone.** Most people will.
- **Check your conditional text both ways.** Every `{if}` needs walking through with the flag set and without it, which is exactly the sort of thing that is easy to convince yourself you have done.

Then hand it to somebody else without telling them anything, and watch. You will learn more in ten minutes of that than in an hour of re-reading it yourself.

## Publishing

From here it is the same as for any Quest Viva game: [Publishing your game](/publishing) covers what the **Publish** dialog produces - a `.quest` file for textadventures.co.uk, or an HTML version you can host anywhere - along with game details, competitions and how to update a game after release.

One thing specific to gamebooks: pictures and sound are what make the file big, and textadventures.co.uk has a 50 MB limit. The Publish dialog tells you how many files it is including and how large they are, and **Manage assets** on the toolbar lets you delete anything you stopped using along the way.

## What next?

You now know everything the gamebook editor does. If you want more than it offers - a world the reader moves around in, objects they carry, commands they type - that is a text adventure, and [the main tutorial](/tutorial/tutorial-introduction) starts from the beginning. The two share the same scripting language, the same text processor, and the same publishing, so none of this is wasted.

If you get stuck, ask on [Discord](https://textadventures.co.uk/community/discord) or in [GitHub Discussions](https://github.com/textadventures/quest/discussions).
