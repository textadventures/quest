// localStorage, or null where there isn't one to use. `typeof localStorage` alone isn't a safe
// check: when the browser blocks storage (Firefox set to block all cookies, some privacy add-ons),
// even reading window.localStorage throws a SecurityError. Unguarded, that took the whole app down
// at startup, since the locale and theme stores read their saved preference before first render.
export function localStorageOrNull(): Storage | null {
    try {
        return typeof localStorage === "undefined" ? null : localStorage;
    } catch {
        return null;
    }
}
