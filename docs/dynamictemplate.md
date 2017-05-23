---
layout: index
title: DynamicTemplate
---

    DynamicTemplate (string  template name, object  object)

or

    DynamicTemplate (string  template name, string  text)

Returns a [string](../types/string.html) containing the requested text, based on the object or string passed in.

You can pass in multiple objects. If you only pass in one, you can refer to it using the variable "object". Otherwise you can use "object1", "object2" etc.

See [Using Templates](../guides/using_templates.html)

*Example:* We want to provide a templated message about a blocked exit.

First the dynamic template is defined as:

    <dynamictemplate name="BlockedExit">"Your exit "+object.alias+" is blocked"</dynamictemplate>

Now we could add a message expesssion to the script in an exit, something like:

    msg (DynamicTemplate("BlockedExit",this))

NOTE: As the script is defined in the *script* attribute of the *exit*, we use the "this" keyword to reference the current *exit* object

NOTE: This function is hard-coded and cannot be overridden.