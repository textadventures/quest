// How a game looks in the v2 player, worked out from its Appearance attributes, for the
// Appearance tab's preview (AppearancePreview.svelte). This mirrors two places, and has to be kept
// in step with them:
// - Core's choice of which colours reach the UI: HasOwnColours, HasDarkColours,
//   HasLightOnlyColours, InitTextDefaults and AddAccentToken (Engine/Core/CoreOutput.aslx), and
//   InitInterface's other tokens (Core.aslx).
// - v2's own colours and how it draws the tokens (WasmPlayer/ui-v2/styles/v2.css), and which
//   scheme a one-scheme game is drawn in (ui-v2/src/scheme.ts's hasDarkBackground).
import { toHexColour } from "./colour.js";

// v2's own colours, for a game that leaves them to the player (as ThemePicker's)
const PLAYER_LIGHT = { background: "#ffffff", text: "#1a1a1a", accent: "#1f6f78" };
const PLAYER_DARK = { background: "#18181b", text: "#e4e4e7", accent: "#7cc4cc" };

export interface SchemeLook {
    scheme: "light" | "dark";
    background: string;
    text: string;
    link: string;
    accent: string;
    // The margins either side of the page
    margin: string;
    muted: string;
    locationBackground: string;
    locationText: string;
    locationBorder: string | null;
}

// Why a game is shown in one scheme only, whatever the player's choice
export type FixedReason = "lightOnlyColours" | "ownColours" | "ownDarkColours";

export interface AppearanceLook {
    // Whether the game has colours of its own (HasOwnColours), so an empty text, link or page
    // colour is Quest's classic Black, Blue or White rather than the player's own
    ownColours: boolean;
    // One scheme for a game whose colours are the same in light and dark mode, or both
    schemes: SchemeLook[];
    fixedReason: FixedReason | null;
    font: string;
    // The web font to load from Google Fonts, if any
    webFont: string | null;
    fontSizePt: number;
    underlineLinks: boolean;
}

export function computeAppearance(get: (attribute: string) => string | null): AppearanceLook {
    const str = (a: string) => (get(a) ?? "").trim();
    const bool = (a: string) => /^true$/i.test(str(a));
    const isDefault = (colour: string, def: string) => colour === "" || colour.toLowerCase() === def;

    const foreground = str("defaultforeground");
    const linkForeground = str("defaultlinkforeground");
    const background = str("defaultbackground");

    const lightOnly = bool("colourblend") || str("marginscolour") !== "" || str("customlocationcolour") !== ""
        || str("customlocationtextcolour") !== "";
    const classic = bool("classicinterface");
    const ownColours = !(isDefault(foreground, "black") && isDefault(linkForeground, "blue") && isDefault(background, "white"))
        || bool("setbackgroundopacity") || (!classic && lightOnly);
    const dark = !classic && !lightOnly && str("darkbackground") !== "";

    let accent = str("accentcolour");
    if (accent === "" && ownColours) accent = linkForeground || "Blue";
    const darkAccent = dark ? (str("darkaccentcolour") || str("darklinkforeground")) : "";

    const look = (scheme: "light" | "dark", bg: string, text: string, link: string | null, accentShown: string): SchemeLook => ({
        scheme,
        background: bg,
        text,
        link: link ?? accentShown,
        accent: accentShown,
        margin: str("marginscolour") || `color-mix(in srgb, ${text} 5%, ${bg})`,
        // With a background of the game's own, light or dark, v2's muted text is its text colour
        // (player.ts's data-qv-own-background).
        muted: ownColours || dark ? text : `color-mix(in srgb, ${text} 68%, ${bg})`,
        locationBackground: str("customlocationcolour") || bg,
        locationText: str("customlocationtextcolour") || text,
        locationBorder: str("customlocationbordercolour") || null,
    });

    const light = ownColours
        ? look("light", background || "White", foreground || "Black", linkForeground || "Blue", accent || PLAYER_LIGHT.accent)
        : look("light", PLAYER_LIGHT.background, PLAYER_LIGHT.text, null, accent || PLAYER_LIGHT.accent);

    // An accent for both schemes was chosen for a light page, so the dark scheme lightens it,
    // unless the game's colours are for one scheme only (then it's drawn as given).
    const darkAccentShown = darkAccent || (accent ? `color-mix(in oklab, ${accent} 45%, white)` : PLAYER_DARK.accent);

    let schemes: SchemeLook[];
    let fixedReason: FixedReason | null = null;
    if (dark) {
        schemes = [light, look("dark", str("darkbackground"), str("darkforeground") || PLAYER_DARK.text,
            str("darklinkforeground") || null, darkAccentShown)];
    } else if (!ownColours) {
        schemes = [light, look("dark", PLAYER_DARK.background, PLAYER_DARK.text, null, darkAccentShown)];
    } else {
        // One scheme: drawn as dark if its background is, with its colours as given.
        const only = { ...light, scheme: isDarkColour(light.background) ? "dark" as const : "light" as const };
        schemes = [only];
        fixedReason = lightOnly ? "lightOnlyColours" : only.scheme === "dark" ? "ownDarkColours" : "ownColours";
    }

    const baseFont = str("defaultfont") || "Georgia, serif";
    const webFont = str("defaultwebfont") || null;
    const size = Number(str("defaultfontsize"));
    return {
        ownColours,
        schemes,
        fixedReason,
        font: webFont ? `'${webFont}', ${baseFont}` : baseFont,
        webFont,
        fontSizePt: Number.isFinite(size) && size > 0 ? size : 12,
        underlineLinks: get("underlinehyperlinks") === null || bool("underlinehyperlinks"),
    };
}

// What an empty text, link or page colour is once the game has colours of its own
// (InitTextDefaults's fallbacks)
export const CLASSIC_COLOURS: Record<string, string> = {
    defaultforeground: "Black",
    defaultlinkforeground: "Blue",
    defaultbackground: "White",
};

// As scheme.ts's hasDarkBackground: a relative luminance under 0.18
function isDarkColour(colour: string): boolean {
    const hex = toHexColour(colour);
    if (!hex) return false;
    const [r, g, b] = [1, 3, 5].map(i => {
        const v = parseInt(hex.slice(i, i + 2), 16) / 255;
        return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
    });
    return 0.2126 * r + 0.7152 * g + 0.0722 * b < 0.18;
}
