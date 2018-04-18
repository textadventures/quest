---
layout: index
title: Images in Quest
---

A picture paints a thousand words, or so they say. So how do you add images to your game?


About Image Files
------------

For photos, the best file format to use is JPG, however, for anything else, you should use PNG. PNG uses lossless compression, and it supports transparency, so you can make the image appear to be any shape you want.

GIMP is very useful for editing images and is free. Inkscape is  good vector-based software for creating images.

Bear in mind that there is a 20 Mb limit on games at textadventures.co.uk, and images, videos and sounds can quickly get you to that limit. Compressing images can help a lot.


Adding Images - The picture command
-------------------

Quest has a built-in picture command, which will show an image aligned to the left. 
```
  picture ("gravestone.png")
  msg ("You are in a room. A large room, entirely white, nothing here but a gravestone.")
  msg ("Your gravestone.")
```

Sometimes you want to change how it is displayed, for example put the image in the middle, or have text flow round it. The picture command gives no control over the image, so we will need to output the raw HTML to do anything more. Sounds scary? You bet!


Using HTML
----------

HTML is the way web pages on the internet tell your browser how they should be displayed. It is a set of codes embedded in the text, and, for example `<i>` will start italics, whilst `</i>` stops it.

The HTML code for an image will look kind of like this; "img" is the tag for image, and "src" is an attribute that indicates the source of the image (traditionally HTML uses double quotes, but single quotes work too, and are easier to use with Quest):
```
  <img src='gravestone.png' />
```
If only it was that simple. That will work offline, but online, you need a full URL for the file to be found. Fortunately Quest has a function built-in to do that for you, and it will work whether your game in online or offline. This means you need to add together three strings:
```
  "<img src='"
  GetFileURL("gravestone.png")
  "' />"
```
The code ends up looking like this.
```
  msg ("<img src='" + GetFileURL("gravestone.png") + "' />")
  msg ("You are in a room. A large room, entirely white, nothing here but a gravestone.")
  msg ("Your gravestone.")
```

Looks just the same so far, but this gives us a handle on changing how it looks, via the style attribute and CSS. 


Floating Images
---------------

Let us make the image float! When the image floats, the text will flow around it. The HTML will look kind of like this:
```
  <img src='gravestone.png' style='float:left;'/>
```
The "style" attribute tells Quest what style you want this thing to be. You need to be pretty specific in the values. The general format is the name of the CSS attribute ("float" in this case) followed by a colon, then the value ("left"), followed by a semi-colon. Just to help the confusion, we have CSS attributes inside of HTML attributes!
```
  msg ("<img src='" + GetFileURL("gravestone.png") + "' style='float:left;' />")
  msg ("You are in a room. A large room, entirely white, nothing here but a gravestone.")
  msg ("Your gravestone.")
```

You can just as easily have the image on the right. Let us add another CSS attribute. The "padding" attribute controls the spacing around the image. You need to specify "px" (pixels) as the units in this case.
```
  msg ("<img src='" + GetFileURL("gravestone.png") + "' style='float:right; padding:15px;' />")
  msg ("You are in a room. A large room, entirely white, nothing here but a gravestone.")
  msg ("Your gravestone.")
```

The CSS to centre an image is rather more complicated than you would imagine:
```
  msg ("<img src='" + GetFileURL("gravestone.png") + "' style='display: block; margin-left: auto; margin-right: auto;' />")
```

Other Effects
-------------

There are all sorts of attributes you can mess around with. Here the image is transparent.
```
msg ("<img src='" + GetFileURL("gravestone.png") + "' style='float:left;opacity:0.5;' />")
```

You can resize it. Changing just the width or height changes the image proportionally, or you can set both.
```
  msg ("<img src='" + GetFileURL("gravestone.png") + "' style='float:left;width:100px;' />")
```

One Image On Top of Another
---------------------------

You can even superimpose one image over another if you feel brave enough. You need to put them both inside an HTML div (this is then the reference point that the images are positioned against), and give the images an absolute position. All that needs to go inside a single "msg" as Quest will add its own HTML, so in this example, a string, `s`, is used, with each bit added to it (we could do it in one line, but it would be very long).
```
  s = "<div style='position:fixed; left: 0px; bottom: 20px; width: 260 px; height: 670px;'>"
  s = s + "<img src='" + GetFileURL("gravestone.png") + "' style=\"position:absolute;top:0px;left:-200px;' />"
  s = s + "<img src='" + GetFileURL("celebrate.png") + "' style=\"position:absolute;' />"
  s = s + "</div>"
  msg (s)
```

Using this sort of positioning puts the images outside the normal flow of elements on the page, and getting the text to go around the images would be very difficult. I have dodged that by putting the images outside the text altogether.

What you can then do is have the images appear in response to how the game progresses.

### A Map

You could, for example, have a series of images where each one is a section of a map, and the map becomes revealed as the player explores. Obviously the rest of the image must be transparent, and each room has to be positioned correctly in the image.

Let us suppose 12 images, called "map1.png" to "map12.png". Give each room on the map a "mapnumber" attribute assigning it to one of the 12 map images. 

You could add this to your start script to add each map to the game (the first two lines move others bits to make space for them).

```
JS.setCss ("#gameBorder", "margin-left:600px;margin-right:10px")
JS.setCss ("#gamePanes", "margin-left:400px")
s = "<div id='imageouter' style='position:fixed; left: 0px; bottom: 20px; width: 260 px; height: 670px;'>"
for (i, 1, 12) {
  s = s + "<img id='plan" + i + "' src='" + GetFileURL("plan" + i + ".png") + "' style='position:absolute;' />"
}
s = s + "</div>"
JS.addScript (s)
```

Then have this script run when the player enters a room (_Scripts_ tab of the game object):

```
for (i, 1, 12) {
  JS.eval ("$('#plan" + i + "').css('visibility', 'hidden')")
}
foreach (o, AllObjects()) {
  if (HasInt(o, "number") and o.visited) {
    JS.setCss ("#plan" + o.number, "visibility: visible;opacity:0.5")
  }
}
if (HasInt(game.pov.parent, "number")) {
  JS.setCss ("#plan" + game.pov.parent.number, "visibility: visible;opacity:1.0")
}
```

The first three lines set all the maps to hidden. The next five lines set the maps for any room visited to be visible, but transparent, so it seems greyed out. The last three lines show the current room fully opaque.

![](images/selective-map.png "selective-map.png")

### Or You Could

Another possibility is to display a map and to have a marker on it that shows the players position, by changing the `margin-top` and `margin-left` values. Or display an image of the player, to which armour and weapons can be added. Or allow the user to design a face. Or...



Hosting images elsewhere
------------------------

One way to circumvent the size limitation is to host your images on another site, such as DeviantArt or Imgur. You can do that easily, using the technique above. In fact, it is even easier; you just add the full address, rather than having to use `GetFileURL`.

```
msg ("<img src='http://www.mydomain.com/images/kitten.png' />")
msg ("<img src='http://www.mydomain.com/images/kitten.png' style=\"display: block; margin-left: auto; margin-right: auto;\" />")
```

If you are using the web editor, this will avoid issues with how you upload your image (otherwise you would need to upload the image with the `picture` command, then swap to the `msg` command) as well as allowing you to edit and delete images, which currently is not possible.