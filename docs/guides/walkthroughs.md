---
layout: index
title: Walkthroughs
---

You can specify a walkthrough to let you automatically test your game.

     <walkthrough name="main">
       <steps>
         look at thing
         take thing
         drop thing
         go east
         speak to guard
         give watch to guard
       <steps>
     <walkthrough>

You can then run the walkthrough by clicking Tools, Walkthrough.

Walkthroughs can share steps - useful if you want to test multiple "branches" of your game. You can set up a hierarchy of walkthroughs. When you run a walkthrough, any steps in its parent(s) are also run.

For example:

     <walkthrough name="root">
       <steps>
         east
         look at poison
       <steps>
       <walkthrough name="death">
          <steps>
             eat poison
          </steps>
       </walkthrough>
       <walkthrough name="life">
          <steps>
             give poison to Jack the Ripper
          </steps>
       </walkthrough>
     <walkthrough>

Here we have two walkthroughs "life" and "death" which descend from a common "root" walkthrough. Running the "death" walkthrough will run the sequence "east/look at poison/eat poison" while running "life" runs "east/look at poison/give poison to Jack the Ripper".

Handling menus and questions
----------------------------

You can add a menu selection to a walkthrough by preceding it with "menu:"

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

Record
------

The easiest way to create a walk-through is to click the record button, and then just do in game what you want in the walk-through. What is particularly neat is that you can click record for an existing walk-through, and Quest will get you to the end of that, and then record after that, adding your new actions to the end of the old walk-through.
