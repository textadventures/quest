---
layout: index
title: Hs-baddy1
---

Adding Baddies who want to Kill or Eat you!
===========================================

*What happens if you go into a room or open an object and there is a baddy who wants to kill you?*

1. I am going to add in a cupboard in a room where an alien is hiding!

2. I add the cupboard and alien objects.

3. Because the alien is inside the cupboard I need to make the cupboard a container.

4. I go to the Container tab. Choose “Container” from the type list, and **untick** the “Is open” box so that the cupboard is closed when the game begins.

     
     [[File:Hsbaddy11.jpg]]

5. I now want to run a script when the player opens the object.

6. We will tell the player they’ve surprised the sleeping (and hungry) alien, give them 10 seconds to get rid of the alien before it kills them.

7. To do this, scroll down to “**After opening the object**”, and add a “**Print a message**” script.

     
     [[File:Hsbaddy12.jpg]]

8. Next I add another script – from the **Timers** section, choose “**Run a script after a number of seconds**”.

     
     [[File:Hsbaddy13.jpg]]

9. I can choose how many seconds to wait before something else happens. In this case, 10 seconds.

10. After 10 seconds, we want to see if the “alien” object is still visible. If so, print a message and kill the player. If not, we don’t need to do anything.

11. So, what we need to do is add an “**If**” inside the “After 10 seconds” script, as shown below:

     
     [[File:Hsbaddy14.jpg]]

12. If the alien is still visible (it has not been killed) then there is a message that says it eats us.

13. Finally we add in a **Finish the game** command to end the game!

14. Now we need to find a way to kill the alien

15. Let’s add a flame thrower object so that when the player uses the flame thrower on the alien, the alien bursts into flames (and disappears)

16. We add an object called “flame thrower” in the room.

17. We then go to the “\*Use/Give\*” tab scroll down to “**Use this on (other object)**”.

18. We choose Select “**Handle objects individually**”, add “alien”, and then edit the script.

19. We choose Add a “**print a message**” command to say something to the player eg “The alien explodes in a large fireball”

20. We then add a “**Remove object**” command to remove the alien from play.

21. Here is what the script looks like this:

     
     [[File:Hsbaddy15.jpg]]

22. If we make the alien disappear then nothing will happen after the 10 seconds!

23. So when the player opens the cupboard, if they use the flame thrower on the alien, the alien will no longer be visible in the room. However, if the player has not used the flame thrower, the alien will still be visible, in which case the alien enjoys a tasty meal.

''This helpsheet is adapted from one written by Alex Warren from <http://www.textadventures.co.uk/blog/2012/02/27/time-limited-puzzles/> ''
