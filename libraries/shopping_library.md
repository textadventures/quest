---
layout: index
title: Shopping Library
---

Allows the creator to readily create a shop.

Contributed by: <span class="author">The Pixie</span>

You are free to use this library in your own games, without crediting me, as long as the library is not modified. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.

#### To use:

1.  Have a Boolean attribute on your shop called “shop” set to true.
2.  Have an integer attribute on the player called “money”, set to the amount of cash the player starts with. It would be good to set this as a status attribute too.
3.  Any goods for sale should be placed in the shop. Set to scenery and not takeable (these will be changed when the item is purchased). They also need an integer attribute “price”; this will be deducted from the player’s money.

[Download]({{site.baseurl}}/files/shopping.zip)
[Discussion](http://www.axeuk.com/phpBB3/viewtopic.php?f=10&t=2557)

#### Player Commands

buy \#object\#; purchase \#object\#

Player buys the given object.
