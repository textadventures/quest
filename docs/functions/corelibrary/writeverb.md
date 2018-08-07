---
layout: index
title: WriteVerb
---

    WriteVerb(obj, verb)

Returns the correct form of the verb for the given object, based on the "gender" attribute of the object, together with the object name, capitalised. This allows authors to create responses neutral with respect to the object.

```
WriteVerb (crowd, "be")
-> "A crowd are"
WriteVerb (crowd, "do")
-> "A crowd do"
WriteVerb (crowd, "sit")
-> "A crowd sit"
WriteVerb (dog, "be")
-> "A dog is"
WriteVerb (dog, "do")
-> "A dog does"
WriteVerb (dog, "sit")
-> "A dog sits"
```

See also [Conjugate](conjugate.html)