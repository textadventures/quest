---
layout: index
title: Editor User Interface Elements
---

Much of the Editor user interface is defined by the core library itself, and the sub-libraries linked via CoreEditor.aslx. Look at the CoreEditor\*.aslx files for examples on creating a user interface for a library.

To add an Editor User Interface for a function defined in your library, add a section to your library like this:

     &lt;editor&gt;
        &lt;appliesto&gt;(function)EnableTimer&lt;/appliesto&gt;
        &lt;display&gt;Enable timer #0&lt;/display&gt;
        &lt;category&gt;Timers&lt;/category&gt;
        &lt;create&gt;EnableTimer ()&lt;/create&gt;
        &lt;add&gt;Enable timer&lt;/add&gt;
     
        &lt;control&gt;
          &lt;controltype&gt;label&lt;/controltype&gt;
          &lt;caption&gt;Enable timer&lt;/caption&gt;
        &lt;/control&gt;
     
        &lt;control&gt;
          &lt;controltype&gt;expression&lt;/controltype&gt;
          &lt;attribute&gt;0&lt;/attribute&gt;
          &lt;simple&gt;name&lt;/simple&gt;
          &lt;simpleeditor&gt;objects&lt;/simpleeditor&gt;
          &lt;source&gt;timer&lt;/source&gt;
        &lt;/control&gt;
     &lt;/editor&gt;

The example above adds the "Enable timer" command to the "Timers" category:

![](Editorui1.png "Editorui1.png")

When selected, the editor looks like this - the example above defines two controls, a label reading "Enable timer" and an expression control, which shows the "timer" dropdown:

![](Editorui2.png "Editorui2.png")

appliesto  
Should be of the format "(function)YourFunctionName"

display  
Text description of the command. "\#x" represents the value of parameter x.

category  
The category to use in the script adder

create  
Blank version of the command created when the user selects this command in the adder

add  
Description of the command to display in the script adder

control  
Multiple <control> elements specify labels and input parameters.

Within a <control> XML tag, you must specify a controltype from the list below. If the control corresponds to a parameter (and it usually will, except for a label), you must also use an <attribute> tag to specify which parameter.

The following controltypes can be used:

-   checkbox
-   dropdown
-   dropdowntypes
-   elementslist
-   expression
-   file
-   filter
-   label
-   list
-   multi
-   number
-   objects
-   script
-   scriptdictionary
-   scriptexpander
-   stringdictionary
-   textbox

The following additional controltypes are also available, but are probably not much use in extension libraries:

-   attributes
-   exits
-   richtext
-   pattern
-   texteditor
-   title
-   verbs

