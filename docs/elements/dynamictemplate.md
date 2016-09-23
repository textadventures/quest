---
layout: index
title: dynamictemplate element
---

    <dynamictemplate name="name">expression</template>

A dynamictemplate is used in a similar way as [template](template.html), except that its value is an expression, not a static string.

You can print a dynamic template using the [DynamicTemplate](../functions/dynamictemplate.html) function. This takes an object or text parameter, which is then passed in to the template expression.
