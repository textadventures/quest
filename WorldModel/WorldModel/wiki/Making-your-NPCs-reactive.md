The Conversation Library offers an advanced feature that will help add life to your characters.


The reactions facility is designed to be a way to have NPCs react to what the player has done. For example, if the player suddenly appears wearing a Mickey Mouse costume, it would be reasonable to expect an NPC to comment on it. This is a system that makes having an NPC comment on what the player has done fairly easy - and by easy, I mean less work, not necessarily conceptually simple.

The basic idea is that each NPC has a set of reactions, and the system tests them to see if any one reaction is applicable.

It is assumed that most of these reactions are just text printed to screen. If you want complex effects, you may need something beyond this (but it should be possible to do that in addition to this system).

Assuming you are already using ConvLib, the first step is to turn the system on. Go to the game object, and add a new Boolean attribute, `showreactions`, and set it to true. Now if you go to the _Conversation_ tab for an NPC, there will be a new section, "Reaction script".

## Reacting to the Player Wearing a Hat

As an example, let us suppose there is an NPC called Mary, and that the player can pick up and wear a couple of hats, one plain and one fancy. If Mary sees the player in a hat, we want her to comment on it. Also, if she sees him in the plain hat first, but later in the fancy hat, we want her to react to that too.

Let us start by noting that this is unlike anything else in Quest! We will be typing instructions in the box, but not as a script. Each reaction is composed of four parts, the first three each getting their own line, while the fourth part can be as many lines as you like.

1. The first part is just a @ on its own. This indicates the start of a reaction.

2. The second part is a name for the reaction; whatever you like that identifies this reaction. It need not be unique, and in fact having several with the same name can be useful, as described later.

3. The third part is Quest code; a condition or set of conditions. When it evaluates to true, this reaction will be done. If it evaluates to false, it will not. You can use _and_ and _or_ as normal to link conditions together. You can also use _this_ as normal to refer to the NPC.

4. The fourth part is what will happen. This can be any number of lines, and each will be processed separately. Most will be treated as text, and will go straight to the screen. You can use text processor commands as normal.

With that in mind, here is the reaction text for Mary:
```
@
plain hat
plain_hat.worn
'Oh, you are wearing a hat,' says Mary.

@
fancy hat
fancy_hat.worn
'Wow, what a fancy hat,' says Mary.
~plain hat
```
As you can see, there are two reactions, each starting with a @. The next line is the reaction name, and then there is condition. In this case, we are testing whether the `worn` flag is set to true. If it is, that reaction will be done. Once a reaction has been done, it is removed from the list, so it can only ever happen once.

After the condition, there are the events. The first just has some text to print. The second has that two, but the last line start with a ~, followed by the name of the first reaction. The ~ indicates that the named reaction should be deleted (in addition to this one).

## Mutually Exclusive Reactions

Let us now suppose the player can win a trophy, and we want Mary to react to that. We want two reactions, one for Mary encountering the player with the trophy and one for when he does not have the trophy. However, we only ever want one to fire.
```
@
trophy
player.won_competition and trophy.parent = player
'Cool trophy,' says Mary.

@
trophy
player.won_competition
'Hey I heard you won the completion,' says Mary.
```
What we could have done is use the ~ to tell the system to delete the other reactions, but in this case that is not necessary. The system deletes _all_the reactions with the same name, so when either one of these gets used, the system deletes all the reactions called "trophy" (for this NPC). 

## Modifying Attributes

The system offers limited support for setting attributes. Here is an example to illustrate the basic format:
```
@
trophy
player.won_competition
'Hey I heard you won the completion,' says Mary.
= this alias "Happy Mary"
+ Mary happiness 5
```
Each instruction gets a line on its own, and the line has to start with =, + or -. Then there is the name of the object, followed by the attribute name, and then the value, all separated by spaces. Note that neither the object nor the attribute can contain spaces, however, you can use "this" to refer to the current NPC.

The + and - can be used with integer attributes only, and will add to or subtract from the current value. If the attribute does not exist, it will be created and given a default value of zero before the addition or subtraction.

## Running Scripts

A final otion is to run a script. This is done in a similar manner to setting attributes, but uses a & at the start of the line. In this example, the "greeting" script of Mary will be run (as before, I could have used "this" to refer to the current NPC).
```
@
plain hat
plain_hat.worn
'Oh, you are wearing a hat,' says Mary.
& Mary greeting
```

## Notes

The system does not use "this" in the normal way; it cheats and substitutes "this" with the name of the character. Generally you will not notice the different, but if you have a name or something in the condition that includes the string "this", perhaps as part of a longer word or name, this will screw up.
