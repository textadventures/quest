---
layout: index
title: More on Tabs (Advanced)
---

This page describes how to create a new tab in the Quest GUI. The concept of tabs was introduced on this page, and it might be a good idea to read that first to understand why they are useful:

[Using Types and Tabs (Advanced)](using_types_and_tabs__advanced_.html)

Adding Tabs
-----------

To add a new tab, put a tab element into your library. The basic format looks like this:

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>My new tab</caption>
      </tab>

This will give a new blank tab for all object types.

Conditional Display
-------------------

Often, a tab is only applicable to certain types, or it not applicable to certain types, and you can help keep the GUI tidy by having your tab only displayed when relevant. This tab will only be shown for objects with the "container" type:

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>My new tab</caption>
        <mustinherit>container</mustinherit>
      </tab>

This tab will not be be shown for rooms and the player object.

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>My new tab</caption>
        <mustnotinherit>editor_room; defaultplayer</mustnotinherit>
      </tab>

The same "mustinherit" and "mustnotinherit" elements can be used inside the controls themselves. This way the user can select the object to be a "container", and the controls relevant to that type will suddenly appear on the tab.

Text controls
-------------

Blank tabs are not much use, let us put something on it. Things are added to a tab using the "control" element, you can have as many as you like, and they appear in the order you add them. Before getting to controls that do stuff, we will put text on the tab. Two control types are available: title and label. Here are examples of both:

          <control>
            <controltype>title</controltype>
            <caption>Colour</caption>
          </control>

          <control>
            <controltype>label</controltype>
            <caption>You can use any valid HTML colour name</caption>
          </control>

The controltype element tells Quest what type of control you want, the caption tab puts text on the page. Both of these should be present in all your controls.

Basic Controls
--------------

These three examples show how to add controls for attributes that are Booleans, integers and strings respectively.

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

The controltype determines what the user sees, and dictates the type of the attribute, so a checkbox will give a Boolean attribute. There is a new element, "attribute", and this contains the name of the attribute.

Not So Basic
------------

Here is a control for a script. Actually no more complicated than the last three.

          <control>
            <controltype>script</controltype>
            <caption>Start script</caption>
            <attribute>start</attribute>
          </control>

This one will give a string, but the type is "richtext", allowing the user to format the string (this also has the "expand" element; what does that do?).

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

The "objects" control allows the user to pick an existing object from the game as an attribute:

        <control>
          <controltype>objects</controltype>
          <caption>Key</caption>
          <attribute>key</attribute>
        </control>

Dropdown lists
--------------

You can add drop-down lists. There are two types, the first looks like this:

          <control>
            <controltype>dropdown</controltype>
            <caption>Category</caption>
            <attribute>category</attribute>
            <validvalues type="simplestringlist">Comedy;Educational;Fantasy;Historical</validvalues>
            <freetext/>
          </control>

The "validvalues" obviously supplies the list the user can pick from. The "freetext" element tells Quest that the user can also just type in a value. For fonts, you should assign a source. The "basefonts" gives the built-in fonts, or "webfonts" for web fonts:

          <control>
            <controltype>dropdown</controltype>
            <caption>Font</caption>
            <attribute>defaultfont</attribute>
            <source>basefonts</source>
            <freetext/>
          </control>

The second type of drop-down is for selecting the type for an object. Here is an example:

        <control>
          <controltype>dropdowntypes</controltype>
          <caption>Container type</caption>
          <types>*=Not a container; container_open=Container; container_closed=Closed container</types>
        </control>

So now the control type is "dropdowntypes", and the different types are listed in the "types" element in the form of a string dictionary. Note that \* is used to denote no selection (and this must be present); all other types ("container\_open" and "container\_closed" in this case) must be defined elsewhere in your game somewhere (and ideally in this same library). Also note there is no attribute element here.

The multi type
--------------

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

Element lists
-------------

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

For Completeness
----------------

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
