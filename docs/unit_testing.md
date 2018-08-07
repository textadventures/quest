---
layout: index
title: Unit Testing
---

Unit testing is a standard procedure in software development; the coder makes an _assertion_ about what is expected from a section of code.

A unit test, then, is a bit of code that runs a method or function and then tests the output or state against expected values. Generally a developer will run a whole set of unit tests (often called a test suite) from one command or click of a button, and any that do not give the expected results are flagged. The test suite can be run at regular intervals to ensure recent changes have not messed up systems that previously worked.

As part of the development of Quest 5.7, I have looked at doing unit testing in Quest. This is most applicable to libraries (and really it is  dubious if it is worth the effort unless you plan to release the library). You can have your game in one file and your unit tests in another, and both use the same set of libraries. With that in mind, this is not a library file, but a game file. To use it, add _your_ library to this file, and then add your tests to this game file.

[unit_test.aslx](util/unit_test.aslx)

It is usual practice in unit testing to have the system automatically reset after each test. That is not practical in Quest, so you need to be aware that the game state will potentially change. If you add a new test at the start and this moves an object, for example, then that might impact on other tests. You need to design your tests with this in mind, and it is good practice after a set of tests to change back anything you have changed.


Where Tests Go
--------------

Tests go in the `game.start` script (or in functions called from it). This is what it looks like before you do anything.

```
player.changedparent => {
}
//
// TEST CODE HERE
//
Results
```

Obviously your tests will go where it says "TEST CODE HERE". In case you are wondering, the first two lines disable the script that runs when the player changes rooms, so you can move the player during a test without the room description getting displayed. The last line prints a summary of the tests.


Basic Testing
--------------

Let us start with a simple example. Here are the unit tests for the `FormatList` and `IndexOf` functions, which were new in Quest 5.7.


```
Testing ("FormatList and IndexOf")
list = Split("one;two;three", ";")
Assert ("one, two or three", FormatList(list, ",", "or", "nothing"))
Assert ("one; two; and three", FormatList(list, ";", "; and", "nothing"))
Assert (0, IndexOf(list, "one"))
Assert (-1, IndexOf(list, "zero"))
list = NewStringList()
Assert ("nothing", FormatList(list, ",", "and", "nothing"))
Assert (-1, IndexOf(list, "one"))
```

The tests start with a line calling `Testing`, which gives this section a title and will make it easier to find which test is failing. The next line sets up a variable that can be used in the test, and the next four lines that use `Assert` are the actual tests. Each `Assert` has two parameters; the expected value and the actual value returned from the function. In this example, list is then set to a new value, and a further test done. It is important that unit tests cover all the possibilities, so here I am testing what happens if the list is empty.

Run it and you will see this:

> ......
> 
> No failures!

What happens when a test fails?
-------------------------------

What do you see if a test fails? Try it with this code (with an extra line added at the end):

```
Testing ("FormatList and IndexOf")
list = Split("one;two;three", ";")
Assert ("one, two or three", FormatList(list, ",", "or", "nothing"))
Assert ("one; two; and three", FormatList(list, ";", "; and", "nothing"))
Assert (0, IndexOf(list, "one"))
Assert (-1, IndexOf(list, "zero"))
list = NewStringList()
Assert ("nothing", FormatList(list, ",", "and", "nothing"))
Assert (-1, IndexOf(list, "one"))
Assert (0, FormatList(list, ",", "and", "nothing"))
```

This is the output.

> ......F
>
> 1 failure(s):
> Error in FormatList and IndexOf: >nothing< (string) was expected to be >0< (int)

For each test, it prints a dot if it passes or an F if it fails. Then, at the end, all the failures are listed. It says it is in `FormatList and IndexOf` list because that was the last `Testing` call. Then it tells you what it expected and what it found (with the types if they do not match).



Testing commands
----------------

You can test commands too, using `AssertCommand`. Again this takes two parameters, but in this case the first is the command, as the player would type it, and the second is the text that will be printed (specifically the last text sent to `msg` or `OutputText`). Here is a section of the testing for wearables, showing this in action.

```
Testing ("Clothing - commands")
AssertCommand ("wear overcoat", "You put it on.")
AssertCommand ("put overcoat on", "You are already wearing it.")
AssertCommand ("put on overalls", "You cannot wear that over an overcoat.")
AssertCommand ("doff overcoat", "You take it off.")
AssertCommand ("don overalls", "You put them on.")
AssertCommand ("wear overcoat", "You put it on.")
AssertCommand ("take overalls off", "You can't remove that while wearing an overcoat.")
```

This set does a command then checks the game state afterwards (in this case the inventory verbs).

```
AssertCommand ("get shoes", "You pick them up.")
Assert ("Look at;Drop;Wear", Join(shoes.inventoryverbs, ";"))
AssertCommand ("don shoes", "You put them on.")
Assert ("Look at;Remove", Join(shoes.inventoryverbs, ";"))
AssertCommand ("get socks", "You pick them up.")
Assert ("Look at;Drop", Join(socks.inventoryverbs, ";"))
```

Text processor commands will get processed; you can test against the output.

```
Testing ("Text processor: text style")
Assert ("This is in <i>italic</i>", ProcessText("This is in {i:italic}"))
Assert ("This is in <b>bold</b>", ProcessText("This is in {b:bold}"))
Testing ("Text processor: eval")
Assert ("player.count = 5", ProcessText("player.count = {=player.count}"))
Assert ("You are in the room", ProcessText("You are in the {=player.parent.name}"))
Assert ("You are in the Room", ProcessText("You are in the {=CapFirst(player.parent.name)}"))
```

Matching text
-------------

You can use `AssertMatch` to test if a string matches a regular expression. A discussion of regular expressions is beyond the scope of this page, but essentially we are matching against a template rather than a specific string. In the examples below, `^` matches the start of the string, `$` the end, and `\\w` matches any letter or number. The first six assertions will pass, the last three will fail (note that `tiger` is an object).

```
Testing ("AssertMatch")
AssertMatch("tiger", tiger.name)
AssertMatch("^tiger$", tiger.name)
AssertMatch("tiger", "I see a tiger.")
AssertMatch("t\\w\\wer", "I see a tiger.")
AssertMatch("t\\w\\wer", "I see a tamer.")
AssertMatch("t\\w\\wer$", "I see a tiger")

AssertMatch("tiger", tiger)
AssertMatch("t\\w\\wer", "I see a tigey.")
AssertMatch("t\\w\\wer$", "I see a tiger.")
```

You can use `AssertCommandMatch` to match against the output of a command.

In use
------

I suggest adding no more than 5 new tests at a time. If you have a failure, you will know it is one of those, and it will not be too tricky to find. If you have a lot of tests and one starts to fail, and you cannot work out which one, add some commands like this in you tests, and you will see "@1" in the output. Where the "F" is in relation to "@1" will help you identify the failure.

```
Assert ("You are in the room", ProcessText("You are in the {=player.parent.name}"))
OutputTextRawNoBr ("@1")
Assert ("You are in the Room", ProcessText("You are in the {=CapFirst(player.parent.name)}"))
```

When you create unit tests, you should think about testing your functions and commands as widely as possible. Test the full range of inputs to a function, and if necessary test what happens if an object is sent instead of a string, etc. if that is a possibility.

For commands, try to test a range of forms and synonyms (DON HAT, PUT ON CAP, PUT CAP ON, etc.), and in particular test when the command will fail. If the player needs to be holding a certain item, test what happens when that is not the case, for example. What happens if the player does it twice?

You will need to add objects and rooms to your unit test game to get this to work. You may find it convenient to have one room for each command or set of commands. Remember that each command will potentially change the state of the game world (eg, the player is now wearing the cap).


