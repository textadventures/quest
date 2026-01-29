---
layout: index
title: IsRegexMatch
---

    IsRegexMatch (string regex, string)

Optionally as of Quest 5.1 there is a new cache ID parameter:

    IsRegexMatch (string regex, string, string cache ID)

Returns a [boolean](../types/boolean.html) - **true** if the string matches the specified regular expression.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to IsRegexMatch (or [Populate](populate.html) or [GetMatchStrength](getmatchstrength.html) ) using the same cache ID.

See also [GetMatchStrength](getmatchstrength.html), [Populate](populate.html)

NOTE: This a [hard-coded function](hardcoded.html).
