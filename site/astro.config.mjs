// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// https://astro.build/config
export default defineConfig({
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
                    label: "Guides",
                    items: [{ autogenerate: { directory: "guides" } }],
                },
                {
                    label: "Quest 5 Reference",
                    items: [
                    {
                        label: "Advanced Topics",
                        items: [
                            {
                                label: "Overview",
                                slug: "advanced"
                            },
                            {
                                label: "Changing templates",
                                slug: "changing_templates"
                            },
                            {
                                label: "Editor User Interface Elements",
                                slug: "editor_user_interface_elements"
                            },
                            {
                                label: "Overriding functions",
                                slug: "overriding"
                            },
                            {
                                label: "Translating Quest",
                                slug: "translating_quest"
                            },
                            {
                                label: "Undo support",
                                slug: "undo_support"
                            },
                            {
                                label: "Using Tabs for Types",
                                slug: "tabs_for_types"
                            },
                            {
                                label: "Using Templates",
                                slug: "using_templates"
                            },
                            {
                                label: "Using and creating libraries",
                                slug: "using_libraries"
                            },
                            {
                                label: "Using inherited types",
                                slug: "using_inherited_types"
                            }
                        ]
                    },
                    {
                        label: "Attribute Types",
                        items: [
                            {
                                autogenerate: {
                                    directory: "types"
                                }
                            }
                        ]
                    },
                    {
                        label: "Attributes",
                        items: [
                            {
                                label: "Overview",
                                slug: "about_attributes"
                            },
                            {
                                label: "Change Script",
                                slug: "change_scripts"
                            },
                            {
                                label: "Important Attributes",
                                slug: "important_attributes"
                            },
                            {
                                label: "Notes",
                                slug: "notes"
                            },
                            {
                                label: "Status Attributes",
                                slug: "status_attributes"
                            },
                            {
                                label: "Attribute Reference",
                                items: [{ autogenerate: { directory: "attributes" } }]
                            }
                        ]
                    },
                    {
                        label: "Characters (NPCs)",
                        items: [
                            {
                                label: "Overview",
                                slug: "npcs"
                            },
                            {
                                label: "Followers",
                                slug: "follower"
                            },
                            {
                                label: "Handling ASK/TELL",
                                slug: "ask_about"
                            },
                            {
                                label: "Handling ASK/TELL",
                                slug: "ask_tell"
                            },
                            {
                                label: "Handling SPEAK TO",
                                slug: "speak_to"
                            },
                            {
                                label: "Introduction to Conversations",
                                slug: "conversations"
                            },
                            {
                                label: "Making NPCs Act Independently",
                                slug: "independent_npcs"
                            },
                            {
                                label: "Making NPCs Patrol",
                                slug: "patrolling_npcs"
                            }
                        ]
                    },
                    {
                        label: "Coding With Quest",
                        items: [
                            {
                                label: "Overview",
                                slug: "coding"
                            },
                            {
                                label: "Advanced game scripts",
                                slug: "advanced_game_scripts"
                            },
                            {
                                label: "Attack of the Clones!",
                                slug: "clones"
                            },
                            {
                                label: "Blocks and scripts",
                                slug: "blocks_and_scripts"
                            },
                            {
                                label: "Creating functions",
                                slug: "creating_functions_which_return_a_value"
                            },
                            {
                                label: "Debugging your game",
                                slug: "debugging_your_game"
                            },
                            {
                                label: "Editing in Full Code View",
                                slug: "full_code_view"
                            },
                            {
                                label: "Expressions",
                                slug: "expressions"
                            },
                            {
                                label: "How to Copy-and-Paste Code",
                                slug: "copy_and_paste_code"
                            },
                            {
                                label: "Introduction to Coding",
                                slug: "introtocoding"
                            },
                            {
                                label: "Much Ado About Nothing",
                                slug: "null"
                            },
                            {
                                label: "Scopes",
                                slug: "scopes"
                            },
                            {
                                label: "The Cloak of Darkness",
                                slug: "cloak_of_darkness"
                            },
                            {
                                label: "Tips and Tricks for Editing in Full Code View",
                                slug: "codeview"
                            },
                            {
                                label: "Types",
                                slug: "about_types"
                            },
                            {
                                label: "Unit Testing",
                                slug: "unit_testing"
                            },
                            {
                                label: "Using \"doubles\"",
                                slug: "using_doubles"
                            },
                            {
                                label: "Using Dictionaries",
                                slug: "using_dictionaries"
                            },
                            {
                                label: "Using Javascript",
                                slug: "using_javascript"
                            },
                            {
                                label: "Using Lists",
                                slug: "using_lists"
                            },
                            {
                                label: "Using Turnscripts",
                                slug: "using_turnscripts"
                            },
                            {
                                label: "Using walkthroughs",
                                slug: "using_walkthroughs"
                            }
                        ]
                    },
                    {
                        label: "Developers",
                        items: [
                            {
                                label: "Overview",
                                slug: "developers"
                            },
                            {
                                label: "Building from Source",
                                slug: "source_code"
                            },
                            {
                                label: "Open source",
                                slug: "open_source"
                            }
                        ]
                    },
                    {
                        label: "Docs Style Guide",
                        slug: "style_guide"
                    },
                    {
                        label: "Features",
                        items: [
                            {
                                label: "Overview",
                                slug: "features"
                            },
                            {
                                label: "Exits",
                                slug: "exits"
                            },
                            {
                                label: "Handling light and dark",
                                slug: "handling_light_and_dark"
                            },
                            {
                                label: "Items that can be switched on and off",
                                slug: "switchable"
                            },
                            {
                                label: "Multi-state wearable items",
                                slug: "multistate-clothing"
                            },
                            {
                                label: "Multimedia",
                                items: [
                                    {
                                        label: "Overview",
                                        slug: "multimedia"
                                    },
                                    {
                                        label: "Adding Sounds to your Game",
                                        slug: "adding_sounds"
                                    },
                                    {
                                        label: "Adding Videos",
                                        slug: "adding_videos"
                                    },
                                    {
                                        label: "Creating Images on the Fly",
                                        slug: "images_on_the_fly"
                                    },
                                    {
                                        label: "Images in Quest",
                                        slug: "images"
                                    }
                                ]
                            },
                            {
                                label: "Score, Health and Money",
                                slug: "score_health_money"
                            },
                            {
                                label: "Text processor",
                                slug: "text_processor"
                            },
                            {
                                label: "Transcripts",
                                slug: "transcript"
                            },
                            {
                                label: "Using Containers",
                                slug: "containers"
                            },
                            {
                                label: "Using lockable exits",
                                slug: "using_lockable_exits"
                            },
                            {
                                label: "Wearable items",
                                slug: "wearables"
                            },
                            {
                                label: "When the Player Saves a Game",
                                slug: "about_save"
                            }
                        ]
                    },
                    {
                        label: "Functions",
                        items: [
                            {
                                autogenerate: {
                                    directory: "functions"
                                }
                            }
                        ]
                    },
                    {
                        label: "Helpsheets",
                        items: [
                            {
                                autogenerate: {
                                    directory: "helpsheets"
                                }
                            }
                        ]
                    },
                    {
                        label: "How To",
                        items: [
                            {
                                label: "Overview",
                                slug: "howto"
                            },
                            {
                                label: "Asking a question",
                                slug: "asking_a_question"
                            },
                            {
                                label: "Asking a simple question",
                                slug: "ask_simple_question"
                            },
                            {
                                label: "Changing the player object",
                                slug: "changing_the_player_object"
                            },
                            {
                                label: "Converting One Thing Into Another",
                                slug: "convert"
                            },
                            {
                                label: "Give the player character memory or Wiki",
                                slug: "memory_or_wiki"
                            },
                            {
                                label: "Handling water",
                                slug: "handling_water"
                            },
                            {
                                label: "How to Build a Transit System",
                                slug: "transit_system"
                            },
                            {
                                label: "How to Keep Score",
                                slug: "keeping_score"
                            },
                            {
                                label: "How to use functions",
                                slug: "about_functions"
                            },
                            {
                                label: "Keeping a journal",
                                slug: "keeping_a_journal"
                            },
                            {
                                label: "Move an object in a direction",
                                slug: "move_object"
                            },
                            {
                                label: "Multiple choices - using a switch script",
                                slug: "multiple_choices___using_a_switch_script"
                            },
                            {
                                label: "Randomisation",
                                slug: "random"
                            },
                            {
                                label: "Resolving Common Problems",
                                slug: "problems"
                            },
                            {
                                label: "Setting Up a Door",
                                slug: "setting_up_door"
                            },
                            {
                                label: "Setting up a shop",
                                slug: "shop"
                            },
                            {
                                label: "Showing a map",
                                slug: "showing_a_map"
                            },
                            {
                                label: "Showing a menu",
                                slug: "showing_a_menu"
                            },
                            {
                                label: "Tracking Time",
                                slug: "time"
                            },
                            {
                                label: "Use maths functionality",
                                slug: "use_maths_functionality"
                            },
                            {
                                label: "Using neutral language",
                                slug: "neutral_language"
                            }
                        ]
                    },
                    {
                        label: "How to use commands",
                        items: [
                            {
                                label: "Overview",
                                slug: "commands"
                            },
                            {
                                label: "Advanced Scope For Items",
                                slug: "advanced_scope"
                            },
                            {
                                label: "Commands Specific to a Room",
                                slug: "commands_for_room"
                            },
                            {
                                label: "Complex commands",
                                slug: "complex_commands"
                            },
                            {
                                label: "Handling Multiple Items (and All)",
                                slug: "handling_multiple"
                            },
                            {
                                label: "How to use verbs",
                                slug: "using_verbs"
                            },
                            {
                                label: "Pattern Matching with Regular Expressions",
                                slug: "pattern_matching"
                            },
                            {
                                label: "Use verbs",
                                slug: "use_verbs"
                            }
                        ]
                    },
                    {
                        label: "Introduction to RPGs",
                        items: [
                            {
                                label: "Overview",
                                slug: "rpg-intro"
                            },
                            {
                                label: "Character Creation",
                                slug: "character_creation"
                            },
                            {
                                label: "Spells for the Zombie Apocalypse",
                                slug: "zombie-apocalypse-spells"
                            },
                            {
                                label: "Zombie Apocalypse (part 1)",
                                slug: "zombie-apocalypse-1"
                            },
                            {
                                label: "Zombie Apocalypse (part 2)",
                                slug: "zombie-apocalypse-2"
                            }
                        ]
                    },
                    {
                        label: "JS Functions",
                        items: [
                            {
                                autogenerate: {
                                    directory: "js"
                                }
                            }
                        ]
                    },
                    {
                        label: "Other Guides",
                        items: [
                            {
                                label: "Overview",
                                slug: "other_guides"
                            },
                            {
                                label: "Creating with Trizbort and Quest",
                                slug: "trizbort"
                            }
                        ]
                    },
                    {
                        label: "Publishing",
                        items: [
                            {
                                label: "Overview",
                                slug: "publishing"
                            },
                            {
                                label: "Competition Entry",
                                slug: "competition_entry"
                            }
                        ]
                    },
                    {
                        label: "Quest Overview",
                        slug: "overview"
                    },
                    {
                        label: "Reference",
                        items: [
                            {
                                label: "Overview",
                                slug: "reference"
                            },
                            {
                                label: "ASL Requirements",
                                slug: "asl_requirements"
                            },
                            {
                                label: "ASLX File Format",
                                slug: "aslx"
                            }
                        ]
                    },
                    {
                        label: "Release Notes",
                        items: [
                            {
                                label: "Overview",
                                slug: "release_notes"
                            },
                            {
                                label: "Older Versions",
                                slug: "older-versions"
                            },
                            {
                                label: "Quest 5.7",
                                slug: "quest5_7"
                            },
                            {
                                label: "Quest 5.8",
                                slug: "quest5_8"
                            },
                            {
                                label: "Upgrade Notes",
                                slug: "upgrade_notes"
                            }
                        ]
                    },
                    {
                        label: "Script Commands",
                        items: [
                            {
                                autogenerate: {
                                    directory: "scripts"
                                }
                            }
                        ]
                    },
                    {
                        label: "Tutorial",
                        items: [
                            {
                                autogenerate: {
                                    directory: "tutorial"
                                }
                            }
                        ]
                    },
                    {
                        label: "User Experience",
                        items: [
                            {
                                label: "Overview",
                                slug: "user_experience"
                            },
                            {
                                label: "Adding a Dialogue Panel",
                                slug: "ui-dialogue"
                            },
                            {
                                label: "Adding a Dialogue Panel That Assigns Points",
                                slug: "ui-dialogue-points"
                            },
                            {
                                label: "Custom Command Panes",
                                slug: "command_pane"
                            },
                            {
                                label: "Custom Status Pane",
                                slug: "custom_panes"
                            },
                            {
                                label: "Customising the UI - Part 1",
                                slug: "ui-javascript"
                            },
                            {
                                label: "Customising the UI - Part 2",
                                slug: "ui-javascript2"
                            },
                            {
                                label: "Customising the UI - Part 3",
                                slug: "ui-javascript3"
                            },
                            {
                                label: "Customising the UI - Part 3",
                                slug: "ui-javascript4"
                            },
                            {
                                label: "Fonts",
                                slug: "ui-fonts"
                            },
                            {
                                label: "JavaScript to Quest with ASLEvent",
                                slug: "ui-callback"
                            },
                            {
                                label: "Messing with the Location Bar",
                                slug: "ui-location-bar"
                            },
                            {
                                label: "Modifying the Status and Game Panes",
                                slug: "ui-custom"
                            },
                            {
                                label: "The UI Style",
                                slug: "ui-style"
                            },
                            {
                                label: "The UI and Game-play",
                                slug: "ui-game-play"
                            },
                            {
                                label: "Using Display Verbs",
                                slug: "display_verbs"
                            }
                        ]
                    },
                    {
                        label: "XML Elements",
                        items: [
                            {
                                autogenerate: {
                                    directory: "elements"
                                }
                            }
                        ]
                    }
                ],
                },
            ],
        }),
    ],
});
