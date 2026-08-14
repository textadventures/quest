---
title: dynamictemplate element
sidebar:
  order: 5
---

    <dynamictemplate name="name">expression</template>

A dynamictemplate is used in a similar way as [template](/elements/template), except that its value is an expression, not a static string. The expression will have access to an object called "object", which you can use to craft a response.

You can print a dynamic template using the [DynamicTemplate](/functions/dynamictemplate) function. This takes an object or text parameter, which is then passed in to the template expression.
