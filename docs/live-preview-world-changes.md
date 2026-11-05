# Live world changes in the Preview

Status: proposal. Nothing built yet.

The editor's Preview already picks up Display and Interface tab changes without restarting: `editor-store.ts`'s `syncPreviewInterface` sends the changed attributes over the `quest-preview` BroadcastChannel, and `wasm-player.js`'s `applyInterfaceChanges` copies them onto the running game (`WasmPlayerBridge.SetGameAttributes`) and rebuilds the UI around the output so far. This plan extends that to the game world: a new room, exit or object, a changed description or script, shows up in a Preview that's already running, where the author is.

The hard part is that the running game has its own state. The player has moved, taken things and set flags, so the editor's version of an object and the running game's can differ for good reasons. This plan applies a change only where the running game hasn't moved away from what the editor had, says so plainly when it can't, and offers a replay from the start as the fallback that's always correct.

## What authors get

- Add an exit, a room or an object, and it's there in the running game: in the exits and places lists, and to commands, on the next turn.
- Change a description or a script, and the next `look` or the next use of that verb shows the new one.
- When a change touches the room the player is in, a marker in the output says so, rather than reprinting the room on its own.
- A list in the Preview of the changes that arrived, each marked applied, or not applied with the reason ("changed during play", "only takes effect when the game starts").
- When anything wasn't applied, a **Replay to here** button: restart the edited game and replay what the player did so far.

## Tier 1: replay

Build this first. It's simpler, it never shows a state that's wrong, and it tells us how long a replay takes on real games before we commit to patching.

The Preview records every input since the game started, the same steps walkthrough recording captures (`recordStep` in `wasm-player.js`: commands, `menu:`, `answer:` and `event:` steps), whether or not a walkthrough is being recorded. **Replay to here** asks the editor for fresh game bytes, as a new Preview's `ready` message does, restarts with them, and runs the steps through `WalkthroughRunner` with the status banner in its `replay` mode.

For the replay to reach the same state, the run has to be reproducible:

- **The random seed.** The Preview picks a seed when it starts and passes it to `WasmPlayerBridge.SetRandomSeed`, which today only the `seed` test parameter does, and the replay reuses it. The engine has been deterministic under a fixed seed since #2271.
- **Time.** `SendCommand` takes the elapsed time and timers fire from `uiTick`, so a game with timers replays differently unless the timing goes into the steps too. Record each step's elapsed time and the ticks between steps, and replay them as recorded instead of in real time. Waits and pauses (`uiEndWait`, `uiEndPause`) aren't recorded today; the runner already gets past them, but it needs checking that it gets past them at the same points.

A replay can go somewhere new: after the lamp is deleted, `take lamp` just fails. That's correct, since it's what the edited game does. The replay carries on through every step, and the Preview marks the first turn whose output differs from what the player saw the first time, so the author can see where the edit changed things. Stopping there instead is an option, but most edits change some output on purpose, so stopping would stop too often.

The replay doesn't touch the editor's walkthroughs. It's the Preview's own record, gone when the Preview closes.

## Tier 2: patching the running game

### Change sets

Everything the editor edits is an element with attributes: rooms, objects and exits are objects whose `parent` attribute places them, and functions, types, commands, verbs, turn scripts and timers are elements too. So a change set only needs three kinds of change:

- **Element added**, with its attributes and its type list.
- **Element removed.**
- **Attribute changed**, with the editor's value before and after the edit (either can be "not set").

The editor keeps a snapshot of what it last sent the Preview, as `_previewInterface` does for the interface attributes now, and after each edit, undo and redo (debounced, as now) compares the game against it, one element at a time. `EditorController` does the comparing and returns the change set as JSON, alongside `GetLiveInterfaceAttributes`. Values are compared as the saver writes them (`GameSaver` in `SaveMode.Editor`), so scripts, lists and dictionaries compare by their text and object references by name.

Interface attributes stay on their own path. They're simply copied onto the game, which is right for them, because a player can't change them.

### Applying a change: compare with the editor's old value

The Preview applies a change set between turns, under the same conditions as interface changes: no turn running or held, no wait, menu or question pending (`applyInterfaceChanges`'s checks, and `ReapplyInterfaceAsync`'s `_pendingCallbackCount`). A new `WorldModel.ApplyEditorChangesAsync` applies each change by one rule:

- **Attribute changed:** if the running game's value is still the editor's old value, the player hasn't touched it, so set the new value. Otherwise leave it, and report "changed during play". This covers moving an object (its `parent`): it moves unless the player has already moved it, by taking it, say.
- **Element added:** create it if no element of that name exists. If one does, it's an object the game made while running (a clone, or one from `create`), so report a name clash and don't apply the change.
- **Element removed:** remove it if none of its attributes have changed during play and nothing the game made while running points to it. Otherwise report it. Removing an object also removes the objects inside it, as `destroy` does.

Code (functions, scripts on objects, commands, verbs, types) goes through the same rule but in practice always applies, since games rarely change their own scripts. Changing a type reaches every object that doesn't override the changed attribute, since inherited attributes are looked up when they're read. Changing an object's type list needs checking: the engine sets it at load time, and it isn't clear yet whether it can be changed safely afterwards.

Some changes can't take effect in a game that's already running, so they're reported as "only takes effect when the game starts" and offer the replay:

- `game.start`, the player's starting room, and `game.pov`.
- Templates. `[TemplateName]` references are filled in when the game loads, so a changed template doesn't reach the attributes that already used it. Dynamic templates are functions and apply as usual.
- Timers that are already running keep their interval until they next restart. A new timer applies as an added element.
- Included libraries being added or removed.

After applying a set, the Preview sends the lists again (`UpdateListsAsync`), so the exits, places and inventory panels update, and refreshes the Debugger if it's open.

### The room the player is in

A change touches the current room (the `parent` of `game.pov`) if it changes the room itself, or an element whose `parent` is that room before or after the change, which covers its objects and exits. Then the output gets a short marker, something like "This room was changed in the editor. Type `look` to see it again." The look command could go in the marker as a link.

It doesn't reprint the description by itself. A reprint clutters the transcript, and the room may well be mid-scene, so its description can be wrong for the moment the player is in.

### Undo

The game keeps an undo transaction open between turns (Core starts one with `start transaction` at each command, and `UndoLogger.RollTransaction` closes the previous one), so a patch made between turns would go into the last turn's undo, and the player's next `undo` would revert the author's edit. The patch has to run with logging off.

Older undo transactions are a problem too: undoing a turn from before a patch puts back old values over attributes the patch has since changed. The simple fix is to clear the undo history whenever a patch changes anything other than code, and say so in the change list. Undo matters much less in a Preview than in a real game.

### Saves

The Preview has no Save button (`setCanSave(!currentIsPreview)`), and that stays true: a patched game is a mix of two versions of the game, and shouldn't be offered as a real save.

## Where the UI goes

The change list and the **Replay to here** button belong to the host (`wasm-player.js` and `chrome.css`), so the legacy and v2 UIs both get them. The current-room marker goes into the output through the UI adapter, so each UI styles it like its other notices.

## Phases

1. **Replay.** Pick and keep a seed, record every input with its timing, add the **Replay to here** button, mark the first turn whose output differs. Useful by itself: it's "restart with my changes and take me back to where I was".
2. **Change sets.** `EditorController` produces them from a snapshot, the editor sends them over the channel, and the Preview shows the change list. Nothing is applied yet: every change is reported as "replay to see this", which tests the comparing and the UI on real editing.
3. **Applying.** `ApplyEditorChangesAsync` with the old-value rule, additions first, then attribute changes, then removals. The lists update and the current-room marker shows. Undo logging is off for the patch, and the history is cleared.
4. **Edge cases.** Name clashes with objects made at runtime, changes to type lists, removing elements that runtime objects still point to.

Each phase needs an e2e script driving the editor and a Preview together, like the existing Preview interface ones, wired into `e2e.yml`.

## Open questions

- Should the Preview apply changes as they arrive, or collect them until the author clicks Apply? Applying as they arrive matches interface changes. But typing a long description sends it many times, so the debounce may need to be longer than 200 ms here.
- Should code changes apply mid-wait? A function changed while a menu is open could be used as soon as the player answers. Holding everything until the turn is over is simpler, and matches interface changes.
- How long does a replay take on a big game? If it's slow, the Preview could keep a save from every few turns and replay from the nearest one before the first change, though a change that applies at start would still need a replay from the start.
