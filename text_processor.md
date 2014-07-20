---
layout: index
title: Text processor
---

Quest 5.4 introduces a text processor, giving an easy way to conditionally print text, show object lnks, show text only once, and more.

To use the text processor, you can simply add a command in curly braces in any text that gets displayed. For example, in an [msg](scripts/msg.html) command:

     msg ("Would you like some {command:help}?")

or in a room description:

     <description>This is a small dungeon. {once:There is a bad smell in here.}</description>

You can use as many sections as you like within the same text, and even nest them:

     msg ("You can {command:go to shop:go into the shop}. {if player.coins>10:You have {player.coins} coins, which is more than enough.}")

Supported processor commands are:

Text adventure mode and Gamebook mode
-------------------------------------

{once:text}  
Displays the text only once. The text will not be printed on subsequent occasions.

{random:text 1:text 2:text 3}  
Choose text at random

{img:filename.png}  
Insert the specified image

{object.attribute}  
Displays the value of an object attribute

{if object.attribute:text}  
Display text only if object attribute is true

{if not object.attribute:text}  
Display text only if object attribute is false

{if object.attribute=value:text}  
Display text only if an object attribute equals a certain value.

{if object.attribute\<\>value:text}  
Display text only if an object attribute does not equal a certain value.

{if object.attribute\>value:text}  
Display text only if an object attribute is greater than a certain value.

{if object.attribute\>=value:text}  
Display text only if an object attribute is greater than or equal to a certain value.

{if object.attribute\<value:text}  
Display text only if an object attribute is less than a certain value.

{if object.attribute\<=value:text}  
Display text only if an object attribute is less than or equal to a certain value.

Additional text adventure commands
----------------------------------

{object:name}  
Displays an object hyperlink (using the object's display alias)

{object:name:link text}  
Displays an object hyperlink (using text you specify)

{object:name:link text}  
Displays an object hyperlink (using text you specify)

{exit:name}  
Displays an exit hyperlink (Text adventure mode only)

{command:help}  
Displays a link that will run a command

{command:help:Need assistance?}  
Displays a link that will run a command, with the link displaying some different text

{rndalt:object}  
Display a randomly chosen name from an object's [alt](attributes/alt.html) list.

{if attribute:text}  
Display text only if game attribute is true

{if not attribute:text}  
Display text only if game attribute is false

Additional gamebook commands
----------------------------

{page:name}  
Displays a page hyperlink (displaying the page name)

{page:name:link text}  
Displays a page hyperlink (using text you specify)

{counter.countername}  
Displays the value of an counter

{if flag:text}  
Display text only if flag is set

{if not flag:text}  
Display text only if flag is not set

{if countername=value:text}  
Display text only if a counter equals a certain value.

{if countername\>value:text}  
Display text only if a counter is greater than a certain value.

{if countername\>=value:text}  
Display text only if a counter is greater than or equal to a certain value.

{if countername\<value:text}  
Display text only if a counter is less than a certain value.

{if countername\<=value:text}  
Display text only if a counter is less than or equal to a certain value.


