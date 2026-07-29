import { StreamLanguage, LanguageSupport } from "@codemirror/language";
import type { StreamParser, StringStream } from "@codemirror/language";

// Approximate, non-validating highlighter for Quest's own line-oriented script
// DSL (e.g. `msg ("Hello")`, `if (x = 1) { ... }`) — distinct from the ASLX XML
// file format. Keyword list curated from the `Keyword =>` properties across
// src/Engine/Scripts/*.cs plus ScriptFactory.cs's special-cased else/else
// if/otherwise handling. Multi-word keywords are matched before single-word
// ones so e.g. "create exit" isn't tokenized as the bare "create" keyword.
const MULTI_WORD_KEYWORDS = [
    "create exit", "create timer", "create turnscript",
    "dictionary add", "dictionary remove",
    "list add", "list remove",
    "get input", "on ready",
    "play sound", "stop sound",
    "else if",
];

const SINGLE_WORD_KEYWORDS = new Set([
    "if", "else", "otherwise", "for", "foreach", "do", "msg", "create", "destroy",
    "ask", "insert", "invoke", "finish", "error", "picture", "requestsave", "firsttime",
]);

const multiWordPattern = new RegExp(`^(${MULTI_WORD_KEYWORDS.join("|")})\\b`);

const questScriptParser: StreamParser<null> = {
    token(stream: StringStream): string | null {
        if (stream.sol() && stream.match(/^\s*\/\//)) {
            stream.skipToEnd();
            return "comment";
        }
        if (stream.match(multiWordPattern)) {
            return "keyword";
        }
        if (stream.match(/^JS\./) || stream.match(/^@failed\b/)) {
            return "keyword";
        }
        if (stream.match(/^"(?:[^"\\]|\\.)*"?/)) {
            return "string";
        }
        if (stream.match(/^\d+(\.\d+)?/)) {
            return "number";
        }
        if (stream.match(/^[{}()[\]]/)) {
            return "bracket";
        }
        const word = stream.match(/^[a-zA-Z_][a-zA-Z0-9_.]*/);
        if (word) {
            const text = (word as RegExpMatchArray)[0];
            return SINGLE_WORD_KEYWORDS.has(text) ? "keyword" : "variableName";
        }
        stream.next();
        return null;
    },
};

// Exported (not just wrapped in questScript()'s LanguageSupport) so xml-with-script.ts can embed
// this language's .parser directly as a Lezer nested parser inside <element type="script"> content.
export const questScriptLanguage = StreamLanguage.define(questScriptParser);

export function questScript(): LanguageSupport {
    return new LanguageSupport(questScriptLanguage);
}
