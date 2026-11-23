<script lang="ts">
    // The game's fonts: the ones copied into it, and adding more from Fontsource (Google Fonts and
    // other open source fonts). An added font is copied into the game's folder as one file, so only
    // adding one needs the internet: the game's fonts work offline, in the Preview and when
    // publishing, and the published game doesn't ask Google for anything. See lib/fonts.ts.
    import { onMount } from "svelte";
    import {
        addFont, DEFAULT_SUBSETS, fallbackFor, fetchCatalogue, fetchFontDetails, firstFamily, fontStack, gameFonts,
        listFont, previewUrl, RECOMMENDED_FONTS, removeFont, subsetLabel,
        type FontDetails, type FontSummary, type GameFont,
    } from "#lib/fonts.js";
    import { setAttribute, removeAttribute } from "#lib/editor-store.js";
    import { confirmDialog } from "#lib/confirm.js";
    import { trapFocus } from "#lib/actions/trapFocus.js";
    import { t } from "#lib/i18n/index.js";
    import { docsUrlForPath } from "#lib/docs-links.js";

    interface Props {
        gameKey: string;
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
        // Whether the game has an interface font setting (not on the classic interface or a gamebook)
        interfaceFont: boolean;
        oncancel: () => void;
    }
    const { gameKey, get, interfaceFont, oncancel }: Props = $props();

    let busy = $state(false);
    let error = $state("");
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    const usedFor = (font: GameFont) => (["defaultfont", ...(interfaceFont ? ["interfacefont"] : [])])
        .filter(attribute => firstFamily(get(attribute) ?? "").toLowerCase() === font.family.toLowerCase());

    // Fonts from Fontsource, shown in their own font if it can be downloaded
    // Which previews have been asked for: not shown, so not reactive
    const loaded: Record<string, true> = {};
    let previewed = $state<Record<string, boolean>>({});
    function loadPreview(id: string) {
        if (loaded[id]) return;
        loaded[id] = true;
        new FontFace(`qv-preview-${id}`, `url(${previewUrl(id)})`).load().then(face => {
            document.fonts.add(face);
            previewed = { ...previewed, [id]: true };
        }, () => {});
    }
    const previewFamily = (id: string, category: string) =>
        previewed[id] ? `"qv-preview-${id}", ${fallbackFor(category)}` : fallbackFor(category);

    // Adding a font
    let online = $state(typeof navigator === "undefined" || navigator.onLine);
    onMount(() => {
        const update = () => { online = navigator.onLine; };
        window.addEventListener("online", update);
        window.addEventListener("offline", update);
        return () => {
            window.removeEventListener("online", update);
            window.removeEventListener("offline", update);
        };
    });

    let search = $state("");
    let catalogue = $state<FontSummary[] | null>(null);
    let catalogueError = $state(false);
    $effect(() => {
        if (!online || !search.trim() || catalogue) return;
        catalogueError = false;
        fetchCatalogue().then(list => { catalogue = list; }, () => { catalogueError = true; });
    });
    const results = $derived.by((): { id: string; family: string; category: string }[] => {
        const query = search.trim().toLowerCase();
        if (!query) return RECOMMENDED_FONTS;
        if (!catalogue) return [];
        return catalogue.filter(f => f.family.toLowerCase().includes(query))
            .sort((a, b) => Number(!a.family.toLowerCase().startsWith(query)) - Number(!b.family.toLowerCase().startsWith(query)))
            .slice(0, 40);
    });
    $effect(() => { if (online) for (const font of results) loadPreview(font.id); });

    let chosen = $state<FontDetails | null>(null);
    let chosenError = $state("");
    let subsets = $state<string[]>([]);
    let useForText = $state(true);
    let progress = $state("");

    async function choose(id: string) {
        chosenError = "";
        try {
            const details = await fetchFontDetails(id);
            chosen = details;
            const defaults = details.subsets.filter(s => DEFAULT_SUBSETS.includes(s));
            subsets = defaults.length ? defaults : details.subsets.slice(0, 1);
            loadPreview(id);
        } catch {
            chosenError = t("fontsModal.fontError");
        }
    }

    function toggleSubset(subset: string, on: boolean) {
        subsets = on ? [...subsets, subset] : subsets.filter(s => s !== subset);
    }

    async function add() {
        if (!chosen || !subsets.length) return;
        const font = chosen;
        if ($gameFonts.some(f => f.file === `${font.id}.font.css`)
            && !(await confirmDialog(t("fontsModal.addAgain", { font: font.family }), { confirmLabel: t("fontsModal.download") }))) return;
        busy = true;
        error = "";
        progress = t("fontsModal.downloading");
        try {
            await addFont(gameKey, get("fontfiles"), font, subsets, (done, total) => {
                progress = t("fontsModal.downloadingProgress", { done, total });
            });
            if (useForText) {
                const result = setAttribute(gameKey, "defaultfont", "font", fontStack(font.family, font.category));
                if (result !== "ok") throw new Error(result);
            }
            chosen = null;
            search = "";
        } catch (err) {
            error = err instanceof Error ? err.message : String(err);
        } finally {
            busy = false;
            progress = "";
        }
    }

    function load(font: GameFont) {
        const result = listFont(gameKey, font);
        if (result !== "ok") error = result;
    }

    function use(font: GameFont) {
        const result = setAttribute(gameKey, "defaultfont", "font", font.stack);
        if (result !== "ok") error = result;
    }

    async function remove(font: GameFont) {
        const using = usedFor(font);
        const message = [
            t("fontsModal.removeConfirm", { font: font.family, file: font.file }),
            ...(using.length ? [t("fontsModal.removeInUse")] : []),
            ...(online ? [] : [t("fontsModal.removeOffline")]),
        ].join(" ");
        if (!(await confirmDialog(message, { confirmLabel: t("fontsModal.remove"), danger: true }))) return;
        busy = true;
        error = "";
        try {
            // A setting that named it keeps the fonts it fell back on
            for (const attribute of using) {
                const rest = (get(attribute) ?? "").split(",").slice(1).join(",").trim();
                const result = rest ? setAttribute(gameKey, attribute, "font", rest)
                    : attribute === "interfacefont" ? removeAttribute(gameKey, attribute)
                    : setAttribute(gameKey, attribute, "font", fallbackFor(font.category));
                if (result !== "ok") throw new Error(result);
            }
            await removeFont(gameKey, get("fontfiles"), font);
        } catch (err) {
            error = err instanceof Error ? err.message : String(err);
        } finally {
            busy = false;
        }
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key !== "Escape") return;
        if (chosen) chosen = null;
        else oncancel();
    }
</script>

<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    aria-labelledby="fonts-title"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={e => { if (e.target === e.currentTarget) oncancel(); }}
    onkeydown={handleKeydown}
    use:trapFocus
    data-fonts-modal
>
    <!-- A fixed height, so the dialog doesn't change size and jump about as a search changes the list -->
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-[36rem] h-[85dvh] p-6 flex flex-col gap-4">
        <div class="flex items-center justify-between">
            <h2 id="fonts-title" class="text-base font-semibold">{t("fontsModal.title")}</h2>
            <button class="btn btn-xs preset-tonal" onclick={oncancel}>{t("common.close")}</button>
        </div>

        <div class="flex-1 min-h-0 overflow-y-auto flex flex-col gap-4">
            <section class="flex flex-col gap-1" aria-labelledby="game-fonts-heading">
                <h3 id="game-fonts-heading" class="text-xs font-semibold uppercase tracking-wide text-surface-600-400">{t("fontsModal.inGame")}</h3>
                {#each $gameFonts as font (font.file)}
                    {@const using = usedFor(font)}
                    <div class="font-row" data-game-font-row={font.family}>
                        <span class="flex-1 min-w-0">
                            <span class="block text-lg truncate" style:font-family={font.stack}>{font.family}</span>
                            <span class="block text-xs text-surface-600-400 truncate">
                                {font.file}{#if using.includes("defaultfont")} · {t("fontsModal.usedForText")}{/if}{#if using.includes("interfacefont")} · {t("fontsModal.usedForInterface")}{/if}
                            </span>
                            {#if !font.listed}
                                <span class="block text-xs text-warning-700-300">{t("fontsModal.notLoaded")}</span>
                            {/if}
                        </span>
                        {#if !font.listed}
                            <button type="button" class="btn btn-xs preset-tonal text-xs px-2 py-0.5" disabled={busy} onclick={() => load(font)}>{t("fontsModal.load")}</button>
                        {:else if !using.includes("defaultfont")}
                            <button type="button" class="btn btn-xs preset-tonal text-xs px-2 py-0.5" disabled={busy} onclick={() => use(font)}>{t("fontsModal.useForText")}</button>
                        {/if}
                        <button type="button" class="btn btn-xs preset-outlined-error-500 text-xs px-2 py-0.5" disabled={busy} onclick={() => remove(font)}>{t("fontsModal.remove")}</button>
                    </div>
                {:else}
                    <p class="text-xs text-surface-600-400">{t("fontsModal.noFonts")}</p>
                {/each}
            </section>

            <section class="flex flex-col gap-2" aria-labelledby="add-font-heading">
                <h3 id="add-font-heading" class="text-xs font-semibold uppercase tracking-wide text-surface-600-400">{t("fontsModal.addHeading")}</h3>
                {#if !online}
                    <p class="text-sm">{t("fontsModal.offline")}</p>
                {:else if chosen}
                    {@const font = chosen}
                    <div class="flex flex-col gap-3">
                        <div class="flex items-baseline justify-between gap-2">
                            <span class="text-lg font-semibold">{font.family}</span>
                            <button type="button" class="btn btn-xs preset-tonal" onclick={() => { chosen = null; }} disabled={busy}>{t("fontsModal.back")}</button>
                        </div>
                        <p class="text-lg leading-relaxed rounded border border-surface-200-800 p-3" style:font-family={previewFamily(font.id, font.category)}>
                            {t("fontsModal.sample")} <b>{t("fontsModal.sampleBold")}</b>, <i>{t("fontsModal.sampleItalic")}</i>.
                        </p>
                        {#if font.subsets.length > 1}
                            <fieldset class="flex flex-col gap-1">
                                <legend class="text-xs text-surface-600-400 mb-1">{t("fontsModal.subsets")}</legend>
                                <div class="flex flex-wrap gap-x-4 gap-y-1">
                                    {#each font.subsets as subset (subset)}
                                        <label class="flex items-center gap-1.5 text-sm">
                                            <input type="checkbox" class="checkbox" checked={subsets.includes(subset)} disabled={busy} onchange={e => toggleSubset(subset, e.currentTarget.checked)} />
                                            {subsetLabel(subset)}
                                        </label>
                                    {/each}
                                </div>
                            </fieldset>
                        {/if}
                        <label class="flex items-center gap-1.5 text-sm">
                            <input type="checkbox" class="checkbox" bind:checked={useForText} disabled={busy} />
                            {t("fontsModal.useItForText")}
                        </label>
                        <div class="flex items-center gap-2">
                            <button type="button" class="btn btn-xs preset-filled-primary-500" onclick={add} disabled={busy || !subsets.length}>{t("fontsModal.addToGame")}</button>
                            {#if progress}<span class="text-xs text-surface-600-400" role="status">{progress}</span>{/if}
                        </div>
                    </div>
                {:else}
                    <input
                        class="input text-sm"
                        type="search"
                        placeholder={t("fontsModal.search")}
                        aria-label={t("fontsModal.search")}
                        bind:value={search}
                    />
                    {#if chosenError}<p class="text-xs text-error-500">{chosenError}</p>{/if}
                    {#if !search.trim()}
                        <p class="text-xs text-surface-600-400">{t("fontsModal.recommended")}</p>
                    {:else if catalogueError}
                        <p class="text-xs text-error-500">{t("fontsModal.catalogueError")}</p>
                    {:else if !catalogue}
                        <p class="text-xs text-surface-600-400">{t("fontsModal.gettingList")}</p>
                    {:else if !results.length}
                        <p class="text-xs text-surface-600-400">{t("fontsModal.noResults")}</p>
                    {/if}
                    <div class="flex flex-col">
                        {#each results as font (font.id)}
                            <button type="button" class="font-row text-left" data-font-result={font.family} onclick={() => choose(font.id)}>
                                <span class="flex-1 min-w-0 text-lg truncate" style:font-family={previewFamily(font.id, font.category)}>{font.family}</span>
                                <span class="text-xs text-surface-600-400 shrink-0">{font.category}</span>
                            </button>
                        {/each}
                    </div>
                {/if}
            </section>
        </div>

        {#if error}<p class="text-xs text-error-500" role="alert">{error}</p>{/if}
        <p class="text-xs text-surface-600-400">
            {t("fontsModal.about")}
            <a class="anchor" href={docsUrlForPath("/customise/look-and-feel/#text")} target="_blank" rel="noopener">{t("fontsModal.moreAboutFonts")}</a>
        </p>
    </div>
</div>

<style>
    .font-row {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.25rem 0.375rem;
        border-radius: 0.25rem;
    }
    .font-row:hover {
        background-color: color-mix(in srgb, var(--color-surface-500) 12%, transparent);
    }
</style>
