---
layout: index
title: Using Templates
---



`<template>` tag
-------------------

Specifies text that is printed, for example:
```
 <template name="UnresolvedObject">I can't see that.</template>
```

Using templates
------------------

You can use a template anywhere by surrounding the name with square brackets. The text will be replaced by the contents of the template.

For example, in Core.aslx in the definition for the **default** type we have:
```
 <displayverbs type="list">[LookAt];[Take];[SpeakTo]</displayverbs>
```

Which means that the default **displayverbs** is specified by the contents of the three templates named **LookAt**, **Take** and **SpeakTo**. So when the ASL file is loaded, this effectively becomes:
```
 <displayverbs type="list">Look at;Take;Speak to</displayverbs>
```

Dynamic Templates
----------------------

Templates are used for all the standard output from Core.aslx. This includes the standard error messages, such as "I don’t understand your command".

A dynamic template is a template that can call functions and read attributes. This is useful for dynamic responses such as when the player tries to take an untakeable object. Usually, a response of "You can’t take it" is sufficient, but what if the object is a female character? Dynamic templates to the rescue – they are essentially functions that return a string value, for example:
```
 <dynamictemplate name="TakeUnsuccessful">
   "You can't take " + object.article + "."
 </dynamictemplate>
```

A female character should have their "article" attribute set to "her", so this will print "You can’t take her."

To print this from a script, call the [DynamicTemplate](functions/dynamictemplate.html) function, which takes two parameters:
```
 msg (DynamicTemplate("TakeUnsuccessful", object))
```

How Quest Uses Templates
------------------------------

It is important to realise that templates and dynamic templates are handled very differently by Quest, although they are designed to do similar things. Templates are used when the aslx file is read, while dynamic templates are used when Quest is generating a response for the player.

The most important consequence of this is that if you are replacing existing templates, then you must do that before the core libraries are loaded. Consider this example:
```
  <include ref="English.aslx"/>
  <include ref="Core.aslx" />
  <template name="SeeListHeader">There's</template>
  <template name="GoListHeader"> Go to </template>
  <template name="UnrecognisedCommand">Unknown command.</template>
  <template name="YouAreIn"></template>
  <template name="PlacesObjectsLabel">Places / Objects</template>
 ```

First the standard language templates are loaded from English.aslx, then Core.aslx is loaded, and as it is loaded, all the template substitutions are made. Then a bunch of templates are defined... but too late! The substitutions have already been made.

Now try this:
```
  <include ref="English.aslx"/>
  <template name="SeeListHeader">There's</template>
  <template name="GoListHeader"> Go to </template>
  <template name="UnrecognisedCommand">Unknown command.</template>
  <template name="YouAreIn"></template>
  <template name="PlacesObjectsLabel">Places / Objects</template>
  <include ref="Core.aslx" />
```

First the standard language templates are loaded from English.aslx, but then the new templates are defined, overriding the standard ones. Now when Core.aslx is loaded, these new templates will get used for the template substitutions.

What this also means is that you can put templates into scripts and functions; Quest will do the text substitution when the file is loaded, so when the code is invoked, the proper text is in place.

It is also worth noting that you can call other functions inside a dynamic template, say to flag that a command completed successfully or to put in some formatting or whatever (as done with random default answers).

Printing Square Brackets
---------------------------

If text in square brackets gets automatically substituted, what do you do if you want square brackets in your output? As the output is HTML, you can use the HTML code, `&#91; `. Simple.

This in your code will give an open square bracket, `[`,  on output:
```
  &#91;
```

See also: [Random default answers](guides/random_default_answers)
