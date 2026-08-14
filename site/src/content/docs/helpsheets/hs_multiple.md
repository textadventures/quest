---
title: Multiple Options using Switch
sidebar:
  order: 14
---

*The Switch command allows you to set lots of different options for a user entering values. In this example I have set this up for dialling telephone numbers*

1. I have made an object called telephone and added a Verb called Dial:

     ![](/images/helpsheets/Hsmultiplel1.jpg)

2. I then add a **Print Message** Script to the Verb to give a message with clear instructions:

     ![](/images/helpsheets/Hsmultiplel2.jpg)

3. Next Add the verb to the **Display verbs**:

     ![](/images/helpsheets/Hsmultiplel3.jpg)

4. Next I add a Command to the Room. The reason I do this is because I can “Dial” the number any time at all.

5. I click on Room and then press **Add** and **Command**:

     ![](/images/helpsheets/Hsmultiplel4.jpg)

6. In the empty box below I type in **Dial \#text\#**

     ![](/images/helpsheets/Hsmultiplel5.jpg)

7. This means Dial followed by whatever number you want to add in (the variable) eg Dial 999

8. Move down to Script and choose the **Switch…** command and type in the variable name “**text**” in the line:

     ![](/images/helpsheets/Hsmultiplel6.jpg)

9. In the Cases box I click **Add** and enter the first number you want to dial eg:

     ![](/images/helpsheets/Hsmultiplel7.jpg)

10. Click **OK** and select **Print a message**:

     
     ![](/images/helpsheets/Hsmultiplel8.jpg)

11. Type in the message you want to have when you type in “999”

     
     ![](/images/helpsheets/Hsmultiplel9.jpg)

12. Close the message to save it:

     ![](/images/helpsheets/Hsmultiplel10.jpg)

13. Repeat this for lots of different numbers eg

     ![](/images/helpsheets/Hsmultiple11.jpg)

15. When you are finished, click **Default** and choose **Print a message** and write something like:

     ![](/images/helpsheets/Hsmultiplel12.jpg)

16. This means that any number not on the list will get this message.

17. Here is the final list:

     ![](/images/helpsheets/Hsmultiplel13.jpg)

18. Here is how it looks with different options in action:

     ![](/images/helpsheets/Hsmultiplel14.jpg)
