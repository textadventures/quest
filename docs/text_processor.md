---
layout: index
title: Text processor
---

Quest 5.4 introduces a text processor, giving an easy way to conditionally print text, show object lnks, show text only once, and more.

To use the text processor, you can simply add a command in curly braces in any text that gets displayed. In this simple example, a room description is set to say that rom smells only the first time the text is printed:

![](text_processor_text.png "text_processor_text.png")

The more important text areas have shortcut buttons for some text processor commands; these are the buttons on the right in the image above. However, you can use text processor commands in any text, for example, in an [msg](scripts/msg.html) command:

     msg ("Would you like some {command:help}?")

You can use as many sections as you like within the same text, and even nest them:

     msg ("You can {command:go to shop:go into the shop}. {if player.coins>10:You have {player.coins} coins, which is more than enough.}")

Supported processor commands are:

Text adventure mode and Gamebook mode
-------------------------------------

{once:_text_}  
Displays the text only once. The text will not be printed on subsequent occasions.

{notfirst:_text_}  
Does not displays the text the first time it is printed; the text will only be printed on subsequent occasions.

{random:_text 1:text 2:text 3_}  
Choose text at random (you can have as many sections as you like).

{img:_filename.png_}  
Insert the specified image.

{_object.attribute_}  
Displays the value of an object's attribute.

{if _object.attribute_:_text_}  
Display text only if object attribute is true.

{if not _object.attribute_:_text_}  
Display text only if object attribute is false.

{if _object.attribute=value_:_text_}  
Display text only if an object attribute equals a certain value.

{if _object.attribute\<\>value_:_text_}  
Display text only if an object attribute does not equal a certain value.

{if _object.attribute\>value_:_text_}  
Display text only if an object attribute is greater than a certain value.

{if _object.attribute\>=value_:_text_}  
Display text only if an object attribute is greater than or equal to a certain value.

{if _object.attribute\<value_:_text_}  
Display text only if an object attribute is less than a certain value.

{if _object.attribute\<=value_:_text_}  
Display text only if an object attribute is less than or equal to a certain value.

Additional text adventure commands
----------------------------------

{object:_name_}  
Displays an object hyperlink, using the object's display alias.

{object:name:link text}  
Displays an object hyperlink, using text you specify.

{exit:_name_}  
Displays an exit hyperlink. The name is the name you give to the exit (by default exits do not have names; you will need to give it a name yourself). The link will appear as the exit's alias ("north", "up", etc.)

{command:_command_}  
Displays a link that will run a command, _comm_, with the command as the text seen. The command will be parsed as normal, so could be as simple as {command:help} or as complicated as {command:put the ball in the chest}.

{command:_command_:_text_}  
Displays a link that will run a command, as before, but displaying some different text.

{rndalt:_object_}  
Display a randomly chosen name from an object's [alt](attributes/alt.html) list.

{if _attribute_:_text_}  
Display text only if game attribute is true

{if not _attribute_:_text_}  
Display text only if game attribute is false

{select:_object.attribute_:_text 0:text 1:text 2_}  
Selects one text to display, based on the value of the object attribute (you can have as many sections as you like). Note that the attribute must be an integer (whole number), and the sections number from zero.



Additional text adventure commands in Quest 5.7
-----------------------------------------------

{i:_text_}
Displays the given text in italic.

{b:_text_}
Displays the given text in bold. To do bold and italic, nest the commands, like this: {b:{i:very important}}.

{u:_text_}
Displays the given text in underline.

{s:_text_}
Displays the given text in strike-through.


{colour:_colour_:_text_}
Displays the given text in the colour specified (you can also used "color", by the way).

{back:_colour_:_text_}
Displays the given text with the colour specified as the background. To show text as white on black, you can combine these like this: {colour:white:{back:black:some highlighted text}}.


{here _object_:_text_}
Displays the text only if the given object is in the current room (but not if in the player's inventory or in a container in the room).

{nothere _object_:_text_}
Displays the text only if the given object is NOT in the current room.

{popup:_text_:_long text_}
Displays a link, withthe first text. When the player clicks on the link, a pop-up will be displayed, containing the long text. The pop-up will disappear when the long text is clicked on. This can be used with the img command to have an image pop-up.


{either _condition_:_text_}
This works similar to the if command above, but with two important differences. The first is the the condition can be any Quest code that results in a Boolean (true or false). The second is that if you are comparing a string it needs to be in double quotes (as is true of normal Quest code).

{either _condition_:_text_:_text_}
As before, but the second text is only seen when the condition fails.
```
"You {either StartsWith(player.name, \"play\") and not player.flag:are the player}"
 -> "You are the player",
"'Oh, {either player.male_flag:he:she} is not worth it.'"
 -> "'Oh, he is not worth it.'",
```

{eval:_code_}
The code is evaluated, just as normal Quest code is, and the result displayed.

{=_code_}
This is a short cut for eval, and works just the same. The samples below show the potential, though by its nature this is rather less forgiving that the other commands available.
```
"You are in the {eval:player.parent.name}"
 -> "You are in the kitchen"
"You are in the {=player.parent.name}"
 -> "You are in the kitchen"
"You are in the {=CapFirst(player.parent.name)}"
 -> "You are in the Kitchen"
"There are {=ListCount(AllObjects())} objects"
-> "There are 6 objects"
"You look out the window: {=LookOutWindow}"
 -> "You look out the window: A figure is moving by the bushes"
```
 



Additional gamebook commands
----------------------------

{page:_name_}  
Displays a page hyperlink (displaying the page name)

{page:_name_:_link text_}  
Displays a page hyperlink (using text you specify)

{counter:_countername_}  
Displays the value of an counter

{if _flag_:_text_}  
Display text only if flag is set

{if not _flag_:_text_}  
Display text only if flag is not set

{if _countername=value_:_text_}  
Display text only if a counter equals a certain value.

{if _countername\>value_:_text_}  
Display text only if a counter is greater than a certain value.

{if _countername\>=value_:_text_}  
Display text only if a counter is greater than or equal to a certain value.

{if _countername\<value_:_text_}  
Display text only if a counter is less than a certain value.

{if _countername\<=value_:_text_}  
Display text only if a counter is less than or equal to a certain value.


Curly braces
------------

Should you want to use curly braces to actually display curly braces, Quest will usually work out that that is what you want. As of Quest 5.7, if you find it is trying to display it as a text processor command (or is throwing an error because it has failed to), you can use `@@@open@@@` and `@@@close@@@` to tell Quest to display curly braces.
```
"player.count = @@@open@@@player.count@@@close@@@"
 -> "player.count = {player.count}"
