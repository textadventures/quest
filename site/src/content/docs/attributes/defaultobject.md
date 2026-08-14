---
title: defaultobject
---

The "defaultobject" type is defined in CoreTypes.aslx. It is automatically inherited by all objects so there is no need to inherit this directly.

This type defines default behaviour for an object:

-   [displayverbs](/attributes/displayverbs) "Look at" and "Take"
-   [inventoryverbs](/attributes/inventoryverbs) "Look at", "Use" and "Drop"
-   not [takeable](/attributes/take)
-   [droppable](/attributes/drop)
-   [gender](/attributes/gender) and [article](/attributes/article) use NeutralGender and NeutralArticle templates
-   not a [container](/attributes/container), therefore not [open](/attributes/isopen), [openable](/attributes/open) or [closeable](/attributes/close)
-   default [descprefix](/attributes/descprefix), [objectslistprefix](/attributes/objectslistprefix), [exitslistprefix](/attributes/exitslistprefix), [contentsprefix](/attributes/contentsprefix)
-   empty [description](/attributes/description)
-   not [scenery](/attributes/scenery)
-   [hidechildren](/attributes/hidechildren) and [listchildren](/attributes/listchildren) both false
-   [usedefaultprefix](/attributes/usedefaultprefix) is true
-   implementation of [onopen](/attributes/onopen), [onclose](/attributes/onclose), [onlock](/attributes/onlock), [onunlock](/attributes/onunlock), [onswitchon](/attributes/onswitchon) and [onswitchoff](/attributes/onswitchoff) by triggering a script when the values of [isopen](/attributes/isopen), [locked](/attributes/locked) and [switchedon](/attributes/switchedon) change

