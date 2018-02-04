This is a library for handling time in your game. As well as tracking time, and displaying it properly, you can also use it to trigger events at a certain time or so many minutes in the future. It assumes that in general one turn takes one minute, and that the game will not continue beyond 99 days.

This tutorial is split into two parts. The first half is pretty easy, and allows you to track time in your game. The second half is more advanced, but allows you to set up timed events to bring your game alive.


## The Library

The library can be downloaded here:

[ClockLib](https://github.com/ThePix/quest/blob/master/ClockLib.aslx)


## Setting up

Once you have included the library in your game, you need to do a few things to get it to work properly. If the player mistypes a command, we do not want that to count as a turn, and there is some work to do to make that happen.

To get this to work, go to the game object and create a Boolean attribute, `notarealturn`, and a script attribute called `unresolvedcommandhandler`. Set the script to this:
```
  game.notarealturn = true
  msg (Template("UnrecognisedCommand"))
```
Quest automatically calls `unresolvedcommandhandler` whenever a command cannot be handled (if it exists), so what this will do is flag that turn up as one that does not count in the game, and the system will not move time on. You might want to add that first line to other commands too, such as HELP and possibly LOOK too. Just paste it in at the end of the command's script.

The next bit is optional, but a good idea. At the top of the _Attributes_ tab are the status attributes. Click _Add_, then type in "clock" in the first box and "Time: !" in the second. 

Now go in to the game, and see what happens. The time will be displayed in the status attributes (after your first turn away, but we will deal with that soon), and will move on by one minute when you do stuff, but not if you type in nonsense.


## Set the start time

By default the game starts at midnight; odds are you want it to start at some other time. To do that, use the `SetTime` function. Go the _Scripts_ tab of the `game` object, and add this to the `start` script:
```
  SetTime("01:14:35")
  game.clock = TimeAsString()
```
The SetTime function will set the game time to the given number of minutes past midnight on day 1, however, it will also accept a string, as above, and that is rather easier. In this case the time will be 1435 (i.e., 2:35 pm), on day 1.


## Longer actions

For commands that take a long time, several minutes or more, add the `SetInc` command (short for "set increment", as it sets how much time will increment this turn). It takes a single parameter, the duration in minutes. For example, for a command that takes 3 minutes, add this:
```
  SetInc(3)
```
For exits that take a long time to get to the destination, tick them to "Run a script", and then use this script (adjusting the time as required):
```
  player.parent = this.to
  SetInc(6)
```

## Is It After That Time?

Often you will want to know how much time has passed. The `IsAfter` function does just that. This example checks if it has gone five past eight.
```
  if (IsAfter("1:20:05") {
```
To check if it is before, just use not.
```
  if (not IsAfter("1:20:05") {
```

## The CLOCK command

There is a built-in command, TIME, CLOCK or WATCH, which will display the time.

## The `EachTurn` Function

Override this to have events fire each _real_ turn (and not fire when the player mistypes).


***


## Events

You can have events fire at set times, by creating event objects. Events objects are just ordinary objects, but they have to be named very precisely. The format of the name is "event", followed by the day, hour and minute that the event should fire, each as double digits, all separated by underscores. Here is an example:
```
  event_01_14_40
```
This event will fire after the turn at 1440 (2:40 pm) on day 1. After it fires, the time will become 1441.

To have the event do something, set its "Look at" to be a script, and put the details in there. For an example, try this:
```
  msg ("An alarm indicator flashes!")
```
Event objects can be anywhere, but I recommend putting them all in a single room that it is not accessible to the player.


## Events and wait

If the player types WAIT, up to fifteen minutes will pass. However, that will get interrupted if an event occurs. In our example, the game starts at 2:35. If the player types WAIT, time will pass until the event at 2:40 occurs. The time will then move on to 2:41.

Often the player will not be around to see the event, and in that case it would be strange if the WAIT command stopped after 5 minutes for no apparent reason. For this reason we have the `Quiet` function. This tells the system that this was not an event that should stop the waiting process.

Let us say the alarm is in the control room, and we have this code on the event:
```
  if (player.parent = Control room) {
    msg ("An alarm sounds!")
  }
  else {
    Quiet
  }
  Control room.alarmtriggered = true
```
The first line tests if the player is in the control room. If she is, the message is displayed, and the waiting process is interrupted. If not, not message is displayed, and, because of the `Quiet`, the wait is not interrupted.

Note that the flag on the control room needs to be set either way.


## Creating Events On The Fly

Sometimes you want to create new events on the fly, scheduling them for a set number of minutes from the current time. Let us suppose dinner is served 5 minutes after the player gets to the dining room. On the Scripts tab of the dining_room, go to Entering the room for the first time, and paste in this code:
```
  CreateEvent(5, "dinner") {
    msg("Dinner is served.")
  }
```
The delay is set to 5 minutes, and everything in the curly braces is the script that will be run. You can make that as complicated as you like, and it would certainly be a good idea to check if the player is actually present.

The second parameter, the string, is just to identify the event when debugging.


## Sequences

An alternative approach to events is to define a sequence of steps.

Each step is an object, just like the events earlier. To link a step to the next one in the sequence, use the `NextStep` function in the "look at" script. For example:
```
  msg ("'Did you enjoy the soup?' enquires the butler.")
  NextStep (soup_rain, 4)
```
This will cause the object "soup_rain" to be done in 4 minutes.

To start a sequence, just call NextStep from any script, sending it the first object in your sequence.

You can also have sequences branch. One step can test conditions, and choose the next step accordingly.
```
  msg ("'Did you enjoy the soup?' enquires the butler.")
  if (player.cursed) {
    NextStep (soup_rain, 4)
  }
  else {  
    NextStep (second_course, 10)
  }
```
Branches can converge simply by pointing to the same step. You could also have steps running in a continuous cycle. It could be useful to have a step linking to itself until a condition is met. This example checks every three turns whether the player is in the dining room, only going to the next step once she is.
```
  if (player.parent = dining_room) {
    msg ("'Did you enjoy the soup?' enquires the butler.") 
    NextStep (soup_rain, 4)
  }
  else { 
    Quiet
    NextStep (this, 3)
  }
```

Note, however, that only one sequence can be running at a time (timed events and events created on the fly are not affected). The system only remembers a single event to do next, and if another sequence calls NextStep, that will get overwritten, and the sequence will terminate.


## Other Options

### Altering the waiting time

By default, the WAIT command will allow up to 15 minutes to pass. You can change that by setting `game_clock.waittime`, in the `game.start` script.
```
  game_clock.waittime = 20
```


### Debugging

Set `game_clock.testing` to true to have the clock system display diagnostic messages. This may help you find obscure bugs.


### The CLOCK message.

Set `game_clock.clockmsg` to a new string to change the message when the player checks the time. Use ### as a stand-in for the time. For example:
```
  game_clock.clockmsg = "You glance at you watch, and see it is ###."
```


### Dates

_Note:_ This was added in version 3.1.

If you game is happening over several days, weeks or months, you may want to track the date. The system tracks the current day in `game_clock.days`. Note that this must be greater than zero at the start (set in `SetTime`). Note that the events system is limited to 99 days (as it takes a two digit day), however, if you are not using that it should be good for over 4000 years!

You can use the `Date` function to get the current date as a string.

```
msg("Today is " + Date())
```

Dates are relative to the year `game_clock.startyear`, which is 2000 by default, but could be changed in your game start script:

```
game_clock.startyear = 2017
```

Note that this is an approximation to the Gregorian Calendar; it will handle simple leap years, but is not perfect. It will consider 1900, for example, to be a leap year, however, it is fine for 1901 to 2099, which should cover most games.

You can override DisplayDate to alter how the date is actually shown (for example, to have months displayed in another language). For reference, here is the default. Note that month zero is blank, as months (and days) count from 1.

```
months = Split(";January;February;March;April;May;June;July;August;September;October;November;December", ";")
return ("" + day + "th of " + StringListItem(months, month) + ", " + year)
```