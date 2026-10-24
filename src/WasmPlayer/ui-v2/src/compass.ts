// The compass pane, and the exits strip (the "exits" part) that repeats its available exits under
// the transcript on a narrow layout, where the compass is tucked away in the panes drawer (#2581).
//
// The twelve directions are named by the game (setCompassDirections, from game.compassdirections,
// so translated games get their own words), in a fixed order: NW, N, NE, W, E, SW, S, SE, up,
// down, in, out. A direction's button sends its name as a command, as legacy's does. The engine
// says which are available after each turn (the adapter's updateCompass).

import { pane } from './panes';
import { player } from './player';
import { state } from './state';

const DEFAULT_DIRECTIONS = ['northwest', 'north', 'northeast', 'west', 'east', 'southwest', 'south', 'southeast', 'up', 'down', 'in', 'out'];
const ARROWS = ['↖', '↑', '↗', '←', '→', '↙', '↓', '↘', '↥', '↧', '', ''];
// Where each of the eight points sits in the compass pane's 3x3 grid (row, column), around an
// empty middle. Up/down and in/out sit beside it in a 2x2 block, as on legacy's compass.
const GRID = [[1, 1], [1, 2], [1, 3], [2, 1], [2, 3], [3, 1], [3, 2], [3, 3]];

let available: string[] = [];

function directions(): string[] {
    return state.compassDirections.length === 12 ? state.compassDirections : DEFAULT_DIRECTIONS;
}

// "in" and "out" are labelled by interface strings, which can differ from the commands they send.
function label(index: number): string {
    const name = directions()[index];
    if (index === 10) return textOf(state.interfaceStrings.get('InButtonLabel')) || name;
    if (index === 11) return textOf(state.interfaceStrings.get('OutButtonLabel')) || name;
    return name;
}

function textOf(html: string | undefined): string {
    if (!html) return '';
    const template = document.createElement('template');
    template.innerHTML = html;
    return template.content.textContent?.trim() ?? '';
}

function directionButton(index: number, className: string): HTMLButtonElement {
    const name = directions()[index];
    const button = document.createElement('button');
    button.type = 'button';
    button.className = className;
    button.dataset.qvDirection = name;
    if (ARROWS[index]) {
        const arrow = document.createElement('span');
        arrow.className = 'qv-direction-arrow';
        arrow.setAttribute('aria-hidden', 'true');
        arrow.textContent = ARROWS[index];
        button.append(arrow);
    }
    return button;
}

/** Draws the compass pane and the exits strip from the current directions, labels and exits. */
export function renderCompass(): void {
    const compass = pane('compass');
    if (compass) {
        const grid = document.createElement('div');
        grid.className = 'qv-compass-points';
        const others = document.createElement('div');
        others.className = 'qv-compass-others';
        directions().forEach((name, i) => {
            const button = directionButton(i, 'qv-compass-button');
            const text = label(i);
            // An arrow speaks as its direction; in and out show their words too.
            button.setAttribute('aria-label', text);
            button.title = text;
            if (!ARROWS[i]) button.append(text);
            button.disabled = !available.includes(name);
            if (GRID[i]) {
                button.style.gridRow = String(GRID[i][0]);
                button.style.gridColumn = String(GRID[i][1]);
                grid.append(button);
            } else {
                others.append(button);
            }
        });
        const compassElement = document.createElement('div');
        compassElement.className = 'qv-compass';
        compassElement.append(grid, others);
        compass.body.replaceChildren(compassElement);
    }
    updateExitStrip();
}

/** The adapter's updateCompass: the available directions, "/"-separated. */
export function updateCompass(data: string): void {
    available = String(data ?? '').split('/').filter(Boolean);
    renderCompass();
}

export function resetCompass(): void {
    available = [];
}

/**
 * The exits strip: the available exits only, as buttons, shown on a narrow layout (v2.css) while
 * the compass would be showing (the game hasn't hidden it or the panes) and between turns, so a
 * tap can't be taken as the answer to a wait, menu or get input.
 */
export function updateExitStrip(): void {
    const root = player();
    const strip = root?.exits;
    if (!root || !strip) return;
    const compass = pane('compass');
    const compassShown = !!compass && !compass.hidden && !root.panes?.hidden;
    const betweenTurns = !state.turnPending && !state.waiting && !state.pausing && !state.inDialog && !state.gameFinished;
    const buttons: HTMLButtonElement[] = [];
    if (compassShown && betweenTurns) {
        directions().forEach((name, i) => {
            if (!available.includes(name)) return;
            const button = directionButton(i, 'qv-exit');
            button.append(label(i));
            buttons.push(button);
        });
    }
    strip.replaceChildren(...buttons);
    strip.setAttribute('aria-label', textOf(state.interfaceStrings.get('CompassLabel')) || 'Compass');
    strip.classList.toggle('qv-exits-active', buttons.length > 0);
}
