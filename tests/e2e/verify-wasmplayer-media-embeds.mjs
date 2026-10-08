// Regression coverage for issue #2364: ShowYouTube emitted a fixed 425x344
// iframe, which made the page scroll sideways on a phone, and only images in
// the output were capped at the text width - an author's own <video> or
// <iframe> overflowed the same way. ShowYouTube (and the legacy AddVimeo) now
// size by aspect ratio, and playercore.css caps video and iframe like img.
//
// Requires the WasmPlayer dev server running locally:
//   node src/WasmPlayer/dev-server.mjs
import { chromium } from 'playwright';

const baseUrl = process.argv[2] || 'http://localhost:5175';

const browser = await chromium.launch();

async function run(viewport, label) {
    const page = await browser.newPage({ viewport });
    page.on('pageerror', err => console.log('[pageerror]', err.message));
    // Keep the test offline: the embeds only need to lay out, not play.
    await page.route(/youtube\.com|vimeo\.com/, route => route.fulfill({ status: 200, contentType: 'text/html', body: '' }));
    try {
        await page.goto(`${baseUrl}/?url=/e2e-fixtures/media-embeds-test.aslx`);
        await page.waitForFunction(() => document.getElementById('divOutput')?.textContent.includes('Embeds done.'), null, { timeout: 30000 });

        const m = await page.evaluate(() => {
            const box = el => el.getBoundingClientRect();
            const output = document.getElementById('divOutput');
            const iframes = [...output.querySelectorAll('iframe')];
            return {
                outputWidth: output.clientWidth,
                scrollWidth: document.documentElement.scrollWidth,
                windowWidth: window.innerWidth,
                youtube: box(iframes.find(f => f.src.includes('youtube.com'))),
                vimeo: box(iframes.find(f => f.src.includes('vimeo.com'))),
                authorIframe: box(document.getElementById('authoriframe')),
                authorVideo: box(document.getElementById('authorvideo')),
            };
        });

        if (m.scrollWidth > m.windowWidth) {
            throw new Error(`[${label}] page scrolls sideways: scrollWidth ${m.scrollWidth} > ${m.windowWidth}`);
        }
        for (const [name, rect] of Object.entries({ youtube: m.youtube, vimeo: m.vimeo, authorIframe: m.authorIframe, authorVideo: m.authorVideo })) {
            if (rect.width > m.outputWidth + 1) {
                throw new Error(`[${label}] ${name} is ${rect.width}px wide, wider than the ${m.outputWidth}px output`);
            }
        }
        for (const [name, rect] of Object.entries({ youtube: m.youtube, vimeo: m.vimeo })) {
            const ratio = rect.width / rect.height;
            if (Math.abs(ratio - 16 / 9) > 0.02) {
                throw new Error(`[${label}] ${name} is ${rect.width}x${rect.height}, not 16:9`);
            }
        }
        console.log(`PASS: [${label}] no sideways scroll; YouTube ${Math.round(m.youtube.width)}x${Math.round(m.youtube.height)}, ` +
            `Vimeo ${Math.round(m.vimeo.width)}x${Math.round(m.vimeo.height)}, author iframe ${Math.round(m.authorIframe.width)}px, ` +
            `author video ${Math.round(m.authorVideo.width)}px, output ${m.outputWidth}px`);
    } finally {
        await page.close();
    }
}

try {
    await run({ width: 390, height: 844 }, 'phone');
    await run({ width: 1280, height: 900 }, 'desktop');
} catch (err) {
    console.error('FAIL:', err.message);
    process.exitCode = 1;
} finally {
    await browser.close();
}
