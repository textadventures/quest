---
title: editor element
sidebar:
  order: 20
---

    <editor name="name">attributes</editor>

This defines the Editor tabs and controls for a particular element type or script command.

It should have nested [tab](/elements/tab) elements and [control](/elements/control) elements. "Name" is optional, but if specified it means the nested tab controls can set their [parent](/attributes/parent) attribute without having to be nested in the parent editor XML definition.

Attributes:

appliesto  
[string](/types/string) specifying which element type or script command this editor definition applies to


