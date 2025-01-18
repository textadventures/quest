---
layout: index
title: Adding a Dialogue Panel
---

We are going to use JQuery/JavaScript together with HTML to build a dialogue panel. This could be used at the start of an RPG-style game to create the character, for example, and you can see what is possible [here](https://textadventures.co.uk/games/view/em15b32xd0o-y-ysvgrtcg/deeper).

This is not trivial, and some idea of CSS and HTML will be useful; it would be a good idea to have read through [part 1](ui-javascript.html) and [part 2](ui-javascript2.html) of customising the UI.

The way it will work is we will hand some HTML to JQuery and JQuery will put it in a dialogue. We will then need to collect the data and pass it to Quest.

There will be quite a bit of HTML and JavaScript code, and the neatest way to handle that is in its own file, so the first step is to create a text file in the sasme folder as your game, and to call in "dialogue.html".


Basic Dialogue Panel
--------------------

The first step is to create a snippet of HTML with all the widgets (a widget is a control such as a checkbox or textfield) you want on your dialogue panel. It all has to go instead a `div` element, with its own id and title, with the class set to "dialog_window". Here is a simple example:

```
<div id="dialog_window_1" class="dialog_window" title="Your Character">
  <table>
    <tr>
      <td colspan="2">Name: <input type="text" id="name_input" value="Skybird"/></td>
    </tr>
    <tr>
      <td>
        Sex: <input type="radio" name="sex_input" value="Male" checked="checked"/>Male
             <input type="radio" name="sex_input" value="Female" checked="checked"/>Female
      </td>
    </tr>
  </table>
</div>
```      

I have chosen to set out the widgets in a table, as this helps keep things neatly aligned. I have a single text field, and two radio buttons. How to code HTML tables and widgets is beyond the scope of this article, but there are plenty of resources on the internet.

It is a good idea to always give default values as it will stop the player leaving anything blank. This is complicated enough without checking for empty fields and then re-showing the dialogue panel!

To get the code into your game, add this to your game start script:

```
JS.addText (GetFileData("dialogue.html"))
```

If you start the game, you will see your widgets, but they are embedded in the page. We need JQuery to insert them into a dialogue panel. To do that, add this JavaScript code to the file:

```      
<script>
    function setValues() {
        $("#dialog_window_1").dialog("close");
    }

    $(document).ready(function () {
        $('#dialog_window_1').dialog({
           height: 220,
           width: 300,
           buttons: {
              "Done": function() { setValues();}
          }
        });
        $("button[title='Close']")[0].style.display = 'none';
    });
 </script>
```
There are two parts to this. The first part of that defines a function called `setValues`. At the  moment it just closes the dialogue box.

The other part puts the HTML into a dialogue box. I am not going deeply into JavaScript, but briefly the first line says we are defining a function that will be called when the document is loaded. The second line puts out HTML into a jQuery dialogue, using the `dialog` method. The next two lines obvious set the width and height of the dialogue (and you may well need to make these bigger for your dialogue panel). The next three lines define a block that adds buttons to it. Just one button here, called "Done", which will call the `setValues` function we defined before. The next line removes the "Close" button from the dialogue, ensuring the only way to get passed the dialogue is clicking the "Done" button (try deleting the line and see what it looks like to see the difference).

Save the file. Now if you go into the game, you will see the dialogue panel, and it will disappear when you click "Done".


Communicating with Quest
------------------------

The next step is to get the data into your game. This will be done with the special JavaScript function `ASLEvent`, which is provided by Quest. A complication here is that that can only take two parameters; the name of the Quest function to use, and a string. Either we need to use it numerous times, once for each value, or use it once but send it all the data in a single string. We will be doing the latter.

In the code above there was this function:

```
    function setValues() {
        $("#dialog_window_1").dialog("close");
    }
```

We need to change that to collect the data, and then to send it to Quest. You can get data from a form element with the JQuery `val` method. For text, it is trivial:

```
name = $('#name_input').val();
```

For the radio buttons, a bit more complicated:

```  
gender = $("input:radio[name='sex_input']:checked").val();
```

Both values need to be combined into a single string, separated by some obscure character; I use |. The new code looks like this:

```
    function setValues() {
        $("#dialog_window_1").dialog("close");
        answer = $('#name_input').val() + "|" + $("input:radio[name='sex_input']:checked").val();
        ASLEvent("HandleDialogue", answer);
    }
```

Then we need to create a function in Quest to accept that data. Add it in the normal way, and call it `HandleDialogue`, no return type, and a single parameter, s. Paste in this code:

```
l = Split(s, "|")
msg ("You are " + StringListItem(l, 0) + ", " + StringListItem(l, 1)) 
```

The first line splits the given string on the separator character, the second line just displays it. Obviously you could set attributes on the player object here if desired.


Disabling Other Input
---------------------

The dialogue box is not "modal", which means that the player can play your game whilst the dialogue box is still there. The best way around that is to turn off the command bar and panes on the right in the editor (_Interface_ tab of the game object), and turn them back on it the `HandleDialogue` function, so that is now:

```
JS.panesVisible(true)
JS.uiShow("#txtCommandDiv")
l = Split(s, "|")
msg ("You are " + StringListItem(l, 0) + ", " + StringListItem(l, 1)) 
```


Using the Web Editor
--------------------

All this _can_ be done with the web editor, but it will require a hack to get the file into your game. Rename the file "dialogue.png" (Quest will only let you upload certain file types). Go to the start script of the game object, and click to add a new script element. Select "Show picture". Click "Choose file", and navigate to the file. Click "Okay", and your file will be uploaded. You can then delete the "Show picture" script element.

You will need to edit the file name game start script too:

```
JS.addText (GetFileData("dialogue.png"))
```


In the [second part](ui-dialogue2.html) we will build on this to create a dialogue panel where the player can assign points to attributes.
