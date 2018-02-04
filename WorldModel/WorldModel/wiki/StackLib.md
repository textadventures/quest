NOTE: _This is applicable to the off-line editor only, as it is a custom library._

NOTE: _This was inspired by Sora's stackable library, though the coding is pretty different, and it is used a little differently. You can find Sora's library [here](http://textadventures.co.uk/forum/samples/topic/3515/stackable-library)._

This library offers a way to have items stack in the player inventory. If the player picks up 15 potions, you can have it so these are a single entry in the inventory pane; the player clicks on that and then gets to choose which one she wants.

[StackLib](https://github.com/ThePix/quest/blob/master/StackLib.aslx)


### Library

See [here](https://github.com/ThePix/quest/wiki/Using-Libraries) for how to add a library to your game.

### Stack Objects

The way the system works is to have objects that represent the stack, as well as the objects that are the members of the stack. The player should have no direct access to the stack objects, so it is best to create a new room with no exits to or from it, and to put the stack objects there.

It supports two types of stacks, heterogeneous and homogeneous. In a homogeneous stack, ever member of the stack is the same, so if the player selects the stack, and then clicks "Use", Quest will just take the item from the top of the stack and apply the command to that.

In a heterogeneous stack, the items can be different (perhaps a variety of different potions). If there is more than one item in the stack, the player will be asked to select one, and Quest will apply the command to that item.

So if we want to have a stack of potions, create an object, "potion stack". Give it an alias; this will be how the stack appears in the inventory pane, with a number appended. In this case "Potions" is good. Now on the _Stackable_ tab, set this to be "Heterogeneous stack".

### Other Objects

For each object that can go into that stack, you need to again go to the _Stackable_ tab, and this time set it to be "Member of a stack". You then need to set the stack object for the item (in this case "potion stack").

### Verbs

By default, the stack will display "Look at", "Use" and "Drop", and will handle each of them - as long as the members of the stack can handle "use" and you have set a description (either text or script).

To change the displayed verbs, go to the _Object_ tab of the stack object, and add or remove inventory verbs as desired. To get the stack to handle a new verb, go to the _Verbs_ tab of the stack, and add the verb there. Now you need a script... For our potions, we might want to add a "drink", so we can use that as an example. The library has a function built-in that does most of the work, `DoStackVerb`, and is used like this:

```
DoStackVerb (this, "drink")
```

It takes two parameters. The first in the stack object, and as this script is an attribute of that, we can just use "this" to stand for it. The second parameter is the verb.

When the player selects "drink" for the potion stack, this function will ask which one, then will then run the verb for that object. Obviously you need to ensure that each member of the stack has a "drink" verb.

For a potion, the drink command will probably include removing the potion, as once it is consumed, it is gone. After the script is run, Quest will automatically recalculate the number of objects in the stack.

### Money

You can also set an object to be a pile of cash. When a pile of cash is picked up, its value is added to the player's money, and the object removed.

Quest 5.7 has a "money" attribute built-in for the player; earlier versions you will need to set it yourself (and as of writing, Quest 5.7 has not been released!).

### Notes

Cloning stackable objects should produce a clone that also stacks.