import { get } from "svelte/store";
import { locale } from "./i18n";

const UNITS: [number, string][] = [[1e9, "gigabyte"], [1e6, "megabyte"], [1e3, "kilobyte"]];

// Decimal units (1 MB = 1,000,000 bytes), matching how macOS and most sites report file sizes.
export function formatFileSize(bytes: number): string {
    const [scale, unit] = UNITS.find(([threshold]) => bytes >= threshold) ?? [1, "byte"];
    const value = bytes / scale;
    return new Intl.NumberFormat(get(locale), {
        style: "unit",
        unit,
        unitDisplay: "short",
        maximumFractionDigits: value < 10 && scale > 1 ? 1 : 0,
    }).format(value);
}
