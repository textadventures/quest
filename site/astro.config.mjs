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
                                    "label": "Objects and rooms",
                                    "slug": "howto/world/objects-and-rooms"
                                },
                                {
                                    "label": "Features",
                                    "slug": "howto/world/features"
                                },
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
                                    "label": "Items that can be eaten",
                                    "slug": "howto/world/edible"
                                },
                                {
                                    "label": "Taking and dropping objects",
                                    "slug": "howto/world/taking-and-dropping"
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
                                    "label": "When scripts run",
                                    "slug": "howto/scripting/when-scripts-run"
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
