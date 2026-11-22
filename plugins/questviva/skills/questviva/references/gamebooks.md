# Gamebooks

A gamebook is pages of text with links between them, and no parser: the reader clicks a link to turn to the next page. It's a different game type from a text adventure, with its own library, `GamebookCore.aslx`, and its own functions ([gamebook functions](/reference/functions/gamebook/)). The text adventure's rooms, exits, commands and verbs don't apply. A complete example is https://beta.questviva.com/examples/lighthouse.aslx.

## The file

```xml
<asl version="610" templatetype="gamebook">
  <include ref="GamebookCore.aslx" />
  <game name="The Lighthouse">
    <gameid>3f6c1d92-4b88-4a7e-9c55-2ad0f1e7b634</gameid>
    <version>1.0</version>
    <firstpublished>2026</firstpublished>
  </game>
  <object name="Start">
    <object name="player">
      <inherit name="defaultplayer" />
    </object>
    <description>The boat that brought you is already a smudge on the horizon.</description>
    <options type="simplestringdictionary">TakeLantern = Take the lantern;Causeway = Walk up to the lighthouse</options>
  </object>
  <object name="TakeLantern">
    <inherit name="scripttext" />
    <script type="script">
      SetFlagOn ("lantern")
    </script>
    <description>It is heavier than it looks.</description>
    <options type="simplestringdictionary">Causeway = Walk up to the lighthouse</options>
  </object>
  <object name="Causeway">
    <description>Spray comes over the causeway in sheets.{if lantern: The lantern has survived, just.}</description>
  </object>
</asl>
```

- `templatetype="gamebook"` and the single `<include ref="GamebookCore.aslx" />` make it a gamebook. Don't add `English.aslx` or `Core.aslx`.
- **Every page is an `<object>`** at the top level. The reader starts on the page that holds the `player` object, which inherits `defaultplayer`. Pages don't inherit `editor_room` or `editor_object`.
- **`description`** is the page's text. HTML works, such as `<br/><br/>` between paragraphs, but then the text needs CDATA ([XML escaping](asl-primer.md)).
- **`options`** are the page's links, in order: `PageName = Link text`, separated by semicolons. The key is the name of the page it goes to. A link to a page that doesn't exist shows as "(broken link)".
- **A page with no `options` is an ending.** Nothing announces it, so write the text so it reads like one.
- Page names can have spaces, but no punctuation, and can't start with a number. The reader never sees them.

## Page types

A page's type is a type it inherits. With none, it's text: its picture if it has one, its description and its options.

| Inherit | Page |
|---|---|
| (none) | Text |
| `picture` | Text with a `picture` above it: a file name in the game's folder |
| `youtube` | Text with a YouTube video above it: `youtube` is the video's id, such as `8jPyg2pK11M` |
| `link` | Not a page: a link to it goes straight to the web address in `address` |
| `scripttext` | Runs its `script`, then shows the page as usual |
| `script` | Only runs its `script`, showing nothing of its own. The script usually ends by sending the reader on with `MovePlayer (page)` |

Any page can also have a `sound`, a file played when the reader turns to it, and `continuesound`, to keep playing on the next pages.

## Keeping track

Gamebooks keep state in attributes of `game`, with functions that make the usual ones easy:

- **Flags**: `SetFlagOn ("lantern")` and `SetFlagOff ("lantern")` set `game.lantern` to true or false. Test one in a script with `GetBoolean (game, "lantern")`.
- **Counters**: `IncreaseCounter ("nerve")`, `DecreaseCounter`, `ChangeCounter ("nerve", 3)` and `SetCounter ("nerve", 0)`, each starting at 0 if it hasn't been set. Read one as `game.nerve`, or `GetInt (game, "nerve")` before anything has set it.
- **Pages seen**: `HasSeenPage (Hall)`.
- **Links that come and go**: in a `scripttext` page's script, `AddPageLink (Hall, Stairs, "Climb the stairs")` and `RemovePageLink (Hall, Stairs)` change the page's `options`. A change lasts for the rest of the game, so remove a link again when its condition stops being true.

In the text, the text processor reads them ([text processor](/howto/text/text-processor/)): `{if lantern:...}` and `{if not lantern:...}` for a flag, `{if game.nerve > 3:...}` or `{either game.nerve > 3:yes|no}` for a counter, `{once:...}` for text shown the first time only.

## Checking and testing

`qv check` works on gamebooks as it does on text adventures. Play one with `start_game` and `send_command`, or `qv play`: **what you type is the name of the page to turn to**, not the link text. A name goes to that page even when the current page has no link to it, so always check that the link is in the last response before you follow it.

A walkthrough's steps are page names in the same way. **A gamebook's walkthrough needs `type="simplestringlist"` on `<steps>`**, which text adventures don't: without it the steps are read as one piece of text, and the walkthrough passes with no steps at all. Check that `qv walkthrough` reports the number of steps you wrote.

For the complete Lighthouse:

```xml
<walkthrough name="lighthouse lit">
  <steps type="simplestringlist">
    TakeLantern
    Causeway
    assert:GetBoolean (game, "lantern")
    assert:DictionaryContains (Causeway.options, "Handle")
    Handle
  </steps>
</walkthrough>
```

Because typing a page name skips the link, assert that each link you rely on is there, as the `DictionaryContains` step does.
