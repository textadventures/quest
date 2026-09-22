---
title: Features
sidebar:
  order: 9
---

Quest Viva hides most of its options until you ask for them. Rather than showing every author every tab, it puts a set of checkboxes on a _Features_ tab — one on the game object, one on each object — and each ticked box reveals the tab where that feature is actually set up.

This keeps the editor manageable, but it does mean that if you are looking for an option and cannot find the tab it lives on, the _Features_ tab is the first place to check.

Ticking a feature does not change how your game behaves on its own. It only makes a tab appear. The behaviour comes from what you then set on that tab.

## Game features

The _Features_ tab of the game object turns on optional systems for the whole game.

| Feature | What it enables |
| --- | --- |
| Score | A score attribute for the player, shown in the status pane. See [Score, health and money](/howto/score/score-health-money). |
| Health | A health attribute for the player, shown in the status pane, plus a "script to run when health reaches zero" on the game's _Player_ tab. Also adds the "Change health by" box to the [_Edible_ tab](/howto/objects/food). |
| Money | A money attribute for the player, and a "Price" box on each object's [_Inventory_ tab](/howto/objects/taking-and-dropping). |
| Inventory limits | Limits on how much the player can carry, by number of objects or by volume. See [Inventory limits](/howto/objects/taking-and-dropping#inventory-limits). |
| Lightness and darkness | Dark rooms and light sources. Adds a _Light/Dark_ tab to every room, and a light/dark feature to each object's own _Features_ tab. See [Handling light and dark](/howto/rooms/light-and-darkness). |
| Ask/Tell | `ASK ABOUT` and `TELL ABOUT` topics on characters. Adds an _Ask/Tell_ tab to every object. See [Building an Ask/Tell system](/howto/characters/ask-tell). |
| Annotations | A _Notes_ tab on rooms, for your own notes. See [Annotations](#annotations) below. |
| Show advanced options for wearables | Layers and slots for clothing, so one garment can cover another. See [Clothing with several states](/howto/objects/clothing#clothing-with-several-states). |
| Advanced scripts | The game's _Advanced Scripts_ tab, holding `inituserinterface`, `unresolvedcommandhandler` and `scopebackdrop`. See [Advanced game scripts](/howto/scripting/advanced-game-scripts). |
| In-room descriptions | An "In-room description" box on every object, whose text is appended to the description of whatever room the object is in. See [the _Setup_ tab](/howto/rooms/objects-and-rooms#the-setup-tab). |
| Multiple commands | Lets the player put several commands on one line, separated by full stops — `GET LAMP. LIGHT LAMP. GO NORTH`. Each is run as a separate turn. |

Three more features are elsewhere, because they change how the game looks: hyperlinks are on the _Display_ tab, and the map and drawing grid and the picture frame are on the _Interface_ tab. See [Look and feel](/customise/look-and-feel#the-display-tab).

Turning a feature off again does not delete anything you set up while it was on; the tab simply disappears, and the attributes stay where they are. That makes it safe to turn a feature on to see what it offers.

## Object features

The _Features_ tab of an object decides what kind of thing that object is. As the tab itself says: select an option here to have a new tab display, then go to that tab to set the object up.

| Feature | Tab it reveals |
| --- | --- |
| Use/Give | _Use/Give_ — what happens when the object is used on its own, used with something else, or given to a character. See [More things to do with objects](/tutorial/more-things-to-do-with-objects). |
| Container | _Container_ — whether the object is a container or a surface, and whether it can be opened, closed and locked. See [Using containers](/howto/objects/containers). |
| Switchable | _Switchable_ — on and off, with messages and scripts for each. See [Items that can be switched on and off](/howto/objects/switchable). |
| Edible | _Edible_ — what happens when the object is eaten. See [Items that can be eaten](/howto/objects/food). |
| Wearable | _Wearable_ — putting the object on and taking it off. See [Wearable items](/howto/objects/clothing). |
| Lightness and darkness | _Light/Dark_ — whether the object is a light source, and how bright. Only offered if the game-level lightness and darkness feature is on. See [Handling light and dark](/howto/rooms/light-and-darkness). |
| Player | _Player_ — lets the player become this object, and sets the name and gender used while they are. See [Changing the player object](/howto/player/player-object). |
| Initialisation script | _Initialisation script_ — a script that runs once for this object as the game starts. See [Object initialisation scripts](/understanding/when-scripts-run#object-initialisation-scripts). |

Rooms have no _Features_ tab. The features above are all about things the player can pick up, open or wear, none of which apply to a room.

A feature on its own is still not enough for the first four in that list: the tab it reveals starts with a dropdown ("Can be eaten", "Object is a container", and so on) that is set to the inactive option. Tick the feature, go to the tab, and choose the option — otherwise nothing has changed.

## Annotations

With the "Annotations" game feature on, every room gains a _Notes_ tab: a free-text box for your own notes about that room. Nothing you write there is ever shown to the player, and it has no effect on the game.

It is a good place for the things that do not fit anywhere else — a puzzle you have half-designed, a reminder that this room's description needs rewriting, a note of which other rooms depend on the state you set here. In a game large enough that you cannot hold all of it in your head, notes attached to the room they concern are much easier to find again than a document kept separately.

Notes are saved in your game file and are inlined into the published game like any other attribute, so treat them as something a determined player could read.
