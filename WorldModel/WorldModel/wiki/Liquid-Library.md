On [this page](https://github.com/ThePix/quest/wiki/Handling-Water) I described how to set up water in your game. This library is a development of that system that can handle different liquids.

[Library](https://github.com/ThePix/quest/blob/master/LiquidLib.aslx) | [Demo](https://github.com/ThePix/quest/blob/master/LiquidDemo.aslx)

Once you have added the library, you will see a new tab appears, Liquids.


### For Rooms

You can set a room to be a source of a certain liquid, and specific what that is. You can also add a script that will be run if a container is emptied in the room.

### For objects

You can set them to be containers (of liquids) and given them a capacity and starting volume and liquid.

### Functions

There are two functions you may want to add to your game to determine what happens if the player mixes two liquids together in a container, and what happens when she drinks from a container.
```
  MixLiquid (object container, string liquidtype)
```
This function needs to set the liquidtype of container, based on what is already in it, and what you are adding to it. It should also print a message. The function is only used if the container was not already full or empty, so you do not need to worry about handling that..
```
  DrinkLiquid (string liquidtype)
```
This function determines what happens when the given liquid is drunk. The volume in the container will be automatically reduced, but you do need to print a message at the very least.