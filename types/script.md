---
layout: index
title: Script
---

A script attribute contains one or more [script commands](../script_commands.html).

Example:

     <look type="script">
       if (not fridge.isopen) {
         msg ("The fridge is open, casting its light out into the gloomy kitchen.")
       }
       else {
         msg ("A big old refrigerator sits in the corner, humming quietly.")
       }
     </look>
