import { hasMessage, t, tPlural } from "#lib/i18n/index.js";
import type { CheckProblem, ProblemText } from "#lib/types.js";

// Args that name an element's kind ("object", "function", ...), each translated as
// checkProblem.kind.<kind>.
const KIND_ARGS = ["kind", "replacementKind"];

// A problem's message in the Editor's language: Engine's ProblemText, looked up as
// checkProblem.<code> and filled in with its args. Falls back to the English message when the
// problem has no text, or there's no template for its code.
export function problemMessage(problem: CheckProblem): string {
    return (problem.text && translate(problem.text)) ?? problem.message;
}

function translate(text: ProblemText): string | null {
    const key = `checkProblem.${text.code}`;
    // A template with plural forms is chosen by its count arg, as ProblemText.ToEnglish does.
    const plural = "count" in text.args;
    if (!hasMessage(plural ? `${key}.other` : key)) return null;

    const params: Record<string, string> = { ...text.args };
    for (const name of KIND_ARGS) {
        const kindKey = `checkProblem.kind.${params[name]}`;
        if (name in params && hasMessage(kindKey)) params[name] = t(kindKey);
    }
    // {reason} is another message, or failing that English text (args.reason) from somewhere
    // that can't be translated, such as NCalc's parser.
    const reason = text.reason && translate(text.reason);
    if (reason) params.reason = reason;

    return plural ? tPlural(key, Number(text.args.count), params) : t(key, params);
}
