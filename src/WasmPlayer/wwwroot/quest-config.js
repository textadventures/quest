// Quest Viva WasmPlayer configuration.
// Replace this file (or set window.QuestVivaConfig before wasm-player.js loads) to customise.
window.QuestVivaConfig = {
    textAdventuresApiRoot: 'https://textadventures.co.uk/api/',

    // Set this to always load a specific game, e.g. when self-hosting WasmPlayer
    // alongside a single .quest/.aslx file. Visitors land straight in the game
    // instead of seeing the file/URL picker. Can be a relative path (resolved
    // against this site) or an absolute URL to a file hosted elsewhere (subject
    // to that host's CORS policy). A ?url= or ?id= query param still overrides
    // this, so links to other games keep working.
    // defaultGameUrl: 'my-game.quest',
};
