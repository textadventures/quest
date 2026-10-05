# Vendored player libraries

These files are checked in, not installed from npm, and are deliberately pinned to old versions. `WasmPlayer.csproj` copies them into the AppBundle's `lib/`, and `index.html` loads them.

| File | Version | Source |
|---|---|---|
| `jquery-2.1.1.min.js` | jQuery 2.1.1 | Identical to npm's `jquery@2.1.1` `dist/jquery.min.js`, minus its trailing `sourceMappingURL` comment |
| `jquery-ui.min.js` | jQuery UI 1.11.2 | Full custom download from jqueryui.com (every widget and effect; see the header). jQuery UI 1.11.x was never published to npm, which jumps from 1.10.5 to 1.12.0 |
| `jquery-ui.min.css`, `images/` | jQuery UI 1.11.1 | ThemeRoller theme (the Redmond preset). The ThemeRoller URL in the CSS header records its exact settings |
| `jquery.multi-open-accordion-1.5.3.js` | 1.5.3 | jQuery UI plugin by Anas Nakawa (2011), from Google Code, which is now gone. Not on npm. Used for the sidebar panes (`multiOpenAccordion` in `playercore.js`) |
| `paper.js` | Paper.js 0.9.12 | Browser build. npm's `paper@0.9.12` only ships `paper-node.js`. Used by the map (`grid.js`) and due to go when the map is rebuilt as SVG (#2437) |

## Why they're pinned

Games ship their own JavaScript, and Quest 5-era games were written against exactly these versions, using `$`, jQuery UI widgets and the player's own markup. Upgrading would change behaviour for those games:

- jQuery 3 removed APIs such as `.size()` and the `.load()`/`.unload()`/`.error()` event shorthands.
- jQuery UI 1.11 predates jQuery 3 support, which arrived in 1.12.
- jQuery UI 1.12 also reworked `.button()`, which `playercore.js` and `wasm-player.js` use, and split checkbox/radio buttons out into a new `checkboxradio` widget.

So an upgrade is a game-compatibility project, to be tested against a corpus of real games, not a routine dependency bump. That's also why Dependabot doesn't manage these files: it would offer jQuery 3 and 4.

## Known advisories

These versions have published advisories, and GitHub's dependency scanning can't see vendored files, so they're listed here:

- jQuery 2.1.1:
  - CVE-2015-9251 (fixed in 3.0.0)
  - CVE-2019-11358 (fixed in 3.4.0)
  - CVE-2020-11022 and CVE-2020-11023 (fixed in 3.5.0)
- jQuery UI 1.11.2:
  - CVE-2016-7103 (fixed in 1.12.0)
  - CVE-2021-41182, CVE-2021-41183 and CVE-2021-41184 (fixed in 1.13.0)

They all need untrusted input passed to a particular jQuery or jQuery UI API. In the player, the only inputs are the game itself and the player's own typing. A game already runs arbitrary JavaScript in this page (through `JS.` calls and its own `<javascript>` resources), so none of these lets a game do anything it couldn't already do. Revisit this if the player ever renders content from a third party.
