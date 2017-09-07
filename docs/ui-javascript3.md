---
layout: index
title: Customising the UI - Part 3
---

We are going to build on the ideas in the previous pages to build a dialogue panel. This could be used at the start of an RPG-style game to create the character, for example. You can see what is possible [here](http://textadventures.co.uk/games/view/em15b32xd0o-y-ysvgrtcg/deeper).

## Basic Dialogue Panel

We can use the functionality of jQuery to make this _relatively_ easy. As before we will add an attribute to the game object that will contain the relevant HTML. In this case it will also have some JavaScript. Remember, this needs to be inserted into the game object in full code view.
```
      <div id="dialog_window_1" class="dialog_window" title="Your Character">
        <table style="-webkit-touch-callout: none; -webkit-user-select: none; -khtml-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none; user-select: none;">
          <tr class="details">
            <td colspan="2">Name: <input type="text" id="name_input" value="Skybird"/></td>
            <td>
              Sex: <input type="radio" name="sex_input" value="Male" checked="checked"/>Male
                   <input type="radio" name="sex_input" value="Female" checked="checked"/>Female
            </td>
          </tr>
        </table>
      </div>
      <script>
          function setValues() {
              $("#dialog_window_1").dialog("close");
          }

          $(document).ready(function () {
              $('#dialog_window_1').dialog({
                 height: 400,
                 width: 640,
                 buttons: {
                    "Done": function() { setValues();}
                }
              });
              $("button[title='Close']")[0].style.display = 'none';
          });
       </script>
```
There are two parts to this. The first is a `div` element containing a table. This is what will go inside the dialogue box. I am using a table because I think it looks neater when the form is aligned, but any HTML code can go in here. The various `input` elements are the bits in the form the player can fill in (knowledge of HTML will help here!).

The second part is the `script` element, where the JavaScript goes. The first part of that defines a function called `setValues`. At the  moment it just closes the dialogue box.

The other part puts the HTML into a dialogue box. I am not going deeply into JavaScript, but briefly the first line says we are defining a function that will be called when the document is loaded. The second line puts out HTML into a jQuery dialogue, using the `dialog` method. The next two lines obvious set the width and height of the dialogue. The next three lines define a block that adds buttons to it. Just one button here, called "Done", which will call the `setValues` function we defined before. The next line removes the "Close" button from the dialogue, ensuring the only way to get passed the dialogue is clicking the "Done" button (try deleting the line and see what it looks like to see the difference).

Now we need to add the content of the attribute to our game. In `game.start` do this:
```
  JS.addText (game.dialogue)
```
This should now work, sort of. You should see a dialogue box, and be able to fill it in, and so far nothing happens with them.

## Default Values?

I have given default values to the controls. This makes the code much simpler, as otherwise you have to check the player has entered a value for everything, and if not, you need to re-display the form, now with all he values she did put in.

It is faster when you are debugging too!

Obviously in your game you should put in your own defaults.

## Communicating with Quest

The next step is to get the data into your game. As with the buttons, this will be done with the special JavaScript function `ASLEvent`, which is provided by Quest. A complication here is that that can only take two paramters; the name of the Quest function to use, and a string. Either we need to use it numerous times, once for each value, or use it once but send it all the data in a single string. We will be doing the latter.

In the code above there was this function:
```
          function setValues() {
              $("#dialog_window_1").dialog("close");
          }
```
We need to change that to collect the data, and then to send it to Quest. You can get data from a form element with the jQuery `val` method. For text, it is trivial:
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

## Disabling Other Input

The dialogue box is not "modal", which means that the player can play your game whilst the dialogue box is still there. The best way around that is to turn off the command bar and panes on the right in the editor (_Interface_ tab of the game object), and turn them back on it the `HandleDialogue` function, so that is now:
```
request (Show, "Panes")
request (Show, "Command")
l = Split(s, "|")
msg ("You are " + StringListItem(l, 0) + ", " + StringListItem(l, 1)) 
```

## Spending Points

Let us suppose you want the player to set some numerical attributes by spending points, for example, the player has 10 points to spend between three attributes; magic, combat and social.

Add rows to your HTML table like this:
```
           <tr>
            <td style="text-align:right;">
              Magic
            </td>
            <td>
              <span onclick="incAtt('magic');" style="cursor: pointer;">&#x25B2;</span>
              <span onclick="decAtt('magic');" style="cursor: pointer;">&#x25BC;</span>
              <div id="magic" style="display:inline;">0</div>
            </td>
           </tr> 
```
The first line that start `span` will add an up arrow (the 25B2 is the code for that character), and when clicked, it will call a function called `intAtt`. The second `span` does that for the down arrow.

We also want to show the points remaining:
```
           <tr>
            <td style="text-align:right;">
              Points left
            </td>
            <td>
              <div id="points" style="display:inline;">10</div>
            </td>
```
And we need to define those `intAtt` and `decAtt` functions, inside the `<script>` element. Here is the former. It grabs the number of remaining points first, and if this is greater than 0, it increases the given stat and decreases the points remaining.
```
            function incAtt(att) {
                pts = parseInt($('#points').html());
                flag = (pts > 0);
                if (flag) {
                  n = parseInt($('#' + att).html());
                  $('#' + att).html(n + 1);
                  $('#points').html(pts - 1);
                }
            }
```
We also need to update `setValues` to get these new values too. And `HandleDialogue`, to do something with the values (and you will need to use the Quest function `ToInt` to convert them from strings to integers). You are on your own for `HandleDialogue`, but it is the complete game.dialogue attribute:
```
    <dialogue><![CDATA[
       <div id="dialog_window_1" class="dialog_window" title="Your Character">
         <table style="-webkit-touch-callout: none; -webkit-user-select: none; -khtml-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none; user-select: none;">
           <tr class="details">
            <td colspan="2">Name: <input type="text" id="name_input" value="Skybird"/></td>
            <td>Sex: <input type="radio" name="sex_input" value="Male" checked="checked"/>Male
                     <input type="radio" name="sex_input" value="Female" checked="checked"/>Female</td>
           </tr>
           <tr>
            <td style="text-align:right;" width="35%">
              <br/><b>Attributes</b>
            </td>
            <td width="15%">
            </td>
           </tr> 
           <tr>
            <td style="text-align:right;">
              Magic
            </td>
            <td>
              <span onclick="incAtt('magic');" style="cursor: pointer;">&#x25B2;</span>
              <span onclick="decAtt('magic');" style="cursor: pointer;">&#x25BC;</span>
              <div id="magic" style="display:inline;">0</div>
            </td>
           </tr> 
           <tr>
            <td style="text-align:right;">
              Combat
            </td>
            <td>
              <span onclick="incAtt('combat');" style="cursor: pointer;">&#x25B2;</span>
              <span onclick="decAtt('combat');" style="cursor: pointer;">&#x25BC;</span>
              <div id="combat" style="display:inline;">0</div>
            </td>
           </tr> 
           <tr>
            <td style="text-align:right;">
              Social
            </td>
            <td>
              <span onclick="incAtt('social');" style="cursor: pointer;">&#x25B2;</span>
              <span onclick="decAtt('social');" style="cursor: pointer;">&#x25BC;</span>
              <div id="social" style="display:inline;">0</div>
            </td>
           </tr> 
           <tr>
            <td style="text-align:right;">
              Points left
            </td>
            <td>
              <div id="points" style="display:inline;">10</div>
            </td>
         </table>
       </div>
       <script>
            function setValues() {
                $("#dialog_window_1").dialog("close");
                answer = $('#name_input').val() + "|" + $("input:radio[name='sex_input']:checked").val();
                answer += "|" + parseInt($('#magic').html()); 
                answer += "|" + parseInt($('#combat').html()); 
                answer += "|" + parseInt($('#social').html()); 
                ASLEvent("HandleDialogue", answer);
            }
            
            function incAtt(att) {
                pts = parseInt($('#points').html());
                flag = (pts > 0);
                if (flag) {
                  n = parseInt($('#' + att).html());
                  $('#' + att).html(n + 1);
                  $('#points').html(pts - 1);
                }
            }
       
            function decAtt(att) {
                n = parseInt($('#' + att).html());
                flag = (n > 0);
                if (flag) {
                  pts = parseInt($('#points').html());
                  $('#' + att).html(n - 1);
                  $('#points').html(pts + 1);
                }
            }
       
            $(document).ready(function () {
                $('#dialog_window_1').dialog({
                   height: 400,
                   width: 640,
                   buttons: {
                      "Done": function() { setValues();}
                  }
                });
                $("button[title='Close']")[0].style.display = 'none';
            });
       </script>
    ]]></dialogue>


Using the Web Editor
--------------------

All this can be done with the web editor, but it will require a hack to get the file into your game. Rename the file so it ends .png. Go to the start script of the game object, and click to add a new script element. Select "Show picture". Click "Choose file", and navigate to the file you renamed (Quest will only let you upload certain file types). Click "Okay", and your file will be uploaded. You can then delete the "Show picture" script element.

You will need to edit the file name in the `JS.addText` command as the file will end .png.
