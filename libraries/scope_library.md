---
layout: index
title: ScopeLibrary
---

General Quest works out what objects are around, and will pass the right object to your command for you to dealk with. Just occasionally, though, you want to apply a command to an object that is not in the inventory or the current room. For example, the player might be in a shop, and you dfind it convenient to keep the available stock in another room, so the player can drop and take items she already owns, but not help herself to the merchandise.

[Download]({{site.baseurl}}/files/ScopeLib.aslx)

Here is an example of it in use for just such an application:

  <command name="buystuff">
    <pattern>buy #text#;purchase #text#</pattern>
    <script><![CDATA[
      if (not HasObject(game.pov.parent, "shopstock")) {
        msg ("Where do you think you are, a shop or something?")
      }
      else {
        ProcessScopeCommand ("buy", GetDirectChildren (game.pov.parent.shopstock), text, "You cannot see that for sale here", "That's not something you can buy")
      }
    ]]></script>
  </command>
  
The command pattern has to be set up to grab the text, not an object.

The script checks this is a shop (by checking if it has a shopstock object in it), and if it does called the ProcessScopeCommand from the library. This function takes five parametersL

script: A string that gives the name of the script to call on the object. in the example above, if an item is found, then its "buy" script will be invoked.
  
scope: An object list from which to get objects; this is what it will search through to find a match.

text: The text variable from command, it will try to match the objects against this name.

noobject: A string to display if no object is found

noscript: A string to display if the object is found, but it does not have the named script

      
If you want to use this for setting up a shop, most of the work is already done in the shopping library        