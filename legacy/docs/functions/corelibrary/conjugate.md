---
layout: index
title: Conjugate
---

    Conjugate (object, string verb)

Returns the correct form of the verb for the given object, based on the "gender" attribute of the object. This allows authors to create responses neutral with respect to the object.

```
Conjugate (crowd, "be")
-> "are"
Conjugate (crowd, "do")
-> "do"
Conjugate (crowd, "sit")
-> "sit"
Conjugate (dog, "be")
-> "is"
Conjugate (dog, "do")
-> "does"
Conjugate (dog, "sit")
-> "sits"
```

See also [WriteVerb](writeverb.html)