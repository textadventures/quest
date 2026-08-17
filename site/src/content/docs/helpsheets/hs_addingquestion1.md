---
title: Adding a yes/no quiz question
sidebar:
  order: 8
---

*Here is a way of creating a quiz style situation*

1. I have set up a room with player and keypad and Exit to the “Outside”

     ![](/images/helpsheets/Hsaddingquestion1.jpg)

2. I then lock the “Outside” exit:

     ![](/images/helpsheets/Hsaddingquestion2.jpg)

3. I make the keypad into a container which can be opened and closed:

     ![](/images/helpsheets/Hsaddingquestion3.jpg)

4. I add a **player answers Yes to** script to the **Script to run when opening object**

5. I type in the Yes/No question that they see on the keypad:

     ![](/images/helpsheets/Hsaddingquestion4.jpg)

6. I put in the answer if they choose “Yes” (the correct answer)

     ![](/images/helpsheets/Hsaddingquestion5.jpg)

7. I then add the section if they say “No” (the wrong answer)

     ![](/images/helpsheets/Hsaddingquestion6.jpg)

Here are the screenshots of what the game now looks like:

![](/images/helpsheets/Hsaddingquestion7.jpg)

### Gating a container

The same pattern works for a container instead of an exit - for example, a key hidden inside a box that only opens once the right answer is given:

![](/images/helpsheets/Hsaddingquestion21.jpg)

Make the box a normal container and tick the appropriate options:

![](/images/helpsheets/Hsaddingquestion22.jpg)

Then add an "If" script to **After Opening the object**, with a "player answers Yes" script attached to **Script to run when opening object**, and the Yes/No question typed in as before. Here, "Yes" is the wrong answer, and it uses **Game Over** to end the game if the player gets it wrong:

![](/images/helpsheets/Hsaddingquestion23.jpg)

![](/images/helpsheets/Hsaddingquestion24.jpg)

Here's what it looks like in play - the text "It contains a Magic Key" appears once the box is opened correctly:

![](/images/helpsheets/Hsaddingquestion25.jpg)

### Delaying the reveal

You can make that text appear only once the player has answered correctly, rather than always being part of the box's description, by keeping the key in a hidden room and moving it into the box on success. Set up a hidden room, put the key there, add a "move object" command to bring the key into the box, then delay the final message with a "Run script after" command so it doesn't appear too abruptly:

![](/images/helpsheets/Hsaddingquestion26.jpg)
