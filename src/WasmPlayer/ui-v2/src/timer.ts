// The game clock: counts seconds once the engine asks for a tick (a timer, or SetTimeout), and
// hands the count back with the next command or tick. Same behaviour as legacy's player.js.

let tickCount = 0;
let nextTickAfter = 0;
let interval: ReturnType<typeof setInterval> | undefined;
let manual = false;

export function requestNextTimerTick(seconds: number): void {
    nextTickAfter = seconds;
    start();
}

/** Stops the clock and returns the seconds counted since it started. */
export function takeTickCount(): number {
    stop();
    return tickCount;
}

/** ?clock=manual: game time only moves when a test calls QuestVivaTest.tick. */
export function useManualClock(): void {
    manual = true;
    stop();
}

export function stop(): void {
    if (interval === undefined) return;
    clearInterval(interval);
    interval = undefined;
}

function start(): void {
    if (manual || interval !== undefined) return;
    tickCount = 0;
    interval = setInterval(async () => {
        tickCount++;
        if (nextTickAfter > 0 && tickCount >= nextTickAfter) {
            await WebPlayer.uiTick(takeTickCount());
        }
    }, 1000);
}
