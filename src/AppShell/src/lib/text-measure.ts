let ctx: CanvasRenderingContext2D | null = null;

// Measures a string's rendered width for a given CSS `font` shorthand (as returned by
// getComputedStyle(el).font) - used to size a control (a <select> or <input>) to its own
// current text precisely, rather than approximating via a `ch`-per-character count. That
// approximation is off by an amount that depends on the actual character mix (the "0" glyph
// that defines 1ch is wider than the average lowercase letter or space), and the error scales
// with string length - fine for a short word, visibly wrong for a long phrase.
export function measureTextPx(text: string, font: string): number {
    if (!ctx) {
        ctx = document.createElement("canvas").getContext("2d");
    }
    if (!ctx) return 0;
    ctx.font = font;
    return ctx.measureText(text).width;
}
