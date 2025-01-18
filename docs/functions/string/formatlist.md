---
layout: index
title: FormatList
---

    FormatList (stringlist or object list, string joiner, string lastjoiner, string nothing)

**New in Quest 5.7**    

Returns a [string](../../types/string.html), listing the entries in the given list. For an object list, the GetDisplayName function is used to get an appropriate string. The last two entries in the list are separated by `lastjoiner`, whilst other entries are separated by `joiner`. If the list is empty the string string in `nothing` is returned.

    list = Split("one;two;three", ";") 
    msg(FormatList(list, ",", "or", "nothing"))
    // "one, two or three"
    msg(FormatList(list, ";", "; and", "nothing"))
    // "one; two; and three"
    list = NewStringList()
    msg(FormatList(list, ",", "and", "nothing"))
    // "nothing"
  
This function was added in Quest 5.7. 