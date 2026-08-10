<script lang="ts">
    import { settingsModalOpen } from "$lib/settings-store";
    import { locale, setLocale, SUPPORTED_LOCALES, t } from "$lib/i18n";

    function displayName(code: string): string {
        try {
            return new Intl.DisplayNames([code], { type: "language" }).of(code) ?? code;
        } catch {
            return code;
        }
    }

    function close() {
        settingsModalOpen.set(false);
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) close();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") close();
    }

    // A language change affects ~200 t()-driven strings across the whole app;
    // reloading is simpler and cheaper than threading live reactivity through
    // every call site for a rarely-touched setting (see i18n/index.ts).
    async function handleLanguageChange(e: Event) {
        const code = (e.target as HTMLSelectElement).value;
        await setLocale(code);
        window.location.reload();
    }
</script>

{#if $settingsModalOpen}
    <div
        role="dialog"
        aria-modal="true"
        tabindex="-1"
        class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
        onclick={onBackdropClick}
        onkeydown={handleKeydown}
    >
        <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-80 p-6 flex flex-col gap-4">
            <div class="flex items-center justify-between">
                <h2 class="text-base font-semibold">{t("settingsModal.title")}</h2>
                <button class="btn btn-sm preset-tonal" onclick={close}>{t("common.close")}</button>
            </div>

            <div class="flex flex-col gap-1">
                <label for="settings-language" class="text-xs text-surface-600-400">{t("settingsModal.language")}</label>
                <select
                    id="settings-language"
                    class="select bg-surface-50-950 px-2 py-1 text-sm"
                    value={$locale}
                    onchange={handleLanguageChange}
                >
                    {#each SUPPORTED_LOCALES as code (code)}
                        <option value={code}>{displayName(code)}</option>
                    {/each}
                </select>
            </div>
        </div>
    </div>
{/if}
