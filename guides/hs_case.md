---
layout: index
title: The CASE Command
---

*Using the CASE command is a much quicker alternative to lots of “nested” IF scripts. This example shows you how to set up a phone where you can dial different numbers and get different responses*

1. I have made an object called telephone and added a Verb called **Start Dialling**:

     
     ![](Hscase1.jpg)

2. I then add a *'Print Message* Script to the Verb to give a message with clear instructions:

     ![](Hscase2.jpg)

3. Next Add the verb to the **Display verbs**:

     
     ![](Hscase3.jpg)

4. Next I add a Command to the Room. The reason I do this is because I can “Dial” the number any time at all.

5. I click on Room and then press **Add** and **Command**:

     
     ![](Hscase4.jpg)

6. In the empty box below I type in **Dial \#text\#**

     ![](Hscase5.jpg)

7. This means Dial followed by whatever number you want to add in (the variable) eg Dial 999

8. Move down to Script and choose the **Switch…** command and type in the variable name “**text**” in the line:

     ![](Hscase6.jpg)

9. In the Cases box I click **Add** and enter the first number you want to dial eg:

     ![](Hscase7.jpg)

10. Click **OK** and select **Print a message**:

     ![](Hscase8.jpg)

11. Type in the message you want to have when you type in “999”

     ![](Hscase9.jpg)

12. Close the message to save it:

     ![](Hscase10.jpg)

13. Repeat this for lots of different numbers eg

     ![](Hscase11.jpg)

14. When you are finished, click **Default** and choose **Print a message** and write something like:

     ![](Hscase12.jpg)

15. This means that any number not on the list will get this message.

16. Here is the final list:

     
     ![](Hscase13.jpg)

17. Here is how it looks with different options in action:

     ![](Hscase14.jpg)
