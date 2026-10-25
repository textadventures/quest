// QuestUI.transcript on v2 (#2627, decision 5): the host keeps the transcript (wasm-player.js's
// WebPlayer.transcript, which legacy writes to as well), so these forward to it. v2's output
// copies what it adds into it too (output.ts), while it's recording.

import type { QuestUITranscript } from './contract';
import type { WebPlayerTranscript } from './host';
import { player } from './player';

const host = (): WebPlayerTranscript | undefined => WebPlayer.transcript;

function addLine(html: string): void {
    const output = player()?.output;
    output?.addText(html);
    output?.scrollToEnd();
}

export const transcript: QuestUITranscript = {
    write: (html) => host()?.write(String(html ?? '')),
    enable: (name) => host()?.enable(name == null ? undefined : String(name)),
    disable: () => host()?.disable(),
    kill: () => host()?.kill(),
    revive: () => host()?.revive(),
    setName: (name) => host()?.setName(String(name ?? '')),
    // replaceTranscriptString's replacement: legacy's has never done anything, since Quest 5.8.
    setPrefix: () => {},
    // As on legacy, the date and time go in the output, and so into the transcript.
    stampDateAndTime: () => addLine(`<b>DATE AND TIME:</b> ${new Date().toLocaleString(navigator.language, { timeZoneName: 'short' })}`),
    // "transcript on" asks for a name: the current one, or the game's, is offered in the command
    // bar to edit.
    promptForName: () => {
        const input = player()?.commandBar?.input;
        if (!input) return;
        input.value = host()?.name || document.title;
        input.focus({ preventScroll: true });
    },
    // A link to the transcript viewer, with 610 Core's translated message and link text.
    show: (message, linkText) => {
        const link = document.createElement('a');
        link.href = host()?.viewerUrl ?? 'TranscriptViewer/index.html';
        link.target = '_blank';
        link.textContent = linkText == null || linkText === '' ? 'Your Transcripts' : String(linkText);
        const text = message == null || message === ''
            ? 'Your transcripts are saved to the localStorage in your browser. You can view, download, or delete them here:'
            : String(message);
        addLine(`${text} ${link.outerHTML}<br/>`);
    },
};
