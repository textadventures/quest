---
layout: index
title: Hs-appearingobjects
---

Objects appearing (inside other objects)
========================================

***Problem:*** *In my game I have a newspaper. I want to read the newspaper and when I get to Page 3 a map is going to appear.*

***Answer:***

1. I have set up a room with a newspaper and a hidden room where the map is:

     
     [[File:hsobjappear1.jpg]]

2. I don’t need to make the newspaper a container (although you can do)

3. I go to **Verbs** menu and click the **+ Add** sign and make a “Read” verb

     [[File:hsobjappear2.jpg]]

4. Next I select **Run a script** instead of **Print Message**

     [[File:hsobjappear3.jpg]]

5. I then add in some messages to make the game a bit more interesting (notice I use the **After xxx seconds run script** command to make it more interesting)

     [[File:hsobjappear4.jpg]]

6. When I get to Page 4 I add in my message and use a **Move** script to move the map to the room:

     [[File:hsobjappear5.jpg]]

7. Then I add in another message but change it to an **expression** by clicking the drop down:

     [[File:hsobjappear6.jpg]]

8. I then type in the expression: **"There is a " + ObjectLink ( Map ) +" in the newspaper."**

     
     [[File:hsobjappear7.jpg]]

9. This puts in the object in the message as a hyperlink (like when you open a container to see what is inside. The object name is in the ( )

10. Next I go to the **Object Tab** and **Display Verbs**.

11. I click the **+ Add** button and add in “Read” to go in the drop down so I can “Read” the newspaper

     [[File:hsobjappear8.jpg]]
     
     [[File:hsobjappear9.jpg]]

12. When I run the game, this happens:

     [[File:hsobjappear10.jpg]]

13. The **Map** object is clickable and you can Take it or add a description etc.

14. To make your game more exciting you could give people a choice when they get to Page 4 using an IF function and Yes/No answer box (see other helpsheet eg)

     [[File:hsobjappear11.jpg]]

     [[File:hsobjappear12.jpg]]
