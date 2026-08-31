import { snippetCompletion } from "@codemirror/autocomplete";
import type { Completion, CompletionContext, CompletionResult } from "@codemirror/autocomplete";
import {
    getExpressionFunctions, getAttributeNames, getObjectNames, getExitNames, getScriptCommandCategories,
} from "./editor-store";

// Turns a function's real parameter names into a tab-stop snippet, e.g. GetAttribute(object,
// property) -> "GetAttribute(${1:object}, ${2:property})", so accepting the completion lets the
// author Tab through each argument in turn rather than just inserting the bare name.
function functionSnippet(name: string, parameters: string[]): string {
    if (parameters.length === 0) return `${name}()`;
    const args = parameters.map((p, i) => `\${${i + 1}:${p}}`).join(", ");
    return `${name}(${args})`;
}

// Completes calls to built-in engine functions and the game's own user-defined Functions (and
// library functions such as DiceRoll). Data comes from EditorController.GetExpressionFunctions
// via the WasmEditor bridge — the same list that already powers the visual editor's
// expression-insert helper (ExpressionInput.svelte / ScriptEditor.svelte's Call-function picker).
//
// Filtered to a case-insensitive prefix match ourselves (rather than returning the whole ~250-ish
// list and letting CodeMirror's default fuzzy scorer loose on it): fuzzy matching against that
// many candidates surfaces arbitrary, barely-related functions for perfectly ordinary text (e.g.
// typing "true" fuzzy-matched an unrelated function whose letters happened to appear in order),
// which is worse than not completing at all. `filter: false` tells CodeMirror to trust this
// pre-filtered, boost-ordered list as-is; omitting validFor makes the source re-run (and
// re-filter) on every keystroke rather than reusing a stale snapshot of the first character typed.
export function questFunctionCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/[a-zA-Z_][a-zA-Z0-9_]*/);
    if (!word) return null;
    if (word.from === word.to && !context.explicit) return null;

    const typed = word.text.toLowerCase();
    const functions = getExpressionFunctions().filter(f => f.name.toLowerCase().startsWith(typed));
    if (functions.length === 0) return null;

    const options: Completion[] = functions.map(f => snippetCompletion(functionSnippet(f.name, f.parameters), {
        label: f.name,
        type: "function",
        detail: `(${f.parameters.join(", ")})`,
        // Game-authored functions are the more likely thing an author wants to call, so rank
        // them above library/built-in functions of the same relevance.
        boost: f.isUserDefined && !f.isLibrary ? 1 : 0,
    }));

    return { from: word.from, options, filter: false };
}

// Names of the built-in functions/script commands whose (single) string argument right after the
// first comma is an attribute name — reflected from ExpressionOwner's real parameter names
// ("property"/"attribute") plus the "set" script command, which takes the same shape
// (object, "attribute", value) but isn't an expression function so it isn't in that list.
const ATTRIBUTE_ARG_CALLS = [
    "GetAttribute", "HasAttribute", "GetBoolean", "GetInt", "GetDouble", "GetString", "TypeOf", "set",
];

const attributeArgPattern = new RegExp(
    `\\b(?:${ATTRIBUTE_ARG_CALLS.join("|")})\\s*\\(\\s*[a-zA-Z_][a-zA-Z0-9_.]*\\s*,\\s*$`,
);

// Completes the attribute-name argument of GetAttribute/HasAttribute/set/etc. with every
// attribute name used anywhere in the game (EditorController.GetPropertyNames, via the bridge's
// GetAttributeNames), not just ones already typed in the currently open script. This DSL is
// line-oriented (see quest-script-lang.ts), so the "are we inside one of these calls" check only
// looks at text on the current line, matching the tokenizer's own per-line scope. Prefix-filtered
// for the same reason as questFunctionCompletions above.
export function questAttributeCompletions(context: CompletionContext): CompletionResult | null {
    const inString = context.matchBefore(/"[^"]*$/);
    if (!inString) return null;

    const line = context.state.doc.lineAt(inString.from);
    const prefix = line.text.slice(0, inString.from - line.from);
    if (!attributeArgPattern.test(prefix)) return null;

    const typed = inString.text.slice(1).toLowerCase();
    const names = getAttributeNames().filter(n => n.toLowerCase().startsWith(typed));
    if (names.length === 0) return null;

    const options: Completion[] = names.map(name => ({ label: name, type: "property" }));
    return { from: inString.from + 1, options, filter: false };
}

// Completes the attribute name after `object.` dot-access syntax (e.g. `player.parent`,
// `oldPOV.alias`). This isn't just cosmetic sugar: QuestNCalcLogicalExpressionParser's
// propertyAccessSuffix rewrites `receiver.name` into GetAttribute(receiver, "name") for reads, and
// SetScript resolves `lhs.name = value` the same way for writes (see Core.aslx's pervasive
// `player.parent = ...` / `oldPOV.alias = ...` style) — so this is exactly as valid a spelling of
// "get/set an attribute" as the quoted-string forms above, and arguably the more common one.
// Prefix-filtered for the same reason as questFunctionCompletions above.
export function questDotAttributeCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/\.[a-zA-Z0-9_]*/);
    if (!word) return null;

    // Require an identifier (not e.g. a decimal number like "3.5") immediately before the dot.
    const before = context.state.sliceDoc(Math.max(0, word.from - 60), word.from);
    if (!/[a-zA-Z_][a-zA-Z0-9_]*$/.test(before)) return null;

    const typed = word.text.slice(1).toLowerCase();
    const names = getAttributeNames().filter(n => n.toLowerCase().startsWith(typed));
    if (names.length === 0) return null;

    const options: Completion[] = names.map(name => ({ label: name, type: "property" }));
    return { from: word.from + 1, options, filter: false };
}

// Completes game object/room/dialogue-page names (getObjectNames — Quest has no separate "room"
// ObjectType, rooms/objects/pages are all plain objects, see Element.cs's ObjectType enum) plus
// exit names (a distinct ObjectType, hence its own call). Mirrors the same two calls
// ScriptEditor.svelte's own name pickers already combine (see e.g. its object/exit dropdown setup)
// — not a new data source, just reusing it for the code view. Prefix-filtered for the same reason
// as questFunctionCompletions above.
export function questObjectCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/[a-zA-Z_][a-zA-Z0-9_]*/);
    if (!word) return null;
    if (word.from === word.to && !context.explicit) return null;

    const typed = word.text.toLowerCase();
    const names = [...(getObjectNames() ?? []), ...(getExitNames() ?? [])]
        .filter(n => n.toLowerCase().startsWith(typed));
    if (names.length === 0) return null;

    const options: Completion[] = names.map(name => ({ label: name, type: "variable" }));
    return { from: word.from, options, filter: false };
}

// Real script-command keywords (if/foreach/msg/set/create timer/...) that the visual editor's own
// "Add script" picker offers, minus a handful it deliberately doesn't (see below). These are
// Script objects (src/Engine/Scripts/*.cs), not expression functions, so none of them appear in
// GetExpressionFunctions — without this, typing "if" or "foreach" only ever offered fuzzy
// function-name noise, never the keyword itself.
//
// Rather than a hand-copied list of every command's syntax (which silently drifts out of sync the
// moment a new one is added to the engine), this is generated from getScriptCommandCategories() —
// the exact same data EditorController.GetScriptEditorData()/WasmEditorBridge.GetScriptCommandCategories
// feeds to ScriptEditor.svelte's "Add script" dropdown — by turning each command's <create> template
// (e.g. `create timer ("")`, `for (,,,1)`) into a tab-stop snippet. A "(function)Name" keyword is a
// convenience shortcut for a built-in function (e.g. "(function)MoveObject") that's already covered,
// with real parameter names, by questFunctionCompletions; "()" is the picker's generic "Call
// function" entry. Both are skipped here rather than duplicated with a worse, nameless snippet.
const NEEDS_TRAILING_BLOCK = new Set(["if", "foreach", "for", "while", "switch", "firsttime"]);

function keywordSnippet(keyword: string, createString: string): string {
    let n = 0;
    let snippet = createString.replace(/\(([^()]*)\)/, (_whole, argsText: string) => {
        // Genuinely empty parens (no comma at all, e.g. "invoke ()"/"if ()") still get one tab
        // stop rather than being left bare — same reasoning as the blank-comma-slot case below,
        // there's just no comma to split on here since there's only one slot to begin with.
        if (argsText === "") return `(\${${++n}})`;
        const args = argsText.split(",").map(arg => {
            const trimmed = arg.trim();
            const quoted = trimmed.match(/^"(.*)"$/);
            if (quoted) return `"\${${++n}${quoted[1] ? `:${quoted[1]}` : ""}}"`;
            if (trimmed === "") return `\${${++n}}`;
            return `\${${++n}:${trimmed}}`;
        });
        return `(${args.join(", ")})`;
    });
    // Block-bodied commands (if/foreach/for/while/switch/firsttime) get no trailing braces in their
    // <create> template — the visual editor represents their body as a structural tree child, not
    // literal text, so there's nothing to strip there; the code view needs real braces though.
    if (NEEDS_TRAILING_BLOCK.has(keyword) && !/\{\s*\}\s*$/.test(snippet)) {
        snippet += ` {\n\t\${${++n}}\n}`;
    }
    return snippet;
}

// A tiny handful of commands where the mechanical <create>-template transform above can't capture
// genuinely useful structure. Right now that's just "switch": its <create> is `switch ()` (its
// case/default children are, like other block bodies, a structural tree child with no textual
// trace to derive from), so the generic transform alone would only ever offer a bare, empty body.
// Everything else still comes from the generic mechanism above and stays automatically in sync
// with the engine; only entries here are overridden entirely, bypassing keywordSnippet (including
// its NEEDS_TRAILING_BLOCK handling — the snippet below already includes the braces it needs).
//
// "do"'s optional third QuickParams argument (do (object, "action", QuickParams("key", value)))
// was special-cased here too at first, but it's a rare pattern in practice — across every .aslx
// file in this repo, only 2 of 103 real `do (...)` call sites use it, and both are Core.aslx's own
// internals dispatching to its own extensibility hooks (CoreCommands' "ondrop", CoreParser's
// "unresolved"), passing extra context a game author's own override reads back as local variables
// in scope — not something an ordinary game author's own `do` call needs. Defaulting the
// suggestion to that form would've pushed the rare case into the common slot, so "do" now falls
// back to the plain, generic 2-arg form that matches the other 101 call sites.
const SPECIAL_CASE_SNIPPETS: Record<string, string> = {
    switch: "switch (${1:expression}) {\n\tcase (\"${2:value}\") {\n\t\t${3}\n\t}\n\tdefault {\n\t\t${4}\n\t}\n}",
};

// boost only orders options within this source's own block (see the registration-order comment in
// quest-script-lang.ts/xml-with-script-lang.ts for why cross-source ordering is controlled there
// instead) — used so e.g. "create" sorts above the longer "create exit"/"create timer"/
// "create turnscript" it's also a prefix of.
function keywordBoost(label: string): number {
    return label.includes(" ") ? 1 : 2;
}

// A handful of real keywords/literals that never come from getScriptCommandCategories(), kept by
// hand since there's no generic data source for either: "else"/"else if"/"otherwise" are added to
// an existing `if` via a dedicated tree-editor action rather than through the generic "Add script"
// picker, and true/false/null are expression literals, not script commands, at all.
const EXTRA_KEYWORDS: Completion[] = [
    { label: "else if", snippet: "else if (${1:condition}) {\n\t${2}\n}" },
    { label: "else", snippet: "else {\n\t${1}\n}" },
    { label: "otherwise", snippet: "otherwise {\n\t${1}\n}" },
    { label: "true", snippet: "true" },
    { label: "false", snippet: "false" },
    { label: "null", snippet: "null" },
].map(k => snippetCompletion(k.snippet, { label: k.label, type: "keyword", boost: keywordBoost(k.label) }));

let cachedKeywordOptions: Completion[] = EXTRA_KEYWORDS;
let loadedFromScriptCommandCategories = false;
let keywordLoadPromise: Promise<void> | null = null;

// getScriptCommandCategories() is async (its EditorController data carries per-command
// onlydisplayif visibility checks) and hits the WASM bridge, which may not be ready yet the first
// time a completion is requested (this module loads well before any game does). Concurrent callers
// share the same in-flight promise rather than each kicking off their own fetch; on failure (bridge
// still not ready) the promise is cleared so the next request gets a fresh attempt, but once it
// succeeds this never re-fetches for the rest of the session. The hand-kept EXTRA_KEYWORDS above
// are usable immediately, before this ever resolves.
function ensureKeywordsLoaded(): Promise<void> {
    if (loadedFromScriptCommandCategories) return Promise.resolve();
    keywordLoadPromise ??= (async () => {
        const data = await getScriptCommandCategories();
        if (!data) {
            keywordLoadPromise = null;
            return;
        }

        const options = [...EXTRA_KEYWORDS];
        const seen = new Set(options.map(o => o.label));
        for (const category of data.categories) {
            for (const command of category.commands) {
                if (command.keyword === "()" || command.keyword.startsWith("(function)")) continue;
                // Symbolic entries (=, =>, //, JS.) can never be reached anyway: this source's own
                // trigger regex below only ever matches typed text starting with a letter.
                if (!/^[a-zA-Z]/.test(command.keyword)) continue;
                if (seen.has(command.keyword)) continue;
                seen.add(command.keyword);
                const snippet = SPECIAL_CASE_SNIPPETS[command.keyword]
                    ?? keywordSnippet(command.keyword, command.createString);
                options.push(snippetCompletion(snippet, {
                    label: command.keyword,
                    type: "keyword",
                    boost: keywordBoost(command.keyword),
                }));
            }
        }
        cachedKeywordOptions = options;
        loadedFromScriptCommandCategories = true;
    })();
    return keywordLoadPromise;
}

function matchKeywords(typed: string, from: number): CompletionResult | null {
    const options = cachedKeywordOptions.filter(c => (c.label ?? "").toLowerCase().startsWith(typed));
    return options.length ? { from, options, filter: false } : null;
}

// Matches a run of up to two space-separated words ending at the cursor, so typing "create t"
// (partway through "create turnscript") is recognised as one candidate string to prefix-match
// against multi-word keyword labels, not just the bare trailing word "t". Returns a Promise (which
// CodeMirror's autocomplete natively supports and waits on) rather than firing the load and
// immediately answering from a possibly-still-cold cache — the first completion request of a
// session would otherwise silently render without any of the generated keywords and never
// refresh once the fetch actually completes, since nothing re-triggers the popup on its own.
export function questKeywordCompletions(
    context: CompletionContext,
): CompletionResult | Promise<CompletionResult | null> | null {
    const word = context.matchBefore(/[a-zA-Z_][a-zA-Z0-9_]*(?: [a-zA-Z_][a-zA-Z0-9_]*)?$/);
    if (!word) return null;

    const typed = word.text.toLowerCase();
    if (!loadedFromScriptCommandCategories) {
        return ensureKeywordsLoaded().then(() => matchKeywords(typed, word.from));
    }
    return matchKeywords(typed, word.from);
}
