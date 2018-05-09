---
layout: index
title: Implementing components of an object
---

Occasionally you would like the player to be able to interact with a component of an object, for example, a machine with a button on it. The player has to be able to press the button. If the machine cannot be moved, you can just have the button as scenery in the room, but what do we do for objects that can be carried around?

Quest actually has this facility built-in, though it may not be obvious.

Create your object first, let us say it is called "machine". Create the component, "button", as a child of that object (right click on machine, select "Add object", and choose "machine" from te dropdown at the bottom; alternatively you can drag an object on to another). In the object hierarchy it should look like this on the left.

![](component.png "component.png")

The master object, in this case "machine" has to be set up as a type of container called a surface, as shown above (click Container on the features tab first). The component, you can have as many as you need, should be set up as scenery.

That is all there is to it.