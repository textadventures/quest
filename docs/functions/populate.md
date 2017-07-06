---
layout: index
title: Populate
---

    Populate (string regex, string input)

Optionally as of Quest 5.1 there is a new cache ID parameter:

    Populate (string regex, string input, string cache ID)

The input must be a match for the regular expression, or an error occurs.

Returns a [stringdictionary](../types/stringdictionary.html), keyed by the group names in the regular expression, with values set to the resolved regex groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to Populate (or [GetMatchStrength](getmatchstrength.html) or [IsRegexMatch](isregexmatch.html) ) using the same cache ID.

For example, given this regex which matches the text "put (object name) on (object name)":

    put (<object1>.*) on (<object2>.*)

Passing this to the Populate function with an input "put book on shelf" will return a [stringdictionary](../types/stringdictionary.html) where object1="book" and object2="shelf".

See also [GetMatchStrength](getmatchstrength.html), [IsRegexMatch](isregexmatch.html)

NOTE: This a [hard-coded function](hardcoded.html).
