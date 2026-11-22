# Elements of a text adventure

Worked examples of the parts of an `.aslx` text adventure that come up most, as the Quest Viva editor saves them. Each is a fragment that goes inside `<asl>`, and together they make one small game. For every element and attribute, see [elements](/reference/elements/) and [attributes](/reference/attributes/all/).

## Rooms and exits

A room is an `<object>` that inherits `editor_room`, and an exit is an `<exit>` inside the room it leads out of. An exit only goes one way, so a way between two rooms is two exits, one in each room. `alias` is the direction the player types, and the direction type (`northdirection`, `southdirection`, `eastdirection`, `westdirection`, `northeastdirection` and so on, `updirection`, `downdirection`, `indirection`, `outdirection`) puts it on the compass.

```xml
<object name="kitchen">
  <inherit name="editor_room" />
  <description>A cold kitchen. The back door is in the south wall.</description>
  <object name="player">
    <inherit name="editor_object" />
    <inherit name="editor_player" />
  </object>
  <exit alias="down" to="cellar">
    <inherit name="downdirection" />
  </exit>
  <exit name="back door exit" alias="south" to="garden">
    <inherit name="southdirection" />
    <locked type="boolean">true</locked>
    <lockmessage>The back door is locked.</lockmessage>
  </exit>
</object>
<object name="garden">
  <inherit name="editor_room" />
  <description>Fresh air at last.</description>
  <exit alias="north" to="kitchen">
    <inherit name="northdirection" />
  </exit>
</object>
```

- An exit only needs a `name` when a script refers to it, as `UnlockExit (back door exit)` does below. Without one, Quest names it itself.
- A locked exit stays locked until a script calls `UnlockExit`. To lock it again, use `LockExit`. A door object with a `lock`/`unlock` script is the usual way to do that ([locked exits](/howto/rooms/doors/)).
- The player starts in whichever room the `player` object is inside.

## Objects

```xml
<object name="brass key">
  <inherit name="editor_object" />
  <look>A small brass key.</look>
  <take />
</object>
<object name="back door">
  <inherit name="editor_object" />
  <scenery />
  <look>A back door with a small brass lock.</look>
  <unlock type="script">
    if (Got (brass key)) {
      msg ("You unlock the back door.")
      UnlockExit (back door exit)
    }
    else {
      msg ("You don't have the key.")
    }
  </unlock>
</object>
```

The door goes inside `kitchen`, and the key inside the cupboard below. `take` lets the player pick an object up, `scenery` leaves it out of the room's list of objects (the description already mentions it), and with it anything on or in it, so don't make a container or surface scenery unless the player is meant to find its contents by looking at it, and `alt` gives it other names the player can use.

## Containers, switchable objects and features

Some of Core's behaviour comes from inheriting one of its types: `container_open`, `container_closed`, `container_limited` and `surface` for containers, `openable` for something that only opens and closes, `switchable`, `edible`, `wearable` ([attributes and types](/understanding/attributes-and-types/)).

```xml
<object name="cupboard">
  <inherit name="editor_object" />
  <inherit name="container_closed" />
  <feature_container />
  <look>A cupboard above the worktop.</look>
  <listchildren />
</object>
<object name="lamp">
  <inherit name="editor_object" />
  <inherit name="switchable" />
  <feature_switchable />
  <feature_lightdark />
  <look>A battered lamp.</look>
  <take />
  <lightstrength>strong</lightstrength>
  <switchonmsg>The lamp flickers into life.</switchonmsg>
  <switchoffmsg>The lamp goes out.</switchoffmsg>
  <onswitchon type="script">
    this.lightsource = true
  </onswitchon>
  <onswitchoff type="script">
    this.lightsource = false
  </onswitchoff>
</object>
<object name="shelf">
  <inherit name="editor_object" />
  <inherit name="surface" />
  <feature_container />
  <look>A wooden shelf.</look>
  <listchildren />
  <listchildrenprefix>On it is</listchildrenprefix>
</object>
```

`listchildren` lists what's inside when the player looks at the container. A surface is listed in the room as "a shelf (on which there is a jar)", but looking at it says "It contains" unless `listchildrenprefix` says otherwise.

## Feature attributes

The `feature_...` attributes only tell the editor which tabs to show: they make no difference to how the game plays. But without them, the author opens an object in the editor and can't see the settings you gave it. So whenever you use one of these, also set its feature attribute on the object, as above: `feature_container` (containers and `openable`), `feature_switchable`, `feature_edible`, `feature_wearable`, `feature_usegive` (`use` and `give` scripts), `feature_lightdark` (a light source or a dark room), `feature_player` and `feature_startscript`. The game has its own: `feature_lightdark` (needed for the objects' one to show), `feature_limitinventory`, `feature_asktell`, `feature_annotations`, `feature_advancedwearables`, `feature_advancedscripts`. Their names have an underscore, so the editor writes them as `<attr name="feature_container" type="boolean">true</attr>`; `<feature_container />` means the same.

## A dark room

```xml
<object name="cellar">
  <inherit name="editor_room" />
  <dark />
  <feature_lightdark />
  <description>Damp stone walls, and a smell of apples.</description>
  <darkroomdescription>It's too dark to see.</darkroomdescription>
  <exit alias="up" to="kitchen">
    <inherit name="updirection" />
    <lightsource />
    <lightstrength>weak</lightstrength>
  </exit>
</object>
```

The game needs `<feature_lightdark />` too. An object that's a `lightsource` with `lightstrength` `strong` lights the room it's in; `weak` only makes itself visible, which is why the way out has it ([light and darkness](/howto/rooms/light-and-darkness/)).

## Types

A `<type>` is a set of attributes that objects take on with `<inherit>`. An object's own attributes override its types', and a type can inherit other types. That's also how Core's types work, so set the attributes they read rather than replacing their scripts: `edible` gives the object an `eat` script that prints `eatmsg`, and an `<eat>` of the object's own would replace it, so eating would no longer destroy the object.

```xml
<type name="fruit">
  <take />
  <eatmsg>Delicious.</eatmsg>
  <inherit name="edible" />
  <feature_edible />
</type>
<object name="apple">
  <inherit name="editor_object" />
  <inherit name="fruit" />
  <look>A red apple.</look>
</object>
```

## Functions

```xml
<function name="DescribeWeight" parameters="obj" type="string">
  if (GetInt (obj, "weight") > 1000) {
    return ("heavy")
  }
  else {
    return ("light")
  }
</function>
```

`type` is only for a function that returns a value, which it does with `return`. One that returns nothing is called as a statement, `DoSomething (a, b)`, and one with a `type` in an expression, `"It feels " + DescribeWeight (apple)`. Check `list_functions` before naming one: a function with the same name as one of Core's replaces it.

## The start script, turn scripts and timers

```xml
<game name="The Cellar">
  <gameid>...</gameid>
  <feature_lightdark />
  <start type="script">
    game.apples_eaten = 0
    msg ("You've come down for apples.")
  </start>
</game>
<turnscript name="turn counter">
  <enabled />
  <script>
    game.turns = GetInt (game, "turns") + 1
  </script>
</turnscript>
<timer name="drip">
  <interval>30</interval>
  <enabled />
  <script>
    if (game.pov.parent = cellar) {
      msg ("Somewhere, water drips.")
    }
  </script>
</timer>
```

- `start` runs once, before the first room's description. It's the place to give attributes a starting value that scripts read later, though setting them in the XML is usually clearer.
- A `<turnscript>` at the top level runs after every turn while it's enabled. Put one inside a room for one that only runs while the player is there. `EnableTurnScript` and `DisableTurnScript` switch one on and off.
- A `<timer>` runs every `interval` seconds of real time while it's enabled, with `EnableTimer` and `DisableTimer`. In `qv walkthrough`, a `delay:` step moves the clock on instead of waiting. The `tick` tool does the same while playing with the MCP tools.

## A walkthrough for all of it

```xml
<walkthrough name="escape">
  <steps>
    open cupboard
    take key
    unlock door
    assert:not back door exit.locked
    s
    assert:game.pov.parent = garden
  </steps>
</walkthrough>
```

An object that's been eaten, or removed with `destroy`, no longer exists, so naming it in an `assert:` is an error: test for it with `GetObject ("apple") = null`.

Steps are what a player types, plus `assert:` with an ASL expression that must be true ([walkthroughs](/howto/testing/walkthroughs/)). Other steps are `menu:<option>` and `answer:yes`/`answer:no` for a menu or question the game is waiting on, `delay:<milliseconds>` and `label:` for a note.
