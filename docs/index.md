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

[Attributes](#Attributes)

[How to...](#Howto)

[Multimedia](#Multimedia)

[The User Experience](#TheUserExperience)

[Handling Characters (NPCs)](#Npcs)

[Creating an RPG](#Rpg)

[Guides to Coding With Quest](#Coding)

[Advanced Topics for Desktop Only](#AdvancedTopics)

[Reference](#Reference)

[Other Guides](#Other)

[Contributing](#Contributing)

[Search](#Search)


You can download a copy (31/Dec/18) of the documentation in a .zip file [here](docs.zip) if you prefer to read it off-line.


<a name="Tutorial"></a>Tutorial
-------------------------------

New to Quest? The [tutorial](tutorial/index.html) will guide you through creating your first game, and is an excellent way to understand what Quest is about.

The Quest Tutorial starts here:

-  [Introduction](tutorial/tutorial_introduction.html)



<a name="Commands"></a>Commands (and Verbs)
---------------------------------------------------

Commands are the heart of a text adventure.

-  [Introduction to commands](commands.html)
-  [Commands for specific rooms](commands_for_room.html)
-  [Using Verbs](using_verbs.html)
-  [Complex commands](complex_commands.html)
-  [Handling multiple objects and ALL](handling_multiple.html)
-  [Pattern matching](pattern_matching.html)
-  [Commands With Unusual Scope](advanced_scope.html)



<a name="Otherfeatures"></a>Features of Quest
---------------------------------------

Quest has a whole range of features built-in and ready to use. You probably won't be using them all, so just dip in as you need to.

-  [The Text Processor](text_processor.html)
-  [Exits](exits.html)
-  [Containers](containers.html)
-  [Switchable objects](switchable.html)
-  [Light and dark](handling_light_and_dark.html)
-  [Clothing](wearables.html)
-  [Score, health and money](score_health_money.html)


<a name="Attributes"></a>Attributes
---------------------------------------

In Quest, everything is an object, from rooms to items, from turn scripts to exits. What makes them work in the way we want them to is attributes.

-  [Introduction to attributes](attributes.html)
-  [Using attributes](using_attributes.html) (including status attributes and change scripts)
-  [Important attributes](important_attributes.html)
-  [Status attributes](status_attributes.html)
-  [Change scripts](change_scripts.html)



<a name="Howto"></a>How to...
---------------------------------------

Some of these will involve some simple coding. _It's not that bad!_ We will walk you through it and you can copy-and-paste all the tricky stuff. All you need to do is change the names so the code applies to things in your game. Look at the second guide to learn how to copy-and-paste code.

-  [Start to code](quest_code.html)
-  [Copy-and-paste Code](copy_and_paste_code.html)
-  [Use functions](functions.html)
-  [Use a `switch` script](multiple_choices___using_a_switch_script.html)
-  [Change the player object](changing_the_player_object.html)
-  [Handle water](handling_water.html)
-  [Show a map](showing_a_map.html)
-  [Ask a question (menu)](ask_simple_question.html)
-  [Ask a question (text input)](asking_a_question.html)
-  [Keep a journal](keeping_a_journal.html)
-  [Keep score](keeping_score.html)
-  [Implement a transit system](transit_system.html)
-  [Transform one (or more) thing to another](convert.html)
-  [Track time](time.html)
-  [Set up a shop](shop.html)
-  [Set up a door](setting_up_door.html)
-  [Give the player a memory or Wiki](memory_or_wiki.html)
-  [Let the player push/pull objects to other rooms](move_object.html)
-  [Use neutral language](neutral_language.html) (grammatically correct responses)
-  [Add randomness](random.html)
-  [Run Quest on Linux](linux.html)


<a name="Multimedia"></a>Multimedia
---------------------------------------

You can add images, sounds and videos to your game. These pages will take your the basics, and on to the more advanced too.

-  [Use multimedia](multimedia.html)
-  [Use images](images.html)
-  [Creating images on the fly](images_on_the_fly.html)
-  [Adding sounds](adding_sounds.html)
-  [Adding videos](adding_videos.html)



<a name="TheUserExperience"></a>The User Experience
---------------------------------------------------

Quest allows you to customise the user interface (UI) to suit the style and mood of your game.

The first two pages require no coding (not even GUI scripts), the next three some very simple coding.

-  [Game-play](ui-game-play.html) (what to consider when designing the interface)
-  [UI style](ui-style.html) (simple options you can set from the game object)
-  [Display verbs](display_verbs.html)
-  [Custom command pane](command_pane.html) (adding an extra pane with simple commands to click)
-  [Simple Customisation](ui-custom.html)
-  [Fonts](ui-fonts.html)

These are rather more advanced, and get in to HTML and JavaScript.

-  [Messing with the location bar](ui-location-bar.md) (have it display the turn and score, or add commands to it)
-  [Custom status pane](custom_panes.html) (adding an extra pane you can do anything with)
-  [ASLEvent: Handling events in JavaScript](ui-callback.html) (have Quest respond to events in the interface)
-  [Dialogue panels](ui-dialogue.html)
-  [Dialogue panels with point buy](ui-dialogue-points.html)

These are more general, and go into detail about the principles of UI customisation.

-  [Customisation part 1](ui-javascript.html) (three pages that explore customisation in detail)
-  [Customisation part 2](ui-javascript2.html)
-  [Customisation part 3](ui-javascript3.html)




<a name="Npcs"></a>Handling Characters (NPCs)
---------------------------------------------

Adding people that the player can interactive with can help bring your game to life, but is hard to do well. These pages will get you started.

-  [Following the player](follower.html)
-  [Introduction to Conversations](conversations.html)
-  [Speak to](speak_to.html)
-  [Ask/Tell](ask_about.html)
-  [Patrolling NPCs](patrolling_npcs.html)
-  [Independent NPCs](independent_npcs.html)
  

  
<a name="Rpg"></a>Creating an RPG-style game
---------------------------------------------

Quest can be used to create an RPG-style game, in which the player has a set of statistics, and these are used to determine success in combat and other situations. The Zombie Apocalypse is an example used to walk you through one possible way of doing it. You do not need to have done the first two parts to use the spells. None of them involve a character creation process.

-  [Introduction](rpg-intro.html)
-  [Character creation](character_creation.html)
-  [The Zombie Apocalypse Part 1](zombie-apocalypse-1.html)
-  [The Zombie Apocalypse Part 2](zombie-apocalypse-2.html)
-  [Spells for the Zombie Apocalypse](zombie-apocalypse-spells.html)


  

<a name="Coding"></a>Guides to Coding With Quest
---------------------------------------

A look at some of the more technical features of Quest. These articles assume you can at least copy-and-paste code. The first takes you through creating a very short, but technically complex, game, and is, if you like, the big brother of the tutorial.

-  [Cloak of Darkness](cloak_of_darkness.html)
-  [Introduction to Coding](introtocoding.html)
-  [Functions](creating_functions_which_return_a_value.html)
-  [Lists](using_lists.html)
-  [Dictionaries](using_dictionaries.html)
-  [Turnscripts](using_turnscripts.html)
-  [Scopes](scopes.html)
-  [Clones](clones.html)
-  [Advanced game scripts](advanced_game_scripts.html)
-  [Javascript](using_javascript.html)


<a name="AdvancedTopics"></a>Advanced Topics for Desktop Only
---------------------------------------------

Due to restrictions in the web editor, the following are only applicable to the desktop version.

-  [Walkthroughs](using_walkthroughs.html)
-  [Overriding functions](overriding.html)
-  [Types](using_inherited_types.html)
-  [Tabs for types](tabs_for_types.html)
-  [Templates](changing_templates.html)
-  [Translating Quest](translating_quest.html)
-  [Libraries](using_libraries.html)
-  [Editor User Interface Elements](editor_user_interface_elements.html)
-  [Debugging your game](debugging_your_game.html)


    
<a name="Reference"></a>Reference
---------------------------------

-   [Using Trizbort](trizbort.html) Draw a map in Trizbort, then export to Quest.
-   [DevMode](devmode.html) A tool for testing and debugging your game.
-   [Common problems](problems.html)
-   [What counts as "adult"](adult.html)
-   [WebEditor](webeditor.html)
-   [Code view](codeview.html)
-   [Notes for users of previous versions of Quest](upgrade_notes.html)
-   [ASL Requirements](asl_requirements.html)
-   ["Undo" support](undo_support.html)
-   [Script Commands](scripts/)
-   [Functions](functions/)
-   [Attribute Types](types/)
-   [Elements](elements/)
-   [Javascript functions](js/)
-   [Null](null.html)
-   [About SAVE](about_save.html)
-   [Using doubles](using_doubles.html)
-   [ASLX File Format](aslx.html)
-   [Editing in Full Code View](full_code_view.html)
-   [Expressions](expressions.html)
-   [Competition entries](competition_entry.html)
-   [Publishing games](publishing.html)
-   [Configuring Quest](configuring_quest.html)
-   [Docs Style Guide](style_guide.html)
-   [History of Quest](history.html)
-   [Quest Source Code](source_code.md)
-   [Unit Testing](unit_testing.html)


<a name="Other"></a>Other Guides and Libraries
---------------------------------

-   [Helpsheets for beginners](http://docs.textadventures.co.uk/quest/helpsheets/) by gideonwilliams
-   [The Pixie's tutorials and libraries](https://github.com/ThePix/quest/wiki)
-   [KV's libraries](https://github.com/KVonGit/quest5-stuff)
-   [SoonGames' libraries](https://github.com/SoonGames/quest_libraries)
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
