---
layout: index
title: Script
---

A script attribute contains one or more [script commands](../scripts/).

Example:

     <look type="script">
       if (not fridge.isopen) {
         msg ("The fridge is open, casting its light out into the gloomy kitchen.")
       }
       else {
         msg ("A big old refrigerator sits in the corner, humming quietly.")
       }
     </look>
