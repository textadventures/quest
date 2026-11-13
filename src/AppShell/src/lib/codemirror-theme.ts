import { EditorView } from "@codemirror/view";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";
import { cssLanguage } from "@codemirror/lang-css";

// Built entirely from Skeleton's paired light/dark CSS custom properties (e.g.
// --color-surface-200-800, which resolves via light-dark() in
// @skeletonlabs/skeleton/src/base/theme.css) rather than a separate CodeMirror
// theme package, so the editor tracks the app's light/dark mode automatically
// and stays consistent with every other panel. See the "invisible text in
// modals in dark mode" fix (commit 3b1f2964) for why bare single-shade colors
// (e.g. --color-primary-500 alone) are unsafe for backgrounds here — only use
// them for accents/text against the paired surface background below.
export const questEditorTheme = EditorView.theme({
    "&": {
        backgroundColor: "var(--color-surface-50-950)",
        color: "var(--color-surface-900-100)",
        fontSize: "var(--text-xs)",
    },
    ".cm-content, .cm-gutters": {
        // matches the old textarea's "text-xs font-mono" — CodeMirror defaults to a much larger
        // browser default font-size otherwise, out of step with the rest of this app's controls.
        lineHeight: "var(--text-xs--line-height)",
    },
    ".cm-content": {
        caretColor: "var(--color-primary-500)",
        fontFamily: "ui-monospace, SFMono-Regular, Menlo, Consolas, monospace",
    },
    ".cm-cursor, .cm-dropCursor": {
        borderLeftColor: "var(--color-primary-500)",
    },
    "&.cm-focused .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection": {
        backgroundColor: "color-mix(in srgb, var(--color-primary-500) 25%, transparent)",
    },
    ".cm-activeLine": {
        backgroundColor: "color-mix(in srgb, var(--color-surface-600-400) 10%, transparent)",
    },
    ".cm-gutters": {
        backgroundColor: "var(--color-surface-50-950)",
        color: "var(--color-surface-600-400)",
        border: "none",
        borderRight: "1px solid var(--color-surface-200-800)",
    },
    ".cm-activeLineGutter": {
        backgroundColor: "color-mix(in srgb, var(--color-surface-600-400) 10%, transparent)",
    },
    ".cm-matchingBracket, .cm-nonmatchingBracket": {
        backgroundColor: "color-mix(in srgb, var(--color-primary-500) 20%, transparent)",
        outline: "none",
    },
}, { dark: false });

export const questHighlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: "var(--color-primary-500)", fontWeight: "bold" },
    { tag: [t.string, t.attributeValue], color: "var(--color-success-600-400)" },
    { tag: t.comment, color: "var(--color-surface-600-400)", fontStyle: "italic" },
    { tag: t.number, color: "var(--color-tertiary-500)" },
    { tag: [t.tagName, t.attributeName], color: "var(--color-primary-500)" },
    { tag: [t.bracket, t.paren, t.squareBracket, t.brace], color: "var(--color-surface-600-400)" },
]);

// CSS's own tokens, which the style above leaves plain: property names (custom properties
// included), class/id/pseudo-class selectors and values. Scoped to CSS so the same tags in
// JavaScript (where every identifier is a variableName) keep their current look. Value
// keywords (atom), units, url() and !important all derive from t.keyword in @lezer/highlight,
// so the base style's bold keyword rule would reach them too: this one is listed first in
// questEditorExtensions so its rules win, and they reset the weight explicitly.
export const cssHighlightStyle = HighlightStyle.define([
    { tag: [t.propertyName, t.variableName], color: "var(--color-primary-500)" },
    { tag: [t.className, t.labelName], color: "var(--color-secondary-500)" },
    { tag: [t.atom, t.color], color: "var(--color-success-600-400)", fontWeight: "normal" },
    { tag: t.unit, color: "var(--color-tertiary-500)", fontWeight: "normal" },
    { tag: [t.operatorKeyword, t.modifier], color: "var(--color-primary-500)", fontWeight: "normal" },
], { scope: cssLanguage });

export const questEditorExtensions = [
    questEditorTheme,
    syntaxHighlighting(cssHighlightStyle),
    syntaxHighlighting(questHighlightStyle),
];
