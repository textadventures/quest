---
layout: index
title: Handling ASK/TELL
---


Ask/Tell
--------

Finally, the player can specify both the person to talk to and the subject:

> ASK BORIS ABOUT KEY

> TELL MARY ABOUT LOCK

You should also consider whether you want both sides of the conversation in the output. Which you chose is up to you, but it will look better if you are consistent, so I suggest decided now, before you start typing.

> \>TALK TO BORIS
>
> 'Hi,' you say to Boris, 'can you help me find the key to this door?'
>
> 'Sure, you need to look in the bedroom.'

> \>TALK TO BORIS
>
> 'Hi,' says Boris, 'do you need to find the key to this door? You need to look in the bedroom.'




ASK ABOUT
---------

In play this might look like this:

> \>ASK BORIS ABOUT KEY
>
> 'Hi,' you say to Boris, 'can you help me find the key to this door?'
>
> 'Sure, you need to look in the bedroom.'

This is the most open-ended approach, though in reality that flexibility is an illusion - the player is limited to asking about only the topics you have included, he just does not know it. This is also the major downfall of this method - the player might not guess the subjects you have coded for, and just get frustrated as the character fails to respond to guess after guess. This system is probably best suited to when you have a large number of topics you will include for every character in the game, as a menu will soon get unwieldly, and thematically, it feels right for a mystery.

Quest has a tab just for Ask/Tell, making it easy to set up each response (but you need to activate in on the Features tab of the game object).

![](images/Talk3.png "Talk3.png")

For each topic, click on add, then put in the topic name. You can put in several; try to guess what words the player will use. They should be separated by spaces, as shown above. Then you need to put in the script, just as before.

You should also put in a script to run for unknown topics.

The Quest Ask/About system is discussed in detail [here](ask_about.html)


Varying Responses
-----------------

In all the above examples, the character will respond the same every time. That is not ideal, people are not like that, they will get annoyed if you ask the same question fifteen times, and wonder why you are asking where the key is when you have just used it to open the door.

For all the methods above, the important part here is the lines that start "msg". We have gone through the three ways to arrive at them, now we are concentrating on just those lines.

If you have a counter in there, then you could have the response depend on how often the topic has been raised (but remember to set up an integer attribute for the counter). Or use a boolean to flag that the topic has been asked (the version below automatically sets the attribute for you).

Instead of:

        msg ("'Hi,' you say to Philippa, 'can you help me find the key to this door?'")
        msg ("'Sure, you need to look in the bedroom.'")

Try:

        if (not GetBoolean (philipa, "key_asked") {
          msg ("'Hi,' you say to Philippa, 'can you help me find the key to this door?'")
          msg ("'Sure, you need to look in the bedroom.'")
          philipa.key_asked = true
        }
        else {
          msg ("'Hi again,' you say to Philippa, 'can you help me find the key to this door?'")
          msg ("'Have you tried looking in the bedroom?'")
        }

Or it could respond to changes in the world. The code below will make Philippa respond differently if the door is unlocked, or you are carrying the key.

        if (not that_door.locked) {
          msg ("'I see you finally managed to unlock the door then,' said Philippa sarcastically.'")
        }
        else if (key.parent = player) {
          msg ("'You found the key then,' said Philippa sarcastically.'")
        }
        else if (not GetBoolean (philippa, "key_asked")) {
          msg ("'Hi,' you say to Philippa, 'can you help me find the key to this door?'")
          msg ("'Sure, you need to look in the bedroom.'")
          philippa.key_asked = true
        }
        else {
          msg ("'Hi again,' you say to Philippa, 'can you help me find the key to this door?'")
          msg ("'Have you tried looking in the bedroom yet?'")
        }

