# App icon

`icon.icns` (macOS), `icon.ico` (Windows), and `icons/*.png` (Linux) are
generated from `icon-source.svg` in this directory — a "QV" monogram (the same
letterforms as the WebPlayer/site favicon's ">QV" wordmark,
`site/src/assets/quest-viva.svg`) filled onto a solid rounded-square so it
survives being shrunk to dock/taskbar sizes, where the transparent wordmark
used to read as a thin, drab, grey-dominated strip. The chevron is dropped
deliberately — at 16–32px the three-glyph wordmark had no room left for the
letters. The site favicon and inline wordmark are untouched; `icon-source.svg`
only feeds the desktop app icon.

electron-builder *can* derive a Linux icon set from `icon.icns` automatically
when no Linux-specific icon is configured, but that extraction is unreliable
in practice (see e.g. electron-userland/electron-builder#4186,
electron-userland/electron-builder#5532 — AppImages built this way often end
up with no icon at all, falling back to the desktop environment's generic
"unknown executable" icon, a cog on GNOME). `icons/` — a directory of PNGs
named `<size>x<size>.png` — is electron-builder's documented, reliable format
for Linux instead, wired up explicitly via `build.linux.icon` in
`package.json` rather than left to auto-detection. `icons/512x512.png` is also
shipped as an extraResource (`icon.png`) for the in-app About panel's icon on
Linux — see the comment above `aboutIconPath()` in `src/main.ts`.

This directory is electron-builder's default `build resources` location
(`directories.buildResources`, unset here so it uses the "build" default) —
it picks up `icon.icns`/`icon.ico` automatically, no explicit `icon` config
needed for those two, unlike Linux's `icons/`.

To regenerate after a design change (needs `rsvg-convert` — `brew install
librsvg` — plus macOS's `iconutil` and ImageMagick's `magick`):

```bash
SRC=icon-source.svg
mkdir -p icon.iconset ico-src

# macOS iconset (iconutil's required filenames/sizes)
for spec in "16:icon_16x16.png" "32:icon_16x16@2x.png" "32:icon_32x32.png" "64:icon_32x32@2x.png" \
            "128:icon_128x128.png" "256:icon_128x128@2x.png" "256:icon_256x256.png" \
            "512:icon_256x256@2x.png" "512:icon_512x512.png" "1024:icon_512x512@2x.png"; do
  size="${spec%%:*}"; name="${spec##*:}"
  rsvg-convert -w "$size" -h "$size" "$SRC" -o "icon.iconset/$name"
done
iconutil -c icns icon.iconset -o icon.icns

# Windows .ico (multi-resolution)
for size in 16 24 32 48 64 128 256; do
  rsvg-convert -w "$size" -h "$size" "$SRC" -o "ico-src/icon_${size}.png"
done
magick ico-src/icon_16.png ico-src/icon_24.png ico-src/icon_32.png ico-src/icon_48.png \
       ico-src/icon_64.png ico-src/icon_128.png ico-src/icon_256.png icon.ico

rm -rf icon.iconset ico-src

# Linux icon set (electron-builder's <size>x<size>.png convention)
rm -rf icons && mkdir -p icons
for size in 16 24 32 48 64 96 128 256 512 1024; do
  rsvg-convert -w "$size" -h "$size" "$SRC" -o "icons/${size}x${size}.png"
done
```
