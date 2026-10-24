# Headless Session, CLI and Agent Interface

Status: design, not yet started.

## Motivation

Quest Viva games can already be driven headlessly, but the logic for doing it lives in three separate places:

- `PlayerCore/WalkthroughRunner.cs` — runs a game's built-in walkthroughs inside the live player UI.
- `quest-e2e-tests` (private sibling repo) — `HeadlessPlayer` + `TranscriptRunner`, which drive ~75 published games end-to-end and diff transcripts against goldens.
- spatterlight's quest5-oracle harness, which the e2e harness was ported from.

The hard-won parts — capturing both output channels (`WriteHTML`/`PrintText` and v540+ `JS.addText` via `RunScriptAsync`), auto-advancing waits and pauses, treating a synchronous `play sound` as a wait, draining `timeout*` timers, resolving menus by key/label/number — are knowledge about engine semantics, and belong next to the engine so they change when it does.

Pulling this into a supported `HeadlessSession` in PlayerCore, and building a CLI on top of it, enables:

- **AI playtesting** — an author asks an agent to play their game and report where it got stuck.
- **A terminal player** — `quest play mygame.quest`.
- **CI for authors** — run a game's walkthroughs from the command line.
- **Compatibility oracles and alternative front-ends** (e.g. spatterlight) without another copy of the harness.
- **AI agent evaluation** — Quest as an environment for benchmarking LLM agents on parser games (see "Downstream: evaluation benchmark" below).

## Layout

### quest (this repo)

- **`PlayerCore`: `HeadlessSession`** — a general-purpose library API, with nothing agent- or benchmark-specific:
  - `Start(gameData, seed)` / reset
  - `Step(command)` → that turn's plain text, any pending menu/question, finished flag
  - menu and question answers
  - auto-advance of waits/pauses, timer draining, HTML-to-text
  - later: structured world-state snapshot, checkpoint/restore
- **`src/Cli`** — new project, shipped as a `dotnet tool`:
  - `quest play <game>` — interactive terminal play
  - `quest serve` — JSON-lines protocol over stdio, for programmatic drivers
  - `quest mcp` — the same operations exposed as MCP tools
  - possibly `quest walkthrough <game> [name]` — run built-in walkthroughs, for authors' CI

### quest-e2e-tests

Stays private (it fetches third-party games). Keeps the corpus, goldens, its walkthrough syntax (`{{expr}}`, `tick:`, `assert:`, ...) and the browser runner. `HeadlessPlayer` and most of `TranscriptRunner` are replaced by `HeadlessSession`.

### New repo (name TBD)

Evaluation-specific work that isn't Quest functionality:

- world generator + solver
- benchmark task sets
- Python package wrapping `quest serve` for Inspect / Verifiers
- baseline runs and write-up

## Scope decisions

- **v5+ only.** `HeadlessSession` works with `WorldModel`; Quest 4 games via `V4Game` are out of scope.
- **`WalkthroughRunner` stays as it is.** It drives a game attached to the live UI player with a real timer, which is a different shape from a session that owns its own headless `IPlayer`.
- **NuGet API stability is not a concern yet.** The only consumer of the PlayerCore package is textadventures.co.uk, so the API can change as it settles.

## Design questions

1. **Agent-visible vs privileged operations.** An agent should get: send command, answer menu, answer question. State dumps, `assert`, `evaluate`, set attribute, and checkpoint/restore are privileged — otherwise an agent can read the solution out of the world model. In `serve` these are protocol commands the harness doesn't forward to the agent. In `mcp` they are hidden unless enabled with a flag (e.g. `--debug-tools`), so a playtesting agent gets a fair game by default.
2. **Observation content.** Plain text by default. The engine also knows exits, visible objects and inventory (via `UpdateList`) — useful, but exactly the kind of privileged information that inflates agent scores. Make it an opt-in "assisted" mode, reported alongside any results.
3. **Reset and checkpoint semantics.** Reset = reload from game bytes with a seed. Checkpoints via `SaveAsync`. Open: does a save taken while a menu or `get input` is pending restore correctly? If not, only allow checkpoints at clean turn boundaries. Worth an early spike.
4. **JS-dependent games.** The e2e harness no-ops `JS.*` calls, and most corpus games make them. Generated worlds won't care, but the CLI should warn when a game appears to depend on JS for more than presentation.
5. **Distribution.** `dotnet tool` suits .NET users; evaluation users are mostly Python users without a .NET SDK. Consider self-contained single-file binaries attached to each GitHub release, which the Python package can download. Needs a release workflow addition (phase 3).
6. **Naming** for the new repo and Python package.

## Phases

1. **`HeadlessSession` in PlayerCore**, ported from the e2e harness, with unit tests against EngineTests fixtures. Switch quest-e2e-tests to use it. **Done when every golden passes unchanged, with no re-recording.**
2. **CLI**: `play`, then `serve`, then `mcp`. Dogfood by pointing an agent at `examples/` and at a corpus game.
3. **New repo**: Python wrapper, an Inspect task over a few hand-built worlds, first baseline runs.
4. **Generator + solver**: tunable difficulty, solvability proven by search.
5. **Calibration** against existing benchmarks (TALES / Jericho), then the write-up.

Phases 1 and 2 are worthwhile for Quest on their own, regardless of whether the benchmark goes ahead.

## Downstream: evaluation benchmark

Context for phases 3–5, from a survey of existing work (October 2026):

- **TextWorld** (Microsoft) procedurally generates Inform 7 games with tunable size and quest length, but its games are simple and current models mostly beat them.
- **Jericho** / **TextQuests** use hand-written classic games (mostly Infocom): hard, but a fixed set, walkthroughs are widely published (training-data contamination), licensing is murky, and scoring is noisy — Epoch AI found 22.6% of TextQuests' progress checkpoints were wrong.
- **TALES** (Microsoft, NeurIPS 2026) unifies these and finds top agents do well on synthetic games but score under 15–25% on human-written ones.
- MCP servers for Z-machine/Glulx games already exist (`mcp-server-if`, `zmachine-mcp`), so "IF over MCP" is not novel by itself.

The gaps Quest can address:

- **The middle ground.** Generated, solver-verified worlds whose difficulty can be tuned from TextWorld-easy up to Jericho-hard — scalable, contamination-resistant, and harder than existing synthetic games.
- **Exact world-state grading.** Inform/Z-machine environments read state by poking interpreter memory. Quest exposes the world as objects and attributes (`IGameDebug.GetObjects`/`GetDebugData`, `AssertAsync`, `EvaluateAsync`, `Fields.AttributeChanged`), allowing exact success checks, dense per-step progress signals, and reset-to-any-state via save/restore.
- **Packaging.** No rich parser-game environment is packaged for Inspect or Verifiers / Prime Intellect's Environments Hub yet.

Risks: text games are a niche within AI labs' priorities; the TextWorld/TALES team could close the same gap; Quest's parser is less refined than Inform's, which should be measured so the benchmark doesn't accidentally score parser quirks.
