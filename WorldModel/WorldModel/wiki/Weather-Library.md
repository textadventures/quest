Adding weather to your game can make the world feel more dynamic, but will take some effort (though it is not overly complicated).

The system implemented here works like the player visiting different rooms, except it is not the player object that is moving around, but the "currentweather" attribute of the game object. Each room it can visit represents a specific weather type, and after a certain time interval, it will select an exit from that room to another, as the weather changes.

[Library](https://github.com/ThePix/quest/blob/master/WeatherLib.aslx)
[Demo](https://github.com/ThePix/quest/blob/master/weather.aslx)


Time
----

Weather changes over time, so this will involving tracking time. At its simplest this is just a turn script, but this could involve ensuring nothing happens if the player mistypes. Whatever system you use, you need to ensure the `Weather` function is called as time passes in game.

For simplicity, we will create a new turn script, tick it to be active from the start, then give it this code:

```
Weather
```
Make sure the turn script is not inside a room; Quest has a habit of putting turnscripts in rooms. If it is, drag it to "Objects" at the top of the left pane.


Weather rooms
-------------

Each weather type is a room, so now you need to create a whole bunch of rooms. For each one, first on the _Setup_ tab, the alias is what the player will see when entering a room; a sentence describing the current weather.

Now go to the _Weather_ tab, and set the room to be "Weather".

The complicated bit here is the "Weather change script". This is what determines if it is time to change the weather. This script must set `game.weathertochange` to `true` if the weather should change this turn, or false otherwise. Note that `game.weathercount` records how long the weather has been like this.

This is the default:
```
game.weathertochange = RandomChance(game.weathercount)
```
This means the weather is very unlikely to change at first, but becomes more likely as time passes.

If modelling a thunder storm, you might want big spots of rain for a couple of minutes:
```
game.weathertochange = (game.weathercount > 2)
```
And lightning just one turn:
```
game.weathercount = true
```


Weather exits
-------------

If the weather is set to change, then a random exit is picked, and the weather will change to what that exit points to. So your "Blazing sun" room might have an exit to "Warm and humid", but not to "Blowing a blizzard". This allows you to have weather that moves from one type to another in a logical way.

You can put a description for the transition ("It is just starting to rain" or "it is starting to feel chilly") in the "message" attribute of te exit (bottom of the _Exit_ tab in Quest 5.7; you will need to do this via the _Attributes_ tab of the exi in earlier versions).


Other rooms
-----------

Obviously the weather only applies if you are outside. For rooms that are inside, go to the _Weather_ tab, and tick the "Inside" box.


Start Up
--------

We need to kick start the system. Go to the _Scripts_ tab of the game object, and the start script at the top. You can either do this:
```
Weather
```
... which obviously is easiest, and will start the game with a random weather type, or you can set a type like this:
```
game.currentweather = hot cloudless
```

It would also be nice to hear about the weather when you enter a room (if it is outside), so in the "Script when entering a room", add this: 

```
WeatherReport
```

How wet is the player?
----------------------

If you adjust the rain amount of each weather type, you can track how wet the player is. This is done with the "soaking" attribute, which can vary from 0 (dry) to 100 (drowned rat). The rain amount for each weather type is the amount that gets added each turn, so at most should be between 10 and 20. You could also set it to a negative number for hot, dry weather, as that will serve to dry the player out. 

If the player is inside, she will dry out by 5 each turn.
