---
layout: index
title: Multi-state wearable items
---

A multi-state [garment](wearables.html) is something that can be worn in more than one way. I am going to use a jacket as an example; it can be worn fastened up, or it can be worn open, or worn half-buttoned. This just has three states, but you can have as many as you want.

Create the jacket as normal, setting it up as wearable. Then tick the "Multistate?" box, and a whole bunch of new stuff will appear (it is easy to untick the box by mistake, and have it all disappears; do not worry, it will all reappear with the data, when you tick it again).

You will see a number of list controls waiting for strings to be put in. Each line will be one state, so the first entry in each control will define the first state, the second line on each control will define the second state and so. It is therefore vital that you have clear in your head what each state is.

Also, bear in mind that the first state will be the default; i.e., this is how it will be whenever the player first puts the garment on.

For the jacket, there are three states:

```
Open
Half-buttoned
Fastened
```

With that in mind, we can fill in the data. The first box is the descriptor - a word to add to the alias to note that it is in this state, just as "(worn)" is added when it is put on. This is optional, and you can use * to indicate nothing should be added. For the jacket, we will put in:

```
unfastened
half-buttoned
*
```

Next the wear slots. Again you can use * to just use the default, and that will be enough for us.

```
*
*
*
```

Now the additional verbs. This is likely to be important as this will be how the player can move between states (though they are only display verbs, the actual work will be done later). When the jacket is open, we want "Fasten" to be displayed, and when fully buttoned, "Unfasten" (we cannot use "open", by the way, because of the container system). If there are several, separate with semi-colons; the middle state, half-buttoned, can be fastened or unfastened.

```
Fasten
Fasten;Unfasten
Unfasten
```

Finally the attribute bonuses. If you have the jack unbuttoned you will feel cool, buttoned up will make you feel warm, so we could add these (assuming "cool" and "warm" are both integer attributes of the player). This is designed for RPGs in which the item might give a bonus, perhaps to the player's armour.

```
cool
*
warm+2;cool-1
```

Every time the player wears a multi-state item the system will check it has these four lists, and that there is the same number of entries in each one, so it is worth going in game and confirming you get no errors when the item is worn.

You should see that if you wear the jacket, it is now `(worn unfastened)` and it has a "Fasten" verb.


Adding verbs
------------

To transition from one state to another, use verbs. Each verb has to check if the item is worn, check it is not already in the new state, and if all is okay, change its state. The state changing is done with the `SetMultistate` function, which takes the object and the new state as parameters. States number from 1, so the first state is 1; you will get an error if you try to set it to a state that does not exist ("Attempt to set state to ...").

For the jacket, then, on the _Verbs_ tab, add a new verb, "fasten". Set it to run a script and paste in this code, which will put the jacket into the second state, fastened up:

```
  if (not this.worn) {
    msg ("You're not wearing it.")
  }
  else if (this.multistate_status = 3) {
    msg ("It already is.")
  }
  else {
    msg ("You button up the jacket.")
    SetMultistate (this, this.multistate_status + 1)
  }
```

All your verbs should be variations on this, just changing the numbers and strings as appropriate.


Only removable when...
--------------------

You may decide the garment should only be removed when in a certain state, perhaps when it is already unfastened. This is easy to accomplish, you just have to set the `removable` flag as appropriate. To ensure the display verbs are right, call `SetVerbs` after doing so. For example: 

```
  if (not this.worn) {
    msg ("You're not wearing it.")
  }
  else if (this.multistate_status = 3) {
    msg ("It already is.")
  }
  else {
    msg ("You button up the jacket.")
    SetMultistate (this, this.multistate_status + 1)
    this.removeable = false
    SetVerbs
  }
```

Of course, your unfasten verb will need to set "removeable" to true when the state becomes 1.

```
  if (not this.worn) {
    msg ("You're not wearing it.")
  }
  else if (this.multistate_status = 1) {
    msg ("It already is.")
  }
  else {
    msg ("You unbutton the jacket.")
    SetMultistate (this, this.multistate_status - 1)
    if (this.multistate_status = 1) {
      this.removeable = true
    }
    this.removeable = false
    SetVerbs
  }
```