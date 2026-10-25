// Video embeds in the output: QuestUI.addYouTube and addVimeo (#2627, decision 6).
//
// YouTube comes from youtube-nocookie.com and Vimeo with dnt=1, so neither sets tracking cookies
// for a player who never presses play. Each embed fits the output's width at 16:9 and has a
// translated title. It autoplays when the game adds it, but not when a save is restored: the
// saved output would otherwise start every video in it at once.

import { player } from './player';

const chromeString = (key: string, fallback: string): string => WebPlayer.chromeStrings?.[key] ?? fallback;

function escapeAttribute(text: string): string {
    return text.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
}

function addVideo(kind: 'youtube' | 'vimeo', src: URL, title: string, extra = ''): void {
    const output = player()?.output;
    if (!output) return;
    output.addText(`<iframe class="qv-video" data-qv-video="${kind}" src="${escapeAttribute(src.href)}"`
        + ` title="${escapeAttribute(title)}" allow="autoplay; fullscreen; picture-in-picture; encrypted-media"`
        + ` allowfullscreen${extra}></iframe>`);
}

export function addYouTube(id: string): void {
    const src = new URL(`https://www.youtube-nocookie.com/embed/${encodeURIComponent(id.trim())}`);
    src.searchParams.set('autoplay', '1');
    src.searchParams.set('rel', '0');
    addVideo('youtube', src, chromeString('YouTubeVideoTitle', 'YouTube video'));
}

/** An unlisted video's id carries its hash after a slash ("123456/abcdef"), as its link does. */
export function addVimeo(id: string): void {
    const [video, hash] = id.trim().split('/');
    const src = new URL(`https://player.vimeo.com/video/${encodeURIComponent(video)}`);
    if (hash) src.searchParams.set('h', hash);
    src.searchParams.set('autoplay', '1');
    src.searchParams.set('dnt', '1');
    // Sandboxed as on legacy: Vimeo's player needs no more than this.
    addVideo('vimeo', src, chromeString('VimeoVideoTitle', 'Vimeo video'),
        ' sandbox="allow-same-origin allow-scripts allow-popups allow-presentation"');
}

/** Turns autoplay off in restored output, before it's in the page and its embeds start loading. */
export function stopRestoredVideos(scope: DocumentFragment): void {
    for (const iframe of scope.querySelectorAll<HTMLIFrameElement>('iframe[data-qv-video]')) {
        let src: URL;
        try {
            src = new URL(iframe.getAttribute('src') ?? '');
        } catch {
            continue;
        }
        if (!src.searchParams.has('autoplay')) continue;
        src.searchParams.delete('autoplay');
        iframe.setAttribute('src', src.href);
    }
}
