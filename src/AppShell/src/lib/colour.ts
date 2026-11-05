// Helpers for ColourInput.svelte. A game's colour attributes hold any CSS colour the player's
// browser understands (a name such as "DarkOrange", "#336699", "rgb(0 0 0)"), so rather than
// keeping our own parser the browser parses them: CSS.supports to validate, and a 2D canvas's
// fillStyle, which normalises any colour it accepts to "#rrggbb" (or rgba() when translucent).

let ctx: CanvasRenderingContext2D | null | undefined;

export function isCssColour(value: string): boolean {
    const v = value.trim();
    return v !== "" && typeof CSS !== "undefined" && CSS.supports("color", v);
}

// The colour as "#rrggbb", for seeding <input type="color">, or null when it isn't a colour.
// Alpha is dropped: the native picker has no alpha channel.
export function toHexColour(value: string): string | null {
    if (!isCssColour(value)) return null;
    if (ctx === undefined) ctx = document.createElement("canvas").getContext("2d");
    if (!ctx) return null;
    ctx.fillStyle = "#000000";
    ctx.fillStyle = value.trim();
    const normalised = String(ctx.fillStyle);
    if (normalised.startsWith("#")) return normalised.toLowerCase();
    const m = normalised.match(/rgba?\(\s*([\d.]+)\s*,\s*([\d.]+)\s*,\s*([\d.]+)/);
    if (!m) return null;
    return "#" + m.slice(1, 4).map(n => Math.round(Number(n)).toString(16).padStart(2, "0")).join("");
}
