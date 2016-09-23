---
layout: index
title: Clothing Library
---

A very simple module that builds wearable items into the game.

Contributed by: <span class="author">The Pixie</span>

You are free to use this library in your own games, without crediting me, as long as the library is not modified. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.

#### To use

Have garments inherit from “clothing\_type”. They will be set to be takeable, and when worn will appear in the inventory with “(being worn)” appended to the name. Check the attribute “being\_worn” to determine if the item is being worn in a script.

[Download](http://textadventures.co.uk/attachment/118)
[Discussion](http://textadventures.co.uk/forum/samples/topic/2567/adding-clothing)

#### Functions

Name (**object** *obj*, **string** *my\_name*)

*Returns:*Some value as an integer

#### Types

clothing\_type

Items of this type can be taken, worn and removed.

#### Player Commands

wear \#object\#

Player puts on the given object.

remove \#object\#

Player removes the given object.
