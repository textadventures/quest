import { loadWasm } from "#lib/wasm.js";

export interface GameTemplate {
    id: string;
    label: string;
    type: "textadventure" | "gamebook";
}

export async function getGameTemplates(): Promise<GameTemplate[]> {
    const bridge = await loadWasm();
    return JSON.parse(bridge.GetGameTemplates()) as GameTemplate[];
}
