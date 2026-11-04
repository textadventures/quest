**Before you write or edit a game file**, read this primer. It covers what agents most often get wrong about Quest Viva's `.aslx` format and its script language, ASL. Every command and function is in the [language reference](/reference/script-commands/), and the example games below are complete files to copy from.

**A new game**, as the Quest Viva editor saves it. An `.aslx` file is XML: one `<asl>` element holding the libraries, one `<game>`, and every room, object, command and function. Rooms are objects, and nesting an `<object>` inside another puts it there at the start of the game. The editor adds `<!--Saved by Quest Viva ...-->` above `<asl>`, which is only a comment.

```xml
<asl version="610">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <game name="My Game">
    <gameid>144f1d2b-e0a0-4ff9-9303-d900af4abf55</gameid>
    <version>1.0</version>
    <firstpublished>2026</firstpublished>
  </game>
  <object name="room">
    <inherit name="editor_room" />
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
  </object>
</asl>
```

- `version="610"` is the version of ASL the file was written for. The editor always saves the current one, 610, and newer fixes only apply to games with a high enough number ([game file versions](/reference/asl-versions/)). Keep an existing game's number unless you mean to upgrade it.
- `English.aslx` and `Core.aslx` are built into Quest. They provide the parser, the standard verbs and the default text, so they aren't files you need. Don't copy their functions into the game.
- `gameid` is a GUID unique to each game. Make a new one for a new game, and never change an existing game's.
- `editor_room` makes an object a room (it sets `isroom`), and `editor_object` and `editor_player` mark an ordinary object and the player. Give every new room and object the same `<inherit>` lines, so the editor shows them as it would its own.
- The player is the object the game starts in. Scripts refer to it as `game.pov`, which is safer than `player` since a game can switch the player to another object.

**Elements** you can put inside `<asl>` ([elements reference](/reference/elements/)):

| Element | For |
|---|---|
| `<game name="...">` | the game's title and game-wide settings and scripts: `start` runs once when the game begins |
| `<object name="...">` | rooms, things and characters. Its child elements are its attributes |
| `<exit alias="north" to="kitchen">` | inside a room: a way out to another room. Add `<inherit name="northdirection" />` (or `southdirection`, `updirection`, `indirection` and so on) for a compass exit. An exit only goes one way, so the room it leads to needs its own exit back |
| `<command name="...">` | a command of the game's own, with a `<pattern>` and a `<script>` |
| `<verb>` | a verb such as "eat" that objects answer with an attribute of the same name: the verb's `<property>` |
| `<function name="..." parameters="a, b" type="string">` | a function. `type` is the return type, and only for a function that returns a value |
| `<turnscript name="...">` | a script that runs after every turn while `enabled` |
| `<timer name="...">` | a script that runs every `interval` seconds while `enabled` |
| `<type name="...">` | a set of attributes that objects take on with `<inherit name="..." />` |
| `<walkthrough name="...">` | a list of steps to play the game automatically, for testing |
| `<include ref="...">` | another library: a `.aslx` file next to the game |

**Attributes** are an object's child elements, named after the attribute. With no `type`, the value is a string, or with no text at all `true`: `<scenery />` sets `scenery` to true. Otherwise say the type ([attribute types](/reference/attributes/types/)):

```xml
<object name="lamp">
  <inherit name="editor_object" />
  <alias>brass lamp</alias>
  <look>A battered brass lamp.</look>
  <take />
  <lit type="boolean">false</lit>
  <oil type="int">3</oil>
  <alt type="stringlist">
    <value>lantern</value>
    <value>light</value>
  </alt>
  <light type="script">
    this.lit = true
    msg ("The lamp flickers into life.")
  </light>
</object>
```

The other types are `double`, `object` (an object's name), `objectlist` and `simplestringlist` (items separated by semicolons), `stringdictionary`, `objectdictionary`, `scriptdictionary`, `list` and `dictionary`. An attribute whose name isn't a valid XML element name, such as one with a space, is written `<attr name="hit points" type="int">10</attr>`. Many attributes are both: `look`, `take` and verb attributes print a string, or run a script when `type="script"`. The ones Core understands, such as `alias`, `look`, `take`, `scenery`, `description`, `visible`, `locked` and `parent`, are in the [attributes reference](/reference/attributes/all/).

**XML escaping.** A script or string containing `<`, `>` or `&` has to be escaped, or the file isn't valid XML and won't load. The editor wraps such text in CDATA, and so should you: comparisons, `and` written as `&&`, and HTML in a message all need it.

```xml
<start type="script"><![CDATA[
  if (player.health < 10) {
    msg ("You feel <b>weak</b>.")
  }
]]></start>
```

**Scripts are ASL, Quest's own language, not JavaScript or C#.** The rules that trip agents up ([script commands](/reference/script-commands/)):

- One command per line. There are no semicolons: `x = 1; y = 2` is an error.
- A block's `{` goes on the same line as its `if`, `else`, `foreach` and so on. A `{` on the next line is an error.
- `=` is both assignment and comparison: `x = 3` sets `x`, and `if (x = 3)` tests it. Not equal is `<>`. `and`, `or` and `not` are the logical operators, and `not` covers the comparison after it: `not x = 4` means `not (x = 4)`. The engine also accepts `==`, `!=`, `&&` and `||`, but games and the docs use the ASL forms.
- There are no `++`, `--`, `+=` or `-=`: write `x = x + 1`.
- Strings use double quotes, with `\"` for a quote inside one. Don't use single quotes: they don't work like double quotes. `+` joins strings, and turns a number into text along the way: `"Score: " + game.score`.
- Dividing two whole numbers gives a whole number: `7 / 2` is 3, and `7 / 2.0` is 3.5.
- Local variables need no declaration, and last until the script ends. Anything that has to last is an attribute: `game.count = 0` or `this.lit = true`.
- `object.attribute` reads and sets attributes. Reading one that has never been set is an error, so give it a value in the XML, or use `GetBoolean (obj, "name")`, `GetInt`, `GetString`, `GetAttribute` or `HasAttribute`, which handle a missing one. Setting an attribute to `null` removes it.
- Objects are referred to by name, without quotes: `lamp.parent = kitchen` moves the lamp to the kitchen. A name with spaces works too: `the box.flag = true`. Names are case-sensitive. To look one up from a string, use `GetObject ("lamp")`.
- `this` is the object a script belongs to.
- Comments start with `//`.

```
msg ("You open the box.")
if (box.isopen and not GetBoolean (key, "found")) {
  key.found = true
  MoveObject (key, game.pov)
}
else if (game.pov.parent = cellar) {
  msg ("Too dark to see.")
}
else {
  msg ("Nothing else inside.")
}
foreach (obj, GetDirectChildren (box)) {
  msg ("There is " + GetDisplayName (obj) + " here.")
}
for (i, 1, 3) {
  msg (i)
}
switch (direction) {
  case ("n", "north") {
    msg ("North it is.")
  }
  default {
    msg ("Which way?")
  }
}
```

- `msg (...)` prints, and needs its parentheses. A function that returns nothing is called as a statement, `MoveObject (key, game.pov)`, and one that returns a value in an expression, `name = GetDisplayAlias (key)`. A function returns its value with `return (value)`.
- `firsttime { ... } otherwise { ... }` runs a script only the first time; `while (...) { ... }` loops.
- Lists come from `NewStringList ()`, `NewObjectList ()` and functions such as `ScopeVisible ()`. Change them with `list add (mylist, item)` and `list remove (mylist, item)`, and read them with `ListCount`, `ListContains` and `foreach` ([list functions](/reference/functions/list/)). Dictionaries are the same with `NewDictionary ()`, `dictionary add (dict, key, value)` and `DictionaryItem` ([dictionary functions](/reference/functions/dictionary/)).
- `obj.attr => { ... }` sets a script attribute, and `do (obj, "attr")` runs one.
- `Ask ("Are you sure?")` returns true or false, `ShowMenu` asks the player to choose, and `GetInput ()` returns what they type. Each waits for the player before the script carries on ([asking the player](/howto/scripting/asking-the-player/)).

**Text in curly braces** in a printed string is the text processor, not ASL: `{once:...}` shows text the first time only, `{object:lamp}` links to an object, and `{if lamp.lit:...}` shows text when an attribute is true ([text processor](/howto/text/text-processor/)). A literal `{` in printed text can start one by accident.

**Commands.** A command's `<pattern>` is what the player types, with alternatives separated by semicolons. A placeholder whose name starts with `object` matches an object the player can see, `exit` an exit and `text` anything; the script reads each from a local variable of the same name ([how commands work](/howto/commands/how-commands-work/)):

```xml
<command name="weigh">
  <pattern>weigh #object#; put #object# on scales</pattern>
  <script>
    if (HasInt (object, "weight")) {
      msg ("It weighs " + object.weight + " grams.")
    }
    else {
      msg ("You can't weigh that.")
    }
  </script>
</command>
```

For an action that many objects answer differently, use a `<verb>` instead, and give each object an attribute named after its `property`: a string to print or a script to run, like the lamp's `light` above. Objects without one get the default text ([verbs](/howto/commands/verbs/)):

```xml
<verb>
  <property>light</property>
  <pattern>light</pattern>
  <defaultexpression>"You can't light " + object.article + "."</defaultexpression>
</verb>
```

Core already has the common verbs, so check before adding one: objects answer `take`, `eat`, `read` and others through attributes of those names.

**Common mistakes**:

- Writing JavaScript or C#: semicolons, a `{` on its own line, `x++` or `+=`, `let` or `var`, single-quoted strings, `===`, or methods such as `list.push`.
- A `<`, `>` or `&` outside CDATA: the file won't load.
- Copying a library function into the game to call it. Core's functions are always there. Defining one of the same name replaces Core's for the whole game, which is how authors change built-in behaviour on purpose and how agents break it by accident.
- Reading an attribute before anything has set it.
- An exit to a room that doesn't exist, which stops the game loading, or a one-way exit where two were wanted.
- Two elements with the same name. Objects, functions, commands and the rest share one set of names: a second element of the same kind silently replaces the first, and one of a different kind stops the game loading.
- Editing a published `.quest` file. It's a zip package, not XML. Edit the `.aslx` the game was published from.
- Guessing a function's name or parameters. Look it up in the reference, and check which of Core's functions the game itself replaces.

**After editing, check the file and play your change.** The `qv` command-line tool (`dotnet tool install -g QuestViva.Cli --prerelease`) finds problems without playing, and runs the game's walkthroughs:

- `qv check game.aslx` reports script commands and functions that don't exist, expressions that don't parse, functions given the wrong number of arguments, duplicate names, and anything that stops the game loading. The engine only parses a script when it first runs, so without this most mistakes only show when a player reaches them. Run it after every edit.
- `qv walkthrough game.aslx [name]` plays a walkthrough and fails if a step's script errors, an `assert:` is false, or the game ends early. To test a change, add a walkthrough that reaches it and ends in `assert:` steps, which are ASL expressions ([walkthroughs](/howto/testing/walkthroughs/)). An error in the game's `start` script doesn't fail a walkthrough, so read the opening text too.

```xml
<walkthrough name="lamp works">
  <steps>
    take lamp
    light lamp
    assert:lamp.lit
  </steps>
</walkthrough>
```

- `qv play game.aslx` plays the game in the terminal, and `qv mcp --debug-tools` gives an agent the same tools over MCP, with `docs` to look things up in this documentation.
