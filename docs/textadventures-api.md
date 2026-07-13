# textadventures.co.uk API for WebEditor

This document describes the server-side API that needs to be implemented on textadventures.co.uk to support the new Quest Viva WebEditor in "online mode".

## Overview

The new WebEditor is a static SvelteKit app (deployed on textadventures.co.uk) that runs entirely in the browser. When opened with a `?game={gameId}` URL parameter, it switches to online mode and fetches the game file from the server instead of asking the user to open a local file.

Game IDs are GUIDs that already exist in the system (e.g. `c362e3f9-873a-4448-93e7-decb9722644c`). Game files (`.aslx`) and their assets are stored in Azure Blob Storage.

## Authentication

Since the WebEditor is served from textadventures.co.uk, all API requests carry the user's existing session cookie automatically. No additional token exchange is needed.

All endpoints must:
- Return `401 Unauthorized` if the user is not logged in
- Return `403 Forbidden` if the logged-in user does not own the requested game

## Endpoints

### GET /api/editor/games/{gameId}

Returns the `.aslx` game file content.

**Response:**
- Status: `200 OK`
- `Content-Type: application/xml`
- `Content-Disposition: attachment; filename="My Game.aslx"` (use the actual filename)
- `X-Preview-Url: https://textadventures.co.uk/games/play/{gameId}` (the public WebPlayer URL for this game)
- Body: the raw `.aslx` file bytes

**Errors:** `401`, `403`, `404`

---

### PUT /api/editor/games/{gameId}

Saves updated `.aslx` content back to Azure Blob Storage.

**Request:**
- `Content-Type: application/xml`
- Body: the raw `.aslx` file bytes

**Response:**
- Status: `204 No Content` on success

**Errors:** `401`, `403`, `404`

---

### GET /api/editor/games/{gameId}/assets

Returns a list of asset filenames for the game (images, sounds, etc.).

**Response:**
- Status: `200 OK`
- `Content-Type: application/json`
- Body:
```json
{
  "assets": [
    { "filename": "dragon.png", "url": "/api/editor/games/{gameId}/assets/dragon.png" }
  ]
}
```

---

### POST /api/editor/games/{gameId}/assets

Uploads a new asset file.

**Request:**
- `Content-Type: multipart/form-data`
- Field `file`: the asset file

**Response:**
- Status: `201 Created`
- Body:
```json
{
  "filename": "dragon.png",
  "url": "/api/editor/games/{gameId}/assets/dragon.png"
}
```

**Errors:** `401`, `403`, `404`, `413 Payload Too Large`

---

### GET /api/editor/games/{gameId}/assets/{filename}

Returns the raw bytes of a specific asset file.

**Response:**
- Status: `200 OK`
- `Content-Type`: appropriate MIME type (e.g. `image/png`)
- Body: asset file bytes

**Errors:** `401`, `403`, `404`

---

### DELETE /api/editor/games/{gameId}/assets/{filename}

Deletes an asset file.

**Response:**
- Status: `204 No Content`

**Errors:** `401`, `403`, `404`

---

### POST /api/editor/games

Creates a new server-side game from a name and `.aslx` file. Used when starting a new game while
in online mode (as opposed to opening an existing one via `?game={gameId}`).

**Request:**
- `Content-Type: multipart/form-data`
- Field `name`: the game name
- Field `file`: the `.aslx` content

**Response:**
- Status: `201 Created`
- Body:
```json
{ "gameId": 123 }
```

---

### POST /api/editor/games/{gameId}/publish

Uploads a compiled `.quest` package (built client-side via `Packager.CreatePackage`, see
[webeditor-wasm-svelte.md](./webeditor-wasm-svelte.md#publish)) and stages it for submission to
the site's public-listing flow.

**Request:**
- `Content-Type: application/octet-stream` (or similar)
- Body: the raw `.quest` file bytes

**Response:**
- Status: `204 No Content` on success

**Errors:** `401`, `403`, `404`

The uploaded bytes are held server-side in a short-lived (15-minute) cache keyed by `gameId`, not
written to blob storage — it's a one-shot handoff to the page load that follows. After a
successful upload, the client should navigate to `/create/publish/{gameId}`, which reads the
cached package once, shows the submission form (title/description/category/cover/visibility) for
a first publish, or updates the existing listing if the game was already published.

## Entry point from textadventures.co.uk

When a user clicks "Edit" on one of their games, redirect them to:

```
/editor/?game={gameId}
```

The WebEditor will auto-load the game. If the user is not logged in, the `GET /api/editor/games/{gameId}` call will return `401` and the editor will show an error.

## Notes

- The WebEditor is served as a static site from textadventures.co.uk (e.g. at `/editor/`). All API URLs are relative to the same origin, so no CORS configuration is needed.
- The existing "play" URL scheme (`https://play.textadventures.co.uk/editor/{gameId}/...`) is unchanged — the WebEditor can still send users there to test their games.
- Game IDs are GUIDs, which already correspond to the existing Azure Blob Storage container structure.
