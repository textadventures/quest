---
layout: index
title: Hs-addingquestion2
---

Adding a Yes/No Question to a Container
=======================================

*A key is inside a box but you have to answer the question correctly to get it – here is how*

1. I have made a **box** container with the key inside it:

     
     [[File:Hsaddingquestion21.jpg]]

2. I make the box a normal container and tick various options:

     
     [[File:Hsaddingquestion22.jpg]]

3. I then add in an IF script on the **After Opening the object**: section

4. I add a **player answers Yes** to script to the **Script to run when opening object**

5. I type in the Yes/No question that they see on the keypad:

6. I put in the answer if they choose “Yes” (the wrong answer)

     
     [[File:Hsaddingquestion23.jpg]]

7. I used the **Game Over** command to end their chances..

8. I then add the section if they say “Yes” (the wrong answer)

     
     [[File:Hsaddingquestion24.jpg]]

Here are the screenshots of what the game now looks like:

     [[File:Hsaddingquestion25.jpg]]  

*The text “It contains a Magic Key” appears at the end*

*You can have a way of making that appear only if they are correct – you do this by moving the object from a “Hidden” room to the box.*

*To do this you need to make a hidden room first and put the key in there then adding a move command. Finally you need to delay the last message coming in using an “Run script after” command*

     [[File:Hsaddingquestion26.jpg]]
