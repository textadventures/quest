---
layout: index
title: OutputTextRawNoBr
---

    OutputTextRawNoBr (string text)

Prints the specified text, without a line break at the end and without passing the text through the text processor. The next text printed will appear on the same line.

The usual `OutputTextRaw` adds an HTML "br" element to the end of the text, to indicate the end of the line; this function omits it.