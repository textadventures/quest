---
layout: index
title: Pattern Matching with Regular Expressions
---

What exactly is a "regular expression"? It is a sort of string that can be used to match against another string. You could think of it as a template or a set of rules that a string can be compared to. Quest handles a regex as a string, but behind the scenes it converts that into a proper regex object deep in its workings.

Quest has three functions that can use a regex. All three functions takes the regex and a string to compare against it as parameters and differ only in what they return.

To investigate what the three functions do, I am going to set up a regex and two strings.
```
regex = "put (?<object1>.*) on (?<object2>.*)"
s1 = "put hat on table"
s2 = "put hat in box"
```

The Regex
---------

So what does that regex mean? We will look at this in detail later, but the regex for a lot of commands use these same components, so we will quickly look at it now. The two bits in brackets, `(?<object1>.*)`, are called "capture groups", that is, groups of characters that we want to capture for later use. The capture group starts with an open bracket, followed by a question mark, and then the name of the group in angle brackets, `<object1>`, followed by the pattern to match, `.*`, which in this case says to match any number of anything, and then ended with a close bracket.

The rest of the regex is simple text and this needs to match exactly.


`IsRegexMatch`
--------------

The `IsRegexMatch` function will return true if there is a match and false if not. For our example, the first string is a match, the words (and spaces) of "put" and "on" match exactly, and hat and table match the capture groups. The second return false, because "in" does not match "on".
```
IsRegexMatch(regex, s1)
=> true
IsRegexMatch(regex, s2)
=> false
```

`GetMatchStrength`
------------------

The `GetMatchStrength` will return an indication of how good the match is, or throw an error if it is not a match. The strength is simply the number of characters that are matched outside of the capture groups. For `s1`, these are "put " and " on ", a total of eight characters.
```
GetMatchStrength(regex, s1)
=> 8
GetMatchStrength(regex, s2)
=> Error running script: Error evaluating expression ...
```

`Populate`
----------

The `Populate` function will return a dictionary containing the capture groups, or throw an error if it is not a match. Each entry in the dictionary will have the name of the capture group paired with the matched text.
```
Populate(regex, s1)
=> Dictionary: object1 = hat;object2 = table
Populate(regex, s2)
=> Error running script: Error evaluating expression ...
```

The "cache ID" parameter
------------------------

All the above functions take an optional third parameter, the "cache ID". If you supply a cache ID, the regex will be saved under that name. The next time you use that cache ID for any of the above functions, Quest will ignore the regex you supply, and use the one it created earlier instead.

Continuing with the example before:
```
IsRegexMatch(regex, s1, "my regex")
=> true
IsRegexMatch("nonsense", s1, "my regex")
=> true
```
The original regex is given a cache ID here (the string "my regex"). When `IsRegexMatch` is called a second time, Quest ignores the nonsense regex, because it already has a regex with that cache ID.

Every time the player types some input, Quest has to compare that against the regex for every command, and using cache IDs makes that process considerably faster (and it does that for any custom command you add yourself). It is doubtful if cache IDs are of significant use outside of that, and are more likely to be a source of obscure bugs, so my advice is to not use them.


Command Matching
----------------

When the player types some input, Quest goes through the list of commands, looking for the best match. A match is determined by using `IsRegexMatch`.

If there are more than one matching commands, quest uses three criteria to select the best. Firstly it looks at the value from `GetMatchStrength`, giving priority to the command with the higher match strength.

If there is a tie for the highest match strength, it will give priority to the command specific to the room. If there is still a tie after that, commands lower down the list take priority (so user defined commands take priority over the built in commands).

Note that verb objects are actually a type of command, so when the game iterates through all the commands, that includes verbs. A verb object is really just a command with some specific behaviour, which is to run a certain script on the given object.


A Note About Patterns
---------------------

You can use a "command pattern" for your command, instead of a regular expression. A command pattern is really just a short hand for a regex, and will get converted into a regex when the game starts. Here is a comparison
```
  regex = "^put (?<object1>.*) on (?<object2>.*)$"
  pattern "put #object1# on #object2#"
```


What About Object Matching?
---------------------------

None of the above has paid any attention to what objects are present in the game or are within reach. All these functions do is match text. I could have used this as the regex, the result would be the same (except the dictionary returned from `Populate` would contain different keys of corse).
```
regex = "put (?<bill>.*) on (?<ben>.*)"
```
Once a command has been selected as the best match, it is only _then_ that Quest will attempt to match the text to the objects present. At this point it will complain if we use "bill" and "ben"; all capture group names _in commands_ must start "object", "exit" or "text", so Quest knows what it is supposed to be matching them to.

Text matching
-------------

Text will match anything, and so is useful if you want to relate a command to an object outside the normal scope. You could also use text matching for open-ended commands, such as SAY, as is done in the basic tutorial. You then need to work out what you will do with the text.

You can limit the text that will be matched. In the following example, a cheat command is set up; the player (presumably the author while testing) can type CHEAT followed by either MOVE, SET or GET, followed by further text. Quest will hand two variables to the command's script, `text1` and `text2`.

```
^cheat (?<text1>move|set|get) (?<text2>.+)$
```

Here is another example that would allow you to handle violent commands peacefully:

```
^(?<text>hit|strike|slap|punch|kick|headbutt|kill|murder) (?<object>.+)$
```

If the player types KICK BORIS, Quest will match it to this command, putting "KICK" in the `text` variable, the object `Boris` in the `object` variable, so you could have have a message like this:

> "For a moment you want to " + LCase(text) + " " + object.name + ", but then you think better of it."

By the way, to get HIT to work, you will need to disable the built-in verb. You can do that in the desktop version only, by copying the verb into your game, and then typing a load of nonsense into the pattern. The player will never type in that nonsense, so the verb will never get matched.



More on Regex
-------------

Quest is based on .Net technology, and so uses the .Net format for regex. That said, it is fairly standard and is used across several programming languages, and not at all specific to Microsoft (one difference, though, is how capture groups are defined).
https://msdn.microsoft.com/en-us/library/az24scfc.aspx

A lot of regex options start with a backslash, and this is a bit of a problem, because Quest is using strings to handle them, and in Quest (and most programming languages) the backslash is an escape character. What this means is that to display a backslash in Quest, you actually need to have two of them.
```
msg("Here is a single backslash: \\")
```
If you want to use any regex option that has a backslash _in your code_ you need to remember to use two! An important use of backslashes is to match against a character that has some special meaning. For example, to match a question mark, the standard way is to use `\?`. In quest you will need to use `\\?`.

However, for pattern-matching _in a command_, you do not need the extra backslash.

So what can we put into a regular expression? There is a variety of options allowing you to specify your template as broadly or as narrowly as you want.


### Classes

You can match against a class of characters. For example, `\d` will match a single digit.
```
\d    Any digit
\D    Any non-digit
\w    Any word character (digit or letter)
\W    Any non-word character
\s    Any white space (space, tab, return)
\S    Anything not white space
.     Anything (except return)
```
You can also set up you own class using square backets. Some examples:
```
[aeiou]     Any single character in the group aeiou
[^aeiou]    Any single character NOT in the group aeiou
[a-mA-M]    Any letter from A to M, upper or lower
```

### Quantifiers

You can control how many of a thing can be matched using ?, + and *. To illustrate, let us start with this: 
```
regex = "\\d+\\.?\\d*"
```
There are three parts to it (removing the extra backslashes):
```
\d+
\.?
\d*
```
The `\d` matches a digit. When followed by `+` it matches 1 or more digits, and when followed by `*` it matches zero or more. A `.` in a regex can match any character, as was seen in the capture groups of the first regex. Here, though, it is preceded by a backslash, so it instead means an actual full stop (period). The question mark after it indicates you can have zero or one of them. This regex will match a string containing a series of digits, optionally followed by a single decimal point and optionally followed by more digits.

You can also use curly braces to specify a specific number or range:
```
\d{2,5}    Between 2 and 5 digits
[aeiou]{4} Exactly 4 vowels
```

### Anchors

Anchors allow to to specify where in the string the match must be. In the previous example, the number could be anywhere in the string. Perhaps you require them to be at the beginning or end?
```
regex = "^help$"
```
The `^` and `$` are special codes that must match the start of the string and the end respectively, and they appear in most built in Quest commands. \A and \z do the same. \b must match the boundary between alphanumerics and non-alphanumerics.


Other Applications
------------------

Here is some code that will handle a string like this:
```
  player.health = 60
```
It uses a regex to first confirm the string is in the right format, and then to split it into the three important parts.
```
regex = "(?<object>.+)\\.(?<attribute>\\S+)\\s*=\\s*(?<value>.+)"
if (not IsRegexMatch(regex, text)) {
  error ("Sorry, wrong format")
}
dict = Populate(regex, text)
obj = GetObject(StringDictionaryItem(dict, "object"))
if (obj = null) {
  error ("Sorry, object not recognised")
}
att = StringDictionaryItem(dict, "attribute")
value = Eval(StringDictionaryItem(dict, "value"))
set (obj, att, value)
```
Note: _The web editor seems to have problems with complicated regex code, and will likely get confused and throw an error at the first line above, so this may be restricted to the desktop editor._
