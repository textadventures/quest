// Shared editor-preview handoff for the WasmPlayer e2e scripts: plays the part
// AppShell's previewInWasmPlayer() plays for a real ?source=editor window,
// handing a fixture game to the player over the 'quest-preview'
// BroadcastChannel.
//
// The player posts 'ready' exactly once, as soon as its boot script runs, and a
// BroadcastChannel drops any message sent while nobody is listening rather than
// queueing it. AppShell is safe because it opens the window and creates its
// channel in the same task. The scripts used to start the sender's listener
// with an un-awaited page.evaluate() and navigate the preview page straight
// after, so nothing guaranteed the listener was live before the player booted:
// if the evaluate lost that race, 'ready' was missed and the script waited on
// it forever. That's how nightly run 41435260262 sat silent in
// wasmplayer/autoscroll.spec.mjs for six hours until GitHub cancelled it.
//
// So this awaits the listener's setup before navigating, and puts a time limit
// on the wait for the handoff itself.

/**
 * Hand the game at `fixtureUrl` to the player loaded at `previewUrl`.
 * `senderPage` must already be on a same-origin page (any page of the dev server).
 */
export async function handOffPreviewGame(senderPage, previewPage, { previewUrl, fixtureUrl, filename, timeout = 30000 }) {
    await senderPage.evaluate(({ url, filename }) => {
        const bc = new BroadcastChannel('quest-preview');
        const ready = new Promise((resolve) => {
            bc.onmessage = ({ data }) => {
                if (data.type === 'ready') resolve();
            };
        });
        window.__questPreviewHandoff = (async () => {
            const bytes = new Uint8Array(await (await fetch(url)).arrayBuffer());
            await ready;
            bc.postMessage({ type: 'game', bytes, filename });
        })();
    }, { url: fixtureUrl, filename });

    await previewPage.goto(previewUrl);

    await senderPage.evaluate((timeout) => Promise.race([
        window.__questPreviewHandoff,
        new Promise((_, reject) => setTimeout(
            () => reject(new Error(`editor-preview handoff timed out after ${timeout}ms (no 'ready' from the player)`)),
            timeout)),
    ]), timeout);
}
