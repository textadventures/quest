// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// https://astro.build/config
export default defineConfig({
    site: "https://questviva.com",
    integrations: [
        starlight({
            title: "Quest Viva",
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
            sidebar: [
                {
                    "label": "Self-Hosting",
                    "collapsed": true,
                    "items": [
                        {
                            "autogenerate": {
                                "directory": "guides"
                            }
                        }
                    ]
                },
                {
                    "label": "Start Here",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "overview"
                        },
                        {
                            "label": "Tutorial",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Tutorial Introduction",
                                    "slug": "tutorial/tutorial_introduction"
                                },
                                {
                                    "label": "Creating a simple game",
                                    "slug": "tutorial/creating_a_simple_game"
                                },
                                {
                                    "label": "Interacting with objects",
                                    "slug": "tutorial/interacting_with_objects"
                                },
                                {
                                    "label": "Anatomy of a Quest game",
                                    "slug": "tutorial/anatomy_of_a_quest_game"
                                },
                                {
                                    "label": "Using scripts",
                                    "slug": "tutorial/using_scripts"
                                },
                                {
                                    "label": "Custom attributes",
                                    "slug": "tutorial/custom_attributes"
                                },
                                {
                                    "label": "Custom commands",
                                    "slug": "tutorial/custom_commands"
                                },
                                {
                                    "label": "More things to do with objects",
                                    "slug": "tutorial/more_things_to_do_with_objects"
                                },
                                {
                                    "label": "Using containers",
                                    "slug": "tutorial/using_containers"
                                },
                                {
                                    "label": "Moving objects during the game",
                                    "slug": "tutorial/moving_objects_during_the_game"
                                },
                                {
                                    "label": "Status Attributes",
                                    "slug": "tutorial/status_attributes"
                                },
                                {
                                    "label": "Using timers and turn scripts",
                                    "slug": "tutorial/using_timers_and_turn_scripts"
                                },
                                {
                                    "label": "Releasing your game",
                                    "slug": "tutorial/releasing_your_game"
                                },
                                {
                                    "label": "Creating a gamebook",
                                    "slug": "tutorial/creating_a_gamebook"
                                }
                            ]
                        }
                    ]
                },
                {
                    "label": "Guides",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Commands & Parser",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "howto/commands/commands"
                                },
                                {
                                    "label": "Commands Specific to a Room",
                                    "slug": "howto/commands/commands_for_room"
                                },
                                {
                                    "label": "How to use verbs",
                                    "slug": "howto/commands/using_verbs"
                                },
                                {
                                    "label": "Complex commands",
                                    "slug": "howto/commands/complex_commands"
                                },
                                {
                                    "label": "Handling Multiple Items (and All)",
                                    "slug": "howto/commands/handling_multiple"
                                },
                                {
                                    "label": "Pattern Matching with Regular Expressions",
                                    "slug": "howto/commands/pattern_matching"
                                },
                                {
                                    "label": "Advanced Scope For Items",
                                    "slug": "howto/commands/advanced_scope"
                                }
                            ]
                        },
                        {
                            "label": "World, Objects & Multimedia",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/world/text_processor"
                                },
                                {
                                    "label": "Exits",
                                    "slug": "howto/world/exits"
                                },
                                {
                                    "label": "Using Containers",
                                    "slug": "howto/world/containers"
                                },
                                {
                                    "label": "Items that can be switched on and off",
                                    "slug": "howto/world/switchable"
                                },
                                {
                                    "label": "Handling light and dark",
                                    "slug": "howto/world/handling_light_and_dark"
                                },
                                {
                                    "label": "Wearable items",
                                    "slug": "howto/world/wearables"
                                },
                                {
                                    "label": "Score, Health and Money",
                                    "slug": "howto/world/score_health_money"
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
                                    "label": "When the Player Saves a Game",
                                    "slug": "howto/world/about_save"
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
                                            "label": "Images in Quest",
                                            "slug": "howto/multimedia/images"
                                        },
                                        {
                                            "label": "Creating Images on the Fly",
                                            "slug": "howto/multimedia/images_on_the_fly"
                                        },
                                        {
                                            "label": "Adding Sounds to your Game",
                                            "slug": "howto/multimedia/adding_sounds"
                                        },
                                        {
                                            "label": "Adding Videos",
                                            "slug": "howto/multimedia/adding_videos"
                                        }
                                    ]
                                }
                            ]
                        },
                        {
                            "label": "Task Recipes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "How to use functions",
                                    "slug": "howto/tasks/about_functions"
                                },
                                {
                                    "label": "Multiple choices - using a switch script",
                                    "slug": "howto/tasks/multiple_choices_using_a_switch_script"
                                },
                                {
                                    "label": "Changing the player object",
                                    "slug": "howto/tasks/changing_the_player_object"
                                },
                                {
                                    "label": "Handling water",
                                    "slug": "howto/tasks/handling_water"
                                },
                                {
                                    "label": "Showing a map",
                                    "slug": "howto/tasks/showing_a_map"
                                },
                                {
                                    "label": "Asking a simple question",
                                    "slug": "howto/tasks/ask_simple_question"
                                },
                                {
                                    "label": "Asking a question",
                                    "slug": "howto/tasks/asking_a_question"
                                },
                                {
                                    "label": "Keeping a journal",
                                    "slug": "howto/tasks/keeping_a_journal"
                                },
                                {
                                    "label": "How to Keep Score",
                                    "slug": "howto/tasks/keeping_score"
                                },
                                {
                                    "label": "How to Build a Transit System",
                                    "slug": "howto/tasks/transit_system"
                                },
                                {
                                    "label": "Converting One Thing Into Another",
                                    "slug": "howto/tasks/convert"
                                },
                                {
                                    "label": "Tracking Time",
                                    "slug": "howto/tasks/time"
                                },
                                {
                                    "label": "Setting up a shop",
                                    "slug": "howto/tasks/shop"
                                },
                                {
                                    "label": "Setting Up a Door",
                                    "slug": "howto/tasks/setting_up_door"
                                },
                                {
                                    "label": "Give the player character memory or Wiki",
                                    "slug": "howto/tasks/memory_or_wiki"
                                },
                                {
                                    "label": "Move an object in a direction",
                                    "slug": "howto/tasks/move_object"
                                },
                                {
                                    "label": "Using neutral language",
                                    "slug": "howto/tasks/neutral_language"
                                },
                                {
                                    "label": "Randomisation",
                                    "slug": "howto/tasks/random"
                                },
                                {
                                    "label": "Showing a menu",
                                    "slug": "howto/tasks/showing_a_menu"
                                },
                                {
                                    "label": "Use maths functionality",
                                    "slug": "howto/tasks/use_maths_functionality"
                                },
                                {
                                    "label": "Resolving Common Problems",
                                    "slug": "howto/tasks/problems"
                                }
                            ]
                        },
                        {
                            "label": "UI & Presentation",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "The UI and Game-play",
                                    "slug": "howto/ux/ui-game-play"
                                },
                                {
                                    "label": "The UI Style",
                                    "slug": "howto/ux/ui-style"
                                },
                                {
                                    "label": "Using Display Verbs",
                                    "slug": "howto/ux/display_verbs"
                                },
                                {
                                    "label": "Custom Command Panes",
                                    "slug": "howto/ux/command_pane"
                                },
                                {
                                    "label": "Modifying the Status and Game Panes",
                                    "slug": "howto/ux/ui-custom"
                                },
                                {
                                    "label": "Fonts",
                                    "slug": "howto/ux/ui-fonts"
                                },
                                {
                                    "label": "Messing with the Location Bar",
                                    "slug": "howto/ux/ui-location-bar"
                                },
                                {
                                    "label": "Custom Status Pane",
                                    "slug": "howto/ux/custom_panes"
                                },
                                {
                                    "label": "JavaScript to Quest with ASLEvent",
                                    "slug": "howto/ux/ui-callback"
                                },
                                {
                                    "label": "Adding a Dialogue Panel",
                                    "slug": "howto/ux/ui-dialogue"
                                },
                                {
                                    "label": "Adding a Dialogue Panel That Assigns Points",
                                    "slug": "howto/ux/ui-dialogue-points"
                                },
                                {
                                    "label": "Customising the UI - Part 1",
                                    "slug": "howto/ux/ui-javascript"
                                },
                                {
                                    "label": "Customising the UI - Part 2",
                                    "slug": "howto/ux/ui-javascript2"
                                },
                                {
                                    "label": "Customising the UI - Part 3",
                                    "slug": "howto/ux/ui-javascript3"
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
                                    "label": "Introduction to Conversations",
                                    "slug": "howto/npcs/conversations"
                                },
                                {
                                    "label": "Handling SPEAK TO",
                                    "slug": "howto/npcs/speak_to"
                                },
                                {
                                    "label": "Building an Ask/Tell System",
                                    "slug": "howto/npcs/ask_about"
                                },
                                {
                                    "label": "Making NPCs Patrol",
                                    "slug": "howto/npcs/patrolling_npcs"
                                },
                                {
                                    "label": "Making NPCs Act Independently",
                                    "slug": "howto/npcs/independent_npcs"
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
                                    "label": "Character Creation",
                                    "slug": "howto/rpg/character_creation"
                                },
                                {
                                    "label": "Zombie Apocalypse (part 1)",
                                    "slug": "howto/rpg/zombie-apocalypse-1"
                                },
                                {
                                    "label": "Zombie Apocalypse (part 2)",
                                    "slug": "howto/rpg/zombie-apocalypse-2"
                                },
                                {
                                    "label": "Spells for the Zombie Apocalypse",
                                    "slug": "howto/rpg/zombie-apocalypse-spells"
                                }
                            ]
                        },
                        {
                            "label": "Scripting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "The Cloak of Darkness",
                                    "slug": "howto/scripting/cloak_of_darkness"
                                },
                                {
                                    "label": "Introduction to Coding",
                                    "slug": "howto/scripting/introtocoding"
                                },
                                {
                                    "label": "Creating functions",
                                    "slug": "howto/scripting/creating_functions_which_return_a_value"
                                },
                                {
                                    "label": "Using Lists",
                                    "slug": "howto/scripting/using_lists"
                                },
                                {
                                    "label": "Using Dictionaries",
                                    "slug": "howto/scripting/using_dictionaries"
                                },
                                {
                                    "label": "Using Turnscripts",
                                    "slug": "howto/scripting/using_turnscripts"
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
                                    "slug": "howto/scripting/advanced_game_scripts"
                                },
                                {
                                    "label": "Blocks and scripts",
                                    "slug": "howto/scripting/blocks_and_scripts"
                                },
                                {
                                    "label": "Expressions",
                                    "slug": "howto/scripting/expressions"
                                },
                                {
                                    "label": "Editing in Full Code View",
                                    "slug": "howto/scripting/codeview"
                                },
                                {
                                    "label": "Much Ado About Nothing",
                                    "slug": "howto/scripting/null"
                                },
                                {
                                    "label": "Using \"doubles\"",
                                    "slug": "howto/scripting/using_doubles"
                                },
                                {
                                    "label": "Unit Testing",
                                    "slug": "howto/scripting/unit_testing"
                                },
                                {
                                    "label": "Types",
                                    "slug": "howto/scripting/about_types"
                                },
                                {
                                    "label": "How to Copy-and-Paste Code",
                                    "slug": "howto/scripting/copy_and_paste_code"
                                },
                                {
                                    "label": "Using walkthroughs",
                                    "slug": "howto/scripting/using_walkthroughs"
                                },
                                {
                                    "label": "Debugging your game",
                                    "slug": "howto/scripting/debugging_your_game"
                                }
                            ]
                        },
                        {
                            "label": "Troubleshooting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "helpsheets"
                                },
                                {
                                    "label": "Introduction at Start of Game",
                                    "slug": "helpsheets/hs_introduction"
                                },
                                {
                                    "label": "Irritated by “a” in your objects?",
                                    "slug": "helpsheets/hs_removea"
                                },
                                {
                                    "label": "Object blocking an exit",
                                    "slug": "helpsheets/hs_blockingexit"
                                },
                                {
                                    "label": "Locked Exits and How to Open them?",
                                    "slug": "helpsheets/hs_lockedexits"
                                },
                                {
                                    "label": "Baddies behind a Door who want to kill you (but if you close the door you won’t die…!)",
                                    "slug": "helpsheets/hs_baddy2"
                                },
                                {
                                    "label": "Making Objects Appear when a Light is Switched On",
                                    "slug": "helpsheets/hs_objectsappear"
                                },
                                {
                                    "label": "Adding a Yes/No Quiz Question",
                                    "slug": "helpsheets/hs_addingquestion1"
                                },
                                {
                                    "label": "The CASE Command",
                                    "slug": "helpsheets/hs_case"
                                },
                                {
                                    "label": "Objects appearing (inside other objects)",
                                    "slug": "helpsheets/hs_appearingobjects"
                                },
                                {
                                    "label": "Ask and Tell",
                                    "slug": "helpsheets/hs_asktell"
                                },
                                {
                                    "label": "“Key” Inside another Object",
                                    "slug": "helpsheets/hs_keyinside"
                                },
                                {
                                    "label": "Adding Music to a Radio",
                                    "slug": "helpsheets/hs_radio"
                                },
                                {
                                    "label": "Creating a Countdown Timer",
                                    "slug": "helpsheets/hs_countdown"
                                }
                            ]
                        },
                        {
                            "label": "Community Recipes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "other_guides/community_guides"
                                },
                                {
                                    "label": "Time-limited puzzles",
                                    "slug": "other_guides/timelimitedpuzzles"
                                },
                                {
                                    "label": "Unlock with combination",
                                    "slug": "other_guides/unlockdoor"
                                },
                                {
                                    "label": "Starting inventory",
                                    "slug": "other_guides/starting_inventory"
                                },
                                {
                                    "label": "Immobilise the player",
                                    "slug": "other_guides/immobilise_the_player"
                                },
                                {
                                    "label": "Help with InvisiClues",
                                    "slug": "other_guides/invisiclues"
                                },
                                {
                                    "label": "Random default answers",
                                    "slug": "other_guides/random_default_answers"
                                },
                                {
                                    "label": "Port and starboard",
                                    "slug": "other_guides/port_and_starboard"
                                },
                                {
                                    "label": "A Hint System",
                                    "slug": "other_guides/a_hint_system"
                                },
                                {
                                    "label": "Turn-based events",
                                    "slug": "other_guides/turn_based_events"
                                },
                                {
                                    "label": "Hyperlinks",
                                    "slug": "other_guides/hyperlinks"
                                },
                                {
                                    "label": "Implementing components of an object",
                                    "slug": "other_guides/implementing_components_of_an_object"
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
                            "slug": "advanced-topics/using_inherited_types"
                        },
                        {
                            "label": "Changing templates",
                            "slug": "advanced-topics/changing_templates"
                        },
                        {
                            "label": "Translating Quest",
                            "slug": "advanced-topics/translating_quest"
                        },
                        {
                            "label": "Using and creating libraries",
                            "slug": "advanced-topics/using_libraries"
                        },
                        {
                            "label": "Using Templates",
                            "slug": "advanced-topics/using_templates"
                        },
                        {
                            "label": "Undo support",
                            "slug": "advanced-topics/undo_support"
                        },
                        {
                            "label": "Using Tabs for Types",
                            "slug": "advanced-topics/tabs_for_types"
                        },
                        {
                            "label": "Editor User Interface Elements",
                            "slug": "advanced-topics/editor_user_interface_elements"
                        }
                    ]
                },
                {
                    "label": "Language Reference",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Script commands",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "scripts"
                                },
                                {
                                    "label": "ask",
                                    "slug": "scripts/ask"
                                },
                                {
                                    "label": "create",
                                    "slug": "scripts/create"
                                },
                                {
                                    "label": "create exit",
                                    "slug": "scripts/create_exit"
                                },
                                {
                                    "label": "create timer",
                                    "slug": "scripts/create_timer"
                                },
                                {
                                    "label": "create turnscript",
                                    "slug": "scripts/create_turnscript"
                                },
                                {
                                    "label": "destroy",
                                    "slug": "scripts/destroy"
                                },
                                {
                                    "label": "dictionary add",
                                    "slug": "scripts/dictionary_add"
                                },
                                {
                                    "label": "dictionary remove",
                                    "slug": "scripts/dictionary_remove"
                                },
                                {
                                    "label": "do",
                                    "slug": "scripts/do"
                                },
                                {
                                    "label": "error",
                                    "slug": "scripts/error"
                                },
                                {
                                    "label": "finish",
                                    "slug": "scripts/finish"
                                },
                                {
                                    "label": "firsttime",
                                    "slug": "scripts/firsttime"
                                },
                                {
                                    "label": "for",
                                    "slug": "scripts/for"
                                },
                                {
                                    "label": "foreach",
                                    "slug": "scripts/foreach"
                                },
                                {
                                    "label": "get input",
                                    "slug": "scripts/get_input"
                                },
                                {
                                    "label": "if",
                                    "slug": "scripts/if"
                                },
                                {
                                    "label": "insert",
                                    "slug": "scripts/insert"
                                },
                                {
                                    "label": "invoke",
                                    "slug": "scripts/invoke"
                                },
                                {
                                    "label": "list add",
                                    "slug": "scripts/list_add"
                                },
                                {
                                    "label": "list remove",
                                    "slug": "scripts/list_remove"
                                },
                                {
                                    "label": "msg",
                                    "slug": "scripts/msg"
                                },
                                {
                                    "label": "on ready",
                                    "slug": "scripts/on_ready"
                                },
                                {
                                    "label": "picture",
                                    "slug": "scripts/picture"
                                },
                                {
                                    "label": "play sound",
                                    "slug": "scripts/play_sound"
                                },
                                {
                                    "label": "request",
                                    "slug": "scripts/request"
                                },
                                {
                                    "label": "return",
                                    "slug": "scripts/return"
                                },
                                {
                                    "label": "rundelegate",
                                    "slug": "scripts/rundelegate"
                                },
                                {
                                    "label": "set",
                                    "slug": "scripts/set"
                                },
                                {
                                    "label": "Setting variables",
                                    "slug": "scripts/setting_variables"
                                },
                                {
                                    "label": "show menu",
                                    "slug": "scripts/show_menu"
                                },
                                {
                                    "label": "start transaction",
                                    "slug": "scripts/start_transaction"
                                },
                                {
                                    "label": "stop sound",
                                    "slug": "scripts/stop_sound"
                                },
                                {
                                    "label": "switch",
                                    "slug": "scripts/switch"
                                },
                                {
                                    "label": "undo",
                                    "slug": "scripts/undo"
                                },
                                {
                                    "label": "wait",
                                    "slug": "scripts/wait"
                                },
                                {
                                    "label": "while",
                                    "slug": "scripts/while"
                                }
                            ]
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
                                    "label": "Functions for Attributes",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-attributes"
                                        },
                                        {
                                            "label": "DecreaseHealth",
                                            "slug": "functions/corelibrary/decreasehealth"
                                        },
                                        {
                                            "label": "DecreaseMoney",
                                            "slug": "functions/corelibrary/decreasemoney"
                                        },
                                        {
                                            "label": "DecreaseScore",
                                            "slug": "functions/corelibrary/decreasescore"
                                        },
                                        {
                                            "label": "GetAttribute",
                                            "slug": "functions/getattribute"
                                        },
                                        {
                                            "label": "GetAttributeNames",
                                            "slug": "functions/getattributenames"
                                        },
                                        {
                                            "label": "GetBoolean",
                                            "slug": "functions/getboolean"
                                        },
                                        {
                                            "label": "GetDouble",
                                            "slug": "functions/getdouble"
                                        },
                                        {
                                            "label": "GetInt",
                                            "slug": "functions/getint"
                                        },
                                        {
                                            "label": "GetString",
                                            "slug": "functions/getstring"
                                        },
                                        {
                                            "label": "HasAttribute",
                                            "slug": "functions/hasattribute"
                                        },
                                        {
                                            "label": "HasBoolean",
                                            "slug": "functions/hasboolean"
                                        },
                                        {
                                            "label": "HasDelegateImplementation",
                                            "slug": "functions/hasdelegateimplementation"
                                        },
                                        {
                                            "label": "HasDouble",
                                            "slug": "functions/hasdouble"
                                        },
                                        {
                                            "label": "HasInt",
                                            "slug": "functions/hasint"
                                        },
                                        {
                                            "label": "HasObject",
                                            "slug": "functions/hasobject"
                                        },
                                        {
                                            "label": "HasScript",
                                            "slug": "functions/hasscript"
                                        },
                                        {
                                            "label": "HasString",
                                            "slug": "functions/hasstring"
                                        },
                                        {
                                            "label": "IncreaseHealth",
                                            "slug": "functions/corelibrary/increasehealth"
                                        },
                                        {
                                            "label": "IncreaseMoney",
                                            "slug": "functions/corelibrary/increasemoney"
                                        },
                                        {
                                            "label": "IncreaseScore",
                                            "slug": "functions/corelibrary/increasescore"
                                        },
                                        {
                                            "label": "SetObjectFlagOff",
                                            "slug": "functions/corelibrary/setobjectflagoff"
                                        },
                                        {
                                            "label": "SetObjectFlagOn",
                                            "slug": "functions/corelibrary/setobjectflagon"
                                        }
                                    ]
                                },
                                {
                                    "label": "Functions for Variables",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-variables"
                                        },
                                        {
                                            "label": "Equal",
                                            "slug": "functions/equal"
                                        },
                                        {
                                            "label": "IsDefined",
                                            "slug": "functions/isdefined"
                                        },
                                        {
                                            "label": "IsDouble",
                                            "slug": "functions/isdouble"
                                        },
                                        {
                                            "label": "IsInt",
                                            "slug": "functions/isint"
                                        },
                                        {
                                            "label": "ToDouble",
                                            "slug": "functions/todouble"
                                        },
                                        {
                                            "label": "ToInt",
                                            "slug": "functions/toint"
                                        },
                                        {
                                            "label": "ToString",
                                            "slug": "functions/tostring"
                                        },
                                        {
                                            "label": "TypeOf",
                                            "slug": "functions/typeof"
                                        }
                                    ]
                                },
                                {
                                    "label": "Functions for Objects and Exits",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-objects"
                                        },
                                        {
                                            "label": "Clone",
                                            "slug": "functions/clone"
                                        },
                                        {
                                            "label": "CloneObject",
                                            "slug": "functions/corelibrary/cloneobject"
                                        },
                                        {
                                            "label": "CloneObjectAndMove",
                                            "slug": "functions/corelibrary/cloneobjectandmove"
                                        },
                                        {
                                            "label": "CloneObjectAndMoveHere",
                                            "slug": "functions/corelibrary/cloneobjectandmovehere"
                                        },
                                        {
                                            "label": "CreateBiExits",
                                            "slug": "functions/createbiexits"
                                        },
                                        {
                                            "label": "DoesInherit",
                                            "slug": "functions/doesinherit"
                                        },
                                        {
                                            "label": "GetExitByLink",
                                            "slug": "functions/getexitbylink"
                                        },
                                        {
                                            "label": "GetExitByName",
                                            "slug": "functions/getexitbyname"
                                        },
                                        {
                                            "label": "GetObject",
                                            "slug": "functions/getobject"
                                        },
                                        {
                                            "label": "LockExit",
                                            "slug": "functions/corelibrary/lockexit"
                                        },
                                        {
                                            "label": "MakeExitInvisible",
                                            "slug": "functions/corelibrary/makeexitinvisible"
                                        },
                                        {
                                            "label": "MakeExitVisible",
                                            "slug": "functions/corelibrary/makeexitvisible"
                                        },
                                        {
                                            "label": "MakeObjectInvisible",
                                            "slug": "functions/corelibrary/makeobjectinvisible"
                                        },
                                        {
                                            "label": "MakeObjectVisible",
                                            "slug": "functions/corelibrary/makeobjectvisible"
                                        },
                                        {
                                            "label": "MoveObject",
                                            "slug": "functions/corelibrary/moveobject"
                                        },
                                        {
                                            "label": "RemoveObject",
                                            "slug": "functions/corelibrary/removeobject"
                                        },
                                        {
                                            "label": "UnlockExit",
                                            "slug": "functions/corelibrary/unlockexit"
                                        }
                                    ]
                                },
                                {
                                    "label": "Timers and Turnscripts",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-tandt"
                                        },
                                        {
                                            "label": "DisableTimer",
                                            "slug": "functions/corelibrary/disabletimer"
                                        },
                                        {
                                            "label": "DisableTurnScript",
                                            "slug": "functions/corelibrary/disableturnscript"
                                        },
                                        {
                                            "label": "EnableTimer",
                                            "slug": "functions/corelibrary/enabletimer"
                                        },
                                        {
                                            "label": "EnableTurnScript",
                                            "slug": "functions/corelibrary/enableturnscript"
                                        },
                                        {
                                            "label": "GetTimer",
                                            "slug": "functions/gettimer"
                                        },
                                        {
                                            "label": "Pause",
                                            "slug": "functions/corelibrary/pause"
                                        },
                                        {
                                            "label": "SetTimeout",
                                            "slug": "functions/corelibrary/settimeout"
                                        },
                                        {
                                            "label": "SetTimeoutID",
                                            "slug": "functions/corelibrary/settimeoutid"
                                        },
                                        {
                                            "label": "SetTimerInterval",
                                            "slug": "functions/corelibrary/settimerinterval"
                                        },
                                        {
                                            "label": "SetTimerScript",
                                            "slug": "functions/corelibrary/settimerscript"
                                        },
                                        {
                                            "label": "SetTurnScript",
                                            "slug": "functions/corelibrary/setturnscript"
                                        },
                                        {
                                            "label": "SetTurnTimeout",
                                            "slug": "functions/corelibrary/setturntimeout"
                                        },
                                        {
                                            "label": "SetTurnTimeoutID",
                                            "slug": "functions/corelibrary/setturntimeoutid"
                                        },
                                        {
                                            "label": "SuppressTurnscripts",
                                            "slug": "functions/suppressturnscripts"
                                        }
                                    ]
                                },
                                {
                                    "label": "User Interface Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-ui"
                                        },
                                        {
                                            "label": "Ask",
                                            "slug": "functions/ask"
                                        },
                                        {
                                            "label": "ClearFramePicture",
                                            "slug": "functions/corelibrary/clearframepicture"
                                        },
                                        {
                                            "label": "ClearScreen",
                                            "slug": "functions/corelibrary/clearscreen"
                                        },
                                        {
                                            "label": "DisplayList",
                                            "slug": "functions/corelibrary/displaylist"
                                        },
                                        {
                                            "label": "DisplayMailtoLink",
                                            "slug": "functions/corelibrary/displaymailtolink"
                                        },
                                        {
                                            "label": "GetCurrentFontFamily",
                                            "slug": "functions/corelibrary/getcurrentfontfamily"
                                        },
                                        {
                                            "label": "GetInput",
                                            "slug": "functions/getinput"
                                        },
                                        {
                                            "label": "InitUserInterface",
                                            "slug": "functions/corelibrary/inituserinterface"
                                        },
                                        {
                                            "label": "OutputText",
                                            "slug": "functions/outputtext"
                                        },
                                        {
                                            "label": "OutputTextNoBr",
                                            "slug": "functions/outputtextnobr"
                                        },
                                        {
                                            "label": "OutputTextRaw",
                                            "slug": "functions/outputtextraw"
                                        },
                                        {
                                            "label": "OutputTextRawNoBr",
                                            "slug": "functions/outputtextrawnobr"
                                        },
                                        {
                                            "label": "PrintCentered",
                                            "slug": "functions/corelibrary/printcentered"
                                        },
                                        {
                                            "label": "SetAlignment",
                                            "slug": "functions/corelibrary/setalignment"
                                        },
                                        {
                                            "label": "SetBackgroundColour",
                                            "slug": "functions/corelibrary/setbackgroundcolour"
                                        },
                                        {
                                            "label": "SetBackgroundImage",
                                            "slug": "functions/corelibrary/setbackgroundimage"
                                        },
                                        {
                                            "label": "SetBackgroundOpacity",
                                            "slug": "functions/corelibrary/setbackgroundopacity"
                                        },
                                        {
                                            "label": "SetFontName",
                                            "slug": "functions/corelibrary/setfontname"
                                        },
                                        {
                                            "label": "SetFontSize",
                                            "slug": "functions/corelibrary/setfontsize"
                                        },
                                        {
                                            "label": "SetForegroundColour",
                                            "slug": "functions/corelibrary/setforegroundcolour"
                                        },
                                        {
                                            "label": "SetFramePicture",
                                            "slug": "functions/corelibrary/setframepicture"
                                        },
                                        {
                                            "label": "SetWebFontName",
                                            "slug": "functions/corelibrary/setwebfontname"
                                        },
                                        {
                                            "label": "ShowMenu",
                                            "slug": "functions/showmenu"
                                        },
                                        {
                                            "label": "ShowVimeo",
                                            "slug": "functions/corelibrary/showvimeo"
                                        },
                                        {
                                            "label": "ShowYouTube",
                                            "slug": "functions/corelibrary/showyoutube"
                                        },
                                        {
                                            "label": "TextFX_Typewriter",
                                            "slug": "functions/corelibrary/textfx_typewriter"
                                        },
                                        {
                                            "label": "TextFX_Unscramble",
                                            "slug": "functions/corelibrary/textfx_unscramble"
                                        },
                                        {
                                            "label": "UpdateStatusAttributes",
                                            "slug": "functions/corelibrary/updatestatusattributes"
                                        },
                                        {
                                            "label": "WaitForKeyPress",
                                            "slug": "functions/corelibrary/waitforkeypress"
                                        }
                                    ]
                                },
                                {
                                    "label": "List Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-list"
                                        },
                                        {
                                            "label": "Contains",
                                            "slug": "functions/contains"
                                        },
                                        {
                                            "label": "FilterByAttribute",
                                            "slug": "functions/filterbyattribute"
                                        },
                                        {
                                            "label": "FilterByNotAttribute",
                                            "slug": "functions/filterbynotattribute"
                                        },
                                        {
                                            "label": "FilterByType",
                                            "slug": "functions/filterbytype"
                                        },
                                        {
                                            "label": "IndexOf",
                                            "slug": "functions/indexof"
                                        },
                                        {
                                            "label": "ListCombine",
                                            "slug": "functions/listcombine"
                                        },
                                        {
                                            "label": "ListCompact",
                                            "slug": "functions/listcompact"
                                        },
                                        {
                                            "label": "ListContains",
                                            "slug": "functions/listcontains"
                                        },
                                        {
                                            "label": "ListCount",
                                            "slug": "functions/listcount"
                                        },
                                        {
                                            "label": "ListExclude",
                                            "slug": "functions/listexclude"
                                        },
                                        {
                                            "label": "ListItem",
                                            "slug": "functions/listitem"
                                        },
                                        {
                                            "label": "NewList",
                                            "slug": "functions/newlist"
                                        },
                                        {
                                            "label": "NewObjectList",
                                            "slug": "functions/newobjectlist"
                                        },
                                        {
                                            "label": "NewStringList",
                                            "slug": "functions/newstringlist"
                                        },
                                        {
                                            "label": "ObjectListCompact",
                                            "slug": "functions/objectlistcompact"
                                        },
                                        {
                                            "label": "ObjectListItem",
                                            "slug": "functions/objectlistitem"
                                        },
                                        {
                                            "label": "ObjectListSort",
                                            "slug": "functions/objectlistsort"
                                        },
                                        {
                                            "label": "ObjectListSortDescending",
                                            "slug": "functions/objectlistsortdescending"
                                        },
                                        {
                                            "label": "ObjectListToStringList",
                                            "slug": "functions/objectlisttostringlist"
                                        },
                                        {
                                            "label": "RemoveSceneryObjects",
                                            "slug": "functions/corelibrary/removesceneryobjects"
                                        },
                                        {
                                            "label": "StringListItem",
                                            "slug": "functions/stringlistitem"
                                        },
                                        {
                                            "label": "StringListSort",
                                            "slug": "functions/stringlistsort"
                                        },
                                        {
                                            "label": "StringListSortDescending",
                                            "slug": "functions/stringlistsortdescending"
                                        }
                                    ]
                                },
                                {
                                    "label": "Scope Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-scope"
                                        },
                                        {
                                            "label": "AllCommands",
                                            "slug": "functions/allcommands"
                                        },
                                        {
                                            "label": "AllExits",
                                            "slug": "functions/allexits"
                                        },
                                        {
                                            "label": "AllObjects",
                                            "slug": "functions/allobjects"
                                        },
                                        {
                                            "label": "AllRooms",
                                            "slug": "functions/allrooms"
                                        },
                                        {
                                            "label": "AllTurnScripts",
                                            "slug": "functions/allturnscripts"
                                        },
                                        {
                                            "label": "GetAllChildObjects",
                                            "slug": "functions/getallchildobjects"
                                        },
                                        {
                                            "label": "GetDirectChildren",
                                            "slug": "functions/getdirectchildren"
                                        },
                                        {
                                            "label": "ListVisible",
                                            "slug": "functions/listvisible"
                                        },
                                        {
                                            "label": "ListVisibleFor",
                                            "slug": "functions/listvisiblefor"
                                        },
                                        {
                                            "label": "ScopeAllExitsForRoom",
                                            "slug": "functions/corelibrary/scopeallexitsforroom"
                                        },
                                        {
                                            "label": "ScopeCommands",
                                            "slug": "functions/corelibrary/scopecommands"
                                        },
                                        {
                                            "label": "ScopeExits",
                                            "slug": "functions/corelibrary/scopeexits"
                                        },
                                        {
                                            "label": "ScopeExitsAll",
                                            "slug": "functions/corelibrary/scopeexitsall"
                                        },
                                        {
                                            "label": "ScopeExitsForRoom",
                                            "slug": "functions/corelibrary/scopeexitsforroom"
                                        },
                                        {
                                            "label": "ScopeInventory",
                                            "slug": "functions/corelibrary/scopeinventory"
                                        },
                                        {
                                            "label": "ScopeInventoryNotScenery",
                                            "slug": "functions/scopeinventorynotscenery"
                                        },
                                        {
                                            "label": "ScopeReachable",
                                            "slug": "functions/corelibrary/scopereachable"
                                        },
                                        {
                                            "label": "ScopeReachableForRoom",
                                            "slug": "functions/corelibrary/scopereachableforroom"
                                        },
                                        {
                                            "label": "ScopeReachableInventory",
                                            "slug": "functions/corelibrary/scopereachableinventory"
                                        },
                                        {
                                            "label": "ScopeReachableNotHeld",
                                            "slug": "functions/corelibrary/scopereachablenotheld"
                                        },
                                        {
                                            "label": "ScopeReachableNotHeldForRoom",
                                            "slug": "functions/corelibrary/scopereachablenotheldforroom"
                                        },
                                        {
                                            "label": "ScopeUnlockedExitsForRoom",
                                            "slug": "functions/corelibrary/scopeunlockedexitsforroom"
                                        },
                                        {
                                            "label": "ScopeUnlockedExitsForRoom",
                                            "slug": "functions/scopeunlockedexitsforroom"
                                        },
                                        {
                                            "label": "ScopeVisible",
                                            "slug": "functions/corelibrary/scopevisible"
                                        },
                                        {
                                            "label": "ScopeVisibleForRoom",
                                            "slug": "functions/corelibrary/scopevisibleforroom"
                                        },
                                        {
                                            "label": "ScopeVisibleNotHeld",
                                            "slug": "functions/corelibrary/scopevisiblenotheld"
                                        },
                                        {
                                            "label": "ScopeVisibleNotHeldForRoom",
                                            "slug": "functions/corelibrary/scopevisiblenotheldforroom"
                                        },
                                        {
                                            "label": "ScopeVisibleNotHeldNotScenery",
                                            "slug": "functions/corelibrary/scopevisiblenotheldnotscenery"
                                        },
                                        {
                                            "label": "ScopeVisibleNotHeldNotSceneryForRoom",
                                            "slug": "functions/corelibrary/scopevisiblenotheldnotsceneryforroom"
                                        },
                                        {
                                            "label": "ScopeVisibleNotReachable",
                                            "slug": "functions/corelibrary/scopevisiblenotreachable"
                                        },
                                        {
                                            "label": "ScopeVisibleNotReachableForRoom",
                                            "slug": "functions/corelibrary/scopevisiblenotreachableforroom"
                                        }
                                    ]
                                },
                                {
                                    "label": "Dictionary Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-dictionary"
                                        },
                                        {
                                            "label": "DictionaryAdd",
                                            "slug": "functions/dictionaryadd"
                                        },
                                        {
                                            "label": "DictionaryContains",
                                            "slug": "functions/dictionarycontains"
                                        },
                                        {
                                            "label": "DictionaryCount",
                                            "slug": "functions/dictionarycount"
                                        },
                                        {
                                            "label": "DictionaryItem",
                                            "slug": "functions/dictionaryitem"
                                        },
                                        {
                                            "label": "DictionaryRemove",
                                            "slug": "functions/dictionaryremove"
                                        },
                                        {
                                            "label": "NewDictionary",
                                            "slug": "functions/newdictionary"
                                        },
                                        {
                                            "label": "NewObjectDictionary",
                                            "slug": "functions/newobjectdictionary"
                                        },
                                        {
                                            "label": "NewScriptDictionary",
                                            "slug": "functions/newscriptdictionary"
                                        },
                                        {
                                            "label": "NewStringDictionary",
                                            "slug": "functions/newstringdictionary"
                                        },
                                        {
                                            "label": "ObjectDictionaryItem",
                                            "slug": "functions/objectdictionaryitem"
                                        },
                                        {
                                            "label": "QuickParams",
                                            "slug": "functions/quickparams"
                                        },
                                        {
                                            "label": "ScriptDictionaryItem",
                                            "slug": "functions/scriptdictionaryitem"
                                        },
                                        {
                                            "label": "StringDictionaryItem",
                                            "slug": "functions/stringdictionaryitem"
                                        }
                                    ]
                                },
                                {
                                    "label": "String Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-string"
                                        },
                                        {
                                            "label": "Asc",
                                            "slug": "functions/string/asc"
                                        },
                                        {
                                            "label": "CapFirst",
                                            "slug": "functions/string/capfirst"
                                        },
                                        {
                                            "label": "Chr",
                                            "slug": "functions/string/chr"
                                        },
                                        {
                                            "label": "Conjugate",
                                            "slug": "functions/corelibrary/conjugate"
                                        },
                                        {
                                            "label": "Decimalise",
                                            "slug": "functions/string/decimalise"
                                        },
                                        {
                                            "label": "DisplayMoney",
                                            "slug": "functions/string/displaymoney"
                                        },
                                        {
                                            "label": "DisplayNumber",
                                            "slug": "functions/string/displaynumber"
                                        },
                                        {
                                            "label": "DynamicTemplate",
                                            "slug": "functions/dynamictemplate"
                                        },
                                        {
                                            "label": "EndsWith",
                                            "slug": "functions/string/endswith"
                                        },
                                        {
                                            "label": "FormatList",
                                            "slug": "functions/string/formatlist"
                                        },
                                        {
                                            "label": "GetMatchStrength",
                                            "slug": "functions/getmatchstrength"
                                        },
                                        {
                                            "label": "Instr",
                                            "slug": "functions/string/instr"
                                        },
                                        {
                                            "label": "InstrRev",
                                            "slug": "functions/string/instrrev"
                                        },
                                        {
                                            "label": "IsNumeric",
                                            "slug": "functions/string/isnumeric"
                                        },
                                        {
                                            "label": "IsRegexMatch",
                                            "slug": "functions/isregexmatch"
                                        },
                                        {
                                            "label": "Join",
                                            "slug": "functions/string/join"
                                        },
                                        {
                                            "label": "LCase",
                                            "slug": "functions/string/lcase"
                                        },
                                        {
                                            "label": "Left",
                                            "slug": "functions/string/left"
                                        },
                                        {
                                            "label": "LengthOf",
                                            "slug": "functions/string/lengthof"
                                        },
                                        {
                                            "label": "LTrim",
                                            "slug": "functions/string/ltrim"
                                        },
                                        {
                                            "label": "Mid",
                                            "slug": "functions/string/mid"
                                        },
                                        {
                                            "label": "PadString",
                                            "slug": "functions/string/padstring"
                                        },
                                        {
                                            "label": "ProcessText",
                                            "slug": "functions/processtext"
                                        },
                                        {
                                            "label": "Replace",
                                            "slug": "functions/string/replace"
                                        },
                                        {
                                            "label": "ReverseDirection",
                                            "slug": "functions/string/reversedirection"
                                        },
                                        {
                                            "label": "Right",
                                            "slug": "functions/string/right"
                                        },
                                        {
                                            "label": "RTrim",
                                            "slug": "functions/string/rtrim"
                                        },
                                        {
                                            "label": "SafeXML",
                                            "slug": "functions/safexml"
                                        },
                                        {
                                            "label": "Spaces",
                                            "slug": "functions/string/spaces"
                                        },
                                        {
                                            "label": "Split",
                                            "slug": "functions/string/split"
                                        },
                                        {
                                            "label": "StartsWith",
                                            "slug": "functions/string/startswith"
                                        },
                                        {
                                            "label": "Template",
                                            "slug": "functions/template"
                                        },
                                        {
                                            "label": "ToRoman",
                                            "slug": "functions/string/toroman"
                                        },
                                        {
                                            "label": "ToWords",
                                            "slug": "functions/string/towords"
                                        },
                                        {
                                            "label": "Trim",
                                            "slug": "functions/string/trim"
                                        },
                                        {
                                            "label": "UCase",
                                            "slug": "functions/string/ucase"
                                        },
                                        {
                                            "label": "WriteVerb",
                                            "slug": "functions/corelibrary/writeverb"
                                        }
                                    ]
                                },
                                {
                                    "label": "Clothing Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-clothing"
                                        },
                                        {
                                            "label": "GetArmour",
                                            "slug": "functions/getarmour"
                                        },
                                        {
                                            "label": "GetArmourFor",
                                            "slug": "functions/getarmourfor"
                                        },
                                        {
                                            "label": "GetOuterFor",
                                            "slug": "functions/getouterfor"
                                        },
                                        {
                                            "label": "ListWorn",
                                            "slug": "functions/getouter"
                                        },
                                        {
                                            "label": "ListWorn",
                                            "slug": "functions/listworn"
                                        },
                                        {
                                            "label": "ListWornFor",
                                            "slug": "functions/listwornfor"
                                        },
                                        {
                                            "label": "RemoveGarment",
                                            "slug": "functions/removegarment"
                                        },
                                        {
                                            "label": "WearGarment",
                                            "slug": "functions/weargarment"
                                        }
                                    ]
                                },
                                {
                                    "label": "Randomising Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-random"
                                        },
                                        {
                                            "label": "DiceRoll",
                                            "slug": "functions/corelibrary/diceroll"
                                        },
                                        {
                                            "label": "GetRandomDouble",
                                            "slug": "functions/getrandomdouble"
                                        },
                                        {
                                            "label": "GetRandomInt",
                                            "slug": "functions/getrandomint"
                                        },
                                        {
                                            "label": "PickOneChild",
                                            "slug": "functions/pickonechild"
                                        },
                                        {
                                            "label": "PickOneChildOfType",
                                            "slug": "functions/pickonechildoftype"
                                        },
                                        {
                                            "label": "PickOneExit",
                                            "slug": "functions/pickoneexit"
                                        },
                                        {
                                            "label": "PickOneObject",
                                            "slug": "functions/pickoneobject"
                                        },
                                        {
                                            "label": "PickOneString",
                                            "slug": "functions/pickonestring"
                                        },
                                        {
                                            "label": "PickOneUnlockedExit",
                                            "slug": "functions/pickoneunlockedexit"
                                        },
                                        {
                                            "label": "RandomChance",
                                            "slug": "functions/corelibrary/randomchance"
                                        }
                                    ]
                                },
                                {
                                    "label": "General Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-general"
                                        },
                                        {
                                            "label": "CurrentDateUTC",
                                            "slug": "functions/currentdateutc"
                                        },
                                        {
                                            "label": "Eval",
                                            "slug": "functions/eval"
                                        },
                                        {
                                            "label": "GetFileData",
                                            "slug": "functions/getfiledata"
                                        },
                                        {
                                            "label": "GetFileURL",
                                            "slug": "functions/getfileurl"
                                        },
                                        {
                                            "label": "Log",
                                            "slug": "functions/corelibrary/log"
                                        },
                                        {
                                            "label": "RunDelegateFunction",
                                            "slug": "functions/rundelegatefunction"
                                        }
                                    ]
                                },
                                {
                                    "label": "Core.aslx Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-core"
                                        },
                                        {
                                            "label": "AddToInventory",
                                            "slug": "functions/corelibrary/addtoinventory"
                                        },
                                        {
                                            "label": "CanReachThrough",
                                            "slug": "functions/corelibrary/canreachthrough"
                                        },
                                        {
                                            "label": "CanSeeThrough",
                                            "slug": "functions/corelibrary/canseethrough"
                                        },
                                        {
                                            "label": "ChangePOV",
                                            "slug": "functions/corelibrary/changepov"
                                        },
                                        {
                                            "label": "CheckDarkness",
                                            "slug": "functions/corelibrary/checkdarkness"
                                        },
                                        {
                                            "label": "FormatExitList",
                                            "slug": "functions/corelibrary/formatexitlist"
                                        },
                                        {
                                            "label": "FormatObjectList",
                                            "slug": "functions/corelibrary/formatobjectlist"
                                        },
                                        {
                                            "label": "GetBlockingObject",
                                            "slug": "functions/corelibrary/getblockingobject"
                                        },
                                        {
                                            "label": "GetDefiniteName",
                                            "slug": "functions/getdefinitename"
                                        },
                                        {
                                            "label": "GetDisplayAlias",
                                            "slug": "functions/corelibrary/getdisplayalias"
                                        },
                                        {
                                            "label": "GetDisplayName",
                                            "slug": "functions/corelibrary/getdisplayname"
                                        },
                                        {
                                            "label": "GetDisplayNameLink",
                                            "slug": "functions/corelibrary/getdisplaynamelink"
                                        },
                                        {
                                            "label": "GetDisplayVerbs",
                                            "slug": "functions/corelibrary/getdisplayverbs"
                                        },
                                        {
                                            "label": "GetListDisplayAlias",
                                            "slug": "functions/corelibrary/getlistdisplayalias"
                                        },
                                        {
                                            "label": "GetNonTransparentParent",
                                            "slug": "functions/corelibrary/getnontransparentparent"
                                        },
                                        {
                                            "label": "GetVolume",
                                            "slug": "functions/corelibrary/getvolume"
                                        },
                                        {
                                            "label": "Got",
                                            "slug": "functions/corelibrary/got"
                                        },
                                        {
                                            "label": "HelperCloseObject",
                                            "slug": "functions/corelibrary/helpercloseobject"
                                        },
                                        {
                                            "label": "HelperOpenObject",
                                            "slug": "functions/corelibrary/helperopenobject"
                                        },
                                        {
                                            "label": "IsSwitchedOn",
                                            "slug": "functions/corelibrary/isswitchedon"
                                        },
                                        {
                                            "label": "ListParents",
                                            "slug": "functions/corelibrary/listparents"
                                        },
                                        {
                                            "label": "SetDark",
                                            "slug": "functions/corelibrary/setdark"
                                        },
                                        {
                                            "label": "SetExitLightstrength",
                                            "slug": "functions/corelibrary/setexitlightstrength"
                                        },
                                        {
                                            "label": "SetLight",
                                            "slug": "functions/corelibrary/setlight"
                                        },
                                        {
                                            "label": "SetObjectLightstrength",
                                            "slug": "functions/corelibrary/setobjectlightstrength"
                                        },
                                        {
                                            "label": "ShowRoomDescription",
                                            "slug": "functions/corelibrary/showroomdescription"
                                        },
                                        {
                                            "label": "SwitchOff",
                                            "slug": "functions/corelibrary/switchoff"
                                        },
                                        {
                                            "label": "SwitchOn",
                                            "slug": "functions/corelibrary/switchon"
                                        }
                                    ]
                                },
                                {
                                    "label": "Internal Core.aslx Functions",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "functions/fn-internal"
                                        },
                                        {
                                            "label": "AddExternalStylesheet",
                                            "slug": "functions/corelibrary/addexternalstylesheet"
                                        },
                                        {
                                            "label": "AddStatusAttributesForElement",
                                            "slug": "functions/corelibrary/addstatusattributesforelement"
                                        },
                                        {
                                            "label": "CloseObject",
                                            "slug": "functions/corelibrary/closeobject"
                                        },
                                        {
                                            "label": "CommandLink",
                                            "slug": "functions/corelibrary/commandlink"
                                        },
                                        {
                                            "label": "CompareNames",
                                            "slug": "functions/corelibrary/comparenames"
                                        },
                                        {
                                            "label": "ContainsAccessible",
                                            "slug": "functions/corelibrary/containsaccessible"
                                        },
                                        {
                                            "label": "ContainsReachable",
                                            "slug": "functions/corelibrary/containsreachable"
                                        },
                                        {
                                            "label": "ContainsVisible",
                                            "slug": "functions/corelibrary/containsvisible"
                                        },
                                        {
                                            "label": "DisplayHttpLink",
                                            "slug": "functions/corelibrary/displayhttplink"
                                        },
                                        {
                                            "label": "DoAskTell",
                                            "slug": "functions/corelibrary/doasktell"
                                        },
                                        {
                                            "label": "DoDrop",
                                            "slug": "functions/corelibrary/dodrop"
                                        },
                                        {
                                            "label": "DoTake",
                                            "slug": "functions/corelibrary/dotake"
                                        },
                                        {
                                            "label": "FormatStatusAttribute",
                                            "slug": "functions/corelibrary/formatstatusattribute"
                                        },
                                        {
                                            "label": "GenerateMenuChoices",
                                            "slug": "functions/corelibrary/generatemenuchoices"
                                        },
                                        {
                                            "label": "GetDefaultPrefix",
                                            "slug": "functions/corelibrary/getdefaultprefix"
                                        },
                                        {
                                            "label": "GetKeywordsMatchStrength",
                                            "slug": "functions/corelibrary/getkeywordsmatchstrength"
                                        },
                                        {
                                            "label": "GetPlacesObjectsList",
                                            "slug": "functions/corelibrary/getplacesobjectslist"
                                        },
                                        {
                                            "label": "GetTaggedName",
                                            "slug": "functions/corelibrary/gettaggedname"
                                        },
                                        {
                                            "label": "GetUniqueElementName",
                                            "slug": "functions/getuniqueelementname"
                                        },
                                        {
                                            "label": "Grid CalculateMapCoordinates",
                                            "slug": "functions/corelibrary/grid_calculatemapcoordinates"
                                        },
                                        {
                                            "label": "Grid DrawPlayerInRoom",
                                            "slug": "functions/corelibrary/grid_drawplayerinroom"
                                        },
                                        {
                                            "label": "Grid DrawRoom",
                                            "slug": "functions/corelibrary/grid_drawroom"
                                        },
                                        {
                                            "label": "Grid SetScale",
                                            "slug": "functions/corelibrary/grid_setscale"
                                        },
                                        {
                                            "label": "Grid ShowCustomLayer",
                                            "slug": "functions/corelibrary/grid_showcustomlayer"
                                        },
                                        {
                                            "label": "Grid_AddNewShapePoint",
                                            "slug": "functions/corelibrary/grid_addnewshapepoint"
                                        },
                                        {
                                            "label": "Grid_ClearCustomLayer",
                                            "slug": "functions/corelibrary/grid_clearcustomlayer"
                                        },
                                        {
                                            "label": "Grid_DrawArrow",
                                            "slug": "functions/corelibrary/grid_drawarrow"
                                        },
                                        {
                                            "label": "Grid_DrawGridLines",
                                            "slug": "functions/corelibrary/grid_drawgridlines"
                                        },
                                        {
                                            "label": "Grid_DrawImage",
                                            "slug": "functions/corelibrary/grid_drawimage"
                                        },
                                        {
                                            "label": "Grid_DrawLine",
                                            "slug": "functions/corelibrary/grid_drawline"
                                        },
                                        {
                                            "label": "Grid_DrawShape",
                                            "slug": "functions/corelibrary/grid_drawshape"
                                        },
                                        {
                                            "label": "Grid_DrawSquare",
                                            "slug": "functions/corelibrary/grid_drawsquare"
                                        },
                                        {
                                            "label": "Grid_DrawSvg",
                                            "slug": "functions/corelibrary/grid_drawsvg"
                                        },
                                        {
                                            "label": "Grid_LoadSvg",
                                            "slug": "functions/corelibrary/grid_loadsvg"
                                        },
                                        {
                                            "label": "Grid_Redraw",
                                            "slug": "functions/corelibrary/grid_redraw"
                                        },
                                        {
                                            "label": "Grid_SetCentre",
                                            "slug": "functions/corelibrary/grid_setcentre"
                                        },
                                        {
                                            "label": "HandleCommand",
                                            "slug": "functions/corelibrary/handlecommand"
                                        },
                                        {
                                            "label": "HandleSingleCommand",
                                            "slug": "functions/corelibrary/handlesinglecommand"
                                        },
                                        {
                                            "label": "HandleSingleCommandPattern",
                                            "slug": "functions/corelibrary/handlesinglecommandpattern"
                                        },
                                        {
                                            "label": "InitInterface",
                                            "slug": "functions/corelibrary/initinterface"
                                        },
                                        {
                                            "label": "InitPOV",
                                            "slug": "functions/corelibrary/initpov"
                                        },
                                        {
                                            "label": "InitVerbsList",
                                            "slug": "functions/corelibrary/initverbslist"
                                        },
                                        {
                                            "label": "IsGameRunning",
                                            "slug": "functions/isgamerunning"
                                        },
                                        {
                                            "label": "ListObjectContents",
                                            "slug": "functions/corelibrary/listobjectcontents"
                                        },
                                        {
                                            "label": "ObjectLink",
                                            "slug": "functions/corelibrary/objectlink"
                                        },
                                        {
                                            "label": "OnEnterRoom",
                                            "slug": "functions/corelibrary/onenterroom"
                                        },
                                        {
                                            "label": "OpenObject",
                                            "slug": "functions/corelibrary/openobject"
                                        },
                                        {
                                            "label": "Populate",
                                            "slug": "functions/populate"
                                        },
                                        {
                                            "label": "ResolveName",
                                            "slug": "functions/corelibrary/resolvename"
                                        },
                                        {
                                            "label": "ResolveNameInternal",
                                            "slug": "functions/corelibrary/resolvenameinternal"
                                        },
                                        {
                                            "label": "ResolveNameList",
                                            "slug": "functions/corelibrary/resolvenamelist"
                                        },
                                        {
                                            "label": "ResolveNameListItem",
                                            "slug": "functions/corelibrary/resolvenamelistitem"
                                        },
                                        {
                                            "label": "RunTurnScripts",
                                            "slug": "functions/corelibrary/runturnscripts"
                                        },
                                        {
                                            "label": "StartGame",
                                            "slug": "functions/corelibrary/startgame"
                                        },
                                        {
                                            "label": "TryOpenClose",
                                            "slug": "functions/corelibrary/tryopenclose"
                                        }
                                    ]
                                },
                                {
                                    "label": "Mathematical Functions",
                                    "slug": "functions/fn-maths"
                                }
                            ]
                        },
                        {
                            "label": "Attributes",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "about_attributes"
                                },
                                {
                                    "label": "Important Attributes",
                                    "slug": "important_attributes"
                                },
                                {
                                    "label": "Status Attributes",
                                    "slug": "status_attributes"
                                },
                                {
                                    "label": "Change Script",
                                    "slug": "change_scripts"
                                },
                                {
                                    "label": "Attribute Types",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "types"
                                        },
                                        {
                                            "label": "Null",
                                            "slug": "types/null"
                                        },
                                        {
                                            "label": "String",
                                            "slug": "types/string"
                                        },
                                        {
                                            "label": "Script",
                                            "slug": "types/script"
                                        },
                                        {
                                            "label": "Boolean",
                                            "slug": "types/boolean"
                                        },
                                        {
                                            "label": "Int",
                                            "slug": "types/int"
                                        },
                                        {
                                            "label": "Double",
                                            "slug": "types/double"
                                        },
                                        {
                                            "label": "Object",
                                            "slug": "types/object"
                                        },
                                        {
                                            "label": "Stringlist",
                                            "slug": "types/stringlist"
                                        },
                                        {
                                            "label": "Objectlist",
                                            "slug": "types/objectlist"
                                        },
                                        {
                                            "label": "List",
                                            "slug": "types/list"
                                        },
                                        {
                                            "label": "Objectdictionary",
                                            "slug": "types/objectdictionary"
                                        },
                                        {
                                            "label": "Scriptdictionary",
                                            "slug": "types/scriptdictionary"
                                        },
                                        {
                                            "label": "Dictionary",
                                            "slug": "types/dictionary"
                                        },
                                        {
                                            "label": "Using Delegates",
                                            "slug": "types/using_delegates"
                                        },
                                        {
                                            "label": "Command pattern",
                                            "slug": "types/command_pattern"
                                        },
                                        {
                                            "label": "Stringdictionary",
                                            "slug": "types/stringdictionary"
                                        }
                                    ]
                                },
                                {
                                    "label": "Attribute Reference",
                                    "collapsed": true,
                                    "items": [
                                        {
                                            "label": "Overview",
                                            "slug": "attributes"
                                        },
                                        {
                                            "label": "alias",
                                            "slug": "attributes/alias"
                                        },
                                        {
                                            "label": "allobjects",
                                            "slug": "attributes/allobjects"
                                        },
                                        {
                                            "label": "alt",
                                            "slug": "attributes/alt"
                                        },
                                        {
                                            "label": "appendobjectdescription",
                                            "slug": "attributes/appendobjectdescription"
                                        },
                                        {
                                            "label": "article",
                                            "slug": "attributes/article"
                                        },
                                        {
                                            "label": "ask",
                                            "slug": "attributes/ask"
                                        },
                                        {
                                            "label": "askdefault",
                                            "slug": "attributes/askdefault"
                                        },
                                        {
                                            "label": "autodescription",
                                            "slug": "attributes/autodescription"
                                        },
                                        {
                                            "label": "autodisplayverbs",
                                            "slug": "attributes/autodisplayverbs"
                                        },
                                        {
                                            "label": "autoopen",
                                            "slug": "attributes/autoopen"
                                        },
                                        {
                                            "label": "autounlock",
                                            "slug": "attributes/autounlock"
                                        },
                                        {
                                            "label": "backgroundimage",
                                            "slug": "attributes/backgroundimage"
                                        },
                                        {
                                            "label": "backgroundopacity",
                                            "slug": "attributes/backgroundopacity"
                                        },
                                        {
                                            "label": "beforefirstenter",
                                            "slug": "attributes/beforefirstenter"
                                        },
                                        {
                                            "label": "canlockopen",
                                            "slug": "attributes/canlockopen"
                                        },
                                        {
                                            "label": "clearframe",
                                            "slug": "attributes/clearframe"
                                        },
                                        {
                                            "label": "close",
                                            "slug": "attributes/close"
                                        },
                                        {
                                            "label": "closescript",
                                            "slug": "attributes/closescript"
                                        },
                                        {
                                            "label": "compassdirections",
                                            "slug": "attributes/compassdirections"
                                        },
                                        {
                                            "label": "container",
                                            "slug": "attributes/container"
                                        },
                                        {
                                            "label": "container_base",
                                            "slug": "attributes/container_base"
                                        },
                                        {
                                            "label": "container_closed",
                                            "slug": "attributes/container_closed"
                                        },
                                        {
                                            "label": "container_limited",
                                            "slug": "attributes/container_limited"
                                        },
                                        {
                                            "label": "container_lockable",
                                            "slug": "attributes/container_lockable"
                                        },
                                        {
                                            "label": "container_open",
                                            "slug": "attributes/container_open"
                                        },
                                        {
                                            "label": "containerfullmessage",
                                            "slug": "attributes/containerfullmessage"
                                        },
                                        {
                                            "label": "contentsprefix",
                                            "slug": "attributes/contentsprefix"
                                        },
                                        {
                                            "label": "dark",
                                            "slug": "attributes/dark"
                                        },
                                        {
                                            "label": "darklevel",
                                            "slug": "attributes/darklevel"
                                        },
                                        {
                                            "label": "defaultbackground",
                                            "slug": "attributes/defaultbackground"
                                        },
                                        {
                                            "label": "defaultfont",
                                            "slug": "attributes/defaultfont"
                                        },
                                        {
                                            "label": "defaultfontsize",
                                            "slug": "attributes/defaultfontsize"
                                        },
                                        {
                                            "label": "defaultforeground",
                                            "slug": "attributes/defaultforeground"
                                        },
                                        {
                                            "label": "defaultlinkforeground",
                                            "slug": "attributes/defaultlinkforeground"
                                        },
                                        {
                                            "label": "defaultobject",
                                            "slug": "attributes/defaultobject"
                                        },
                                        {
                                            "label": "defaultwebfont",
                                            "slug": "attributes/defaultwebfont"
                                        },
                                        {
                                            "label": "descprefix",
                                            "slug": "attributes/descprefix"
                                        },
                                        {
                                            "label": "description",
                                            "slug": "attributes/description"
                                        },
                                        {
                                            "label": "displayroomdescriptiononstart",
                                            "slug": "attributes/displayroomdescriptiononstart"
                                        },
                                        {
                                            "label": "displayverbs",
                                            "slug": "attributes/displayverbs"
                                        },
                                        {
                                            "label": "drop",
                                            "slug": "attributes/drop"
                                        },
                                        {
                                            "label": "dropmsg",
                                            "slug": "attributes/dropmsg"
                                        },
                                        {
                                            "label": "echohyperlinks",
                                            "slug": "attributes/echohyperlinks"
                                        },
                                        {
                                            "label": "edible",
                                            "slug": "attributes/edible"
                                        },
                                        {
                                            "label": "editor_object",
                                            "slug": "attributes/editor_object"
                                        },
                                        {
                                            "label": "editor_room",
                                            "slug": "attributes/editor_room"
                                        },
                                        {
                                            "label": "enablehyperlinks",
                                            "slug": "attributes/enablehyperlinks"
                                        },
                                        {
                                            "label": "enter",
                                            "slug": "attributes/enter"
                                        },
                                        {
                                            "label": "exitslistprefix",
                                            "slug": "attributes/exitslistprefix"
                                        },
                                        {
                                            "label": "female",
                                            "slug": "attributes/female"
                                        },
                                        {
                                            "label": "femaleplural",
                                            "slug": "attributes/femaleplural"
                                        },
                                        {
                                            "label": "firstenter",
                                            "slug": "attributes/firstenter"
                                        },
                                        {
                                            "label": "gender",
                                            "slug": "attributes/gender"
                                        },
                                        {
                                            "label": "give",
                                            "slug": "attributes/give"
                                        },
                                        {
                                            "label": "giveanything",
                                            "slug": "attributes/giveanything"
                                        },
                                        {
                                            "label": "givesingle",
                                            "slug": "attributes/givesingle"
                                        },
                                        {
                                            "label": "giveto",
                                            "slug": "attributes/giveto"
                                        },
                                        {
                                            "label": "givetoanything",
                                            "slug": "attributes/givetoanything"
                                        },
                                        {
                                            "label": "grid_border",
                                            "slug": "attributes/grid_border"
                                        },
                                        {
                                            "label": "grid_bordersides",
                                            "slug": "attributes/grid_bordersides"
                                        },
                                        {
                                            "label": "grid_borderwidth",
                                            "slug": "attributes/grid_borderwidth"
                                        },
                                        {
                                            "label": "grid_fill",
                                            "slug": "attributes/grid_fill"
                                        },
                                        {
                                            "label": "grid_label",
                                            "slug": "attributes/grid_label"
                                        },
                                        {
                                            "label": "grid_length",
                                            "slug": "attributes/grid_length"
                                        },
                                        {
                                            "label": "grid_parent_offset_auto",
                                            "slug": "attributes/grid_parent_offset_auto"
                                        },
                                        {
                                            "label": "grid_parent_offset_x",
                                            "slug": "attributes/grid_parent_offset_x"
                                        },
                                        {
                                            "label": "grid_parent_offset_y",
                                            "slug": "attributes/grid_parent_offset_y"
                                        },
                                        {
                                            "label": "grid_render",
                                            "slug": "attributes/grid_render"
                                        },
                                        {
                                            "label": "grid_width",
                                            "slug": "attributes/grid_width"
                                        },
                                        {
                                            "label": "gridmap",
                                            "slug": "attributes/gridmap"
                                        },
                                        {
                                            "label": "hidechildren",
                                            "slug": "attributes/hidechildren"
                                        },
                                        {
                                            "label": "inventoryverbs",
                                            "slug": "attributes/inventoryverbs"
                                        },
                                        {
                                            "label": "isopen",
                                            "slug": "attributes/isopen"
                                        },
                                        {
                                            "label": "key",
                                            "slug": "attributes/key"
                                        },
                                        {
                                            "label": "languageid",
                                            "slug": "attributes/languageid"
                                        },
                                        {
                                            "label": "lightstrength",
                                            "slug": "attributes/lightstrength"
                                        },
                                        {
                                            "label": "listchildren",
                                            "slug": "attributes/listchildren"
                                        },
                                        {
                                            "label": "listchildrenprefix",
                                            "slug": "attributes/listchildrenprefix"
                                        },
                                        {
                                            "label": "locked",
                                            "slug": "attributes/locked"
                                        },
                                        {
                                            "label": "lockmessage",
                                            "slug": "attributes/lockmessage"
                                        },
                                        {
                                            "label": "look",
                                            "slug": "attributes/look"
                                        },
                                        {
                                            "label": "male",
                                            "slug": "attributes/male"
                                        },
                                        {
                                            "label": "maleplural",
                                            "slug": "attributes/maleplural"
                                        },
                                        {
                                            "label": "mapscale",
                                            "slug": "attributes/mapscale"
                                        },
                                        {
                                            "label": "mapsize",
                                            "slug": "attributes/mapsize"
                                        },
                                        {
                                            "label": "maxobjects",
                                            "slug": "attributes/maxobjects"
                                        },
                                        {
                                            "label": "menubackground",
                                            "slug": "attributes/menubackground"
                                        },
                                        {
                                            "label": "menufont",
                                            "slug": "attributes/menufont"
                                        },
                                        {
                                            "label": "menufontsize",
                                            "slug": "attributes/menufontsize"
                                        },
                                        {
                                            "label": "menuforeground",
                                            "slug": "attributes/menuforeground"
                                        },
                                        {
                                            "label": "menuhoverbackground",
                                            "slug": "attributes/menuhoverbackground"
                                        },
                                        {
                                            "label": "menuhoverforeground",
                                            "slug": "attributes/menuhoverforeground"
                                        },
                                        {
                                            "label": "namedfemale",
                                            "slug": "attributes/namedfemale"
                                        },
                                        {
                                            "label": "namedmale",
                                            "slug": "attributes/namedmale"
                                        },
                                        {
                                            "label": "nokeymessage",
                                            "slug": "attributes/nokeymessage"
                                        },
                                        {
                                            "label": "objectslistprefix",
                                            "slug": "attributes/objectslistprefix"
                                        },
                                        {
                                            "label": "onclose",
                                            "slug": "attributes/onclose"
                                        },
                                        {
                                            "label": "ondrop",
                                            "slug": "attributes/ondrop"
                                        },
                                        {
                                            "label": "onexit",
                                            "slug": "attributes/onexit"
                                        },
                                        {
                                            "label": "onlock",
                                            "slug": "attributes/onlock"
                                        },
                                        {
                                            "label": "onopen",
                                            "slug": "attributes/onopen"
                                        },
                                        {
                                            "label": "onswitchoff",
                                            "slug": "attributes/onswitchoff"
                                        },
                                        {
                                            "label": "onswitchon",
                                            "slug": "attributes/onswitchon"
                                        },
                                        {
                                            "label": "ontake",
                                            "slug": "attributes/ontake"
                                        },
                                        {
                                            "label": "onunlock",
                                            "slug": "attributes/onunlock"
                                        },
                                        {
                                            "label": "open",
                                            "slug": "attributes/open"
                                        },
                                        {
                                            "label": "openable",
                                            "slug": "attributes/openable"
                                        },
                                        {
                                            "label": "openscript",
                                            "slug": "attributes/openscript"
                                        },
                                        {
                                            "label": "parent",
                                            "slug": "attributes/parent"
                                        },
                                        {
                                            "label": "parserignoreprefixes",
                                            "slug": "attributes/parserignoreprefixes"
                                        },
                                        {
                                            "label": "picture (attribute)",
                                            "slug": "attributes/picture"
                                        },
                                        {
                                            "label": "plural",
                                            "slug": "attributes/plural"
                                        },
                                        {
                                            "label": "pov_alias",
                                            "slug": "attributes/pov_alias"
                                        },
                                        {
                                            "label": "pov_alt",
                                            "slug": "attributes/pov_alt"
                                        },
                                        {
                                            "label": "pov_article",
                                            "slug": "attributes/pov_article"
                                        },
                                        {
                                            "label": "pov_gender",
                                            "slug": "attributes/pov_gender"
                                        },
                                        {
                                            "label": "pov_look",
                                            "slug": "attributes/pov_look"
                                        },
                                        {
                                            "label": "prefix",
                                            "slug": "attributes/prefix"
                                        },
                                        {
                                            "label": "scenery",
                                            "slug": "attributes/scenery"
                                        },
                                        {
                                            "label": "selfuseanything",
                                            "slug": "attributes/selfuseanything"
                                        },
                                        {
                                            "label": "selfuseon",
                                            "slug": "attributes/selfuseon"
                                        },
                                        {
                                            "label": "setbackgroundopacity",
                                            "slug": "attributes/setbackgroundopacity"
                                        },
                                        {
                                            "label": "showdescriptiononenter",
                                            "slug": "attributes/showdescriptiononenter"
                                        },
                                        {
                                            "label": "showhealth",
                                            "slug": "attributes/showhealth"
                                        },
                                        {
                                            "label": "showpanes",
                                            "slug": "attributes/showpanes"
                                        },
                                        {
                                            "label": "showscore",
                                            "slug": "attributes/showscore"
                                        },
                                        {
                                            "label": "start",
                                            "slug": "attributes/start"
                                        },
                                        {
                                            "label": "statusattributes",
                                            "slug": "attributes/statusattributes"
                                        },
                                        {
                                            "label": "suffix",
                                            "slug": "attributes/suffix"
                                        },
                                        {
                                            "label": "surface",
                                            "slug": "attributes/surface"
                                        },
                                        {
                                            "label": "switchable",
                                            "slug": "attributes/switchable"
                                        },
                                        {
                                            "label": "switchedoffdesc",
                                            "slug": "attributes/switchedoffdesc"
                                        },
                                        {
                                            "label": "switchedon",
                                            "slug": "attributes/switchedon"
                                        },
                                        {
                                            "label": "switchedondesc",
                                            "slug": "attributes/switchedondesc"
                                        },
                                        {
                                            "label": "switchoffmsg",
                                            "slug": "attributes/switchoffmsg"
                                        },
                                        {
                                            "label": "switchonmsg",
                                            "slug": "attributes/switchonmsg"
                                        },
                                        {
                                            "label": "take",
                                            "slug": "attributes/take"
                                        },
                                        {
                                            "label": "takemsg",
                                            "slug": "attributes/takemsg"
                                        },
                                        {
                                            "label": "tell",
                                            "slug": "attributes/tell"
                                        },
                                        {
                                            "label": "telldefault",
                                            "slug": "attributes/telldefault"
                                        },
                                        {
                                            "label": "transparent",
                                            "slug": "attributes/transparent"
                                        },
                                        {
                                            "label": "underlinehyperlinks",
                                            "slug": "attributes/underlinehyperlinks"
                                        },
                                        {
                                            "label": "unlockmessage",
                                            "slug": "attributes/unlockmessage"
                                        },
                                        {
                                            "label": "use",
                                            "slug": "attributes/use"
                                        },
                                        {
                                            "label": "useanything",
                                            "slug": "attributes/useanything"
                                        },
                                        {
                                            "label": "usedefaultprefix",
                                            "slug": "attributes/usedefaultprefix"
                                        },
                                        {
                                            "label": "useframe",
                                            "slug": "attributes/useframe"
                                        },
                                        {
                                            "label": "useon",
                                            "slug": "attributes/useon"
                                        },
                                        {
                                            "label": "visible",
                                            "slug": "attributes/visible"
                                        },
                                        {
                                            "label": "visited",
                                            "slug": "attributes/visited"
                                        },
                                        {
                                            "label": "volume",
                                            "slug": "attributes/volume"
                                        }
                                    ]
                                },
                                {
                                    "label": "Mutable Attributes on Inherited Types",
                                    "slug": "notes"
                                }
                            ]
                        },
                        {
                            "label": "XML Elements",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "elements"
                                },
                                {
                                    "label": "asl element",
                                    "slug": "elements/asl"
                                },
                                {
                                    "label": "library element",
                                    "slug": "elements/library"
                                },
                                {
                                    "label": "include element",
                                    "slug": "elements/include"
                                },
                                {
                                    "label": "template element",
                                    "slug": "elements/template"
                                },
                                {
                                    "label": "dynamictemplate element",
                                    "slug": "elements/dynamictemplate"
                                },
                                {
                                    "label": "verbtemplate element",
                                    "slug": "elements/verbtemplate"
                                },
                                {
                                    "label": "function element",
                                    "slug": "elements/function"
                                },
                                {
                                    "label": "command element",
                                    "slug": "elements/command"
                                },
                                {
                                    "label": "verb element",
                                    "slug": "elements/verb"
                                },
                                {
                                    "label": "type element",
                                    "slug": "elements/type"
                                },
                                {
                                    "label": "game element",
                                    "slug": "elements/game"
                                },
                                {
                                    "label": "object element",
                                    "slug": "elements/object"
                                },
                                {
                                    "label": "exit element",
                                    "slug": "elements/exit"
                                },
                                {
                                    "label": "walkthrough element",
                                    "slug": "elements/walkthrough"
                                },
                                {
                                    "label": "timer element",
                                    "slug": "elements/timer"
                                },
                                {
                                    "label": "turnscript element",
                                    "slug": "elements/turnscript"
                                },
                                {
                                    "label": "implied element",
                                    "slug": "elements/implied"
                                },
                                {
                                    "label": "delegate element",
                                    "slug": "elements/delegate"
                                },
                                {
                                    "label": "javascript element",
                                    "slug": "elements/javascript"
                                },
                                {
                                    "label": "editor element",
                                    "slug": "elements/editor"
                                },
                                {
                                    "label": "tab element",
                                    "slug": "elements/tab"
                                },
                                {
                                    "label": "control element",
                                    "slug": "elements/control"
                                },
                                {
                                    "label": "resource element",
                                    "slug": "elements/resource"
                                },
                                {
                                    "label": "inherit element",
                                    "slug": "elements/inherit"
                                }
                            ]
                        },
                        {
                            "label": "JS functions",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "js/js"
                                },
                                {
                                    "label": "addScript",
                                    "slug": "js/addscript"
                                },
                                {
                                    "label": "addText",
                                    "slug": "js/addtext"
                                },
                                {
                                    "label": "ShowGrid",
                                    "slug": "js/colourblend"
                                },
                                {
                                    "label": "eval",
                                    "slug": "js/eval"
                                },
                                {
                                    "label": "scrollToEnd",
                                    "slug": "js/scrolltoend"
                                },
                                {
                                    "label": "setCommands",
                                    "slug": "js/setcommands"
                                },
                                {
                                    "label": "setCss",
                                    "slug": "js/setcss"
                                },
                                {
                                    "label": "setCustomStatus",
                                    "slug": "js/setcustomstatus"
                                },
                                {
                                    "label": "setInterfaceString",
                                    "slug": "js/setinterfacestring"
                                },
                                {
                                    "label": "setPanes",
                                    "slug": "js/setpanes"
                                },
                                {
                                    "label": "ShowGrid",
                                    "slug": "js/showgrid"
                                },
                                {
                                    "label": "showPopup/showPopupCustomSize/showPopupFullscreen",
                                    "slug": "js/showpopup"
                                },
                                {
                                    "label": "uiShow/uiHide",
                                    "slug": "js/uishow"
                                },
                                {
                                    "label": "updateLocation",
                                    "slug": "js/updatelocation"
                                },
                                {
                                    "label": "updateStatus",
                                    "slug": "js/updatestatus"
                                },
                                {
                                    "label": "whereAmI",
                                    "slug": "js/whereami"
                                }
                            ]
                        },
                        {
                            "label": "Hard-coded Functions and Library Functions",
                            "slug": "functions/hardcoded"
                        },
                        {
                            "label": "ASLX File Format",
                            "slug": "aslx"
                        },
                        {
                            "label": "ASL Requirements",
                            "slug": "asl_requirements"
                        }
                    ]
                },
                {
                    "label": "Publishing & Community",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "publishing/publishing"
                        },
                        {
                            "label": "Competition Entry",
                            "slug": "publishing/competition_entry"
                        },
                        {
                            "label": "Creating with Trizbort and Quest",
                            "slug": "publishing/trizbort"
                        }
                    ]
                },
                {
                    "label": "Release Notes",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Quest 5.8",
                            "slug": "release-notes/quest5_8"
                        },
                        {
                            "label": "Quest 5.7",
                            "slug": "release-notes/quest5_7"
                        },
                        {
                            "label": "Upgrade Notes",
                            "slug": "release-notes/upgrade_notes"
                        },
                        {
                            "label": "Older Versions",
                            "slug": "release-notes/older-versions"
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
                            "label": "Building from Source",
                            "slug": "developers/source_code"
                        },
                        {
                            "label": "Open source",
                            "slug": "developers/open_source"
                        }
                    ]
                }
            ],
        }),
    ],
});
