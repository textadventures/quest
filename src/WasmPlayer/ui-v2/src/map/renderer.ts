// The map in SVG (#2437), for QuestUI.map on v2. Built as its own script, map.js, which v2 loads
// the first time a game uses the map (../map.ts), since most games never do.
//
// It takes the same calls, with the same arguments, as legacy's Grid_* functions (contract
// decision 3), and keeps legacy's behaviour where 610 CoreGrid relies on it:
//   - Coordinates are grid squares, drawn at the scale in effect when each item is drawn.
//   - Each z-level is a layer. Drawing a room, label or the player makes its level the current
//     one, and every other level is faded to 0.2.
//   - Lines, arrows, grid lines and the custom drawing calls go to the current level, or to the
//     custom layer while the game has shown it (showCustomLayer(true)) and hasn't drawn a room
//     since.
//   - Reusing an id replaces the item, with arrows, squares and shapes sharing one set of ids and
//     SVG symbols and images having one each, as on legacy.
//   - The first room drawn centres the map, as does the player when first drawn; after that the
//     player's dot moves to its new room and the map recentres on it.
// Where legacy's paper.js renderer had bugs, this doesn't copy them (#2437, #2435): the custom
// layer is an overlay above the map rather than a replacement for it, everything pans together
// under one transform, and positions and sizes aren't truncated to whole squares.

import type { MapHost, MapRenderer } from './types';

const SVG_NS = 'http://www.w3.org/2000/svg';
const FADED_LEVEL_OPACITY = 0.2;
// Legacy moved the dot, then recentred, each over ten frames.
const MOVE_MS = 170;
// How far a press may move and still count as a click rather than a drag.
const DRAG_THRESHOLD_PX = 4;
const KEY_PAN_PX = 40;
const ZOOM_STEP = 1.25;
const MIN_ZOOM = 0.1;
const MAX_ZOOM = 10;

type IdKind = 'objects' | 'svg' | 'images';
interface Point { x: number; y: number }

let instanceCount = 0;

function num(value: unknown): number {
    const n = typeof value === 'number' ? value : parseFloat(String(value));
    return Number.isFinite(n) ? n : 0;
}

// A colour as the game gives it (any CSS colour), set as an attribute so it can't be anything
// but a value. An empty one draws nothing, as paper.js did.
function colour(value: unknown, fallback = 'none'): string {
    const text = value == null ? '' : String(value).trim();
    return text === '' ? fallback : text;
}

function svg<K extends keyof SVGElementTagNameMap>(name: K, attributes: Record<string, string | number> = {}): SVGElementTagNameMap[K] {
    const element = document.createElementNS(SVG_NS, name);
    for (const [key, value] of Object.entries(attributes)) element.setAttribute(key, String(value));
    return element;
}

/** One z-level, or the custom layer: groups drawn in order, so labels and borders stay above fills (#1038). */
class Layer {
    readonly g = svg('g');
    readonly grid = svg('g');
    readonly fills = svg('g');
    readonly shapes = svg('g');
    readonly lines = svg('g', { 'stroke-linecap': 'round', 'stroke-linejoin': 'miter', fill: 'none' });
    readonly labels = svg('g', { class: 'qv-map-labels' });

    constructor(readonly z: number) {
        this.g.append(this.grid, this.fills, this.shapes, this.lines, this.labels);
        this.g.dataset.z = String(z);
    }

    clear(): void {
        for (const group of [this.grid, this.fills, this.shapes, this.lines, this.labels]) group.replaceChildren();
    }
}

class SvgMap implements MapRenderer {
    readonly #host: MapHost;
    readonly #svg: SVGSVGElement;
    readonly #defs = svg('defs');
    readonly #viewport = svg('g');
    readonly #levelsGroup = svg('g');
    readonly #levels = new Map<number, Layer>();
    readonly #custom = new Layer(NaN);
    readonly #zoomButtons: HTMLElement;
    readonly #prefix = `qv-map-${++instanceCount}`;

    #scale = 30;
    #currentZ = 0;
    #drawingCustom = false;
    #firstBox = true;
    #respondToClicks = false;
    #interactive = true;

    readonly #ids: Record<IdKind, Map<string, Element>> = { objects: new Map(), svg: new Map(), images: new Map() };
    readonly #symbols = new Map<string, string>();
    #symbolCount = 0;
    #shapePoints: Point[] = [];

    // The view: the map point at the centre, and the zoom.
    #centre: Point = { x: 0, y: 0 };
    #zoom = 1;

    #player: SVGCircleElement | null = null;
    #playerAt: Point = { x: 0, y: 0 };
    #animation: { kind: 'player' | 'centre'; from: Point; to: Point; start: number } | null = null;
    #frame = 0;

    readonly #pointers = new Map<number, Point>();
    #press: { start: Point; dragged: boolean } | null = null;
    #pinch: { distance: number } | null = null;

    constructor(container: HTMLElement, host: MapHost) {
        this.#host = host;
        const strings = host.strings();
        this.#svg = svg('svg', { class: 'qv-map-svg', role: 'group' });
        this.#svg.setAttribute('aria-label', strings.map);
        this.#custom.g.classList.add('qv-map-custom');
        this.#custom.g.setAttribute('display', 'none');
        this.#viewport.append(this.#levelsGroup, this.#custom.g);
        this.#svg.append(this.#defs, this.#viewport);

        this.#zoomButtons = document.createElement('div');
        this.#zoomButtons.className = 'qv-map-zoom';
        this.#zoomButtons.append(
            this.#button('+', strings.zoomIn, () => this.#zoomBy(ZOOM_STEP)),
            this.#button('−', strings.zoomOut, () => this.#zoomBy(1 / ZOOM_STEP)),
        );
        container.replaceChildren(this.#svg, this.#zoomButtons);

        this.#svg.addEventListener('pointerdown', (e) => this.#pointerDown(e));
        this.#svg.addEventListener('pointermove', (e) => this.#pointerMove(e));
        this.#svg.addEventListener('pointerup', (e) => this.#pointerUp(e));
        this.#svg.addEventListener('pointercancel', (e) => this.#pointerEnd(e));
        this.#svg.addEventListener('wheel', (e) => this.#wheel(e), { passive: false });
        this.#svg.addEventListener('keydown', (e) => this.#keyDown(e));
        new ResizeObserver(() => this.#applyView()).observe(this.#svg);
        this.#setInteractive(true);
        this.#activate(0);
    }

    call(name: string, args: unknown[]): void {
        const a = args;
        switch (name) {
            case 'setScale': {
                const scale = num(a[0]);
                if (scale > 0) this.#scale = scale;
                break;
            }
            case 'setCentre':
                this.#cancelAnimation();
                this.#centreOn(this.#point(a[0], a[1]));
                break;
            case 'setInteractive':
                this.#setInteractive(!!a[0] && a[0] !== 'false');
                break;
            case 'respondToClicks':
                this.#respondToClicks = !!a[0] && a[0] !== 'false';
                this.#svg.classList.toggle('qv-map-clickable', this.#respondToClicks);
                break;
            case 'drawBox':
                this.#drawBox(a);
                break;
            case 'drawLabel':
                this.#drawLabel(a);
                break;
            case 'drawPlayer':
                this.#drawPlayer(a);
                break;
            case 'drawLine':
                this.#target().lines.append(this.#line(a[0], a[1], a[2], a[3], a[4], a[5]));
                break;
            case 'drawArrow':
                this.#drawArrow(a);
                break;
            case 'drawGridLines':
                this.#drawGridLines(a);
                break;
            case 'drawSquare':
                this.#drawSquare(a);
                break;
            case 'addShapePoint':
                this.#shapePoints.push({ x: num(a[0]), y: num(a[1]) });
                break;
            case 'drawShape':
                this.#drawShape(a);
                break;
            case 'loadSvg':
                this.#loadSvg(a[0], a[1]);
                break;
            case 'drawSvg':
                this.#drawSvg(a);
                break;
            case 'drawImage':
                this.#drawImage(a);
                break;
            case 'showCustomLayer': {
                // Legacy's Grid_ShowCustomLayer took the string "true"; 610 CoreGrid passes a boolean.
                const visible = a[0] === true || a[0] === 'true';
                this.#custom.g.setAttribute('display', visible ? 'inline' : 'none');
                this.#drawingCustom = visible;
                break;
            }
            case 'clearCustomLayer':
                this.#custom.clear();
                this.#forgetRemoved();
                break;
            case 'clearAllLayers':
                // The map levels only, as on legacy: the custom layer and the view are kept, and the
                // player is drawn afresh (and centred on) next time.
                for (const level of this.#levels.values()) level.g.remove();
                this.#levels.clear();
                this.#player = null;
                this.#cancelAnimation();
                this.#forgetRemoved();
                this.#activate(this.#currentZ);
                break;
        }
    }

    // ── Drawing ──────────────────────────────────────────────────────────────

    #point(x: unknown, y: unknown): Point {
        return { x: num(x) * this.#scale, y: num(y) * this.#scale };
    }

    #level(z: number): Layer {
        let level = this.#levels.get(z);
        if (!level) {
            level = new Layer(z);
            this.#levels.set(z, level);
            // Higher levels are drawn above lower ones.
            const above = [...this.#levels.values()].filter((l) => l.z > z).sort((p, q) => p.z - q.z)[0];
            this.#levelsGroup.insertBefore(level.g, above?.g ?? null);
        }
        return level;
    }

    /** Map drawing (a room, label or the player) makes its level current and fades the rest. */
    #activate(z: number): Layer {
        this.#currentZ = z;
        this.#drawingCustom = false;
        const current = this.#level(z);
        for (const level of this.#levels.values()) {
            level.g.setAttribute('opacity', level === current ? '1' : String(FADED_LEVEL_OPACITY));
        }
        return current;
    }

    /** Where lines, arrows, grid lines and the custom drawing calls go. */
    #target(): Layer {
        return this.#drawingCustom ? this.#custom : this.#level(this.#currentZ);
    }

    #line(x1: unknown, y1: unknown, x2: unknown, y2: unknown, stroke: unknown, width: unknown): SVGLineElement {
        const start = this.#point(x1, y1);
        const end = this.#point(x2, y2);
        return svg('line', {
            x1: start.x, y1: start.y, x2: end.x, y2: end.y,
            stroke: colour(stroke), 'stroke-width': num(width),
        });
    }

    // drawBox(x, y, z, width, height, border, borderWidth, fill, sides): sides is a bit per edge
    // to draw, N=8, E=4, S=2, W=1.
    #drawBox(a: unknown[]): void {
        const [x, y, z, width, height] = [num(a[0]), num(a[1]), num(a[2]), num(a[3]), num(a[4])];
        const level = this.#activate(z);
        if (this.#firstBox) {
            this.#firstBox = false;
            this.#centreOn(this.#point(x + width / 2, y + height / 2));
        }
        const corners = [this.#point(x, y), this.#point(x + width, y), this.#point(x + width, y + height), this.#point(x, y + height)];
        const [nw, se] = [corners[0], corners[2]];
        level.fills.append(svg('rect', {
            x: nw.x, y: nw.y, width: se.x - nw.x, height: se.y - nw.y, fill: colour(a[7]),
        }));

        const borderWidth = num(a[6]);
        if (borderWidth <= 0) return;
        const sides = num(a[8]);
        const draw = [sides & 8, sides & 4, sides & 2, sides & 1];
        // Each run of adjacent edges is one stroke, so its corners join.
        let d = '';
        if (draw.every(Boolean)) {
            d = `M${corners.map((c) => `${c.x},${c.y}`).join('L')}Z`;
        } else {
            const first = draw.findIndex((edge) => !edge);
            let open = false;
            for (let n = 1; n <= 4; n++) {
                const i = (first + n) % 4;
                if (draw[i]) {
                    const from = corners[i];
                    const to = corners[(i + 1) % 4];
                    d += open ? `L${to.x},${to.y}` : `M${from.x},${from.y}L${to.x},${to.y}`;
                    open = true;
                } else {
                    open = false;
                }
            }
        }
        if (d) level.lines.append(svg('path', { d, stroke: colour(a[5]), 'stroke-width': borderWidth }));
    }

    #drawLabel(a: unknown[]): void {
        const level = this.#activate(num(a[2]));
        const at = this.#point(a[0], a[1]);
        const text = svg('text', { x: at.x, y: at.y, 'text-anchor': 'middle', fill: colour(a[4], 'black') });
        text.textContent = a[3] == null ? '' : String(a[3]);
        level.labels.append(text);
    }

    // drawPlayer(x, y, z, radius, border, borderWidth, fill)
    #drawPlayer(a: unknown[]): void {
        const level = this.#activate(num(a[2]));
        const to = this.#point(a[0], a[1]);
        if (!this.#player) {
            this.#player = svg('circle', {
                class: 'qv-map-player', r: num(a[3]), stroke: colour(a[4]), 'stroke-width': num(a[5]), fill: colour(a[6]),
            });
            level.g.append(this.#player);
            this.#placePlayer(to);
            this.#cancelAnimation();
            this.#centreOn(to);
            return;
        }
        // Last in its level, so it's drawn above the rooms.
        level.g.append(this.#player);
        if (!this.#host.animate()) {
            this.#cancelAnimation();
            this.#placePlayer(to);
            this.#centreOn(to);
            return;
        }
        this.#animate('player', { ...this.#playerAt }, to);
    }

    #placePlayer(at: Point): void {
        this.#playerAt = at;
        this.#player?.setAttribute('cx', String(at.x));
        this.#player?.setAttribute('cy', String(at.y));
    }

    // drawArrow(id, x1, y1, x2, y2, border, borderWidth): a line with a 10px head at its end.
    #drawArrow(a: unknown[]): void {
        const start = this.#point(a[1], a[2]);
        const end = this.#point(a[3], a[4]);
        const g = svg('g', { stroke: colour(a[5]), 'stroke-width': num(a[6]) });
        g.append(this.#line(a[1], a[2], a[3], a[4], a[5], a[6]));
        const length = Math.hypot(end.x - start.x, end.y - start.y);
        if (length > 0) {
            const ux = ((end.x - start.x) / length) * 10;
            const uy = ((end.y - start.y) / length) * 10;
            const rotate = (degrees: number): string => {
                const r = (degrees * Math.PI) / 180;
                return `${end.x + ux * Math.cos(r) - uy * Math.sin(r)},${end.y + ux * Math.sin(r) + uy * Math.cos(r)}`;
            };
            g.append(svg('path', { d: `M${rotate(150)}L${end.x},${end.y}L${rotate(-150)}` }));
        }
        this.#replace('objects', a[0], g, this.#target().lines);
    }

    // drawGridLines(minX, minY, maxX, maxY, border): one line per square, as a single path.
    #drawGridLines(a: unknown[]): void {
        const [minX, minY, maxX, maxY] = [num(a[0]), num(a[1]), num(a[2]), num(a[3])];
        let d = '';
        for (let x = minX; x <= maxX; x++) {
            const [p, q] = [this.#point(x, minY), this.#point(x, maxY)];
            d += `M${p.x},${p.y}L${q.x},${q.y}`;
        }
        for (let y = minY; y <= maxY; y++) {
            const [p, q] = [this.#point(minX, y), this.#point(maxX, y)];
            d += `M${p.x},${p.y}L${q.x},${q.y}`;
        }
        if (d) this.#target().grid.append(svg('path', { d, stroke: colour(a[4]), 'stroke-width': 1, fill: 'none' }));
    }

    // drawSquare(id, x, y, width, height, text, fill): inset by a pixel, with optional text in the middle.
    #drawSquare(a: unknown[]): void {
        const [x, y, width, height] = [num(a[1]), num(a[2]), num(a[3]), num(a[4])];
        const nw = this.#point(x, y);
        const se = this.#point(x + width, y + height);
        const fill = colour(a[6]);
        const g = svg('g');
        g.append(svg('rect', {
            x: nw.x + 1, y: nw.y + 1, width: Math.max(0, se.x - nw.x - 2), height: Math.max(0, se.y - nw.y - 2),
            fill, stroke: fill,
        }));
        if (a[5] != null && String(a[5]) !== '') {
            const centre = this.#point(x + width / 2, y + height / 2);
            const text = svg('text', { x: centre.x, y: centre.y, 'text-anchor': 'middle', fill: 'black', class: 'qv-map-label' });
            text.textContent = String(a[5]);
            g.append(text);
        }
        this.#replace('objects', a[0], g, this.#target().shapes);
    }

    // drawShape(id, border, fill, opacity): a polygon through the points addShapePoint gathered.
    #drawShape(a: unknown[]): void {
        const points = this.#shapePoints.map((p) => this.#point(p.x, p.y));
        this.#shapePoints = [];
        const polygon = svg('polygon', {
            points: points.map((p) => `${p.x},${p.y}`).join(' '), stroke: colour(a[1]), fill: colour(a[2]),
        });
        if (a[3] != null && String(a[3]) !== '') polygon.setAttribute('opacity', String(num(a[3])));
        this.#replace('objects', a[0], polygon, this.#target().shapes);
    }

    // loadSvg(data, id): keeps an SVG document as a symbol for drawSvg.
    #loadSvg(data: unknown, id: unknown): void {
        const doc = new DOMParser().parseFromString(String(data ?? ''), 'image/svg+xml');
        const source = doc.documentElement;
        if (source.nodeName !== 'svg' || doc.querySelector('parsererror')) return;
        // A game's own SVG, but nothing in it may run.
        for (const element of source.querySelectorAll('script, foreignObject')) element.remove();
        for (const element of [source, ...source.querySelectorAll('*')]) {
            for (const attribute of [...element.attributes]) {
                const unsafe = /^on/i.test(attribute.name)
                    || (/href$/i.test(attribute.name) && /^\s*javascript:/i.test(attribute.value));
                if (unsafe) element.removeAttribute(attribute.name);
            }
        }
        const symbolId = `${this.#prefix}-symbol-${++this.#symbolCount}`;
        const symbol = svg('symbol', { id: symbolId, preserveAspectRatio: 'none' });
        const viewBox = source.getAttribute('viewBox')
            ?? (source.hasAttribute('width') && source.hasAttribute('height')
                ? `0 0 ${num(source.getAttribute('width'))} ${num(source.getAttribute('height'))}` : null);
        if (viewBox) symbol.setAttribute('viewBox', viewBox);
        for (const child of [...source.childNodes]) symbol.append(document.importNode(child, true));
        const previous = this.#symbols.get(String(id));
        if (previous) this.#defs.querySelector(`#${previous}`)?.remove();
        this.#defs.append(symbol);
        this.#symbols.set(String(id), symbolId);
    }

    // drawSvg(id, symbolId, x, y, width, height): a loaded symbol stretched over those squares.
    #drawSvg(a: unknown[]): void {
        const symbolId = this.#symbols.get(String(a[1]));
        if (!symbolId) {
            console.log(`No symbol loaded with id '${String(a[1])}'`);
            return;
        }
        const existing = this.#ids.svg.get(String(a[0]));
        const use = existing ?? svg('use');
        use.setAttribute('href', `#${symbolId}`);
        this.#placeBox(use, a[2], a[3], a[4], a[5]);
        if (!existing) this.#replace('svg', a[0], use, this.#target().shapes);
    }

    // drawImage(id, url, x, y, width, height): stretched over those squares, as on legacy.
    #drawImage(a: unknown[]): void {
        const existing = this.#ids.images.get(String(a[0]));
        const image = existing ?? svg('image', { preserveAspectRatio: 'none' });
        image.setAttribute('href', String(a[1] ?? ''));
        this.#placeBox(image, a[2], a[3], a[4], a[5]);
        if (!existing) this.#replace('images', a[0], image, this.#target().shapes);
    }

    #placeBox(element: Element, x: unknown, y: unknown, width: unknown, height: unknown): void {
        const nw = this.#point(x, y);
        element.setAttribute('x', String(nw.x));
        element.setAttribute('y', String(nw.y));
        element.setAttribute('width', String(Math.max(0, num(width) * this.#scale)));
        element.setAttribute('height', String(Math.max(0, num(height) * this.#scale)));
    }

    #replace(kind: IdKind, id: unknown, element: Element, group: SVGGElement): void {
        const key = String(id ?? '');
        this.#ids[kind].get(key)?.remove();
        this.#ids[kind].set(key, element);
        group.append(element);
    }

    #forgetRemoved(): void {
        for (const ids of Object.values(this.#ids)) {
            for (const [id, element] of ids) if (!element.isConnected) ids.delete(id);
        }
    }

    // ── The view ─────────────────────────────────────────────────────────────

    #centreOn(at: Point): void {
        this.#centre = { ...at };
        this.#applyView();
    }

    #applyView(): void {
        const width = this.#svg.clientWidth;
        const height = this.#svg.clientHeight;
        const tx = width / 2 - this.#zoom * this.#centre.x;
        const ty = height / 2 - this.#zoom * this.#centre.y;
        this.#viewport.setAttribute('transform', `translate(${tx} ${ty}) scale(${this.#zoom})`);
    }

    /** The map point under a point in the SVG's own pixels. */
    #toMap(x: number, y: number): Point {
        return {
            x: this.#centre.x + (x - this.#svg.clientWidth / 2) / this.#zoom,
            y: this.#centre.y + (y - this.#svg.clientHeight / 2) / this.#zoom,
        };
    }

    // Panning or zooming is the player taking charge of the view, so the dot finishes its move at
    // once and the map doesn't recentre on it afterwards.
    #panBy(dx: number, dy: number): void {
        this.#cancelAnimation();
        this.#centre = { x: this.#centre.x - dx / this.#zoom, y: this.#centre.y - dy / this.#zoom };
        this.#applyView();
    }

    /** Zooms, keeping the map point under (x, y) in the SVG, or its centre, where it is. */
    #zoomBy(factor: number, x?: number, y?: number): void {
        if (!this.#interactive) return;
        this.#cancelAnimation();
        const zoom = Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, this.#zoom * factor));
        if (x != null && y != null) {
            const before = this.#toMap(x, y);
            this.#zoom = zoom;
            const after = this.#toMap(x, y);
            this.#centre = { x: this.#centre.x + before.x - after.x, y: this.#centre.y + before.y - after.y };
        } else {
            this.#zoom = zoom;
        }
        this.#applyView();
    }

    #setInteractive(on: boolean): void {
        this.#interactive = on;
        this.#zoomButtons.hidden = !on;
        this.#svg.classList.toggle('qv-map-interactive', on);
        if (on) {
            this.#svg.setAttribute('tabindex', '0');
            this.#svg.setAttribute('aria-keyshortcuts', 'ArrowUp ArrowDown ArrowLeft ArrowRight + -');
        } else {
            this.#svg.removeAttribute('tabindex');
            this.#svg.removeAttribute('aria-keyshortcuts');
            this.#press = null;
            this.#pinch = null;
        }
    }

    #button(text: string, label: string, action: () => void): HTMLButtonElement {
        const button = document.createElement('button');
        button.type = 'button';
        button.textContent = text;
        button.setAttribute('aria-label', label);
        button.addEventListener('click', action);
        return button;
    }

    // ── Animation ────────────────────────────────────────────────────────────

    // The player's dot moves to its new room, then the map recentres on it.
    #animate(kind: 'player' | 'centre', from: Point, to: Point): void {
        this.#animation = { kind, from, to, start: performance.now() };
        if (!this.#frame) this.#frame = requestAnimationFrame((t) => this.#step(t));
    }

    #step(now: number): void {
        this.#frame = 0;
        const animation = this.#animation;
        if (!animation) return;
        const t = Math.min(1, (now - animation.start) / MOVE_MS);
        const at = {
            x: animation.from.x + (animation.to.x - animation.from.x) * t,
            y: animation.from.y + (animation.to.y - animation.from.y) * t,
        };
        if (animation.kind === 'player') this.#placePlayer(at);
        else this.#centreOn(at);
        if (t < 1) {
            this.#frame = requestAnimationFrame((next) => this.#step(next));
        } else if (animation.kind === 'player') {
            this.#animate('centre', { ...this.#centre }, { ...this.#playerAt });
        } else {
            this.#animation = null;
        }
    }

    /** Stops an animation, leaving the dot where it was going and the view where it is. */
    #cancelAnimation(): void {
        const animation = this.#animation;
        if (!animation) return;
        this.#animation = null;
        if (animation.kind === 'player') this.#placePlayer(animation.to);
    }

    // ── Pointer and keyboard ─────────────────────────────────────────────────

    #local(e: PointerEvent | WheelEvent): Point {
        const box = this.#svg.getBoundingClientRect();
        return { x: e.clientX - box.left, y: e.clientY - box.top };
    }

    #pointerDown(e: PointerEvent): void {
        if (e.pointerType === 'mouse' && e.button !== 0) return;
        this.#pointers.set(e.pointerId, this.#local(e));
        if (this.#pointers.size === 1) {
            this.#press = { start: this.#local(e), dragged: false };
        } else if (this.#pointers.size === 2 && this.#interactive) {
            this.#press = null;
            this.#pinch = { distance: this.#pointerDistance() };
        }
        if (this.#interactive) this.#svg.setPointerCapture(e.pointerId);
    }

    #pointerMove(e: PointerEvent): void {
        const last = this.#pointers.get(e.pointerId);
        if (!last) return;
        const at = this.#local(e);
        this.#pointers.set(e.pointerId, at);
        if (!this.#interactive) {
            if (this.#press && Math.hypot(at.x - this.#press.start.x, at.y - this.#press.start.y) > DRAG_THRESHOLD_PX) {
                this.#press.dragged = true;
            }
            return;
        }
        if (this.#pinch && this.#pointers.size === 2) {
            const distance = this.#pointerDistance();
            if (this.#pinch.distance > 0) {
                const [p, q] = [...this.#pointers.values()];
                this.#zoomBy(distance / this.#pinch.distance, (p.x + q.x) / 2, (p.y + q.y) / 2);
            }
            this.#pinch.distance = distance;
            return;
        }
        const press = this.#press;
        if (!press) return;
        if (!press.dragged && Math.hypot(at.x - press.start.x, at.y - press.start.y) <= DRAG_THRESHOLD_PX) return;
        if (!press.dragged) {
            press.dragged = true;
            this.#svg.classList.add('qv-map-dragging');
            this.#panBy(at.x - press.start.x, at.y - press.start.y);
        } else {
            this.#panBy(at.x - last.x, at.y - last.y);
        }
    }

    #pointerUp(e: PointerEvent): void {
        const press = this.#press;
        const wasPinch = !!this.#pinch;
        this.#pointerEnd(e);
        if (!press || press.dragged || wasPinch || !this.#respondToClicks) return;
        const at = this.#toMap(press.start.x, press.start.y);
        this.#host.gridSquareClicked(Math.floor(at.x / this.#scale), Math.floor(at.y / this.#scale));
    }

    #pointerEnd(e: PointerEvent): void {
        this.#pointers.delete(e.pointerId);
        if (this.#pointers.size < 2) this.#pinch = null;
        if (this.#pointers.size === 0) {
            this.#press = null;
            this.#svg.classList.remove('qv-map-dragging');
        }
    }

    #pointerDistance(): number {
        const [p, q] = [...this.#pointers.values()];
        return p && q ? Math.hypot(p.x - q.x, p.y - q.y) : 0;
    }

    #wheel(e: WheelEvent): void {
        if (!this.#interactive) return;
        e.preventDefault();
        // About one step of legacy's 1.1 per wheel notch.
        const pixels = e.deltaMode === WheelEvent.DOM_DELTA_LINE ? e.deltaY * 33 : e.deltaY;
        const at = this.#local(e);
        this.#zoomBy(1.1 ** (-pixels / 100), at.x, at.y);
    }

    #keyDown(e: KeyboardEvent): void {
        if (!this.#interactive || e.altKey || e.ctrlKey || e.metaKey) return;
        const pans: Record<string, [number, number]> = {
            ArrowUp: [0, KEY_PAN_PX], ArrowDown: [0, -KEY_PAN_PX], ArrowLeft: [KEY_PAN_PX, 0], ArrowRight: [-KEY_PAN_PX, 0],
        };
        if (pans[e.key]) {
            this.#panBy(...pans[e.key]);
        } else if (e.key === '+' || e.key === '=') {
            this.#zoomBy(ZOOM_STEP);
        } else if (e.key === '-' || e.key === '_') {
            this.#zoomBy(1 / ZOOM_STEP);
        } else {
            return;
        }
        e.preventDefault();
        // Not a key press that continues a wait (main.ts).
        e.stopPropagation();
    }
}

export function create(container: HTMLElement, host: MapHost): MapRenderer {
    return new SvgMap(container, host);
}
