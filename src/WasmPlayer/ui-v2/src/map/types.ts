// What v2's main bundle (../map.ts) and its lazily loaded map renderer (map.js, built from
// main.ts here) share. Type-only, so neither bundle carries the other's code.

/** What the renderer needs from the rest of v2. */
export interface MapHost {
    /** A click on a grid square, while the game wants them (QuestUI.map.respondToClicks). */
    gridSquareClicked(x: number, y: number): void;
    /** Whether to animate the player's dot and recentring (not under reduced motion or a walkthrough). */
    animate(): boolean;
    /** Translated labels for the map and its zoom buttons. */
    strings(): { map: string; zoomIn: string; zoomOut: string };
}

export interface MapRenderer {
    /** A QuestUI.map member by name, with its arguments as 610 CoreGrid passes them. */
    call(name: string, args: unknown[]): void;
}

export interface MapModule {
    create(container: HTMLElement, host: MapHost): MapRenderer;
}

declare global {
    interface Window {
        /** Set by map.js once it has loaded. */
        QuestUIV2Map?: MapModule;
    }
}
