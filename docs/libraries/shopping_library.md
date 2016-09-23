---
layout: index
title: Shopping Library
---

Allows the creator to readily create a shop.

Contributed by: <span class="author">The Pixie</span>

You are free to use this library in your own games, without crediting me, as long as the library is not modified. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.

[Download](https://textadventures.co.uk/attachment/799)

There are two types of goods handled by this library:
  
multi_merchandise: Items of this type are cloned when the player buys them, and destroyed when he sells them. He can therefore buy as many as he wants. These items must be in the inventory (and must only start the game in shop inventories).
  
oneoff_merchandise: When the player buys one of these, it is moved to him, and therefore no longer available for sale. When he sells it, it will then be offered for sale again. These items must be in the stock to be available to buy, but can be found and taken anywhere in the game world.
  
If an object is not of one of these types, the player will not be able to buy or sell it. Also, if it does not have a price attribute, the player will not be able to buy or sell it.
  
  
###How to use:

Set a string on the game object called currency or precurrency.
The value of precurrency will go before the value, currency will go after it.
  
Set an integer on the player object called gold; this will be the money she starts with.
  
Set up rooms to hold inventory and stock. Remember, inventory is for items that will be cloned, stock is for one-off items. There should be no exits going to or from these rooms.
  
Set up one locations as your shop. You can set this to be of the "shop_location"  type, but it is not necessary. To allow the shop to buy goods from the player, you must set a boolean called "buys" to true.
  
Set up objects to be of type "multi_merchandise" or "oneoff_merchandise" and give them a price
  
The description of the shop can include a call to ShopInventory; a list of goods with prices will be displayed.

    msg (ShopInventory ())
  
The player can type BUY [obj] to make a purchase, or just BUY to select from a menu. Sellable items should have a sell option in the inventory pane, or the player can type SELL [obj].
  
NOTE:  You can have as many shops as you want, and they can all use the same inventory room, but each should have its own stock room. Both stock room and inventory are optional. If a shop has no stock room, the player will be unable to sell anything there. If it has neither, it will not be considered a shop.
  
NOTE: It was developed with an RPG type game in mind, and to some degree the design reflects that.
  
NOTE: Goods are sold at one half the buying price.
    
