Note: _In Quest, everything is an object, even rooms. To try to avoid confusion, I will use "item" to refer to things the player can manipulate, "room" to refer to rooms (obviously) and "object" to refer to everything, including the game and the player!_

Often you want the description of an object to change, depending on what the player has done. Perhaps she has blown a hole in the wall, and the room description needs to reflect that. Perhaps she has turning on the bizarre machine, and the item description needs to mention that.


## Step 1

First you need a Boolean attribute, also called a flag, which will be used to remember the state of the room or item. A Boolean can have the value True or False. When it is called a flag, it can be on or off (but behind the scenes a Boolean is the same as a flag).

Attributes are attached to objects, and it does not matter what object you pick. It could be the game object, it could be the player object, it could be some other object. Generally, it is easiest to set an attribute on the object in question (the room or item). Whatever you choose, the procedure is more or less the same.

Go to the Attributes tab of the object, and look at the lower section, also called Attributes. Click on Add, and type a suitable name. I recommend something descriptive of the state it will become. If a cistern needs to be emptied, use "emptied", if a wall that needs to be smashed, use "smashed".

Right at the bottom, click on the button that says "String", and set it to "Boolean" instead. It will start unticked, i.e., false or off.


## Step 2

For now, I am assuming we have a Boolean attribute called "emptied", on the object itself.

Go to the description for the item (on the Setup tab) or room (Room tab). If you have already typed something there, copy-and-paste it somewhere first. Click on "Text", and select instead "Run script".

Click the "Add new script" button, and select "If...", and then click the "Add" button. A box with lots of stuff will appear within the object description area, with "If: expression" at the top. In the text box to the right of that type "this.flag" (no quotes).

Click "Add new script", select "Print a message", and in the new text box type in the description the player should see after the flag has been set.

Click "Add Else", then click the new "Add new script" button, and select "Print a message" again. In the new text box type in the description the player should see before the flag has been set.

It should look like this:

![](https://raw.githubusercontent.com/ThePix/quest/master/desc1.png)



So what does it mean?

The expression is what Quest will use to decide what to do. Quest has a special word, "this", that refers to the current object (the object that the script belongs to), in this case your room or object. It does not matter what you called the object, "this" will refer to it. The dot indicates that what follows will be an attribute, and "flag" is just the name of the attribute we set up in step 1.

So if the flag is set to True, the top section of the box is done, and if it is set to False, the bottom section (the "Else" section) is done. And all each section does is print a message.

While we are here, look just under "Run script", and click on the seventh icon, "Code view". You will see this:
```
  if (this.flag) {
    msg ("A big room, with wood-panelled walls. There is a large hole in the west wall, where the wood is blackeden.")
 }
  else {
    msg ("A big room, with wood-panelled walls.")
  }
```

It is exactly same set of instructions, just displayed differently. If the flag is True do the first bit, if it is False, do the other bit. It is useful to get familiar with this code view, as it is much easier to use on the forums.


## Step 3

The final set is to have something trigger the change. You are kind of on your own here as there are so many ways this might happen, but I will go through an example.

Let us suppose breaking a teapot causes something to explode in a room called "room". On the Verb tab of the teapot, "Add" a new verb, and change "Print a message" to "Run a script". From the "Add new script" menu, select "Set object flag", set the object to be the room (because that is trhe object with the flag attribute), and the flag name to "flag". You will probably want to print a message too.

![](https://raw.githubusercontent.com/ThePix/quest/master/desc2.png)
For reference, the code view looks like this:
```
SetObjectFlagOn (room, "flag")
msg ("As the teapot breaks, the chamberpot suddenly explodes in a suicidal act of sympathy.")
```
And if I was typing it in,I would write this:
```
room.flag = true
msg ("As the teapot breaks, the chamberpot suddenly explodes in a suicidal act of sympathy.")
```
It is all the same, the first line is setting the flag to have a have of true. 


## I want one flag to affect several descriptions

This is easy, you just select one object as the master object, and have all the other objects check the flag on that.

For example, a table in the room might have a description like this (note that "this" in the first line has been replaced by "room"):
```
if (room.flag) {
  msg ("A mahogany table, now showing some scorching and soot.")
}
else {
  msg ("A mahogany table in excellent condition.")
}
```

## I want several different states

Let us say you want your external locations to change depending on the time of day and weather. In this case you are probably better setting an attribute on the game object. You do this just the same as before, but give it a meaningful name, let us say "external_state", and set it to be an integer, rather than a Boolean. It will default to zero, which is fine.

This attribute will track the state of the game, so a value of 0 might indicate that it is sunny, 1 could be dusk and 3 could be nighttime (4 could be thunderstorm, etc.).

Now go to your room description, and change "Text" to "Run script". This time, select "Switch..." from the "add new script" menu. In the switch text box, type "game.external_state" (no quotes).

For each possibility, you need to click on the Add button, type in a number for the key, then click Add new script, select "Print a message", and type in the room description for that state.

![](https://raw.githubusercontent.com/ThePix/quest/master/desc3.png)
Here is the code view:
```
  switch (game.external_state) {
    case (0) {
      msg ("The path looks pretty in the sunshine.")
    }
    case (1) {
      msg ("The path stetched west towards the setting sun.")
    }
    case (3) {
      msg ("There was something spooky about the path now it was night time,")
    }
  }
```
Now you need some triggers. Perhaps as the player gets to a certain location, the game state is updated, and time moves on. The possibilities are endless, but here, briefly, is one way:

![](https://raw.githubusercontent.com/ThePix/quest/master/desc4.png)
Code view:
```
  game.external_state = 1
```