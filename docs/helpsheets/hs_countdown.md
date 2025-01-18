---
layout: index
title: Creating a Countdown Timer
---

*There used to be a TV program called the Crystal Maze where a team had to recover crystals from different rooms by doing certain challenges. Each person had to do a challenge and the difficult bit was that they only had 5 minutes to do it before the room was locked.*

Important note: in Quest 5.3 the object 'player' is replaced with 'game.pov' so if you are working with Q 5.3 or higher replace 'player' with 'game.pov'

1. I have set up two rooms in Quest. One room is called **Dungeon** where the challenge is set.

In the room are lots of different objects:

![](hscountdown1.jpg)

2. The first thing I need to do is make a Countdown variable and set it to the time I need

3. I highlight the player and go to **Attributes** and click the **Add** symbol.

4. I give the Attribute a single word name eg: **countdown** (CASE SENSITIVE!)

     ![](hscountdown2.jpg)

5. When I click **OK** it appears at the bottom:

     ![](hscountdown3.jpg)

6. I now need to give the countdown variable a value.

7. I click on the **String** drop down box and choose **Integer** and type the number of seconds I want the clock to run for in the box:

     ![](hscountdown4.jpg)

8. When you click off you see that the **attribute** now says 30 next to it:

     ![](hscountdown5.jpg)

9. Now I need to add a label so the value appears when you play.

10. I go up to **status attributes** and click the **Add** button and type in the name of the attribute: (CASE SENSITIVE!)

     ![](hscountdown6.jpg)

11. When I click OK I am asked for a Format string. You can leave this blank and the computer will put in countdown: 30

12. If you want to have a different value, type in the words you want and put a ! where the seconds will go eg

     
     ![](hscountdown7.jpg)

13. This would look like this when the game runs:

     ![](hscountdown8.jpg)

14. When you click **OK** you will see the different attributes:

     
     ![](hscountdown9.jpg)

15. The next step is to add the **Timer**

16. I do this by going to **Add \> Timer** and giving it a name:

     ![](hscountdown10.jpg)

17. Next I need to tell the computer to start on 30s and countdown in steps of 1.

18. I do this by clicking on **Script** typing in **variable** and choosing **Set a variable or attribute**:

     ![](hscountdown11.jpg)
     

19. I double click on the script to choose it and it looks like this:

     ![](hscountdown12.jpg)

20. I now CAREFULLY type in the name of my variable – **player.countdown** (Q5.3: game.pov.countdown)

     ![](hscountdown13.jpg)

21. In the **expression** box I type in the same variable but then with MINUS 1 eg **player.countdown -1** (Q5.3: game.pov.countdown -1)

     ![](hscountdown14.jpg)

22. This basically tells the counter to countdown from 30 it steps of 1

23. Now I add in another script using IF to say what happens if the counter gets to 0

     ![](hscountdown15.jpg)

24. Next we need to tell the computer to start the timer when we go into the **Dungeon**.

25. Click on Dungeon then **Scripts** and find the **After entering room** section.

26. Add a script and search for **Enable**. Enable **Timer1** to start:

     ![](hscountdown16.jpg)

27. Next we need to tell the computer to stop the time when the **Dungeon**.

28. Click in the **Dungeon** room and select scripts.

29. Find the **After leaving room** section and make up these scripts:

     ![](hscountdown17.jpg)

30. This tells the computer to stop the timer running, write a message and then “reset” it back to 30 when you leave the **Dungeon**

31. Run the game. When you go into the room, the timer starts automatically:

     ![](hscountdown18.jpg)

32. Here is the timer counting down some more

     ![](hscountdown19.jpg)

33. Here is the game finished when I did not get out in time

     ![](hscountdown20.jpg)

34. Here is when I get out of the room and go back to the other room – the timer resets to 30s

     ![](hscountdown21.jpg)

35. If you wanted the timer on for the entire game you would go back to the timer and tick the **start timer when game begins** box

     
     ![](hscountdown22.jpg)

**I AM SURE THERE ARE LOTS OF OTHER THINGS YOU CAN DO WITH THIS CODE!**
