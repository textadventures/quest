---
layout: index
title: Quest 5 - Documentation
---

Quest 5 is free, [open source](open_source.html) software for creating text adventure games. It is designed to be powerful, extensible and easy to learn. You can create games in any language - Quest currently has templates for English, French, German, Spanish, Dutch, Italian, Portuguese, Romanian, Esperanto, Russian and Icelandic.

You can [use Quest in your web browser](http://textadventures.co.uk/create), or [download a version for Windows](http://textadventures.co.uk/quest/desktop) (or start with the web, and if you like it, download the game you are working on, and finish it on the desktop version).


Contents
--------

[Tutorial](#Tutorial)

[Commands (and Verbs)](#Commands)

[Features of Quest](#Otherfeatures)

[How to...](#Howto)

[The User Experience](#TheUserExperience)

[Handling Characters (NPCs)](#Npcs)

[Guides to Coding With Quest](#Coding)

[Advanced Topics for Desktop Only](#AdvancedTopics)

[Reference](#Reference)

[Contributing](#Contributing)

[Search](#Search)




<a name="Tutorial"></a>Tutorial
-------------------------------

New to Quest? The [tutorial](tutorial/index.html) will guide you through creating your first game, and is an excellent way to understand what Quest is about.

The Quest Tutorial starts here:

-  [Introduction](tutorial/tutorial_introduction.html)



<a name="Commands"></a>Commands (and Verbs)
---------------------------------------------------

Commands are the heart of a text adventure.

-  [Commands](commands.html)
-  [Commands for specific rooms](commands_for_room.html)
-  [Using Verbs](using_verbs.html)
-  [Complex commands](complex_commands.html)
-  [Pattern matching](pattern_matching.html)
-  [Commands With Unusual Scope](advanced_scope.html)



<a name="Otherfeatures"></a>Features of Quest
---------------------------------------

Quest has a whole range of features built in and ready to use. You probably won't be using them all, so just dip in as you need to.

-  [The Text Processor](text_processor.html)
-  [Lockable exits](using_lockable_exits.html)
-  [Multimedia](multimedia.html)
-  [Important attributes](important_attributes.html)
-  [Status attributes](tutorial/status_attributes.html) (link to the tutorial page)
-  [Using functions](functions.html)
-  [Using a switch script](multiple_choices___using_a_switch_script.html)
-  [Using containers](tutorial/using_containers.html) (link to the tutorial page)
-  [Using timers and turn scripts](tutorial/using_timers_and_turn_scripts.html) (link to the tutorial page)
-  [Using score, health and money](score_health_money.html)



<a name="Howto"></a>How to...
---------------------------------------

Some of these will involve some simple coding. _It's not that bad!_ We will walk you through and you can copy-and-paste all the tricky stuff. All you need to do is change the names so the code applies to things in your game. Look at the first guide to learn how to copy-and-paste code.

-  [Introduction to coding](quest_code.html)
-  [Copy-and-paste Code](copy_and_paste_code.html)
-  [Change the player object](changing_the_player_object.html)
-  [Light and dark](handling_light_and_dark.html)
-  [Handle water](handling_water.html)
-  [Showing a map](showing_a_map.html)
-  [Adding clothing to your game](wearables.html)
-  [Ask a question](asking_a_question.html)
-  [Keep a journal](keeping_a_journal.html)
-  [Keep score](keeping_score.html)
-  [Implement a transit system](transit_system.html)
-  [Transform one (or more) thing to another](convert.html)
-  [Tracking time](time.html)
-  [Setting up a shop](shop.md)





<a name="TheUserExperience"></a>The User Experience
---------------------------------------------------

Quest allows you to customise the user interface (UI) to suit the style and mood of your game.

-  [Game-play](ui-game-play.html)
-  [UI style](ui-style.html)
-  [Display verbs](display_verbs.html)
-  [Custom commands and status pane](custom_panes.html)
-  [Further options with JavaScript](ui-javascript.html)


<a name="Npcs"></a>Handling Characters (NPCs)
---------------------------------------------

Adding people that the player can interactive with can help bring your game to life, but is hard to do well. These pages will get you started.

-  [Following the player](follower.html)
-  [Introduction to Conversations](conversations.html)
-  [Ask/Tell](ask_about.html)
  


<a name="Coding"></a>Guides to Coding With Quest
---------------------------------------

A look at some of the more technical features of Quest. These articles assume you can at least copy-and-paste code.

-  [Functions](creating_functions_which_return_a_value.html)
-  [Lists](using_lists.html)
-  [Dictionaries](using_dictionaries.html)
-  [Scopes](scopes.html)
-  [Change scripts](change_scripts.html)
-  [Javascript](using_javascript.html)


<a name="AdvancedTopics"></a>Advanced Topics for Desktop Only
---------------------------------------------

Due to restrictions in the web editor, the following are only applicable to the desktop version.

-  [Walkthroughs](using_walkthroughs.html)
-  [Overriding functions](overriding.html)
-  [Types](using_inherited_types.html)
-  [Templates](changing_templates.html)
-  [Translating Quest](translating_quest.html)
-  [Libraries](using_libraries.html)
-  [Editor User Interface Elements](editor_user_interface_elements.html)
-  [Debugging your game](debugging_your_game.html)


    
<a name="Reference"></a>Reference
---------------------------------

-   [WebEditor](webeditor.html)
-   [Notes for users of previous versions of Quest](upgrade_notes.html)
-   [ASL Requirements](asl_requirements.html)
-   ["Undo" support](undo_support.html)
-   [Script Commands](scripts/)
-   [Functions](functions/)
-   [Attribute Types](types/)
-   [Elements](elements/)
-   [ASLX File Format](aslx.html)
-   [Expressions](expressions.html)
-   [Configuring Quest](configuring_quest.html)
-   [User contributed libraries](libraries.html)
-   [User contributed guides](guides/)



<a name="Contributing"></a>Contributing
---------------------------------------

If you would like to help with developing Quest, please see the [Developers](developers.html) page.

If you find a bug in Quest (as opposed to your own game), please log it on the [Issue Tracker](https://github.com/textadventures/quest/issues), or if there is a feature you would like included. We cannot guarantee all bugs and feature requests will be addressed, but they are more likely to be if reported here. Try to include as much detail as possible, includiong a same game that illustrates the issue if at all possible.

You can also discuss Quest at [the forum](http://textadventures.co.uk/forum/quest). If you have an issue with your game, this is the place to go!

Quest is completely open source, including this documentation! The source code and documentation both live [on GitHub](https://github.com/textadventures/quest) (documentation is in the `docs` folder).



<a name="Search"></a>Search
---------------------------

<script>
  (function() {
    var cx = '015306987908116640949:jr9g5bqdxsa';
    var gcse = document.createElement('script');
    gcse.type = 'text/javascript';
    gcse.async = true;
    gcse.src = (document.location.protocol == 'https:' ? 'https:' : 'http:') +
        '//www.google.com/cse/cse.js?cx=' + cx;
    var s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(gcse, s);
  })();
</script>

<style>
	.gcs,
	.gcs *,
	.gcs *:before,
	.gcs *:after {
	  -webkit-box-sizing: content-box;
	     -moz-box-sizing: content-box;
	          box-sizing: content-box;
	}
</style>

<div class="gcs">
	<gcse:searchbox-only></gcse:searchbox-only>
</div>
