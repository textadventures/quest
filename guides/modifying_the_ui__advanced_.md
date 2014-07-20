---
layout: index
title: Modifying the UI (Advanced)
---

As of Quest 5.3, there is a lot you can do to customise the player's user interface (UI).

Look at the "Display" tab of the *game* object to see how to set a font and basic colours, including hyperlinks and hyperlink menus. You can also turn off the panes on the right here. On the "Room Descriptions" tab you can also turn off hyperlinks in the text. What if you want to do more? Well, you can modify the entire colour scheme if you want, though it is a little more complicated.

This screenshot is from the example game listed at the end of this page to give an idea of what is possible.

![](Styles5.png "Styles5.png")

The Quest UI is HTML, just like any web page, and this means you can change the style of each component. If you are playing a Quest game, try right-clicking on it; you will see an option "view source". Select it and you will see the HTML code used to create what you can see. It will help if you know a little HTML and CSS at this point...

HTML
----

HTML is a kind of computer language that tells a browser how to display a page. It uses *tags* to denote *mark-up*, and these tags give instructions on the style to use. Here is a simple example:

      This sentence uses &lt;i>italic&lt;/i> text.

The word "italic" will be displayed in italic. As you can see, tags start with a *less than* symbol, and end with a *greater than* symbols (also know as angle brackets). You need tags at the start and the end, and the end tag also needs a slash. The "i" indicates italics. The start tag, the end tag and the text between are known as an element, so in the example we have an italic element.

The basic HTML document is a declaration and an "html" element, and the "html" element contains a "head" element, and a "body" element", and they in turn contain other elements. Something like this:

      <!DOCTYPE html><html><head>
      ...
      </head>
      <body>
      ...
      </body></html>

The head element contains JavaScript code, which we can ignore completely. The body element contains the actual content, and we need to pick out from that the various elements displayed on the screen.

CSS
---

CSS is the way to control how an HTML element is displayed. By default, the italic element causes text to be displayed in italics, but using CSS you can change that. In practice it is better not to, instead, we commonly use *div* and *span* elements instead, as it is less confusing, and change them. The *div* element is used for blocks of text, the ''span' element for sections of text within a paragraph. So to put text in italics using CSS, first define the CSS like this:

      span {
        font-style: italic;
      }

Then use it like this:

      This sentence uses &lt;span>italic&lt;/span> text.

That will make all the span elements display as italic. You might want just some to, so you give the span element a class or id (an id is used for a specific span; there can only be one element with that id).

      .bld {
        font-style: bold;
      }
      #ital {
        font-style: italic;
      }

Then use it like this:

      This sentence uses &lt;span id="ital">italic</span> and &lt;span class="bld">bold</span>text.

Note that a class name uses a dot, and an id uses a hash in the CSS. After the element name, you need a pair of curly braces to enclose the style information. The styles are defined by the name of the attribute, a colon, the value to assign to it, and a semi-colon.

Insert HTML
-----------

If you look at the Quest source HTML, you will see a huge number of HTML elements with class or id set on them. All we need to do is work out which we want to modify, and change it.

The way to change it is to insert our CSS styles into the HTML code. To do that, we first need to create our own HTML file, inside the same folder as your game. We can call it test.html (but you can give it any name, as long as it ends .html). It should look like this:

      <!DOCTYPE html>
      <html>
      <head>
      <style type="text/css">
        body {
          background-color: blue;
        }
      </style>
      </head>
      </html>

All this will do is to change background colour of the body element to be blue. You now need to tell Quest to use the file, and that is done through a startup script. Set it up like this:

![](Styles2.png "Styles2.png")

Or in code view:

        <start type="script">
          insert ("test.html")
        </start>

Play the game, and you should see a blue background (if you cannot make sure your Quest window is wide enough to see the edges).

You can experiment with different colours, a full list of those available can be found [here](http://en.wikipedia.org/wiki/Web_colors).

You can also the standard HTML notation, so \#0000FF for blue, for example.

Changing Other Elements
-----------------------

You can potentially change the colour of all sorts of thing in the UI, you just need to know the element name, class or id. Here are some examples:

      #location: This is the title bar at the top that has the room name in it.
      #gameContent: Where the text goes, including the input box.
      #divOutput: Inside gameContent, where the text goes, but excluding the input box,
      #txtCommand: The input box.
      .ui-widget: Border around each pane on the right
      #gamePanesFinished: This is the pane on the right shown when the game ends
      #msgbox, #msgboxCaption: For pop-ups.
      #dialog, #dialogCaption, #dialogOptions: For menu pop-ups.

As well as the background, you can set foreground colour (with the *color* attribute) and font too, among other things.

To remove the focus box on the input box, do this:

      *:focus {
        outline: none;
      }

Notes
-----

Be aware that HTML and CSS spell colour as "color".

Be careful what fonts you specify. If you use a web font in Quest, then you can be pretty sure that that one is on the players computer (I do not know how to get Quest to download more than one web font, though I imagine it is possible). There is no guarantee that any other font will be available; even a basic font like *Arial* is not on an Apple Mac, where it is called in *Helvetica*. You can, however, specify a list of fonts, and Quest will use the first one that is available (see, for example, [here](http://www.w3schools.com/cssref/css_websafe_fonts.asp).

The rounded input box in the image about was achieved using the border-radius attribute. Unfortunately, this does not work on the box containing the input box and output text, I think because other parts of the display are covering it up, and only the bottom left corner is rounded.

Although you can use background images in HTML/CSS, this is not readily feasible in Quest through JavaScipt, because of the way the interface references other files (i.e., the image file). If you want a background image (round the outside only), set it in the Quest UI, and make sure the *body* element is not changed. You can set other components to be transparent so the background shows through. That said, the *status* component (at the top, where the room name appears) adopts a blue wave image as its background when transparent, and the only way to get rid of it is to set the *status* component's *display* attribute to *none* - which means the room name is not then displayed.

Using JavaScript
----------------

Certain attributes seem to be hard coded in, such as the background colour for *gameBorder* and *gamePanes*, and the alignment of the output text. What we can do, however, is run a JavaScript function to override those settings.

JavaScript goes in its own file, let us call it test.js. Here is some example text to put in it.

      function setUI(){ 
        document.getElementById("gameBorder").setAttribute("style" , "background: red;");
        document.getElementById("gamePanes").setAttribute("style" , "background: yellow;");
      }

In your game, you need to tell quest to load that file:

![](Styles3.png "Styles3.png")

Or in code:

      <javascript src="test.js" />

Then you need to invoke the JavaScript function in your start-up script:

![](Styles4.png "Styles4.png")

Or in code:

      request (RunScript, "setUI")

So hopefully you now see big blocks of red and yellow in your game.

Let us see that JavaScript in detail now. Here is a slightly more advanced example:

      function setUI(){ 
        document.getElementById("gameBorder").setAttribute("style" , "background: blue; border: blue;");
        document.getElementById("gamePanes").setAttribute("style" , "background: blue;");
      }

The two important lines in the middle change two different HTML elements, *gameBorder* and *gamePanes*. Note that this technique will only work for elements with an id (you can use JavaScript to change other elements, but it gets complicated, and well beyond this discussion). For those elements we are modifying the style, and the bit in quotes at the end is what you want to change. This is in the same format as the CSS inside the curly braces earlier. You can put in as much style information in there as you want; you can see the first changes the background and the border.

This line of JavaScript will change the prompt:

      document.getElementById("txtCommand").setAttribute("placeholder", "So now what?");

Note that while the HTML/CSS method adds style information to an element, the JavaScript method replaces the existing style information. If we do this:

        document.getElementById("gameBorder").setAttribute("style" , "background: blue;");
        document.getElementById("gameBorder").setAttribute("style" , "border: blue;");

... then the first instruction gets overwritten by the second, and will have no effect.

Changing on the Fly
-------------------

You can call a JavaScript function at any point in the game, which means you can change the UI colour scheme as many times as you want during the course of play. I do suggest some restraint though!

Here is a full example. You will need to copy and paste the code into their respective files (this Wiki does not allow these file types to be uploaded). This is the *colors.js* file content. The function *setUI* is called from Quest, and accepts four parameters; this then calls the other functions to set all the style attributes.

      function setUI(font, panel, back, text){ 
        setComponentBackground(panel);
        setBackground(back);
        setLocation(panel, font);
        setInput(panel, font, 14);
        setPanes(text, font, 14);
      }
      function setBackground(c){ 
        document.getElementById("location").setAttribute("style" , "background: " + c + "; border: " + c + "; ");
        document.getElementById("gameBorder").setAttribute("style" , "background: " + c + "; border: " + c + "; ");
        document.getElementById("gamePanes").setAttribute("style" , "background: " + c + "; ");
        document.getElementById("status").setAttribute("style" , "background: " + c + "; border: darkblue; ");
        document.getElementById("gamePanesRunning").setAttribute("style" , "background: " + c + "; border: none; ");
        document.body.setAttribute("style" , "background: " + c + "; ");
      }
      function setComponentBackground(c){ 
        document.getElementById("gameContent").setAttribute("style" , "background: " + c + "; ");
        document.getElementById("gamePanel").setAttribute("style" , "background: " + c + "; ");
        document.getElementById("gridPanel").setAttribute("style" , "background: " + c + "; ");
        document.getElementById("divOutput").setAttribute("style" , "background: " + c + ";");
        document.getElementById("inventoryLabel").setAttribute("style" , "background: " + c + ";");
        document.getElementById("statusVarsLabel").setAttribute("style" , "background: " + c + ";");
        document.getElementById("placesObjectsLabel").setAttribute("style" , "background: " + c + ";");
        document.getElementById("compassLabel").setAttribute("style" , "background: " + c + ";");
      }
      function setLocation(c, font, size){ 
        document.getElementById("location").setAttribute("style" , "font-family: " + font + "; font-size: 18px; color: " + c + "; padding: 1px; ");
      }
      function setInput(c, font, size){ 
        document.getElementById("txtCommand").setAttribute("style" , "font-family: " + font + "; font-size: " + size + "px; color: " + c + "; padding: 3px; border-radius: 15px; ");
      }
      function setPanes(c, font, size){ 
        var s = "font-family: " + font + "; font-size: " + size + "px; color: " + c + "; "
        document.getElementById("lstInventory").setAttribute("style" , s);
        document.getElementById("lstPlacesObjects").setAttribute("style" , s);
        document.getElementById("statusVars").setAttribute("style" , s);
        for (var i = 1; i < 10; i++) {
          document.getElementById("cmdPlacesObjects"+i).setAttribute("style" , s);
          document.getElementById("cmdInventory"+i).setAttribute("style" , s);
        }
        s = "font-family: " + font + "; font-size: " + size + "px; color: white; "
        document.getElementById("cmdCompassIn").setAttribute("style" , s);
        document.getElementById("cmdCompassOut").setAttribute("style" , s);
        s = document.getElementById("inventoryLabel").getAttribute("style") + " font-family: " + font + "; font-size: " + size + "px;";
        document.getElementById("inventoryLabel").setAttribute("style" , s);
        document.getElementById("statusVarsLabel").setAttribute("style" , s);
        document.getElementById("placesObjectsLabel").setAttribute("style" , s);
        document.getElementById("compassLabel").setAttribute("style" , s);
      }

The HTML file, *nofocus.html*. All the wok is done in the JavaScript file; the only thing I could not work out how to do there was to stop the border on the input box when it has the focus.

      <!DOCTYPE html>
      <html>
      <head>
       <style type="text/css">
      *:focus {
          outline: none;
      }
      </style>
      </head>
      </html>

And the Quest game code. You can use this to experiment with different colour schemes of your own. If you do not want hanging colour schemes, you just need to copy the startup script and the JavaScript import (the lines in bold) into your own game.

      <!--Saved by Quest 5.3.4750.31396-->
      <asl version="530">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <game name="test">
          <gameid>39fea4b4-586a-4c72-9404-501f5f3a0c18</gameid>
          <version>1.0</version>
          <firstpublished>2012</firstpublished>
          <defaultwebfont>Calligraffitti</defaultwebfont>
          <defaultfontsize type="int">12</defaultfontsize>
          <defaultfont>'Arial Black', Gadget, sans-serif</defaultfont>
          <showscore />
          <showhealth />
          <showpanes />
          <setbackgroundopacity type="boolean">false</setbackgroundopacity>
          <defaultbackground>Silver</defaultbackground>
          <defaultlinkforeground>LightPink</defaultlinkforeground>
          <echohyperlinks type="boolean">false</echohyperlinks>
          <enablehyperlinks type="boolean">false</enablehyperlinks>
          '''<start type="script">'''
            '''insert ("nofocus.html")'''
            '''request (RunScript, "setUI;" + game.defaultwebfont + ";LightSteelBlue;DarkSlateGrey;DarkSlateGrey")'''
          '''</start>'''
        </game>
        <object name="room">
          <inherit name="editor_room" />
          <description>The first room of the game. Type a colour to change the UI.</description>
          <alias>First Room</alias>
          <object name="player">
            <inherit name="editor_object" />
            <inherit name="editor_player" />
            <object name="aaa">
              <inherit name="editor_object" />
              <use />
            </object>
          </object>
          <object name="ppp">
            <inherit name="editor_object" />
          </object>
          <object name="ooo">
            <inherit name="editor_object" />
          </object>
          <exit alias="east" to="room2">
            <inherit name="eastdirection" />
          </exit>
        </object>
        <object name="room2">
          <inherit name="editor_room" />
          <description>The second room</description>
          <alias>Room 2</alias>
          <exit alias="west" to="room">
            <inherit name="westdirection" />
          </exit>
        </object>
        <command name="blue">
          <pattern>blue</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";LightSteelBlue;DarkSlateGrey;DarkSlateGrey")
            msg ("I'm feelin' blue...")
          </script>
        </command>
        <command name="yellow">
          <pattern>yellow</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";LemonChiffon;Khaki;Tan")
            msg ("Ah, mellow yellow")
          </script>
        </command>
        <command name="green">
          <pattern>green</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";SpringGreen;DarkOliveGreen;DarkOliveGreen")
            msg ("Hey, everything green, man!")
          </script>
        </command>
        <command name="red">
          <pattern>red</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";Salmon;DarkRed;DarkRed")
            msg ("It's all red")
          </script>
        </command>
        <command name="white">
          <pattern>white</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";WhiteSmoke;AntiqueWhite;Tan")
            msg ("It's so bright")
          </script>
        </command>
        <command name="black">
          <pattern>black</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";Indigo;Black;Indigo")
            msg ("It's all dark")
          </script>
        </command>
        <command name="gray">
          <pattern>gray;grey</pattern>
          <script>
            request (RunScript, "setUI;" + game.defaultwebfont + ";LightSlateGray;DarkSlateGray;SteelBlue")
            msg ("It's all so boring")
          </script>
        </command>
        '''<javascript src="colours.js" />'''
      </asl>

Play the game and type some colours at the prompt!
