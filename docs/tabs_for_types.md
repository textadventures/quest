---
layout: index
title: Using Tabs for Types
---

<div class="alert alert-info">
Note: Inherited types and tabs can currently only be edited in the Windows desktop version of Quest.
</div>

So you know all about types, from the [Using Types](using_inherited_types.html) page, right? Let us do some more. In this tutorial we will add a new tab to an object so we can easily edit attributes of objects of that type.

Some familiarity with XML (or HTML) will be useful!

This is great if you want to share your library as it makes it much easier for other people to include objects of your type - it is all done through a dedicated tab. If can be useful for you too; if you have a dozen or so of the same type of thing, it may be easier to set up a tab, and then modify the attributes there.


Use a library
-------------

If we are going to get serious, it is better to put your types in another file (and personally, I would put verbs, commands, functions, turnscripts and templates all in there too), so we will do that first.

One reason to use a library is that Quest will not save your tabs, so if you add them to the main game they will disappear!

Quest cannot edit library files, so open up in a text editor like Notepad++. Your basic library has a start tag and an end tag. I also put in an XML directive first, so I can use an online XML validator on it (such as [this](http://validator.w3.org/#validate_by_input)), in case I have messed up the code so much Quest cannot handle it. The basic framework looks like this:

      <?xml version="1.0"?>
      <library>
      </library>

Go into the code in Quest, and cut the type from there, and paste it into your library (if you are following this as a tutorial from the previous page, you will see that I have improved the text here a little).

      <?xml version="1.0"?>
      <library>
        <type name="spell">
          <inventoryverbs type="list">Cast</inventoryverbs>
          <displayverbs type="list">Learn</displayverbs>
          <drop type="boolean">false</drop>
          <take type="boolean">false</take>
          <usedefaultprefix type="boolean">false</usedefaultprefix>
          <learn type="script"><![CDATA[
            if (not this.parent = player) {
              this.parent = player
              msg ("In a process that seems at once unfathomable, and yet familiar, the spell fades away, and you realise you are now able to cast the <i>" + this.alias + "</i> spell.")
            }
            else {
              msg ("Er, you already know that one!")
            }
          ]]></learn>
        </type>
      </library>

It is vital that you cut-and-paste everything from the start tag to the end tag, including both tags. Save this file as "library.aslx" in the same directory as your game.

In your game file in Quest, you need an extra line at the top of the code, telling Quest to include your library. The top five lines will be like this, with your library after the two standard libraries (the numbers in the first two lines may be different if you are using a different version).

      <!--Saved by Quest 5.7.6404.15496-->
      <asl version="550">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <include ref="Library.aslx" />
        ...
     
Now go into the game, and check it still works.


More Types
----------

We will now set up a new type, attackspell. The attackspell is a particular type of spell, so it needs all the properties of an ordinary spell, but perhaps some new ones too. So it wants to *inherit* those properties from spell. Here, then, is our new type:

      <type name="attackspell">
        <inherit name="spell"/>
      </type>


A Simple Tab
------------

Let us suppose attack spells will use different elements and have different power ratings. The way to do that is to have attributes on each spell, "powerrating" and "element". The cool way to do *that*, is to create new tabs in the editor.

Here is the basic code (paste this in before the </library> tag).

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>Spell</caption>
        <mustnotinherit>editor_room; defaultplayer</mustnotinherit>
        
        <control>
          <controltype>dropdowntypes</controltype>
          <caption>Spell type</caption>
          <types>*=Not a spell; spell=Non-attack spell; attackspell=Attack spell</types>
          <width>150</width>
        </control>
      </tab>

So what do we see here? It starts and ends with `<tab>` and `</tab>`, so Quest knows this is a tab. The `parent` element tells Quest this is an editor for an object (I think they all are). The `caption` is the name on the tab, and `mustnotinherit` stops this tab appearing for rooms and the player (note these types are separated by semi-colons).

Then there is our control, and you can have several of these. This one gives a dropdown menu, allowing the user to select whether this is not spell, is a non-attack spell, or is an attack spell. The asterisk indicates the null choice, by the way.

Quest will not realise you have changed your library file; to see a difference, you will need to close your game (from the _File_ menu - no need to exit Quest completely), then open it again. Hopefully, if you click on an object you will see a new tab called "Spell"!


More Controls
-------------

Now we will put in a powerrating control. Try this (remember, it has to go before the </tag> line, because that marks the end of the tag):

        <control>
          <controltype>number</controltype>
          <caption>Power of attack (1-10)</caption>
          <attribute>powerrating</attribute>
          <width>100</width>
          <mustinherit>attackspell</mustinherit>
          <minimum>0</minimum>
          <maximum>10</maximum>
        </control>

The `controltype` says this is for an integer number, the `caption` is again the label the user will see, the `attribute` is what is being set, and `width`, of course, is the size (not sure if it actually gets used). By setting `mustinherit`, this is only visible for attackspells. Also, `minimum` and `maximum` values are set (but these are only for this tab, the values can change outside the tab by editing the attribute directly or during play, so do not rely on them being in that range).

Now here is a text box, which is pretty straightforward. We can use this for descriptions of what happens.

        <control>
          <controltype>textbox</controltype>
          <caption>Description</caption>
          <attribute>description</attribute>
          <mustinherit>spell</mustinherit>
        </control>

Note that this must inherit from spell, so will be visible for both types of spells (as attack spells inherit from spell).

We also want to be able to select an element (fire for fireball, etc). Now we can create the dropdown list just as we did before (inside the "tab" tags):

      <control>
        <controltype>dropdown</controltype>
        <caption>Element</caption>
        <attribute>category</attribute>
        <validvalues type="simplestringlist">none;Fire;Frost;Storm</validvalues>
        <mustinherit>attackspell</mustinherit>
      </control>
      

Other Options
-------------

Now you know the basics, you will want to know what other options are available.

### Conditional Display

Often, a tab is only applicable to certain types, or it not applicable to certain types, and you can help keep the GUI tidy by having your tab only displayed when relevant. In this example, the tab will only be shown for objects with the "container" type:

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>My new tab</caption>
        <mustinherit>container</mustinherit>
      </tab>

This tab will _not_ be be shown for rooms and the player object.

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>My new tab</caption>
        <mustnotinherit>editor_room; defaultplayer</mustnotinherit>
      </tab>

As we saw earlier, the same "mustinherit" and "mustnotinherit" elements can be used inside the controls themselves. This way the user can select the object to be a "container", and the controls relevant to that type will suddenly appear on the tab.

You can also control what is displayed based on the value of an attribute, using `onlydisplayif`. This takes a condition expressed in Quest code. Here are some examples from the core library.

This will display if the "feature_annotations" attribute of the game object is true:

    <onlydisplayif>game.feature_annotations</onlydisplayif>

This displays if the "lookonly" attribute of this object is _not_ true:

    <onlydisplayif>not this.lookonly</onlydisplayif>
    
These next three illustrate how you can use any code that you might put inside an `if` condition:

    <onlydisplayif>game.pov = null</onlydisplayif>

    <onlydisplayif>GetInt(this, "keycount") > 2</onlydisplayif>

    <onlydisplayif>(GetBoolean(this, "locked") or not GetBoolean(this, "visible"))</onlydisplayif>


### Text controls

There are several types of controls you can put on your tabs, the simplest are text controls.

          <control>
            <controltype>title</controltype>
            <caption>Colour</caption>
          </control>

          <control>
            <controltype>label</controltype>
            <caption>You can use any valid HTML colour name</caption>
          </control>

The `controltype` element tells Quest what type of control you want, the `caption` tab puts text on the page. Both `controltype` and `caption` should be present in all your controls.


### Basic Controls

These five examples show how to add controls for attributes that are Booleans, integers, strings, objects and scripts respectively.

          <control>
            <controltype>checkbox</controltype>
            <caption>Underline hyperlinks</caption>
            <attribute>underlinehyperlinks</attribute>
          </control>

          <control>
            <controltype>number</controltype>
            <caption>Font size</caption>
            <attribute>menufontsize</attribute>
          </control>

          <control>
            <controltype>textbox</controltype>
            <caption>Version</caption>
            <attribute>version</attribute>
          </control>

          <control>
            <controltype>objects</controltype>
            <caption>Key</caption>
            <attribute>key</attribute>
         </control>

          <control>
            <controltype>script</controltype>
            <caption>Start script</caption>
            <attribute>start</attribute>
          </control>

There is a new element, `attribute`, and this contains the name of the attribute that will be set. It is generally a good idea to set a default value on your type by the way.


### Not So Basic

This one will give a string, but the type is "richtext", allowing the user to format the string (this also has the "expand" element, so the text area will expand to fill the tab).

          <control>
            <controltype>richtext</controltype>
            <caption>Description</caption>
            <attribute>description</attribute>
            <expand/>
          </control>

For a string list, use the "list" control. You should also add an "editprompt" element, this is the text the user will see as each item is added.

          <control>
            <controltype>list</controltype>
            <caption>Parameters</caption>
            <attribute>paramnames</attribute>
            <editprompt>Please enter an parameter name</editprompt>
          </control>

For a stringdictionary, you need two prompts, like this:

          <control>
            <controltype>stringdictionary</controltype>
            <caption>Status attributes</caption>
            <keyprompt>Please enter the attribute name</keyprompt>
            <valueprompt>Please enter the format string (blank for default)</valueprompt>
            <attribute>statusattributes</attribute>
          </control>


### Dropdown lists

You can add drop-down lists. There are two types, the first looks like this:

          <control>
            <controltype>dropdown</controltype>
            <caption>Category</caption>
            <attribute>category</attribute>
            <validvalues type="simplestringlist">Comedy;Educational;Fantasy;Historical</validvalues>
            <freetext/>
          </control>

The "validvalues" obviously supplies the list the user can pick from. The "freetext" element tells Quest that the user can also just type in a value.

The second type of drop-down is for selecting the type for an object. Here is an example:

        <control>
          <controltype>dropdowntypes</controltype>
          <caption>Container type</caption>
          <types>*=Not a container; container_open=Container; container_closed=Closed container</types>
        </control>

So now the control type is "dropdowntypes", and the different types are listed in the "types" element in the form of a string dictionary. Note that \* is used to denote no selection (and this must be present); all other types ("container\_open" and "container\_closed" in this case) must be defined elsewhere in your game somewhere (and ideally in this same library). Also note there is no attribute element here.


### The multi type

Sometimes you want to allow the user to decide what type the attribute will be. Use the multi control. As well as the usual elements, you also need a "types" element, a string dictionary that sets up the types:

          <control>
            <caption>Look</caption>
            <controltype>multi</controltype>
            <attribute>look</attribute>
            <types>
              null=No description; string=Text; script=Run script
            </types>
            <editors>
              string=textbox
            </editors>     
          </control>

The types here are "null" (no attribute to be set), "string" and "script". Note that for the string option an editor is specified. I think "textbox" is actually the default, so is not required here; "richtext" is an alternative.

Here is another example, this has boolean as one type, and the associated checkbox is set up as well.

        <control>
          <controltype>multi</controltype>
          <caption>Take</caption>
          <attribute>take</attribute>
          <types>
            boolean=Default behaviour; script=Run script
          </types>
          <checkbox>Object can be taken</checkbox>
        </control>

You can also use "stringlist".

Further examples, for completeness:

        <control>
          <controltype>multi</controltype>
          <selfcaption>Action</selfcaption>
          <attribute>useon</attribute>
          <types>
            null=None;scriptdictionary=Handle objects individually;string=Print a message
          </types>
          <keyname>Object</keyname>
          <keyprompt>Please enter the object name</keyprompt>
          <source>object</source>
        </control>

        <control>
          <controltype>multi</controltype>
          <caption>Pattern</caption>
          <attribute>pattern</attribute>
          <types>
            simplepattern=Command pattern; string=Regular expression
          </types>
        </control>


### Element lists

These seem to add a new child object to the object, rather than an attribute. Here are some examples:

        <control>
          <controltype>elementslist</controltype>
          <elementtype>object</elementtype>
          <objecttype>object</objecttype>
          <expand/>
        </control>

        <control>
          <caption>Turn scripts - run after every turn the player takes in this room</caption>
          <controltype>elementslist</controltype>
          <elementtype>object</elementtype>
          <objecttype>turnscript</objecttype>
        </control>

        <control>
          <caption>Commands</caption>
          <controltype>elementslist</controltype>
          <elementtype>object</elementtype>
          <objecttype>command</objecttype>
        </control>

Probably not so useful for your custom library - easier to create the object through the GUI normally.


### For Completeness

A couple more examples you will probably never need to see.

          <control>
            <controltype>attributes</controltype>
            <expand/>
          </control>

          <control>
            <controltype>gameid</controltype>
            <caption>Game ID</caption>
            <attribute>gameid</attribute>
            <advanced/>
            <desktop/>
          </control>
