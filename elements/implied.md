---
layout: index
title: implied element
---

    <implied element="element" property="attribute name" type="type"/>

Specifies an implied type. For example, the "alt" attribute on an object is usually a list, so to save having to specify the type each time we can use this:

     <implied element="object" property="alt" type="list">

This means we can specify an alt attribute without specifying the type:

     <alt>telly; television</alt>
