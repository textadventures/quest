// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import { questGrammar } from "./src/quest-grammar.mjs";
import { redirects } from "./src/redirects.mjs";

// https://astro.build/config
export default defineConfig({
    site: "https://questviva.com",
    redirects,
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
                    "collapsed": false,
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
                        }
                    ]
                },
                {
                    "label": "Tutorial",
                    "collapsed": false,
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
                {
                    "label": "Build Your Game",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "howto"
                        },
                        {
                            "label": "Rooms and exits",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Objects and rooms",
                                    "slug": "howto/world/objects-and-rooms"
                                },
                                {
                                    "label": "Exits",
                                    "slug": "howto/world/exits"
                                },
                                {
                                    "label": "Doors, locks and keys",
                                    "slug": "howto/world/doors"
                                },
                                {
                                    "label": "Light and darkness",
                                    "slug": "howto/world/handling-light-and-dark"
                                },
                                {
                                    "label": "The map",
                                    "slug": "howto/tasks/showing-a-map"
                                },
                                {
                                    "label": "Fast travel",
                                    "slug": "howto/tasks/transit-system"
                                }
                            ]
                        },
                        {
                            "label": "Objects",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Object and game features",
                                    "slug": "howto/world/features"
                                },
                                {
                                    "label": "Taking and dropping objects",
                                    "slug": "howto/world/taking-and-dropping"
                                },
                                {
                                    "label": "Containers and surfaces",
                                    "slug": "howto/world/containers"
                                },
                                {
                                    "label": "Switchable objects",
                                    "slug": "howto/world/switchable"
                                },
                                {
                                    "label": "Clothing",
                                    "slug": "howto/world/wearables"
                                },
                                {
                                    "label": "Food",
                                    "slug": "howto/world/edible"
                                },
                                {
                                    "label": "Liquids",
                                    "slug": "howto/tasks/handling-water"
                                },
                                {
                                    "label": "Turning one thing into another",
                                    "slug": "howto/tasks/convert"
                                },
                                {
                                    "label": "Pushing objects between rooms",
                                    "slug": "howto/tasks/move-object"
                                }
                            ]
                        },
                        {
                            "label": "The player",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Changing the player object",
                                    "slug": "howto/tasks/changing-the-player-object"
                                },
                                {
                                    "label": "Character creation",
                                    "slug": "howto/rpg/character-creation"
                                }
                            ]
                        },
                        {
                            "label": "Characters and conversation",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Talking to characters",
                                    "slug": "howto/npcs/conversations"
                                },
                                {
                                    "label": "Ask/Tell topics",
                                    "slug": "howto/npcs/ask-about"
                                },
                                {
                                    "label": "Conversations with Pages",
                                    "slug": "howto/npcs/dialogue-pages"
                                },
                                {
                                    "label": "Characters that move",
                                    "slug": "howto/npcs/npcs-that-move"
                                }
                            ]
                        },
                        {
                            "label": "Commands and verbs",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "How commands work",
                                    "slug": "howto/commands/commands"
                                },
                                {
                                    "label": "Verbs",
                                    "slug": "howto/commands/using-verbs"
                                },
                                {
                                    "label": "Commands with two objects",
                                    "slug": "howto/commands/complex-commands"
                                },
                                {
                                    "label": "Handling multiple items (and all)",
                                    "slug": "howto/commands/handling-multiple"
                                },
                                {
                                    "label": "Scope",
                                    "slug": "howto/commands/advanced-scope"
                                },
                                {
                                    "label": "Regular expressions in commands",
                                    "slug": "howto/commands/pattern-matching"
                                }
                            ]
                        },
                        {
                            "label": "Time and events",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Time, turns and timers",
                                    "slug": "howto/scripting/using-turnscripts"
                                }
                            ]
                        },
                        {
                            "label": "Score, health and money",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Score, health and money",
                                    "slug": "howto/world/score-health-money"
                                },
                                {
                                    "label": "Setting up a shop",
                                    "slug": "howto/tasks/shop"
                                }
                            ]
                        },
                        {
                            "label": "RPGs and combat",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Designing an RPG",
                                    "slug": "howto/rpg/rpg-intro"
                                },
                                {
                                    "label": "A simple combat system",
                                    "slug": "howto/rpg/zombie-apocalypse-1"
                                },
                                {
                                    "label": "Spells and magic",
                                    "slug": "howto/rpg/zombie-apocalypse-spells"
                                }
                            ]
                        },
                        {
                            "label": "Text and messages",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/world/text-processor"
                                },
                                {
                                    "label": "Changing the game's messages",
                                    "slug": "howto/world/changing-templates"
                                },
                                {
                                    "label": "Using neutral language",
                                    "slug": "howto/tasks/neutral-language"
                                }
                            ]
                        },
                        {
                            "label": "Pictures, sound and video",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Pictures",
                                    "slug": "howto/multimedia/images"
                                },
                                {
                                    "label": "Sound and video",
                                    "slug": "howto/multimedia/adding-sounds"
                                }
                            ]
                        },
                        {
                            "label": "Hints and extras",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Hints",
                                    "slug": "other-guides/a-hint-system"
                                },
                                {
                                    "label": "Journals and player notes",
                                    "slug": "howto/tasks/keeping-a-journal"
                                }
                            ]
                        },
                        {
                            "label": "Scripting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Writing code",
                                    "slug": "howto/scripting/introtocoding"
                                },
                                {
                                    "label": "Asking the player",
                                    "slug": "howto/scripting/asking-the-player"
                                },
                                {
                                    "label": "Functions",
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
                                    "label": "Randomness",
                                    "slug": "howto/tasks/random"
                                },
                                {
                                    "label": "Maths",
                                    "slug": "howto/tasks/use-maths-functionality"
                                },
                                {
                                    "label": "Clones",
                                    "slug": "howto/scripting/clones"
                                },
                                {
                                    "label": "Advanced game scripts",
                                    "slug": "howto/scripting/advanced-game-scripts"
                                },
                                {
                                    "label": "Editing the raw XML",
                                    "slug": "howto/scripting/codeview"
                                }
                            ]
                        },
                        {
                            "label": "Testing and debugging",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Debugging your game",
                                    "slug": "howto/scripting/debugging-your-game"
                                },
                                {
                                    "label": "Walkthroughs",
                                    "slug": "howto/scripting/using-walkthroughs"
                                },
                                {
                                    "label": "Transcripts",
                                    "slug": "howto/world/transcript"
                                },
                                {
                                    "label": "Troubleshooting",
                                    "slug": "howto/tasks/problems"
                                }
                            ]
                        },
                        {
                            "label": "Worked example: Cloak of Darkness",
                            "slug": "cloak-of-darkness"
                        }
                    ]
                },
                {
                    "label": "Understanding Quest Viva",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "When scripts run",
                            "slug": "howto/scripting/when-scripts-run"
                        },
                        {
                            "label": "Values and types",
                            "slug": "howto/scripting/null"
                        },
                        {
                            "label": "Object types",
                            "slug": "advanced-topics/about-types"
                        },
                        {
                            "label": "Undo",
                            "slug": "advanced-topics/undo-support"
                        }
                    ]
                },
                {
                    "label": "Customise and Extend",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "The player interface",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "The player interface",
                                    "slug": "howto/ux/ui-game-play"
                                },
                                {
                                    "label": "Look and feel",
                                    "slug": "howto/ux/ui-style"
                                },
                                {
                                    "label": "Object verbs",
                                    "slug": "howto/ux/display-verbs"
                                },
                                {
                                    "label": "Panes",
                                    "slug": "howto/ux/custom-panes"
                                },
                                {
                                    "label": "Styling the player with CSS",
                                    "slug": "howto/ux/customising-the-ui"
                                },
                                {
                                    "label": "Calling the game from JavaScript",
                                    "slug": "howto/ux/ui-callback"
                                }
                            ]
                        },
                        {
                            "label": "Types and libraries",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Creating your own types",
                                    "slug": "advanced-topics/using-inherited-types"
                                },
                                {
                                    "label": "Using and creating libraries",
                                    "slug": "advanced-topics/using-libraries"
                                },
                                {
                                    "label": "Editor tabs for types",
                                    "slug": "advanced-topics/tabs-for-types"
                                },
                                {
                                    "label": "Editor user interface elements",
                                    "slug": "advanced-topics/editor-user-interface-elements"
                                },
                                {
                                    "label": "Overriding built-in functions",
                                    "slug": "advanced-topics/overriding"
                                },
                                {
                                    "label": "Using delegates",
                                    "slug": "advanced-topics/using-delegates"
                                }
                            ]
                        },
                        {
                            "label": "Translating Quest Viva",
                            "slug": "advanced-topics/translation"
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
                            "label": "Your game's details",
                            "slug": "publishing/game-details"
                        },
                        {
                            "label": "Hosting your game",
                            "slug": "publishing/hosting"
                        },
                        {
                            "label": "Competition entry",
                            "slug": "publishing/competition-entry"
                        },
                        {
                            "label": "Updating a released game",
                            "slug": "howto/world/about-save"
                        },
                        {
                            "label": "WebPlayer",
                            "slug": "publishing/webplayer"
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
                                    "slug": "reference/functions"
                                },
                                {
                                    "label": "Functions for attributes",
                                    "slug": "reference/functions/attributes"
                                },
                                {
                                    "label": "Functions for variables",
                                    "slug": "reference/functions/variables"
                                },
                                {
                                    "label": "Functions for objects and exits",
                                    "slug": "reference/functions/objects"
                                },
                                {
                                    "label": "Timers and turnscripts",
                                    "slug": "reference/functions/timers-turnscripts"
                                },
                                {
                                    "label": "User interface functions",
                                    "slug": "reference/functions/user-interface"
                                },
                                {
                                    "label": "List functions",
                                    "slug": "reference/functions/list"
                                },
                                {
                                    "label": "Scope functions",
                                    "slug": "reference/functions/scope"
                                },
                                {
                                    "label": "Dictionary functions",
                                    "slug": "reference/functions/dictionary"
                                },
                                {
                                    "label": "String functions",
                                    "slug": "reference/functions/string"
                                },
                                {
                                    "label": "Clothing functions",
                                    "slug": "reference/functions/clothing"
                                },
                                {
                                    "label": "Randomising functions",
                                    "slug": "reference/functions/random"
                                },
                                {
                                    "label": "General functions",
                                    "slug": "reference/functions/general"
                                },
                                {
                                    "label": "Core.aslx functions",
                                    "slug": "reference/functions/core"
                                },
                                {
                                    "label": "Internal Core.aslx functions",
                                    "slug": "reference/functions/internal-core"
                                },
                                {
                                    "label": "Mathematical functions",
                                    "slug": "reference/functions/maths"
                                },
                                {
                                    "label": "Gamebook functions",
                                    "slug": "reference/functions/gamebook"
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
                            "slug": "reference/functions/hardcoded"
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
                    "label": "Contribute",
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
