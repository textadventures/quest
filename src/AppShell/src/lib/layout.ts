// Which parts of the v2 player a game shows, and where its panes go, worked out from its Interface
// attributes, for the Interface tab's preview (InterfacePreview.svelte). This mirrors 610 Core's
// InitInterface (Engine/Core/Core.aslx: setVisible, setPaneOrder, setLayoutPreset, setMapPosition)
// and v2's layout presets (docs/ui-v2-contract.md, "Layout presets"), and has to be kept in step
// with them.

export type Preset = "sidepanes" | "bottombar" | "nopanes";

export type PaneId = "map" | "inventory" | "objects" | "status" | "compass" | "commands" | "customstatus";

export interface LayoutLook {
    // The room name in the location bar (the bar itself, with Save/Load, is always there)
    locationName: boolean;
    border: boolean;
    commandBar: boolean;
    pictureFrame: boolean;
    // Where the map is, if the game has one: above the game, or as the first pane
    map: "header" | "panes" | null;
    // The panes shown, in order, the map first when it's one of them
    panes: PaneId[];
    // Where the panes go on a wide screen, and on a phone (sidepanes there is a drawer)
    wide: Preset;
    narrow: Preset;
    // On a phone, the strip of exit buttons under the text, standing in for the compass
    exitsStrip: boolean;
}

const DEFAULT_ORDER: PaneId[] = ["inventory", "objects", "status", "compass", "commands", "customstatus"];
// statusandcompassattop (Core's setPaneOrder): these first, the rest after in their places
const STATUS_FIRST: PaneId[] = ["commands", "customstatus", "status", "compass", "inventory", "objects"];

function preset(value: string): Preset | null {
    return value === "sidepanes" || value === "bottombar" || value === "nopanes" ? value : null;
}

export function computeLayout(get: (attribute: string) => string | null): LayoutLook {
    const str = (a: string) => (get(a) ?? "").trim();
    const bool = (a: string) => /^true$/i.test(str(a));

    const showPanes = bool("showpanes");
    const wide: Preset = showPanes ? preset(str("layoutpreset")) ?? "sidepanes" : "nopanes";
    const narrow: Preset = showPanes ? preset(str("narrowlayoutpreset")) ?? wide : "nopanes";

    // The status pane shows the score, health and money, and any status attributes
    const status = bool("showscore") || bool("showhealth") || bool("showmoney") || str("statusattributes") !== "";
    const shown: Record<PaneId, boolean> = {
        map: false,
        inventory: !bool("turnoffinventory"),
        objects: !bool("turnoffplacesandobjects"),
        status,
        compass: !bool("turnoffcompass"),
        commands: bool("commandpane"),
        customstatus: bool("customstatuspane"),
    };
    let panes = (bool("statusandcompassattop") ? STATUS_FIRST : DEFAULT_ORDER).filter(p => shown[p]);

    // The map goes in the panes only while there are panes to put it in, else above the game
    let map: LayoutLook["map"] = null;
    if (bool("gridmap")) {
        map = str("mapposition") === "panes" && wide !== "nopanes" ? "panes" : "header";
        if (map === "panes") panes = ["map", ...panes];
    }

    return {
        locationName: bool("showlocation"),
        border: bool("showborder"),
        commandBar: bool("showcommandbar"),
        pictureFrame: bool("feature_pictureframe"),
        map,
        panes,
        wide,
        narrow,
        exitsStrip: narrow !== "nopanes" && shown.compass,
    };
}
