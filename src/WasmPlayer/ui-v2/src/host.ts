// What v2 uses of the host (wasm-player.js, game-saver.js), and the globals it defines for it.

import type { PlayerUiAdapter } from './adapter';
import type { QuestUIContract } from './contract';

export interface WebPlayerHost {
    sendCommand(command: string, tickCount: number, metadata: Record<string, string> | null): Promise<void>;
    uiSendEvent(eventName: string, param: string): Promise<void>;
    uiTick(tickCount: number): Promise<void>;
    uiEndWait(): Promise<void>;
    uiChoice(choice: string): Promise<void>;
    uiChoiceCancel(): Promise<void>;
    uiSetQuestionResponse(response: boolean): Promise<void>;
}

declare global {
    const WebPlayer: WebPlayerHost;
    const GameSaver: { save(): Promise<void> };
    /** A game's own stylesheet, added after the player UI's. */
    function addExternalStylesheet(url: string): void;
    /** Resume the engine after a wait or pause, then call the adapter's waitEnded/pauseEnded. */
    function sendEndWait(): void;
    function sendEndPause(): void;

    interface Window {
        QuestPlayerUis?: Record<string, PlayerUiAdapter>;
        QuestUI: QuestUIContract;
        /** Read and set by the host too: it's true again once a command's turn has finished. */
        canSendCommand: boolean;
        sendCommand(text: string, metadata?: Record<string, string>): void;
        ASLEvent(name: string, param?: unknown): boolean;
        /** Called by the host around each turn the player starts (see playerTurn). */
        notifyQuestUITurn?(started: boolean): void;
        /** Called by the host when it swaps in fresh markup for a new game or restart. */
        resetQuestUIState?(): void;
        /** Restarts the game in place (QuestUI.restart, and Quest 5 Core's restart command). */
        RestartGame?(): void;
    }
}
