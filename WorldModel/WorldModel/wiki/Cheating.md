Note: _The on-line editor seems to have problems with complicated regex code, and will likely get confused and throw an error when you try to paste in this code, so this may be restricted to the off-line editor._

Sometimes when you are testing your game it is useful to be able quickly get to a certain room, get an object or variously change the game state, without having to go through the whole game, no matter how cool it is.

Here is a CHEAT command that will do all that! Just remember to delete it from your game before release.

Set up a new command. Set it to use a regular expression instead of a command pattern, and put this in:
```
^cheat (?<text1>move|set|get) (?<text2>.+)$
```
Then paste in this code:
```
switch (LCase(text1)) {
  case ("move") {
    o = GetObject(text2)
    if (o = null) {
      error ("I cannot find a location called " + text2)
    }
    else {
      player.parent = o
    }
  }
  case ("get") {
    o = GetObject(text2)
    if (o = null) {
      error ("I cannot find an object called " + text2)
    }
    else {
      o.parent = player
    }
  }
  case ("set") {
    msg("Doing " + text2)
    regex = "(?<object>.+)\\.(?<attribute>\\S+)\\s*=\\s*(?<value>.+)"
    if (not IsRegexMatch(regex, text2)) {
      error ("Sorry, wrong format")
    }
    dict = Populate(regex, text2)
    obj = GetObject(StringDictionaryItem(dict, "object"))
    if (obj = null) {
      error ("Sorry, object not recognised")
    }
    att = StringDictionaryItem(dict, "attribute")
    value = Eval(StringDictionaryItem(dict, "value"))
    set (obj, att, value)
    msg("Done")
  }
}
```
Now you can do:

> CHEAT MOVE My bedroom

> CHEAT GET lantern

> CHEAT SET player.health=60

> CHEAT SET Mary.parent = lounge

> CHEAT SET bronze sword.inventoryverbs = Split("Lookat;Drop;Destroy", ";")

So the first one moves you to another room, the second puts an item in your inventory, the third sets an attribute just as in normal Quest code.

If you want to try to understand how the code works, read the [pattern matching](https://github.com/ThePix/quest/wiki/Pattern-Matching-with-Regular-Expressions) page, which covers it to some degree.