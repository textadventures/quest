---
layout: index
title: Ask a question
---

Often in a text adventure you want the game to ask an open-ended question of the player - the answer to a riddle, perhaps. The "get input" script command exists to handle just that. Here is a script:

      msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
      get input {
        if (result = "man") {
          msg ("'Is it a man?' you ask.")
          msg ("'How come everyone knows the answer?'")
          msg ("'We have this thing called the internet nowadays...'")
        }
        else {
          msg ("'Is it \"" + result + "\"?' you ask.")
          msg ("'No!'")
        }
      }

The first line just asks the riddle. Then we see the "get input" command. The block after that gets run only once the player has typed a response. a magic variable called *result* has the text the player typed, so we just need to check it is the correct answer and react accordingly.

This will work, but has a couple of issues that we can handle.

Testing Input
-------------

The first problem is that the player has to type the exact string "man". What if he types "Man" or "a man"? We might also like to handle "woman" and "human" so the riddle is more politically correct.

To do this, replace the third line with this:

      if (IsRegexMatch  ("man", LCase (result))) {

The LCase function will convert the players text to all lower case, so "Man" will be handled as "man". This text will then be matched against a Regex (a pattern) rather than a specific string. As long as the player has "man" somewhere in the answer, the pattern will match.

In this case, that will match human and woman too, which is great. Usually multiple answers are not so convenient, but you can do that too - just put them inside brackets and separate each with a vertical bar.

      if (IsRegexMatch  ("(man|lady)", LCase (result))) {

There is more on [Pattern Matching](pattern_matching.html) elsewhere.

Changing the Prompt
-------------------

This is a little more complicated (and really only possible if you are using Quest offline), but is worthwhile as it makes it clear to the player that he or she should not be typing a command. To get this to work, you need to use some JavaScript! We will go though it step by step.

Create a new text file (say with Notepad), called "functions.js", and past this into it:

      function setPrompt(s) {
        document.getElementById("txtCommand").setAttribute("placeholder", s);
      }

After you save it, check the file really is called "functions.js", and not "functions.js.txt" (Windows has a habit of doing that).

In Quest, open up the "Advanced" node, right at the bottom in the right pane. click on "Javascrpt". In the left pane, click "Add". You should find "functions.js" in the Filename dropdown, otherwise just browse to it (if you cannot find it, it probably has that .txt extension, and you will need to rename it). If all goes well, you should see the code above appear.

Now modify your script to this:

      msg ("'Hello. Can you answer my riddle? What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
      request (RunScript, "setPrompt;Your answer?")
      get input {
        if (IsRegexMatch  ("(man|lady)", LCase (result))) {
          msg ("'Is it a man?' you ask.")
          msg ("'How come everyone knows the answer?'")
          msg ("'We have this thing called the internet nowadays...'")
        }
        else {
           msg ("'Is it \"" + result + "\"?' you ask.")
           msg ("'No!'")
         }
       request (RunScript, "setPrompt;Type here...")
     }

The new bits are those two lines that start "request". They run the JavaScript in the file, calling a function called "setPrompt", and sending a new prompt string.
