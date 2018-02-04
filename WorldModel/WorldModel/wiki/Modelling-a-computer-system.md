_Note:_ This is only applicable to the desktop, as you have to be able to give exits arbitrary aliases.

_Note:_ This system could readily be used for a conversation system or a character creation process. Or an emtire game. Anything where the player is selective from successive menus.

This is a way to model a computer system that gives the user a set number of options in a menu. When the player makes a selection, she is presented with a new screen, and a new set of options. What we have, then, is a set of nodes, each of which has a set of options leading to other nodes...

So basically the same as a Quest, except we have nodes rather than room. So why not use rooms and disguise them as nodes? What we will do is set up a number of rooms, with exits between them, and we will track where the player is using the "context" attribute.

### The `ConsoleOptions` function

The basis of the system is a function called "ConsoleOptions"; it has no return type or parameters. Paste in this code:

```
if (HasScript(game.pov.context, "description")) {
  do (game.pov.context, "description")
}
else {
  msg (game.pov.context.description)
}
opts = ObjectListToStringList(ScopeUnlockedExitsForRoom(game.pov.context), "alias")
ShowMenu ("Please select:", opts + "Quit", false) {
  if (result = "Quit") {
    msg ("Logging off")
    SetFontName ("Georgia, serif")
  }
  else {
    foreach (exit, ScopeUnlockedExitsForRoom(game.pov.context)) {
      if (exit.alias = result) {
        game.pov.context = exit.to
        ConsoleOptions
      }
    }
  }
}
```

The first six lines display the current node information. Just like a room, this uses the "description" attribute, which can be either a script of a string.

Now we need to set up the options. The seventh line gets the aliases for all the exits from this room, and the eighth line displays them in a menu (adding "Quit" to the list).

If the result is "Quit", we give a message, reset the font, and stop (you might want to modify this so any changes to the user interface you have in your game are reverted; you might want to clear the screen too). Otherwise, we look through the exits to match the alias, set the "context" attribute to the exit's destination, and call this function again for the new room.

### Nodes

Nodes are just rooms. Give them a description as normal, either text or script. You can have as many as you like, and it is easy to add more later as required.

### Exits

Each node will need some exits connecting it to other nodes. These are not directional exits, so are not created in quite the normal way. If you go to the _Exits_ tab. under the compass rose there is a list of exits. Click on "Add" to create a new, non-directional exit. You will then find yourself on the exit's page. In the "To" bit, select the node this exit will connect to, and in the "Alias" bit type the text that should be displayed in the menu option.

### Logging on

We need a way to kick-start the system. For example, this could be the "Use" script on a computer object. It needs to do four things: tell the player what is happening, set up the user interface to make clear this is on the screen (I am just setting the font), set the "context" attribute of the player to the first node (if my case "comp_toplevel", and call `ConsoleOptions`.

```
msg ("You sit at the computer, and turn it on.")
msg(" ")
SetFontName ("'Lucida Console', Monaco, monospace")
player.context = comp_toplevel
ConsoleOptions
```

### Locked exits

Note that you can lock exits, and so make them inaccessible (they will not appear in the list of options). Once the player has high security access, for example, new options can be added by unlocking the exits.


### Messages

You could set up one node/room to show messages. Each message is an object in the room, and you could move new objects into the room or set invisible objects to visible as the game progresses, simulating receiving new messages. Here is some simple code, which will show new messages in bold (the content of a message is the object description).

```
msg ("Messages found:")
flag = false
foreach (o, FilterByAttribute(GetDirectChildren(this), "visible", true)) {
  if (GetBoolean(o, "hasbeenread")) {
    msg (o.look)
  }
  else {
    msg ("{b:" + o.look + "}")
    o.hasbeenread = true
  }
  flag = true
}
if (not flag) {
  msg ("You have no messages")
}
```

### Adding numerous entries

If you have a lot of nodes to add, ypou might want a short cut. Let us suppose a section of the system is an encyclopedia, and you have twenty entries to make... that would be forty exits. We can get Quest to do some of the work.

Create the entries as objects, rather than rooms, then add them to the system with a function. Objects use the "look" attribute, rather than "description" and we will use that plus the alias to create the description as well as the two exits.

In fact, we will use two functions, one for adding encyclopedia entries, linked to a node called console_encyclo, one to create mission entries, linked to a node called console_missions. Note that they also differ in how the description is created, so the end result is a set of encyclopedia entries in one format and a set of missions in another.

```
  <function name="AddEntry" parameters="entry"><![CDATA[
    create exit (entry.alias, console_encyclo, entry, "defaultexit")
    create exit ("Return", entry, console_encyclo, "defaultexit")
    entry.description = "{b:" + entry.alias + "}<br/><br/>" + entry.look
  ]]></function>

  <function name="AddMission" parameters="entry"><![CDATA[
    create exit (entry.alias, console_missions, entry, "defaultexit")
    create exit ("Return", entry, console_missions, "defaultexit")
    entry.description = "Communication from Star Base 142 (Sector 7 Iota):<br/><br/>{i:Mission: " + entry.alias + "<br/><br/>" + entry.look + "}"
  ]]></function>
```


### Locking exits

Here is a function that will lock all the exits to the given node, effectively removing it from your game.

```
  <function name="RemoveNode" parameters="node"><![CDATA[
    foreach (ext, FilterByAttribute(AllExits(), "to", node)) {
      ext.locked = true
    }
  ]]></function>
```

You could easily add an `AddNode` function that unlocks all the exits to a node too.