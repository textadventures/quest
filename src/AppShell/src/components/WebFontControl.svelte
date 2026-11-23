<script lang="ts">
    // A game's web font (a "webfont" control, shown only when the game has one): a font from
    // Google Fonts, which the player downloads when it's online (defaultwebfont, from before the
    // Fonts dialog). Copy into game swaps it for the same font from Fontsource, copied into the
    // game (lib/fonts.ts), and puts it at the front of the game's font.
    import { onMount } from "svelte";
    import { setAttribute } from "#lib/editor-store.js";
    import { addFont, DEFAULT_SUBSETS, fetchFontDetails, fontsourceId, quoteFamily } from "#lib/fonts.js";
    import { t } from "#lib/i18n/index.js";

    let { value, gameKey, get }: {
        value: string;
        gameKey: string;
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
    } = $props();

    let busy = $state(false);
    let error = $state("");
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

    // Empty rather than removed, so it also clears one a theme gives (the retired Retro theme's)
    const clearWebFont = () => setAttribute(gameKey, "defaultwebfont", "textbox", "");

    async function copyIn() {
        busy = true;
        error = "";
        try {
            let font;
            try {
                font = await fetchFontDetails(fontsourceId(value));
            } catch {
                throw new Error(t("webFont.notFound", { font: value }));
            }
            const subsets = font.subsets.filter(s => DEFAULT_SUBSETS.includes(s));
            await addFont(gameKey, get("fontfiles"), font, subsets.length ? subsets : font.subsets.slice(0, 1));
            const base = (get("defaultfont") ?? "").trim() || "Georgia, serif";
            let result = setAttribute(gameKey, "defaultfont", "font", `${quoteFamily(font.family)}, ${base}`);
            if (result === "ok") result = clearWebFont();
            if (result !== "ok") throw new Error(result);
        } catch (err) {
            error = err instanceof Error ? err.message : String(err);
        } finally {
            busy = false;
        }
    }
</script>

<div class="flex flex-col gap-1" data-webfont-control>
    <div class="flex flex-wrap items-center gap-2">
        <span class="text-sm" style:font-family={`${quoteFamily(value)}, ${get("defaultfont") ?? "serif"}`}>{value}</span>
        <button
            type="button"
            class="btn btn-xs preset-filled-primary-500 text-xs px-2 py-0.5"
            disabled={busy || !online}
            title={online ? undefined : t("webFont.offline")}
            onclick={copyIn}
        >{busy ? t("webFont.copying") : t("webFont.copyIn")}</button>
        <button
            type="button"
            class="btn btn-xs preset-tonal text-xs px-2 py-0.5"
            disabled={busy}
            onclick={() => { const result = clearWebFont(); if (result !== "ok") error = result; }}
        >{t("webFont.remove")}</button>
    </div>
    <span class="text-xs text-surface-600-400 italic">{t("webFont.help")}</span>
    {#if error}<span class="text-xs text-error-500" role="alert">{error}</span>{/if}
</div>
