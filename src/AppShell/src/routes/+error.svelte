<script lang="ts">
    // Shown in place of a page when the app hits an exception it didn't catch (with the real
    // message, from hooks.client.ts), or for an address that isn't one of the app's pages.
    // The open game lives in the WASM bridge and outlives the page that crashed, so its unsaved
    // changes are saved straight away (saveAfterCrash).
    import { onMount } from "svelte";
    import { page } from "$app/state";
    import { PUBLIC_APPSHELL_VERSION } from "$app/env/public";
    import { base } from "#lib/base-path.js";
    import { saveAfterCrash } from "#lib/editor-store.js";
    import { isElectron } from "#lib/runtime.js";
    import { t } from "#lib/i18n/index.js";
    import TriangleAlert from "@lucide/svelte/icons/triangle-alert";

    const ISSUES_URL = "https://github.com/textadventures/quest/issues/new";
    const version = PUBLIC_APPSHELL_VERSION || "dev";

    const notFound = $derived(page.status === 404);
    const message = $derived(page.error?.message ?? "Unknown error");
    const detail = $derived(page.error?.detail);

    // "none": no game open. null: saved. Otherwise why it couldn't be saved.
    let saved = $state<"saving" | "none" | string | null>("saving");
    let copied = $state(false);

    onMount(() => {
        if (notFound) {
            saved = "none";
            return;
        }
        saveAfterCrash().then(
            result => saved = result,
            (err: unknown) => saved = err instanceof Error ? err.message : String(err),
        );
    });

    async function copyDetails() {
        const text = [
            `Quest Viva ${version}${isElectron() ? " (desktop)" : ""}`,
            navigator.userAgent,
            page.url.pathname,
            "",
            detail ?? message,
        ].join("\n");
        try {
            await navigator.clipboard.writeText(text);
            copied = true;
            setTimeout(() => copied = false, 2000);
        } catch {
            // The details are on the page to select by hand
        }
    }
</script>

<main class="max-w-2xl mx-auto px-4 py-10 flex flex-col gap-5" data-testid="error-page">
    {#if notFound}
        <h1 class="text-xl font-semibold">{t("errorPage.notFoundTitle")}</h1>
        <p class="text-sm">{t("errorPage.notFoundMessage")}</p>
        <div>
            <a class="btn btn-sm preset-filled-primary-500" href="{base}/">{t("errorPage.home")}</a>
        </div>
    {:else}
        <div class="flex items-center gap-3">
            <TriangleAlert size={26} class="shrink-0 text-error-500" aria-hidden="true" />
            <h1 class="text-xl font-semibold">{t("errorPage.title")}</h1>
        </div>

        {#if saved === "saving"}
            <p class="text-sm">{t("errorPage.saving")}</p>
        {:else if saved === null}
            <p class="text-sm" data-testid="error-page-saved">{t("errorPage.saved")}</p>
        {:else if saved !== "none"}
            <p role="alert" class="text-sm text-error-600-400" data-testid="error-page-save-failed">{t("errorPage.saveFailed", { error: saved })}</p>
        {/if}

        <div class="flex flex-wrap gap-2">
            <button type="button" class="btn btn-sm preset-filled-primary-500" disabled={saved === "saving"} onclick={() => location.reload()}>{t("errorPage.reload")}</button>
            <a class="btn btn-sm preset-tonal" href="{base}/" data-sveltekit-reload>{t("errorPage.home")}</a>
        </div>

        <section aria-labelledby="error-details-heading" class="card preset-outlined-error-500 p-4 flex flex-col gap-3">
            <h2 id="error-details-heading" class="font-semibold text-sm break-words">{message}</h2>
            {#if detail}
                <pre class="text-xs overflow-x-auto whitespace-pre max-h-72">{detail}</pre>
            {/if}
            <p class="text-xs text-surface-600-400">
                {t("errorPage.bug", { version })}
                <a class="anchor" href={ISSUES_URL} target="_blank" rel="noopener noreferrer">{t("errorPage.report")}</a>
            </p>
            <div>
                <button type="button" class="btn btn-xs preset-tonal" onclick={copyDetails}>{copied ? t("errorPage.copied") : t("errorPage.copyDetails")}</button>
            </div>
        </section>
    {/if}
</main>
