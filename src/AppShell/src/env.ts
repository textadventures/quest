import { defineEnvVars } from "@sveltejs/kit/env";

// Build-time settings, set by the deploy workflows and dev.sh. They're optional:
// an unset variable reads as "", as it did under SvelteKit 2's $env/static/public.
const optional = (value: string | undefined) => value ?? "";

export const variables = defineEnvVars({
    PUBLIC_APPSHELL_VERSION: { public: true, static: true, schema: optional },
    PUBLIC_BETA_SITE: { public: true, static: true, schema: optional },
    PUBLIC_SHOW_HOME: { public: true, static: true, schema: optional },
    PUBLIC_HAS_SERVER: { public: true, static: true, schema: optional },
    PUBLIC_WASM_PLAYER_URL: { public: true, static: true, schema: optional },
});
