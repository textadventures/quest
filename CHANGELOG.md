# Changelog

## [6.0.0-beta.56](https://github.com/textadventures/quest/compare/v6.0.0-beta.55...v6.0.0-beta.56) (2026-08-28)


### Bug Fixes

* add expression control and hide-when-empty to script-parameter editors ([#2164](https://github.com/textadventures/quest/issues/2164)) ([d9f84c9](https://github.com/textadventures/quest/commit/d9f84c98349ca4675b1cd4e3f5364bebf3fbfd90))
* **AppShell:** default new rooms to top level, fix Combobox label/value mixup ([#2161](https://github.com/textadventures/quest/issues/2161)) ([f63ad3e](https://github.com/textadventures/quest/commit/f63ad3e658a5d409832e88251cb801acbbeb2ee5))
* **AppShell:** expression-editor parity for if/set/foreach templates ([#2151](https://github.com/textadventures/quest/issues/2151)) ([60f23dc](https://github.com/textadventures/quest/commit/60f23dca7602f234809b8942c626f1a111b683a3))
* **AppShell:** remove dead &lt;filefiltername&gt; and &lt;preview/&gt; hints ([#2152](https://github.com/textadventures/quest/issues/2152)) ([f4b107b](https://github.com/textadventures/quest/commit/f4b107b14cf6260d39ffcfdf7019b09aa3ae4f12))
* **AppShell:** remove dead &lt;keyname&gt; control hint ([#2153](https://github.com/textadventures/quest/issues/2153)) ([df18dce](https://github.com/textadventures/quest/commit/df18dce8a90e2d2aa4719e9df86432d1422ae2c1))
* **AppShell:** remove redundant pane header labels in editor ([#2144](https://github.com/textadventures/quest/issues/2144)) ([dd3f6b3](https://github.com/textadventures/quest/commit/dd3f6b31b237a79a4634d067f6fa06d9207813aa))
* **AppShell:** stop Enter-confirm from re-triggering the trigger button in modals ([#2160](https://github.com/textadventures/quest/issues/2160)) ([60f73e8](https://github.com/textadventures/quest/commit/60f73e83241e901f43ce4437117b0e3dce612bfb))
* **AppShell:** wire up &lt;bold/&gt; control hint for labels ([#2154](https://github.com/textadventures/quest/issues/2154)) ([923a517](https://github.com/textadventures/quest/commit/923a517fe3c4756f40725ef5f22ff7f5d027f1e5))
* **AppShell:** wire up &lt;colour&gt; control hint for script textboxes ([#2156](https://github.com/textadventures/quest/issues/2156)) ([36cd0cc](https://github.com/textadventures/quest/commit/36cd0cca4aabb63da16ece019f2faaf0c2e39497))
* **AppShell:** wire up &lt;freetext/&gt; control hint for dropdowns ([#2147](https://github.com/textadventures/quest/issues/2147)) ([f18d327](https://github.com/textadventures/quest/commit/f18d327ab8232bb8ac20bd5bd7cc5305fdb2b551))
* **AppShell:** wire up &lt;minimum&gt;/&lt;maximum&gt;/&lt;increment&gt; control hints ([#2148](https://github.com/textadventures/quest/issues/2148)) ([f12b8fe](https://github.com/textadventures/quest/commit/f12b8fe403d763baf67819f80e480af77721e25f))
* **AppShell:** wire up &lt;nullable/&gt; control hint ([#2149](https://github.com/textadventures/quest/issues/2149)) ([f82ca3d](https://github.com/textadventures/quest/commit/f82ca3d10985218b827b9e521e15eef200f51874))
* **AppShell:** wire up &lt;width/&gt; control hint to the frontend ([#2150](https://github.com/textadventures/quest/issues/2150)) ([c018a96](https://github.com/textadventures/quest/commit/c018a966a727a5511f3661e47392ef9c91d87d21))
* **Engine:** remove no-op RequestSpeak function ([#2146](https://github.com/textadventures/quest/issues/2146)) ([d430a68](https://github.com/textadventures/quest/commit/d430a68865d936265c7af4913538cf4cbd88a43b))
* **WasmEditor:** convert richtext newlines to &lt;br/&gt; on save, and back on load ([#2165](https://github.com/textadventures/quest/issues/2165)) ([ca4875a](https://github.com/textadventures/quest/commit/ca4875ab056630bae36f2e6c4e5d1f9d74506462))

## [6.0.0-beta.55](https://github.com/textadventures/quest/compare/v6.0.0-beta.54...v6.0.0-beta.55) (2026-08-24)


### Features

* **AppShell:** announce save status and add editor landmarks ([#2141](https://github.com/textadventures/quest/issues/2141)) ([994a229](https://github.com/textadventures/quest/commit/994a229bbeaf685d0203f1963df86c0723c2f735))
* **AppShell:** IntelliSense-style autocomplete for the script editor ([#2129](https://github.com/textadventures/quest/issues/2129)) ([6ec1745](https://github.com/textadventures/quest/commit/6ec174584099a3bfba28214cab696b20bdd2ba31))
* **AppShell:** keyboard-operable dropdown menu and icon button labels ([#2139](https://github.com/textadventures/quest/issues/2139)) ([3e0daca](https://github.com/textadventures/quest/commit/3e0dacaccd5212f2f27296f6621e4ad6f53d0c0b))
* **AppShell:** move function folders and add objects/functions in-place ([#2128](https://github.com/textadventures/quest/issues/2128)) ([5be71c4](https://github.com/textadventures/quest/commit/5be71c4bae23349b40a6950ad102b4c6d8653f38))
* **AppShell:** trap and restore focus in modal dialogs ([#2138](https://github.com/textadventures/quest/issues/2138)) ([c2e56c1](https://github.com/textadventures/quest/commit/c2e56c15f00008a2861e8d7805e454dcc99f5c10))
* **Engine:** accessible accordion sidebar panels ([#2136](https://github.com/textadventures/quest/issues/2136)) ([fc6e2c5](https://github.com/textadventures/quest/commit/fc6e2c5b418340a52f28a95a625af05a972262e9))
* **Engine:** keyboard support for the item/verb popup menu ([#2135](https://github.com/textadventures/quest/issues/2135)) ([cde20f6](https://github.com/textadventures/quest/commit/cde20f6bae0d4fa26aad6b2e0af635fc5081615e))
* **WasmPlayer:** show Debug options for .aslx played from the Play tab ([#2121](https://github.com/textadventures/quest/issues/2121)) ([3a2fbab](https://github.com/textadventures/quest/commit/3a2fbab875bd62a9a0ed0462555de2944e605ecd))
* **WasmPlayer:** sort the debugger's object list by Name ([#2127](https://github.com/textadventures/quest/issues/2127)) ([2020e15](https://github.com/textadventures/quest/commit/2020e156af54eb97bd7867ce8edba5cb88ac5751))


### Bug Fixes

* **AppShell:** accept .xml extension for included libraries ([#2123](https://github.com/textadventures/quest/issues/2123)) ([5737ecb](https://github.com/textadventures/quest/commit/5737ecbbb55c8fd5aaf43f9878f78698b1de57a8))
* **AppShell:** dialogs move focus on open so Escape closes them ([#2125](https://github.com/textadventures/quest/issues/2125)) ([e4c62c8](https://github.com/textadventures/quest/commit/e4c62c86026e6ef68f86ff1c7a8c0312dc3f3e91))
* **AppShell:** keep hover-revealed row actions visible to keyboard focus ([#2140](https://github.com/textadventures/quest/issues/2140)) ([dee05d5](https://github.com/textadventures/quest/commit/dee05d53f0340c5adb7567fd5a9ec50d68bd6166))
* **AppShell:** label PropertyEditor fields and announce validation errors ([#2137](https://github.com/textadventures/quest/issues/2137)) ([b3a143b](https://github.com/textadventures/quest/commit/b3a143b0152aba511434c39baf6b0c20f4b67e05))
* **AppShell:** match textarea in code-view e2e round-trip check ([#2117](https://github.com/textadventures/quest/issues/2117)) ([0a20b2f](https://github.com/textadventures/quest/commit/0a20b2f2549e7d52b4a57f58eedcca35b4274895))
* **AppShell:** script editor controls reclaim width on touch ([#2122](https://github.com/textadventures/quest/issues/2122)) ([e987e1a](https://github.com/textadventures/quest/commit/e987e1a3e45b23dc097c5d05b2eed5458ddb1775))
* **Engine:** announce game output and label controls for screen readers ([#2134](https://github.com/textadventures/quest/issues/2134)) ([22d2019](https://github.com/textadventures/quest/commit/22d20191ad5798093aa6e088161720f5ff0de4dc))
* **Engine:** announce the actual message in msgbox-family dialogs ([#2142](https://github.com/textadventures/quest/issues/2142)) ([58bd274](https://github.com/textadventures/quest/commit/58bd274f63b1076232d6eaecda004152009cdccf))
* **Engine:** make transcript viewer message translatable ([#2120](https://github.com/textadventures/quest/issues/2120)) ([58401b4](https://github.com/textadventures/quest/commit/58401b43b15b3ed756283b04cf721369169de850))
* support running multiple dev.sh instances concurrently ([#2130](https://github.com/textadventures/quest/issues/2130)) ([c135257](https://github.com/textadventures/quest/commit/c13525788520e158c7a362da01bc06d0fefb7b2b))

## [6.0.0-beta.54](https://github.com/textadventures/quest/compare/v6.0.0-beta.53...v6.0.0-beta.54) (2026-08-22)


### Features

* **AppShell:** default-to-code-view setting, taller attribute value box ([#2112](https://github.com/textadventures/quest/issues/2112)) ([932c590](https://github.com/textadventures/quest/commit/932c590061e14d4599f50d7f4ecd98e854ae17cf))
* **AppShell:** group library-origin elements by source file in editor tree ([#2113](https://github.com/textadventures/quest/issues/2113)) ([cfb74f4](https://github.com/textadventures/quest/commit/cfb74f40823bda2b260c1eed26788896b119b8e8))
* **AppShell:** let authors group Functions into user-named folders ([#2114](https://github.com/textadventures/quest/issues/2114)) ([091bcad](https://github.com/textadventures/quest/commit/091bcada7411736e0b08c51a07b906a746331371))
* **Engine:** add "require all keys" toggle for lockable containers ([#2092](https://github.com/textadventures/quest/issues/2092)) ([9c64f28](https://github.com/textadventures/quest/commit/9c64f282c689e5c106317e4f514fc29219d0e591))
* **Engine:** AddPageLink/RemovePageLink for Text Adventure games ([#2105](https://github.com/textadventures/quest/issues/2105)) ([d5549ee](https://github.com/textadventures/quest/commit/d5549ee4c5785bcfd4e2cba7abfa9edf2a73db3c))


### Bug Fixes

* **AppShell:** add rename support to ScriptDictionaryEditor ([#2091](https://github.com/textadventures/quest/issues/2091)) ([039a760](https://github.com/textadventures/quest/commit/039a760bd38cb91e95feb13cd5895308bc8055d8))
* **AppShell:** honor multiline/expand script control hints, auto-size code view ([#2115](https://github.com/textadventures/quest/issues/2115)) ([e9f26fb](https://github.com/textadventures/quest/commit/e9f26fbf7d9a3a3b3a47001468d9c9c459699765))
* **AppShell:** refresh code view when switching commands in editor ([#2111](https://github.com/textadventures/quest/issues/2111)) ([2086ca8](https://github.com/textadventures/quest/commit/2086ca8e04292813aa542eaa46e2bb76385811c5))
* **AppShell:** render Switch command's Cases editor in ScriptEditor ([#2090](https://github.com/textadventures/quest/issues/2090)) ([1141a8a](https://github.com/textadventures/quest/commit/1141a8a4902163f885f9fcb559e2dc9ba683ab12))
* **Engine:** make the 'in' operator work on dictionaries ([#2110](https://github.com/textadventures/quest/issues/2110)) ([56ec22c](https://github.com/textadventures/quest/commit/56ec22c6416aa80674078586042918009e8e513a))
* **Engine:** remove Vimeo support entirely, keep runtime playback for existing games ([#2106](https://github.com/textadventures/quest/issues/2106)) ([6f067f1](https://github.com/textadventures/quest/commit/6f067f137fe1ddd7fea7cb69dd227c9022edc7a3))
* **PlayerCore:** restore WriteToLog as a no-op ([#2107](https://github.com/textadventures/quest/issues/2107)) ([2135ae5](https://github.com/textadventures/quest/commit/2135ae5696aef86d174a86e66a6d689c3bc54db0))

## [6.0.0-beta.53](https://github.com/textadventures/quest/compare/v6.0.0-beta.52...v6.0.0-beta.53) (2026-08-16)


### Features

* **AppShell:** add collapsible code sections and autocompletion to CodeMirror editors ([#2082](https://github.com/textadventures/quest/issues/2082)) ([a2103bb](https://github.com/textadventures/quest/commit/a2103bb856f004e1b2060d9a5dede3260a985557))
* **AppShell:** add Light/Dark/Match system theme toggle to settings ([#2077](https://github.com/textadventures/quest/issues/2077)) ([3e12c6a](https://github.com/textadventures/quest/commit/3e12c6ac08108486a0b5134582c55021aaad2162))
* **AppShell:** collapsed-by-default game tree with per-game expansion state, expand/collapse-all and move up/down ([#2085](https://github.com/textadventures/quest/issues/2085)) ([af83647](https://github.com/textadventures/quest/commit/af83647bcc9785ff256f12243977fe5cb6c7a56e))
* **AppShell:** edit Included Library contents from the tree editor ([#2063](https://github.com/textadventures/quest/issues/2063)) ([6a90251](https://github.com/textadventures/quest/commit/6a902519ed3645cb411a67f53ff35666c35336eb))
* **AppShell:** fall back to Safe Mode raw XML editor when a game fails to load ([#2076](https://github.com/textadventures/quest/issues/2076)) ([5d72d50](https://github.com/textadventures/quest/commit/5d72d50fadd2f7057583c64bef235e3190d79cfa))


### Bug Fixes

* **AppShell:** render Command Pattern and Use/Give per-object controls ([#2086](https://github.com/textadventures/quest/issues/2086)) ([2f49d1c](https://github.com/textadventures/quest/commit/2f49d1cacfa51b172293ae98fb82c73b1ec90083))
* **AppShell:** show script selection checkboxes in nested blocks ([#2060](https://github.com/textadventures/quest/issues/2060)) ([dcf6a92](https://github.com/textadventures/quest/commit/dcf6a927098274f8f1d1a23a53649fb42954e3a8))
* **AppShell:** use Lucide icon for non-image assets ([#2078](https://github.com/textadventures/quest/issues/2078)) ([0368c38](https://github.com/textadventures/quest/commit/0368c385f4392cacff20d849ebb7ee587c14764c))
* **ElectronApp:** wire transcript preload into editor Preview popup ([#2084](https://github.com/textadventures/quest/issues/2084)) ([2dfca49](https://github.com/textadventures/quest/commit/2dfca49336419be8048c4537640aeee31b3ef4da))
* **Engine:** restore exit grid offset X/Y editor controls ([#2067](https://github.com/textadventures/quest/issues/2067)) ([12c2c2a](https://github.com/textadventures/quest/commit/12c2c2ab19dae1b2a1ce6091f5eb229774d33878))
* expand collapsed game tree nodes in e2e verbs-editor test ([#2087](https://github.com/textadventures/quest/issues/2087)) ([e8f3bf1](https://github.com/textadventures/quest/commit/e8f3bf1276581698cf4f02f05f9fb60c0ff28398))
* stop e2e unsaved-close test failing on temp-dir cleanup race ([#2083](https://github.com/textadventures/quest/issues/2083)) ([a8ccb77](https://github.com/textadventures/quest/commit/a8ccb77f00436d18483fa206a659a3a6abbe49cd))
* **WasmPlayer:** keep save-slot titles at the dialog's base font size ([#2079](https://github.com/textadventures/quest/issues/2079)) ([1198b4e](https://github.com/textadventures/quest/commit/1198b4e9fe711ee74b954f3551da00dfc2c190da))
* **WasmPlayer:** surface game-load errors on the start screen instead of hanging ([#2062](https://github.com/textadventures/quest/issues/2062)) ([71b4175](https://github.com/textadventures/quest/commit/71b41752686b1f35a7bce34434d4886fe472584f))

## [6.0.0-beta.52](https://github.com/textadventures/quest/compare/v6.0.0-beta.51...v6.0.0-beta.52) (2026-08-12)


### Features

* **AppShell:** add Page support to the Objects tab ([#2058](https://github.com/textadventures/quest/issues/2058)) ([7a95f9f](https://github.com/textadventures/quest/commit/7a95f9f09f6faebd4f98f3afcf7d02c1a89c9d49))
* **AppShell:** add tree node icons, collapse s_-prefixed icon indirection ([#2054](https://github.com/textadventures/quest/issues/2054)) ([271f6fc](https://github.com/textadventures/quest/commit/271f6fcf88e3e9df404bbaab6a378f5792214929))
* **Engine:** add Text Adventure dialogue pages (Pages) ([#2053](https://github.com/textadventures/quest/issues/2053)) ([9aaddf0](https://github.com/textadventures/quest/commit/9aaddf0b85af367b5927303abf9e7ea51646922e))
* translate the Save/Load dialog via per-language .aslx templates ([#2050](https://github.com/textadventures/quest/issues/2050)) ([1c6ffc0](https://github.com/textadventures/quest/commit/1c6ffc0128c9ae2612d170b676d0622d3ae8b1e6))
* **WasmPlayer:** port TranscriptViewer to WasmPlayer, fix Electron access ([#2046](https://github.com/textadventures/quest/issues/2046)) ([00e4b97](https://github.com/textadventures/quest/commit/00e4b97a9b887adbfb4ea34b3aef05d1d9fac8b5))


### Bug Fixes

* **AppShell:** give the player object its own tree icon ([#2057](https://github.com/textadventures/quest/issues/2057)) ([aac6b75](https://github.com/textadventures/quest/commit/aac6b75de0339306e6f69a8cf4a19cc59b289335))
* **AppShell:** show Paste on nested Add script buttons, fix if/then layout ([#2044](https://github.com/textadventures/quest/issues/2044)) ([dd9d823](https://github.com/textadventures/quest/commit/dd9d823ff1088745ddd49f833bc27febfe7869ee))
* **AppShell:** source Script Adder shortcut labels from .aslx common field ([#2045](https://github.com/textadventures/quest/issues/2045)) ([30c62f8](https://github.com/textadventures/quest/commit/30c62f8365007ea23641d2a452e6036b641a1883))
* **AppShell:** stop brief error flash when opening a game from server ([#2039](https://github.com/textadventures/quest/issues/2039)) ([098d4cc](https://github.com/textadventures/quest/commit/098d4cc2c0a64323a10a4bb5e2beb0af68e18b46))
* **AppShell:** stop import loading spinner from dropping early ([#2042](https://github.com/textadventures/quest/issues/2042)) ([9898f45](https://github.com/textadventures/quest/commit/9898f450e3b7b548de5c0c47f8504678c32f0067))
* **AppShell:** trim superfluous clause from download draft tooltip ([#2043](https://github.com/textadventures/quest/issues/2043)) ([9f15c2f](https://github.com/textadventures/quest/commit/9f15c2f7adc0f64b1777d07f2fe9675c480f9218))
* **EditorCore:** exclude dialogue Pages from object/room pickers ([#2056](https://github.com/textadventures/quest/issues/2056)) ([a8d8549](https://github.com/textadventures/quest/commit/a8d854910b71b6ce5c3b22f7ee2e5473459f0b93))
* **EditorCore:** initialize Text field on new Dynamic Templates ([#2055](https://github.com/textadventures/quest/issues/2055)) ([9a955cf](https://github.com/textadventures/quest/commit/9a955cfbeec43811e9490c63b1b7309e802bdf3b))
* **ElectronApp:** move transcripts and game saves off origin-scoped browser storage ([#2047](https://github.com/textadventures/quest/issues/2047)) ([4dc3988](https://github.com/textadventures/quest/commit/4dc3988aaf12b5904f8d3c2f95e577e267176ff1))
* **ElectronApp:** persist UI language setting across relaunches ([#2037](https://github.com/textadventures/quest/issues/2037)) ([6a11394](https://github.com/textadventures/quest/commit/6a1139461622a304e6bfe9ce29e286fd14ebc56c))
* **Engine:** correct Spanish Scripts tab translations ([#2049](https://github.com/textadventures/quest/issues/2049)) ([98a5355](https://github.com/textadventures/quest/commit/98a5355ba17bc6ee67b853bd8a9cdd26a60cbdc2))
* repair three e2e tests broken by stale locators and a load race ([#2051](https://github.com/textadventures/quest/issues/2051)) ([e244ac9](https://github.com/textadventures/quest/commit/e244ac9fca54241be534e0c65978c75c6381db49))
* **WasmPlayer:** resolve custom Included Library content in Preview/Play ([#2041](https://github.com/textadventures/quest/issues/2041)) ([54ff5b5](https://github.com/textadventures/quest/commit/54ff5b55f07bf263c282ca21075145f82292adee))

## [6.0.0-beta.51](https://github.com/textadventures/quest/compare/v6.0.0-beta.50...v6.0.0-beta.51) (2026-08-11)


### Features

* add German gamebook template and editable language field ([#2032](https://github.com/textadventures/quest/issues/2032)) ([e01ae97](https://github.com/textadventures/quest/commit/e01ae97664c82e8bb563cfb07af1417a80c0a270))
* add i18n gap-detection tooling and language registry ([#2034](https://github.com/textadventures/quest/issues/2034)) ([97b00b3](https://github.com/textadventures/quest/commit/97b00b3407165cf421ac25c307e2a495172e80d3))
* **AppShell:** add complete German translation ([#2030](https://github.com/textadventures/quest/issues/2030)) ([90ebe2c](https://github.com/textadventures/quest/commit/90ebe2cfc6b8cb93b448b15891f570d6c0335037))
* **AppShell:** add i18n infrastructure ([#2024](https://github.com/textadventures/quest/issues/2024)) ([45fe8b1](https://github.com/textadventures/quest/commit/45fe8b15991b2d27e6c6add310f1eb3a0e75fcdb))
* bring Spanish to full 3-layer parity ([#2035](https://github.com/textadventures/quest/issues/2035)) ([d6f0abe](https://github.com/textadventures/quest/commit/d6f0abe676bb5ca6f4f3503a03d0e3cae2203965))
* **ElectronApp:** reveal recent files in Finder, detect external file changes ([#2020](https://github.com/textadventures/quest/issues/2020)) ([921ba45](https://github.com/textadventures/quest/commit/921ba45c27449e6ead56bddb1c065dbde5a7ee4f))


### Bug Fixes

* **AppShell:** hide library files when opening, repair missing gameid on import ([#2022](https://github.com/textadventures/quest/issues/2022)) ([d0012ca](https://github.com/textadventures/quest/commit/d0012ca964f4a07316cdd8610c049807b4f788fc))
* **AppShell:** resolve custom Included Library content on reload ([#2017](https://github.com/textadventures/quest/issues/2017)) ([49fb9ae](https://github.com/textadventures/quest/commit/49fb9aeea9fc7ae6b04ab70c93d68db34bb3385b))
* **AppShell:** resolve Included Library content for Electron and FSA folders ([#2021](https://github.com/textadventures/quest/issues/2021)) ([865ce1a](https://github.com/textadventures/quest/commit/865ce1a47fff68b03aa08e22030fd94e56c95119))
* **AppShell:** show a reload banner when an Included Library changes ([#2019](https://github.com/textadventures/quest/issues/2019)) ([7d5ec65](https://github.com/textadventures/quest/commit/7d5ec6544436984238a202f32c09023de23b21af))
* **AppShell:** Spanish gamebook template, locale capitalization, picker labels ([#2036](https://github.com/textadventures/quest/issues/2036)) ([a3b83d2](https://github.com/textadventures/quest/commit/a3b83d24171ea4c65e7b8bd778d9b3e477c7fb2b))
* **AppShell:** translate remaining strings and fix Create-tab button overflow ([#2031](https://github.com/textadventures/quest/issues/2031)) ([aa620d4](https://github.com/textadventures/quest/commit/aa620d4bb8ef85f4ec98737836de53f403d825f5))
* **Engine:** don't crash on a duplicate key in a simple dictionary attribute ([#2018](https://github.com/textadventures/quest/issues/2018)) ([80c64e9](https://github.com/textadventures/quest/commit/80c64e93af305f5cb64e8c9d46339ab8ad5a6dc8))
* **Engine:** use consistent Du register across German translations ([#2033](https://github.com/textadventures/quest/issues/2033)) ([7851024](https://github.com/textadventures/quest/commit/78510248e658b75329ef7ac8281dd39942f4cd7d))
* flatten native input chrome and tighten tree filter icon spacing ([#2023](https://github.com/textadventures/quest/issues/2023)) ([d371869](https://github.com/textadventures/quest/commit/d37186949e5a9ad2157a824ad1904ad1dde49156))
* **WasmPlayer:** pass --tag latest to npm publish ([#2014](https://github.com/textadventures/quest/issues/2014)) ([a93f483](https://github.com/textadventures/quest/commit/a93f4834e9e33d72e61e6682aac5a2e4997375eb))

## [6.0.0-beta.50](https://github.com/textadventures/quest/compare/v6.0.0-beta.49...v6.0.0-beta.50) (2026-08-09)


### Features

* **AppShell:** add Included Library upload flow, sync asset/element deletes ([#2001](https://github.com/textadventures/quest/issues/2001)) ([9b3d0c0](https://github.com/textadventures/quest/commit/9b3d0c0cf801413cdcf37fe4691b102a7855f975))
* **AppShell:** match Play tab to textadventures.co.uk's catalog/search/gamebook API ([#2003](https://github.com/textadventures/quest/issues/2003)) ([dfdeeb5](https://github.com/textadventures/quest/commit/dfdeeb57674b758b8422523026f4a13361ed69de))
* **AppShell:** show cached cover art for local Play tab recents ([#2005](https://github.com/textadventures/quest/issues/2005)) ([91e1497](https://github.com/textadventures/quest/commit/91e149773c1a1d7cd06a5aece0c274d2ea9253e1))
* **AppShell:** unify Recently Played across catalog and local files ([#2004](https://github.com/textadventures/quest/issues/2004)) ([5e5b2c3](https://github.com/textadventures/quest/commit/5e5b2c3cd8ef64fa3e2ff5800a3a5bda9e0dd9dc))
* **Engine:** introduce WorldModelVersion 600, restore sync script support ([#2007](https://github.com/textadventures/quest/issues/2007)) ([44669ec](https://github.com/textadventures/quest/commit/44669ece57fc0711876e35a19f1f168aecae66d1))
* **WasmPlayer:** implement single-file, CDN-linked game export ([#2012](https://github.com/textadventures/quest/issues/2012)) ([b9bc2ed](https://github.com/textadventures/quest/commit/b9bc2eda224d596fcf923e06b897a0b8f42736e0))


### Bug Fixes

* fix two unrelated e2e test flakes found in a manual run ([#2013](https://github.com/textadventures/quest/issues/2013)) ([9421df9](https://github.com/textadventures/quest/commit/9421df97fe80de655f2562922a6d455514b01865))
* **PlayerCore:** disable autocapitalize on game command input ([#2008](https://github.com/textadventures/quest/issues/2008)) ([8399691](https://github.com/textadventures/quest/commit/839969112f2b8bc032b0983ac27688e47852a883))
* **PlayerCore:** fix output auto-scroll stutter and picture-frame occlusion ([#2009](https://github.com/textadventures/quest/issues/2009)) ([33ce1a5](https://github.com/textadventures/quest/commit/33ce1a5ddca02ec1aab93dd1d06cd82e43756e8b))

## [6.0.0-beta.49](https://github.com/textadventures/quest/compare/v6.0.0-beta.48...v6.0.0-beta.49) (2026-08-07)


### Features

* **AppShell:** add Call function to the Script Adder ([#1991](https://github.com/textadventures/quest/issues/1991)) ([03ebd7d](https://github.com/textadventures/quest/commit/03ebd7d38f3623c0ba8ec4c89c2dd7b54599ff91))
* **AppShell:** improve Call function picker UX ([#1992](https://github.com/textadventures/quest/issues/1992)) ([dc98eef](https://github.com/textadventures/quest/commit/dc98eef0bf91a9cdcaff7c2128806258dd0cb154))


### Bug Fixes

* **AppShell:** correct console banner text and version on play.questviva.com ([#1995](https://github.com/textadventures/quest/issues/1995)) ([15afa91](https://github.com/textadventures/quest/commit/15afa91d8177f97ccac4e83b165b632739622fb8))
* **AppShell:** fix editor bugs and rework the JavaScript element add flow ([#1996](https://github.com/textadventures/quest/issues/1996)) ([5069164](https://github.com/textadventures/quest/commit/50691648f138dac52d4a2dd80247dc56f41732a8))
* **AppShell:** protect built-in libraries from deletion and improve load-error handling ([#1997](https://github.com/textadventures/quest/issues/1997)) ([4f282d2](https://github.com/textadventures/quest/commit/4f282d232d4e832d02207e9f9e96857006a0a785))
* **Engine:** defer FinishTurn across a wait for pre-v580 games ([#1994](https://github.com/textadventures/quest/issues/1994)) ([b24dcbd](https://github.com/textadventures/quest/commit/b24dcbdce77e20853518ff39886fcc6bf69012f5))
* **Engine:** gate changed&lt;attr&gt; dispatch on an actual value change ([#1993](https://github.com/textadventures/quest/issues/1993)) ([d21c316](https://github.com/textadventures/quest/commit/d21c316b94d05eba9699ca813779db07a50bead3))
* fix CI-only flakes found running the newly-wired e2e suite ([#1978](https://github.com/textadventures/quest/issues/1978)) ([1c3b9ae](https://github.com/textadventures/quest/commit/1c3b9ae785209dd78c835e603fd09bfde994b685))
* on-ready queue draining + WasmPlayer JS-call flush timing for recursive get-input loops ([#1980](https://github.com/textadventures/quest/issues/1980)) ([13a353a](https://github.com/textadventures/quest/commit/13a353ac4bad55cfd6cb6888012ce43c4fb909b2))

## [6.0.0-beta.48](https://github.com/textadventures/quest/compare/v6.0.0-beta.47...v6.0.0-beta.48) (2026-08-01)


### Features

* **AppShell:** add object/function insert helper for expression fields ([#1973](https://github.com/textadventures/quest/issues/1973)) ([4c10bfc](https://github.com/textadventures/quest/commit/4c10bfc889c12ff786ae86dd6df93f9eb93ba1fa))


### Bug Fixes

* **AppShell:** fix web fonts, verb pattern default, and turn scripts list ([#1972](https://github.com/textadventures/quest/issues/1972)) ([7e10c9e](https://github.com/textadventures/quest/commit/7e10c9e25600f80bc0c22f941870ba084db1bf99))
* **AppShell:** make textadventures.co.uk editor open-only, no create ([#1971](https://github.com/textadventures/quest/issues/1971)) ([221754f](https://github.com/textadventures/quest/commit/221754f7d12c64b95fb008e9148efb7fca0e5c92))
* **AppShell:** suppress stale svelte-check warning, fail CI on future ones ([#1970](https://github.com/textadventures/quest/issues/1970)) ([3c6edf9](https://github.com/textadventures/quest/commit/3c6edf9cff9ea94409f9fa767f1a2d544dda2050))
* **Engine:** request the next timer tick only after Tick() fully completes ([#1976](https://github.com/textadventures/quest/issues/1976)) ([ede13cb](https://github.com/textadventures/quest/commit/ede13cb2f264cde9fb524bd9a8ca14e1c9e135eb))
* fix editor keyboard shortcuts and unsaved-close dialog freeze ([#1968](https://github.com/textadventures/quest/issues/1968)) ([bd15a74](https://github.com/textadventures/quest/commit/bd15a7451c7feb3913ef7a426cb2f6642cf4cd50))
* **WasmPlayer:** batch JS.* script calls instead of painting per call ([#1975](https://github.com/textadventures/quest/issues/1975)) ([2f49f03](https://github.com/textadventures/quest/commit/2f49f03e0eac4ba859ada336516e5dca84948bb2))
* wire all tests/e2e/*.mjs scripts into e2e.yml, fix drift found along the way ([#1977](https://github.com/textadventures/quest/issues/1977)) ([df177e0](https://github.com/textadventures/quest/commit/df177e0c81b8689476579a7fdcd26110d494b93f))

## [6.0.0-beta.47](https://github.com/textadventures/quest/compare/v6.0.0-beta.46...v6.0.0-beta.47) (2026-07-30)


### Features

* add walkthrough recording and playback to the editor ([#1966](https://github.com/textadventures/quest/issues/1966)) ([6ce82e7](https://github.com/textadventures/quest/commit/6ce82e7ffc0636dbe9b4b7b57feba445ebb59052))
* **AppShell:** add Back/Forward tree navigation history ([#1964](https://github.com/textadventures/quest/issues/1964)) ([934fd8d](https://github.com/textadventures/quest/commit/934fd8d2cbfcc0e0e652296cdc4900587b8bfb31))
* **AppShell:** add Code View with syntax highlighting for scripts and raw XML ([#1958](https://github.com/textadventures/quest/issues/1958)) ([ab0acc6](https://github.com/textadventures/quest/commit/ab0acc6036c658d218fe5b93dffe8a0bccf0d60d))
* **AppShell:** add Show Library Elements toggle to editor tree ([#1961](https://github.com/textadventures/quest/issues/1961)) ([b77467f](https://github.com/textadventures/quest/commit/b77467fc69b120147eb45be37c55eb5969c28632))
* **AppShell:** redesign rich text toolbar as icons + grouped Insert menu ([#1967](https://github.com/textadventures/quest/issues/1967)) ([ae33774](https://github.com/textadventures/quest/commit/ae33774be800ddba737ee469904ae4619f82319c))
* **WasmPlayer:** responsive, non-modal Debugger dialog that stays in sync while playing ([#1965](https://github.com/textadventures/quest/issues/1965)) ([c89f663](https://github.com/textadventures/quest/commit/c89f663b3b95aba9cc97fb3505b28aec0240ad01))


### Bug Fixes

* **AppShell:** correct Inventory take/drop editor and Attributes editor gaps ([#1960](https://github.com/textadventures/quest/issues/1960)) ([24a1eab](https://github.com/textadventures/quest/commit/24a1eab9af48c3161edf7f32242f1bfb8638b28b))
* **AppShell:** replace invalid text-surface-400-500/500-400 utilities ([#1962](https://github.com/textadventures/quest/issues/1962)) ([7b40e3e](https://github.com/textadventures/quest/commit/7b40e3e2efe4bf998d0db3a89b37f2bd303f82ea))
* **AppShell:** wire up the gamebook page Options link editor ([#1957](https://github.com/textadventures/quest/issues/1957)) ([6c97f4a](https://github.com/textadventures/quest/commit/6c97f4abc94fce8281e8aacd76275a8132804359))
* **Engine:** shorten Status Attributes editor prompts to fit the UI ([#1963](https://github.com/textadventures/quest/issues/1963)) ([62c3f82](https://github.com/textadventures/quest/commit/62c3f821fecb75aa74cb05401787aacce7024d43))

## [6.0.0-beta.46](https://github.com/textadventures/quest/compare/v6.0.0-beta.45...v6.0.0-beta.46) (2026-07-28)


### Features

* **AppShell:** add move/cut/copy/paste for tree elements ([#1955](https://github.com/textadventures/quest/issues/1955)) ([51a8ebf](https://github.com/textadventures/quest/commit/51a8ebf594671eb280b0296ed5f5079ec2348121))
* **ElectronApp:** clean up native Edit/View menus, reserve DevTools for Player windows ([#1944](https://github.com/textadventures/quest/issues/1944)) ([fa7378e](https://github.com/textadventures/quest/commit/fa7378e6f31ef26057f4a7cf6e729de26d5c6bf0))


### Bug Fixes

* **AppShell:** add spacing above Advanced section in property editor ([#1946](https://github.com/textadventures/quest/issues/1946)) ([af5474d](https://github.com/textadventures/quest/commit/af5474d3f154fdf3c3deeaa71c179deea961be2e))
* **AppShell:** fix invisible text in modals in dark mode ([#1953](https://github.com/textadventures/quest/issues/1953)) ([3b1f296](https://github.com/textadventures/quest/commit/3b1f2964463d129ddba6002454979a892343258e))
* **AppShell:** stop file-upload picker overflowing on mobile ([#1942](https://github.com/textadventures/quest/issues/1942)) ([197f665](https://github.com/textadventures/quest/commit/197f665db29415b65bcc53a69132b26593f4b044))
* **PlayerCore:** cap map panel width to game width when panes are hidden ([#1954](https://github.com/textadventures/quest/issues/1954)) ([14551f8](https://github.com/textadventures/quest/commit/14551f884b5a18e9931a9bd67719a8469a6a62a3))
* **PlayerCore:** stop ShowMenu dialog throwing when no option is manually clicked ([#1951](https://github.com/textadventures/quest/issues/1951)) ([5f523fe](https://github.com/textadventures/quest/commit/5f523fe1355e3935b101908d444e7b772c8eec1c))
* **WasmPlayer:** default Save to disabled, only enable once a turn genuinely finishes ([#1950](https://github.com/textadventures/quest/issues/1950)) ([f81fe8f](https://github.com/textadventures/quest/commit/f81fe8f302079bba080c57ae7085c98097ba7df3))
* **WasmPlayer:** disable Save while a wait()/get input() prompt is pending ([#1945](https://github.com/textadventures/quest/issues/1945)) ([ddeea26](https://github.com/textadventures/quest/commit/ddeea260307cb7441081cdbf57167261c38490b8))

## [6.0.0-beta.45](https://github.com/textadventures/quest/compare/v6.0.0-beta.44...v6.0.0-beta.45) (2026-07-26)


### Features

* **AppShell:** demote advanced categories in the script adder ([#1936](https://github.com/textadventures/quest/issues/1936)) ([98bad1a](https://github.com/textadventures/quest/commit/98bad1aa561151b0554d0b6ee74618005a209ad7))
* **AppShell:** fold advanced controls into a collapsed expander ([#1934](https://github.com/textadventures/quest/issues/1934)) ([dd42925](https://github.com/textadventures/quest/commit/dd42925e3130712450852dee8471177885fada38))
* **AppShell:** hide empty advanced tree categories ([#1935](https://github.com/textadventures/quest/issues/1935)) ([63212d1](https://github.com/textadventures/quest/commit/63212d192027945bb3cd91b0436fbe34778c4ce3))
* **AppShell:** move rare element adders into the Advanced tree node ([#1939](https://github.com/textadventures/quest/issues/1939)) ([abe98bd](https://github.com/textadventures/quest/commit/abe98bdfba2cb9cd27fe3bfe31fc5453d3bade8e))
* **AppShell:** responsive layout for phone/tablet screens ([#1932](https://github.com/textadventures/quest/issues/1932)) ([fab44dd](https://github.com/textadventures/quest/commit/fab44ddc86c4c9eed9e1eec90438ee3ce3da3e9f))
* **AppShell:** support Gamebook mode in the editor ([#1940](https://github.com/textadventures/quest/issues/1940)) ([7f0d9b7](https://github.com/textadventures/quest/commit/7f0d9b7bed21ed53010efe52ba906d748c51bad3))
* **Engine:** revise advanced-flag metadata after progressive disclosure audit ([#1937](https://github.com/textadventures/quest/issues/1937)) ([7a66dae](https://github.com/textadventures/quest/commit/7a66daed1f4ea49fda3e2a25f739260c67681e13))


### Bug Fixes

* **AppShell:** reset script command scroll on category switch ([#1938](https://github.com/textadventures/quest/issues/1938)) ([8670bbb](https://github.com/textadventures/quest/commit/8670bbb810743a8a37c79f5e1ba08ce624ae9670))
* **AppShell:** update stale empty-advanced-categories e2e test ([#1941](https://github.com/textadventures/quest/issues/1941)) ([faf24ad](https://github.com/textadventures/quest/commit/faf24adaec6c048478393317b35eed603a3d6c63))

## [6.0.0-beta.44](https://github.com/textadventures/quest/compare/v6.0.0-beta.43...v6.0.0-beta.44) (2026-07-23)


### Features

* **AppShell:** add filter textbox and keyboard nav to the script command adder ([#1930](https://github.com/textadventures/quest/issues/1930)) ([4bc97b2](https://github.com/textadventures/quest/commit/4bc97b2ae7e3e461e1bb04aaa6f7c4ea8dcd61a8))
* **AppShell:** add filter textbox to the elements tree ([#1929](https://github.com/textadventures/quest/issues/1929)) ([ffc9b51](https://github.com/textadventures/quest/commit/ffc9b51c91cf3427d422163ba394c148cc9b1ade))
* **AppShell:** add object Verbs editor tab ([#1922](https://github.com/textadventures/quest/issues/1922)) ([9544266](https://github.com/textadventures/quest/commit/9544266c0255e67bf9bf82a718383a19bd6acd02))
* **AppShell:** add room Exits editor tab ([#1920](https://github.com/textadventures/quest/issues/1920)) ([5eb09b5](https://github.com/textadventures/quest/commit/5eb09b53806098c10aebab182179bf22b670d450))
* **WasmPlayer:** add a resizable Debugger with attribute override ([#1919](https://github.com/textadventures/quest/issues/1919)) ([3729201](https://github.com/textadventures/quest/commit/3729201901040b41d34e452df45e82ce37347273))


### Bug Fixes

* **AppShell:** hide desktop download link on mobile, fix local editor link ([#1917](https://github.com/textadventures/quest/issues/1917)) ([1146b4f](https://github.com/textadventures/quest/commit/1146b4f88133d8c32aa7c665a845ee22bf3ede2b))
* **AppShell:** list named exits, not objects, in exit-targeted script pickers ([#1927](https://github.com/textadventures/quest/issues/1927)) ([a027890](https://github.com/textadventures/quest/commit/a027890c6478d6ade338a1e08d774db40b69d7f5))
* **AppShell:** surface clearer validation errors when renaming an object ([#1926](https://github.com/textadventures/quest/issues/1926)) ([61aa37f](https://github.com/textadventures/quest/commit/61aa37f029bc5961e9fed70824d63ea1953d2d31))
* bigger Electron preview window, resizable editor tree pane ([#1928](https://github.com/textadventures/quest/issues/1928)) ([c0233c1](https://github.com/textadventures/quest/commit/c0233c1bba661b99d53f221aed6eb96764b5de50))
* **Engine:** always allow creating look-only exits ([#1923](https://github.com/textadventures/quest/issues/1923)) ([f47cddf](https://github.com/textadventures/quest/commit/f47cddfa34703c4b53e78e5d35ecd39f9a5509ad))
* **Engine:** remove "Hide the Save button" game option ([#1925](https://github.com/textadventures/quest/issues/1925)) ([d111239](https://github.com/textadventures/quest/commit/d1112390c2d6593b14b9cdc21e0f2068e3e410ef))
* **Engine:** remove dead "write log to file" feature ([#1924](https://github.com/textadventures/quest/issues/1924)) ([00f4aa8](https://github.com/textadventures/quest/commit/00f4aa8474278eba58c240daf74e8d6cb0b0a001))
* resolve e2e failures across WasmPlayer restart, AppShell autosave, and Electron ([#1931](https://github.com/textadventures/quest/issues/1931)) ([e61d919](https://github.com/textadventures/quest/commit/e61d91989b619c0ae129857814e5ee63a90780e8))

## [6.0.0-beta.43](https://github.com/textadventures/quest/compare/v6.0.0-beta.42...v6.0.0-beta.43) (2026-07-21)


### Features

* add smart desktop-app download links to play.questviva.com and questviva.com ([#1911](https://github.com/textadventures/quest/issues/1911)) ([1bf3746](https://github.com/textadventures/quest/commit/1bf3746b4a2766f979d22671b43ac47c417c7c4f))
* **AppShell:** show a dismissible update banner for the desktop app ([#1904](https://github.com/textadventures/quest/issues/1904)) ([f2ec8fa](https://github.com/textadventures/quest/commit/f2ec8fa37080ee0d800a19f230afff2142a970ca))
* **AppShell:** warn that the editor is in preview on the Create tab ([#1914](https://github.com/textadventures/quest/issues/1914)) ([e48f2ca](https://github.com/textadventures/quest/commit/e48f2ca7510af24f630f1f9af82516713356faf9))
* **ElectronApp:** register file associations for .aslx/.quest/.asl/.cas ([#1907](https://github.com/textadventures/quest/issues/1907)) ([a154c0d](https://github.com/textadventures/quest/commit/a154c0d21b4b6c56a2cce364e892a099628f7908))
* **ElectronApp:** sign and notarize macOS builds in CI ([#1905](https://github.com/textadventures/quest/issues/1905)) ([54b963a](https://github.com/textadventures/quest/commit/54b963a5055e5cd6c710e88c44ecd9f9f388b400))


### Bug Fixes

* **AppShell:** editor toolbar tweaks (Home routing, Discord/GitHub links, autocapitalize) ([#1910](https://github.com/textadventures/quest/issues/1910)) ([46889b6](https://github.com/textadventures/quest/commit/46889b64e9455af1d60a2da0f9abb387d7f7eb3e))
* **ElectronApp:** fix Linux taskbar icon, add .deb alongside AppImage ([#1900](https://github.com/textadventures/quest/issues/1900)) ([15238a3](https://github.com/textadventures/quest/commit/15238a30400726673e05e085dc1edc0afbd46f9f))
* **Engine:** preserve insertion order in OrderedDictionary after remove+add ([#1906](https://github.com/textadventures/quest/issues/1906)) ([d4f3941](https://github.com/textadventures/quest/commit/d4f394100863f5f6d8ebb2ce144f11a6ed9062e9))
* send client info (source/version/platform) on game-load requests ([#1903](https://github.com/textadventures/quest/issues/1903)) ([907c8bd](https://github.com/textadventures/quest/commit/907c8bd07e4392e7dc6072d90525a0743e58567d))
* **WasmPlayer:** stop shipping debug symbols that fetch from raw.githubusercontent.com ([#1902](https://github.com/textadventures/quest/issues/1902)) ([f132bef](https://github.com/textadventures/quest/commit/f132bef3e3fdcc1a3d25840580bdcb484bb498eb))

## [6.0.0-beta.42](https://github.com/textadventures/quest/compare/v6.0.0-beta.41...v6.0.0-beta.42) (2026-07-19)


### Features

* **AppShell:** add a Home button to the editor toolbar ([#1894](https://github.com/textadventures/quest/issues/1894)) ([9660dee](https://github.com/textadventures/quest/commit/9660dee6ccc63f9ca6104f8ea87ac034999e7d29))
* **AppShell:** open a local game file from the Play tab ([#1885](https://github.com/textadventures/quest/issues/1885)) ([967864c](https://github.com/textadventures/quest/commit/967864cdb0a8c3ef947970def8daee20c984ac54))
* **AppShell:** replace emoji icons with Lucide and tidy the editor toolbar ([#1895](https://github.com/textadventures/quest/issues/1895)) ([794f20c](https://github.com/textadventures/quest/commit/794f20cdc726b8b0ff9f66840c777f1c09af5be3))
* **ElectronApp:** one-click local play, unrestricted autoplay, sibling resources ([#1886](https://github.com/textadventures/quest/issues/1886)) ([d94e7bb](https://github.com/textadventures/quest/commit/d94e7bb086e5949724e76b8b89bf12536def5e09))


### Bug Fixes

* **AppShell:** keep Delete button visible but disabled instead of hiding it ([#1899](https://github.com/textadventures/quest/issues/1899)) ([a4fcb29](https://github.com/textadventures/quest/commit/a4fcb29d93b143cea01c45e71fc1c43f26a19fb9))
* **AppShell:** preserve language selection when toggling game type on Create tab ([#1897](https://github.com/textadventures/quest/issues/1897)) ([88e64fc](https://github.com/textadventures/quest/commit/88e64fc7c3e483368d793a8a827bfabac4ece1d1))
* **AppShell:** stabilize the toolbar save-status pill ([#1898](https://github.com/textadventures/quest/issues/1898)) ([0d607c4](https://github.com/textadventures/quest/commit/0d607c4aa1609a7834e8c7fa73bf06bce2448fd1))
* **AppShell:** stop centering the Create tab against the full viewport ([#1893](https://github.com/textadventures/quest/issues/1893)) ([0db7825](https://github.com/textadventures/quest/commit/0db78250d6dbb26922bbe962b4d3aaad00cda158))
* drop WebKit from the e2e OPFS job, Linux WebKit has no OPFS support ([#1889](https://github.com/textadventures/quest/issues/1889)) ([ec582bc](https://github.com/textadventures/quest/commit/ec582bc04be56997742abe0700f885a02418cb6b))
* **ElectronApp:** set BrowserWindow icon on Linux ([#1882](https://github.com/textadventures/quest/issues/1882)) ([7f03a89](https://github.com/textadventures/quest/commit/7f03a89e3018e639ec1ff67433d6691b32d8cacd))
* **ElectronApp:** show a confirm dialog instead of silently blocking window close on unsaved changes ([#1891](https://github.com/textadventures/quest/issues/1891)) ([910f176](https://github.com/textadventures/quest/commit/910f1764c3c88566eff7f4b47acb31afe7d6c83e))
* **ElectronApp:** split editor/play Recent lists, focus window on File menu actions ([#1892](https://github.com/textadventures/quest/issues/1892)) ([9ff1d03](https://github.com/textadventures/quest/commit/9ff1d03e6dcac653cbdc0c1f97378420f0b0b3c0))
* **WasmPlayer:** offer saving for games launched from a local file ([#1896](https://github.com/textadventures/quest/issues/1896)) ([3d5e641](https://github.com/textadventures/quest/commit/3d5e641452f7eb39eadd2b5385ca979560d47298))
* **WasmPlayer:** time out unanswered resource-request handoffs instead of hanging forever ([#1890](https://github.com/textadventures/quest/issues/1890)) ([fad24ab](https://github.com/textadventures/quest/commit/fad24abdbffce91d0218a0de711138bbfb438753))

## [6.0.0-beta.41](https://github.com/textadventures/quest/compare/v6.0.0-beta.40...v6.0.0-beta.41) (2026-07-16)


### Features

* **WebEditor:** add Play/Create home screen ([#1881](https://github.com/textadventures/quest/issues/1881)) ([0a8730d](https://github.com/textadventures/quest/commit/0a8730d22afde5df501643fc30c804d371b9aec2))


### Bug Fixes

* **ElectronApp:** Linux app icon and About panel ([#1880](https://github.com/textadventures/quest/issues/1880)) ([6e35c49](https://github.com/textadventures/quest/commit/6e35c490bfc2026075a483be2b2cf42c16f3cd16))
* **ElectronApp:** skip ad-hoc codesign on non-macOS builds ([#1878](https://github.com/textadventures/quest/issues/1878)) ([e6fd86c](https://github.com/textadventures/quest/commit/e6fd86c458fa5c159148106ac0e8139e1ebc7da9))

## [6.0.0-beta.40](https://github.com/textadventures/quest/compare/v6.0.0-beta.39...v6.0.0-beta.40) (2026-07-15)


### Features

* **ElectronApp:** native menu bar (File/Save/Help) + Linux sandbox fix ([#1868](https://github.com/textadventures/quest/issues/1868)) ([92ce64f](https://github.com/textadventures/quest/commit/92ce64f6cb302cbfcda50d03cb661092739a6cdb))
* **ElectronApp:** new game creates its own folder ([#1873](https://github.com/textadventures/quest/issues/1873)) ([369f098](https://github.com/textadventures/quest/commit/369f098fea55dfca88c919e27ce514336f874191))
* **ElectronApp:** recently opened games list ([#1871](https://github.com/textadventures/quest/issues/1871)) ([2938810](https://github.com/textadventures/quest/commit/29388108cb4ada4dab0435577921a95ae1c3f4f4))
* **WebEditor:** autosave instead of explicit Save button ([#1877](https://github.com/textadventures/quest/issues/1877)) ([39e881b](https://github.com/textadventures/quest/commit/39e881b1af15a826cac3e9903ecf23d7a235786a))
* **WebEditor:** default to OPFS local drafts on all browsers ([#1870](https://github.com/textadventures/quest/issues/1870)) ([d588754](https://github.com/textadventures/quest/commit/d588754703943675474bcaa692beed5cee98fb9f))
* **WebEditor:** Open screen cleanup and backup-reminder banner ([#1876](https://github.com/textadventures/quest/issues/1876)) ([f65c8cf](https://github.com/textadventures/quest/commit/f65c8cf42d6c7d1eb73604c2e761f9d9e8c9d40b))


### Bug Fixes

* **ElectronApp:** arm64 Linux, mac x64 drop, ad-hoc signing ([#1866](https://github.com/textadventures/quest/issues/1866)) ([924fe85](https://github.com/textadventures/quest/commit/924fe856477a6978ef7681e40fba611d7849729d))
* **ElectronApp:** improve app icon legibility at small sizes ([#1874](https://github.com/textadventures/quest/issues/1874)) ([8810042](https://github.com/textadventures/quest/commit/8810042a20ddf89f618f6b21472ce0315e92244e))
* unbound LAUNCH_ARGS under macOS's default bash 3.2 in electron.sh ([#1872](https://github.com/textadventures/quest/issues/1872)) ([8d79377](https://github.com/textadventures/quest/commit/8d79377cf33736d5c1989433a11e514f2906051f))

## [6.0.0-beta.39](https://github.com/textadventures/quest/compare/v6.0.0-beta.38...v6.0.0-beta.39) (2026-07-14)


### Features

* **ElectronApp:** Phase 1 desktop app ([#1865](https://github.com/textadventures/quest/issues/1865)) ([acd6769](https://github.com/textadventures/quest/commit/acd6769c66d36b8bd824601986c9634b9f0f5463))
* **WebEditor:** split local vs server-save UI per deployment domain ([#1862](https://github.com/textadventures/quest/issues/1862)) ([4f5ed69](https://github.com/textadventures/quest/commit/4f5ed69fb42b9da4da4d56f86def8966a6781f9c))


### Bug Fixes

* allow "main" scope in PR title lint ([#1864](https://github.com/textadventures/quest/issues/1864)) ([f127012](https://github.com/textadventures/quest/commit/f127012f96bf8bb66958c8b8ad9a752b970b3f36))

## [6.0.0-beta.38](https://github.com/textadventures/quest/compare/v6.0.0-beta.37...v6.0.0-beta.38) (2026-07-13)


### Bug Fixes

* regenerate WebEditor lockfile and harden CI against lockfile drift ([#1857](https://github.com/textadventures/quest/issues/1857)) ([2b15c8d](https://github.com/textadventures/quest/commit/2b15c8d105e3f9eb21a96a6eee5b7f9de6fd8132))

## [6.0.0-beta.37](https://github.com/textadventures/quest/compare/v6.0.0-beta.36...v6.0.0-beta.37) (2026-07-13)


### Features

* add asset picker UI for WebEditor file controls ([#1853](https://github.com/textadventures/quest/issues/1853)) ([e57a2bf](https://github.com/textadventures/quest/commit/e57a2bf3c064f4afb6626ca99999b244b0b4d87e))
* persist WebEditor local drafts in OPFS for Firefox and Safari ([#1854](https://github.com/textadventures/quest/issues/1854)) ([a0f1c3c](https://github.com/textadventures/quest/commit/a0f1c3ce63ed4cb7fcf8480dbae080fff394eb0e))
* preview server-backed editor games in WasmPlayer instead of WebPlayer ([#1852](https://github.com/textadventures/quest/issues/1852)) ([98192d9](https://github.com/textadventures/quest/commit/98192d974e44b5c74359cc819e9d848d5ffb792f))
* rename WebEditor Export to Backup, implement Publish (.quest packaging) ([#1855](https://github.com/textadventures/quest/issues/1855)) ([a6db9d3](https://github.com/textadventures/quest/commit/a6db9d32abba5f43923c0d851f22a38bfb1e8eaa))


### Bug Fixes

* deploy-play uploads to the release-please-created Release instead of recreating it ([#1850](https://github.com/textadventures/quest/issues/1850)) ([67a6d4b](https://github.com/textadventures/quest/commit/67a6d4bf6f1cda9c19e60c1df450fe0024dd763e))
* keep newest release marked "Latest" despite perpetual-beta prerelease flag ([#1849](https://github.com/textadventures/quest/issues/1849)) ([d322a0a](https://github.com/textadventures/quest/commit/d322a0a654be80fb4b85cb9d06653e62459997b4))

## [6.0.0-beta.36](https://github.com/textadventures/quest/compare/v6.0.0-beta.35...v6.0.0-beta.36) (2026-07-12)


### Bug Fixes

* base .aslx file's own duplicate template names lose to the first, not the last ([#1843](https://github.com/textadventures/quest/issues/1843)) ([4c40f89](https://github.com/textadventures/quest/commit/4c40f89507545ff9572ff9b4fa189fc2043698dc))
* NULL/Null literal fails with "Unknown object or variable" in scripts ([#1845](https://github.com/textadventures/quest/issues/1845)) ([d3192fa](https://github.com/textadventures/quest/commit/d3192fa51a9e64497483a1bddd19400c50088512))
* wedged script session could OOM-crash the shared WebPlayer process ([#1844](https://github.com/textadventures/quest/issues/1844)) ([63e52d0](https://github.com/textadventures/quest/commit/63e52d0ba8b2484a474b6152f297197051fc645e))
