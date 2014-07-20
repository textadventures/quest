---
layout: index
title: editor element
---

    <editor name="name">attributes</editor>

This defines the Editor tabs and controls for a particular element type or script command.

It should have nested [tab](tab_element.html) elements and [control](control_element.html) elements. "Name" is optional, but if specified it means the nested tab controls can set their [parent](../attributes/parent.html) attribute without having to be nested in the parent editor XML definition.

Attributes:

appliesto  
[string](../types/string.html) specifying which element type or script command this editor definition applies to


