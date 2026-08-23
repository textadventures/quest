import { snippetCompletion } from "@codemirror/autocomplete";
import type { Completion, CompletionContext, CompletionResult } from "@codemirror/autocomplete";
import { getExpressionFunctions, getAttributeNames } from "./editor-store";

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

// Real script-command keyword forms (if/foreach/msg/set/create timer/...). These are Script
// objects (src/Engine/Scripts/*.cs) rather than expression functions, so none of them appear in
// GetExpressionFunctions — without this list, typing "if" or "foreach" only ever offered fuzzy
// function-name noise, never the keyword itself. Snippet bodies mirror the exact template each
// script's own <create> element uses in src/Engine/Core/CoreEditorScripts*.aslx (what the visual
// editor's "Add script" picker inserts), cross-checked against each Script(Constructor)'s real
// ExpectedParameters/param names for the ones whose <create> template only shows blank commas.
const SCRIPT_KEYWORDS: { label: string; snippet: string }[] = [
    { label: "msg", snippet: "msg (\"${1:text}\")" },
    { label: "if", snippet: "if (${1:condition}) {\n\t${2}\n}" },
    { label: "else if", snippet: "else if (${1:condition}) {\n\t${2}\n}" },
    { label: "else", snippet: "else {\n\t${1}\n}" },
    { label: "otherwise", snippet: "otherwise {\n\t${1}\n}" },
    { label: "foreach", snippet: "foreach (${1:item}, ${2:list}) {\n\t${3}\n}" },
    { label: "for", snippet: "for (${1:i}, ${2:from}, ${3:to}) {\n\t${4}\n}" },
    { label: "while", snippet: "while (${1:condition}) {\n\t${2}\n}" },
    { label: "switch", snippet: "switch (${1:expression}) {\n\tcase (\"${2:value}\") {\n\t\t${3}\n\t}\n\tdefault {\n\t\t${4}\n\t}\n}" },
    { label: "do", snippet: "do (${1:object}, \"${2:action}\", QuickParams(\"${3:param}\", ${4:value}))" },
    { label: "set", snippet: "set (${1:object}, \"${2:attribute}\", ${3:value})" },
    { label: "get input", snippet: "get input {\n\t${1}\n}" },
    { label: "list add", snippet: "list add (${1:list}, ${2:item})" },
    { label: "list remove", snippet: "list remove (${1:list}, ${2:item})" },
    { label: "dictionary add", snippet: "dictionary add (${1:dict}, \"${2:key}\", ${3:value})" },
    { label: "dictionary remove", snippet: "dictionary remove (${1:dict}, \"${2:key}\")" },
    { label: "create", snippet: "create (\"${1:name}\")" },
    { label: "create exit", snippet: "create exit (\"${1:name}\", ${2:from}, ${3:to})" },
    { label: "create timer", snippet: "create timer (\"${1:name}\")" },
    { label: "create turnscript", snippet: "create turnscript (\"${1:name}\")" },
    { label: "destroy", snippet: "destroy (${1:object})" },
    { label: "ask", snippet: "ask (\"${1:question}\") {\n\t${2}\n}" },
    { label: "insert", snippet: "insert (\"${1:text}\")" },
    { label: "invoke", snippet: "invoke (${1:script})" },
    { label: "finish", snippet: "finish" },
    { label: "error", snippet: "error (\"${1:message}\")" },
    { label: "picture", snippet: "picture (\"${1:filename}\")" },
    { label: "requestsave", snippet: "requestsave" },
    { label: "firsttime", snippet: "firsttime {\n\t${1}\n}" },
    { label: "on ready", snippet: "on ready {\n\t${1}\n}" },
    { label: "play sound", snippet: "play sound (\"${1:filename}\", ${2:synchronous}, ${3:loop})" },
    { label: "stop sound", snippet: "stop sound" },
    { label: "wait", snippet: "wait {\n\t${1}\n}" },
    { label: "undo", snippet: "undo" },
    { label: "return", snippet: "return (${1:value})" },
    { label: "request", snippet: "request (${1:type}, ${2:value})" },
];

// Boolean/null literals — real NCalc/Quest expression literals (e.g. Core.aslx's
// `return (null)`), but not functions, objects, or attributes, so nothing else here suggests them.
const LITERAL_KEYWORDS: { label: string; snippet: string }[] = [
    { label: "true", snippet: "true" },
    { label: "false", snippet: "false" },
    { label: "null", snippet: "null" },
];

// boost only orders options within this source's own block (see the registration-order comment in
// quest-script-lang.ts/xml-with-script-lang.ts for why cross-source ordering is controlled there
// instead) — set here so e.g. "create" sorts above the longer "create exit"/"create timer"/
// "create turnscript" it's also a prefix of.
const KEYWORD_OPTIONS: Completion[] = [...SCRIPT_KEYWORDS, ...LITERAL_KEYWORDS].map(k =>
    snippetCompletion(k.snippet, { label: k.label, type: "keyword", boost: k.label.includes(" ") ? 1 : 2 }));

// Matches a run of up to two space-separated words ending at the cursor, so typing "create t"
// (partway through "create turnscript") is recognised as one candidate string to prefix-match
// against multi-word keyword labels, not just the bare trailing word "t".
export function questKeywordCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/[a-zA-Z_][a-zA-Z0-9_]*(?: [a-zA-Z_][a-zA-Z0-9_]*)?$/);
    if (!word) return null;

    const typed = word.text.toLowerCase();
    const options = KEYWORD_OPTIONS.filter(c => (c.label ?? "").toLowerCase().startsWith(typed));
    if (options.length === 0) return null;

    return { from: word.from, options, filter: false };
}
