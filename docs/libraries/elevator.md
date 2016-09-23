---
layout: index
title: Elevator
---

Created by jaynabonne
---------------------

[Download](http://textadventures.co.uk/attachment/371)
[Discussion](http://textadventures.co.uk/forum/samples/topic/3212/elevator-action)

Basically, an elevator is a room that moves among a set of other rooms in an orderly way. To create an elevator, you set up a parameters object with a list of "floor parameter" objects (each of which specifies the target exit room and the room designation), and then you invoke Elevator\_Create. It probably sounds more complicated than it actually is. Check out the sample to see how it works.

The core elevator code is in "Elevator.aslx" and the elevator sample code is in "ElevatorSample.aslx". The sample includes a stairwell as well, so you can look at the elevator from different floors.

There are built-in components you can use if you just want to slam an elevator into your building. But there are provisions in place for customizing it all to your own design. If you have any trouble customizing it, let me know.

The elevator leverages some other code that was developed explicitly for it, but which has lots of other uses: a "goals" engine. This engine implements a "goals object" which is an object you wish to be "goal-driven." You then add goals to it and update it periodically to let it follows its goals. The goals engines is in "GoalsObject.aslx"

The elevator needs to be updated regularly to operate properly. It is an autonomous object, but it needs a kick to keep it going. This can be any way you like, but the two straightforward ways are via a turn script and via a timer. The former appies if your game design (or philosophy) is for things to happen only when a user makes a turn. The latter is for those games that are comfortable with events happening in real time. I have exercised this code in both modes. In the turn-based mode, you will often need to type "wait" or perform some other actions (like looking around) to make the world state advance. In the real-time mode, the elevator just goes on as it does, but it won't wait for you! I have left the code in the real-time mode, since it's a bit more fun. :) (The turn script is in the sample if you want to use it. Just disable the timer and enable the turn script.)

Note that you must include both "GoalsObject.aslx" and "Elevator.aslx" to incorporate this into your project.

This code could easily be adapted to other uses - anything that has a door and travels (for example, a Star Trek style turbolift, an auto taxi sort of thing, etc.

As the elevator code is goal based (with two goals currently - "close door" and "seek floor"), its behavior can easily be adapted as well by adding your own goals. For example, if you wanted to have a haunted house where the elevator breaks down, you could implement a goal that moves the car between floors and then stops.
