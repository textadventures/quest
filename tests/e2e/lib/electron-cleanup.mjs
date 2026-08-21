// Shared teardown helpers for the Electron e2e scripts (tests/e2e/verify-electron-*.mjs
// and friends). Every one of them launches a real Electron/Chromium process against a
// throwaway --user-data-dir and has to tear both down afterwards. Chromium's disk-cache
// writeback (and, on some runners, a lingering Crashpad handler) can still be touching
// that directory for a moment after the app process exits, so an immediate rmdir can hit
// ENOTEMPTY. That race was independently rediscovered and independently "fixed" in at
// least three prior PRs (#1978, #2059, #2083), each time by copy-pasting a slightly
// different retry/try-catch snippet into one script — which is exactly how the same crash
// (see PR #2083 for verify-electron-unsaved-close-dialog.mjs) still turned up months later
// in a sibling script that never got the fix. Centralizing it here means a runner-flake fix
// only has to happen once.
import { rmSync } from 'node:fs';

/**
 * SIGKILL the Electron app's underlying process and wait for it to actually exit, so a
 * subsequent removeTempDirs() isn't racing a still-running process. kill() only sends the
 * signal — it doesn't wait for termination.
 */
export async function killElectronApp(app) {
    const proc = app?.process();
    if (proc && proc.exitCode === null) {
        const exited = new Promise((resolve) => proc.once('exit', resolve));
        proc.kill('SIGKILL');
        await exited;
    }
}

/**
 * Best-effort recursive removal of one or more temp directories (userData dirs, scratch
 * game folders, etc). Retries to ride out the post-exit writeback race, but — unlike a bare
 * rmSync — never throws: this runs in test teardown, on an OS temp dir that the runner
 * discards anyway, so leftover junk here must never fail an otherwise-passing test.
 */
export function removeTempDirs(...dirs) {
    for (const dir of dirs) {
        if (!dir) continue;
        try {
            rmSync(dir, { recursive: true, force: true, maxRetries: 10, retryDelay: 300 });
        } catch (err) {
            console.warn(`WARN: could not remove temp dir ${dir}: ${err.message}`);
        }
    }
}
