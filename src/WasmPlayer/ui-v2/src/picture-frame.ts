// The picture frame (the "picture-frame" part): a picture above the output, usually the current
// room's (SetFramePicture), kept in view with the top bar as the transcript scrolls, as legacy's
// was. 610 Core sets it through QuestUI.setPictureFrame, and older games through the engine's
// SetPanelContents request (the adapter's setPanelContents).
//
// A frame image that hasn't loaded yet has no height, so the output holds off scrolling until it
// has (output.ts) and scrolls again once it settles, as legacy's setPanelHeight did.

import { player } from './player';

export function setPictureFrame(html: string): void {
    const frame = player()?.pictureFrame;
    if (!frame) return;
    frame.innerHTML = html;
    frame.classList.toggle('qv-empty', html.trim() === '');
    for (const img of frame.querySelectorAll('img')) {
        // The frame illustrates what the text already says: Core's picture has no alt text, so
        // it's marked decorative rather than read out as a file name.
        if (!img.hasAttribute('alt')) img.alt = '';
        if (!img.complete) {
            img.addEventListener('load', rescroll, { once: true });
            img.addEventListener('error', rescroll, { once: true });
        }
    }
    rescroll();
}

/** Whether the frame is showing an image that hasn't loaded (or failed) yet. */
export function pictureFramePending(): boolean {
    const frame = player()?.pictureFrame;
    if (!frame || frame.hidden || frame.classList.contains('qv-empty')) return false;
    return [...frame.querySelectorAll('img')].some((img) => !img.complete);
}

function rescroll(): void {
    player()?.output?.rescroll();
}
