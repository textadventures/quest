// Playwright 1.65's Firefox (#2804) fires a "cancel" event on a file input when a
// script answers its file chooser with setFiles(), usually just before the "change"
// that carries the chosen file. A real user's pick never does that: Firefox fires
// "cancel" only when the picker is dismissed, and then no "change" follows.
//
// AppShell's pickFile() (src/lib/filesystem/file-picker.ts) resolves to null on
// "cancel", so the stray event makes Import game file do nothing at all: no draft,
// no error. Nightly appshell_opfs_firefox failed on it every night from the bump
// on, in appshell-firefox/drafts-rekey-backup.spec.mjs; appshell-firefox/multi-aslx-zip.spec.mjs
// and appshell-firefox/local-drafts.spec.mjs import the same way and only got lucky with
// the event order.
//
// Call this before the page loads (it's an init script) in any script that answers
// a file chooser in Firefox. It keeps pages from registering "cancel" listeners on
// file inputs, which only this test-only event would ever trigger.
export async function ignoreFileChooserCancel(page) {
    await page.addInitScript(() => {
        const addEventListener = EventTarget.prototype.addEventListener;
        EventTarget.prototype.addEventListener = function (type, listener, options) {
            if (type === 'cancel' && this instanceof HTMLInputElement && this.type === 'file') return;
            return addEventListener.call(this, type, listener, options);
        };
    });
}
