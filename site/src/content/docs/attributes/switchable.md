---
title: switchable
---

The "switchable" type is defined in CoreTypes.aslx.

It marks the object as switched off ([switchedon](/attributes/switchedon) = false), and adds "turn on/switch on" and "turn off/switch off" verbs to the object. These provide a default implementation so that sensible responses are printed when the player tries to switch on an object twice etc. The verbs simply toggle the state of the [switchedon](/attributes/switchedon) attribute. You can use [onswitchon](/attributes/onswitchon) and [onswitchoff](/attributes/onswitchoff) to trigger events in the game when the player switches the object on or off.
