---
layout: index
title: Handling SPEAK TO
---


When setting up conversations between the player and a character, one option is to implement the SPEAK TO command.

> SPEAK TO BORIS

This could than be handled with a stock response or by offering a list of options (Quest will accept TALK TO as a synonym of SPEAK TO). This is a restricted approach to conversations, compared to [ASK/TELL](ask_about.html), because while the player can choose who to talk to, you are dictating what the conversation is about.


Stock responses
----------------------------

The examples above illustrate this in play.

> \>TALK TO BORIS
>
>‘Hi,’ you say to Boris, ‘can you help me find the key to this door?’
>
>‘Sure, you need to look in the bedroom.’

To set this up in Quest, select the character, go to the verbs tab, and add a new verb, "speak" (Quest will match this up to the right command as you type; if you type in "talk to" if will find "speak to" for you too). Then put in the response underneath. It could look like this:

![](images/Talk1.png "Talk1.png")

Here it is in code, which we will be using from now on!

```
msg ("'Hi,' you say to Boris, 'can you help me find the key to this door?'")
msg ("'Sure, you need to look in the bedroom.'")
```



Varying Responses
-----------------

In the above example, the character will respond the same every time. That is not ideal, people are not like that, they will get annoyed if you ask the same question fifteen times, and wonder why you are asking where the key is when you have just used it to open the door.

If you have a counter in there, then you could have the response depend on how often the topic has been raised (but remember to set up an integer attribute for the counter). Or use a Boolean to flag that the topic has been asked. In the example below a Boolean is used, but as we use `GetBoolean` we do not need to set it before hand (as `GetBoolean` returns `false` if it is not set).

Instead of:

```
msg ("'Hi,' you say to Boris, 'can you help me find the key to this door?'")
msg ("'Sure, you need to look in the bedroom.'")
```

Try:

```
if (not GetBoolean (boris, "key_asked") {
  msg ("'Hi,' you say to Boris, 'can you help me find the key to this door?'")
  msg ("'Sure, you need to look in the bedroom.'")
  boris.key_asked = true
}
else {
  msg ("'Hi again,' you say to Boris, 'can you help me find the key to this door?'")
  msg ("'Have you {i:tried} looking in the bedroom?'")
}
```

Or it could respond to changes in the world. The code below will make Boris respond differently if the door is unlocked, or you are carrying the key.

```
if (not that_door.locked) {
  msg ("'I see you finally managed to unlock the door then,' says Boris sarcastically.'")
}
else if (key.parent = player) {
  msg ("'You found the key then,' says Boris sarcastically.'")
}
else if (not GetBoolean (boris, "key_asked")) {
  msg ("'Hi,' you say to Boris, 'can you help me find the key to this door?'")
  msg ("'Sure, you need to look in the bedroom.'")
  boris.key_asked = true
}
else {
  msg ("'Hi again,' you say to Boris, 'can you help me find the key to this door?'")
  msg ("'Have you {i:tried} looking in the bedroom yet?'")
}
```


A menu of responses
---------------------------

In play this might look like this:

> \>TALK TO BORIS
>
> 1. Where is key
>
> 2. Who is the Queen
>
> 3. How do I defeat the troll
>
> \>1
>
> 'Hi,' you say to Boris, 'can you help me find the key to this door?'
>
> 'Sure, you need to look in the bedroom.'

Quest handles menus well, and it would actually look rather more slick than that.

As this is again using the "TALK TO" command, setting it up is like the first option. However, the script for that "SPEAK" verb is rather more complicated.

![](images/Talk2.png "Talk2.png")

In code view, it looks like this:

```
topics = Split ("Where is key;Who is the Queen;How do I defeat the troll", ";")
ShowMenu ("Talk to Boris about...", topics, true) {
  switch (result) {
    case ("Where is key") {
      msg ("'You need to look in the bedroom.'")
    }
    case ("Who is the Queen") {
      msg ("'Just some girl.'")
    }
    case ("How do I defeat the troll") {
      msg ("'Use fire to stop it regenerating.'")
    }
  }
}
```

The first line of the script sets up the topics. They need to be a string list, and the easiest way to create one is to use the Split function. Each topic is separated by a semi-colon, with no spaces.

A switch statement is used to decide which response will be seen. Note that the key for each case must be exactly the same as the topic you listed before. The script for each case is set up just as the script for the first option.


Menu and varying?
-----------------

We can combine the last two, giving the player a menu to pick from, but having the character give a response depending on game state. In this example, it is only done for the first option. All we have done is inserted the code from "Varying Responses" into the above.

```
topics = Split ("Where is key;Who is the Queen;How do I defeat the troll", ";")
ShowMenu ("Talk to Boris about...", topics, true) {
  switch (result) {
    case ("Where is key") {
      if (not that_door.locked) {
        msg ("'I see you finally managed to unlock the door then,' says Boris sarcastically.'")
      }
      else if (key.parent = player) {
        msg ("'You found the key then,' says Boris sarcastically.'")
      }
      else if (not GetBoolean (boris, "key_asked")) {
        msg ("'Hi,' you say to Boris, 'can you help me find the key to this door?'")
        msg ("'Sure, you need to look in the bedroom.'")
        boris.key_asked = true
      }
      else {
        msg ("'Hi again,' you say to Boris, 'can you help me find the key to this door?'")
        msg ("'Have you {i:tried} looking in the bedroom yet?'")
      }
    }
    case ("Who is the Queen") {
      msg ("'Just some girl.'")
    }
    case ("How do I defeat the troll") {
      msg ("'Use fire to stop it regenerating.'")
    }
  }
}
```

This does have the potential to become complex, but that is the nature of the beast. The more complex, the more natural it will seem to the player. It will need thorough testing, however!