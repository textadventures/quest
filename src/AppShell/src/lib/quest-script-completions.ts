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
export function questFunctionCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/[a-zA-Z_][a-zA-Z0-9_]*/);
    if (!word) return null;
    if (word.from === word.to && !context.explicit) return null;

    const functions = getExpressionFunctions();
    if (functions.length === 0) return null;

    const options: Completion[] = functions.map(f => snippetCompletion(functionSnippet(f.name, f.parameters), {
        label: f.name,
        type: "function",
        detail: `(${f.parameters.join(", ")})`,
        // Game-authored functions are the more likely thing an author wants to call, so rank
        // them above library/built-in functions of the same relevance.
        boost: f.isUserDefined && !f.isLibrary ? 1 : 0,
    }));

    return { from: word.from, options, validFor: /^[a-zA-Z_][a-zA-Z0-9_]*$/ };
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
// looks at text on the current line, matching the tokenizer's own per-line scope.
export function questAttributeCompletions(context: CompletionContext): CompletionResult | null {
    const inString = context.matchBefore(/"[^"]*$/);
    if (!inString) return null;

    const line = context.state.doc.lineAt(inString.from);
    const prefix = line.text.slice(0, inString.from - line.from);
    if (!attributeArgPattern.test(prefix)) return null;

    const names = getAttributeNames();
    if (names.length === 0) return null;

    const options: Completion[] = names.map(name => ({ label: name, type: "property" }));
    return { from: inString.from + 1, options, validFor: /^[^"]*$/ };
}

// Completes the attribute name after `object.` dot-access syntax (e.g. `player.parent`,
// `oldPOV.alias`). This isn't just cosmetic sugar: QuestNCalcLogicalExpressionParser's
// propertyAccessSuffix rewrites `receiver.name` into GetAttribute(receiver, "name") for reads, and
// SetScript resolves `lhs.name = value` the same way for writes (see Core.aslx's pervasive
// `player.parent = ...` / `oldPOV.alias = ...` style) — so this is exactly as valid a spelling of
// "get/set an attribute" as the quoted-string forms above, and arguably the more common one.
export function questDotAttributeCompletions(context: CompletionContext): CompletionResult | null {
    const word = context.matchBefore(/\.[a-zA-Z0-9_]*/);
    if (!word) return null;

    // Require an identifier (not e.g. a decimal number like "3.5") immediately before the dot.
    const before = context.state.sliceDoc(Math.max(0, word.from - 60), word.from);
    if (!/[a-zA-Z_][a-zA-Z0-9_]*$/.test(before)) return null;

    const names = getAttributeNames();
    if (names.length === 0) return null;

    const options: Completion[] = names.map(name => ({ label: name, type: "property" }));
    return { from: word.from + 1, options, validFor: /^[a-zA-Z0-9_]*$/ };
}
