Following a recent discussion on the Quest forum, I have been thinking about how a group could collaborate to create a game. What I am thinking of here is a vertical division of labour, with each person responsible for one zone, and the player able to travel to each zone from a central hub.

With a view to how that might work in practice, I created a template game and library. The way it would work is each person creates their own game, based on the template. At the same time the library can be developed, and updated versions of the library sent to authors when required. At the end of the process, each zone is converted to a library, and added to the master (which would potentially be a blank template).

The trick is to not have any reference in each zone to anything outside the zone. At the start of the game, each zone's gateway registers with the hub, so it is listed as a destination.

[Template](https://github.com/ThePix/quest/blob/master/group_project.aslx) | [Library](https://github.com/ThePix/quest/blob/master/group_project_library.aslx)

Instructions
------------

### Step 1: Setting Up The Project Template

The first step is for a member of the project to download the template and set up the various options on the game object. It does not matter if this is not perfect, but it will help authors get  feel for what it will look like. Once this is done, the template for the project should be uploaded to somewhere that all members can access (Github is a good option). Once the style is agreed, the template becomes set in stone.


### Step 2: Setting up individual templates

Each author should now download both the template and the library. The template has the game, the hub and the gateway objects. The game and the hub are off-limits - you can makes changes to them or add things to them, but when your game is added to the project, it will all get deleted.

The gateway object is the start of your world. The first thing you must do is rename it.

Authors _must_ prefix their identifier to the start of all rooms, objects, exits (if you name them), commands (if you name them) and functions to ensure there are no conflicts, and you need to start with the gateway. If your initials are "px", you could call it "px_gateway". You should also give it an alias.

Now go to the _Gateway_ tab. Here you can give your dimension a name and a description, as well as a colour for the link when the player is selecting from the hub.

There is also a start script. This will fire when the game starts, and is the alternative to the start script on the game object (remember; anything you add to the game object will get deleted). There is already one line there; you must not delete or change it!

### Step 3: Create Your Dimension

Now your copy of the template is set up, you can create your game. Add rooms and items as normal (but remember the prefix), linking exits to and from the gateway.

When you test your game, you will start in the hub. Use the TRAVEL command to go to your dimension.

### Step 4: Update The Library

The library can be updated at the same time as people create their dimensions. There are broadly three reasons to update:

- Add general commands and functions

- Add other libraries (libraries can be invoked from other libraries and this is the best way to ensure everyone is using the same ones.

- Change the hub

Functions in the library determine the hub room description and the player description, so these can be updated here as ideas develop, rather than in the template. The travel command can also be modified, so you could change the command pattern to include CATCH BUSES or CAST GATEWAY or whatever. The text displayed while traveling is an attribute of the command.

Once a dimension is completed, a copy should be sent to the project lead.

### Step 5: The Masterfile

The masterfile is the template file with no gateway, kept by the project lead. The final details of the game setting can be set here.

The Boolean attribute "combined" of the game object should be set to true.

### Step 6: Combine

The project lead should convert each game he or she receives in to a library.

Go into full code view (F9). Delete the top five lines, plus the game object and the hub object (these last two are best done by collapsing them by clicking on the minus in a box to he left). Also delete the very last line.

Then add this to the top:

```
<library>
```

And this to the end:

```
</library>
```

Save the file, and then add it to the master file as a library.


Notes
-----

### travel to and from

From the hub, the player can only travel to the gateway location.

The player can only leave a dimension if she is in a room with a Boolean attribute "gateway" set to true. This is the case for the gateway room, but you can set it to false, and have it set on another room, or as may as you like.

You can also set `player.cantravelfromanywhere` to true in the "enter" script for your gateway to allow the player to return to the hub from any room in your dimension.

### Items in other dimension

You can set items (or whatever) to be set up in a different dimension. You should discuss with the other author how this will be done, and it is ultimately up to the other author to allow.

Use the `StartMeElsewhere` function to do this, because your object will need to be in your game, but the room it is to be in will not until the game is combined. The function will move the object to the room if it exists, or to the hub otherwise (so you can test your game).

The function is used like this:
```
StartMeElsewhere (object obj, string roomname)
```

### Is this the full game?

Your scripts can check the `game.combined` attribute to see if it is the combined game, or just your own.

### Completion?

It may be useful to track if a dimension has been completed. This is tracked by its gateway location, but there are functions that do the work.

Use `DimensionComplete(object gateway)` to flag your dimension as completed.

Use `IsDimensionCompleted(object gateway)` to discover if your dimension as completed.

Use `AreAllDimensionsCompleted()` to discover if all the dimensions have been completed. This could be used in the hub to test if the game is over.


Issues with Contributors
------------------------

The nature of a group project is that people will drop out and others will join. As long as the members have access to the most recent library and template, the project can continue. If everyone except you drops out, just release the game you have been working on.

New members can be added at any time. Even after the game is uploaded, a new dimension can be added with no need to update anything except add another library to the master file (note that the new dimension will not get added to saved games).


Further Considerations
----------------------
The various setting for the game object would be best decided before hand. Some issues to decide:

- Input by command line or hyperlinks or panes?
- How will conversations be handled?
- Single or double quotes?
- Will there be an economy, if so what Currency?
- Who is the player?
- Combat? Magic? Tech level?
- Will the player development (level up)?
- What player attributes will be used?
- Is it "color" or "colour"?

Some random ideas for a hub:

- a magic portal
- stargate command
- spaceship
- spaceport
- rocket ship (for retro sci-fi)
- airship (steampunk, perhaps between floating worlds)
- a university (Indian Jones style pulp)
- bus/train/monorail station
- purgatory (player is a spirit that possesses people in different worlds)
- a blue phonebox (adventures in time and space)