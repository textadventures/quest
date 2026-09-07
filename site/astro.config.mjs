// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import { questGrammar } from "./src/quest-grammar.mjs";

// https://astro.build/config
export default defineConfig({
    site: "https://questviva.com",
    redirects: {
        "/tutorial/cloak_of_darkness": "/cloak-of-darkness",
        // Merged into the Introduction - it had become the same page with
        // screenshots, and both sat at the top of Start Here.
        "/overview": "/intro",
        // Underscore -> dash URL cleanup, and Quest -> Quest Viva renames.
        "/about_attributes": "/about-attributes",
        "/advanced-topics/about_types": "/advanced-topics/about-types",
        "/advanced-topics/editor_user_interface_elements": "/advanced-topics/editor-user-interface-elements",
        "/advanced-topics/tabs_for_types": "/advanced-topics/tabs-for-types",
        "/advanced-topics/translating_quest": "/advanced-topics/translation",
        "/advanced-topics/undo_support": "/advanced-topics/undo-support",
        "/advanced-topics/using_delegates": "/advanced-topics/using-delegates",
        "/advanced-topics/using_inherited_types": "/advanced-topics/using-inherited-types",
        "/advanced-topics/using_libraries": "/advanced-topics/using-libraries",
        "/asl_requirements": "/asl-requirements",
        "/change_scripts": "/change-scripts",
        "/cloak_of_darkness": "/cloak-of-darkness",
        "/developers/open_source": "/developers/open-source",
        "/developers/source_code": "/developers/source-code",
        "/howto/commands/advanced_scope": "/howto/commands/advanced-scope",
        "/howto/commands/commands_for_room": "/howto/commands/commands-for-room",
        "/howto/commands/complex_commands": "/howto/commands/complex-commands",
        "/howto/commands/handling_multiple": "/howto/commands/handling-multiple",
        "/howto/commands/pattern_matching": "/howto/commands/pattern-matching",
        "/howto/commands/using_verbs": "/howto/commands/using-verbs",
        "/howto/multimedia/adding_sounds": "/howto/multimedia/adding-sounds",
        "/howto/multimedia/adding_videos": "/howto/multimedia/adding-videos",
        "/howto/multimedia/images_on_the_fly": "/howto/multimedia/images-on-the-fly",
        "/howto/npcs/ask_about": "/howto/npcs/ask-about",
        "/howto/npcs/dialogue_pages": "/howto/npcs/dialogue-pages",
        "/howto/npcs/independent_npcs": "/howto/npcs/independent-npcs",
        "/howto/npcs/patrolling_npcs": "/howto/npcs/patrolling-npcs",
        "/howto/npcs/speak_to": "/howto/npcs/speak-to",
        "/howto/rpg/character_creation": "/howto/rpg/character-creation",
        "/howto/scripting/advanced_game_scripts": "/howto/scripting/advanced-game-scripts",
        "/howto/scripting/blocks_and_scripts": "/howto/scripting/blocks-and-scripts",
        "/howto/scripting/copy_and_paste_code": "/howto/scripting/copy-and-paste-code",
        "/howto/scripting/creating_functions_which_return_a_value": "/howto/scripting/creating-functions-which-return-a-value",
        "/howto/scripting/debugging_your_game": "/howto/scripting/debugging-your-game",
        "/howto/scripting/unit_testing": "/howto/scripting/unit-testing",
        "/howto/scripting/using_dictionaries": "/howto/scripting/using-dictionaries",
        "/howto/scripting/using_doubles": "/howto/scripting/using-doubles",
        "/howto/scripting/using_lists": "/howto/scripting/using-lists",
        "/howto/scripting/using_turnscripts": "/howto/scripting/using-turnscripts",
        "/howto/scripting/using_walkthroughs": "/howto/scripting/using-walkthroughs",
        "/howto/tasks/about_functions": "/howto/tasks/about-functions",
        "/howto/tasks/ask_simple_question": "/howto/tasks/ask-simple-question",
        "/howto/tasks/asking_a_question": "/howto/tasks/asking-a-question",
        "/howto/tasks/changing_the_player_object": "/howto/tasks/changing-the-player-object",
        "/howto/tasks/handling_water": "/howto/tasks/handling-water",
        "/howto/tasks/keeping_a_journal": "/howto/tasks/keeping-a-journal",
        "/howto/tasks/keeping_score": "/howto/tasks/keeping-score",
        "/howto/tasks/memory_or_wiki": "/howto/tasks/memory-or-wiki",
        "/howto/tasks/move_object": "/howto/tasks/move-object",
        "/howto/tasks/multiple_choices_using_a_switch_script": "/howto/tasks/multiple-choices-using-a-switch-script",
        "/howto/tasks/neutral_language": "/howto/tasks/neutral-language",
        "/howto/tasks/setting_up_door": "/howto/tasks/setting-up-door",
        "/howto/tasks/showing_a_map": "/howto/tasks/showing-a-map",
        "/howto/tasks/showing_a_menu": "/howto/tasks/showing-a-menu",
        "/howto/tasks/transit_system": "/howto/tasks/transit-system",
        "/howto/tasks/use_maths_functionality": "/howto/tasks/use-maths-functionality",
        "/howto/ux/command_pane": "/howto/ux/command-pane",
        "/howto/ux/custom_panes": "/howto/ux/custom-panes",
        "/howto/ux/customising_the_ui": "/howto/ux/customising-the-ui",
        "/howto/ux/display_verbs": "/howto/ux/display-verbs",
        "/howto/world/about_save": "/howto/world/about-save",
        "/howto/world/changing_templates": "/howto/world/changing-templates",
        "/howto/world/handling_light_and_dark": "/howto/world/handling-light-and-dark",
        "/howto/world/score_health_money": "/howto/world/score-health-money",
        "/howto/world/text_processor": "/howto/world/text-processor",
        "/important_attributes": "/important-attributes",
        "/other_guides/a_hint_system": "/other-guides/a-hint-system",
        "/other_guides/community_guides": "/other-guides/community-guides",
        "/other_guides/hyperlinks": "/other-guides/hyperlinks",
        "/other_guides/immobilise_the_player": "/other-guides/immobilise-the-player",
        "/other_guides/implementing_components_of_an_object": "/other-guides/implementing-components-of-an-object",
        "/other_guides/invisiclues": "/other-guides/invisiclues",
        "/other_guides/port_and_starboard": "/other-guides/port-and-starboard",
        "/other_guides/random_default_answers": "/other-guides/random-default-answers",
        "/other_guides/starting_inventory": "/other-guides/starting-inventory",
        "/other_guides/timelimitedpuzzles": "/other-guides/timelimitedpuzzles",
        "/other_guides/turn_based_events": "/other-guides/turn-based-events",
        "/other_guides/unlockdoor": "/other-guides/unlockdoor",
        "/publishing/competition_entry": "/publishing/competition-entry",
        "/status_attributes": "/status-attributes",
        "/tutorial/anatomy_of_a_quest_game": "/tutorial/anatomy-of-a-quest-viva-game",
        "/tutorial/creating_a_gamebook": "/tutorial/creating-a-gamebook",
        "/tutorial/creating_a_simple_game": "/tutorial/creating-a-simple-game",
        "/tutorial/custom_attributes": "/tutorial/custom-attributes",
        "/tutorial/custom_commands": "/tutorial/custom-commands",
        "/tutorial/interacting_with_objects": "/tutorial/interacting-with-objects",
        "/tutorial/more_things_to_do_with_objects": "/tutorial/more-things-to-do-with-objects",
        "/tutorial/moving_objects_during_the_game": "/tutorial/moving-objects-during-the-game",
        "/tutorial/releasing_your_game": "/tutorial/releasing-your-game",
        "/tutorial/status_attributes": "/tutorial/status-attributes",
        "/tutorial/tutorial_introduction": "/tutorial/tutorial-introduction",
        "/tutorial/using_containers": "/tutorial/using-containers",
        "/tutorial/using_pages": "/tutorial/using-pages",
        "/tutorial/using_scripts": "/tutorial/using-scripts",
        "/tutorial/using_timers_and_turn_scripts": "/tutorial/using-timers-and-turn-scripts",
        "/tutorial/verbs_in_depth": "/tutorial/verbs-in-depth",
        "/whats_new": "/whats-new",
    },
    integrations: [
        starlight({
            title: "Quest Viva",
            expressiveCode: {
                shiki: {
                    langs: [questGrammar],
                },
            },
            logo: {
                src: "./src/assets/quest-viva.svg",
            },
            social: [
                { icon: "github", label: "GitHub", href: "https://github.com/textadventures/quest" },
                { icon: "discord", label: "Discord", href: "https://textadventures.co.uk/community/discord" },
            ],
            editLink: {
                baseUrl: "https://github.com/textadventures/quest/edit/main/site/",
            },
            customCss: ["./src/styles/custom.css"],
            sidebar: [
                {
                    "label": "Start Here",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Introduction",
                            "slug": "intro"
                        },
                        {
                            "label": "What's new in Quest Viva 6.0",
                            "slug": "whats-new"
                        },
                        {
                            "label": "Download the app",
                            "slug": "download"
                        },
                        {
                            "label": "Tutorial",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Tutorial introduction",
                                    "slug": "tutorial/tutorial-introduction"
                                },
                                {
                                    "label": "Creating a simple game",
                                    "slug": "tutorial/creating-a-simple-game"
                                },
                                {
                                    "label": "Interacting with objects",
                                    "slug": "tutorial/interacting-with-objects"
                                },
                                {
                                    "label": "Anatomy of a Quest Viva game",
                                    "slug": "tutorial/anatomy-of-a-quest-viva-game"
                                },
                                {
                                    "label": "Using scripts",
                                    "slug": "tutorial/using-scripts"
                                },
                                {
                                    "label": "Custom attributes",
                                    "slug": "tutorial/custom-attributes"
                                },
                                {
                                    "label": "Custom commands",
                                    "slug": "tutorial/custom-commands"
                                },
                                {
                                    "label": "Verbs in depth",
                                    "slug": "tutorial/verbs-in-depth"
                                },
                                {
                                    "label": "More things to do with objects",
                                    "slug": "tutorial/more-things-to-do-with-objects"
                                },
                                {
                                    "label": "Using Pages",
                                    "slug": "tutorial/using-pages"
                                },
                                {
                                    "label": "Using containers",
                                    "slug": "tutorial/using-containers"
                                },
                                {
                                    "label": "Moving objects during the game",
                                    "slug": "tutorial/moving-objects-during-the-game"
                                },
                                {
                                    "label": "Status attributes",
                                    "slug": "tutorial/status-attributes"
                                },
                                {
                                    "label": "Using timers and turn scripts",
                                    "slug": "tutorial/using-timers-and-turn-scripts"
                                },
                                {
                                    "label": "Releasing your game",
                                    "slug": "tutorial/releasing-your-game"
                                },
                                {
                                    "label": "Creating a gamebook",
                                    "slug": "tutorial/creating-a-gamebook"
                                }
                            ]
                        },
                    ]
                },
                {
                    "label": "Guides",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "The Cloak of Darkness",
                            "slug": "cloak-of-darkness"
                        },
                        {
                            "label": "Commands & Parser",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "howto/commands/commands"
                                },
                                {
                                    "label": "Commands specific to a room",
                                    "slug": "howto/commands/commands-for-room"
                                },
                                {
                                    "label": "How to use verbs",
                                    "slug": "howto/commands/using-verbs"
                                },
                                {
                                    "label": "Complex commands",
                                    "slug": "howto/commands/complex-commands"
                                },
                                {
                                    "label": "Handling multiple items (and all)",
                                    "slug": "howto/commands/handling-multiple"
                                },
                                {
                                    "label": "Pattern matching with regular expressions",
                                    "slug": "howto/commands/pattern-matching"
                                },
                                {
                                    "label": "Advanced scope for items",
                                    "slug": "howto/commands/advanced-scope"
                                }
                            ]
                        },
                        {
                            "label": "World & Objects",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/world/text-processor"
                                },
                                {
                                    "label": "Changing templates",
                                    "slug": "howto/world/changing-templates"
                                },
                                {
                                    "label": "Exits",
                                    "slug": "howto/world/exits"
                                },
                                {
                                    "label": "Creating with Trizbort and Quest Viva",
                                    "slug": "howto/world/trizbort"
                                },
                                {
                                    "label": "Using containers",
                                    "slug": "howto/world/containers"
                                },
                                {
                                    "label": "Items that can be switched on and off",
                                    "slug": "howto/world/switchable"
                                },
                                {
                                    "label": "Handling light and dark",
                                    "slug": "howto/world/handling-light-and-dark"
                                },
                                {
                                    "label": "Wearable items",
                                    "slug": "howto/world/wearables"
                                },
                                {
                                    "label": "Score, health and money",
                                    "slug": "howto/world/score-health-money"
                                },
                                {
                                    "label": "Multi-state wearable items",
                                    "slug": "howto/world/multistate-clothing"
                                },
                                {
                                    "label": "Transcripts",
                                    "slug": "howto/world/transcript"
                                },
                                {
                                    "label": "When the player saves a game",
                                    "slug": "howto/world/about-save"
                                }
                            ]
                        },
                        {
                            "label": "Multimedia",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "howto/multimedia/multimedia"
                                },
                                {
                                    "label": "Images in Quest Viva",
                                    "slug": "howto/multimedia/images"
                                },
                                {
                                    "label": "Creating images on the fly",
                                    "slug": "howto/multimedia/images-on-the-fly"
                                },
                                {
                                    "label": "Adding sounds to your game",
                                    "slug": "howto/multimedia/adding-sounds"
                                },
                                {
                                    "label": "Adding videos",
                                    "slug": "howto/multimedia/adding-videos"
                                }
                            ]
                        },
                        {
                            "label": "Scripting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Introduction to coding",
                                    "slug": "howto/scripting/introtocoding"
                                },
                                {
                                    "label": "Creating functions",
                                    "slug": "howto/scripting/creating-functions-which-return-a-value"
                                },
                                {
                                    "label": "Using lists",
                                    "slug": "howto/scripting/using-lists"
                                },
                                {
                                    "label": "Using dictionaries",
                                    "slug": "howto/scripting/using-dictionaries"
                                },
                                {
                                    "label": "Using turnscripts",
                                    "slug": "howto/scripting/using-turnscripts"
                                },
                                {
                                    "label": "Scopes",
                                    "slug": "howto/scripting/scopes"
                                },
                                {
                                    "label": "Attack of the Clones!",
                                    "slug": "howto/scripting/clones"
                                },
                                {
                                    "label": "Advanced game scripts",
                                    "slug": "howto/scripting/advanced-game-scripts"
                                },
                                {
                                    "label": "Blocks and scripts",
                                    "slug": "howto/scripting/blocks-and-scripts"
                                },
                                {
                                    "label": "Expressions",
                                    "slug": "howto/scripting/expressions"
                                },
                                {
                                    "label": "Editing in full code view",
                                    "slug": "howto/scripting/codeview"
                                },
                                {
                                    "label": "Much Ado About Nothing",
                                    "slug": "howto/scripting/null"
                                },
                                {
                                    "label": "Using \"doubles\"",
                                    "slug": "howto/scripting/using-doubles"
                                },
                                {
                                    "label": "Unit testing",
                                    "slug": "howto/scripting/unit-testing"
                                },
                                {
                                    "label": "How to copy-and-paste code",
                                    "slug": "howto/scripting/copy-and-paste-code"
                                },
                                {
                                    "label": "Using walkthroughs",
                                    "slug": "howto/scripting/using-walkthroughs"
                                },
                                {
                                    "label": "Debugging your game",
                                    "slug": "howto/scripting/debugging-your-game"
                                }
                            ]
                        },
                        {
                            "label": "Task Recipes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "How to use functions",
                                    "slug": "howto/tasks/about-functions"
                                },
                                {
                                    "label": "Multiple choices - using a switch script",
                                    "slug": "howto/tasks/multiple-choices-using-a-switch-script"
                                },
                                {
                                    "label": "Changing the player object",
                                    "slug": "howto/tasks/changing-the-player-object"
                                },
                                {
                                    "label": "Handling water",
                                    "slug": "howto/tasks/handling-water"
                                },
                                {
                                    "label": "Showing a map",
                                    "slug": "howto/tasks/showing-a-map"
                                },
                                {
                                    "label": "Asking a simple question",
                                    "slug": "howto/tasks/ask-simple-question"
                                },
                                {
                                    "label": "Asking a question",
                                    "slug": "howto/tasks/asking-a-question"
                                },
                                {
                                    "label": "Keeping a journal",
                                    "slug": "howto/tasks/keeping-a-journal"
                                },
                                {
                                    "label": "How to keep score",
                                    "slug": "howto/tasks/keeping-score"
                                },
                                {
                                    "label": "How to build a transit system",
                                    "slug": "howto/tasks/transit-system"
                                },
                                {
                                    "label": "Converting one thing into another",
                                    "slug": "howto/tasks/convert"
                                },
                                {
                                    "label": "Tracking time",
                                    "slug": "howto/tasks/time"
                                },
                                {
                                    "label": "Setting up a shop",
                                    "slug": "howto/tasks/shop"
                                },
                                {
                                    "label": "Setting up a door",
                                    "slug": "howto/tasks/setting-up-door"
                                },
                                {
                                    "label": "Give the player character memory or wiki",
                                    "slug": "howto/tasks/memory-or-wiki"
                                },
                                {
                                    "label": "Move an object in a direction",
                                    "slug": "howto/tasks/move-object"
                                },
                                {
                                    "label": "Using neutral language",
                                    "slug": "howto/tasks/neutral-language"
                                },
                                {
                                    "label": "Randomisation",
                                    "slug": "howto/tasks/random"
                                },
                                {
                                    "label": "Showing a menu",
                                    "slug": "howto/tasks/showing-a-menu"
                                },
                                {
                                    "label": "Use maths functionality",
                                    "slug": "howto/tasks/use-maths-functionality"
                                },
                                {
                                    "label": "Resolving common problems",
                                    "slug": "howto/tasks/problems"
                                }
                            ]
                        },
                        {
                            "label": "NPCs & Dialogue",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Followers",
                                    "slug": "howto/npcs/follower"
                                },
                                {
                                    "label": "Introduction to conversations",
                                    "slug": "howto/npcs/conversations"
                                },
                                {
                                    "label": "Handling SPEAK TO",
                                    "slug": "howto/npcs/speak-to"
                                },
                                {
                                    "label": "Building an Ask/Tell system",
                                    "slug": "howto/npcs/ask-about"
                                },
                                {
                                    "label": "Building a conversation with Pages",
                                    "slug": "howto/npcs/dialogue-pages"
                                },
                                {
                                    "label": "Making NPCs patrol",
                                    "slug": "howto/npcs/patrolling-npcs"
                                },
                                {
                                    "label": "Making NPCs act independently",
                                    "slug": "howto/npcs/independent-npcs"
                                }
                            ]
                        },
                        {
                            "label": "UI & Presentation",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "The UI and game-play",
                                    "slug": "howto/ux/ui-game-play"
                                },
                                {
                                    "label": "The UI style",
                                    "slug": "howto/ux/ui-style"
                                },
                                {
                                    "label": "Using display verbs",
                                    "slug": "howto/ux/display-verbs"
                                },
                                {
                                    "label": "Custom command panes",
                                    "slug": "howto/ux/command-pane"
                                },
                                {
                                    "label": "Modifying the status and game panes",
                                    "slug": "howto/ux/ui-custom"
                                },
                                {
                                    "label": "Fonts",
                                    "slug": "howto/ux/ui-fonts"
                                },
                                {
                                    "label": "Messing with the location bar",
                                    "slug": "howto/ux/ui-location-bar"
                                },
                                {
                                    "label": "Custom status pane",
                                    "slug": "howto/ux/custom-panes"
                                },
                                {
                                    "label": "JavaScript to Quest Viva with ASLEvent",
                                    "slug": "howto/ux/ui-callback"
                                },
                                {
                                    "label": "Adding a dialogue panel",
                                    "slug": "howto/ux/ui-dialogue"
                                },
                                {
                                    "label": "Adding a dialogue panel that assigns points",
                                    "slug": "howto/ux/ui-dialogue-points"
                                },
                                {
                                    "label": "Customising the UI",
                                    "slug": "howto/ux/customising-the-ui"
                                }
                            ]
                        },
                        {
                            "label": "RPG Mechanics",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "howto/rpg/rpg-intro"
                                },
                                {
                                    "label": "Character creation",
                                    "slug": "howto/rpg/character-creation"
                                },
                                {
                                    "label": "Zombie Apocalypse (Part 1)",
                                    "slug": "howto/rpg/zombie-apocalypse-1"
                                },
                                {
                                    "label": "Zombie Apocalypse (Part 2)",
                                    "slug": "howto/rpg/zombie-apocalypse-2"
                                },
                                {
                                    "label": "Spells for the Zombie Apocalypse",
                                    "slug": "howto/rpg/zombie-apocalypse-spells"
                                }
                            ]
                        },
                        {
                            "label": "Community Recipes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "other-guides/community-guides"
                                },
                                {
                                    "label": "Time-limited puzzles",
                                    "slug": "other-guides/timelimitedpuzzles"
                                },
                                {
                                    "label": "Unlock with combination",
                                    "slug": "other-guides/unlockdoor"
                                },
                                {
                                    "label": "Starting inventory",
                                    "slug": "other-guides/starting-inventory"
                                },
                                {
                                    "label": "Immobilise the player",
                                    "slug": "other-guides/immobilise-the-player"
                                },
                                {
                                    "label": "Help with InvisiClues",
                                    "slug": "other-guides/invisiclues"
                                },
                                {
                                    "label": "Random default answers",
                                    "slug": "other-guides/random-default-answers"
                                },
                                {
                                    "label": "Port and starboard",
                                    "slug": "other-guides/port-and-starboard"
                                },
                                {
                                    "label": "A hint system",
                                    "slug": "other-guides/a-hint-system"
                                },
                                {
                                    "label": "Turn-based events",
                                    "slug": "other-guides/turn-based-events"
                                },
                                {
                                    "label": "Hyperlinks",
                                    "slug": "other-guides/hyperlinks"
                                },
                                {
                                    "label": "Implementing components of an object",
                                    "slug": "other-guides/implementing-components-of-an-object"
                                }
                            ]
                        }
                    ]
                },
                {
                    "label": "Advanced Topics",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overriding functions",
                            "slug": "advanced-topics/overriding"
                        },
                        {
                            "label": "Using inherited types",
                            "slug": "advanced-topics/using-inherited-types"
                        },
                        {
                            "label": "Types",
                            "slug": "advanced-topics/about-types"
                        },
                        {
                            "label": "Using delegates",
                            "slug": "advanced-topics/using-delegates"
                        },
                        {
                            "label": "Translating Quest Viva",
                            "slug": "advanced-topics/translation"
                        },
                        {
                            "label": "Using and creating libraries",
                            "slug": "advanced-topics/using-libraries"
                        },
                        {
                            "label": "Undo support",
                            "slug": "advanced-topics/undo-support"
                        },
                        {
                            "label": "Using tabs for types",
                            "slug": "advanced-topics/tabs-for-types"
                        },
                        {
                            "label": "Editor user interface elements",
                            "slug": "advanced-topics/editor-user-interface-elements"
                        }
                    ]
                },
                {
                    "label": "Language Reference",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Script commands",
                            "slug": "scripts"
                        },
                        {
                            "label": "Functions",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "functions"
                                },
                                {
                                    "label": "Functions for attributes",
                                    "slug": "functions/attributes"
                                },
                                {
                                    "label": "Functions for variables",
                                    "slug": "functions/variables"
                                },
                                {
                                    "label": "Functions for objects and exits",
                                    "slug": "functions/objects"
                                },
                                {
                                    "label": "Timers and turnscripts",
                                    "slug": "functions/timers-turnscripts"
                                },
                                {
                                    "label": "User interface functions",
                                    "slug": "functions/user-interface"
                                },
                                {
                                    "label": "List functions",
                                    "slug": "functions/list"
                                },
                                {
                                    "label": "Scope functions",
                                    "slug": "functions/scope"
                                },
                                {
                                    "label": "Dictionary functions",
                                    "slug": "functions/dictionary"
                                },
                                {
                                    "label": "String functions",
                                    "slug": "functions/string"
                                },
                                {
                                    "label": "Clothing functions",
                                    "slug": "functions/clothing"
                                },
                                {
                                    "label": "Randomising functions",
                                    "slug": "functions/random"
                                },
                                {
                                    "label": "General functions",
                                    "slug": "functions/general"
                                },
                                {
                                    "label": "Core.aslx functions",
                                    "slug": "functions/core"
                                },
                                {
                                    "label": "Internal Core.aslx functions",
                                    "slug": "functions/internal-core"
                                },
                                {
                                    "label": "Mathematical functions",
                                    "slug": "functions/maths"
                                }
                            ]
                        },
                        {
                            "label": "Attributes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "about-attributes"
                                },
                                {
                                    "label": "Important attributes",
                                    "slug": "important-attributes"
                                },
                                {
                                    "label": "Status attributes",
                                    "slug": "status-attributes"
                                },
                                {
                                    "label": "Change script",
                                    "slug": "change-scripts"
                                },
                                {
                                    "label": "Attribute Types",
                                    "slug": "types"
                                },
                                {
                                    "label": "Attribute reference",
                                    "slug": "attributes"
                                },
                                {
                                    "label": "Mutable attributes on inherited types",
                                    "slug": "notes"
                                }
                            ]
                        },
                        {
                            "label": "XML elements",
                            "slug": "elements"
                        },
                        {
                            "label": "JS functions",
                            "slug": "js"
                        },
                        {
                            "label": "Hard-coded functions and library functions",
                            "slug": "functions/hardcoded"
                        },
                        {
                            "label": "ASLX file format",
                            "slug": "aslx"
                        },
                        {
                            "label": "ASL requirements",
                            "slug": "asl-requirements"
                        }
                    ]
                },
                {
                    "label": "Publishing",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "publishing/publishing"
                        },
                        {
                            "label": "Hosting your game",
                            "slug": "publishing/hosting"
                        },
                        {
                            "label": "WebPlayer",
                            "slug": "publishing/webplayer"
                        },
                        {
                            "label": "Competition entry",
                            "slug": "publishing/competition-entry"
                        }
                    ]
                },
                {
                    "label": "Developers",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "developers/developers"
                        },
                        {
                            "label": "Building from source",
                            "slug": "developers/source-code"
                        },
                        {
                            "label": "Open source",
                            "slug": "developers/open-source"
                        },
                        {
                            "label": "Older versions",
                            "slug": "developers/older-versions"
                        }
                    ]
                }
            ],
        }),
    ],
});
