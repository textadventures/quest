---
layout: index
title: implied element
---

    <implied element="element" property="attribute name" type="type"/>

Specifies an implied type. For example, the "alt" attribute on an object is usually a list, so to save having to specify the type each time we can use this:

     &lt;implied element="object" property="alt" type="list"/&gt;

This means we can specify an alt attribute without specifying the type:

     &lt;alt&gt;telly; television&lt;/alt&gt;
