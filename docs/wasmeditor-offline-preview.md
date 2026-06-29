# WasmEditor: Offline Preview via WasmPlayer

## Motivation

When WasmEditor is used outside of textadventures.co.uk (local files, CDN deployment), the preview button currently shows an informational message because preview previously required a server-side player (WebPlayer). Now that WasmPlayer runs entirely in the browser, it is possible to preview a game without any server — the editor can open WasmPlayer in a new tab and communicate with it directly via the browser's `BroadcastChannel` API.

---

## Architecture

The editor opens WasmPlayer in a new tab and they communicate over a named `BroadcastChannel`. The editor is the authority on game data; the player requests what it needs.

```
WasmEditor tab                          WasmPlayer tab
──────────────                          ──────────────
User clicks "Preview"
  window.open(playerUrl + '?source=editor')
  BroadcastChannel('quest-preview')
                                          registers BroadcastChannel
                          ←── {type:'ready'} ───
  sends {type:'game', bytes, filename}
  ────────────────────────────────────→
                                          Bridge.Initialise(bytes, filename)
                                          game loads, game starts

  [during gameplay, as resources are needed]

                          ←── {type:'resource-request', name, id} ───
  reads file from FSA/OPFS handle
  sends {type:'resource-response', id, dataUrl}
  ────────────────────────────────────→
                                          resolves pending promise → resource displayed
```

Resource requests are on-demand (lazy): WasmPlayer only asks for a file when the game engine actually needs it, and caches the result so each file is only transferred once per session.

---

## Why on-demand and not batch upfront

Pre-fetching all resources at game start would transfer files the player may never need, and converting every image/sound to a data URL before the game begins would add noticeable startup latency for resource-heavy games. The on-demand proxy sends each resource exactly once, when it is first used.

This requires `GetURL` (the engine's resource URL resolver) to be async. See Phase 1.

---

## Phases

### Phase 1 — Make `GetURL` async

**Goal:** Pure refactor, no behaviour change. Propagate `async`/`Task<string>` through the resource URL resolution stack so that Phase 3's on-demand proxy can await a BroadcastChannel response from inside `GetURL`.

**Files to change:**

| File | Change |
|------|--------|
| `src/Common/IGame.cs` | `string GetURL(string)` → `Task<string> GetURL(string)` |
| `src/Engine/WorldModel.cs` | `GetExternalUrl` → `async Task<string>` |
| `src/Engine/Functions/ExpressionOwner.cs` | `GetFileURL` → `async Task<string>` |
| `src/Engine/Scripts/PictureScript.cs` | `await` the now-async `GetFileURL` call (already in `ExecuteAsync`) |
| `src/PlayerCore/Player.cs` | `GetURL`, `PlaySound`, `ShowPicture` → async |
| `src/PlayerCore/GameQuery.cs` | `GetURL` → async |
| `src/WasmPlayer/WasmPlayerBridge.cs` | `WasmPlayerUi.GetURL` → `async Task<string>` |
| `src/Legacy/V4Game.Part2.cs` | One `GetURL` call site — check whether it is already in an async context |

The `IPlayer.PlaySound` and `IPlayer.ShowPicture` interface methods call `GetURL` in their implementations. These become `Task`-returning; their callers are all in `ExecuteAsync` script methods so no sync-context issues are expected.

**Done when:** All tests pass with no behaviour change.

---

### Phase 2 — WasmPlayer: receive game from BroadcastChannel

**Goal:** WasmPlayer can be opened by the editor and receive the game bytes over the channel, as an alternative to the `?game=url` path.

**`src/WasmPlayer/wasm-player.js`** — add alongside the existing URL-loading path:

```js
if (params.get('source') === 'editor') {
    const bc = new BroadcastChannel('quest-preview');
    bc.postMessage({ type: 'ready' });
    bc.onmessage = async ({ data }) => {
        if (data.type === 'game') {
            bc.onmessage = null;   // hand off to resource handler (Phase 3)
            await initWasmPlayer(data.bytes, data.filename, bc);
        }
    };
    return;
}
```

`initWasmPlayer` gains an optional `bc` parameter used in Phase 3; when null (normal URL mode) behaviour is unchanged.

**Done when:** Opening `player/?source=editor` and manually dispatching a `BroadcastChannel` message with game bytes starts the game correctly.

---

### Phase 3 — WasmPlayer: on-demand resource proxy

**Goal:** When the game engine requests a resource and no local stream is available, WasmPlayer asks the editor for it over the channel and awaits the response.

**`src/WasmPlayer/wasm-player.js`** — expose `getResourceUrl` to C#:

```js
const pendingResources = new Map();   // requestId → resolve

export function getResourceUrl(name) {
    return new Promise(resolve => {
        const id = crypto.randomUUID();
        pendingResources.set(id, resolve);
        editorChannel.postMessage({ type: 'resource-request', name, id });
    });
}

// In the BroadcastChannel message handler:
if (data.type === 'resource-response') {
    pendingResources.get(data.id)?.(data.dataUrl);
    pendingResources.delete(data.id);
}
```

**`src/WasmPlayer/WasmPlayerBridge.cs`** — add JSImport and use it in `GetURL`:

```csharp
[JSImport("getResourceUrl", "wasm-player")]
private static partial Task<string> JsGetResourceUrl(string filename);

public async Task<string> GetURL(string filename)
{
    if (_resourceUrls.TryGetValue(filename, out var cached))
        return cached;

    var stream = _game.GetResourceStream(filename);
    if (stream != null)
    {
        // existing data-URL conversion for embedded resources
        ...
        _resourceUrls[filename] = dataUrl;
        return dataUrl;
    }

    // Fall back to editor proxy (no-op returns filename as-is when not in editor mode)
    var url = await JsGetResourceUrl(filename);
    _resourceUrls[filename] = url;
    return url;
}
```

When WasmPlayer is not in editor mode, `getResourceUrl` returns the filename unchanged (current behaviour — the browser attempts to fetch it as a relative URL, which works for URL-loaded games served from a real HTTP server).

**Done when:** A game with local image/sound resources previewed from the editor displays those resources correctly.

---

### Phase 4 — WasmEditor: send game and serve resource requests

**Goal:** The editor's Preview button opens WasmPlayer, sends the current game state, and handles incoming resource requests.

**`src/WebEditor/src/components/Toolbar.svelte`** — replace the info banner with:

```ts
async function previewInWasmPlayer() {
    const xml = WasmEditorBridge.Save();           // current game XML from C# bridge
    const bytes = new TextEncoder().encode(xml);

    const playerTab = window.open(wasmPlayerUrl + '?source=editor', '_blank');
    const bc = new BroadcastChannel('quest-preview');

    bc.onmessage = async ({ data }) => {
        if (data.type === 'ready') {
            bc.postMessage({ type: 'game', bytes, filename: currentFilename });
        }
        else if (data.type === 'resource-request') {
            const fileBytes = await adapter.readFile(data.name);  // reads from FSA/OPFS handle
            const mimeType = getMimeType(data.name);
            const dataUrl = await toDataUrl(fileBytes, mimeType);
            bc.postMessage({ type: 'resource-response', id: data.id, dataUrl });
        }
    };
}
```

`adapter.readFile(name)` reads from whichever `FileAdapter` is active (FSA directory handle or OPFS). The `BroadcastChannel` stays open for the lifetime of the preview session so late resource requests are served.

**`wasmPlayerUrl`** — needs to resolve to the correct WasmPlayer deployment. Options:
- Config value set at build time (CDN deployment)
- Relative path if editor and player are co-deployed (e.g. `/player/`)
- `window.location.origin + '/player/'` as a convention

**Done when:** Clicking Preview from the editor opens the game in WasmPlayer with images and sounds working, entirely without a server.

---

## Open questions

- **Multiple preview tabs:** If the user clicks Preview again, does it reuse the existing tab or open a new one? `BroadcastChannel` is one-to-many, so multiple player tabs would all receive the game bytes — probably fine for now.
- **Resource request cancellation / timeout:** If the editor tab is closed while the player is still running, pending resource requests will never resolve. A timeout or an `unload` event on the editor side could post a `{type:'abort'}` message.
- **Save-before-preview:** Should Preview auto-save, or send the in-memory (possibly unsaved) game state? Sending unsaved state is more useful for a fast edit-preview loop.
- **`wasmPlayerUrl` for offline/Electron use:** When running offline (PWA or Electron), the player URL needs to be discoverable without a network round-trip. Worth aligning with the Electron architecture doc.
