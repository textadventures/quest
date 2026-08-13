<script lang="ts">
    import { untrack } from "svelte";
    import { EditorState, Compartment, Transaction } from "@codemirror/state";
    import { EditorView, keymap, lineNumbers, highlightActiveLine } from "@codemirror/view";
    import { history, defaultKeymap, historyKeymap, indentWithTab } from "@codemirror/commands";
    import { bracketMatching, indentOnInput } from "@codemirror/language";
    import { closeBrackets, closeBracketsKeymap } from "@codemirror/autocomplete";
    import { searchKeymap } from "@codemirror/search";
    import { javascript } from "@codemirror/lang-javascript";
    import { questScript } from "$lib/quest-script-lang";
    import { xmlWithScript } from "$lib/xml-with-script-lang";
    import { questEditorExtensions } from "$lib/codemirror-theme";
    import { registerActiveCmView } from "$lib/code-editor-registry";

    interface Props {
        value: string;
        language: "xml" | "quest-script" | "javascript";
        readonly?: boolean;
        onChange?: (value: string) => void;
        minHeight?: string;
        class?: string;
    }

    let { value, language, readonly = false, onChange, minHeight = "10rem", class: className = "" }: Props = $props();

    let editorEl = $state<HTMLDivElement | null>(null);
    let view: EditorView | null = null;

    const languageCompartment = new Compartment();
    const readonlyCompartment = new Compartment();

    function languageExtension(lang: "xml" | "quest-script" | "javascript") {
        if (lang === "xml") return xmlWithScript();
        if (lang === "javascript") return javascript();
        return questScript();
    }

    function readonlyExtension(ro: boolean) {
        return [EditorState.readOnly.of(ro), EditorView.editable.of(!ro)];
    }

    // Mounts once when the host element first appears, and never again — value/language/readonly
    // are read via untrack() here so prop changes don't retrigger a full remount (which would
    // otherwise wipe CodeMirror's own cursor position and undo history on every parent re-render).
    // Later prop changes are instead applied surgically by the two $effects below.
    $effect(() => {
        if (!editorEl) return;

        const initialValue = untrack(() => value);
        const initialLanguage = untrack(() => language);
        const initialReadonly = untrack(() => readonly);

        view = new EditorView({
            doc: initialValue,
            parent: editorEl,
            extensions: [
                lineNumbers(),
                highlightActiveLine(),
                history(),
                bracketMatching(),
                closeBrackets(),
                indentOnInput(),
                EditorView.lineWrapping,
                keymap.of([
                    ...closeBracketsKeymap,
                    ...defaultKeymap,
                    ...historyKeymap,
                    ...searchKeymap,
                    indentWithTab,
                    {
                        key: "Mod-s",
                        preventDefault: true,
                        run: v => {
                            onChange?.(v.state.doc.toString());
                            return true;
                        },
                    },
                ]),
                EditorView.domEventHandlers({
                    blur: (_event, v) => {
                        onChange?.(v.state.doc.toString());
                    },
                }),
                languageCompartment.of(languageExtension(initialLanguage)),
                readonlyCompartment.of(readonlyExtension(initialReadonly)),
                EditorView.theme({
                    "&": { minHeight, height: "100%" },
                    ".cm-scroller": { overflow: "auto" },
                }),
                ...questEditorExtensions,
            ],
        });

        return () => {
            registerActiveCmView(null);
            view?.destroy();
            view = null;
        };
    });

    // Reconfigure language/readonly in place on prop change — avoids the remount above. Also
    // (re)registers this view as the toolbar's active Undo/Redo target whenever it's editable, so
    // those buttons operate on the text actually on screen instead of Quest's own undo stack —
    // except while readonly (locked/inherited scripts), where there's nothing here to undo and
    // the toolbar should keep acting on the real model as normal.
    $effect(() => {
        if (!view) return;
        view.dispatch({
            effects: [
                languageCompartment.reconfigure(languageExtension(language)),
                readonlyCompartment.reconfigure(readonlyExtension(readonly)),
            ],
        });
        registerActiveCmView(readonly ? null : view);
    });

    // External value sync: only replace the doc when the incoming prop actually differs from
    // what the editor already shows, so this doesn't fight in-progress typing or clobber
    // CodeMirror's own undo stack every time onChange echoes the same text back through this prop.
    // The replacement is explicitly non-undoable (Transaction.addToHistory.of(false)): it's a
    // programmatic reload of the buffer, not a user edit, so it mustn't pollute the history —
    // otherwise async file loads that land after mount (e.g. the raw XML panel or a library file
    // fetched over the bridge) make the toolbar's Undo revert to the empty pre-load doc.
    $effect(() => {
        if (!view) return;
        if (value !== view.state.doc.toString()) {
            view.dispatch({
                changes: { from: 0, to: view.state.doc.length, insert: value },
                annotations: Transaction.addToHistory.of(false),
            });
        }
    });
</script>

<div bind:this={editorEl} class="code-editor rounded border border-surface-200-800 overflow-hidden h-full {className}"></div>
