---
layout: index
title: ASL Requirements
---

Quest expects an ASL file to provide a few standard items. These are all provided by Core.aslx, but if this file is replaced, the ASL must still deliver the below requirements to Quest.

-   There must be one object named "**player**"
-   The following functions must be defined:
    -   **ScopeInventory**, returning a list of objects (used to fill the "Inventory" list)
    -   **GetPlacesObjectsList**, returning a list of objects (for the "Places and Objects" list)
    -   **ScopeExits**, returning a list of exits (for the "Places and Objects" list and Compass)
    -   **HandleCommand**, accepting one string parameter (no return value). This handles player input.

On the "Places and Objects" list, the buttons that appear for an item are defined by the **displayverbs** attribute, which is a list of strings. On the "Inventory" list, the buttons are specified by the **inventoryverbs** attribute.

Optionally:

-   There may be an **InitInterface** function, called when the game is launched.
-   There may be a **StartGame** function, called when the game is launched (but not when loaded from a .quest-save file).
-   There may be a **GetDisplayAlias** function, taking one object parameter (called "obj"), which returns the name of an object to use in the "Places and Objects" lists.
-   If an **UpdateStatusAttributes** function is defined, it will be run after timer scripts run
-   As of v5.1, if a **FinishTurn** function is defined, it is run when a turn has finished (after running a command, handling an async menu selection etc., when there are no more pending scripts to run)
-   As of v5.4, if an **OutputText** function is defined, all text sent to the [msg](scripts/msg.html) command will be sent through it, instead of text being printed directly.

