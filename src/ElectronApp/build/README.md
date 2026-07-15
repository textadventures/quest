# App icon

`icon.icns` (macOS) and `icon.ico` (Windows) are generated from `icon-source.svg`
in this directory — a "QV" monogram (the same letterforms as the WebPlayer/site
favicon's ">QV" wordmark, `site/src/assets/quest-viva.svg`) filled onto a solid
rounded-square so it survives being shrunk to dock/taskbar sizes, where the
transparent wordmark used to read as a thin, drab, grey-dominated strip. The
chevron is dropped deliberately — at 16–32px the three-glyph wordmark had no
room left for the letters. The site favicon and inline wordmark are untouched;
`icon-source.svg` only feeds the desktop app icon. Linux's AppImage icon is
auto-derived from `icon.icns` by electron-builder — no separate file needed.

This directory is electron-builder's default `build resources` location
(`directories.buildResources`, unset here so it uses the "build" default) —
it picks up `icon.icns`/`icon.ico` automatically, no explicit `icon` config.

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
```
