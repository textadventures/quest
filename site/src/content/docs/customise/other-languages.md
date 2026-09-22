---
title: Writing a game in another language
description: Start a game in one of the fifteen built-in languages, handle gendered objects and verbs, and contribute or update a language library
sidebar:
  order: 4
---

There is no English in Quest Viva's engine. Every word the game says to the player - "You pick it up.", "I can't see that.", the compass directions, even the words the parser recognises as verbs - comes from a **language library**, a file of [templates](/howto/text/messages) that Core.aslx looks up as it runs. Swap the library and the whole game speaks a different language. Fifteen ship with Quest Viva, contributed by the community over many years.

| You want to | Go to |
|---|---|
| Start a new game in another language | [Starting a game in another language](#starting-a-game-in-another-language) |
| Change the language of a game you've already started | [Switching an existing game](#switching-an-existing-game) |
| Use the editor itself in another language | [The editor's own language](#the-editors-own-language) |
| Change one or two messages in an English game | [Changing the game's messages](/howto/text/messages) |
| Add a new language, or update an existing one | [Writing a language library](#writing-a-language-library) |

## The built-in languages

| Language | Library file | Code |
|---|---|---|
| Dansk | `Dansk.aslx` | `da` |
| Deutsch | `Deutsch.aslx` | `de` |
| English | `English.aslx` | `en` |
| Español | `Espanol.aslx` | `es` |
| Esperanto | `Esperanto.aslx` | `eo` |
| Français | `Francais.aslx` | `fr` |
| Greek | `Greek.aslx` | `el` |
| Icelandic | `Icelandic.aslx` | `is` |
| Italiano | `Italiano.aslx` | `it` |
| Nederlands | `Nederlands.aslx` | `nl` |
| Norsk | `Norsk.aslx` | `nb` |
| Português (Brasil) | `Portugues.aslx` | `pt-BR` |
| Português (Portugal) | `Portugues-Portugal.aslx` | `pt-PT` |
| Română | `Romana.aslx` | `ro` |
| Русский | `Russian.aslx` | `ru` |

Every one of these includes `English.aslx`, so anything a translator hasn't got to yet falls back to the English wording rather than breaking. Coverage varies: some are close to complete, others were translated years ago and don't have the newer messages. If English text turns up in your game, the fix is to translate that one template - and it's worth [sending the change back](#contributing-your-work).

## Starting a game in another language

Pick the language when you create the game - that's all there is to it:

1. On the Create tab, under "Create new game", type your game's name.
2. Under "Game type", choose "Text Adventure" or "Gamebook".
3. Pick the language from the dropdown underneath. The list uses each language's own name.
4. Click "Create local draft" (or "Save to folder…").

Gamebooks offer English, Deutsch and Español only. A gamebook has no parser and few standard messages, so its text comes from the editor libraries rather than a game language library - see [the editor's own language](#the-editors-own-language) below.

The only difference in the resulting game file is the first `include` line:

```xml
<asl version="600">
  <include ref="Francais.aslx" />
  <include ref="Core.aslx" />
```

## Switching an existing game

The language library has to be loaded **before** `Core.aslx`, because Core's commands and verbs are built out of the templates as it loads. Adding it with "Add Library" puts it at the end of the list, after Core, which leaves you with a half-translated game - the room descriptions and commands stay in English. Change the include line instead:

1. Click "Raw XML code view" on the toolbar (on a narrow screen it's in the "More" menu).
2. In the "File:" list, choose your own game file.
3. Change `<include ref="English.aslx" />` to the library you want, such as `<include ref="Francais.aslx" />`.
4. Click "Apply", and confirm.

Text you have already written stays as you wrote it. Exits you created before the change keep their old direction names, because each exit stores its own name as its alias - the same problem as [renaming the compass directions](/howto/text/messages#custom-directions).

## Gendered objects

English doesn't give inanimate objects a gender, so the "Type" dropdown on an object's Setup tab normally offers "Inanimate object", "Inanimate objects (plural)" and the character types. Languages that do have gender add their own entries there. In a French game the dropdown gains "Inanimate object (masculine)" and "Inanimate object (feminine)", which set the object's [article](/reference/attributes/all#article) to *le* or *la* and its [gender](/reference/attributes/all#gender) to *il* or *elle*. The library's messages are written in terms of those two attributes, so everything follows:

```
> prendre pomme
Vous la prenez.

> prendre livre
Vous le prenez.
```

Which types a language offers is up to its library: Greek has masculine, feminine and neuter, each with a plural, and Русский has nine, one per declension pattern. Some libraries have none yet, in which case every object uses the library's default wording. Adding them is a good first contribution - see [language-specific object types](#language-specific-object-types).

## Verbs and commands

The library translates the commands and verbs that Core defines, so `prendre`, `regarder` and `inventaire` work in a French game without you doing anything.

Your own [verbs](/howto/commands/verbs) need a little more care. When the player clicks a verb button next to an object, Quest Viva sends the verb's name followed by the object's - "wear" plus "hat" gives "wear hat", which works in English but not everywhere. The German for "wear hat" is "ziehe Hut an", with the verb split in two.

The fix is to give the verb more than one pattern. A verb's "Pattern" field takes semicolon-separated [patterns](/howto/commands/regular-expressions), so `ziehe #object# an; anziehen #object#` accepts both. The second is clumsy German, but the player never types it - it's only there so the verb button has something to send.

## Names in other alphabets

Object, room, function and command *names* can be written in any alphabet, and scripts can use them. This works:

```quest
πέτρα.isheavy = true
```

Attribute and variable names are the exception: they must **start** with a Latin letter (`A`-`Z`, `a`-`z`) or an underscore, although the rest of the name can be anything. `πέτρα.ωραία` is rejected when the game loads, with "Invalid attribute name"; `πέτρα.xωραία` is accepted. Keeping attribute names in English is the simplest way round it.

Some of the older language libraries advise naming every object in Latin characters and putting the real name in the alias. That's no longer necessary.

## The editor's own language

Two separate things are translated, and they're set in two different places.

**The editor's own chrome** - its menus, buttons and dialogs - follows the "Language" setting in Settings, under the "⋯" menu on the toolbar. English, Deutsch and Español are available. It's a preference for you, not part of your game.

**The editor's tab and field captions** - "Setup", "Features", "Inherited Type" and so on - come from an editor library that your game's language library includes. Only `EditorDeutsch.aslx` and `EditorEspanol.aslx` exist, and only `Deutsch.aslx` and `Espanol.aslx` pull them in. Every other language library gets `EditorEnglish.aslx` by way of `English.aslx`, so the tabs stay in English even when the game itself is fully translated.

## Writing a language library

The rest of this page is for translators. A language library is a plain XML file, and the quickest way to start is to copy `English.aslx` and edit it.

### The header

`English.aslx` begins like this:

```xml
<library>
  <include ref="EditorEnglish.aslx"/>
  <template name="LanguageId">en</template>
```

In your copy, include `English.aslx` instead of `EditorEnglish.aslx`, and change the language ID to your language's code:

```xml
<library>
  <include ref="English.aslx"/>
  <template name="LanguageId">is</template>
```

Including `English.aslx` means every template you haven't translated yet still has a value, so a half-finished library never causes a "No template named…" error - the player just sees some English. `LanguageId` also becomes `game.languageid`, which is how a published game tells a catalogue what language it's in.

### Templates and dynamic templates

A `template` is static text. Translate what's between the tags and leave the name alone:

```xml
<template name="LookAt">Посмотреть на</template>
<template name="Take">Взять</template>
<template name="SpeakTo">Поговорить с</template>
```

A `dynamictemplate` is an [expression](/howto/scripting/writing-code#expressions-and-operators), evaluated when it's printed, with the object it's about in a variable called `object`:

```xml
<dynamictemplate name="TakeSuccessful">"You pick " + object.article + " up."</dynamictemplate>
```

Your version has to be an expression too, but it doesn't have to use the same attributes. If your language needs the [gender](/reference/attributes/all#gender) rather than the [article](/reference/attributes/all#article), use that instead:

```xml
<dynamictemplate name="DropSuccessful">"Ты оставляешь " + object.article + " здесь."</dynamictemplate>
```

Some helper functions used inside dynamic templates, such as `GetDefaultPrefix` and [Conjugate](/reference/functions/string#conjugate), are defined in `English.aslx` itself rather than the engine. You can override them in your library, or add your own - Русский defines a `GetSkl` function for noun declension, Română defines `Greu`.

To find where a template is used, search the `Engine/Core/*.aslx` files for its name with a "find in files" search.

### Commands and verbs

Verbs are simple - each `verbtemplate` is one word or phrase the parser will accept:

```xml
<verbtemplate name="take">prendre</verbtemplate>
<verbtemplate name="take">attraper</verbtemplate>
<verbtemplate name="take">ramasser</verbtemplate>
```

Add as many alternatives as your language needs, and drop any you don't - there's no requirement to match English one for one.

Commands are [regular expressions](/howto/commands/regular-expressions), because they have to pull the object names out of what the player typed:

```xml
<template templatetype="command" name="put"><![CDATA[^put (?<object1>.*) (on|in) (?<object2>.*)$]]></template>
```

Leave `templatetype="command"` alone - it tells the editor this is a command pattern, not a message - and keep the `<![CDATA[ … ]]>` wrapper, which is only there because the pattern contains `<` and `>`. The `^` and `$` mean it has to match the whole of what the player typed, and `(?<object1>.*)` captures a chunk of text under the name `object1`. Everything else is yours to translate:

```xml
<template templatetype="command" name="put"><![CDATA[^mettre (?<object1>.*) (dessus|dedans|sur|dans) (?<object2>.*)$]]></template>
```

Alternatives separated by `|` let you accept several phrasings, as French does for "put on" and "put in" above.

### Language-specific object types

To give the author gendered object types, define the types and then list them in the `LanguageSpecificObjectTypes` template. French:

```xml
<type name="masculine">
  <gender>il</gender>
  <article>le</article>
</type>

<type name="feminine">
  <gender>elle</gender>
  <article>la</article>
</type>

<template name="LanguageSpecificObjectTypes">masculine=Inanimate object (masculine); feminine=Inanimate object (feminine); </template>
```

The names on the left of each `=` must match the types you defined, or the editor shows errors. The captions on the right are what the author sees in the "Type" dropdown, so write them in your language. The trailing `; ` matters - don't delete it. Add as many types as your language needs; Greek has six.

### Testing it

Translate, then play. Create a small game from your language's template - two rooms, a few objects of each gender, something takeable, a character to talk to - and work through the standard commands: look, the compass directions, take, drop, inventory, examine, open, speak to, and a command that fails so you see the parser's error messages. Untranslated templates show up as English in the transcript, which is the quickest way to see what's left. Then open the same game in the editor: a mistake in `LanguageSpecificObjectTypes` shows up in the object "Type" dropdown.

### Contributing your work

If you'd like your language to ship with Quest Viva so other authors can use it, open a pull request on [GitHub](https://github.com/textadventures/quest). Updates to existing translations are just as welcome - most of them have gaps.

To see what's changed in `English.aslx` since a translation was last touched, open `src/Engine/Core/Languages/English.aslx` on GitHub and click "History". New templates are added there as Quest Viva gains features, and every language library needs the same additions eventually.

## See also

- [Changing the game's messages](/howto/text/messages) - overriding individual templates in one game
- [Using neutral language](/howto/text/neutral-language) - `article`, `gender` and `WriteVerb`
- [Using and creating libraries](/customise/libraries)
- [template](/reference/elements#template) and [dynamictemplate](/reference/elements#dynamictemplate) in the XML elements reference
