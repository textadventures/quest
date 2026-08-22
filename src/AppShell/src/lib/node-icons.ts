import type { Component } from "svelte";
import DoorOpen from "@lucide/svelte/icons/door-open";
import Package from "@lucide/svelte/icons/package";
import FileText from "@lucide/svelte/icons/file-text";
import Signpost from "@lucide/svelte/icons/signpost";
import MessageSquare from "@lucide/svelte/icons/message-square";
import Terminal from "@lucide/svelte/icons/terminal";
import RotateCw from "@lucide/svelte/icons/rotate-cw";
import SquareFunction from "@lucide/svelte/icons/square-function";
import Timer from "@lucide/svelte/icons/timer";
import Route from "@lucide/svelte/icons/route";
import Library from "@lucide/svelte/icons/library";
import LayoutTemplate from "@lucide/svelte/icons/layout-template";
import Braces from "@lucide/svelte/icons/braces";
import Shapes from "@lucide/svelte/icons/shapes";
import FileCode from "@lucide/svelte/icons/file-code";
import Gamepad2 from "@lucide/svelte/icons/gamepad-2";
import Puzzle from "@lucide/svelte/icons/puzzle";
import Folder from "@lucide/svelte/icons/folder";
import User from "@lucide/svelte/icons/user";

export type IconComponent = Component<{ size?: number; class?: string }>;

// One icon per leaf nodeType (see EditorController.GetNodeType / WasmEditorBridge.TreeNodeData).
export const NODE_TYPE_ICON: Record<string, IconComponent> = {
    game: Gamepad2,
    room: DoorOpen,
    object: Package,
    page: FileText,
    exit: Signpost,
    verb: MessageSquare,
    command: Terminal,
    turnscript: RotateCw,
    function: SquareFunction,
    timer: Timer,
    walkthrough: Route,
    include: Library,
    template: LayoutTemplate,
    dynamictemplate: Braces,
    type: Shapes,
    javascript: FileCode,
    // Synthetic "grouped by source library file" folder node — see TreePanel's groupLibraryChildren.
    librarygroup: Folder,
};

// Every header node's nodeType is the generic "header" (see WasmEditorBridge), so headers
// are iconified by their fixed key instead, echoing the category they contain.
export const HEADER_ICON: Record<string, IconComponent> = {
    _objects: Package,
    _advanced: Folder,
    _functions: SquareFunction,
    _timers: Timer,
    _walkthrough: Route,
    _include: Library,
    _template: LayoutTemplate,
    _dynamictemplate: Braces,
    _objecttype: Shapes,
    _javascript: FileCode,
    _gameVerbs: MessageSquare,
    _gameCommands: Terminal,
};

// The player object is a plain Object element underneath (nodeType "object" in Text
// Adventure mode, "page" in Gamebook mode — see EditorController.GetNodeType), but it's
// always the single fixed "player" element key, so it gets its own icon regardless of mode.
export function nodeIcon(id: string, nodeType: string): IconComponent {
    if (nodeType === "header") return HEADER_ICON[id] ?? Folder;
    if (id === "player") return User;
    return NODE_TYPE_ICON[nodeType] ?? Puzzle;
}
