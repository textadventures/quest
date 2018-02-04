The built-in WAIT command allows the player to wait for just one turn. We can improve on that by adding a new command that allows the player to wait any number of turns. There is an issue here: what if the player types WAIT 10 TURNS, and then gets a hoard of orcs appear after 2 turns. Is she really going to want to wait for the next eight turns? What we want is to allow the game to interrupt the waiting process if something important happens.

### A new function

This is actually pretty straight forward to do; essentially just a loop that calls the built-in `RunTurnScripts` function. We will put the loop in a function so we can break out of it if required.

So create a new function, and call it WaitN. Set it to return an Integer, and give it a single parameter, n (we will ensure n is an integer before passing it to the function). Paste in this code:
```
game.stop_waiting = false
for (i, 1, n-1) {
  RunTurnScripts
  if (game.stop_waiting) {
    return (i)
  }
}
return (n)
```

### A new command

Now create a new command, and set it to use a regular expression for the pattern. Paste in this pattern:
```
^(wait|z) (?<text>\w*)( turn| turns|)$
```
If in your game one turn is one minute, you might want to use this instead:
```
^(wait|z) (?<text>\w*)( minute| minutes| turn| turns|)$
```
This will allow the user to type Z 4 or WAIT 5 TURNS or whatever.

The code has to check the player typed a number, and if she did, run our new function. Here is the code:
```
if (not IsInt(text)) {
  msg ("Wait how long (I did not understand \"" + text + "\")?")
}
else {
  msg ("You wait for " + WaitN(ToInt(text)) + " minutes.")
}
```
You might want to change "minutes" to "turns" for your game. Go into the game and check it works.

### Your turn scripts

The next step is to modify your turn scripts so they will interrupt the waiting process. This is easy to do, you just need to add this line in any block of code that causes a significant event:
```
game.stop_waiting = true
```
Here is a trivial example in case you want to test the system:
```
switch (GetRandomInt(1, 8)) {
  case (1) {
    msg ("A butterfly flutters by.")
  }
  case (2) {
    msg ("You heard a lark, singing in the distance.")
  }
  case (3) {
    msg ("An ant crawls over your foot.")
  }
  case (4) {
    msg ("A hoard of orcs appears!!!")
    game.stop_waiting = true
  }
}
```

### Limitations

Note that turns scripts will run after the wait has terminated. If the player does WAIT 10 TURNS, and the orc horde appears on turn 2, this command will terminate, and then, as with all other commands, the turn scripts will all be run.

### Plural or singular?

It would be more slick if it said "You wait 1 minute" rather than "You wait 1 minutes". We can use the text processor to handle that, though we do need to set an attribute on something that is the number of turns waited for the text processor to check. Go to the code of the command, and replace the last three lines with these four lines
```
else {
  game.waitcount = WaitN(ToInt(text))
  msg ("You wait for " + game.waitcount + " minute{if game.waitcount>1:s}.")
}
```

### Further improvement

It would also be nice to be able to handle WAIT THREE MINUTES. We can do that by creating a new function, `ConvertToNumber`, which will compare the text to a list of numbers. It will be easiest if it returns a string, so it can return the original if it is not successful. It also needs a single parameter, s. Here is the code:
```
numbers = Split("zero;one;two;three;four;five;six;seven;eight;nine;ten;eleven;twelve", ";")
for (i, 0, ListCount(numbers) - 1) {
  if (s = StringListItem(numbers, i)) {
    return ("" + i)
  }
}
return (s)
```
We now need to modify the command, to use the new function. This requires inserting a new line at the start. Here is the new code for the command:
```
text = ConvertToNumber(text)
if (not IsInt(text)) {
  msg ("Wait how long (I did not understand \"" + text + "\")?")
}
else {
  game.waitcount = WaitN(ToInt(text))
  msg ("You wait for " + game.waitcount + " minute{if game.waitcount>1:s}.")
}