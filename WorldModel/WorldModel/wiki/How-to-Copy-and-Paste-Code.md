Behind the scenes, Quest handles things using it own programming language, or code. If you are asking about how to do something on the forums, chances are people will respond by posting the code, and if you have never seen code before you may be left wondering what you do with it.

Once you understand the basics, code is a lot easier to show on the forum, and far easier to copy from one place and paste into another.


## Create a new function (commands are similar)

Right click in the left pane, and select add function. Give it the right name (same capitalisation, etc.). Bottom of the stuff on the right is Script. Click on the seventh icon (_Code view_) if off-line. If you are working on-line, click the "Code view" button at the bottom. You should now get a text box below. Just paste the code into this box.

Click on _Code view_ again, and you should see the normal Quest view. If you see some red text, something has gone wrong. Check that you copy-and-pasted the whole code and nothing but the code (though it could even be a mistake in the code).

You may need to set the return type or add parameters - see what the forum post says. To add a parameter, just click on the plus by the word "Parameters". Make sure you give the exact names specified and in the same order.


## The start script

Go to the "game" object, and the Script tab. At the top is the Script section. As before, click on the _Code view_ icon. You should now get a text box below. Just paste the code into this box. If there is already some text there, paste the new text underneath it.


## Verb on an object

Go to the specific object, and the Verbs tab. Click "Add", and type in the verb. Make sure the verb is selected, under the box, click on "Print a message" and select instead "Run a script". As before, click on the _Code view_ icon. You should now get a text box below. Just paste the code into this box.


## Script as attribute

Go to the specific object, and the Attributes tab. Click "Add" in the lower section, and type in the name of the attribute. Make sure the attribute is selected, under the box, click on "String" and select instead "Script". As before, click on the _Code view_ button. You should now get a text box below. Just paste the code into this box.

_You cannot do this when editing on-line._


## The GUI vs Code

Anything written in code can also be written using the GUI, and anything created using the GUI can also be written in code. They are just two ways of looking at the same thing.

That said, there are various helper functions that are designed to make the GUI easier, but are not much use when writing code. In the GUI there is an option "Move object to current room". If you look at the code, it looks like this:
```
  MoveObjectHere (hat)
```
If I was writing that if code, I would do this:
```
  hat.parent = player.parent
```
It looks completely different, but what the first does is call a function, `MoveObjectHere`, and the function then does the same thing. And both can be displayed in the GUI (though again they look quite different).