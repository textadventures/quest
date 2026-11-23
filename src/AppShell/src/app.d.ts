// See https://svelte.dev/docs/kit/types#app.d.ts
declare global {
    namespace App {
        interface Error {
            // The stack of an exception the app didn't catch (hooks.client.ts)
            detail?: string;
        }
    }
}

export {};
