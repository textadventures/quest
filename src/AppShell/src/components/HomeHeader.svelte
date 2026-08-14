<script lang="ts">
    import { base } from "$app/paths";
    import { isElectron } from "$lib/runtime";
    import { settingsModalOpen } from "$lib/settings-store";
    import { t } from "$lib/i18n";
    import DiscordIcon from "$components/DiscordIcon.svelte";
    import GithubIcon from "$components/GithubIcon.svelte";
    import DownloadButton from "$components/DownloadButton.svelte";
    import SettingsIcon from "@lucide/svelte/icons/settings";

    let { forceDark = false }: { forceDark?: boolean } = $props();

    // Never shown inside Electron — that user already has the app.
    const isElectronApp = isElectron();
</script>

<header class="flex items-center gap-3 px-4 pt-4 pb-2 {forceDark ? "text-surface-100" : "text-surface-900-100"}">
    <img src="{base}/logo.svg" alt="" class="h-7" />
    <h1 class="text-xl font-semibold">Quest Viva</h1>
    <div class="ml-auto flex items-center gap-1">
        {#if !isElectronApp}
            <div class="hidden sm:block">
                <DownloadButton compact />
            </div>
        {/if}
        <a
            href="https://textadventures.co.uk/community/discord"
            target="_blank"
            rel="noopener noreferrer"
            class="home-header-link"
            title={t("homeHeader.joinDiscord")}
            aria-label={t("homeHeader.joinDiscord")}
        >
            <DiscordIcon size={18} />
        </a>
        <a
            href="https://github.com/textadventures/quest"
            target="_blank"
            rel="noopener noreferrer"
            class="home-header-link"
            title={t("homeHeader.viewGithub")}
            aria-label={t("homeHeader.viewGithub")}
        >
            <GithubIcon size={18} />
        </a>
        <button
            type="button"
            class="home-header-link"
            title={t("common.settings")}
            aria-label={t("common.settings")}
            onclick={() => settingsModalOpen.set(true)}
        >
            <SettingsIcon size={18} />
        </button>
    </div>
</header>

<style>
    .home-header-link {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        border-radius: var(--radius-container);
        opacity: 0.7;
    }
    .home-header-link:hover {
        opacity: 1;
        background-color: color-mix(in srgb, currentColor 12%, transparent);
    }
</style>
