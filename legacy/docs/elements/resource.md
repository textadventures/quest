---
layout: index
title: resource element
---

    <resource src="filename"/>

Specifies that a particular file should be included when building a .quest package.

**This is usually not required** - the Packager will pick up all supported files in the same directory as the game. The only time this is required is when an additional file in the library directory is required - so this element is only intended to be used by the Core library.
