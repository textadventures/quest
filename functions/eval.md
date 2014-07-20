---
layout: index
title: Eval
---

    Eval (string expression, dictionary parameters)

or

    Eval (string expression)

Returns the result of the specified expression.

The parameters dictionary can be used to add variables into the eval context. The parameters will be usable by the evaluated expression.

Example:

      params = NewDictionary()
      dictionary add(params, "x", 50)
      dictionary add(params, "y", 100)
      msg(Eval ("x + y", params))

This will result in "150" being printed.
