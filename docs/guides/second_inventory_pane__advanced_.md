---
layout: index
title: Second Inventory Pane (Advanced)
---

Sometimes it is useful to have a second inventory panel, and a good example of that is to hold spells available to the player. To illustrate how to implement this, I am going to add this functionality to the combat game developed in previous pages.

Step 1 - Downloads
------------------

You need a library file and a JavaScript file written by jay Nabonne, and a library file by the Pixie. You can get them here:

[InvPane2.aslx]({{site.baseurl}}/files/InvPane2.aslx) | [InvPane2.js]({{site.baseurl}}/files/InvPane2.js) | [ScopeLib.aslx]({{site.baseurl}}/files/ScopeLib.aslx)

Step 1 - Infrastructure
-----------------------

You now need to add the libraries to your game. In the right hand pane, click on Advanced "+" to open that up, then select "Included Libraries". Click "Add", and the "Browse", and navigate to the downloaded file *InvPane2.aslx*. Click add again, and navigate to the downloaded file *ScopeLib.aslx*. You then need to save your game, and click on the reload button at the top right.

Click on Advanced "+" again to open that up, then select "JavaScript". As before, click "Add", and the "Browse", and navigate to the downloaded file *InvPane2.js*. You will suddenly see a lot of code in the window, but you do not have to do anything with it!

Create a new room, let us say it is called *spells\_known*.

Step 2 - Start-up Script
------------------------

Create a new start up script on the Script tab of the Game object (if you already have a start-up script, just add these lines to it).

      SetInventory2Label ("Spells")
      SetInventory2 (GetDirectChildren (spells_known))

This first line gives your new inventory a title, the second populates the inventory with anything in the *spells\_known* room.

Step 3 - Putting things in the inventory at the start
-----------------------------------------------------

Anything you want the player to start the game with should go into the *spells\_known* room. Simple. You may want to change the inventory verbs; for spells this would mean having *Cast*, but not *Learn*.

Step 4 - Putting things in the inventory during play
----------------------------------------------------

How you do this depends on how you are using it. The important steps are to move the object to the room, *spells\_known*, and to update the inventory when you have done so. In this example, the learn script will then look like this:

        <learn type="script"><![CDATA[
          if (not this.parent = game.pov) {
            this.parent = spells_known
            this.inventoryverbs = Split ("Cast", " ")
            msg (DynamicTemplate("SpellLearnt", this))
            SetInventory2 (GetDirectChildren (spells_known))        
          }
          else {
            msg ("[SpellAlreadyKnown]")
          }
        ]]></learn>

Step 5 - Taking things out of the inventory
-------------------------------------------

This is just the reverse of step 4. the object has to be moved away, and then you call:

      SetInventory2 (GetDirectChildren (spells_known)).

Step 6 - Using things in the inventory
--------------------------------------

If the player tries to use something from the inventory at this point, he will be told it is not here - as far as Quest in concerned it is in another room!

To get around that, we will use *ScopeLib*. This will require that cast is handled as a command, rather than a verb, so the verb can be removed entirely. The command will look like this:

      <command name="cast_spell">
        <pattern>cast #text#;invoke #text#</pattern>
        <script>
          ProcessScopeCommand ("cast", spells_known, text, "You don't know a spell like that", "I'm not sure that is really a spell")
        </script>
      </command>

All the difficult stuff happens in *ProcessScopeCommand*. All we need to know is the parameters. The first is the name of the script that will be called. That is already there, and is called "cast". The second parameter is a list of objects, in this case the spells in the *spells\_known* room. The third is the variable Quest is using to hold the text, and is called *text*. The last two are error responses - what the player will see in case the spell is not found or does not have the right script.

Step 7 - Tidying up
-------------------

Finally we need to modify the cast scripts, as they check the player is holding the spell. The *ProcessScopeCommand* has already checked the spell is in the right place, so it is just a case of deleting five lines. From this:

        <cast type="script"><![CDATA[
          if (this.parent = game.pov) {
            DescribeCast (this)
            flag = False
            foreach (obj, ScopeVisibleNotHeld ()) {
              if (DoesInherit (obj, "monster") and not GetBoolean (obj, "dead")) {
                SpellAttackMonster (obj, this)
                flag = True
              }
            }
            if (not flag) {
              msg ("... [NoMonstersPresent]")
            }
            CancelSpell ()
          }
          else {
            msg ("[SpellNotKnown]")
          }
        ]]></cast>

To this;

        <cast type="script"><![CDATA[
            DescribeCast (this)
            flag = False
            foreach (obj, ScopeVisibleNotHeld ()) {
              if (DoesInherit (obj, "monster") and not GetBoolean (obj, "dead")) {
                SpellAttackMonster (obj, this)
                flag = True
              }
            }
            if (not flag) {
              msg ("... [NoMonstersPresent]")
            }
            CancelSpell ()
        ]]></cast>

Similar for other cast scripts.

Everything Done
---------------

The new combat library and demo can be downloaded here (but remember you still need the three files mentioned at the start as well):

[CombatLibrary2.aslx]({{site.baseurl}}/files/CombatLibrary2.aslx) | [CombatDemo2.aslx]({{site.baseurl}}/files/CombatDemo2.aslx)
