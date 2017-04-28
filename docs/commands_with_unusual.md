---
layout: index
title: Commands With Unusual Scope
---

When Quest processes a command that the player is tyed, it will attempt to match any object mentioned againt items in the room or that the player is holding. This is for occasions when you want a command to access a _different_ scope. Some examples:

-  _A shop inventory_ I want the player to be able to type BUY BREAD, and Quest will look though the shop inventory object for the bread object.
-  _A spell book_ I want the player to be able to type CAST UNLOCK, and Quest will look though the spell book object for the unlock object.
-  _A contact list for a phone_ I want the player to be able to call characters that are not in the current room.


The problem
-----------

Let's look at the problem in more detail first.

Say we want to set up a shop, and allow the player to buy items from its stock. If each item is unique, that is easy enough, but most shops have several of each item. Say the player buys a hat, we would expect that the shop still has hats, and the player could buy a second and a third. That means you need to clone the original, and that means that if the player does WEAR HAT whilst in the shop, the player will be asked which one? The one he is carrying, or the one in the shop?

One solution is to not have the hat in the shop in the first place. The problem with _that_ is that Quest will try to match the text in the command with an object that is present, and not with anythingh in the shop's stock.


A new function
--------------

To get this to work, we are going to create a new function. The function is general, you just need one, and all your commands of unusual scope can use it. To create a new function, click _Functions_ in the pane on the left, then click _Add_ above the list of functions on the right.

Give it the name `ProcessScopeCommand`. Set the return type to `Boolean`, and give it three parameters: `func`, `scope` and `text`.

Click on _Code view_ for the function, and paste this in (most of which is taken from the regular Quest command handling code):

```
value = Trim (LCase (text))
fullmatches = NewObjectList()
partialmatches = NewObjectList()
foreach (obj, scope) {
  name = LCase(GetDisplayAlias(obj))
  CompareNames (name, value, obj, fullmatches, partialmatches)
  if (obj.alt <> null) {
    foreach (altname, obj.alt) {
      CompareNames (LCase(altname), value, obj, fullmatches, partialmatches)
    }
  }
}
if (game.lastobjects <> null) {
  foreach (obj, game.lastobjects) {
    CompareNames (LCase(obj.article), value, obj, fullmatches, partialmatches)
    CompareNames (LCase(obj.gender), value, obj, fullmatches, partialmatches)
  }
}
if (ListCount(fullmatches) = 1) {
  game.scopecommandpending = ListItem(fullmatches, 0)
}
else if (ListCount(fullmatches) = 0 and ListCount(partialmatches) = 1) {
  game.scopecommandpending = ListItem(partialmatches, 0)
}
else if (ListCount(fullmatches) + ListCount(partialmatches) = 0) {
  return (false)
}
else {
  menu = NewStringDictionary()
  GenerateMenuChoices (menu, fullmatches)
  GenerateMenuChoices (menu, partialmatches)
  show menu (DynamicTemplate("DisambiguateMenu", value), menu, true) {
    if (result <> null) {
      game.scopecommandpending = GetObject(result)
    }
    else {
      return (false)
    }
  }
}
on ready {
  if (game.scopecommandpending = null) {
    return (false)
  }
  else {
    return (eval (func + "(" + game.scopecommandpending.name + ")"))
  }
}
```

I am not even going to try to explain that! Just go back to GUI view, and check there is no red, to ensure you copied it right.


About the function
------------------

So here are the parameters again:

-  **func**: A string that gives the name of the custom function to call for your command
-  **scope**: An object list from which to get objects
-  **text**: The text variable from command


How is it used?
---------------

So we have two rooms; the shop, and the shop's stock (the latter inaccessible to the player). Everything for sale goes in the stock room.

Now create the command. The pattern to match will be this:

```
  buy #text#
```

We cannot use #object# here; Quest would match that against objects held on in the room. Instead, we match the text, and the function above will tryu to match it.

Here is the script for the command. We want to check the player is in a shop, and if the text is not matched, the function will return false, so we need to handle that too.

```
if (not player.parent = Hat Shop) {
  msg ("You are not in a shop!")
}
else {
  f = ProcessScopeCommand ("BuyFunction", GetDirectChildren (Shop Stockroom), text)
  if (not f) {
    msg ("You can't buy " + text + " here.")
  }
}
```

The important part of the script is this:

```
  ProcessScopeCommand ("BuyFunction", GetDirectChildren (Shop Stockroom), text)
```

We are telling `ProcessScopeCommand` that if it matches an object, it should call a function called `BuyFunction` (we will have to create that later). The `GetDirectChildren` function returns the contents of a room, so this is the scope that the command will use; an object list of the shop's stock. The `text` variable is what the player typed.


### A custom BUY function

Finally, we need a function that does the buying, the BuyFunction. This is specific to your game, so this is just an example. You can call it whatever you like, but obviously it has to match the name you use in the command. You must set the return type to Boolean, and it must have exactly one parameter, object, the item the player wants to buy.

The function will only be called if the object is in the scope list, in this case in the shop's stock, so no need to check that.

Let us suppose the player has an attribute `money` and the items in the shop all have an attribute, `cost`. The script in the function might look like this:

```
if (object.price > player.money) {
  msg("You can't afford that.")
}
else {
  msg ("You buy " + GetDisplayName(object) + ".")
  player.money = player.money - object.price
  CloneObjectAndMove(object, player)
}
return (true)
```

Note that the function should return true if it has handled the situation, whether successfully or not, as in this case. It should return false if it does not handle the situation.


Phone a Friend
--------------

Another example might be using a telephone to phone someone. We can use the same function, but obviously need a new command. Here is the command pattern (in this case three options are allowed):

```
  call #text#;phone #text#; telephone #text#
```

The script might look like this (I am assuming a mobile phone the player must have in her inventory; a landline would be handled by checking the current room as before):

```
if (not mobile_phone.parent = player) {
  msg ("You do not have a phone!")
}
else {
  f = ProcessScopeCommand ("PhoneAFriend", player.contacts, text)
}
```

Note that we assign the result of ProcessScopeCommand to a variable, _f_, but do nothing with it. In this case ProcessScopeCommand will always be returning _true_ so no need to do anything, but Quest can get awkward if the return value is not used.

The list of options is `player.contacts`. The is a list of objects, which can be set up in the `game.start` script (even if using the desktop version; you cannot add objects lists on the _Attributes_ tab). In this instance, the player starts with Michael and Mary in her contacts:

```
player.contacts = NewObjectList()
list add(player.contacts, Mary)
list add(player.contacts, Michael)
```

With the shop it was easier to store all the things in one room, and then use the contents of that for the ProcessScopeCommand. This time, there is a good chance Mary and Michael have to be in specific places in the game so the player can meet them in person, so it is better so keep a list. If the player makes new friends, they can give their number to the player, and we can add them to the object list, `player.contacts`. You could make it possible for the player to see a list of contacts too.

### Custom function

Now we need the PhoneAFriend function. The slick way to do this is to have the function call a script on that character, and have the script do the work. That way you can tailor each script to the character. If you later add another character, just add the script to that character, and have the character added to the contacts some how.

Add a function in the normal way, set it to return a Boolean, and give it a single parameter, object. Paste in this script.
```
if (HasScript(object, "phoneafriend")) {
  do (object, "phoneafriend")
}
else {
  msg ("You try to phone " + GetDisplayName(object) + ", but it goes straight to ansaphone.")
}
return (true)
```
What the script does is check if the object has the relevant script. If it does, it runs that, otherwise it gives a default message. Finally it returns a value, to confirm to the requirements for _ProcessScopeCommand_.


### Adding a Script

So now we need a script called "phoneafriend" for each character who might appear in the list of contacts. If off-line, go to the _Attributes_ tab, and click _Add_ in the "Attributes" section, type in "phoneafriend", and set its type to script.

If you are using the on-line editor, you cannot do that, but there is a way around that, though it is hack. Go to the _Verbs_ tab, and click _Add_ there. You can then type in "phoneafriend", and set it to "Run a script". This will give you a "phoneafriend" script attribute just as before. However, it will also create a verb object that you will never see, but does mean a player could type PHONEAFRIEND MARY as a command so this is not ideal (and this is why I choose an odd, lengthy script name).

What goes in the script? Anything you like!

```
msg ("You phone Mary; 'Oh it's you! she exclaims before hanging up.")
```

### Contacts

For completeness Here is a quick script for a CONTACTS command:
```
if (not mobile_phone.parent = player) {
  msg ("You do not have a phone!")
}
else {
  s = "You look through your contacts on your phone."
  foreach (o, player.contacts) {
    s = s + " " + GetDisplayName(o) + "."
  }
  msg (s)
}
```

