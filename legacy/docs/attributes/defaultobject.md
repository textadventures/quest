---
layout: index
title: defaultobject
---

The "defaultobject" type is defined in CoreTypes.aslx. It is automatically inherited by all objects so there is no need to inherit this directly.

This type defines default behaviour for an object:

-   [displayverbs](displayverbs.html) "Look at" and "Take"
-   [inventoryverbs](inventoryverbs.html) "Look at", "Use" and "Drop"
-   not [takeable](take.html)
-   [droppable](drop.html)
-   [gender](gender.html) and [article](article.html) use NeutralGender and NeutralArticle templates
-   not a [container](container.html), therefore not [open](isopen.html), [openable](open.html) or [closeable](close.html)
-   default [descprefix](descprefix.html), [objectslistprefix](objectslistprefix.html), [exitslistprefix](exitslistprefix.html), [contentsprefix](contentsprefix.html)
-   empty [description](description.html)
-   not [scenery](scenery.html)
-   [hidechildren](hidechildren.html) and [listchildren](listchildren.html) both false
-   [usedefaultprefix](usedefaultprefix.html) is true
-   implementation of [onopen](onopen.html), [onclose](onclose.html), [onlock](onlock.html), [onunlock](onunlock.html), [onswitchon](onswitchon.html) and [onswitchoff](onswitchoff.html) by triggering a script when the values of [isopen](isopen.html), [locked](locked.html) and [switchedon](switchedon.html) change

