<script lang="ts">
    import { onMount } from "svelte";
    import type { UpdateInfo } from "$lib/home-catalog";
    import { t } from "$lib/i18n";
    import { isElectron } from "$lib/runtime";

    let { update }: { update: UpdateInfo } = $props();

    const dismissedKey = "questviva-update-dismissed-version";
    const isElectronApp = isElectron();

    let manuallyDismissed = $state(false);
    // Electron: the dismissed version lives in a userData file, read over IPC
    // (see update-dismiss-store.ts — static-server.ts's origin isn't
    // guaranteed to stay the same across restarts, so localStorage can't be
    // used there). Starts true so the banner never flashes on and back off
    // while that read is in flight; onMount below corrects it once resolved.
    let electronDismissed = $state(isElectronApp);

    onMount(() => {
        if (!isElectronApp) return;
        void (async () => {
            try {
                electronDismissed = (await window.electronApp!.updateDismiss.get()) === update.latestVersion;
            } catch {
                electronDismissed = false;
            }
        })();
    });

    // Dismissing only suppresses this specific version's banner — a later
    // release with a newer latestVersion re-triggers it.
    const dismissed = $derived(
        manuallyDismissed ||
        (isElectronApp
            ? electronDismissed
            : typeof localStorage !== "undefined" && localStorage.getItem(dismissedKey) === update.latestVersion)
    );

    function handleDismiss() {
        if (isElectronApp) {
            void window.electronApp!.updateDismiss.set(update.latestVersion);
        } else {
            localStorage.setItem(dismissedKey, update.latestVersion);
        }
        manuallyDismissed = true;
    }
</script>

{#if !dismissed}
    <!-- Fixed shades throughout (bg-primary-900, preset-outlined-surface-500),
         not Skeleton's paired light/dark-auto-switching tokens
         (bg-primary-100-900, preset-tonal, etc — see presets.css's use of
         light-dark()): the Play tab is force-dark regardless of system theme
         (unlike the editor, where BackupBanner's use of those pairs is fine),
         so an auto-switching background/text pair here would go low-contrast
         under a light-mode system. -->
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-900 border-b border-primary-700 text-sm">
        <span class="flex-1">{t("updateBanner.availableMessage", { version: update.latestVersion })}</span>
        <a href={update.url} target="_blank" rel="noopener" class="btn btn-sm preset-filled-primary-500">{t("updateBanner.downloadButton")}</a>
        <button type="button" class="btn btn-sm preset-outlined-surface-500" onclick={handleDismiss}>{t("updateBanner.later")}</button>
    </div>
{/if}
