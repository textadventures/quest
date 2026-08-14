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
against. Only `en`/`de`/`es` are marked `supported: true` today (see
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

## Spanish (es)

**Register: informal "tú" throughout.** Not "usted".

Applies to all three layers - AppShell UI, `EditorEspanol.aslx`,
`Espanol.aslx` - and to gamebook templates (`Gamebook-Espanol.template`).

Why: already the consistent convention in the existing Spanish
translations before this backfill (`puedes`, `No puedes ir por ahí`, etc.)
- no register conflict to resolve here, just kept new content consistent
with it. "Tú" is also the norm for Spanish gaming/hobbyist software (Steam,
Duolingo), matching the same reasoning as German above.

**Glossary: "script"/"scripts" → "programa"/"programas".** Not "guión"
(that word is the film/theatre sense of "script", not the programming
one). Keep English's singular/plural distinction where a control's
content is genuinely one script vs. several, but note that the Game
editor's `EditorGameScript` tab and the Room editor's
`EditorObjectScriptsScripts` tab both hold multiple named scripts despite
English calling one of them "Script" - translate both as the plural
"Programas" to match what the tab actually contains, not the English
label literally.

Found via user report 2026-08: `EditorObjectScriptsScripts` had drifted to
"Guiones" (wrong word) while the sibling key `EditorScriptsScriptsScripts`
- same English source text, "Scripts" - correctly said "Programas". See
the "terminology consistency" section below for the general version of
this failure mode and the tooling that now catches it.

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

## Terminology consistency

The `.aslx` language files repeat plain English words like "Scripts",
"Background", or "Text" across dozens of unrelated `<template>` keys
(tab captions, control labels, category names, ...). Because each key is
translated independently - by different AI-assisted passes over time, or
by different human contributors - the same English source text can drift
to different words in the target language even though nothing about the
English changed. That's what happened to Spanish's `EditorObjectScriptsScripts`
above: it and `EditorScriptsScriptsScripts` are both literally "Scripts"
in English, but only one of them got translated as "Programas".

`tools/i18n/audit.mjs` now groups baseline keys by identical source text
and reports, per supported language, any group whose translations don't
all match (a "Terminology" column in the summary table, with the specific
keys/values listed below it). This is **informational, not enforced** -
many groups are legitimate false positives, because the same English word
can need different translations for different grammatical contexts (e.g.
German's "Objekt ist verschlossen" vs. "Verschlossen" for two different
"Locked" controls, or gendered/case agreement). Treat a hit as "worth a
second look", not "must fix":

- If the surrounding context is the same concept (a tab caption vs. a
  category caption for the same feature, like the Scripts example) -
  it's very likely a real inconsistency. Fix it and add a glossary line
  above so it doesn't drift back.
- If the contexts genuinely differ (a UI verb vs. a noun, or a different
  grammatical case/gender is required) - it's a legitimate difference.
  No fix needed, and no need to silence it in the tool; the report is
  cheap to skim per language.

When translating (or reviewing an AI-assisted pass over) a batch of keys,
run `node tools/i18n/audit.mjs` and check the Terminology section for the
language you touched before calling the pass done - it's the mechanical
check for exactly the class of bug a per-string reviewer tends to miss,
since no single diff shows two unrelated keys side by side.
