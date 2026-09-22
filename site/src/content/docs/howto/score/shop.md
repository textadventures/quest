---
title: Setting up a shop
description: Let the player buy and sell items using the built-in money feature, with stock kept in a stockroom
---

This page shows you how to build a shop where the player can see what's for sale, buy things if they can afford them, and sell things back. It uses Quest Viva's built-in money feature, a stockroom the player can never reach, and three commands: BUY, BROWSE and SELL. The same three commands work for every shop in your game.

As an example, we'll build a bakery selling a lamington, a Victoria sponge and a fairy cake.

## Turn on money

1. On the game object's _Features_ tab, tick "Money". The player's money now appears in the status pane.
2. On the game object's _Player_ tab, set "Format for money". The `!` stands for the amount, so `£!` shows 10 as "£10". Money is always a whole number, so decide whether your unit is pounds or pence. See [DisplayMoney](/reference/functions/string#displaymoney) for more formats.
3. On the player object's _Player_ tab, set "Starting money". If you leave it, the player starts with nothing.

There's more about money, and the "Increase money" and "Decrease money" script commands, in [Score, health and money](/howto/score/score-health-money).

## The shop and its stockroom

Create the shop as an ordinary room, "bakery", with exits to and from it as usual.

Then create a second room, "bakery stock", and give it no exits. This is the stockroom. Put everything the bakery sells inside it, and on each item's _Inventory_ tab, set its "Price". Because the player can never get to the stockroom, they can't see or take anything in it - the only way to get stock out is to buy it.

Finally, link the shop to its stockroom. On the bakery's _Attributes_ tab, add an attribute called `stock`, set its type to "Object", and choose "bakery stock". Every room with a `stock` attribute is a shop, and the commands below look in whichever stockroom it points to.

You can also set it in a script, such as the game's start script on the _Scripts_ tab:

```quest
bakery.stock = bakery stock
```

## Buying

Add a command with the "Add Command" button, and set:

- **Pattern**: "Command pattern", `buy #object#; purchase #object#`
- **Scope**: `stock`
- **Unresolved object text**: "Text", `That isn't for sale here.`

Setting the scope to `stock` tells Quest Viva to look for `#object#` among the contents of the stockroom that the current room's `stock` attribute points to. Anywhere that isn't a shop, there's no `stock` attribute, so BUY finds nothing for sale.

Then paste this into the command's script (click "Code view" in the script editor first):

```quest
if (not HasObject(game.pov.parent, "stock")) {
  msg ("There's nothing for sale here.")
}
else if (object.parent = game.pov) {
  msg ("You already have " + object.article + ".")
}
else if (not object.parent = game.pov.parent.stock) {
  msg ("That isn't for sale here.")
}
else if (object.price > game.pov.money) {
  msg ("You can't afford " + GetDisplayName(object) + ". It costs " + DisplayMoney(object.price) + ".")
}
else {
  DecreaseMoney (object.price)
  MoveObject (object, game.pov)
  msg ("You buy " + GetDisplayName(object) + " for " + DisplayMoney(object.price) + ".")
}
```

The checks run in order:

1. The player isn't in a shop. If Quest Viva can't find the object in a stockroom, it also looks in the room and the player's inventory, so BUY LAMINGTON outside the shop can still match a lamington the player is carrying.
2. The player already has the item - for example, BUY SPONGE after buying the only sponge.
3. The item is somewhere else, such as lying on the shop floor, rather than in this shop's stockroom.
4. The player can't afford it. Because the purchase only happens when the price is no more than the player's money, money can never go below zero. A player with exactly enough money can buy the item and is left with nothing.
5. Otherwise, the item is sold. `DecreaseMoney` takes the price off the player's money, and the item moves from the stockroom to the player.

Here's how it plays, starting with £10:

```
> buy sponge
You buy a Victoria sponge for £8.

> buy lamington
You can't afford a lamington. It costs £3.

> buy fairy cake
You buy a fairy cake for £2.

> buy sponge
You already have it.
```

Your BUY command takes the place of the built-in BUY verb, which only ever tells the player they can't buy the object.

## Seeing what's for sale

The player can't see into the stockroom, so give them a way to list it. Add a second command with the pattern `browse; list stock; wares`, leave the scope blank, and paste in this script:

```quest
if (not HasObject(game.pov.parent, "stock")) {
  msg ("There's nothing for sale here.")
}
else {
  items = GetDirectChildren(game.pov.parent.stock)
  if (ListCount(items) = 0) {
    msg ("Everything has sold out.")
  }
  else {
    msg ("For sale:")
    foreach (item, items) {
      msg ("- " + CapFirst(GetDisplayAlias(item)) + ", " + DisplayMoney(item.price))
    }
  }
}
```

```
> browse
For sale:
- Lamington, £3
- Victoria sponge, £8
- Fairy cake, £2
```

Items the player has bought are no longer in the stockroom, so they drop off the list. You can also mention the command in the shop's description so the player knows it's there, for example "A sign says: BROWSE to see what's for sale."

## Selling

Selling works the other way round: the item goes from the player into the shop's stockroom, and the player gets money for it. Add a third command:

- **Pattern**: "Command pattern", `sell #object#`
- **Scope**: `inventory`
- **Unresolved object text**: "Text", `You aren't carrying that.`

The `inventory` scope makes Quest Viva look in the player's inventory first. Paste in this script:

```quest
if (not HasObject(game.pov.parent, "stock")) {
  msg ("There's no one here to buy it.")
}
else if (not object.parent = game.pov) {
  msg ("You aren't carrying " + object.article + ".")
}
else if (object.price = 0) {
  msg ("The shopkeeper doesn't want " + object.article + ".")
}
else {
  offer = object.price / 2
  IncreaseMoney (offer)
  MoveObject (object, game.pov.parent.stock)
  msg ("You sell " + GetDisplayName(object) + " for " + DisplayMoney(offer) + ".")
}
```

The shop pays half the item's price, so the player can't make money by buying and selling the same thing. Dividing one whole number by another gives a whole number, rounded down, so a £3 lamington sells for £1. Items with no price, such as a pebble the player picked up, can't be sold.

Sold items go into the shop's stockroom, so they appear in BROWSE and the player can buy them back at the full price:

```
> sell lamington
You sell a lamington for £1.

> browse
For sale:
- Victoria sponge, £8
- Fairy cake, £2
- Lamington, £3
```

If you don't want the player to sell anything at all, leave this command out.

## Adding more shops

For each new shop, create its room and a stockroom, put the stock in the stockroom, and give the shop a `stock` attribute pointing to it. You don't need to change the commands.

## Variation: shops that never run out

To let the player buy as many of an item as they like, sell them a copy - a [clone](/howto/scripting/clones) - and leave the original in the stockroom. In the BUY script, replace the `MoveObject` line with:

```quest
CloneObjectAndMove (object, game.pov)
```

The original never leaves the stockroom, so it stays in BROWSE, and BUY LAMINGTON always finds it before any lamington the player is carrying.

Selling a clone back would then leave two lamingtons in the stockroom. Instead, in the SELL script, get rid of clones and only move originals into the stockroom. Replace the `MoveObject` line with:

```quest
if (HasObject(object, "prototype")) {
  RemoveObject (object)
}
else {
  MoveObject (object, game.pov.parent.stock)
}
```

Every clone has a `prototype` attribute pointing to the object it was copied from, so this is how you tell them apart.

## See also

- [Score, health and money](/howto/score/score-health-money)
- [Commands](/howto/commands/how-commands-work)
- [Advanced scope](/howto/commands/scope) - more ways to set a command's scope
- [Clones](/howto/scripting/clones)
