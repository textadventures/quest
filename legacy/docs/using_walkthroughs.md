---
layout: index
title: Using walkthroughs
---

<div class="alert alert-info">
Note: Walkthroughs are currently only available in the Windows desktop version of Quest.

</div>

What is a walkthrough?
----------------------

A walkthrough is the set of steps or commands that you can record in the editor, and then play during the game. Quest lets you record and play back walkthroughs, allowing you to test your game - after making changes, you can run your walkthrough to ensure your game is still winnable, and that any alternative endings also still work.

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

Currently you need to go into Full Code View to create a sub-walkthrough.


Handling menus and questions
----------------------------

You can manually add a menu selection to a walkthrough by preceding it with "menu:"

For example if you have two objects "potato" and "potassium", you can put this in the walkthrough:

     take pot
     menu:potato

You can also add an answer to a question by preceding it with "answer:"

For example if somebody asks you a question when you speak to them, put this in the walkthrough:

     speak to Bill
     answer:yes

If you forget to put in these two statements, the walkthrough will immediately stop.

Assertions
----------

As of Quest 5.1, you can use walkthrough assertions to test your game. Simply include a line starting with "assert:", and then any expression which should be true.

For example, in the walkthrough below, the assert expression checks that the "take biscuit" command succeeded:

     <walkthrough name="main">
       <steps>
         look at tin
         open tin
         look at biscuit
         take biscuit
         assert:biscuit.parent = player
       <steps>
     <walkthrough>

If a walkthrough assert expression returns false, the walkthrough is immediately stopped.

Displaying runtime
------------------

As of Quest 5.9 you can include the line

     runtime:

anywhere in the walkthrough to display the total runtime of the walkthrough. The command can also be used several times in a walkthrough.

Output speed
------------

From Quest 5.9 you can influence the speed of the output with the line "delay:". The delay in milliseconds is specified behind it. The setting takes effect after the delay command line.

     <walkthrough name="main">
       <steps>
         look 
         get apple
         delay:1000
         examine apple
         north
         look
         delay:200
         examine horse
         use apple with horse
         runtime:
       <steps>
     <walkthrough>

The command can also be used several times in a walkthrough if certain areas are to be displayed with their own speed.
