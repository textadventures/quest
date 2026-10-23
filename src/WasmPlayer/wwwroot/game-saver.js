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

    async function saveGame(gameId, slotIndex, dataUint8Array, name) {
        const chromeStrings = window.WebPlayer?.chromeStrings ?? null;
        if (useElectronGameSaves()) {
            await window.electronGameSaves.save(gameId, slotIndex, dataUint8Array, name || null, chromeStrings);
            return;
        }
        const db = await openDatabase();
        const tx = db.transaction('saves', 'readwrite');
        const store = tx.objectStore('saves');

        const entry = {
            gameId,
            slotIndex,
            data: dataUint8Array,
            name,
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

    return {
        save: async (name) => {
            const gameId = WebPlayer.gameId;
            const saveData = playerUi.getOutputHtml();
            const result = await WebPlayer.uiSaveGame(saveData);
            const slotIndex = await nextSlotIndex(gameId);
            // Locale-formatted (not ISO) so it reads naturally wherever it's shown —
            // WebPlayer's Slots list shows only the name, with no separate timestamp.
            const labelTemplate = window.WebPlayer?.chromeStrings?.SavedGameDefaultLabel || "Saved game — [timestamp]";
            const label = name || labelTemplate.replace('[timestamp]', new Date().toLocaleString());
            await saveGame(gameId, slotIndex, result, label);
            playerUi.gameSaved();
            if (!persistenceRequested) {
                await ensurePersistentStorage();
                persistenceRequested = true;
            }
            return {slotIndex, name: label};
        },
        listSaves: async (gameId = WebPlayer.gameId) => {
            return await listSaves(gameId);
        },
        load: async (slotIndex, gameId = WebPlayer.gameId) => {
            return await loadGame(gameId, slotIndex);
        },
        deleteSlot: async (slotIndex, gameId = WebPlayer.gameId) => {
            return await deleteSaveSlot(gameId, slotIndex);
        }
    }
})();