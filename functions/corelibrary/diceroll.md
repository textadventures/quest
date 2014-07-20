---
layout: index
title: DiceRoll
---

    DiceRoll (string dicetype)

Returns an [int](../../types/int.html) - the result of the dice roll.

Format dicetype: [number of dice]d[number of sides]

      //roll 3 dice with 6 sides
      result=DiceRoll("3d6")
      if (result>12){
         msg ("you hit the troll.")
      } 

This function was added in Quest 5.3.
