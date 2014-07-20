---
layout: index
title: wait
---

    wait {script}

Waits for the user to press a key or click on a "Continue" link, and then runs the nested script. Each successive part needs to be nested inside the one before, like this:

      msg ("First bit")
      wait {
        msg ("Second bit")
        wait {
          msg ("Third bit")
        }
      }

This command was added in Quest 5.1.
