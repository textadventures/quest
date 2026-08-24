<script lang="ts">
    import { Switch } from "@skeletonlabs/skeleton-svelte";
    import { settingsModalOpen } from "$lib/settings-store";
    import { locale, setLocale, SUPPORTED_LOCALES, t } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";
    import { theme, setTheme, type ThemePreference } from "$lib/theme-store";
    import { defaultCodeView, setDefaultCodeView } from "$lib/code-view-store";

    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { if ($settingsModalOpen) dialogEl?.focus(); });

    function displayName(code: string): string {
        let name: string;
        try {
            name = new Intl.DisplayNames([code], { type: "language" }).of(code) ?? code;
        } catch {
            name = code;
        }
        // CLDR autonyms aren't capitalized for every language (e.g. "español"),
        // but this list is presented like a proper-noun menu ("English", "Deutsch"),
        // so normalize every entry to match.
        return name.charAt(0).toUpperCase() + name.slice(1);
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

    // Unlike language, a theme change applies live — the .dark class and
    // color-scheme toggle take effect immediately, no reload needed.
    function handleThemeChange(e: Event) {
        setTheme((e.target as HTMLSelectElement).value as ThemePreference);
    }
</script>

{#if $settingsModalOpen}
    <div
        bind:this={dialogEl}
        role="dialog"
        aria-modal="true"
        tabindex="-1"
        class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
        onclick={onBackdropClick}
        onkeydown={handleKeydown}
        use:trapFocus
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

            <div class="flex flex-col gap-1">
                <label for="settings-theme" class="text-xs text-surface-600-400">{t("settingsModal.theme")}</label>
                <select
                    id="settings-theme"
                    class="select bg-surface-50-950 px-2 py-1 text-sm"
                    value={$theme}
                    onchange={handleThemeChange}
                >
                    <option value="light">{t("settingsModal.themeLight")}</option>
                    <option value="dark">{t("settingsModal.themeDark")}</option>
                    <option value="system">{t("settingsModal.themeSystem")}</option>
                </select>
            </div>

            <div class="flex flex-col gap-1">
                <Switch
                    checked={$defaultCodeView}
                    onCheckedChange={(e) => setDefaultCodeView(e.checked)}
                >
                    <Switch.Control><Switch.Thumb /></Switch.Control>
                    <Switch.HiddenInput />
                    <Switch.Label class="text-xs text-surface-600-400">{t("settingsModal.defaultCodeView")}</Switch.Label>
                </Switch>
            </div>
        </div>
    </div>
{/if}
