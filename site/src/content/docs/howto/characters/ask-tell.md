---
title: Building an Ask/Tell system
description: Set up Ask/Tell topics on a character, understand how topics are matched, and add TOPICS, ASK ABOUT and ASK commands
---

Ask/Tell lets the player question characters about anything they like - `ASK MARY ABOUT DR BLACK`, `TELL MARY ABOUT THE POISON` - and give them orders, like `TELL MARY TO DANCE`. It suits mysteries and investigations, where working out what to ask is part of the puzzle. This page covers setting up topics, exactly how Quest Viva matches what the player types against them, what happens when nothing matches, and some commands you can add to make topics easier to find.

If you're still deciding how the player should talk to your characters, start with [Talking to characters](/howto/characters/talking).

## Turning on Ask/Tell

Select the game in the tree, go to the _Features_ tab and tick "Ask/Tell: players can ask or tell characters about selected topics". Every object then has an _Ask/Tell_ tab, with three lists:

| List | Handles |
|---|---|
| "Ask about" | `ASK MARY ABOUT ...` |
| "Tell about" | `TELL MARY ABOUT ...` |
| "Tell to" (under "Order") | `TELL MARY TO ...`, `ASK MARY TO ...` and `MARY, ...` |

All three work the same way, so everything below applies to each of them.

## Adding topics

Select the character and go to the _Ask/Tell_ tab. Type the topic's keywords into the "Add entry key..." box under "Ask about", separated by spaces - for example `dr doctor black murder` - and click "Add". Then add the script to run when the player asks about it:

```quest
msg ("'Me? I know nothing about the murder!'")
```

To change a topic's keywords later, select it and click "Edit Key".

In code, the topics are a script dictionary attribute called `ask` (or `tell`, or `tellto`), where each key is a set of keywords and each value is the script. Inside the script, `this` is the character.

## How topics are matched

When the player types `ASK MARY ABOUT THE BLACK CAT`, Quest Viva takes the subject - "the black cat" - and scores every topic on Mary's "Ask about" list against it:

- The subject is split into words, and so are the topic's keywords.
- A word matches a keyword if the word **begins with** the keyword. Upper and lower case don't matter.
- Each match adds the length of the keyword to the topic's score. Words that don't match anything, like "the", are ignored.
- The topic with the highest score wins. If two topics have the same score, the one further down the list wins.

So with the keywords `dr doctor black murder`, all of these find the same topic:

```
> ASK MARY ABOUT DR BLACK
> ASK MARY ABOUT DOCTOR BLACK
> ASK MARY ABOUT THE MURDER
```

This has some consequences worth knowing:

- **Short keywords match a lot.** `dr` matches "drinks" and "dress" too, because they begin with "dr". Keep keywords reasonably distinctive.
- **Because matching is by the start of the word, you get some plurals and variations for free.** `report` matches "reports", and `black` matches "blackmail" - which may not be what you want.
- **Longer keywords outscore shorter ones.** If Mary has `dr doctor black murder` and `cat`, then `ASK MARY ABOUT THE BLACK CAT` gets the murder topic, because "black" (5 letters) beats "cat" (3). To make sure the cat topic wins, give it the keywords `black cat` - it then scores 8.

## Unknown topics

If no topic matches, the character "does not reply" (for "Tell to", "does nothing"). To say something better, expand the "Advanced" section at the bottom of the _Ask/Tell_ tab and fill in "Script to run when asked about an unknown topic". There's one for each list - "Script to run when told about an unknown topic" and "Script to run when keywords not recognised".

The variable `text` holds what the player asked about, converted to lower case:

```quest
msg ("Mary shrugs. 'I know nothing about " + text + ".'")
```

```
> ASK MARY ABOUT PIES
Mary shrugs. 'I know nothing about pies.'
```

Players tend to try the same topics on every character, so it's worth giving each character a sensible reply to topics they don't know about - especially the ones that matter to the plot.

## Varying the replies

A topic's script can check anything in the game, so a character can say something different the second time you ask, or once you've found some evidence. See [Varying what characters say](/howto/characters/talking#varying-what-characters-say) - everything there works in an Ask/Tell topic.

## Starting a conversation from a topic

A topic's script can start a [Pages](/howto/characters/pages) conversation, so that asking about something leads into a dialogue tree. Add "Show a page" from the Pages category, or in code:

```quest
ShowPage (mary_heart, true, false)
```

`ASK MARY ABOUT HER HEART` then shows the `mary_heart` page and its options. See [Combining approaches](/howto/characters/talking#combining-approaches) for how this fits with a main conversation started by `TALK TO`.

## Helping the player find topics

Guessing topics can be frustrating. The commands in this section make it easier. To add one, select "Commands" in the tree, click "+ Add" and choose "Add Command", then enter the pattern and paste in the script.

All three use a `topics` attribute on each character: a string list of the subjects that character can talk about, written the way the player should see them. Add it on the character's _Attributes_ tab, as a "String List" - for Mary, say, "Dr Black" and "the lab report". Each one needs to match one of her topics' keywords.

As the plot develops, add to the list:

```quest
list add (Mary.topics, "the poison bottle")
```

### A TOPICS command

With the pattern `topics`, this lists what each character in the room can be asked about:

```quest
found = false
foreach (o, GetDirectChildren(game.pov.parent)) {
  if (HasAttribute(o, "topics")) {
    if (ListCount(o.topics) > 0) {
      msg ("You could ask " + GetDisplayName(o) + " about " + FormatList(o.topics, ", ", " or ", "") + ".")
      found = true
    }
  }
}
if (not found) {
  msg ("There's no one here to ask about anything.")
}
```

```
> TOPICS
You could ask Mary about Dr Black or the lab report.
You could ask Bob about the weather.
```

### ASK ABOUT without a character

With the pattern `ask about #text#`, the player can leave out who they're asking. If only one character is in the room, they're asked; if there are several, a menu asks who:

```quest
people = NewObjectList()
foreach (o, GetDirectChildren(game.pov.parent)) {
  if (HasAttribute(o, "ask")) {
    list add (people, o)
  }
}
if (ListCount(people) = 0) {
  msg ("There's no one here to ask.")
}
else if (ListCount(people) = 1) {
  DoAskTell (ObjectListItem(people, 0), text, "ask", "askdefault", "DefaultAsk")
}
else {
  names = NewStringDictionary()
  foreach (o, people) {
    dictionary add (names, o.name, GetDisplayName(o))
  }
  who = ShowMenu("Ask who?", names, true)
  if (who <> "") {
    DoAskTell (GetObject(who), text, "ask", "askdefault", "DefaultAsk")
  }
}
```

A character that has "Ask about" topics has an `ask` attribute, so that's what the first part looks for. `DoAskTell` is the function the built-in `ASK` command uses: it matches the text against the character's topics and runs the right script, or the unknown-topic script. For `TELL ABOUT`, use `"tell", "telldefault", "DefaultTell"` instead.

`ShowMenu` needs a list of strings or a string dictionary rather than a list of objects, so the menu shows each character's display name and returns their object name. If the player types something else instead of choosing, `ShowMenu` returns an empty string and nothing happens.

### ASK a character

With the pattern `ask #object#`, `ASK MARY` offers a menu of Mary's topics:

```quest
if (not HasAttribute(object, "topics")) {
  msg (CapFirst(GetDisplayName(object)) + " has nothing to tell you.")
}
else {
  topic = ShowMenu("Ask " + GetDisplayName(object) + " about...", object.topics, true)
  if (topic <> "") {
    DoAskTell (object, topic, "ask", "askdefault", "DefaultAsk")
  }
}
```

The chosen topic's text is matched against Mary's keywords just as if the player had typed it, so "the lab report" finds the topic with the keywords `lab report`. `ASK MARY ABOUT ...` still works as normal alongside this command.

These menus use `ShowMenu()`, so the player can't save while one is waiting. For a quick choice like this that's rarely a problem - see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting) if it matters.

## See also

- [Talking to characters](/howto/characters/talking)
- [Building a conversation with Pages](/howto/characters/pages)
- [Custom commands](/tutorial/custom-commands)
