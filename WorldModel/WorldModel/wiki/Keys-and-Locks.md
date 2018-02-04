## Locked Containers

Quest makes locking a container very simple, so we will start there.

The first step is to create two objects, the container and the key. Let us say we have a chest and a silver key.

For the chest object, go to the Features tab, and tick Container, then go to the Container tab, and select Closed Container from the drop down list. Most the rest of the page you can skip, we just need to jump down to the Locking section at the bottom, and set the Lock type to Lockable. Set the number of keys to 1, and the Key to be the silver key (if on-line, you may need to go to another object after setting the number of keys, and then come back to the chest, as it does not seem to update the page automatically).

That is it, nothing needs be done with the key, except to check the box that allows the player to pick it up.


## Locked Exits

To lock an exit, just tick its Locked checkbox. To have it unlock with a key... is a bit more tricky.


## Script on the Exit

The simplest is to have the exit test if the player has the key, and allow her to pass if she does. You should not have the exit ticked as Locked, as everything happens in the script, but instead tick the "Run a script..." check box. A box for a script will appear.

Paste in this script (which uses a key called "blue key"):
```
  if (blue key.parent = player) {
    player.parent = this.to
  }
  else {
    msg ("That way is locked.")
  }
```
This technique is easiest, but does not support the UNLOCK command, and most players when they find a key will expect to use UNLOCK.


## Implementing UNLOCK

If you want to implement UNLOCK, then you need an object that can be unlocked, so we need a door object.

We also need the exit to be set as Locked, and to have a name (it needs a name so we can reference it later). For the example, let us say "exit to lounge" (if I give an exit a name, I like to start it "exit" so I can tell what it is in a long list of objects). It is counter-intuitive, but the locked message should be changed to say the door is closed, not the exit is locked. The player cannot use the exit because the door is closed, and cannot open the door because it is locked.

We are going to need an attribute that will hold the status of the door. We could create a new attribute on the door object, but there is no need, the exit already has an attribute doing just so, so we can use that one. As a general rule, if you can use the built-in attributes, that is better than using your own; they are already doing the work for you, no need to repeat the effort.


## UNLOCK Command

Generally I would recommend doing this with a verb, because it always relates to an object, but the built-in unlock system stops you doing that, so it will have to be a command.

Go to Commands on the left, and then click Add. For your new command, fill in the fields like this (script is in code view):
```
Pattern:  Command pattern
          unlock #object#
Name      unlock cmd
Script    if (not object = door) {
            msg ("You cannot unlock that!")
          }
          else if (not blue key.parent = player) {
            msg ("You don't have the key.")
          }
          else if (not exit to lounge.locked) {
            msg ("It's already unlocked.")
          }
          else {
            msg ("You unlock the door with the blue key.")
            exit to lounge.locked = false
          }
```
When the player types in a command, Quest looks through all those in the game, looking for a match. In this case it will match "unlock" followed by the name of an object in the room or inventory (#object# matches any object present). If this command matches, Quest will run its script.


The script is typical of many commands, in that it tests each condition in turn, and only if they all pass does it actually do something. In this case it checks the thing the player wants to unlock is the door, then checks the player has the key for it, then checks it is not already unlocked. If all pass, the exit is unlocked.


It may also be worth implementing a LOCK command that works the same way, as some players will expect to be able to do that too.
```
Pattern:  Command pattern
          lock #object#
Name      lock cmd
Script    if (not object = door) {
            msg ("You cannot lock that!")
          }
          else if (not blue key.parent = player) {
            msg ("You don't have the key.")
          }
          else if (exit to lounge.locked) {
            msg ("It's already locked.")
          }
          else {
            msg ("You unlock the door with the blue key.")
            exit to lounge.locked = true
          }
```
Note that this system only requires the player to unlock the door, not to open it. You may want to modify the response to make it clear that the door is opened when it is unlocked.


## Multiple Locked Doors

If you have more than one locked door in your game, the coding can get complicated, but as long as they are in different rooms, there is a trick you can use - move the commands into the room (on-line, sith the command selected, click the Move button top right; off-line just drag it). Now you can have the same commands in each room, tweaked for the specific door, key and exit (each command will need to have its own name however).


## Two Way Doors

In real life, most doors can be locked from either side. This is a problem, because in Quest the door is only in one room. One solution would be to move the door in to the right room as the player enters it, however, I think it is easier to have two objects, the front side of he door, and the back side.

So to start off, we clearly need two rooms (say kitchen and lounge), with named exits going both ways between them (say exit to lounge and exit to kitchen), both exits checked as locked. Then we need two doors, one in each room, and a LOCK command in each room and an UNLOCK command in each room, all using the same key.

The trick, then, is to tie them together so unlocking one also unlocks the other. The important part of the UNLOCK command is the end:
```
          else {
            msg ("You unlock the door with the blue key.")
            exit to lounge.locked = false
          }
```
All you have to do is add the other exit to unlock:
```
          else {
            msg ("You unlock the door with the blue key.")
            exit to lounge.locked = false
            exit to kitchen.locked = false
          }
```
Remember to do that for both UNLOCK commands. For the LOCK commands, you do almost the same, except that locked will be set to true.


## Finally

It may also be worth creating dummy LOCK and UNLOCK command, with the same pattern, so if the player tries to unlock something in a room without a door, she will not get told the game does not understand the word "unlock", which might dissuade her from trying the command in the proper room.