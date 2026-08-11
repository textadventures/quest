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

Why: "Du" is the norm for German gaming/hobbyist/open-source software
(Steam, itch.io, Duolingo), vs. "Sie" being more the convention for
enterprise/banking/government software - and it's the majority convention
already in this codebase.

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
