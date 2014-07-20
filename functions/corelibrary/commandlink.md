---
layout: index
title: CommandLink
---

    CommandLink (string command, string link text)

Returns a [string](../../../types/string.html) containing the XML required to display a hyperlink. When the hyperlink is clicked, the specified player command will be run. For example:

     msg (CommandLink("undo", "Click here to undo the previous turn"))

outputs a link titled "Click here to undo the previous turn" - when clicked, the "undo" player command is run.

See also: [ObjectLink](objectlink.html)
