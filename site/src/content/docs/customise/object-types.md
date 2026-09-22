---
title: Creating and using object types
description: Make your own object type in the editor, apply it to objects, override it on one of them, and combine several types
---

When several objects in your game behave the same way - a dozen spells, a shelf of books, every coin in the dungeon - you don't want to build each one by hand. An **object type** holds the attributes and scripts once, and every object that inherits it gets them.

After this page you'll be able to create a type in the editor, apply it to objects, change one object's behaviour without disturbing the rest, and combine two types on the same object.

If you haven't met attributes and inheritance before, read [Attributes and types](/understanding/attributes-and-types) first - it explains where an attribute's value comes from, which is what everything below relies on.

## Is a type the right tool?

| You want | Use |
|---|---|
| The same attributes and scripts on several objects | A type |
| The same behaviour available to several *games* | A [library](/customise/libraries), usually containing types |
| One piece of logic called from several places | A [function](/howto/scripting/functions) |
| To change how Quest Viva itself behaves everywhere | [Override the Core library](/customise/overriding) - including its `defaultobject` type, which every object inherits |

A type earns its place at about three objects. Below that, setting the attributes on each object is clearer.

## Creating a type

1. In the tree on the left, select **Advanced**.
2. Click **+ Add Type**.
3. In the "Add Type" dialog, type a **Name** and click **Add Type**.

Use a short lower-case name with no spaces, as you would for an object. The new type appears under _Advanced > Object Types_.

A type has a single _Type_ tab, with its **Name** and the same attributes list you get on an object: _Inherited types_ at the top, _Attributes_ underneath. Everything a type does, it does through those attributes.

Here's a `spell` type. Spells can't be taken or dropped in the usual way, and they can be learned:

| Attribute | Type | Value |
|---|---|---|
| `take` | Boolean | false |
| `drop` | Boolean | false |
| `known` | Boolean | false |
| `learnmsg` | String | The words settle into your memory. |
| `learn` | Script | see below |

Type each name into the **Add attribute...** box, click **Add**, then set its type and value on the right.

`learn` is a [verb](/howto/commands/verbs). A type has no _Verbs_ tab, so add the verb as an attribute here, using the verb's attribute name. The verb itself has to exist before objects respond to it, so if it's one of your own, add it once on any object's _Verbs_ tab to create it.

The script has to work for any spell, so write it with `this` - inside a script on a type, `this` is whichever object the script is running on:

```quest
if (this.known) {
  msg ("You already know " + GetDisplayName(this) + ".")
}
else {
  this.known = true
  this.parent = game.pov
  msg (this.learnmsg)
}
```

For the same reason, avoid naming a specific object anywhere in a type's scripts, and avoid [text processor](/howto/text/text-processor) directives that name one.

## Applying a type to an object

Select the object, go to its _Attributes_ tab, and under _Inherited types_ pick your type from the **Add type...** dropdown and click **Add**.

The type's attributes immediately appear in the attributes list below, greyed out, with your type's name in the **Source** column. Add the type to as many objects as you like - `fireball`, `frostbolt`, `invisibility` - and each one is a working spell with no further setup.

To take a type off again, click the ✕ beside it in the _Inherited types_ list. Types that Quest Viva applies to every object, such as `defaultobject`, have no ✕ and can't be removed.

## Overriding an inherited attribute

One object can always disagree with its type. Set the attribute on the object itself, and the object's own value wins:

```quest
fireball.learnmsg = "The page bursts into flame as you read it."
```

In the editor, add the attribute on the object's _Attributes_ tab exactly as you would any other. The greyed inherited row is replaced by your own value, and the other objects of the type are untouched.

To go back to the type's value, delete the object's copy - click the ✕ on its row, or in a script set it to `null`. That removes the object's own attribute rather than blanking it, so the lookup falls through to the type again:

```quest
fireball.learnmsg = null   // back to the type's message
```

## Several types on one object

An object can inherit any number of types, and a type can inherit other types. A `cursed_spell` type can inherit `spell` and add to it:

```xml
<type name="cursed_spell">
  <inherit name="spell"/>
  <curse type="int">5</curse>
</type>
```

If two of an object's types define the same attribute, the one added most recently wins - that is, the last `<inherit>` tag in the XML. Quest Viva searches that type and everything it inherits before it looks at the next type down, so a value inherited indirectly through a late type still beats one defined directly on an earlier type.

You rarely have to work this out in your head: select the attribute on the _Attributes_ tab and the **Source** column names the type whose value is actually in force. If you find yourself relying on the order, that's usually a sign the two types should be one type, or that the object should set the attribute itself.

## Testing for a type in a script

[`DoesInherit`](/reference/functions/objects#doesinherit) tells you whether an object is of a given type, directly or indirectly:

```quest
if (DoesInherit (fireball, "spell")) {
  msg ("You mutter the words under your breath.")
}
```

This is how the Core library decides whether an object is a container, wearable, switchable and so on, and it's the neatest way to write a command that only applies to some of your objects:

```quest
if (not DoesInherit (object, "spell")) {
  msg ("That isn't something you can cast.")
}
```

## Turning an existing object into a type

There is no "extract a type from this object" command in the editor. To move attributes you've already set up on a prototype object into a new type, open the **Raw XML code view** on the toolbar, cut the attribute elements out of the `<object>` and paste them into the `<type>` ([Editing the raw XML](/howto/scripting/raw-xml) explains how to apply changes safely). The alternative is to add the attributes to the type by hand and delete them from the object afterwards.

Either way it's worth deciding up front which attributes belong to the type and which are particular to that one object - the description and alias almost always stay on the object.

## Lists and dictionaries on types

A list or dictionary defined on a type is shared by every object that inherits it, so Quest Viva locks it: `list add (fireball.words, "zap")` raises an error rather than silently changing every spell in the game. See [Mutable attributes on inherited types](/understanding/attributes-and-types#mutable-attributes-on-inherited-types) for the one-line fix.

## See also

- [Attributes and types](/understanding/attributes-and-types) - what inheritance actually does
- [Verbs on types](/howto/commands/verbs#verbs-on-types)
- [Adding a tab for your type](/customise/editor-tabs) - give your type its own editor tab
- [Using libraries](/customise/libraries) - sharing types between games
