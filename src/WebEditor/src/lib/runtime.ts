// Detects the Electron desktop shell. Checked ahead of hasFSA() in the open
// flow — Electron's Chromium does support showDirectoryPicker, but
// docs/electron-desktop-app.md flags known FSA parity bugs there (missing
// persistent permissions, broken directory iteration), so Electron always
// uses window.electronApp's Node fs bridge instead, never FSA.
export function isElectron(): boolean {
    return typeof window !== "undefined" && !!window.electronApp;
}
