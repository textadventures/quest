// Saved games, kept by the host rather than a player UI: the boot-time "Continue" prompt reads
// them before any UI is loaded, and every UI saves the same way. The UI only supplies the
// transcript HTML to save and its own "Game saved." feedback (see playerUi in wasm-player.js).
const GameSaver = (() => {
    // Electron: file storage under userData rather than IndexedDB, since
    // this window's origin (a local loopback port) isn't guaranteed to stay
    // the same across restarts — see save-store.ts. window.electronGameSaves
    // is bridged by player-preload.ts for every player window.
    function useElectronGameSaves() {
        return navigator.userAgent.includes('Electron/') && !!window.electronGameSaves;
    }

    function openDatabase() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('quest-viva-saves', 1);

            request.onupgradeneeded = () => {
                const db = request.result;
                if (!db.objectStoreNames.contains('saves')) {
                    db.createObjectStore('saves', {keyPath: ['gameId', 'slotIndex']});
                }
            };

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async function ensurePersistentStorage() {
        if (navigator.storage && navigator.storage.persist) {
            const isPersisted = await navigator.storage.persisted();
            if (isPersisted) {
                console.log('Storage is already persistent.');
                return true;
            }

            const granted = await navigator.storage.persist();
            if (granted) {
                console.log('Storage permission granted: persistent!');
            } else {
                console.warn('Persistent storage request was denied.');
            }
            return granted;
        } else {
            console.warn('StorageManager API not supported.');
            return false;
        }
    }

    // location: where the player was (the game's location bar text), shown beside the save so a
    // player can tell their saves apart without naming them.
    async function saveGame(gameId, slotIndex, dataUint8Array, name, location) {
        const chromeStrings = window.WebPlayer?.chromeStrings ?? null;
        if (useElectronGameSaves()) {
            await window.electronGameSaves.save(gameId, slotIndex, dataUint8Array, name || null, chromeStrings, location || null);
            return;
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readwrite');
        const store = tx.objectStore('saves');

        const entry = {
            gameId,
            slotIndex,
            data: dataUint8Array,
            name: name || null,
            location: location || null,
            timestamp: new Date(),
            // Snapshot of the game's own chrome-string translations at the moment
            // it was saved - lets the boot-time "Continue your game?" prompt show
            // the right language next session, before anything is parsed into a
            // WorldModel (see WebPlayer.setChromeStrings / Bridge.GetChromeStringsJson).
            chromeStrings
        };

        store.put(entry);

        return tx.complete || tx.done || new Promise((res, rej) => {
            tx.oncomplete = res;
            tx.onerror = rej;
        });
    }

    async function listSaves(gameId) {
        if (useElectronGameSaves()) {
            return await window.electronGameSaves.list(gameId);
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readonly');
        const store = tx.objectStore('saves');

        const request = store.getAll();

        return new Promise((resolve, reject) => {
            request.onsuccess = () => {
                const slots = request.result
                    .filter(entry => entry.gameId === gameId)
                    .map(entry => ({
                        slotIndex: entry.slotIndex,
                        name: entry.name || null,
                        timestamp: entry.timestamp || null,
                        location: entry.location || null,
                        chromeStrings: entry.chromeStrings || null,
                    }));

                resolve(slots);
            };
            request.onerror = () => reject(request.error);
        });
    }

    async function loadGame(gameId, slotIndex) {
        if (useElectronGameSaves()) {
            return await window.electronGameSaves.load(gameId, slotIndex);
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readonly');
        const store = tx.objectStore('saves');
        const request = store.get([gameId, slotIndex]);

        return new Promise((resolve, reject) => {
            request.onsuccess = () => resolve(request.result?.data || null);
            request.onerror = () => reject(request.error);
        });
    }

    async function renameSaveSlot(gameId, slotIndex, name) {
        if (useElectronGameSaves()) {
            await window.electronGameSaves.rename(gameId, slotIndex, name || null);
            return;
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readwrite');
        const store = tx.objectStore('saves');
        const request = store.get([gameId, slotIndex]);
        request.onsuccess = () => {
            if (request.result) store.put({...request.result, name: name || null});
        };

        return new Promise((res, rej) => {
            tx.oncomplete = res;
            tx.onerror = rej;
        });
    }

    async function deleteSaveSlot(gameId, slotIndex) {
        if (useElectronGameSaves()) {
            await window.electronGameSaves.delete(gameId, slotIndex);
            return;
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readwrite');
        const store = tx.objectStore('saves');
        store.delete([gameId, slotIndex]);

        return tx.complete || tx.done || new Promise((res, rej) => {
            tx.oncomplete = res;
            tx.onerror = rej;
        });
    }

    async function nextSlotIndex(gameId) {
        const existing = await listSaves(gameId);
        return existing.length ? Math.max(...existing.map(s => s.slotIndex)) + 1 : 0;
    }

    let persistenceRequested = false;

    // Saves the game as it is now into slotIndex, or a new slot. A save with no name is shown by
    // where the player was and when (see renderSavesList in wasm-player.js), so none is made up.
    async function saveCurrentGame(slotIndex, name) {
        const gameId = WebPlayer.gameId;
        const saveData = playerUi.getOutputHtml();
        const result = await WebPlayer.uiSaveGame(saveData);
        slotIndex ??= await nextSlotIndex(gameId);
        await saveGame(gameId, slotIndex, result, name, playerUi.getLocation?.() || null);
        playerUi.gameSaved();
        WebPlayer.onGameSaved?.();
        if (!persistenceRequested) {
            await ensurePersistentStorage();
            persistenceRequested = true;
        }
        return {slotIndex, name: name || null};
    }

    function timeOf(save) {
        return save.timestamp ? new Date(save.timestamp).getTime() : 0;
    }

    return {
        save: async (name) => saveCurrentGame(null, name),
        // Replaces a save with the game as it is now, keeping its name.
        overwrite: async (slotIndex) => {
            const existing = (await listSaves(WebPlayer.gameId)).find(s => s.slotIndex === slotIndex);
            return saveCurrentGame(slotIndex, existing?.name);
        },
        rename: async (slotIndex, name, gameId = WebPlayer.gameId) => {
            return await renameSaveSlot(gameId, slotIndex, name);
        },
        // Newest first.
        listSaves: async (gameId = WebPlayer.gameId) => {
            return (await listSaves(gameId)).sort((a, b) => timeOf(b) - timeOf(a) || b.slotIndex - a.slotIndex);
        },
        load: async (slotIndex, gameId = WebPlayer.gameId) => {
            return await loadGame(gameId, slotIndex);
        },
        deleteSlot: async (slotIndex, gameId = WebPlayer.gameId) => {
            return await deleteSaveSlot(gameId, slotIndex);
        }
    }
})();