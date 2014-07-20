---
layout: index
title: Hs-multiple
---

Multiple Options using Switch
=============================

*The Switch command allows you to set lots of different options for a user entering values. In this example I have set this up for dialling telephone numbers*

1. I have made an object called telephone and added a Verb called Dial:

     [[File:hsmultiplel1.jpg]]

2. I then add a **Print Message** Script to the Verb to give a message with clear instructions:

     [[File:hsmultiplel2.jpg]]

3. Next Add the verb to the **Display verbs**:

     [[File:hsmultiplel3.jpg]]

4. Next I add a Command to the Room. The reason I do this is because I can “Dial” the number any time at all.

5. I click on Room and then press **Add** and **Command**:

     [[File:hsmultiplel4.jpg]]

6. In the empty box below I type in **Dial \#text\#**

     [[File:hsmultiplel5.jpg]]

7. This means Dial followed by whatever number you want to add in (the variable) eg Dial 999

8. Move down to Script and choose the **Switch…** command and type in the variable name “**text**” in the line:

     [[File:hsmultiplel6.jpg]]

9. In the Cases box I click **Add** and enter the first number you want to dial eg:

     [[File:hsmultiplel7.jpg]]

10. Click **OK** and select **Print a message**:

     
     [[File:hsmultiplel8.jpg]]

11. Type in the message you want to have when you type in “999”

     
     [[File:hsmultiplel9.jpg]]

12. Close the message to save it:

     [[File:hsmultiplel10.jpg]]

13. Repeat this for lots of different numbers eg

     [[File:hsmultiple11.jpg]]

15. When you are finished, click **Default** and choose **Print a message** and write something like:

     [[File:hsmultiplel12.jpg]]

16. This means that any number not on the list will get this message.

17. Here is the final list:

     [[File:hsmultiplel13.jpg]]

18. Here is how it looks with different options in action:

     [[File:hsmultiplel14.jpg]]
