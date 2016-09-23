---
layout: index
title: Implement a Lift
---

<div class="alert alert-info">
This page needs updating with the correct links
</div>

NB: This page and the downloadable library have now been updated for Quest 5.4.

This is a way to implement a system in which the player walks into the device, presses a button and is taken to a destination. This could be a lift, but it might be an automated rail system or a robotic car or whatever.

The system is set up using the LiftLib library (right click and save to your computer):

[LiftLib5-4.aslx]({{site.baseurl}}/files/LiftLib5-4.aslx)

### Setting Up

The first thing you need to do is to include the library in your game, so (once you have it downloaded) expand the "Advanced" object in the left pane, then click on "Included Libraries". In the right pane, click on "Add", and navigate to the downloaded file. Save and reload.

### The Lift

Create a new room to be the lift. You will find there is now a "Lift" tab, go there, and set this object's type to "Lift". There will now be three text boxes for you to fill in. The top one will appear if the player presses the button for the floor she is already on, the second if the lift will be going up, and the third for going down. You can use three hashes to stand in for the destination floor. Here are some examples:

      You press the button for ###, but nothing happens. Perhaps because you are already on ###.

      You press the button for ###. The lift doors closed, and with a slight jolt the lift moves up to ###.

      You press the button for ###. The lift doors closed, and with a slight jolt the lift moves down to ###.

The room needs an exit, but it does not matter where to (oh, and never put more than one exit goinmg from this room).

### Buttons

Inside the lift room, create some objects. Each of these will be a button with an associated destination. For each button, on the "Lift" tab, set the type to "Lift Button", set the destination to be where the player will end up after pressing this button and leaving the lift, and set the number of this floor.

You can also set a name for the floor, a message the player sees when she first arrives and a message she sees each time she arrives, but these are all optional.

### Lift Entrances

Every floor the lift can call at must have an exit leading to the lift (which is therefore a lift entrance). As this is an exit, it is not quite as simple to set up as the lift itself and the button (because its tabs cannot be modified), so you have to go to the "Attributes" tab, and, in the *Inherited Types* section, click "Add", then select "LiftEntrance".

That is all for entrances. Your lift should now be working.

### Lift Control

A lift control offers an alternative way for the player to interact with the lift, so the following steps are optional.

Create a new object in the lift, and on the Lift tab, set it to be a Lift Controls object. For each of the lift buttons, set it to be scenery. Now the player can "use" the controls, and when she does, she will be presented with a menu of destinations.

### Notes

Note that as implemented the player does not have to press a button to call the lift - the lift is automatically at whatever floor the player wants to get on at. You might want to have the lift entrances locked, and require the player to press a button to call the lift, which then unlocks the exit. Have the exit lock when the player goes that way.

Do not start the player in the lift!

A lift can go to as many floors as you like, and you can even add more floors during the game - just create the lift button somewhere else and move it into the lift as required. You can also have as many lifts as you like in one game.

### Working Example

This very simple game shows the system in use (right click and save to your computer). It has a lift using buttons and a simple train using a control panel.

[LiftDemo5-4.aslx]({{site.baseurl}}/files/LiftDemo5-4.aslx)
