---
title: Asking a question
sidebar:
  order: 9
---

## Open-ended question

Often in a text adventure you want the game to ask an open-ended question of the player. A simple example would be to allow the player to name the character. The `GetInput` function exists to handle just that - in the script editor you will find it as the "player's typed input" template on a "Set a variable or attribute" action.

![](/images/Question1.png)

```quest
msg ("What is your name?")
player.alias = GetInput()
```

The first line just asks the question. Then we see `GetInput()`. This suspends the script until the player has typed a response, and then hands back what they typed, as a string - so we can assign it straight to an attribute, and the next line of the script does not run until the answer is in.

Note that we are setting the "alias" attribute; the "name" attribute cannot be changed during play as Quest Viva uses that to track each object. You can then use the text processor to insert the character's name in text:

```quest
'Hi, {player.alias},' says the oddly-shaped doll.
```

**Note:** A game written for Quest 5 may use the [get input](/scripts#get-input) script command instead, which runs a nested block once the player has answered - that was the way to do this from Quest 5.4 onwards. It still works, and is still editable if a game already contains it, but it is no longer offered when you add a new script command.

## Multiple questions

Asking a second question is just a case of writing a second question:

![](/images/Question2.png)

```quest
msg ("What is your name?")
player.alias = GetInput()
msg ("How old are you?")
player.age = GetInput()
```

The player is asked for her name, the game waits, she answers; only then is she asked her age. You can carry on like that for as many questions as you like.


## Looking for a specific answer

You might want the player to give a specific answer - the answer to a riddle, perhaps. Here is an example script:

![](/images/Question3.png)

```quest
msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
answer = GetInput()
if (answer = "man") {
  msg ("'Is it a man?' you ask.")
  msg ("'How come everyone knows the answer?'")
  msg ("'We have this thing called the internet nowadays...'")
}
else {
  msg ("'Is it \"" + answer + "\"?' you ask.")
  msg ("'No!'")
}
```

The first line just asks the riddle. The second waits for the reply and puts it in a variable called `answer` - you can call that whatever you like. Then we check it is the correct answer and react accordingly.

This will work, but has a couple of issues that we want to resolve.


## Testing input

The first problem is that the player has to type the exact string “man”. What if he types “Man” or “a man”? We might also like to handle “woman” and “human” so the riddle is more politically correct.

To do this, replace the `if` line with this:

```quest
if (IsRegexMatch  ("man", LCase (answer))) {
```

The LCase function will convert the player's text to all lower case, so “Man” will be handled as “man”. This text will then be matched against a Regex (a pattern) rather than a specific string. As long as the player has “man” somewhere in the answer, the pattern will match.

In this case, that will match human and woman too, which is great. Usually multiple answers are not so convenient, but you can do that too - just put them inside brackets and separate each with a vertical bar.

```quest
if (IsRegexMatch  ("(man|lady)", LCase (answer))) {
```

In fact, this will match anything with "man" in the word, so would also match "shaman". We could improve it by using `\b` to match against the word boundary, and then include all the options we will allow (note that you have to "escape" backslashes, so we use two here, `\\b`):

```quest
if (IsRegexMatch  ("\\b(man|lady|woman|human|person)\\b", LCase (answer))) {
```

We could optionally match "a" (the `?` indicates it is optional), and also match the start and end of the string with `^` and `$`:

```quest
if (IsRegexMatch  ("^(a )?(man|lady|woman|human|person)$", LCase (answer))) {
```

There is a section on Regex in the [pattern matching](/howto/commands/pattern-matching) page.


## Changing the prompt

This is worthwhile doing as it makes it clear to the player that he or she should not be typing a command. To get this to work, you need to use some JavaScript! 
```quest
JS.eval("$('#txtCommand').attr('placeholder', 'Your answer');")
```
It might be worth also turning off the panes on the right, to stop the player messing with them when she should be answering the question:
```quest
JS.panesVisible(false)
```
Remember to set them back to normal after.


## Altogether now...

Here is the full the script, doing all we have discussed:

![](/images/Question4.png)

```quest
msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
JS.eval("$('#txtCommand').attr('placeholder', 'Your answer');")
JS.panesVisible(false)
answer = GetInput()
if (IsRegexMatch  ("^(a )?(man|lady|woman|human|person)$", LCase (answer))) {
  msg ("'Is it a man?' you ask.")
  msg ("'How come everyone knows the answer?'")
  msg ("'We have this thing called the internet nowadays...'")
}
else {
  msg ("'Is it \"" + answer + "\"?' you ask.")
  msg ("'No!'")
}
JS.eval("$('#txtCommand').attr('placeholder', 'Type here...');")
JS.panesVisible(true)
```


## Dozens of questions

For a complex RPG-style game you might have lots of questions to create the character. Because the script simply waits at each `GetInput()`, you can put the whole lot in a loop.

The objective here is to ask a series of questions, which we will say is in a string list, game.questions, and put the answers in a second list, game.answers.

```quest
foreach (question, game.questions) {
  msg (question)
  answer = GetInput()
  list add (game.answers, answer)
  msg ("> " + answer)
  msg ("Thanks")
}
msg (" ")
msg ("Name: " + StringListItem(game.answers, 0))
msg ("Age: " + StringListItem(game.answers, 1))
msg ("Favourite colour: " + StringListItem(game.answers, 2))
msg (" ")
```

The loop takes each question in turn, prints it, waits for the answer, and adds that answer to the list. Once the list of questions has run out, the loop ends and the summary at the bottom is printed.
