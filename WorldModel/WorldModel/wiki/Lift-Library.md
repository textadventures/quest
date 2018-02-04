_NOTE: If working on-line, you will be unable to use this, as custom libraries cannot be used._

Using this library you can easily implement one or more lifts in your game. Each lift can stop at any number of floors. Of course, you could apply this to any transport system that involves stepping into a location, and pressing a button, then stepping out at the destination.

[LiftLib](https://github.com/ThePix/quest/blob/master/LiftLib.aslx)


## Instructions

### Rooms and objects

To implement, create a room that will be the lift, and in it put a number of objects that will be buttons. You will obviously need one button per floor. For each floor, create an exit that goes to the lift room (but not the other way), also create a single exit for the lift room (does not matter where to).

### Exits

For each exit going to the lift, on its attributes tab, add "liftentrance" as a new type (do not do this for the exit in the lift room).

### Buttons

For each button, on the Lift tab, set it to be "Lift Button", assign a floor number and a destination for each (i.e., where the lift will exit to when this button is pressed). There are text fields you can fill in, but they are optional.

### Lift

For the lift room itself, on the Lift tab, set it to be a "Lift". Again, the textfields are optional. You can use ### in these strings and this will be replaced by the destination floor.


## Not just for lifts

This system could readily be used for other systems that move at the push of a button, for example a shaceship for dummies! On the lift object, remove the going up and going down messages, but on the buttons, add descriptive departure and arrival messages, such as:

> The clamps were released, and the ship fell away from the rotating spacestation.

> Ahead was the space station, slowly rotating. The ship matched rotation, as its nose penetrated the centre of the wheel-shaped structure.

In this case, the floor name might be _Spacestation_, and the button could be called: _"Spacestation" button_.


## Call the lift

Note that as implemented the player does not have to press a button to call the lift - the lift is automatically at whatever floor the player wants to get on at. You might want to have the lift entrances locked, and require the player to press a button to call the lift, which then unlocks the exit.

The library has support for setting this up, but you have to do some of it yourself. The problem is deciding when and what gets locked. If the player steps into the lift, does the door shut at that point, or only when the player goes to a different floor? When the player steps out of the lift, does the door close immediately, or does it wait for a few turns. These are decisions you need to make. However, the functions below should make this relatively easy to implement, by [overriding](https://github.com/ThePix/quest/wiki/Overriding-Functions) LeavingTheLift, and using LiftExit and LiftAllEntrances to get all the exits, and then locking them.


## The `LeavingTheLift` Function

The `LeavingTheLift` function is called when the player leaves the lift, after the leaving message is displayed (if there is one). It does nothing, but can be [overridden](https://github.com/ThePix/quest/wiki/Overriding-Functions) as required, for example to lock the lift doors as mentioned above.


## Someone else pressing the button?

If you want the lift to move for some reason other than the player pushing the button, invoke the `move` script on the relevant button. For example, if an NPC enters the lift and presses the button for the top floor:
```
  do(top_floor_button, "move")
```

## Other Functions

There are a number of other functions that can be used to find other parts of the lift system. All of them take an object as the only parameter; this can be either the lift itself or one of its buttons.

* `LiftRoom` simply returs the room that is the lift.

* `LiftExit` returns the exit from the lift room.

* `LiftCurrentFloor` returns the room that the lift exit goes to, i.e., the floor the lift is currently at.

* `LiftCurrentButton` returns the button that takes the lift to the current floor.

* `LiftAllButtons` returns an object list of all the buttons in the lift (this will include any that are invisible or scenery).

* `LiftCurrentEntrance` returns the exit from the current floor into the lift.

* `LiftAllEntrances` returns an object list of all the exits into the lift. This is built from the buttons in the lift; if you have an exit to the lift without a corresponding button, it will get missed. If you have a button for a floor without an exit into the lift, it will throw an error.


## Notes

Do not start the player in the lift!

Only have one exit on the lift room itself.