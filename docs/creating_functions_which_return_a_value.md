---
layout: index
title: Creating functions
---

If you find you have several places in the game where you have script (or bits of scripts) doing essentially the same thing, you are probably better creating a function to do it just one. The basic idea is that you put the code in just one place, and then anywhere that needs to use the code, you send to the place.

If you did the tutorial, you will already have encountered functions [here](tutorial/more_things_to_do_with_objects.html#Using_Functions).

A typical use of a function in Quest is when you have several ways the player can do something. Perhaps there is a chair the player can sit on, and she might type SIT, SIT ON CHAIR or USE CHAIR. You could handle SIT as a command, SIT ON CHAIR as a verb or a command and USE CHAIR via the Use/Give feature... but they all do the same thing, so we will use a function.

To create a function, go to "Functions" in the left pane, then click the Add button in the right pane (there are others ways too on the desktop version). Give it a name. The standard format is to capitalise each word, and leave out the gaps, so we will call ours "SitOnChair".

You can add a script just as you would for a command or anything else.



Parameters
----------

### Calling the function

You can send values to your script and have it use them when it runs. Perhaps there are several things the player can sit on, and we need the function to react appropriately. We can do that by sending the object the player is trying to sit on to the function.

In this example, you may want to handle the command with a pattern like this:

> sit on #object#

The script for the command could then pass the object to the function, like this:

```
SitOnChair(object)
```

Your Use/Give script (and _Verbs_ script if you did it that way) can be modified like this:

```
SitOnChair(this)
```

The special identifier `this` indicates the object the script is attached to.

The script for the SIT command would have to specify the chair. How do you know the chair is in the room? One way would be to not allow the player to move the chair, and to move the command into that room. You could have a SIT command in every room where there is something the player could sit on.

```
SitOnChair(chair)
```

Hmm, we have a design decision to make. The player might want to try sitting on anything, so somewhere we need to check if the object can be sat. Do we do that in the function, or in the command? We do not need to do it for the Use/Give script (or the verb script), because they will only be on objects you can sit on, and the SIT command is only going to send items you can sit on too. So it is probably best to check this in the command, so we will update the script to do that (if you have lots of things the player can sit on, you might want to check a flag):

```
if (not object = chair) {
  msg("That's not something you can sit on.")
}
else {
  SitOnChair(object)
}
```


### Changing the function

Now we need to modify the function. Above the script, there is a list of parameters, currently empty. Click Add, and type in the name. Note that the name does not have to be the same as we used earlier (just as well, as we have used "object", "this" and "chair"). We know that what we send here can be sat on, so we will call it "chair". If we had chosen to check in the function, it would be better to call it "object", as a hint that the function can handle any object.

A function can have any number of parameters. If it has more than one, then the order is very important. When you call a function, the parameters you send must be in the same order that they are listed in the function - this is how Quest decides which is which.

In the script for the function, you can refer to the parameters as you named them in the list.

```
msg("You sit on the " + GetDisplayAlias(chair) + ".")
player.sat = true
```


Returning A Value
-----------------

It is often useful to have function return a value. There may be several points in your game where you need to do the same calculation; it would be better to do the calculation in a function, and have the function return the result as an integer. Or there might be several conditions that the player needs to meet to advance to the next stage, and you want a function that returns a Boolean. Functions can return any type including lists, but, while you can have as many parameters as you like, you can only return one value.

By way of an example, we will create a function that will get a list of objects the player can sit on in the current room. We will suppose that any such object is flagged by have an attribute "cansiton" set to true.

Create the function as normal, give it a name and then set the return type to "object list". Here is some code:

```
list = NewObjectList()
foreach (o, ScopeVisible()) {
  if (GetBoolean(o, "cansiton")) {
    list add (list, o)
  }
}
return (list)
```

Look at the last line. This is telling Quest what value we want the function to come back with. In this case, a local variable, `list`. The rest of the code is initialising `list`, and then adding values to it.

By the way, there is a quick way to do this in Quest:

```
FilterByAttribute(ScopeVisible(), "cansiton", true))
```


### Shortcutting

Let us suppose you just want a single object you can sit on from your function. In this case the return type would be "object", and the code might look like this:

```
foreach (o, ScopeVisible()) {
  if (GetBoolean(o, "cansiton")) {
    return (o)
  }
}
return (null)
```

What we are doing here is that as soon as we find an object that matches, we return that value straightaway. No need to go through the rest of the list, or indeed any other code in the function, we have what we want already.

We do have to take account of what will happen if we find nothing, so the last line returns a default value. For objects that should be null. For integers there might be a value that makes sense. Whatever it is, whenever you use the function, you need to remember that it could return the default function.



Testing
-------

It is important to test your functions properly. Often the most convenient way to do that is to _temporarily_ put them into the "start" script of the `game` object. Let us suppose the script to get a list of objects that can be sat on is called "GetSittables", you could put this in the start script:

```
msg("---------------------------")
msg(GetSittables())
player.parent = lounge
msg(GetSittables())
```

The first line just prints a line, making it easier to see where the results are. The second line prints whatever is returned by the function, hopefully a list of objects that can be sat on. Then we move the player to a new room, and try the function there.

Remember to remove the code before releasing the game.

For functions that do not return a value, you would need to print out something that does change. If using the desktop version, you can also use the _Debugger_ to check the current value of object attributes.