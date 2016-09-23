---
layout: index
title: Using libraries
---

<div class="alert alert-info">
Note: Libraries are currently only available in the Windows desktop version of Quest.

</div>
Libraries allow you to reuse elements in multiple games. That might be [object types](using_inherited_types.html), [functions](creating_functions_which_return_a_value.html), or even common objects. In fact, libraries are the basis of how Quest works - Core.aslx is a library, included by default in all Quest games. It handles much of the standard text adventure game functionality - working out which objects the player can see, handling player commands, implementing containers, and much more. This means that Quest's built-in functionality is extensible and indeed replaceable.

If you've created some functionality in your game that you think would be useful to others, consider turning it into a library, and add a short demo game to show what it can do. This has a number of benefits:

-   Creating a demo will help you to think about other ways the library could be used, and you will end up with a more robust system
-   Others will look at your library, and test it too, perhaps suggesting improves; again, you end up with a more reliable system
-   In a few weeks when your game will not reload for some inexplicable reason, see if the demo will load. If it does, that library is okay, if not, the error is in the library; either way you have helped to isolate the problem.
-   Finally, of course, others can get the benefit of your efforts too.

See [Creating Libraries](../creating_libraries.html) for more information.

You can find libraries at the [Libraries and Code Samples forum](http://textadventures.co.uk/forum/samples). When you've downloaded a library, go to Included Libraries in the tree (under the Advanced section) and click Add to add it to your game.

[Next: Using Javascript](using_javascript.html)
