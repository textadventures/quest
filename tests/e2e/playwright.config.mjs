// Playwright Test config for the e2e suite. Each project is one directory of
// `*.spec.mjs` files that need the same kind of server (or none), and each CI job
// in .github/workflows/e2e.yml runs one project: `npx playwright test --project=site`.
// A new spec is picked up just by being in the right directory.
//
// The servers themselves are started outside Playwright (by the CI job, or by
// ./dev.sh locally), because each project needs a different one. Point a project
// at a different server with its env var, e.g. SITE_URL=http://localhost:4322.
import { defineConfig } from '@playwright/test';

const ci = !!process.env.CI;

export default defineConfig({
    // Most specs drive a whole session end to end in one test, and several
    // (Electron especially) take minutes.
    timeout: 5 * 60_000,
    // Spec files run in parallel, four at a time (GitHub's Linux runners have 4
    // vCPUs); tests inside one file still run in order. Each spec launches its own
    // browser, so IndexedDB/OPFS never collide, and anything a spec writes to disk
    // goes to a mkdtemp folder or a /tmp path named after that spec. On an 8-core
    // Mac, wasmplayer went from 14.9 to 4.4 minutes and appshell from 12.7 to 4.1.
    // A busy machine makes everything slower, so a spec that sleeps a fixed time for
    // the page to settle, or checks before it has, can start failing; wait for the
    // state itself instead. Running a spec with --repeat-each=32 --workers=8 shows up
    // most of these. Electron stays at one: its specs share ~/Documents/Quest Games
    // and the app's single-instance lock.
    workers: 4,
    fullyParallel: false,
    forbidOnly: ci,
    retries: 0,
    reporter: ci
        ? [['list'], ['github'], ['html', { open: 'never' }]]
        : [['list']],
    projects: [
        {
            name: 'site',
            testDir: './site',
            use: { baseURL: process.env.SITE_URL ?? 'http://localhost:4321' },
        },
        {
            name: 'wasmplayer',
            testDir: './wasmplayer',
            use: { baseURL: process.env.WASMPLAYER_URL ?? 'http://localhost:5175' },
        },
        {
            name: 'appshell',
            testDir: './appshell',
            use: { baseURL: process.env.APPSHELL_URL ?? 'http://localhost:5174' },
        },
        {
            // OPFS local drafts in a browser without the File System Access API, against
            // AppShell built with a Release WasmEditor (the appshell_opfs_firefox job).
            name: 'appshell-firefox',
            testDir: './appshell-firefox',
            use: { baseURL: process.env.APPSHELL_URL ?? 'http://localhost:5174' },
        },
        {
            name: 'electron',
            testDir: './electron',
            workers: 1,
        },
    ],
});
