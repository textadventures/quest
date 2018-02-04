NOTE: _This is applicable to the off-line editor only, as it is a custom library._

This library offers a simple way to add the shopping experience to your game. The way it works is that each shop has a separate room, not accessible to the player, where stock is kept. Items there can be purchased, but cannot be taken.

[ShopLib](https://github.com/ThePix/quest/blob/master/ShopLib.aslx)

## Before you start

You should think about how finely you want to track money. In UK terms, do you want to counts the pennies, or just the pounds? If you want to track pennies, then all your prices should be in pennies; computers are happiest handling whole numbers, so it is best to work in whole numbers of pennies, rather than using decimals and floating point.

For this tutorial, we will be working in pennies.


### Setting up

Add the library to your game (see [here](https://github.com/ThePix/quest/wiki/Using-Libraries) for how).

The system is an extension to the [built-in money system](http://docs.textadventures.co.uk/quest/score_health_money.html), so the next step is to set that up. Give the player £6.35 (i.e., set money to 635). The format for money should be:

> £!1.2!

If your game allows the player to assume control of more than one point-of-view, you should set the `money` attribute for each of these player objects. Each will have his or her own cash to spend.

### Setting up a shop

Shops need a second room that the player cannot access, a stockroom. In the stockroom you put all the goods the shop has for sale. Say we have a shop called "Ye Olde Shoppe", and we will need a stock room for it, which I will call "stock room for Ye Olde Shoppe". In the shop itself, add a new object attribute, `shopstock`. Set it to the stock room.

You may want to have the stock room inside the shop; if so set it to be invisible, so it does not appear in the room description.


### Setting up merchandise

So we have a shop, now we need to put stuff in it for sale. Just create the object in the stockroom, and set its price (on the _inventory_ tab). Let us add a ball at 199, and a hat at 1399.

If you now go into the game, you should be able to buy the ball, but not the hat, because it is more money than you have. It will say you spent 199 credits, but we can sort that later.

Note that if the player just types BUY in the shop, she will see a list of items for sale, with the cost, and select from that menu.


### Setting up items to sell

Everything has a price of zero by default. To give items the player finds a value, just modify their price.


### Conclusion

So now we have a basic shop, where the player can buy and sell goods, without any coding!


## Advanced features

### Clones and junk

In real life shops usually have several of each item. If it sells milk, it will have a lot of bottles. To have an item in the shop's stock get cloned instead of moved to the player's inventory, give it a Boolean attribute, `cloneonpurchase`, and set it to true. Now when the player purchases the item, a clone will be created, and that will get added to her inventory. The original will remain in stock.

Conversely, when a player sells something to the shop, you might not want it to go into the shops stock. In that case, give it a Boolean attribute, `destroyonsale`, and set it to true. By default, this will also be the case for anything that has `cloneonpurchase` set, but you can prevent the clones being destroyed by setting `destroyonsale` to false (i.e., if present, `destroyonsale` determines the fate, if not present clones are destroyed, others are not).

If the player is in a shop, and types SELL JUNK, any items in get inventory flagged as `destroyonsale` will get sold.


### Shop inventory

You can use the `ShopInventory` function to list what is available in the shop, with prices. Perhaps the room description is a script like this:
```
  msg("You look around the quaint shop. For sale, you see: " + ShopInventory() + ".")
```

### Shops that only sell

In real life, you will not be able to sell anything in most shops. If you want to have a shop that will only sell to the player, not buy anything, give it a Boolean attribute, `buyonly`, and set it to true.


### Profits for merchants

Your shop-keepers will want to make a profit, which means selling at a higher price than they pay. By default, that is not the case, there is just one flat rate. You can change that by [overriding](https://github.com/ThePix/quest/wiki/Overriding-Functions) the `BuyingPrice` and/or `SellingPrice` functions, both of which must return an integer, and take a single parameter, the object. If you want shop keepers to buy goods at only half their value, the code for `SellingPrice` (because it is the player selling) would look like this:
```
  return (obj.price / 2)
```

You could have a shop offer a discount too. Use this code in BuyingPrice, and if the shop has an int attribute called "discount", the player will buy stuff at a discount. For 15% off, set discount to 15.

```
  if (HasInt(player.parent, "discount")) {
    return (obj.price * (100 - player.parent.discount) / 100)
  }
  return (obj.price)
```

Adapted from Deeper, if the player has the status (i.e., spell) "Merchant's Tongue", she can sell the item for 25% extra if `SellingPrice` has this code:

```
  if (players.status = "Merchant's Tongue") {
    return (obj.price * (125) / 100)
  }
  return (obj.price)
```

### Inventory limits

The library automatically handles inventory limits, by preventing the player from making a purchase if she is already holding the maximum number of items. However, it does _not_ support the volume limit.


### Customisation

The library uses a number of dynamic templates, and you can [override](https://github.com/ThePix/quest/wiki/Overriding-Functions) them in your game.

Here is a full list of existing templates.
```
  <dynamictemplate name="NotForSale">"Sorry " + GetDisplayName(object) + " is not for sale."</dynamictemplate>
  <dynamictemplate name="CannotAfford">"You look longingly at " + GetDisplayName(object) + ", but you cannot afford it."</dynamictemplate>
  <dynamictemplate name="BuySuccessful">"You buy " + GetDisplayName(object) + " for " + DisplayMoney(BuyingPrice(object)) + "."</dynamictemplate>
  <dynamictemplate name="SellSuccessful">"You sell " + GetDisplayName(object) + " for " + DisplayMoney(SellingPrice(object)) + "."</dynamictemplate>
  <dynamictemplate name="NotAShop">"Where do you think you are, a shop or something?"</dynamictemplate>
  <dynamictemplate name="BuyOnly">"You cannot sell things here - only buy."</dynamictemplate>
  <dynamictemplate name="DoNotHaveToSell">"You do not have " + GetDisplayName (object) + "."</dynamictemplate>
  <dynamictemplate name="ShopMenuHeader">"Item to purchase (have " + DisplayMoney(game.pov.money) + ")"</dynamictemplate>
  <dynamictemplate name="ChangeMind">"You think about making a purchase, but decide not to."</dynamictemplate>
```

### Actually no thanks

If you add a Boolean attribute, "addnothingtoshoplist" to the game object and set it to true, then when the player does BUY and is presented with a list, "Nothing" will be added to the list, and the player will have to make a choice before proceeding.

You can override the `ChangeMind` template to change the default response