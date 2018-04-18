---
layout: index
title: Docs Style Guide
---


This gives some general rules for anyone writing or editing documents for Quest. To a larghe extent, this reflect the way it has always been done, which should be followed for consistency, even if it is not the best way.


Filenames
---------

Files should be named in lower case, as Github ordering takes account of case, so files starting with a capital will be out of order. Filenames should be composed of letters, numbers and underscores only (no spaces), and end ".md".

Note that links to files should be in the same case, but must link to the ".html" version.


Header
-----

The standard header looks like this and must be at the top of every page:

```
---
layout: index
title: Docs Style Guide
---
```

Headings
--------

The documents use two levels of headings, H1 and H3. H1 should be indicated by a row of hyphens under the title, whilst H3 should be indicated by three hashes at the start of the line.


Inline Formating
----------

Inline formating is for a short bit of text within a line or paragraph.

Names of functions should be denoted with single backticks. Names of objects and attributes can be denoted with double quotes (but is not done with any consistency at present!).

Commands or anything the player will type should be written in full capitals.


Block Formating
----------

Block formating is where the text is a whole line or paragraph.

For text the player will see or will type, use the greater than symbol to produce block quotes, and again, what theplayer types should be in capitals. For code, use either three back ticks or indent every line by four (or more) spaces.




Images
------

Images should be in PNG format, in the docs/images folder. They should be inserted into the document in the following format:

```
![](images/myimage.png "myimage.png")
```

The bit in quotes is the "alt text", what the user sees when hovering over the image. If you want to put meaningful text in there, that is to be encouraged, but sadly lacking at present.