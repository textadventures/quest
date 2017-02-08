---
layout: index
title: Using walkthroughs
---

<div class="alert alert-info">
Note: Walkthroughs are currently only available in the Windows desktop version of Quest.

</div>

What is a walkthrough?
----------------------

A walkthrough is the set of steps required to reach one of the endings in your game (or a part of those steps). Quest lets you record and play back walkthroughs, allowing you to test your game - after making changes, you can run your walkthrough to ensure your game is still winnable, and that any alternative endings also still work.

Another use for walkthroughs is when you have a problem with Quest - it helps hugely if we can play your game through to the required point, and a walkthrough will take us straight there.

You can record and play back walkthroughs from the Editor, or you can also play a walkthrough from within a game by going to the Tools menu. Walkthroughs are automatically removed from published .quest files.

Creating and recording a walkthrough
------------------------------------

To add a walkthrough, you can right-click the tree and choose "Add Walkthrough", or you can go via the "Add" menu. Give the walkthrough a name to describe it, for example "win game".

Now you'll see the walkthrough editor. Here you can add, edit and delete steps manually, and you can also click the Play and Record buttons. If you click Record, any existing steps in your walkthrough will be run, and then any new moves that you make will be added to the end. Click the Record button now and make a few moves. When you're done, click File, then Stop Game.

You'll see that the walkthrough editor has now been updated with the moves you made in the game. The walkthrough will also record any selections you made from menus which appeared.

If you want to add steps to an existing walkthrough, choose this walkthrough und click the Record button. The walkthrough is then executed and all new moves will be appended.

Creating sub-walkthroughs
-------------------------

Many of your walkthroughs may share the same steps - for example, if your game has multiple endings, there may be points in the walkthrough where you want to "branch off". Quest lets you handle this by creating a hierarchy of walkthroughs - if you move one walkthrough in the tree to be a child of another walkthrough, when the child walkthrough is run, it will run all the steps of its parent walkthrough(s) first.