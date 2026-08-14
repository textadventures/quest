import { parseMixed } from "@lezer/common";
import type { SyntaxNodeRef, Input } from "@lezer/common";
import { xmlLanguage, autoCloseTags } from "@codemirror/lang-xml";
import { LanguageSupport } from "@codemirror/language";
import { questScriptLanguage, questScriptFoldService } from "./quest-script-lang";

// ASLX marks every script-bearing attribute the same way regardless of the wrapping element's own
// name — e.g. <start type="script">, <script type="script">, <take type="script">, <lock
// type="script"> all through src/Engine/Core/*.aslx — so "does this element's opening tag have
// type="script"?" is a reliable, element-name-agnostic signal for where to switch languages.
function isScriptElement(node: SyntaxNodeRef, input: Input): boolean {
    const openTag = node.node.parent?.firstChild;
    if (!openTag || openTag.type.name !== "OpenTag") return false;
    for (let attr = openTag.firstChild; attr; attr = attr.nextSibling) {
        if (attr.type.name !== "Attribute") continue;
        const nameNode = attr.getChild("AttributeName");
        const valueNode = attr.getChild("AttributeValue");
        if (!nameNode || !valueNode) continue;
        if (input.read(nameNode.from, nameNode.to) !== "type") continue;
        if (input.read(valueNode.from, valueNode.to).replace(/"/g, "") === "script") return true;
    }
    return false;
}

// xmlLanguage.configure() reuses the same language-data facet and layers this wrap onto the
// existing (already tag/attribute-highlighting-configured) parser, rather than rebuilding one from
// scratch — see LRLanguage.configure in @codemirror/language.
const xmlWithScriptLanguage = xmlLanguage.configure({
    wrap: parseMixed((node, input) => {
        if (node.type.name !== "Text" && node.type.name !== "CData") return null;
        return isScriptElement(node, input) ? { parser: questScriptLanguage.parser } : null;
    }),
});

export function xmlWithScript(): LanguageSupport {
    // questScriptFoldService is registered for the whole document rather than just the embedded
    // script regions — it only ever fires on `{`-terminated lines (a very Quest-script-y signal that
    // won't occur in XML markup or prose), which is what lets the raw XML view fold script blocks
    // that the base xmlLanguage's foldNodeProp can't see into.
    return new LanguageSupport(xmlWithScriptLanguage, [autoCloseTags, questScriptFoldService]);
}
