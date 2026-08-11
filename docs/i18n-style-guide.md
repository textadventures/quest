# i18n style guide

Register, formality, and glossary decisions for each `supported: true`
language in [`tools/i18n/languages.json`](../tools/i18n/languages.json).
This is the fixed reference to check a translation (AI-assisted or human)
against, rather than re-deriving convention from scratch each time -
see [`tools/i18n/audit.mjs`](../tools/i18n/audit.mjs) for the automated
completeness/consistency check this complements.

Add a section here *before* starting a new language's backfill, not after -
the whole point is to fix the decision first so an AI-assisted first pass
doesn't have to guess and a reviewer has something concrete to check
against. Only `en`/`de` are marked `supported: true` today (see
`tools/i18n/languages.json`) - bringing another language to that bar means
completing all three translation layers (AppShell UI json, Editor `.aslx`,
player `.aslx`) plus its template/gamebook-template wiring, then flipping
its `supported` flag once `tools/i18n/audit.mjs --strict` passes clean.

## German (de)

**Register: informal "Du" throughout, no exceptions.** Not "Sie".

Applies to all three layers - AppShell UI, `EditorDeutsch.aslx`,
`Deutsch.aslx` - and to gamebook templates (`Gamebook-Deutsch.template`).

Why: raised on Discord by Pertex - German software conventionally splits
between formal "Sie" and informal "Du" address, and an AI-assisted
translation pass had inconsistently picked "Sie" for new content (the
gamebook template) while the rest of the project (AppShell `de.json`, the
original `EditorDeutsch.aslx`/`Deutsch.aslx` translations) was already
consistently "Du". Rather than offer two German variants, standardized on
one: "Du" is the norm for German gaming/hobbyist/open-source software
(Steam, itch.io, Duolingo), vs. "Sie" being more the convention for
enterprise/banking/government software - and it was already the majority
convention in this codebase.

Fixed in [#2033](https://github.com/textadventures/quest/pull/2033), which
also closed several unrelated completeness gaps found while auditing for
register consistency (missing `EditorDeutsch.aslx` keys, a couple of dead
duplicate template keys, and made two implicitly-inherited player-layer
keys explicit).

Note: `ObjectCannotBeStored` in `Deutsch.aslx` was one of the two
duplicate-name cases #2033 found - the version it kept turned out to be
the less specific of the two (it didn't use the object's own article, e.g.
"you can't put that there" rather than "you can't put the key there").
[#2034](https://github.com/textadventures/quest/pull/2034) restored the
more specific, article-aware phrasing in Du register, matching the pattern
already used by `CantOpen`/`CantClose` and English's own version of the
same template.

## English (en)

The baseline every other language is diffed against - not itself a
translation, so no register decision applies. Existing copy is already the
de facto style reference for tone/terminology.

## (other languages)

No section yet - add one here as part of bringing a language to
`supported: true`. Pick a register
by checking what mainstream gaming/hobbyist software in that language
actually uses (the same reasoning as German above), not by asking an LLM
to guess in isolation per-string - that's exactly the failure mode that
prompted this file.
