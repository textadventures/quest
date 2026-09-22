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
                                    "slug": "howto/rooms/objects-and-rooms"
                                },
                                {
                                    "label": "Exits",
                                    "slug": "howto/rooms/exits"
                                },
                                {
                                    "label": "Doors, locks and keys",
                                    "slug": "howto/rooms/doors"
                                },
                                {
                                    "label": "Light and darkness",
                                    "slug": "howto/rooms/light-and-darkness"
                                },
                                {
                                    "label": "The map",
                                    "slug": "howto/rooms/map"
                                },
                                {
                                    "label": "Fast travel",
                                    "slug": "howto/rooms/fast-travel"
                                }
                            ]
                        },
                        {
                            "label": "Objects",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Object and game features",
                                    "slug": "howto/objects/features"
                                },
                                {
                                    "label": "Taking and dropping objects",
                                    "slug": "howto/objects/taking-and-dropping"
                                },
                                {
                                    "label": "Containers and surfaces",
                                    "slug": "howto/objects/containers"
                                },
                                {
                                    "label": "Switchable objects",
                                    "slug": "howto/objects/switchable"
                                },
                                {
                                    "label": "Clothing",
                                    "slug": "howto/objects/clothing"
                                },
                                {
                                    "label": "Food",
                                    "slug": "howto/objects/food"
                                },
                                {
                                    "label": "Liquids",
                                    "slug": "howto/objects/liquids"
                                },
                                {
                                    "label": "Turning one thing into another",
                                    "slug": "howto/objects/transforming"
                                },
                                {
                                    "label": "Pushing objects between rooms",
                                    "slug": "howto/objects/pushing"
                                }
                            ]
                        },
                        {
                            "label": "The player",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Changing the player object",
                                    "slug": "howto/player/player-object"
                                },
                                {
                                    "label": "Character creation",
                                    "slug": "howto/player/character-creation"
                                }
                            ]
                        },
                        {
                            "label": "Characters and conversation",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Talking to characters",
                                    "slug": "howto/characters/talking"
                                },
                                {
                                    "label": "Ask/Tell topics",
                                    "slug": "howto/characters/ask-tell"
                                },
                                {
                                    "label": "Conversations with Pages",
                                    "slug": "howto/characters/pages"
                                },
                                {
                                    "label": "Characters that move",
                                    "slug": "howto/characters/moving"
                                }
                            ]
                        },
                        {
                            "label": "Commands and verbs",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "How commands work",
                                    "slug": "howto/commands/how-commands-work"
                                },
                                {
                                    "label": "Verbs",
                                    "slug": "howto/commands/verbs"
                                },
                                {
                                    "label": "Commands with two objects",
                                    "slug": "howto/commands/two-objects"
                                },
                                {
                                    "label": "Handling multiple items (and all)",
                                    "slug": "howto/commands/multiple-items"
                                },
                                {
                                    "label": "Scope",
                                    "slug": "howto/commands/scope"
                                },
                                {
                                    "label": "Regular expressions in commands",
                                    "slug": "howto/commands/regular-expressions"
                                }
                            ]
                        },
                        {
                            "label": "Time and events",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Time, turns and timers",
                                    "slug": "howto/time/time-turns-and-timers"
                                }
                            ]
                        },
                        {
                            "label": "Score, health and money",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Score, health and money",
                                    "slug": "howto/score/score-health-money"
                                },
                                {
                                    "label": "Setting up a shop",
                                    "slug": "howto/score/shop"
                                }
                            ]
                        },
                        {
                            "label": "RPGs and combat",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Designing an RPG",
                                    "slug": "howto/rpg/designing-an-rpg"
                                },
                                {
                                    "label": "A simple combat system",
                                    "slug": "howto/rpg/combat"
                                },
                                {
                                    "label": "Spells and magic",
                                    "slug": "howto/rpg/spells"
                                }
                            ]
                        },
                        {
                            "label": "Text and messages",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Text processor",
                                    "slug": "howto/text/text-processor"
                                },
                                {
                                    "label": "Changing the game's messages",
                                    "slug": "howto/text/messages"
                                },
                                {
                                    "label": "Using neutral language",
                                    "slug": "howto/text/neutral-language"
                                }
                            ]
                        },
                        {
                            "label": "Pictures, sound and video",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Pictures",
                                    "slug": "howto/media/pictures"
                                },
                                {
                                    "label": "Sound and video",
                                    "slug": "howto/media/sound-and-video"
                                }
                            ]
                        },
                        {
                            "label": "Hints and extras",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Hints",
                                    "slug": "howto/extras/hints"
                                },
                                {
                                    "label": "Journals and player notes",
                                    "slug": "howto/extras/journals"
                                }
                            ]
                        },
                        {
                            "label": "Scripting",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Writing code",
                                    "slug": "howto/scripting/writing-code"
                                },
                                {
                                    "label": "Asking the player",
                                    "slug": "howto/scripting/asking-the-player"
                                },
                                {
                                    "label": "Functions",
                                    "slug": "howto/scripting/functions"
                                },
                                {
                                    "label": "Using lists",
                                    "slug": "howto/scripting/lists"
                                },
                                {
                                    "label": "Using dictionaries",
                                    "slug": "howto/scripting/dictionaries"
                                },
                                {
                                    "label": "Randomness",
                                    "slug": "howto/scripting/randomness"
                                },
                                {
                                    "label": "Maths",
                                    "slug": "howto/scripting/maths"
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
                                    "slug": "howto/scripting/raw-xml"
                                }
                            ]
                        },
                        {
                            "label": "Testing and debugging",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Debugging your game",
                                    "slug": "howto/testing/debugging"
                                },
                                {
                                    "label": "Walkthroughs",
                                    "slug": "howto/testing/walkthroughs"
                                },
                                {
                                    "label": "Transcripts",
                                    "slug": "howto/testing/transcripts"
                                },
                                {
                                    "label": "Troubleshooting",
                                    "slug": "howto/testing/troubleshooting"
                                }
                            ]
                        },
                        {
                            "label": "Worked example: Cloak of Darkness",
                            "slug": "tutorial/cloak-of-darkness"
                        }
                    ]
                },
                {
                    "label": "Understanding Quest Viva",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "When scripts run",
                            "slug": "understanding/when-scripts-run"
                        },
                        {
                            "label": "Values and types",
                            "slug": "understanding/values-and-types"
                        },
                        {
                            "label": "Attributes and types",
                            "slug": "understanding/attributes-and-types"
                        },
                        {
                            "label": "Undo",
                            "slug": "understanding/undo"
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
                                    "slug": "customise/player-interface"
                                },
                                {
                                    "label": "Look and feel",
                                    "slug": "customise/look-and-feel"
                                },
                                {
                                    "label": "Object verbs",
                                    "slug": "customise/object-verbs"
                                },
                                {
                                    "label": "Panes",
                                    "slug": "customise/panes"
                                },
                                {
                                    "label": "Styling the player with CSS",
                                    "slug": "customise/css"
                                },
                                {
                                    "label": "Calling the game from JavaScript",
                                    "slug": "customise/javascript"
                                }
                            ]
                        },
                        {
                            "label": "Types and libraries",
                            "collapsed": true,
                            "items": [
                                {
                                    "label": "Creating and using object types",
                                    "slug": "customise/object-types"
                                },
                                {
                                    "label": "Using and creating libraries",
                                    "slug": "customise/libraries"
                                },
                                {
                                    "label": "Adding editor tabs and script commands",
                                    "slug": "customise/editor-tabs"
                                },
                                {
                                    "label": "Overriding Core library functions",
                                    "slug": "customise/overriding"
                                },
                                {
                                    "label": "Using delegates",
                                    "slug": "customise/delegates"
                                }
                            ]
                        },
                        {
                            "label": "Writing a game in another language",
                            "slug": "customise/other-languages"
                        }
                    ]
                },
                {
                    "label": "Publishing",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Publishing your game",
                            "slug": "publishing"
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
                            "slug": "publishing/updating-a-released-game"
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
                            "slug": "reference/script-commands"
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
                                    "slug": "reference/attributes"
                                },
                                {
                                    "label": "Important attributes",
                                    "slug": "reference/attributes/important"
                                },
                                {
                                    "label": "Status attributes",
                                    "slug": "reference/attributes/status"
                                },
                                {
                                    "label": "Change script",
                                    "slug": "reference/attributes/change-scripts"
                                },
                                {
                                    "label": "Attribute Types",
                                    "slug": "reference/attributes/types"
                                },
                                {
                                    "label": "Attribute reference",
                                    "slug": "reference/attributes/all"
                                }
                            ]
                        },
                        {
                            "label": "XML elements",
                            "slug": "reference/elements"
                        },
                        {
                            "label": "JS functions",
                            "slug": "reference/js"
                        },
                        {
                            "label": "Hard-coded functions and library functions",
                            "slug": "reference/functions/hardcoded"
                        },
                        {
                            "label": "ASLX file format",
                            "slug": "reference/aslx"
                        },
                        {
                            "label": "ASL requirements",
                            "slug": "reference/asl-requirements"
                        }
                    ]
                },
                {
                    "label": "Contribute",
                    "collapsed": true,
                    "items": [
                        {
                            "label": "Overview",
                            "slug": "contribute"
                        },
                        {
                            "label": "Building from source",
                            "slug": "contribute/building-from-source"
                        }
                    ]
                }
            ],
        }),
    ],
});
