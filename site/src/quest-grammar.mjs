// TextMate grammar for Quest's line-oriented script DSL (e.g. `msg ("Hello")`,
// `if (x = 1) { ... }`), used to syntax-highlight ```quest code fences via Shiki.
// This is a from-scratch port, not an import: the editor's own highlighter
// (src/AppShell/src/lib/quest-script-lang.ts) is a CodeMirror StreamParser, which
// Shiki/expressive-code can't consume directly. Keep the keyword lists below in
// sync with that file by hand if either changes.
const MULTI_WORD_KEYWORDS = [
    "create exit", "create timer", "create turnscript",
    "dictionary add", "dictionary remove",
    "list add", "list remove",
    "get input", "on ready",
    "play sound", "stop sound",
    "else if",
];

const SINGLE_WORD_KEYWORDS = [
    "if", "else", "otherwise", "for", "foreach", "do", "msg", "create", "destroy",
    "ask", "insert", "invoke", "finish", "error", "picture", "requestsave", "firsttime",
];

export const questGrammar = {
    name: "quest",
    scopeName: "source.quest",
    patterns: [
        { include: "#comment" },
        { include: "#keywords" },
        { include: "#strings" },
        { include: "#numbers" },
        { include: "#punctuation" },
    ],
    repository: {
        comment: {
            match: "//.*$",
            name: "comment.line.double-slash.quest",
        },
        keywords: {
            patterns: [
                { match: `\\b(${MULTI_WORD_KEYWORDS.join("|")})\\b`, name: "keyword.control.quest" },
                { match: "\\bJS\\.", name: "keyword.other.quest" },
                { match: "@failed\\b", name: "keyword.other.quest" },
                { match: `\\b(${SINGLE_WORD_KEYWORDS.join("|")})\\b`, name: "keyword.control.quest" },
            ],
        },
        strings: {
            match: '"(?:[^"\\\\\\n]|\\\\.)*"?',
            name: "string.quoted.double.quest",
        },
        numbers: {
            match: "\\b\\d+(?:\\.\\d+)?\\b",
            name: "constant.numeric.quest",
        },
        punctuation: {
            match: "[{}()\\[\\]]",
            name: "punctuation.quest",
        },
    },
};
