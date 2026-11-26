<script lang="ts">
    import { t } from "#lib/i18n/index.js";
    import { stableVersion } from "#lib/channel.js";
</script>

<!-- Only built in for play-beta.questviva.com (PUBLIC_BETA_SITE, set by
     deploy-play.yml) — see docs/release-channels.md. Between a stable release
     and the next version's first beta, deploy-play.yml puts that stable
     release here too, and the banner then says there's no beta just now.
     Fixed shades rather than
     Skeleton's paired light/dark tokens, for the same reason as UpdateBanner:
     it sits above both the always-dark Play tab and the theme-following
     Create tab, so it has to read the same on either. -->
<div class="flex flex-wrap items-center gap-x-3 gap-y-1 px-4 py-2 bg-warning-900 border-b border-warning-700 text-warning-50 text-sm">
    <span class="flex-1 basis-80">{stableVersion ? t("betaBanner.noBetaMessage", { version: stableVersion }) : t("betaBanner.message")}</span>
    <a href="https://play.questviva.com/" class="underline font-semibold">{t("betaBanner.stableLink")}</a>
</div>
