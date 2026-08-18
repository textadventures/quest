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
                    "label": "Start Here",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Introduction",
                            "slug": "intro"
                        },
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
                        },
                        {
                            "label": "The Cloak of Darkness",
                            "slug": "tutorial/cloak_of_darkness"
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
                                    "label": "Commands specific to a room",
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
                                    "label": "Handling multiple items (and all)",
                                    "slug": "howto/commands/handling_multiple"
                                },
                                {
                                    "label": "Pattern matching with regular expressions",
                                    "slug": "howto/commands/pattern_matching"
                                },
                                {
                                    "label": "Advanced scope for items",
                                    "slug": "howto/commands/advanced_scope"
                                }
                            ]
                        },
                        {
                            "label": "World & Objects",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/world/text_processor"
                                },
                                {
                                    "label": "Changing templates",
                                    "slug": "howto/world/changing_templates"
                                },
                                {
                                    "label": "Exits",
                                    "slug": "howto/world/exits"
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
                                    "slug": "howto/world/handling_light_and_dark"
                                },
                                {
                                    "label": "Wearable items",
                                    "slug": "howto/world/wearables"
                                },
                                {
                                    "label": "Score, health and money",
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
                                    "label": "When the player saves a game",
                                    "slug": "howto/world/about_save"
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
                                    "label": "Images in Quest",
                                    "slug": "howto/multimedia/images"
                                },
                                {
                                    "label": "Creating images on the fly",
                                    "slug": "howto/multimedia/images_on_the_fly"
                                },
                                {
                                    "label": "Adding sounds to your game",
                                    "slug": "howto/multimedia/adding_sounds"
                                },
                                {
                                    "label": "Adding videos",
                                    "slug": "howto/multimedia/adding_videos"
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
                                    "slug": "howto/scripting/creating_functions_which_return_a_value"
                                },
                                {
                                    "label": "Using lists",
                                    "slug": "howto/scripting/using_lists"
                                },
                                {
                                    "label": "Using dictionaries",
                                    "slug": "howto/scripting/using_dictionaries"
                                },
                                {
                                    "label": "Using turnscripts",
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
                                    "label": "Editing in full code view",
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
                                    "label": "Unit testing",
                                    "slug": "howto/scripting/unit_testing"
                                },
                                {
                                    "label": "How to copy-and-paste code",
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
                                    "label": "How to keep score",
                                    "slug": "howto/tasks/keeping_score"
                                },
                                {
                                    "label": "How to build a transit system",
                                    "slug": "howto/tasks/transit_system"
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
                                    "slug": "howto/tasks/setting_up_door"
                                },
                                {
                                    "label": "Give the player character memory or wiki",
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
                                    "slug": "howto/npcs/speak_to"
                                },
                                {
                                    "label": "Building an Ask/Tell system",
                                    "slug": "howto/npcs/ask_about"
                                },
                                {
                                    "label": "Making NPCs patrol",
                                    "slug": "howto/npcs/patrolling_npcs"
                                },
                                {
                                    "label": "Making NPCs act independently",
                                    "slug": "howto/npcs/independent_npcs"
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
                                    "slug": "howto/ux/display_verbs"
                                },
                                {
                                    "label": "Custom command panes",
                                    "slug": "howto/ux/command_pane"
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
                                    "slug": "howto/ux/custom_panes"
                                },
                                {
                                    "label": "JavaScript to Quest with ASLEvent",
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
                            "label": "RPG Mechanics",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "howto/rpg/rpg-intro"
                                },
                                {
                                    "label": "Character creation",
                                    "slug": "howto/rpg/character_creation"
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
                            "label": "Troubleshooting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Overview",
                                    "slug": "helpsheets"
                                },
                                {
                                    "label": "Introduction at start of game",
                                    "slug": "helpsheets/hs_introduction"
                                },
                                {
                                    "label": "Removing the \"a\" before an object's name",
                                    "slug": "helpsheets/hs_removea"
                                },
                                {
                                    "label": "Object blocking an exit",
                                    "slug": "helpsheets/hs_blockingexit"
                                },
                                {
                                    "label": "Locked exits and how to open them?",
                                    "slug": "helpsheets/hs_lockedexits"
                                },
                                {
                                    "label": "A monster waiting behind a door",
                                    "slug": "helpsheets/hs_baddy2"
                                },
                                {
                                    "label": "Making objects appear when a light is switched on",
                                    "slug": "helpsheets/hs_objectsappear"
                                },
                                {
                                    "label": "Adding a yes/no quiz question",
                                    "slug": "helpsheets/hs_addingquestion1"
                                },
                                {
                                    "label": "Objects appearing (inside other objects)",
                                    "slug": "helpsheets/hs_appearingobjects"
                                },
                                {
                                    "label": "\"Key\" inside another object",
                                    "slug": "helpsheets/hs_keyinside"
                                },
                                {
                                    "label": "Adding music to a radio",
                                    "slug": "helpsheets/hs_radio"
                                },
                                {
                                    "label": "Creating a countdown timer",
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
                                    "label": "A hint system",
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
                            "label": "Types",
                            "slug": "advanced-topics/about_types"
                        },
                        {
                            "label": "Using Delegates",
                            "slug": "advanced-topics/using_delegates"
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
                                    "label": "Functions for Attributes",
                                    "slug": "functions/attributes"
                                },
                                {
                                    "label": "Functions for Variables",
                                    "slug": "functions/variables"
                                },
                                {
                                    "label": "Functions for Objects and Exits",
                                    "slug": "functions/objects"
                                },
                                {
                                    "label": "Timers and Turnscripts",
                                    "slug": "functions/timers-turnscripts"
                                },
                                {
                                    "label": "User Interface Functions",
                                    "slug": "functions/user-interface"
                                },
                                {
                                    "label": "List Functions",
                                    "slug": "functions/list"
                                },
                                {
                                    "label": "Scope Functions",
                                    "slug": "functions/scope"
                                },
                                {
                                    "label": "Dictionary Functions",
                                    "slug": "functions/dictionary"
                                },
                                {
                                    "label": "String Functions",
                                    "slug": "functions/string"
                                },
                                {
                                    "label": "Clothing Functions",
                                    "slug": "functions/clothing"
                                },
                                {
                                    "label": "Randomising Functions",
                                    "slug": "functions/random"
                                },
                                {
                                    "label": "General Functions",
                                    "slug": "functions/general"
                                },
                                {
                                    "label": "Core.aslx Functions",
                                    "slug": "functions/core"
                                },
                                {
                                    "label": "Internal Core.aslx Functions",
                                    "slug": "functions/internal-core"
                                },
                                {
                                    "label": "Mathematical Functions",
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
                                    "slug": "types"
                                },
                                {
                                    "label": "Attribute Reference",
                                    "slug": "attributes"
                                },
                                {
                                    "label": "Mutable Attributes on Inherited Types",
                                    "slug": "notes"
                                }
                            ]
                        },
                        {
                            "label": "XML Elements",
                            "slug": "elements"
                        },
                        {
                            "label": "JS functions",
                            "slug": "js"
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
                            "label": "Hosting your game",
                            "slug": "publishing/hosting"
                        },
                        {
                            "label": "WebPlayer",
                            "slug": "publishing/webplayer"
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
