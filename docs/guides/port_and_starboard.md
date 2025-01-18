---
layout: index
title: Port and starboard
---

So you have this plan for a game, but it is set on a ship or a starship, and north and south do not make any sense. The standard for marine ships is to use forward, starboard, aft and port, so why not implement that for your game? This is actually pretty easy to do using Quest's built-in language support.

One limitation of the shipwise directions is that you lose four directions. While "northeast" is well established, I think people will find "forwardport" rather odd. Remember that when adding exits to your game!

So what do we need to do? All that is needed is to change about a dozen templates. It is very import that these changes are done in the correct place in the code, and unfortunately the GUI fails to do that (as of version 5.2 to 5.4), so you really need to do this in the code.

In code view, at the very top, it will look like this:

      <!--Saved by Quest 5.2.4515.34846-->
      <asl version="520">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />

All the default templates are in English.aslx, and we want to override them, so our templates have to go after that third line. However, templates are used as the files are loaded (but note that dynamic templates are different; they are implemented during game play, so can appear in your code anywhere after the third line), so they have to be before the core library is called in the fourth line.

So let us add some templates.

This set changes the directions Quest uses in description when it says "You can go"

      <template name="CompassN">forward</template>
      <template name="CompassW">port</template>
      <template name="CompassE">starboard</template>
      <template name="CompassS">aft</template>

Also need to change the abbreviated versions.

      <template name="CompassNShort">f</template>
      <template name="CompassWShort">p</template>
      <template name="CompassEShort">s</template>
      <template name="CompassSShort">a</template>

Quest uses these next two for pattern matching the player input. The specific direction is matched against the templates above.

      <template templatetype="command" name="go"><![CDATA[^go to (?<exit>.*)$|^go (?<exit>.*)$|^(?<exit>forward|port|starboard|aft|f|p|a|s|in|out|up|down|o|u|d)$]]></template>
      <template templatetype="command" name="lookdir"><![CDATA[^look (?<exit>forward|port|starboard|aft|f|p|a|s|in|out|up|down|o|u|d)$]]></template>

You also need to change the help command. One way is to override the DefaultHelp template (which I am not doing here as it seriously messes with Wiki's formatting), but you might prefer to create your own help command and do it there. It may be useful to spell out the directions as people are somewhat less familiar with ship directions, and to point out they only have four directions instead of eight.

So what does it look like now? The top of your code should now look like this (this is without the help template):

      <!--Saved by Quest 5.2.4515.34846-->
      <asl version="520">
        <include ref="English.aslx" />
        <template name="CompassN">forward</template>
        <template name="CompassW">port</template>
        <template name="CompassE">starboard</template>
        <template name="CompassS">aft</template>
        <template name="CompassNShort">f</template>
        <template name="CompassWShort">p</template>
        <template name="CompassEShort">s</template>
        <template name="CompassSShort">a</template>
        <template name="go"><![CDATA[^go to (?<exit>.*)$|^go (?<exit>.*)$|^(?<exit>forward|port|starboard|aft|f|p|a|s|in|out|up|down|o|u|d)$]]></template>
        <template name="lookdir"><![CDATA[^look (?<exit>forward|port|starboard|aft|f|p|a|s|in|out|up|down|o|u|d)$]]></template>
        <include ref="Core.aslx" />

Wouldn't this be easier in a library?

Well, go on then.

[ShipwiseLib.aslx](https://raw.githubusercontent.com/ThePix/quest/refs/heads/master/ShipwiseLib.aslx)

Save this file to your game's folder, and modify the code at the start of the file to this:

      <!--Saved by Quest 5.2.4515.34846-->
      <asl version="520">
        <include ref="English.aslx" />
        <include ref="ShipwiseLib.aslx" />
        <include ref="Core.aslx" />

One last note. After adding new templates, or a library with templates, you need to save the game, quit Quest, then open it up again to get the templates loaded up properly.

* * * * *

It might be a good idea to implement a command so your game responds to NORTH, EAST, etc., explaining the system. Here is an example of such a command, but you will probably want to tailor it to your game and style.

      <command name="compass_directions">
        <pattern>w;e;s;n;se;ne;sw;ne;west;south;east;north;northeast;southeast;northwest;southwest</pattern>
        <script>
          msg ("This story is set on a ship, and the standard compass directions make no sense in that context. Instead,directions are referenced shipwise. Looking towards the front of the ship, ahead of you is \"forward\" (where north would be on the compass rose to the right) and behind you is \"aft\". To the left is \"port\", and to the right is \"starboard\".")
          msg ("If you get confused about port and starboard, remember that \"port\" and \"left\" are both four letters long. Or remember that starboard is a corruption of stearboard, an early form of rudder. Most people are right handed so the stearboard was always on the right,")
        </script>
      </command>

Note that this will not work if included in the library above. I suspect it has to be after the core library is loaded. Put it in the main file or a library that appears after Core.aslx in the list.
