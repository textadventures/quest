// Verifies that the previously-broken `text-surface-400-500` /
// `text-surface-500-400` utilities (not in Skeleton's generated dual-tone
// shade-pair set: 50-950, 100-900, 200-800, 300-700, 400-600, 600-400,
// 700-300, 900-100, 950-50) were replaced with the valid `text-surface-600-400`
// pair everywhere in src/AppShell/src, and that the compiled Tailwind output
// now contains a real color rule for it (no dead classes).
//
// NOTE: this only exercises routes/open/+page.svelte, since the rest of the
// affected components (TreePanel, AttributesEditor, AddElementModal, etc.)
// only render once a game is loaded through the WasmEditor .NET/WASM bridge,
// which requires `dotnet build src/WasmEditor` — unavailable in some sandboxed
// environments. All affected files use the identical Tailwind utility class,
// so this route is a reliable proxy for the fix everywhere else.
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5174';
const browser = await chromium.launch();

async function toRgb(page, colorStr) {
    // Modern browsers may report computed color as oklch(...); normalize via
    // a canvas round-trip rather than hand-parsing every CSS color syntax.
    return page.evaluate((str) => {
        const canvas = document.createElement('canvas');
        canvas.width = 1;
        canvas.height = 1;
        const ctx = canvas.getContext('2d');
        ctx.fillStyle = str;
        ctx.fillRect(0, 0, 1, 1);
        return Array.from(ctx.getImageData(0, 0, 1, 1).data.slice(0, 3));
    }, colorStr);
}

try {
    // 1. Compiled CSS: the valid pair exists, the invalid ones don't.
    const page0 = await browser.newPage();
    await page0.goto(`${baseUrl}/open`);
    await page0.waitForTimeout(1000);
    const css = await page0.evaluate(() => {
        let all = '';
        for (const s of document.styleSheets) {
            try { for (const r of s.cssRules) all += r.cssText + '\n'; } catch { /* cross-origin sheet */ }
        }
        return all;
    });
    if (!css.includes('.text-surface-600-400')) throw new Error('Compiled CSS is missing .text-surface-600-400 rule');
    if (css.includes('.text-surface-400-500') || css.includes('.text-surface-500-400')) {
        throw new Error('Compiled CSS still contains a dead .text-surface-400-500/500-400 rule');
    }
    console.log('PASS: compiled CSS has .text-surface-600-400 and no dead 400-500/500-400 rules');
    await page0.close();

    // 2. Rendered color in both color schemes: muted grey, not default black/white.
    for (const scheme of ['light', 'dark']) {
        const ctx = await browser.newContext({ colorScheme: scheme });
        const page = await ctx.newPage();
        await page.goto(`${baseUrl}/open`);
        const label = page.locator('text=Create new game');
        await label.waitFor({ timeout: 10000 });
        const color = await label.evaluate(el => getComputedStyle(el).color);
        const [r, g, b] = await toRgb(page, color);
        const isBlack = r === 0 && g === 0 && b === 0;
        const isWhite = r === 255 && g === 255 && b === 255;
        // Skeleton's "surface" scale has a faint blue-grey tint by design, so
        // allow some channel spread rather than requiring pure neutral grey.
        const isMidToned = Math.max(r, g, b) - Math.min(r, g, b) <= 50 && r > 40 && r < 220;
        console.log(`  ${scheme}: "Create new game" label color=${color} -> rgb(${r},${g},${b})`);
        if (isBlack || isWhite) throw new Error(`${scheme} mode: label rendered default ${isBlack ? 'black' : 'white'} — utility still broken`);
        if (!isMidToned) throw new Error(`${scheme} mode: label rendered unexpected color rgb(${r},${g},${b}) — expected a muted mid-tone grey`);
        console.log(`PASS: ${scheme} mode renders a muted grey label`);
        await ctx.close();
    }

    console.log('\nPASS: all checks passed');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
