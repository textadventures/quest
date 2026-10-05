// The path AppShell is served under ("" at the root, "/editor" on play.questviva.com, ...),
// from the same BASE_PATH that vite.config.ts passes to SvelteKit. SvelteKit 3 removed
// `base` from $app/paths in favour of resolve() and asset(), but those only accept this
// app's own routes and static files, and some links here point at the WasmPlayer that's
// deployed alongside it (`${base}/player/...`).
declare const __APP_BASE_PATH__: string;

export const base: string = __APP_BASE_PATH__;
