---
layout: index
title: GetMatchStrength
---

    GetMatchStrength (string regex, string input)

Optionally as of Quest 5.1 there is a new cache ID parameter:

    GetMatchStrength (string regex, string input, string cache ID)

Returns an [int](../types/int.html) indicating how strongly the given input matches the regular expression.

The strength is defined as the length of the "required" parts of the string, i.e. the total length of the string *minus* the total length of all named groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to GetMatchStrength (or [IsRegexMatch](isregexmatch.html) or [Populate](populate.html) ) using the same cache ID.

For example, given this regex which matches the text "look at " followed by any object name:

     look at (?<object>.*)

An input of "look at dog" has a strength of 8.

This is calculated as follows:

-   The string "look at dog" has a length of 11
-   The named group "object" matches the substring "dog", which has a length of 3
-   The strength therefore is 11 - 3 = 8

The strength is used by the command handling functions in CoreCommands.aslx to determine which command is the best match for a given input.

See also [IsRegexMatch](isregexmatch.html), [Populate](populate.html)

NOTE: This function is hard-coded and cannot be overridden.
