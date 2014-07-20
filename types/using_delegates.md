---
layout: index
title: Using Delegates
---

It is easy to create a [script](script.html) attribute to run at a particular point in the game, but what if you want to create a script attribute that returns a value, or accepts particular parameters? It would look a lot like a function. The answer is to use **delegates**.

First you need to define the delegate, using a [delegate](../elements/delegate.html) XML tag. This accepts the same attributes as the [function](../elements/function.html) tag, so you can specify parameters and/or a return value type.

Now you can simply use the delegate name as an attribute type name.

-   To run a delegate which does not return a value, use the [rundelegate](../scripts/rundelegate.html) script command.
-   To get a return value from a delegate, use the [RunDelegateFunction](../functions/rundelegatefunction.html) function.
-   To see if an object implements a delegate, use the [HasDelegateImplementation](../functions/hasdelegateimplementation.html) function.

Delegates in Action
-------------------

Let us see this in action. First, a bit of terminology. In "object-orientated programming" a function that is attached to an object is called a "method", and I am going to adopt that term here. The "signature" of a method is the return type and the parameters it expects (a Quest function similarly has a signature).

Here is a very simple game where you can hit a goblin.

      <!--Saved by Quest 5.4.4873.16527-->
      <asl version="540">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <game name="test">
          <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
          <version>1.1</version>
          <firstpublished>2013</firstpublished>
        </game>
        <object name="room">
          <inherit name="editor_room" />
          <object name="player">
            <inherit name="editor_object" />
            <inherit name="editor_player" />
          </object>
          <object name="goblin">
            <inherit name="editor_object" />
            <fullhits type="int">10</fullhits>
            <hitslost type="int">0</hitslost>
            <hit type="script">
              msg ("You hit the " + this.name + " and it loses 7 hits!")
              this.hitslost = this.hitslost + 7
            </hit>
          </object>
        </object>
      </asl>

The goblin object has a method, called "hit". It is also a script, as it takes no parameters and returns no value.

We will now set up a new method, which will return the percentage of the goblin's full hits that it has remaining. A delegate is essentially a way to define a custom signature, in this case it will look like this:

      <delegate name="script_returns_int" type="int" />

This has to appear in your code before the delegate is used - just after the library includes seems best to me. Here is the modified game:

      <!--Saved by Quest 5.4.4873.16527-->
      <asl version="540">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <delegate name="script_returns_int" type="int" />
        <game name="test">
          <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
          <version>1.1</version>
          <firstpublished>2013</firstpublished>
        </game>
        <object name="room">
          <inherit name="editor_room" />
          <object name="player">
            <inherit name="editor_object" />
            <inherit name="editor_player" />
          </object>
          <object name="goblin">
            <inherit name="editor_object" />
            <fullhits type="int">10</fullhits>
            <hitslost type="int">0</hitslost>
            <getpc type="script_returns_int">
              return (100 * (this.fullhits - this.hitslost) / this.fullhits)
            </getpc>
            <hit type="script">
              msg ("You hit the " + this.name + " and it loses 7 hits!")
              this.hitslost = this.hitslost + 7
              msg ("The " + this.name + " has " + RunDelegateFunction (this, "getpc") + "% of its hits.")
            </hit>
          </object>
        </object>
      </asl>

Note the new method, "getpc". It is set up like a script, but its type is what was defined by the delegate, rather than "script". Also, to invoke it, use "RunDelegateFunction" rather than "do".

Suppose we want to kick that goblin too. We do not want to have to repeat code, so the kick and hit scripts are going to call a new method, "attack", and send a parameter, the damage done. We need a new delegate, this time allowing a parameter to be sent, rather than a value to be returned (you can readily do both, of course, if you need to).

      <delegate name="script_with_1" parameters="a" />

The game now looks like this:

      <!--Saved by Quest 5.4.4873.16527-->
      <asl version="540">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <delegate name="script_returns_int" parameters="" type="int" />
        <delegate name="script_with_1" parameters="a" />
        <game name="test">
          <gameid>cb4455e4-6e7c-45da-bf39-f2126e817fb1</gameid>
          <version>1.1</version>
          <firstpublished>2013</firstpublished>
        </game>
        <object name="room">
          <inherit name="editor_room" />
          <object name="player">
            <inherit name="editor_object" />
            <inherit name="editor_player" />
          </object>
          <object name="goblin">
            <inherit name="editor_object" />
            <fullhits type="int">25</fullhits>
            <hitslost type="int">0</hitslost>
            <getpc type="script_returns_int">
              return (100 * (this.fullhits - this.hitslost) / this.fullhits)
            </getpc>
            <attack type="script_with_1">
              msg ("You hit the " + this.name + " and it loses " + a + " hits!")
              this.hitslost = this.hitslost + a
              msg ("The " + this.name + " has " + RunDelegateFunction (this, "getpc") + "% of its hits.")
            </attack>
            <gethits type="script_returns_int">
              return (this.fullhits - this.hitslost)
            </gethits>
            <hit type="script">
              rundelegate (this, "attack", 4)
            </hit>
            <kick type="script">
              rundelegate (this, "attack", 7)
            </kick>
          </object>
        </object>
        <verb>
          <property>kick</property>
          <pattern>kick</pattern>
          <defaultexpression>"You can't kick " + object.article + "."</defaultexpression>
        </verb>
      </asl>

Note that we use "rundelegate" to invoke the new method, as it does not return a value.

Also note that the parameters for your method must have the same names as those in your delegate definition.
