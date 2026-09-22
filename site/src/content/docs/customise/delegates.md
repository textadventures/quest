---
title: Using delegates
description: Store a script on an object that takes parameters or returns a value, by defining a delegate to describe its signature
sidebar:
  order: 3
---

A [script](/reference/attributes/types#script) attribute lets you attach a script to an object and run it with `do`. But a script attribute takes no parameters and returns no value, so it can't answer a question or be told how much damage to do. A **delegate** fills that gap: it defines a signature - parameters, a return type, or both - that a script attribute can then use instead of plain `script`.

:::note
Delegates can currently only be edited in the raw XML code view. There is no dedicated editor support for delegates, so if you want the editor's help, see "Passing values in a dictionary" at the end of this page.
:::

To use one:

1. Define the delegate with a [`<delegate>`](/reference/elements#delegate) element. It takes the same attributes as [`<function>`](/reference/elements#function), so you can give it parameters, a return type, or both.
2. Give an attribute that delegate's name as its type, instead of `script`.
3. Run it with [`rundelegate`](/reference/script-commands#rundelegate) if it returns nothing, or [`RunDelegateFunction`](/reference/functions/general#rundelegatefunction) if it returns a value.

[`HasDelegateImplementation`](/reference/functions/attributes#hasdelegateimplementation) tells you whether a particular object has implemented one.

The `<delegate>` element has to appear in the file **before** anything that uses it - just after the library includes is a good place. Put it later and the game won't load: it stops with "Unrecognised attribute type '...' in '...'".

## Delegates in action

In object-oriented programming, a function attached to an object is called a *method*, and that's the word used here. A method's *signature* is its return type and the parameters it expects.

Here is a very simple game where you can hit a goblin. English.aslx already defines a `hit` verb, so the goblin's `hit` script attribute runs when the player types HIT GOBLIN.

```xml
<!--Saved by Quest Viva 6.0.0-->
<asl version="600">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <game name="test">
    <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
  </game>
  <object name="room">
    <inherit name="editor_room" />
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
    <object name="goblin">
      <inherit name="editor_object" />
      <fullhits type="int">10</fullhits>
      <hitslost type="int">0</hitslost>
      <hit type="script">
        msg ("You hit the " + this.name + " and it loses 7 hits!")
        this.hitslost = this.hitslost + 7
      </hit>
    </object>
  </object>
</asl>
```

### Returning a value

Now add a method that returns the percentage of its full hits the goblin has left. A delegate is a way to define a custom signature, and this one needs no parameters and returns an int:

```xml
<delegate name="script_returns_int" type="int" />
```

The goblin gets a new attribute, `getpc`, set up like a script but with the delegate's name as its type. Because it returns a value, call it with `RunDelegateFunction` rather than `do`:

```xml
<!--Saved by Quest Viva 6.0.0-->
<asl version="600">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <delegate name="script_returns_int" type="int" />
  <game name="test">
    <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
  </game>
  <object name="room">
    <inherit name="editor_room" />
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
    <object name="goblin">
      <inherit name="editor_object" />
      <fullhits type="int">10</fullhits>
      <hitslost type="int">0</hitslost>
      <getpc type="script_returns_int">
        return (100 * (this.fullhits - this.hitslost) / this.fullhits)
      </getpc>
      <hit type="script">
        msg ("You hit the " + this.name + " and it loses 7 hits!")
        this.hitslost = this.hitslost + 7
        msg ("The " + this.name + " has " + RunDelegateFunction (this, "getpc") + "% of its hits.")
      </hit>
    </object>
  </object>
</asl>
```

The goblin's own `getpc` decides how its percentage is worked out, so a different monster can carry a different calculation under the same name - armour that soaks up damage, say - and the code that prints the message doesn't have to know or care.

### Taking parameters

Suppose the player can kick the goblin as well as hit it. Rather than repeat the code, both scripts call a new method, `attack`, and pass it the damage done. That needs a second delegate, this time with a parameter instead of a return value (a delegate can have both, if you need it to):

```xml
<delegate name="script_with_1" parameters="a" />
```

The parameters of the method must have the same names as those in the delegate definition - here, `a`. Invoke it with `rundelegate`, because it returns nothing:

```xml
<!--Saved by Quest Viva 6.0.0-->
<asl version="600">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <delegate name="script_returns_int" type="int" />
  <delegate name="script_with_1" parameters="a" />
  <game name="test">
    <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
  </game>
  <object name="room">
    <inherit name="editor_room" />
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
    <object name="goblin">
      <inherit name="editor_object" />
      <fullhits type="int">25</fullhits>
      <hitslost type="int">0</hitslost>
      <getpc type="script_returns_int">
        return (100 * (this.fullhits - this.hitslost) / this.fullhits)
      </getpc>
      <gethits type="script_returns_int">
        return (this.fullhits - this.hitslost)
      </gethits>
      <attack type="script_with_1">
        msg ("The " + this.name + " loses " + a + " hits!")
        this.hitslost = this.hitslost + a
        msg ("The " + this.name + " has " + RunDelegateFunction (this, "getpc") + "% of its hits.")
      </attack>
      <hit type="script">
        rundelegate (this, "attack", 4)
      </hit>
      <kick type="script">
        rundelegate (this, "attack", 7)
      </kick>
    </object>
  </object>
  <verb>
    <property>kick</property>
    <pattern>kick</pattern>
    <defaultexpression>"You can't kick " + object.article + "."</defaultexpression>
  </verb>
</asl>
```

## Passing values in a dictionary

If all you need is to pass parameters to a script - not to get a value back - you don't need a delegate at all. Put the values in a dictionary and pass that to `do`. The script picks them up as ordinary variables, so `attack` goes back to being a plain `script` attribute, and the editor can edit it like any other script:

```xml
<attack type="script">
  msg ("The " + this.name + " loses " + a + " hits!")
  this.hitslost = this.hitslost + a
</attack>
<hit type="script">
  d = NewDictionary()
  dictionary add (d, "a", 4)
  do (this, "attack", d)
</hit>
<kick type="script">
  d = NewDictionary()
  dictionary add (d, "a", 7)
  do (this, "attack", d)
</kick>
```

Which you prefer is a matter of taste. The dictionary is easier to edit and needs no `<delegate>` element; the delegate makes the signature explicit, and it's the only option when you need a return value.

## See also

- [Creating functions which return a value](/howto/scripting/functions)
- [`<delegate>` in the XML elements reference](/reference/elements#delegate)
- [Editing the raw XML](/howto/scripting/raw-xml)
