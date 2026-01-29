---
layout: index
title: Creating Images on the Fly
---


Did you know you can create images in code? It is a technique called Scalable Vector Graphics (SVG) and is XML, just like Quest, and gets interpreted by the browser, just like Quest.

There is a tutorial on SVG here (I would advise getting familiar with XML first):
[https://www.w3schools.com/graphics/svg_intro.asp](https://www.w3schools.com/graphics/svg_intro.asp)

This is the example on the first page; it draws a green circle.

```
<svg width="100" height="100">
  <circle cx="50" cy="50" r="40" stroke="green" stroke-width="4" fill="yellow" />
</svg>
```

To convert that to Quest, just put backslashes before each double quote, and print it out!

```
  msg("<svg width=\"100\" height=\"100\"><circle cx=\"50\" cy=\"50\" r=\"40\" stroke=\"green\" stroke-width=\"4\" fill=\"yellow\" /></svg>")
```

For more complex drawings, building up a string is a good idea:

```
  // step 1, the svg element defines the drawing board
  s = "<svg width=\"200\" height=\"100\">"
  // step 2, draw a circle
  s = s + "<circle cx=\"50\" cy=\"50\" r=\"40\" stroke=\"green\" stroke-width=\"4\" fill=\"yellow\" />"
  // step 3 draw a transparent rectangle
  s = s + "<rect x=\"50\" y=\"20\" width=\"150\" height=\"150\" style=\"fill:blue;stroke:pink;stroke-width:5 fill-opacity:0.3;stroke-opacity:0.9\" />"
  // step 4 end the svg element
  s = s + "</svg>"
  // step 5 print it out
  msg (s)
```

For really complicated shapes, you might want to look at using InkScape, a drawing program that will produce output in SVG format (disclaimer: while I have used InkScape, but never tried to convert the output to Quest).

You can even capture mouse events to make the image interactive. Here is the start of a strategy game, go to the Stars & Planets section, to see a galactic map. Click on the side arrows to move and the + and - to zoom in and out.

[https://textadventures.co.uk/games/view/ohqyc_lfuuiqjsuwmvbkbw/star-empire-v0-1](https://textadventures.co.uk/games/view/ohqyc_lfuuiqjsuwmvbkbw/star-empire-v0-1)