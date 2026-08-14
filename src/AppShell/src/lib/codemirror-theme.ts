import { EditorView } from "@codemirror/view";
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language";
import { tags as t } from "@lezer/highlight";

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

export const questEditorExtensions = [questEditorTheme, syntaxHighlighting(questHighlightStyle)];
