# WebEditor

Browser-based game editor for Quest Viva, built with Blazor Server and MudBlazor.

## Background

This is a new replacement for the legacy ASP.NET MVC editor at `legacy/WebEditor/`. That old editor used `EditorController` from EditorCore via a server-side `EditorService` that translated between HTTP request/response and the controller's event-driven model. This new WebEditor takes the same approach — wrapping `EditorController` — but uses Blazor Server, which is a much better fit because its persistent SignalR circuit maps naturally to `EditorController`'s stateful, event-driven architecture. No request/response translation is needed; components simply subscribe to C# events and call `StateHasChanged()`.

Key reference files from the legacy editor:
- `legacy/WebEditor/WebEditor/Services/EditorService.cs` — shows the pattern of wrapping EditorController, wiring events, managing tree state
- `legacy/WebEditor/WebEditor/Views/Edit/` — shows how tabs, controls, and scripts were rendered

## Architecture

```
EditorService (scoped, one per Blazor circuit)
  └── EditorController (from EditorCore)
        └── WorldModel (from Engine)
```

Each Blazor Server circuit gets its own `EditorService` wrapping its own `EditorController`. The service subscribes to EditorController events (tree changes, undo state, element updates) and exposes C# `Action` events that Blazor components use to trigger re-renders.

## Running

```bash
dotnet run --project src/WebEditor
# Opens at https://localhost:5101
```

## What's Implemented (MVP Phase 1)

- **Project scaffolding**: Blazor Server + MudBlazor, added to QuestViva.sln
- **Landing page** (`Index.razor`): "New Game" (template selection dialog) and "Open Game" (file upload)
- **Editor layout** (`Edit.razor`): 3-pane layout with toolbar, element tree, and element editor
- **EditorService**: Wraps EditorController, bridges tree/undo/element events, manages temp files, sets `EditorMode.Web`
- **Element tree** (`ElementTree.razor`): MudTreeView displaying game element hierarchy via recursive `RenderTreeNode` components
- **Element editor** (`ElementEditor.razor`): Reads `IEditorDefinition`/`IEditorData` for selected element, renders MudTabs per visible tab, iterates controls through `EditorControlFactory`
- **Basic controls**: TextBox, CheckBox, DropDown, MultiLineText, Number, Label — unsupported types show a placeholder alert
- **Toolbar**: Save (JS interop file download), Undo/Redo
- **JS interop** (`editor.js`): File input click trigger, file download via Blob URL

## Known Issues / Not Yet Working

- **Element tree may not display items** — the data is confirmed populated via events but MudTreeView rendering needs debugging (tree events fire correctly, `TreeNodes` list has items after `UpdateTree()`)
- Controls use transaction wrapping (`StartTransaction`/`EndTransaction`) for undo/redo but this hasn't been end-to-end tested yet

## What's Not Yet Implemented

- **Script editor** — the biggest gap; script controls (if/then, print message, etc.) show as "not yet supported" placeholders
- **Exit editor controls** — exits/compass directions
- **File/image controls** — for adding images, sounds, etc.
- **Element creation/deletion** — no UI for adding rooms, objects, etc. to the tree
- **Element drag-and-drop** — moving elements between parents
- **Right-click context menus** on tree nodes
- **Copy/paste** of elements and scripts
- **Find/replace**
- **Game publishing** (`.quest` file output)
- **Code view** — raw ASLX editing
- **Expression editor** — for editing expressions with validation
- **Filter options** — show/hide library elements, filter by type
- **Simple mode** toggle
- **Error/validation display** — showing script errors, element validation

## Key EditorCore Integration Points

- `EditorController.UpdateTree()` requires `BeginTreeUpdate` event to have subscribers or it silently returns (this was a bug we hit)
- `EditorController.GetElementEditorName(key)` → `GetEditorDefinition(editorName)` is the two-step lookup for getting the editor definition for an element
- `CreateNewGameFile()` takes a resource name (e.g. `QuestViva.Engine.Core.Templates.English.template`), not the display name
- `IEditorTab.GetBool("desktop")` returns true for tabs that should only show in the desktop editor — we skip these
- `IEditorTab.IsTabVisible(data)` filters tabs based on element state
- `IEditorControl.IsControlVisible(data)` filters controls based on element state
