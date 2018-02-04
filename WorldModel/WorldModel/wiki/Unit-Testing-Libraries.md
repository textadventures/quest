Unit testing is a standard procedure in software development; the coder makes an _assertion_ about what is expected from a section of code. 
A unit test, then, is a bit of code that runs a method or function and then tests the output or state against expected values, and generally unit testing will run through a set of unit tests, flagging any that do not give the expected results.

As part of the development of Quest 5.7, I have looked at doing unit testing in Quest. This is most applicable to libraries; you can have your game in one file and your unit tests in another, and both use the same set of libraries. With that in mind, this is not a library, as such, but a game file. To use it, add your library to this file, and then add your tests (requires Quest 5.7).

It is usual practice in unit testing to reset the system after each test. That is not practical in Quest, so you need to be aware that the game state will potentially change. If you add a new test at the start and this moves an object, for example, then that might impact on other tests. You need to design your tests with this in mind, and it is good practice after a set of tests to chanmge back anything you have changed.

Basic Testing
--------------

Let us start with a simple example. Here are the unit tests for the new `FormatList` function.


```
player.changedparent => {
}
Testing("FormatList")
list = Split("one;two;three", ";")
Assert ("one, two or three", FormatList(list, ",", "or", "nothing"))
Assert ("one; two; and three", FormatList(list, ";", "; and", "nothing"))
list = NewStringList()
Assert ("nothing", FormatList(list, ",", "and", "nothing"))
Results
```

The first two lines remove the usual script from the player, which allows us to silently move the player. It can be helpful to set up tests in different rooms, and to move the player to the relevant room as required. These two lines are only required once for all your tests. The final line calls `Results`, which gives a summary of the tests. Again, this should only be called once, after all the tests are done.

The tests themselves start with a line calling `Testing`, which gives this section a title and will make it easier to find which test is failing. The next line sets up a variable that can be used in the test, and the next two lines that use `Assert` are the actual tests. Each `Assert` has two parameters; the expected value and the actual value returned from the function. In this example, list is then set to a new value, and a further test done.

Run it and you will see this:

> ... 
> 
> No failures!

What happens when a test fails?
-------------------------------

What do you see if a test fails? Try it with this code (with an extra line added almost at the end):

```
player.changedparent => {
}
Testing("FormatList")
list = Split("one;two;three", ";")
Assert ("one, two or three", FormatList(list, ",", "or", "nothing"))
Assert ("one; two; and three", FormatList(list, ";", "; and", "nothing"))
list = NewStringList()
Assert ("nothing", FormatList(list, ",", "and", "nothing"))
Assert (0, FormatList(list, ",", "and", "nothing"))
Results
```

This is the output.

> ...F
>
> 1 failure(s):
> Error in FormatList and IndexOf: >nothing< (string) was expected to be >0< (int)

For each test, it prints a dot if it passes or an F if it fails. Then, at the end, all the failures are listed. It says it is in `Format` list because that was the last `Testing` call. Then it tells you what it expected and what it found (with the types if they do not match).

Adding more tests
-----------------

Here are some more tests added, now testing `CloneObjectAndMoveHere` as well (and the failing test removed).

```
player.changedparent => {
}
Testing("FormatList")
list = Split("one;two;three", ";")
Assert ("one, two or three", FormatList(list, ",", "or", "nothing"))
Assert ("one; two; and three", FormatList(list, ";", "; and", "nothing"))
list = NewStringList()
Assert ("nothing", FormatList(list, ",", "and", "nothing"))
Assert (0, FormatList(list, ",", "and", "nothing"))
Testing("CloneObjectAndMoveHere")
cl = CloneObjectAndMoveHere (tiger)
Assert (room, cl.parent)
Assert ("tiger", cl.alias)
Results
```

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

You can use `AssertMatch` to test if a string matches a regular expression. A discussion of regular expressions is beyond the scope of this page, but essentually we are matching against a template rather than a specific string. In the examples below, `^` matches the start of the string, `$` the end, and `\\w` matches any letter or number. The first six assertions will pass, the last three will fail (note that `tiger` is an object).

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


Unit Testing Framework
----------------------

Use this as the basis for your own unit tests. Add the file to your game's folder, and then add your library files to this game. You can then test all your library code.

This will not get added to your game when you publish, by the way!

```
<!--Saved by Quest 5.6.6108.15891-->
<asl version="550">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <dynamictemplate name="ObjectCannotBeStored">"You cannot put " + GetDisplayName(object) + " there."</dynamictemplate>
  <delegate name="textprocessor" parameters="" type="string" />
  <game name="Unit testing">
    <gameid>120a1c08-57db-4210-bdb0-540c78231fc4</gameid>
    <version>1.0</version>
    <firstpublished>2017</firstpublished>
    <defaultforeground>Black</defaultforeground>
    <showpanes type="boolean">false</showpanes>
    <testcount type="int">0</testcount>
    <testfailures type="stringlist" />
    <defaultfont>'Courier New', Courier, monospace</defaultfont>
    <feature_annotations />
    <feature_advancedwearables />
    <attr name="autodescription_youarein" type="int">0</attr>
    <attr name="autodescription_youcansee" type="int">0</attr>
    <attr name="autodescription_youcango" type="int">0</attr>
    <attr name="autodescription_description" type="int">0</attr>
    <showcommandbar type="boolean">false</showcommandbar>
    <showlocation type="boolean">false</showlocation>
    <showborder type="boolean">false</showborder>
    <gridmap type="boolean">false</gridmap>
    <classiclocation />
    <attr name="feature_pictureframe" type="boolean">false</attr>
    <shadowbox />
    <roomenter type="script">
    </roomenter>
    <start type="script"><![CDATA[
      player.changedparent => {
      }
      Testing ("Your tests here")
      Assert ("player", player.name)
      Results
    ]]></start>
  </game>
  <object name="room">
    <inherit name="editor_room" />
    <usedefaultprefix />
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
  </object>
  <function name="Testing" parameters="s">
    game.testingtitle = s
  </function>
  <function name="OutputText" parameters="text">
    game.lastoutputtext = ProcessText(text)
    if (not GetBoolean(game, "hideoutput")) {
      OutputTextRaw (game.lastoutputtext)
    }
  </function>
  <function name="AssertOutput" parameters="s">
    Assert (s, game.lastoutputtext)
  </function>
  <function name="Assert" parameters="expected, actual"><![CDATA[
    if (not TypeOf(expected) = TypeOf(actual)) {
      Result (">" + actual + "< (" + TypeOf(actual) + ") was expected to be >" + expected + "< (" + TypeOf(expected) + ")")
    }
    else if (not expected = actual) {
      Result (">" + actual + "< was expected to be >" + expected + "<")
    }
    else {
      Result (null)
    }
  ]]></function>
  <function name="AssertMatch" parameters="expected, actual"><![CDATA[
    if (not "string" = TypeOf(actual)) {
      Result (">" + actual + "< (" + TypeOf(actual) + ") was expected to match >" + expected + "< (" + TypeOf(expected) + ")")
    }
    else if (not IsRegexMatch(expected, actual)) {
      Result (">" + actual + "< was expected to match >" + expected + "<")
    }
    else {
      Result (null)
    }
  ]]></function>
  <function name="AssertCommand" parameters="com, s">
    game.hideoutput = true
    HandleSingleCommand (com)
    game.hideoutput = false
    Assert (s, game.lastoutputtext)
  </function>
  <function name="AssertCommandMatch" parameters="com, s">
    game.hideoutput = true
    HandleSingleCommand (com)
    game.hideoutput = false
    AssertMatch (s, game.lastoutputtext)
  </function>
  <function name="AssertIn" parameters="list, actual"><![CDATA[
    if (not ListContains(list, actual)) {
      Result (">" + actual + "< expected to be in " + list)
    }
    else {
      Result (null)
    }
  ]]></function>
  <function name="AssertInRange" parameters="from, to, code"><![CDATA[
    actual = eval(code)
    if (actual < from or actual > to) {
      Result (">" + actual + "< expected to be in range " + to + " to " + from + ": " + code)
    }
    else {
      Result (null)
    }
  ]]></function>
  <function name="Result" parameters="message"><![CDATA[
    if (message = null) {
      JS.addText (".")
    }
    else {
      JS.addText ("F")
      list add (game.testfailures, "Error in " + game.testingtitle + ": " + message)
    }
    game.testcount = game.testcount + 1
    if (game.testcount = 50) {
      JS.addText ("<br/>")
      game.testcount = 0
    }
  ]]></function>
  <function name="Results">
    game.testingdone = true
    msg (" ")
    msg (" ")
    if (ListCount(game.testfailures) = 0) {
      msg ("No failures!")
    }
    else {
      msg (ListCount(game.testfailures) + " failure(s):")
      foreach (s, game.testfailures) {
        OutputTextRaw (s)
      }
    }
    msg (" ")
    msg (" ")
    finish
  </function>
</asl>


