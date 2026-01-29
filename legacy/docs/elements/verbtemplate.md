---
layout: index
title: verbtemplate element
---

    <verbtemplate name="name">text</template>

Creates or adds to a verb template of the specified name. Specifying multiple verb templates with the same name lets you handle multiple verbs with one template.

You can refer to verbtemplates within a [verb element](verb.html), or using the "template" attribute of a [command element](command.html).

The text can optionally include `#object#` as a stand-in for the object name; if it is omitted, the object name is assumed to be at the end. For example:

```
<verbtemplate name="wear">wear</verbtemplate>
<verbtemplate name="wear">put on</verbtemplate>
<verbtemplate name="wear">put #object# on</verbtemplate>
<verbtemplate name="wear">don</verbtemplate>
```
