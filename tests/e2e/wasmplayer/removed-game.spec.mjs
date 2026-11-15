import { test } from '@playwright/test';

// textadventures.co.uk answers api/Game/{id} with 410 Gone for a game it has
// removed. The player should say the game was removed, not suggest the player
// mistyped the address (#2785). Any other API status keeps the generic message.
async function errorTextFor(page, baseURL, status) {
    await page.route('**/api/gameprogress/**', (route) => route.fulfill({ status: 204 }));
    await page.route('**/api/game/**', (route) => route.fulfill({
        status,
        contentType: 'application/json',
        body: '{}',
    }));
    await page.goto(`${baseURL}/?id=test-game-id`);
    const errorEl = page.locator('#qv-error');
    await errorEl.waitFor({ state: 'visible', timeout: 15000 });
    return { text: await errorEl.innerText(), errorEl };
}

test('removed game (410) explains the removal', async ({ page, baseURL }) => {
    const { text, errorEl } = await errorTextFor(page, baseURL, 410);
    if (!text.includes('This game is no longer available.')) {
        throw new Error(`Expected the removed-game message, got: ${text}`);
    }
    if (/double-check/i.test(text)) {
        throw new Error(`Removed-game message should not suggest checking the address: ${text}`);
    }
    const href = await errorEl.locator('a').first().getAttribute('href');
    if (href !== 'https://textadventures.co.uk/') {
        throw new Error(`Expected a link to textadventures.co.uk, got ${href}`);
    }
    if (!(await page.locator('#qv-pickers').isVisible())) {
        throw new Error('Expected the file pickers to be shown');
    }
});

test('other API errors keep the generic message', async ({ page, baseURL }) => {
    const { text } = await errorTextFor(page, baseURL, 404);
    if (!text.includes('Couldn\'t load the game.') || !/double-check/i.test(text)) {
        throw new Error(`Expected the generic load error, got: ${text}`);
    }
});
