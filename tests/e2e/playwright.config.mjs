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
    // Specs were written to run one at a time (Electron ones share
    // ~/Documents/Quest Games, and the dev servers aren't sized for parallel load).
    workers: 1,
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
            name: 'electron',
            testDir: './electron',
        },
    ],
});
