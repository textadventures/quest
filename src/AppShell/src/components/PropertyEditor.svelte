<script lang="ts">
    import { selectedKey, selectedData, treeNodes, isGamebook, setAttribute, setDropdownType, setMultiType, setObjectReference, setSelectedFilter, addDictItem, removeDictItem, updateDictItem, getObjectNames, getExitNames, getPageNames, selectNode, createPageSilent, openAddModal, openAddLibraryModal, openAddJavascriptModal, getAssetText, putAssetText, putLibraryAssetText, isBuiltInLibrary, getLibraryXml, setPatternAttribute } from "$lib/editor-store";
    import { showToast } from "$lib/toast";
    import { isLibraryFilename } from "$lib/filesystem/types";
    import { t } from "$lib/i18n";
    import type { ControlInfo, ControlOption, TextProcessorCommand } from "$lib/types";
    import type { TreeNode } from "$lib/types";
    import ChevronLeft from "@lucide/svelte/icons/chevron-left";
    import ArrowRight from "@lucide/svelte/icons/arrow-right";
    import Bold from "@lucide/svelte/icons/bold";
    import Italic from "@lucide/svelte/icons/italic";
    import Underline from "@lucide/svelte/icons/underline";
    import Repeat1 from "@lucide/svelte/icons/repeat-1";
    import LinkIcon from "@lucide/svelte/icons/link";
    import Terminal from "@lucide/svelte/icons/terminal";
    import LogOut from "@lucide/svelte/icons/log-out";
    import GitBranch from "@lucide/svelte/icons/git-branch";
    import GitFork from "@lucide/svelte/icons/git-fork";
    import MapPin from "@lucide/svelte/icons/map-pin";
    import MapPinOff from "@lucide/svelte/icons/map-pin-off";
    import Dices from "@lucide/svelte/icons/dices";
    import ImageIcon from "@lucide/svelte/icons/image";
    import BookOpen from "@lucide/svelte/icons/book-open";
    import LifeBuoy from "@lucide/svelte/icons/life-buoy";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";
    import ListPlus from "@lucide/svelte/icons/list-plus";
    import ScriptEditor from "./ScriptEditor.svelte";
    import CodeEditor from "./CodeEditor.svelte";
    import DropdownMenu from "./DropdownMenu.svelte";
    import type { DropdownMenuItem } from "./DropdownMenu.svelte";
    import LinkPickerModal from "./LinkPickerModal.svelte";
    import ScriptDictionaryEditor from "./ScriptDictionaryEditor.svelte";
    import Combobox from "./Combobox.svelte";
    import AttributesEditor from "./AttributesEditor.svelte";
    import ListEditor from "./ListEditor.svelte";
    import ElementsList from "./ElementsList.svelte";
    import ExpressionInput from "./ExpressionInput.svelte";
    import AssetPicker from "./AssetPicker.svelte";
    import ExitsEditor from "./ExitsEditor.svelte";
    import VerbsEditor from "./VerbsEditor.svelte";
    import AddElementModal from "./AddElementModal.svelte";
    import LibraryElementBanner from "./LibraryElementBanner.svelte";

    let { onback }: { onback?: () => void } = $props();

    // Same derivation as Toolbar's selectedNode — used for the mobile back header's label.
    let selectedNode = $derived<TreeNode | null>(
        $treeNodes.find(n => n.key === $selectedKey) ?? null
    );

    // The "_advanced" tree header has no elementType of its own (EditorController
    // registers it with type: null), so $selectedData is always null for it and it
    // would otherwise fall into the generic "No properties available" state below.
    // It still needs to be a real entry point — its sub-category headers (Functions,
    // Timers, Library, ...) stay hidden from the tree until they have their first
    // element (TreePanel's HIDE_WHEN_EMPTY), so this is the only place to add the
    // first one of each.
    const ALL_ADVANCED_ADDERS: { label: string; action: () => void; gamebook: boolean }[] = [
        { label: t("elementAdders.function"), action: () => openAddModal("function", null), gamebook: true },
        { label: t("elementAdders.timer"), action: () => openAddModal("timer", null), gamebook: false },
        { label: t("elementAdders.walkthrough"), action: () => openAddModal("walkthrough", null), gamebook: false },
        { label: t("elementAdders.library"), action: () => openAddLibraryModal(), gamebook: true },
        { label: t("elementAdders.template"), action: () => openAddModal("template", null), gamebook: false },
        { label: t("elementAdders.dynamicTemplate"), action: () => openAddModal("dynamictemplate", null), gamebook: false },
        { label: t("elementAdders.type"), action: () => openAddModal("type", null), gamebook: false },
        { label: t("elementAdders.javascript"), action: () => openAddJavascriptModal(), gamebook: true },
    ];
    // Gamebook mode only supports Function/Library/JavaScript — Timer/Walkthrough
    // don't apply to a flat page-based game, and Template/Object Type are in
    // EditorController's m_ignoredTypes for gamebook (adding one would create an
    // invisible, orphaned element).
    let ADVANCED_ADDERS = $derived(ALL_ADVANCED_ADDERS.filter(a => !$isGamebook || a.gamebook));

    let activeTab = $state<string | null>(null);
    let lastKey = $state<string | null>(null);
    let editingItem = $state<{attribute: string, key: string, value: string} | null>(null);
    let newDictItems = $state<Record<string, {key: string, value: string}>>({});
    let attributeErrors = $state<Record<string, string>>({});
    // Last text written to the on-disk copy of a texteditor-bound file (JS/ASLX), so a no-op
    // change is skipped. CodeEditor fires onChange on every blur (and Mod-s) even when the
    // content is untouched — writing the file back anyway would be wasteful, and for library
    // files it would wrongly raise the "reload needed" banner on a pure focus/blur with no edit.
    let lastTexteditorWrite = $state<{ filename: string; text: string } | null>(null);

    function handleTexteditorChange(filename: string, loadedContent: string, value: string) {
        const onDisk = lastTexteditorWrite?.filename === filename ? lastTexteditorWrite.text : loadedContent;
        if (value === onDisk) return;
        lastTexteditorWrite = { filename, text: value };
        if (isLibraryFilename(filename)) {
            void putLibraryAssetText(filename, value).then(result => {
                // A malformed library edit is rejected before it's written (putLibraryAssetText
                // validates against a throwaway controller first), so the game can't be reloaded
                // into an unloadable state. lastTexteditorWrite keeps the rejected text, so
                // blurring again without changing it stays quiet rather than re-validating and
                // re-toasting; a fixed edit changes the value and re-validates normally.
                if (result !== "ok") showToast(result.startsWith("error:") ? result.slice("error:".length) : result, "error");
            });
        } else {
            void putAssetText(filename, value);
        }
    }
    // Refetched whenever the selection changes, for dictionary controls whose keys are
    // object names (e.g. gamebook page "Options" links) rather than free text. The page
    // list serves controls with sourceType "dialoguepage" (the TA page tab's options
    // control), which select pages only — filtered by type, not location.
    let dictSourceObjectNames = $state<string[]>([]);
    let dictSourcePageNames = $state<string[]>([]);
    $effect(() => {
        if ($selectedKey) {
            dictSourceObjectNames = getObjectNames() ?? [];
            dictSourcePageNames = getPageNames() ?? [];
        }
    });
    // Which gamebookoptions control (keyed by attribute) has its "new page" dialog open.
    let newPageModalFor = $state<string | null>(null);

    $effect(() => {
        const key = $selectedKey;
        const data = $selectedData;
        if (key !== lastKey) {
            lastKey = key;
            attributeErrors = {};
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else if (activeTab !== null) {
            // Keep activeTab valid after a data refresh (tab list is stable for the same node)
            const stillExists = data?.tabs.some(t => t.caption === activeTab);
            if (!stillExists) activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else {
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        }
    });

    function recordResult(attribute: string, result: string) {
        const error = result.startsWith("error:") ? result.slice("error:".length) : "";
        attributeErrors = { ...attributeErrors, [attribute]: error };
        // The inline error below the field only stays visible while this element/tab is in
        // view — a toast survives switching to a different tab or element entirely.
        if (error) showToast(error, "error");
    }

    function onTextChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, controlType, value));
    }

    // Routed through setPatternAttribute (not the generic setAttribute/textbox path) so an
    // in-place edit updates the existing IEditableCommandPattern's text rather than replacing
    // it with a raw string - overwriting it would silently reclassify the attribute as the
    // "string" (regex) mode on next render, breaking "#text#"-style simple-pattern placeholders.
    function onPatternTextChange(attribute: string, value: string) {
        if ($selectedKey) recordResult(attribute, setPatternAttribute($selectedKey, attribute, value));
    }

    function onMultiTypeChange(subAttribute: string, newType: string) {
        if ($selectedKey) recordResult(subAttribute, setMultiType($selectedKey, subAttribute, newType));
    }

    function onCheckboxChange(attribute: string, checked: boolean) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, "checkbox", checked.toString()));
    }

    function onNumberChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, controlType, value));
    }

    function onDropdownChange(attribute: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, "dropdown", value));
    }

    function getControlsForView(): ControlInfo[] {
        const data = $selectedData;
        if (!data) return [];
        if (data.tabs.length > 0) {
            return data.tabs.find(t => t.caption === activeTab)?.controls ?? [];
        }
        return data.controls;
    }

    function partitionControls(controls: ControlInfo[]): { main: ControlInfo[]; advanced: ControlInfo[] } {
        const advanced = controls.filter(c => c.advanced);
        if (advanced.length === 0 || advanced.length === controls.length) {
            return { main: controls, advanced: [] };
        }
        return { main: controls.filter(c => !c.advanced), advanced };
    }

    function attrValue(attribute: string): string | null {
        return $selectedData?.attributes[attribute] ?? null;
    }

    function boolValue(attribute: string): boolean {
        const v = attrValue(attribute);
        return v === "True" || v === "true";
    }

    // Keyed by insertBefore (Quest syntax, stable across locales) rather than cmd.command
    // (a localized caption) so the toolbar icons/grouping don't depend on the editor language.
    // These commands stay as always-visible buttons instead of going in the "Insert" dropdown:
    // Bold/Italic/Underline as the most-reached-for formatting, and (gamebook mode only) Page
    // Link since linking between pages is the core authoring action in a gamebook.
    const PINNED_ICONS: Record<string, typeof Bold | undefined> = {
        "<b>": Bold,
        "<i>": Italic,
        "<u>": Underline,
        "{page:": BookOpen,
    };
    const INSERT_MENU_GROUPS: Record<string, { group: string; icon: typeof Bold }> = {
        "{object:": { group: "Links", icon: LinkIcon },
        "{command:": { group: "Links", icon: Terminal },
        "{exit:": { group: "Links", icon: LogOut },
        "{if ": { group: "Conditions", icon: GitBranch },
        "{if not ": { group: "Conditions", icon: GitFork },
        "{here ": { group: "Conditions", icon: MapPin },
        "{nothere ": { group: "Conditions", icon: MapPinOff },
        "{once:": { group: "Other", icon: Repeat1 },
        "{random:": { group: "Other", icon: Dices },
        "{img:": { group: "Other", icon: ImageIcon },
    };
    const INSERT_MENU_GROUP_ORDER = ["Links", "Conditions", "Other", "More"];

    function textProcessorTextareaId(attribute: string): string {
        return `richtext-${attribute}`;
    }

    function insertTextProcessorText(attribute: string, controlType: string, insertBefore: string, insertAfter: string) {
        const textarea = document.getElementById(textProcessorTextareaId(attribute)) as HTMLTextAreaElement | null;
        if (!textarea) return;
        const start = textarea.selectionStart ?? 0;
        const end = textarea.selectionEnd ?? 0;
        const selectedText = textarea.value.substring(start, end);
        textarea.value = textarea.value.substring(0, start) + insertBefore + selectedText + insertAfter + textarea.value.substring(end);
        textarea.selectionStart = start + insertBefore.length;
        textarea.selectionEnd = start + insertBefore.length + selectedText.length;
        textarea.focus();
        onTextChange(attribute, controlType, textarea.value);
    }

    function insertComposedText(attribute: string, controlType: string, range: { start: number; end: number }, text: string) {
        const textarea = document.getElementById(textProcessorTextareaId(attribute)) as HTMLTextAreaElement | null;
        if (!textarea) return;
        textarea.value = textarea.value.substring(0, range.start) + text + textarea.value.substring(range.end);
        const cursor = range.start + text.length;
        textarea.selectionStart = cursor;
        textarea.selectionEnd = cursor;
        textarea.focus();
        onTextChange(attribute, controlType, textarea.value);
    }

    // Commands whose insertBefore appears here need a target picked from a list (or an asset
    // upload) rather than just wrapping the selection in fixed markup — matches the `source`
    // hint the old Quest 5 editor used to decide when to pop up a picker dialog. Keyed by
    // insertBefore for the same locale-independence reason as PINNED_ICONS/INSERT_MENU_GROUPS.
    interface LinkCommandConfig {
        kind: "objects" | "exits" | "pages" | "images";
        textMode: "none" | "optional" | "required";
        targetLabel: string;
        textLabel?: string;
        format: (target: string, text: string) => string;
    }
    const LINK_COMMANDS: Record<string, LinkCommandConfig> = {
        "{object:": {
            kind: "objects", textMode: "optional", targetLabel: t("addElementModal.labels.object"),
            format: (target, text) => (text ? `{object:${target}:${text}}` : `{object:${target}}`),
        },
        "{exit:": {
            kind: "exits", textMode: "none", targetLabel: t("propertyEditor.linkTargetLabels.exit"),
            format: (target) => `{exit:${target}}`,
        },
        "{here ": {
            kind: "objects", textMode: "required", targetLabel: t("addElementModal.labels.object"), textLabel: t("propertyEditor.linkTextToShow"),
            format: (target, text) => `{here ${target}:${text}}`,
        },
        "{nothere ": {
            kind: "objects", textMode: "required", targetLabel: t("addElementModal.labels.object"), textLabel: t("propertyEditor.linkTextToShow"),
            format: (target, text) => `{nothere ${target}:${text}}`,
        },
        "{img:": {
            kind: "images", textMode: "none", targetLabel: t("propertyEditor.linkTargetLabels.image"),
            format: (target) => `{img:${target}}`,
        },
        "{page:": {
            kind: "pages", textMode: "optional", targetLabel: t("addElementModal.labels.page"),
            format: (target, text) => (text ? `{page:${target}:${text}}` : `{page:${target}}`),
        },
    };

    interface ActiveLinkCommand {
        title: string;
        config: LinkCommandConfig;
        options: ControlOption[];
        initialText: string;
        range: { start: number; end: number };
        attribute: string;
        controlType: string;
    }
    let activeLinkCommand = $state<ActiveLinkCommand | null>(null);

    // Entry point for every toolbar/menu command: link commands (see LINK_COMMANDS) open the
    // picker modal, everything else keeps the old immediate wrap-the-selection insert.
    function activateTextProcessorCommand(cmd: TextProcessorCommand, attribute: string, controlType: string) {
        const config = LINK_COMMANDS[cmd.insertBefore];
        if (!config) {
            insertTextProcessorText(attribute, controlType, cmd.insertBefore, cmd.insertAfter);
            return;
        }
        const textarea = document.getElementById(textProcessorTextareaId(attribute)) as HTMLTextAreaElement | null;
        const start = textarea?.selectionStart ?? 0;
        const end = textarea?.selectionEnd ?? 0;
        // Gamebook has no "dialoguepage" type - every object under _objects is a page there,
        // so GetPageNames() (which filters by that type) would come back empty; use the plain
        // object list in that mode, and the type-filtered one for Text Adventure's real pages.
        const options: ControlOption[] =
            config.kind === "exits" ? (getExitNames() ?? []).map(name => ({ value: name, label: name })) :
                config.kind === "images" ? [] :
                    config.kind === "pages" ? ((($isGamebook ? getObjectNames() : getPageNames()) ?? []).map(name => ({ value: name, label: name }))) :
                    (getObjectNames() ?? []).map(name => ({ value: name, label: name }));
        activeLinkCommand = {
            title: cmd.command,
            config,
            options,
            initialText: textarea?.value.substring(start, end) ?? "",
            range: { start, end },
            attribute,
            controlType,
        };
    }

    function confirmLinkCommand(target: string, text: string) {
        if (!activeLinkCommand) return;
        const { config, range, attribute, controlType } = activeLinkCommand;
        insertComposedText(attribute, controlType, range, config.format(target, text));
        activeLinkCommand = null;
    }

    // Commands not covered by PINNED_ICONS go into the "Insert" dropdown, grouped by
    // INSERT_MENU_GROUPS; anything not in that map (e.g. a future CoreEditor.aslx addition)
    // still shows up, ungrouped, under "More" rather than silently disappearing.
    function buildInsertMenuItems(commands: TextProcessorCommand[], attribute: string, controlType: string): DropdownMenuItem[] {
        const byGroup: Record<string, TextProcessorCommand[]> = {};
        for (const cmd of commands) {
            if (PINNED_ICONS[cmd.insertBefore]) continue;
            const group = INSERT_MENU_GROUPS[cmd.insertBefore]?.group ?? "More";
            (byGroup[group] ??= []).push(cmd);
        }
        const items: DropdownMenuItem[] = [];
        let seenGroup = false;
        for (const group of INSERT_MENU_GROUP_ORDER) {
            const cmds = byGroup[group];
            if (!cmds) continue;
            cmds.forEach((cmd, i) => {
                items.push({
                    label: cmd.command,
                    icon: INSERT_MENU_GROUPS[cmd.insertBefore]?.icon,
                    heading: i === 0 ? group : undefined,
                    divider: i === 0 && seenGroup,
                    action: () => activateTextProcessorCommand(cmd, attribute, controlType),
                });
            });
            seenGroup = true;
        }
        return items;
    }

    function focusOnMount(node: HTMLElement) {
        node.focus();
        if (node instanceof HTMLInputElement || node instanceof HTMLTextAreaElement) node.select();
    }

    function tabClass(caption: string | null): string {
        return activeTab === caption
            ? "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-primary-600-400 border-b-2 border-primary-500 font-medium"
            : "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-surface-600-400 hover:text-surface-900-100";
    }

    // The "_objects" header's single tab is captioned "Objects" server-side (see
    // CoreEditorElements.aslx's EditorElementsObjects template), which has no game-type
    // awareness. Override the displayed label in Gamebook mode, echoing TreePanel's
    // headerText() override for the same node - the underlying caption stays the tab identity.
    function tabLabel(caption: string | null): string {
        if ($selectedKey === "_objects" && $isGamebook && caption) return t("treePanel.header.pages");
        return caption ?? t("propertyEditor.tabFallback");
    }
</script>

<div class="@container flex flex-col flex-1 bg-surface-50-950 overflow-hidden">
    {#if onback}
        <div class="px-3 py-2 border-b border-surface-200-800">
            <button
                type="button"
                class="flex items-center gap-1 -ml-1 px-1 text-sm font-medium text-surface-900-50"
                onclick={onback}
            ><ChevronLeft size={16} /> {selectedNode?.text ?? t("propertyEditor.propertiesFallback")}</button>
        </div>
    {/if}
    <LibraryElementBanner />

    {#if $selectedKey === null}
        <p class="px-3 py-4 text-sm text-surface-600-400">{t("propertyEditor.selectPrompt")}</p>
    {:else if $selectedKey === "_advanced"}
        <div class="flex flex-col items-start gap-1.5 px-3 py-3">
            {#each ADVANCED_ADDERS as adder (adder.label)}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={adder.action}
                >+ {adder.label}</button>
            {/each}
        </div>
    {:else if $selectedData === null}
        <p class="px-3 py-4 text-sm text-surface-600-400">{t("propertyEditor.noProperties")}</p>
    {:else}
        {#if $selectedData.tabs.length > 0}
            <div class="flex border-b border-surface-200-800 overflow-x-auto flex-shrink-0">
                {#each $selectedData.tabs as tab, ti (ti)}
                    <button
                        type="button"
                        class={tabClass(tab.caption)}
                        onclick={() => { activeTab = tab.caption; }}
                    >
                        {tabLabel(tab.caption)}
                    </button>
                {/each}
            </div>
        {/if}

        {@const viewControls = getControlsForView()}
        {@const hasAttributesPanel = viewControls.some(c => c.controlType === "attributes")}
        {@const readOnly = $selectedData.isLibraryElement}
        {#if hasAttributesPanel}
            {@const { main, advanced } = partitionControls(viewControls.filter(c => c.controlType !== "attributes"))}
            <div class="flex-1 overflow-hidden flex flex-col min-h-0 {readOnly ? "pointer-events-none opacity-60" : ""}">
                <AttributesEditor>
                    {#snippet extraControls()}
                        {#each main as ctrl, i (i)}
                            {@render controlRow(ctrl)}
                        {/each}
                        {@render advancedExpander(advanced)}
                    {/snippet}
                </AttributesEditor>
            </div>
        {:else}
            {@const { main, advanced } = partitionControls(viewControls)}
            <div class="flex-1 overflow-y-auto {readOnly ? "pointer-events-none opacity-60" : ""}">
                {#each main as ctrl, i (i)}
                    {@render controlRow(ctrl)}
                {/each}
                {@render advancedExpander(advanced)}
            </div>
        {/if}
    {/if}
</div>

{#if activeLinkCommand}
    <LinkPickerModal
        title={activeLinkCommand.title}
        targetLabel={activeLinkCommand.config.targetLabel}
        targetOptions={activeLinkCommand.options}
        assetSource={activeLinkCommand.config.kind === "images" ? "*.jpg;*.jpeg;*.png;*.gif" : null}
        textMode={activeLinkCommand.config.textMode}
        textLabel={activeLinkCommand.config.textLabel}
        initialText={activeLinkCommand.initialText}
        onconfirm={confirmLinkCommand}
        oncancel={() => { activeLinkCommand = null; }}
    />
{/if}

{#snippet textProcessorPanel(commands: TextProcessorCommand[], attribute: string, controlType: string)}
    <div class="flex items-center gap-1 shrink-0">
        {#each commands as cmd (cmd.command)}
            {@const Icon = PINNED_ICONS[cmd.insertBefore]}
            {#if Icon}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
                    title="{cmd.command} {cmd.info}"
                    aria-label={cmd.command}
                    onclick={() => activateTextProcessorCommand(cmd, attribute, controlType)}
                ><Icon size={14} aria-hidden="true" /></button>
            {/if}
        {/each}
        <span class="w-px h-5 bg-surface-200-800 mx-0.5"></span>
        <DropdownMenu items={buildInsertMenuItems(commands, attribute, controlType)} align="left">
            {#snippet trigger(toggle, open)}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 gap-1"
                    onclick={toggle}
                    aria-haspopup="menu"
                    aria-expanded={open}
                ><ListPlus size={14} aria-hidden="true" />{t("common.insert")}<ChevronDown size={12} aria-hidden="true" /></button>
            {/snippet}
        </DropdownMenu>
        <a
            href="https://docs.textadventures.co.uk/quest/text_processor.html"
            target="_blank"
            class="btn btn-sm text-xs px-2 py-0.5 text-surface-600-400 ml-auto"
            title={t("propertyEditor.textProcessorHelp")}
            aria-label={t("propertyEditor.textProcessorHelp")}
        ><LifeBuoy size={14} aria-hidden="true" /></a>
    </div>
{/snippet}

{#snippet controlOnly(ctrl: ControlInfo)}
    {#if ctrl.controlType === "number"}
        <input
            type="number"
            min={ctrl.minimum ?? undefined}
            max={ctrl.maximum ?? undefined}
            step={ctrl.increment ?? undefined}
            class="input text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onNumberChange(ctrl.attribute!, "number", (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "numberdouble"}
        <input
            type="number"
            min={ctrl.minimum ?? undefined}
            max={ctrl.maximum ?? undefined}
            step={ctrl.increment ?? "any"}
            class="input text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onNumberChange(ctrl.attribute!, "numberdouble", (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "dropdown" && ctrl.options}
        {#if ctrl.freetext}
            <Combobox
                value={attrValue(ctrl.attribute!) ?? ""}
                options={ctrl.options}
                onchange={(v) => onDropdownChange(ctrl.attribute!, v)}
                class="input text-xs py-0.5 px-1.5 w-auto min-w-24"
            />
        {:else}
            <!-- No <freetext/> hint - restrict to the listed options, unlike Combobox which
                 always accepts arbitrary typed text. -->
            <select
                class="select text-xs py-0.5 px-1.5 w-auto"
                value={attrValue(ctrl.attribute!) ?? ""}
                onchange={(e) => onDropdownChange(ctrl.attribute!, (e.target as HTMLSelectElement).value)}
            >
                {#each ctrl.options as opt (opt.value)}
                    <option value={opt.value}>{opt.label || opt.value || t("common.none")}</option>
                {/each}
            </select>
        {/if}
    {:else if ctrl.controlType === "dropdowntypes" && ctrl.options && ctrl.attribute}
        <select
            class="select text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute) ?? "*"}
            onchange={(e) => $selectedKey && setDropdownType($selectedKey, ctrl.attribute!, (e.target as HTMLSelectElement).value)}
        >
            {#each ctrl.options as opt, oi (oi)}
                <option value={opt.value}>{opt.label}</option>
            {/each}
        </select>
    {:else if ctrl.controlType === "richtext"}
        {#if ctrl.textProcessorCommands?.length}
            <div class="flex flex-col gap-1 w-full">
                {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.attribute!, ctrl.controlType)}
                <textarea
                    id={textProcessorTextareaId(ctrl.attribute!)}
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                    value={attrValue(ctrl.attribute!) ?? ""}
                    onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
                ></textarea>
            </div>
        {:else}
            <textarea
                autocapitalize="off"
                class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                value={attrValue(ctrl.attribute!) ?? ""}
                onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
            ></textarea>
        {/if}
    {:else if ctrl.controlType === "textbox"}
        <input
            type="text"
            autocapitalize="off"
            class={"input text-xs py-0.5 px-1.5 w-full" + (ctrl.attribute && attributeErrors[ctrl.attribute] ? " !border-error-500" : "")}
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "expression"}
        <ExpressionInput
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(v) => onTextChange(ctrl.attribute!, ctrl.controlType, v)}
            objectNames={dictSourceObjectNames}
            class={"input text-xs py-0.5 px-1.5 w-full" + (ctrl.attribute && attributeErrors[ctrl.attribute] ? " !border-error-500" : "")}
        />
    {:else if ctrl.controlType === "filter" && ctrl.options}
        <select
            class="select text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => $selectedKey && setSelectedFilter($selectedKey, ctrl.subAttribute!, (e.target as HTMLSelectElement).value)}
        >
            {#each ctrl.options as opt (opt.value)}
                <option value={opt.value}>{opt.label}</option>
            {/each}
        </select>
    {:else if ctrl.controlType === "gameid"}
        <div class="flex items-center gap-2 w-full">
            <input
                type="text"
                autocapitalize="off"
                class="input text-xs py-0.5 px-1.5 flex-1"
                readonly
                value={attrValue(ctrl.attribute!) ?? ""}
            />
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 whitespace-nowrap"
                onclick={() => onTextChange(ctrl.attribute!, "textbox", crypto.randomUUID())}
            >{t("propertyEditor.generateButton")}</button>
        </div>
    {:else if ctrl.controlType === "file"}
        <AssetPicker
            value={attrValue(ctrl.attribute!) ?? ""}
            source={ctrl.source}
            creatable={!!ctrl.newFile}
            readonly={!!(ctrl.lockedAfterCreate && attrValue(ctrl.attribute!))}
            onchange={(v) => onTextChange(ctrl.attribute!, ctrl.controlType, v)}
            containerClass="w-full"
        />
    {:else if ctrl.controlType === "list" && ctrl.attribute && $selectedKey}
        <ListEditor elementKey={$selectedKey} attribute={ctrl.attribute} value={attrValue(ctrl.attribute)} addPrompt={ctrl.addPrompt ?? undefined} isWalkthrough={ctrl.isWalkthrough ?? false} />
    {:else if (ctrl.controlType === "stringdictionary" || ctrl.controlType === "gamebookoptions") && ctrl.attribute}
        {@const items = (() => { try { return JSON.parse(attrValue(ctrl.attribute) ?? "[]") as {key: string, value: string}[]; } catch { return []; } })()}
        {@const dk = ctrl.attribute}
        {@const isObjectSource = ctrl.source === "object"}
        {@const sourcePool = ctrl.sourceType === "dialoguepage" ? dictSourcePageNames : dictSourceObjectNames}
        {@const excludedNames = new Set((ctrl.sourceExclude ?? "").split(/[;,]/).map(s => s.trim()).filter(Boolean))}
        {@const availableObjectNames = isObjectSource ? sourcePool.filter(n => !excludedNames.has(n) && !items.some(i => i.key === n)) : []}
        <div class="flex flex-col gap-1 w-full">
            {#each items as item (item.key)}
                {@const isEditing = editingItem?.attribute === dk && editingItem?.key === item.key}
                <div class="flex items-center gap-1">
                    <span class="text-xs text-surface-600-400 w-24 flex-shrink-0 truncate" title={item.key}>{item.key}</span>
                    {#if isEditing}
                        <input
                            type="text"
                            autocapitalize="off"
                            class="input text-xs py-0.5 px-1.5 flex-1"
                            use:focusOnMount
                            value={editingItem!.value}
                            oninput={(e) => { if (editingItem) editingItem.value = (e.target as HTMLInputElement).value; }}
                            onkeydown={(e) => {
                                if (e.key === "Enter" && $selectedKey && editingItem) {
                                    updateDictItem($selectedKey, dk, editingItem.key, editingItem.value);
                                    editingItem = null;
                                } else if (e.key === "Escape") {
                                    editingItem = null;
                                }
                            }}
                            onblur={() => {
                                if ($selectedKey && editingItem) {
                                    updateDictItem($selectedKey, dk, editingItem.key, editingItem.value);
                                    editingItem = null;
                                }
                            }}
                        />
                    {:else}
                        <button
                            type="button"
                            class="text-xs flex-1 text-left px-1.5 py-0.5 hover:text-primary-600-400 truncate"
                            onclick={() => { editingItem = { attribute: dk, key: item.key, value: item.value }; }}
                        >{item.value}</button>
                    {/if}
                    {#if isObjectSource}
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                            title={t("propertyEditor.goTo", { name: item.key })}
                            onclick={() => selectNode(item.key)}
                        ><ArrowRight size={11} /></button>
                    {/if}
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                        onclick={() => $selectedKey && removeDictItem($selectedKey, dk, item.key)}
                    >✕</button>
                </div>
            {/each}
            <div class="flex items-center gap-1 mt-0.5">
                {#if isObjectSource}
                    <select
                        class="select text-xs py-0.5 px-1.5 w-24 flex-shrink-0"
                        aria-label={ctrl.keyPrompt ?? t("propertyEditor.keyFallback")}
                        title={ctrl.keyPrompt ?? undefined}
                        data-staging
                        value={newDictItems[dk]?.key ?? ""}
                        onchange={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), key: (e.target as HTMLSelectElement).value }; }}
                    >
                        <option value="">{t("propertyEditor.selectOption")}</option>
                        {#each availableObjectNames as name (name)}
                            <option value={name}>{name}</option>
                        {/each}
                    </select>
                {:else}
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0.5 px-1.5 w-24 flex-shrink-0"
                        placeholder={ctrl.keyPrompt ?? t("propertyEditor.keyFallback")}
                        data-staging
                        value={newDictItems[dk]?.key ?? ""}
                        oninput={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), key: (e.target as HTMLInputElement).value }; }}
                    />
                {/if}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    placeholder={ctrl.valuePrompt ?? t("propertyEditor.valueFallback")}
                    data-staging
                    value={newDictItems[dk]?.value ?? ""}
                    oninput={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), value: (e.target as HTMLInputElement).value }; }}
                    onkeydown={(e) => {
                        if (e.key === "Enter" && $selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = { key: "", value: "" };
                        }
                    }}
                />
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                    onclick={() => {
                        if ($selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = { key: "", value: "" };
                        }
                    }}
                >{t("propertyEditor.addButton")}</button>
                {#if ctrl.controlType === "gamebookoptions"}
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 whitespace-nowrap"
                        onclick={() => { newPageModalFor = dk; }}
                    >+ {t("propertyEditor.newPageButton")}</button>
                {/if}
            </div>
        </div>
        {#if newPageModalFor === dk}
            {@const newPageParent = $treeNodes.find(n => n.key === $selectedKey)?.parent ?? null}
            <AddElementModal
                elementType="page"
                parent={newPageParent}
                onconfirm={(name) => {
                    newPageModalFor = null;
                    if (!$selectedKey) return;
                    const result = createPageSilent(name, newPageParent);
                    if (result.startsWith("error:")) {
                        showToast(result.slice("error:".length));
                        return;
                    }
                    const value = newDictItems[dk]?.value?.trim() || result;
                    addDictItem($selectedKey, dk, result, value);
                    newDictItems[dk] = { key: "", value: "" };
                    editingItem = { attribute: dk, key: result, value };
                }}
                oncancel={() => { newPageModalFor = null; }}
            />
        {/if}
    {:else if ctrl.controlType === "objects" && ctrl.options}
        <Combobox
            value={attrValue(ctrl.attribute!) ?? ""}
            options={ctrl.options}
            onchange={(v) => $selectedKey && setObjectReference($selectedKey, ctrl.attribute!, v)}
            class="input text-xs py-0.5 px-1.5 w-auto min-w-24"
        />
    {:else if ctrl.controlType === "multi" && ctrl.options}
        {@const selectedType = attrValue(ctrl.attribute!) ?? "null"}
        {@const subEditorType = ctrl.subEditors?.find(e => e.value === selectedType)?.label ?? selectedType}
        <div class="flex flex-col gap-1 w-full">
            <select
                class="select text-xs py-0.5 px-1.5 w-auto self-start"
                value={selectedType}
                onchange={(e) => onMultiTypeChange(ctrl.subAttribute!, (e.target as HTMLSelectElement).value)}
            >
                {#each ctrl.options as opt (opt.value)}
                    <option value={opt.value}>{opt.label}</option>
                {/each}
            </select>
            {#if subEditorType === "richtext" && ctrl.subAttribute !== null}
                {#if ctrl.textProcessorCommands?.length}
                    <div class="flex flex-col gap-1 w-full">
                        {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                        <textarea
                            id={textProcessorTextareaId(ctrl.subAttribute)}
                            autocapitalize="off"
                            class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                    </div>
                {:else}
                    <textarea
                        autocapitalize="off"
                        class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                    ></textarea>
                {/if}
            {:else if (subEditorType === "textbox" || subEditorType === "string") && ctrl.subAttribute !== null}
                <!-- "string" (with no <editors> override) falls through to a plain textbox by
                     default - e.g. Command's "Regular expression" pattern mode and Use/Give's
                     "Print a message" mode. A control that wants richtext instead (e.g. object
                     descriptions) opts in via <editors>string=richtext</editors>. -->
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.subAttribute) ?? ""}
                    onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                />
            {:else if subEditorType === "simplepattern" && ctrl.subAttribute !== null}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.subAttribute) ?? ""}
                    onchange={(e) => onPatternTextChange(ctrl.subAttribute!, (e.target as HTMLInputElement).value)}
                />
            {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
            {:else if subEditorType === "boolean" && ctrl.subAttribute !== null}
                <label class="flex items-center gap-2">
                    <input
                        type="checkbox"
                        class="checkbox flex-shrink-0"
                        checked={boolValue(ctrl.subAttribute)}
                        onchange={(e) => onCheckboxChange(ctrl.subAttribute!, (e.target as HTMLInputElement).checked)}
                    />
                    <span class="text-xs text-surface-600-400">{ctrl.checkboxCaption ?? ctrl.subAttribute}</span>
                </label>
            {:else if subEditorType === "scriptdictionary" && ctrl.subAttribute !== null && $selectedKey !== null}
                <ScriptDictionaryEditor
                    elementKey={$selectedKey}
                    attribute={ctrl.subAttribute}
                    value={attrValue(ctrl.subAttribute)}
                    keySource={ctrl.source === "object" ? "object" : "text"}
                />
            {/if}
        </div>
    {:else if ctrl.controlType === "script" && ctrl.attribute !== null && $selectedKey !== null}
        <div class="flex-1 min-w-0 overflow-hidden">
            <ScriptEditor elementKey={$selectedKey} attribute={ctrl.attribute} />
        </div>
    {:else if ctrl.controlType === "texteditor" && ctrl.attribute !== null}
        {@const filename = attrValue(ctrl.attribute)}
        <!-- The same control type drives both file kinds: a Javascript element's src (.js — the
             original use, editable in place) and an Included Library's filename (.aslx, or .xml
             for compatibility with libraries authored in Quest 5's desktop editor — see
             CoreEditorIncludedLibrary.aslx and isLibraryFilename). Library content is handled differently because it's
             consumed at load time, not at play time: custom libraries are written via
             putLibraryAssetText (which flags that a reload is needed), and the engine-shipped
             built-in libraries are shown read-only since they don't belong to the game at all. -->
        {@const isLibraryFile = !!filename && isLibraryFilename(filename)}
        {@const builtInLibrary = isLibraryFile && !!filename && isBuiltInLibrary(filename)}
        {@const language = isLibraryFile ? "xml" : "javascript"}
        <div class="w-full min-h-64">
            {#if !filename}
                <p class="text-xs text-surface-600-400">{t("propertyEditor.chooseOrUploadPrompt")}</p>
            {:else if builtInLibrary}
                {@const content = filename ? getLibraryXml(filename) : null}
                {#if content === null}
                    <p class="text-xs text-surface-600-400">{t("propertyEditor.libraryNotReadable")}</p>
                {:else}
                    <div class="flex flex-col gap-1">
                        <p class="text-xs text-surface-600-400">{t("propertyEditor.libraryReadOnly")}</p>
                        <CodeEditor
                            value={content}
                            language={language}
                            readonly
                            minHeight="16rem"
                            class="h-full"
                        />
                    </div>
                {/if}
            {:else}
                {#await getAssetText(filename)}
                    <p class="text-xs text-surface-600-400">{t("propertyEditor.loading")}</p>
                {:then content}
                    <CodeEditor
                        value={content ?? ""}
                        language={language}
                        onChange={(v) => handleTexteditorChange(filename, content ?? "", v)}
                        minHeight="16rem"
                        class="h-full"
                    />
                {/await}
            {/if}
        </div>
    {:else if ctrl.controlType === "scriptdictionary" && ctrl.attribute && $selectedKey}
        <ScriptDictionaryEditor
            elementKey={$selectedKey}
            attribute={ctrl.attribute}
            value={attrValue(ctrl.attribute)}
            keySource={ctrl.source === "object" ? "object" : "text"}
        />
    {:else}
        {#if attrValue(ctrl.attribute!) !== null}
            <span class="text-xs overflow-hidden text-ellipsis whitespace-nowrap" title={attrValue(ctrl.attribute!) ?? ""}>
                {attrValue(ctrl.attribute!)}
            </span>
        {:else}
            <em class="text-xs text-surface-600-400">{t("propertyEditor.nullValue")}</em>
        {/if}
    {/if}
{/snippet}

{#snippet advancedExpander(controls: ControlInfo[])}
    {#if controls.length > 0}
        <details class="mt-2 border-t border-surface-200-800">
            <summary class="px-3 pt-2.5 pb-1.5 text-xs font-semibold uppercase text-surface-600-400 cursor-pointer select-none">
                {t("common.advanced")}
            </summary>
            {#each controls as ctrl, i (i)}
                {@render controlRow(ctrl)}
            {/each}
        </details>
    {/if}
{/snippet}

{#snippet controlRow(ctrl: ControlInfo)}
    {#if ctrl.controlType === "attributes"}
        <AttributesEditor />
    {:else if ctrl.controlType === "title"}
        <div class="px-3 pt-3 pb-1 text-xs font-semibold text-surface-600-400 uppercase tracking-wide">
            {ctrl.caption ?? ""}
        </div>
    {:else if ctrl.controlType === "label"}
        {#if ctrl.href}
            <a
                href={ctrl.href}
                target="_blank"
                rel="noopener noreferrer"
                class="block px-3 py-1 text-xs text-primary-600-400 italic hover:underline"
            >{ctrl.caption ?? ""}</a>
        {:else}
            <div class="px-3 py-1 text-xs text-surface-600-400 italic">
                {ctrl.caption ?? ""}
            </div>
        {/if}
    {:else if ctrl.controlType === "elementslist" && $selectedKey}
        <div class="flex flex-col gap-1">
            {#if ctrl.caption}
                <span class="text-xs text-surface-600-400 px-3 pt-1.5">{ctrl.caption}</span>
            {/if}
            <ElementsList
                elementKey={$selectedKey}
                elementType={ctrl.elementType ?? "object"}
                objectType={ctrl.objectType}
                listFilter={ctrl.listFilter}
            />
        </div>
    {:else if ctrl.controlType === "exits" && $selectedKey}
        <ExitsEditor elementKey={$selectedKey} />
    {:else if ctrl.controlType === "verbs" && $selectedKey}
        <VerbsEditor elementKey={$selectedKey} />
    {:else if ctrl.attribute !== null}
        {#if ctrl.controlType === "checkbox"}
            <label class="flex items-center gap-2 px-3 py-1.5 min-h-8 cursor-pointer">
                <input
                    type="checkbox"
                    class="checkbox flex-shrink-0"
                    checked={boolValue(ctrl.attribute)}
                    onchange={(e) => onCheckboxChange(ctrl.attribute!, (e.target as HTMLInputElement).checked)}
                />
                <span class="text-xs text-surface-600-400">
                    {ctrl.caption ?? ctrl.attribute}
                </span>
            </label>
        {:else if ctrl.controlType === "multi" && ctrl.options}
            {@const label = ctrl.caption ?? ctrl.attribute}
            {@const selectedType = attrValue(ctrl.attribute!) ?? "null"}
            {@const subEditorType = ctrl.subEditors?.find(e => e.value === selectedType)?.label ?? selectedType}
            <div class="flex flex-col gap-1 px-3 py-1.5">
                <label class="flex items-center gap-2">
                    <span class="text-xs text-surface-600-400 whitespace-nowrap">{label}:</span>
                    <select
                        class="select text-xs py-0.5 px-1.5 w-auto"
                        value={selectedType}
                        onchange={(e) => onMultiTypeChange(ctrl.subAttribute!, (e.target as HTMLSelectElement).value)}
                    >
                        {#each ctrl.options as opt (opt.value)}
                            <option value={opt.value}>{opt.label}</option>
                        {/each}
                    </select>
                </label>
                {#if subEditorType === "richtext" && ctrl.subAttribute !== null}
                    {#if ctrl.textProcessorCommands?.length}
                        <div class="flex flex-col gap-1 w-full">
                            {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                            <textarea
                                id={textProcessorTextareaId(ctrl.subAttribute)}
                                autocapitalize="off"
                                aria-label={label}
                                class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                                value={attrValue(ctrl.subAttribute) ?? ""}
                                onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                            ></textarea>
                        </div>
                    {:else}
                        <textarea
                            autocapitalize="off"
                            aria-label={label}
                            class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                    {/if}
                {:else if (subEditorType === "textbox" || subEditorType === "string") && ctrl.subAttribute !== null}
                    <input
                        type="text"
                        autocapitalize="off"
                        aria-label={label}
                        class="input text-xs py-0.5 px-1.5 w-full"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                    />
                {:else if subEditorType === "simplepattern" && ctrl.subAttribute !== null}
                    <input
                        type="text"
                        autocapitalize="off"
                        aria-label={label}
                        class="input text-xs py-0.5 px-1.5 w-full"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onPatternTextChange(ctrl.subAttribute!, (e.target as HTMLInputElement).value)}
                    />
                {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                    <div role="group" aria-label={label} class="contents">
                        <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
                    </div>
                {:else if subEditorType === "boolean" && ctrl.subAttribute !== null}
                    <label class="flex items-center gap-2">
                        <input
                            type="checkbox"
                            class="checkbox flex-shrink-0"
                            checked={boolValue(ctrl.subAttribute)}
                            onchange={(e) => onCheckboxChange(ctrl.subAttribute!, (e.target as HTMLInputElement).checked)}
                        />
                        <span class="text-xs text-surface-600-400">{ctrl.checkboxCaption ?? ctrl.subAttribute}</span>
                    </label>
                {:else if subEditorType === "scriptdictionary" && ctrl.subAttribute !== null && $selectedKey !== null}
                    <div role="group" aria-label={label} class="contents">
                        <ScriptDictionaryEditor
                            elementKey={$selectedKey}
                            attribute={ctrl.subAttribute}
                            value={attrValue(ctrl.subAttribute)}
                            keySource={ctrl.source === "object" ? "object" : "text"}
                        />
                    </div>
                {/if}
            </div>
        {:else}
            {@const label = ctrl.caption ?? ctrl.attribute}
            {@const isMultiline = ctrl.controlType === "richtext" || ctrl.controlType === "script" || ctrl.controlType === "texteditor" || ctrl.controlType === "list" || ctrl.controlType === "stringdictionary" || ctrl.controlType === "scriptdictionary" || ctrl.controlType === "gamebookoptions"}
            {@const stacksBelowLabel = label.length > 20 || isMultiline}
            {#if stacksBelowLabel}
                <div class="flex flex-col gap-1 px-3 py-1.5">
                    <svelte:element
                        this={isMultiline ? "div" : "label"}
                        role={isMultiline ? "group" : undefined}
                        aria-label={isMultiline ? label : undefined}
                        class="contents"
                    >
                        {#if ctrl.controlType !== "texteditor"}
                            <span class="text-xs text-surface-600-400">{label}:</span>
                        {/if}
                        {@render controlOnly(ctrl)}
                    </svelte:element>
                    {#if ctrl.attribute && attributeErrors[ctrl.attribute]}
                        <p class="text-xs text-error-500" role="alert">{attributeErrors[ctrl.attribute]}</p>
                    {/if}
                </div>
            {:else}
                <div class="flex flex-col gap-1 px-3 py-1.5">
                    <label class="flex items-center gap-2 min-h-8 cursor-pointer">
                        <span class="text-xs text-surface-600-400 w-32 flex-shrink-0">{label}:</span>
                        {@render controlOnly(ctrl)}
                    </label>
                    {#if ctrl.attribute && attributeErrors[ctrl.attribute]}
                        <p class="text-xs text-error-500" role="alert">{attributeErrors[ctrl.attribute]}</p>
                    {/if}
                </div>
            {/if}
        {/if}
    {/if}
{/snippet}
