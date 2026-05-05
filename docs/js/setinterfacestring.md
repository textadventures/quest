---
title: setInterfaceString
parent: "JS functions"
grand_parent: "Reference"
nav_order: 9
---

    JS.setInterfaceString(string name, string value)

Use this to set the text of the various elements of the user interface. The values allowed for the name are:

> InventoryLabel, StatusLabel, PlacesObjectsLabel, CompassLabel
> InButtonLabel, OutButtonLabel
> EmptyListLabel, NothingSelectedLabel, TypeHereLabel, ContinueLabel

For example, to change the name of the player inventory:

```
JS.setInterfaceString("InventoryLabel", "You are holding")
```
