---
title: Regular expressions in commands
description: Write a command pattern as a regular expression when the simple form isn't enough, and capture what the player typed into named variables
---

Most commands don't need a regular expression. The simple "Command pattern" form - `attack #object#;strike #object#` - covers alternatives, objects, exits and free text, and is described in [How commands work](/howto/commands/how-commands-work). Reach for a regular expression when you need something it can't express:

- a set of alternative words in the middle of a pattern, without writing out every combination
- a restriction on what the player can type, such as digits only
- optional words, so that `ASK MAN ABOUT THE SWORD` and `ASK MAN ABOUT SWORD` both work from one pattern
- a pattern where one of `*`, `+`, `[`, `]`, `|`, `^`, `$` or `\` has to be a literal character (the simple form only escapes `.`, `?`, `(` and `)`)

To use one, set the dropdown next to the Pattern box on the command's _Command_ tab from "Command pattern" to "Regular expression".

## What a command's regular expression looks like

A **capture group** stands in for what the player types, the way `#object#` does in a simple pattern. It is written `(?<name>...)`: an open bracket, a question mark, the group's name in angle brackets, then the pattern to match, then a close bracket.

In a command, a group's name must start with `object`, `exit` or `text`, so Quest Viva knows what to match it against. Anything else fails at runtime with "command variable names must begin with 'object', 'exit' or 'text'". Those names become the variables your script receives, exactly as with a simple pattern.

That is all a simple pattern is: a short form for a regular expression, converted when the game loads. These two are the same command:

```quest
put #object1# on #object2#
```

```quest
^put (?<object1>.*) on (?<object2>.*)$
```

Each semicolon-separated alternative in a simple pattern becomes one `^...$` section, joined with `|`, so `help;?` becomes `^help$|^\?$`.

Matching ignores capitals - `PUT HAT ON TABLE` matches the pattern above - so there is no need to allow for them yourself.

### Anchors and greediness

`^` matches the start of the input and `$` the end. Without them, your pattern can match part of what the player typed rather than all of it, so start and end every command pattern with them. `\b` matches a boundary between word and non-word characters.

`.*` is greedy: it takes as much as it can while still allowing the rest of the pattern to match. With `^greet (?<text1>.*) for (?<text2>.*)$`, typing `GREET ANNA FOR BOB FOR CAROL` gives `text1` = "anna for bob" and `text2` = "carol". Use `.*?` instead to make a group take as little as possible.

### Character classes

```
\d    Any digit
\D    Any non-digit
\w    Any word character (digit or letter)
\W    Any non-word character
\s    Any white space (space, tab, return)
\S    Anything that is not white space
.     Anything except a line break
```

Square brackets define a class of your own:

```
[aeiou]     Any single character in the group aeiou
[^aeiou]    Any single character not in the group aeiou
[a-mA-M]    Any letter from A to M, upper or lower case
```

### Quantifiers

`?` means zero or one, `+` means one or more, and `*` means zero or more. Curly braces give an exact number or a range:

```
\d{2,5}      Between 2 and 5 digits
[aeiou]{4}   Exactly 4 vowels
```

So `^set dial to (?<text>\d+\.?\d*)$` accepts `SET DIAL TO 42.5` but not `SET DIAL TO ABC`. The `\.` is an escaped full stop, meaning a literal `.` rather than "any character".

### Alternatives

Round brackets with `|` inside give a choice of words. Put the choice in a named group when you want to know which one the player used:

```quest
^cheat (?<text1>move|set|get) (?<text2>.+)$
```

`CHEAT SET score 100` gives `text1` = "set" and `text2` = "score 100".

:::caution
Putting words inside a capture group lowers the command's [match strength](/howto/commands/how-commands-work#how-quest-viva-picks-a-command), because strength counts the characters matched *outside* the groups. Suppose you want to catch violent commands and turn them aside. This pattern scores only 1 for `HIT BORIS` - just the space, because "hit" and "boris" are both inside groups - so the built-in HIT verb, which scores 4, wins instead:

```quest
^(?<text>hit|slap|kick) (?<object>.+)$
```

Writing the alternatives out as separate `^...$` sections, with the verb word outside the group, brings the score back up to 4. That ties with the verb, and a tie goes to whichever was defined later - which is yours:

```quest
^hit (?<object>.+)$|^slap (?<object>.+)$|^kick (?<object>.+)$
```

The simple pattern `hit #object#;slap #object#;kick #object#` converts to the same thing, and beats the verb for the same reason. Reach for the group form only when your script needs to know which word the player used.
:::

### Backslashes

.NET's regular expressions are what Quest Viva uses, and Microsoft's [quick reference](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) is the full list of what you can write.

In a command's Pattern box, type backslashes exactly as the regular expression needs them: `^set dial to (?<text>\d+)$`.

Inside a Quest script, a regular expression is an ordinary string, and the backslash is the string escape character - so every backslash has to be doubled:

```quest
if (IsRegexMatch("^\\d+$", answer)) {
  msg ("That's a number.")
}
```

Writing `"^\d+$"` there is an error: "Invalid token in expression".

## Trying a regular expression out

Three functions let you check a regular expression against a string, which is the quickest way to find out why a pattern isn't doing what you expect. Add a temporary command of your own that prints the results, play the game and try a few strings.

```quest
regex = "put (?<object1>.*) on (?<object2>.*)"
IsRegexMatch(regex, "put hat on table")
=> true
GetMatchStrength(regex, "put hat on table")
=> 8
Populate(regex, "put hat on table")
=> Dictionary: object1 = hat;object2 = table
```

- [`IsRegexMatch`](/reference/functions/string#isregexmatch) returns true or false.
- [`GetMatchStrength`](/reference/functions/string#getmatchstrength) returns the number of characters matched outside the capture groups - here "put " and " on ", so 8. This is the number Quest Viva compares when [choosing between commands](/howto/commands/how-commands-work#how-quest-viva-picks-a-command).
- [`Populate`](/reference/functions/internal-core#populate) returns a string dictionary of the capture groups and what they matched.

`GetMatchStrength` and `Populate` throw an error if the string doesn't match at all, so test with `IsRegexMatch` first.

All three ignore what objects are actually present - they only match text. Matching a word to an object happens afterwards, and only for the command Quest Viva has already chosen.

### The cache ID

Each of the three takes an optional third argument, a cache ID. The first call with a given ID compiles and stores the regular expression under that name; every later call with the same ID reuses the stored one and **ignores the regular expression you passed**.

This is how the parser stays fast: it tests the player's input against every command's pattern on every turn, using each command's name as the cache ID. It also means a command's pattern is fixed once the game is running - assigning to `cmd.pattern` during play changes the attribute but not the pattern the parser uses.

Outside that, a cache ID is rarely worth it and easy to get wrong, since a stale entry silently makes a different regular expression match. Leave the argument out.

## Using a regular expression elsewhere

Regular expressions aren't only for commands. This script takes a string like `player.health = 60`, checks the shape, pulls out the three parts and applies them - the basis of a cheat command for testing:

```quest
regex = "^(?<object>.+)\\.(?<attribute>\\S+)\\s*=\\s*(?<value>.+)$"
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

Checking a typed answer against a regular expression rather than an exact string is also the reliable way to accept a riddle answer - see [Asking the player](/howto/scripting/asking-the-player#checking-the-answer).

## See also

- [How commands work](/howto/commands/how-commands-work) - simple patterns, and how the parser chooses between commands
- [String functions](/reference/functions/string) - the full signatures for `IsRegexMatch` and `GetMatchStrength`
