---
layout: index
title: Adding Introduction Text
---

Many games start with some introductory text before getting the player started. This is an suggestion how to create an introduction. This version is supported by Quest5.2

The introduction is devited into three parts. In the first part a startpicture is displayed. The complete intro is coded in the startscript of the game calling an additional function questionname.

After creating a nearly full-screen picture, set it as the frame picture of the startroom.

     
      SetFramePicture ("Intro.jpg")
      

To provide scrolling insert a "press any key to continue..." and a wait-command in the startscript of the game, so the player can enjoy this painting.

     msg ("press any key to continue...")
     wait {
       ...
       

After pressing a key the roompicture is removed

     ClearFramePicture

and the player advances to part two. This is just displaying some text, the story of the game. Again after "press any key to continue..." the player proceeds to part three. Here he should insert his name. This is done by calling the function questionname

     <function name="questionname"><![CDATA[
        msg ("\"So would you tell me your first name please?\"")
        get input {
          game.firstname = result
          msg ("> " + game.firstname)
          msg ("\"Thank you. And now your noble surname?\"")
          get input {
            game.surname = result
            msg ("> " + game.surname)
      ...

His name is saved in the variables game.firstname and game.surname, which can be used in scripts in the later game.

     msg ("\"Then you are called " + game.firstname + " " + game.surname + "?\"")

After he inserted his name, he is asked to confirm his name. If he denies this, he is asked for his name again. This is the reason of the function questionname so it can called again and again until the player is happy with his name.
