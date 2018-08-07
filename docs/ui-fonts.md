---
layout: index
title: Fonts
---

## Fonts

There are about a dozen "base fonts" available in Quest. These are fonts that are pretty much guaranteed to be available on any computer (or at least equivalents, so we have Arial on PC, or Helvetica on Mac or failing that sans-serif).

If you want to change the font during a game, use the `SetFontName` function. This allows you to list the equivalent fonts, so will ensure users on other operating systems see more-or-less the same thing.

```
  SetFontName("Arial, Heletica, sans-serif")
  msg("This is in Heletica")
  SetFontName("'Courier New', Courier, monospace")
  msg("This is in Courier")
  SetFontName("Impact, Charcoal, sans-serif")
  msg("This is in Charcoal")
```

The sans-serif and monospace are generic fonts; there are also serif, cursive and fantasy. They will all map to something on every computer, though the cursive and fantasy tend to fall well short of the names.

You also have access to web fonts. These are provided on-line by Google, and by default you can access just one in your game. To use any more, you need to call the `SetWebFontName` to pull the font off the internet, and then `SetFontName` as normal to actually use it.

```
  // Pull the fonts off the internet
  SetWebFontName("Wallpoet")
  SetWebFontName("Admina")

  // Now we can swap between them as much as we like
  SetFontName("Wallpoet")
  msg("This is in Wallpoet")
  SetFontName("Admina")
  msg("This is in Admina")
  SetFontName("Wallpoet")
  msg("This is in Wallpoet")
```

Make sure you choose a font that is easy to read for the main text!