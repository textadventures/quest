// routes/+error.svelte: an address that isn't one of the app's pages gets a plain "Page not
// found" with a way home, not SvelteKit's bare "404 Not Found". The crash half of the page (the
// real message and stack, and saving the open game) needs a component that throws, so it was
// checked by hand with a temporary throw instead.
import { test } from '@playwright/test';
import { chromium } from '../lib/tracked-chromium.mjs';

test('an unknown address shows the not-found page', async ({ baseURL }) => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    try {
        await page.goto(`${baseURL}/no-such-page`);
        await page.getByTestId('error-page').waitFor({ timeout: 30000 });
        await page.getByRole('heading', { name: 'Page not found' }).waitFor({ timeout: 10000 });
        const home = page.getByRole('link', { name: 'Go to the home page' });
        const href = await home.getAttribute('href');
        if (!href?.endsWith('/')) throw new Error(`home link goes to ${href}`);
        if (await page.locator('#error-details-heading').count() !== 0) {
            throw new Error('the not-found page shows crash details');
        }
    } finally {
        await browser.close();
    }
});
