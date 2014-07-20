---
layout: index
title: Multiple choices - using a switch script
---

A "switch" is like an "if" script, but it caters for more options without you having to to add a load of "else if" entries.

In this example, we'll add a telephone to the lounge and set up a "dial" command. The player will be able to dial a variety of different numbers:

-   999 will print the message "There's no need to call the police now."
-   94672 will print the message "Madame Buxom, Queen of Pain, is on her way."
-   32461 will print the message "A voice at the end of the line says 'Stop calling this number you pervert!'"
-   15629 will print the message "You’re not hungry or drunk enough for anything from Luigi's Pizza Palace right now."
-   Otherwise "Sorry, wrong number!" will be printed

First, add a telephone to the lounge and give it a sensible description. You should also go to the Object tab and give it an "Other name" of "phone". Next, add a command "dial \#text\#". Whenever the player types "dial", the text that follows will be put into the "text" string variable.

In the command script, add "switch". You can now enter a Switch expression, and any number of cases. Here, we want to compare the variable "text" against different values, so the Switch expression is "text". Each of the cases that we add will be compared against this.

Click Add to add the first case. Enter the expression "999".

     

Click OK. The Script Editor will now be displayed, ready for you to enter the script that should run if \#text\# is equal to "999". Print the message "There's no need to call the police now." and close the window.

Follow the same process to enter the responses for the other phone numbers. When you reach the bottom of the list, add script to the "Default" section to print "Sorry, wrong number!". This will handle any other input that wasn't matched by our cases.

When you have finished, the Script Editor should look like this:

![](Switch.png "Switch.png")

This is much easier to read, edit and add to than if we had used lots of "if" and "else if" scripts.

Launch the game and dial a few numbers to check that you see the correct response.

[Next: Debugging your game](debugging_your_game.html)