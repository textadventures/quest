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
                                    "label": "Mapping with Trizbort",
                                    "slug": "howto/world/trizbort"
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
                                    "label": "Containers",
                                    "slug": "howto/world/containers"
                                },
                                {
                                    "label": "Parts of an object",
                                    "slug": "other-guides/implementing-components-of-an-object"
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
                                    "label": "Clothing with several states",
                                    "slug": "howto/world/multistate-clothing"
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
                                },
                                {
                                    "label": "Starting inventory",
                                    "slug": "other-guides/starting-inventory"
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
                                    "label": "Followers",
                                    "slug": "howto/npcs/follower"
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
                            "label": "Commands and verbs",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "How commands work",
                                    "slug": "howto/commands/commands"
                                },
                                {
                                    "label": "Commands specific to a room",
                                    "slug": "howto/commands/commands-for-room"
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
                                    "label": "Scope for commands",
                                    "slug": "howto/commands/advanced-scope"
                                },
                                {
                                    "label": "Regular expressions in commands",
                                    "slug": "howto/commands/pattern-matching"
                                }
                            ]
                        },
                        {
                            "label": "Time, turns and timers",
                            "slug": "howto/scripting/using-turnscripts"
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
                                    "label": "Keeping score",
                                    "slug": "howto/tasks/keeping-score"
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
                            "label": "Text and messages",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/world/text-processor"
                                },
                                {
                                    "label": "Links",
                                    "slug": "other-guides/hyperlinks"
                                },
                                {
                                    "label": "Changing the game's messages",
                                    "slug": "howto/world/changing-templates"
                                },
                                {
                                    "label": "Varying default responses",
                                    "slug": "other-guides/random-default-answers"
                                },
                                {
                                    "label": "Custom directions",
                                    "slug": "other-guides/port-and-starboard"
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
                                    "label": "Overview",
                                    "slug": "howto/multimedia/multimedia"
                                },
                                {
                                    "label": "Pictures",
                                    "slug": "howto/multimedia/images"
                                },
                                {
                                    "label": "Creating images on the fly",
                                    "slug": "howto/multimedia/images-on-the-fly"
                                },
                                {
                                    "label": "Sound",
                                    "slug": "howto/multimedia/adding-sounds"
                                },
                                {
                                    "label": "Video",
                                    "slug": "howto/multimedia/adding-videos"
                                }
                            ]
                        },
                        {
                            "label": "Hints and extras",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "A hint system",
                                    "slug": "other-guides/a-hint-system"
                                },
                                {
                                    "label": "InvisiClues-style hints",
                                    "slug": "other-guides/invisiclues"
                                },
                                {
                                    "label": "Keeping a journal",
                                    "slug": "howto/tasks/keeping-a-journal"
                                },
                                {
                                    "label": "Player memory or wiki",
                                    "slug": "howto/tasks/memory-or-wiki"
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
                                    "label": "Asking the player",
                                    "slug": "howto/scripting/asking-the-player"
                                },
                                {
                                    "label": "Using functions",
                                    "slug": "howto/tasks/about-functions"
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
                                    "label": "Randomness",
                                    "slug": "howto/tasks/random"
                                },
                                {
                                    "label": "Maths",
                                    "slug": "howto/tasks/use-maths-functionality"
                                },
                                {
                                    "label": "Decimal numbers",
                                    "slug": "howto/scripting/using-doubles"
                                },
                                {
                                    "label": "Clones",
                                    "slug": "howto/scripting/clones"
                                },
                                {
                                    "label": "Blocks and scripts",
                                    "slug": "howto/scripting/blocks-and-scripts"
                                },
                                {
                                    "label": "Advanced game scripts",
                                    "slug": "howto/scripting/advanced-game-scripts"
                                },
                                {
                                    "label": "Copying and pasting code",
                                    "slug": "howto/scripting/copy-and-paste-code"
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
                                    "label": "Unit testing",
                                    "slug": "howto/scripting/unit-testing"
                                },
                                {
                                    "label": "Common problems",
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
                            "label": "Scope: what the player can see and reach",
                            "slug": "howto/scripting/scopes"
                        },
                        {
                            "label": "Expressions",
                            "slug": "howto/scripting/expressions"
                        },
                        {
                            "label": "Null and unset values",
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
                                    "label": "How players interact",
                                    "slug": "howto/ux/ui-game-play"
                                },
                                {
                                    "label": "Look and feel",
                                    "slug": "howto/ux/ui-style"
                                },
                                {
                                    "label": "Fonts",
                                    "slug": "howto/ux/ui-fonts"
                                },
                                {
                                    "label": "Object verb menus",
                                    "slug": "howto/ux/display-verbs"
                                },
                                {
                                    "label": "Custom command panes",
                                    "slug": "howto/ux/command-pane"
                                },
                                {
                                    "label": "Custom status pane",
                                    "slug": "howto/ux/custom-panes"
                                },
                                {
                                    "label": "Modifying the status and game panes",
                                    "slug": "howto/ux/ui-custom"
                                },
                                {
                                    "label": "The location bar",
                                    "slug": "howto/ux/ui-location-bar"
                                },
                                {
                                    "label": "Custom CSS and HTML",
                                    "slug": "howto/ux/customising-the-ui"
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
