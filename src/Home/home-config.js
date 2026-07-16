// Quest Viva Home page configuration.
window.QuestVivaHomeConfig = {
    catalogApiUrl: 'https://textadventures.co.uk/api/catalog',

    // The highest ASL version this build's WasmPlayer can load — passed to
    // the catalog API so it excludes games written for a newer ASL version
    // than this player understands. Must track the `Versions` dictionary in
    // src/Engine/GameLoader/GameLoader.cs; bump this alongside any change
    // there that adds a new version.
    maxAslVersion: 580,
};
