---
title: Taking and dropping objects
sidebar:
  order: 10
---

Whether the player can pick an object up, what they are told when they try, and what happens afterwards are all set on the object's _Inventory_ tab.

By default a new object cannot be taken at all: the player is told they can't have that. Tick "Take" to change that, and the rest of the tab's options become useful.

## Take

The "Take" option has two settings, plus a checkbox that turns the whole thing on and off.

**Default behaviour** moves the object into the player's inventory and prints a message. Leave "Take message" blank and the player sees the standard message for your game's language ("You pick it up."); fill it in and they see your text instead:

```
You wrestle the crowbar free from the rubble.
```

**Run a script** hands the whole thing over to you. This is the important difference: when take is a script, Quest Viva does *not* move the object. If the player should end up holding it, your script has to say so:

```quest
if (player.hasgloves) {
  msg ("Protected by the gloves, you lift the shard of glass.")
  MoveObject (this, player)
}
else {
  msg ("It is far too sharp to pick up with bare hands.")
}
```

A script is the right choice whenever taking the object can fail for a reason the player should be told about, or whenever picking it up should trigger something — a trap, a change in the room, a character noticing.

### After taking the object

If you want the object to move normally *and* something extra to happen, leave "Take" on default behaviour and use the "After taking the object" script instead. It runs after the take message has been printed:

```quest
msg ("As you lift the idol, the floor begins to rumble.")
EnableTurnScript (templecollapse)
```

### Objects excluded from TAKE ALL

`TAKE ALL` picks up everything the player can reach. Tick "Object is excluded when entering TAKE ALL" for anything that should not be swept up that way — something enormous, something that belongs to somebody, something whose taking should be a deliberate act.

The object can still be taken by name; it is only excluded from `ALL`. Scenery is excluded automatically, so you do not need this for that.

## Drop

Drop works the same way as take, in mirror image: default behaviour with an optional message, or a script that has to do the moving itself.

One difference is worth knowing. A drop script is passed a `destination` parameter — the place the object would have gone, which is normally the player's current room but can be redirected (see [drop destinations](/howto/world/objects-and-rooms#the-room-tab)). Use it rather than assuming `player.parent`:

```quest
MoveObject (this, destination)
msg ("You put the vase down very carefully.")
```

The "Run script after" script — the drop equivalent of "after taking" — is passed a Boolean telling you whether the object actually moved. The variable is called `successful`:

```quest
if (successful) {
  msg ("The lamp goes out as it leaves your hand.")
  lamp.lightsource = false
}
```

## Scenery and taking

If the player successfully takes an object that was marked as [scenery](/howto/world/objects-and-rooms#the-setup-tab), Quest Viva clears the scenery flag for you. An object the player is carrying is not scenery any more, and would otherwise vanish from the room description of wherever they eventually dropped it.

## Price

With the "Money" game feature enabled, a "Price" box appears on this tab. It holds what the object costs, for use with a shop. Setting it does not by itself make the object buyable — see [Setting up a shop](/howto/tasks/shop) for the rest.

## Inventory limits

With the "Inventory limits" game feature enabled, the player object's _Inventory_ tab gains a section for limiting what can be carried, and every object gains a "Volume" box.

There are two independent limits, and either can be left unset:

- **Maximum objects** — how many things the player can carry at once. Set it to 0 for no limit.
- **Maximum volume of inventory** — the total volume the player can carry. Each object's volume defaults to nothing, so give a volume to anything bulky.

Both limits count everything the player is carrying, including the contents of containers they are carrying — filling a rucksack does not get around a volume limit.

When a take would breach either limit, the player is told the inventory is full. You can replace the standard wording with the "Full container message" boxes beneath each limit.

Limits are checked before the take happens, so an object that would not fit is never picked up, and the "after taking" script does not run.
