# i18n style guide

Register, formality, and glossary decisions for each `supported: true`
language in [`tools/i18n/languages.json`](../tools/i18n/languages.json).
This is the fixed reference to check a translation (AI-assisted or human)
against, rather than re-deriving convention from scratch each time -
see [`tools/i18n/audit.mjs`](../tools/i18n/audit.mjs) for the automated
completeness/consistency check this complements.

Add a section here *before* starting a new language's backfill (see
Part 5 of the tooling plan that introduced this file), not after -
the whole point is to fix the decision first so an AI-assisted first pass
doesn't have to guess and a reviewer has something concrete to check
against.

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

## English (en)

The baseline every other language is diffed against - not itself a
translation, so no register decision applies. Existing copy is already the
de facto style reference for tone/terminology.

## (other languages)

No section yet - add one here as part of bringing a language to
`supported: true` (see the tooling plan's Part 5 rollout). Pick a register
by checking what mainstream gaming/hobbyist software in that language
actually uses (the same reasoning as German above), not by asking an LLM
to guess in isolation per-string - that's exactly the failure mode that
prompted this file.
