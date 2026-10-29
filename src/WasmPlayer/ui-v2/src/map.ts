// The map (the "map" part): QuestUI.map, setMapInteractive and setMapPosition on v2. It sits
// under the picture frame in the sticky header, game.mapsize pixels high (map.show), at every
// width. On a narrow layout its height is capped, and a top bar button collapses it.
//
// setMapPosition("panes") moves it into a pane of its own, first in the panes region, wherever the
// layout has panes (the column, drawer or sheet). With no panes it stays in the header.
//
// The drawing is done in SVG by map.js (map/renderer.ts), which is loaded the first time a game
// calls a map member, since most games have no map. Calls made before it has loaded are kept,
// in order, and replayed once it has.

import { sendEvent } from './engine';
import type { MapHost, MapRenderer } from './map/types';
import { type QvPane, pane } from './panes';
import { player } from './player';
import { state } from './state';

// Captured while v2.js runs, since document.currentScript is only set then. map.js is beside it,
// with the same ?v= cache key, wherever v2.js came from (a CDN, for the single-file export).
const v2ScriptUrl = document.currentScript instanceof HTMLScriptElement ? document.currentScript.src : '';

let loading: Promise<void> | null = null;
let renderer: MapRenderer | null = null;
let rendererView: HTMLElement | null = null;
let queue: [string, unknown[]][] = [];

// Per game: the height map.show gave (0 until it's called), whether the game has hidden the
// part (setVisible("map", false)), and whether the player has collapsed it on a narrow layout.
let size = 0;
let visible = true;
let collapsed = false;
let position: 'header' | 'panes' = 'header';

const chromeString = (key: string, fallback: string): string => WebPlayer.chromeStrings?.[key] ?? fallback;

const host: MapHost = {
    gridSquareClicked: (x, y) => void sendEvent('JS_GridSquareClick', `${x};${y}`),
    animate: () => state.animateScroll && !matchMedia('(prefers-reduced-motion: reduce)').matches,
    strings: () => ({
        map: chromeString('MapLabel', 'Map'),
        zoomIn: chromeString('MapZoomIn', 'Zoom in'),
        zoomOut: chromeString('MapZoomOut', 'Zoom out'),
    }),
};

function loadRenderer(): Promise<void> {
    if (window.QuestUIV2Map) return Promise.resolve();
    loading ??= new Promise<void>((resolve, reject) => {
        const url = new URL(v2ScriptUrl ? 'map.js' : 'v2/map.js', v2ScriptUrl || document.baseURI);
        if (v2ScriptUrl) url.search = new URL(v2ScriptUrl).search;
        const script = document.createElement('script');
        script.src = url.href;
        script.onload = () => resolve();
        script.onerror = () => {
            loading = null;
            reject(new Error(`Failed to load ${url.href}`));
        };
        document.head.append(script);
    });
    return loading;
}

function view(): HTMLElement | null {
    return player()?.map?.querySelector<HTMLElement>('.qv-map-view') ?? null;
}

function flush(): void {
    const container = view();
    if (!container || !window.QuestUIV2Map) return;
    if (!renderer || rendererView !== container) {
        renderer = window.QuestUIV2Map.create(container, host);
        rendererView = container;
    }
    const calls = queue;
    queue = [];
    for (const [name, args] of calls) {
        try {
            renderer.call(name, args);
        } catch (e) {
            console.error(e);
        }
    }
}

/** A QuestUI.map member, by name. */
export function mapCall(name: string, args: unknown[]): void {
    if (!player()?.map) return;
    if (name === 'show') {
        const height = typeof args[0] === 'number' ? args[0] : parseFloat(String(args[0]));
        size = Number.isFinite(height) ? height : 0;
        updateMap();
    }
    if (renderer && rendererView === view() && queue.length === 0) {
        renderer.call(name, args);
        return;
    }
    queue.push([name, args]);
    loadRenderer().then(flush, (e: unknown) => console.error(e));
}

/** setVisible("map", …). */
export function setMapVisible(value: boolean): void {
    visible = value;
    updateMap();
}

/** QuestUI.setMapPosition: header (the default) or panes. Any other value changes nothing. */
export function setMapPosition(value: string): void {
    if (value !== 'header' && value !== 'panes') return;
    position = value;
    updateMap();
}

/**
 * Shows the part once map.show has given it a height, in the header or its pane, and the collapse
 * button where it's narrow and in the header.
 */
export function updateMap(): void {
    const p = player();
    const region = p?.map;
    if (!p || !region) return;
    region.hidden = !(size > 0 && visible);
    region.style.setProperty('--qv-map-height', `${size}px`);
    region.classList.toggle('qv-map-collapsed', collapsed);
    const inPanes = placeMap(region);
    const button = p.mapButton;
    if (button) {
        button.hidden = region.hidden || inPanes || !p.hasAttribute('data-qv-narrow');
        button.textContent = chromeString('MapLabel', 'Map');
        button.setAttribute('aria-expanded', String(!collapsed));
    }
    p.updateTopbar();
}

// Moves the map into its pane, or back under the picture frame, and says which. Moving the element
// keeps its renderer, which redraws for its new size.
function placeMap(region: HTMLElement): boolean {
    const p = player();
    if (!p) return false;
    const inPanes = position === 'panes' && p.layout !== 'none' && !!p.panes && !p.panes.hidden;
    let mapPane = pane('map');
    if (inPanes && !mapPane && p.panes) {
        mapPane = document.createElement('qv-pane') as QvPane;
        mapPane.dataset.qvPane = 'map';
        mapPane.setAttribute('label', chromeString('MapLabel', 'Map'));
        p.panes.prepend(mapPane);
    }
    if (inPanes && mapPane) {
        if (region.parentElement !== mapPane.body) mapPane.body.append(region);
        mapPane.hidden = region.hidden;
    } else {
        const header = p.querySelector('.qv-header');
        if (header && region.parentElement !== header) header.append(region);
        if (mapPane) mapPane.hidden = true;
    }
    return inPanes;
}

export function toggleMapCollapsed(): void {
    collapsed = !collapsed;
    updateMap();
}

/** For a new game or restart, whose fresh markup has no map until the game draws one. */
export function resetMap(): void {
    renderer = null;
    rendererView = null;
    queue = [];
    size = 0;
    visible = true;
    collapsed = false;
    position = 'header';
}
