---
layout: index
title: dynamictemplate element
---

    <dynamictemplate name="name">expression</template>

A dynamictemplate is used in a similar way as [template](template.html), except that its value is an expression, not a static string. The expression will have access to an object called "object", which you can use to craft a response.

You can print a dynamic template using the [DynamicTemplate](../functions/dynamictemplate.html) function. This takes an object or text parameter, which is then passed in to the template expression.
