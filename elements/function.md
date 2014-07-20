---
layout: index
title: function element
---

    <function name="name"optional type="type"optional parameters="parameters">script</function>

Creates a function.

If no type is specified, the function does not return a value.

If the function does return a value, the type should be one of the valid [Attribute Types](../types/). Return a value within the function using the [return](../scripts/return.html) command.

If the function takes parameters, the parameters should be specified as a comma-delimited list.

For example:

    &lt;function name="FormatObjectList" type="string" parameters="preList, parent, preFinal, postList"&gt;
    <pre>  ...

\</function\>

</pre>
### The attributes of a function

**name:** This is the name of the function. Every function must have name, and that is the name you use to invoke the function in some other script.

**parameters:** These are the values (if any) passed into the function. You give them names, and when the function is called, those parameters must be set by giving values in the function call (e.g. MyFunction(a, b) ). The values are mapped to the parameters in the order they are given. If your function does not take input parameters, then you can omit this or leave it as an empty string.

**type:** This is the return type of the function (the value passed out), if the function returns a value. Some functions do, and some don't. If you use a "[return](../scripts/return.html)" statement in your function to send a value back to the caller, then you need to specify the return type, so that Quest knows what type the function is expected to return. If your function does not return a value, then you can omit this or leave it an empty string.

Quest will object if there is a return statement, but no type specified; or if there is a type specified, but no return statement.

### A Working Example

Here is a trivial example. It's a function to concatenate two strings and return the result. Clearly, you don't need this function (since you can just use the "+" yourself), but hopefully it illustrates how functions are set up.

        <function name="ConcatStrings" parameters="s1, s2" type="string">
          return (s1 + s2)
        </function>

This basically says, "We have a function called 'ConcatStrings', it takes two input parameters, which we will call 's1' and 's2' inside the function, and the function returns a string value."

The function would be invoked as:

        s = ConcatStrings("Mama ", "Mia")

The resulting "s" would be "Mama Mia"
