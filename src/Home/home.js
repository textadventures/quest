// Quest Viva Home — Play/Create landing page (play.questviva.com root).
// Plain HTML/CSS/JS, no build step, no WASM: this page only needs to render a
// game catalog and link out to /player/ and /editor/, so it deliberately
// doesn't pull in the WasmPlayer/WebEditor toolchains.
(function () {
    'use strict';

    function $(id) { return document.getElementById(id); }

    // ── Tabs ─────────────────────────────────────────────────────────────────

    function wireTabs() {
        const tabs = [
            { button: $('qv-tab-play'), panel: $('qv-panel-play') },
            { button: $('qv-tab-create'), panel: $('qv-panel-create') },
        ];

        function select(target) {
            for (const { button, panel } of tabs) {
                const active = button === target;
                button.classList.toggle('active', active);
                button.setAttribute('aria-selected', String(active));
                panel.classList.toggle('hidden', !active);
            }
        }

        for (const { button } of tabs) {
            button.addEventListener('click', () => select(button));
        }
    }

    // ── Client info (analytics metadata sent to the catalog API) ────────────

    // window.electronApp is injected by ElectronApp's preload.ts contextBridge
    // (see docs/electron-desktop-app.md) and is only ever present there, never
    // in a plain browser tab — the same check WebEditor's runtime.ts uses.
    function getClientInfo(version) {
        const isElectron = typeof window.electronApp !== 'undefined';
        return {
            source: isElectron ? 'electron' : 'web',
            version: version || null,
            // Not yet exposed by preload.ts (see docs/electron-desktop-app.md
            // Phase 3) — reads through automatically once it is.
            platform: (isElectron && window.electronApp.platform) || null,
        };
    }

    async function fetchVersion() {
        try {
            const response = await fetch('VERSION', { cache: 'no-cache' });
            if (!response.ok) return null;
            return (await response.text()).trim() || null;
        } catch {
            return null;
        }
    }

    // ── Catalog ──────────────────────────────────────────────────────────────

    function buildCatalogUrl(clientInfo) {
        const config = window.QuestVivaHomeConfig;
        const params = new URLSearchParams({ maxAslVersion: String(config.maxAslVersion) });
        if (clientInfo.source) params.set('source', clientInfo.source);
        if (clientInfo.version) params.set('version', clientInfo.version);
        if (clientInfo.platform) params.set('platform', clientInfo.platform);
        return `${config.catalogApiUrl}?${params.toString()}`;
    }

    function ratingStars(rating) {
        const rounded = Math.round(rating);
        return '★'.repeat(Math.max(0, Math.min(5, rounded))) + '☆'.repeat(5 - Math.max(0, Math.min(5, rounded)));
    }

    function buildGameCard(game) {
        const card = document.createElement('a');
        card.className = 'qv-game-card';
        card.href = `player/?id=${encodeURIComponent(game.id)}`;

        const coverWrap = document.createElement('div');
        coverWrap.className = 'qv-game-cover';
        const coverUrl = game.cover || game.thumbnail;
        if (coverUrl) {
            const img = document.createElement('img');
            img.src = coverUrl;
            img.alt = '';
            img.loading = 'lazy';
            coverWrap.appendChild(img);
        }
        card.appendChild(coverWrap);

        const name = document.createElement('div');
        name.className = 'qv-game-name';
        name.textContent = game.name;
        card.appendChild(name);

        if (game.author) {
            const author = document.createElement('div');
            author.className = 'qv-game-author';
            author.textContent = `by ${game.author}`;
            card.appendChild(author);
        }

        if (game.rating > 0) {
            const rating = document.createElement('div');
            rating.className = 'qv-game-rating';
            rating.textContent = ratingStars(game.rating);
            card.appendChild(rating);
        }

        return card;
    }

    function renderCategories(categories) {
        const container = $('qv-play-categories');
        container.textContent = '';

        for (const category of categories) {
            if (!category.games || category.games.length === 0) continue;

            const section = document.createElement('section');
            section.className = 'qv-category';

            const heading = document.createElement('h2');
            heading.textContent = category.title;
            section.appendChild(heading);

            const grid = document.createElement('div');
            grid.className = 'qv-game-grid';
            for (const game of category.games) {
                grid.appendChild(buildGameCard(game));
            }
            section.appendChild(grid);

            container.appendChild(section);
        }
    }

    async function loadCatalog(clientInfo) {
        const loading = $('qv-play-loading');
        const error = $('qv-play-error');
        const categories = $('qv-play-categories');

        loading.classList.remove('hidden');
        error.classList.add('hidden');
        categories.textContent = '';

        try {
            const response = await fetch(buildCatalogUrl(clientInfo));
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const data = await response.json();
            renderCategories(data.categories || []);
        } catch {
            error.classList.remove('hidden');
        } finally {
            loading.classList.add('hidden');
        }
    }

    // ── Boot ─────────────────────────────────────────────────────────────────

    document.addEventListener('DOMContentLoaded', async () => {
        wireTabs();

        const version = await fetchVersion();
        const versionEl = $('qv-version');
        if (version) versionEl.textContent = `v${version}`;

        const clientInfo = getClientInfo(version);

        $('qv-play-retry').addEventListener('click', () => loadCatalog(clientInfo));
        await loadCatalog(clientInfo);
    });
})();
