---
layout: index
title: Adding a Dialogue Panel That Assigns Points
---


This will build on the dialogue in the [first part](ui-dialogue.html). 

Let us suppose you want the player to set some numerical attributes by spending points, for example, the player has 10 points to spend between three attributes; magic, combat and social.

You need to add a row to your HTML table (in dialogue.html) for each attribute. Here is one row, for magic, you will need one of these for each attribute (they need to go before the `</table>` line):

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

We also want to show the points remaining, so add this too (again just before the `</table>` line):

```
           <tr>
            <td style="text-align:right;">
              Points left
            </td>
            <td>
              <div id="points" style="display:inline;">10</div>
            </td>
```

And we need to define those `intAtt` and `decAtt` functions. This needs to go inside the `<script>` element. Here is the former. It grabs the number of remaining points first, and if this is greater than 0, it increases the given stat and decreases the points remaining. You can add `decAtt` yourself!

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

We also need to update `setValues` to get these new values too. And `HandleDialogue`, to do something with the values (and you will need to use the Quest function `ToInt` to convert them from strings to integers). The dialogue will need to be bigger too. You are on your own for `HandleDialogue`, but here is the complete file:

```
 <div id="dialog_window_1" class="dialog_window" title="Your Character">
   <table>
     <tr>
      <td colspan="2">
        Name: <input type="text" id="name_input" value="Skybird"/>
      </td>
     </tr>
     <tr>
      <td colspan="2">
        Sex: <input type="radio" name="sex_input" value="Male" checked="checked"/>Male
        <input type="radio" name="sex_input" value="Female" checked="checked"/>Female
      </td>
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
             height: 360,
             width: 400,
             buttons: {
                "Done": function() { setValues();}
            }
          });
          $("button[title='Close']")[0].style.display = 'none';
      });
 </script>
