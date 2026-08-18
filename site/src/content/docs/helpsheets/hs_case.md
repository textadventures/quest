---
title: The CASE command
sidebar:
  order: 10
---

*Using the CASE command is a much quicker alternative to lots of “nested” IF scripts. This example shows you how to set up a phone where you can dial different numbers and get different responses*

1. I have made an object called telephone and added a Verb called **Start Dialling**:

     
     ![](/images/helpsheets/Hscase1.jpg)

2. I then add a *'Print Message* Script to the Verb to give a message with clear instructions:

     ![](/images/helpsheets/Hscase2.jpg)

3. Next Add the verb to the **Display verbs**:

     
     ![](/images/helpsheets/Hscase3.jpg)

4. Next I add a Command to the Room. The reason I do this is because I can “Dial” the number any time at all.

5. I click on Room and then press **Add** and **Command**:

     
     ![](/images/helpsheets/Hscase4.jpg)

6. In the empty box below I type in **Dial \#text\#**

     ![](/images/helpsheets/Hscase5.jpg)

7. This means Dial followed by whatever number you want to add in (the variable) eg Dial 999

8. Move down to Script and choose the **Switch…** command and type in the variable name “**text**” in the line:

     ![](/images/helpsheets/Hscase6.jpg)

9. In the Cases box I click **Add** and enter the first number you want to dial eg:

     ![](/images/helpsheets/Hscase7.jpg)

10. Click **OK** and select **Print a message**:

     ![](/images/helpsheets/Hscase8.jpg)

11. Type in the message you want to have when you type in “999”

     ![](/images/helpsheets/Hscase9.jpg)

12. Close the message to save it:

     ![](/images/helpsheets/Hscase10.jpg)

13. Repeat this for lots of different numbers eg

     ![](/images/helpsheets/Hscase11.jpg)

14. When you are finished, click **Default** and choose **Print a message** and write something like:

     ![](/images/helpsheets/Hscase12.jpg)

15. This means that any number not on the list will get this message.

16. Here is the final list:

     
     ![](/images/helpsheets/Hscase13.jpg)

17. Here is how it looks with different options in action:

     ![](/images/helpsheets/Hscase14.jpg)

For more on this script, including matching multiple values in a single case and using ranges, see [Multiple choices - using a switch script](/howto/tasks/multiple_choices_using_a_switch_script).
