// <qv-command-bar>: the prompt and the command input, with history on the up and down keys. While
// the game waits, a Continue button takes the input's place.

import { canSendCommand, sendCommand } from './engine';
import { endWait } from './prompts';
import { state } from './state';

/** input: the command input; continue: the Continue button (a wait); none: neither, briefly. */
export type CommandBarMode = 'input' | 'continue' | 'none';

export class QvCommandBar extends HTMLElement {
    #form!: HTMLFormElement;
    #input!: HTMLInputElement;
    #prompt!: HTMLElement;
    #continue!: HTMLButtonElement;
    #mode: CommandBarMode = 'input';
    #history: string[] = [];
    // Where up/down has got to in #history; #history.length means the line being typed.
    #historyIndex = 0;
    // What was being typed before stepping back through the history, restored on the way down.
    #draft = '';

    connectedCallback(): void {
        if (this.#input) return;
        const form = this.#form = document.createElement('form');
        form.className = 'qv-command-form';
        this.#prompt = document.createElement('span');
        this.#prompt.className = 'qv-command-prompt';
        this.#prompt.setAttribute('aria-hidden', 'true');
        this.#input = document.createElement('input');
        this.#input.type = 'text';
        this.#input.className = 'qv-command-input';
        this.#input.autocomplete = 'off';
        this.#input.spellcheck = false;
        this.#input.setAttribute('autocapitalize', 'off');
        this.#input.setAttribute('enterkeyhint', 'send');
        this.#input.setAttribute('aria-label', 'Command');
        form.append(this.#prompt, this.#input);
        this.#continue = document.createElement('button');
        this.#continue.type = 'button';
        this.#continue.className = 'qv-continue';
        this.#continue.hidden = true;
        this.#continue.addEventListener('click', endWait);
        this.append(form, this.#continue);

        form.addEventListener('submit', (e) => {
            e.preventDefault();
            this.#submit();
        });
        this.#input.addEventListener('keydown', (e) => this.#onKeyDown(e));
        this.updatePlaceholder();
        this.updateContinueLabel();
    }

    setMode(mode: CommandBarMode): void {
        this.#mode = mode;
        this.#form.hidden = mode !== 'input';
        this.#continue.hidden = mode !== 'continue';
        if (mode === 'continue') this.#continue.focus({ preventScroll: true });
    }

    get input(): HTMLInputElement {
        return this.#input;
    }

    /** Focuses whichever control the bar is showing. */
    focusInput(): void {
        if (this.hidden) return;
        const control = this.#mode === 'input' ? this.#input : this.#mode === 'continue' ? this.#continue : null;
        if (control && document.activeElement !== control) control.focus({ preventScroll: true });
    }

    setPrompt(html: string): void {
        this.#prompt.innerHTML = html;
    }

    setStyle(css: string): void {
        this.#input.style.cssText = css;
    }

    updateContinueLabel(): void {
        this.#continue.textContent = textOf(state.interfaceStrings.get('ContinueLabel') ?? '') || 'Continue';
    }

    /** setCommandPlaceholder wins over the game's TypeHereLabel. */
    updatePlaceholder(): void {
        this.#input.placeholder = state.commandPlaceholder
            ?? textOf(state.interfaceStrings.get('TypeHereLabel') ?? '');
    }

    #submit(): void {
        const command = this.#input.value;
        // While a turn is in progress, keep what was typed so it can be sent once it's over.
        if (command.length === 0 || !canSendCommand()) return;
        if (!sendCommand(command)) return;
        if (this.#history.at(-1) !== command) this.#history.push(command);
        this.#historyIndex = this.#history.length;
        this.#draft = '';
        this.#input.value = '';
    }

    #onKeyDown(e: KeyboardEvent): void {
        if (e.key !== 'ArrowUp' && e.key !== 'ArrowDown') return;
        if (e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
        const index = this.#historyIndex + (e.key === 'ArrowUp' ? -1 : 1);
        if (index < 0 || index > this.#history.length) return;
        e.preventDefault();
        if (this.#historyIndex === this.#history.length) this.#draft = this.#input.value;
        this.#historyIndex = index;
        this.#input.value = index === this.#history.length ? this.#draft : this.#history[index];
        const end = this.#input.value.length;
        this.#input.setSelectionRange(end, end);
    }
}

// Interface strings may hold markup (legacy sets them with .html()), but a placeholder is text.
function textOf(html: string): string {
    const template = document.createElement('template');
    template.innerHTML = html;
    return template.content.textContent ?? '';
}
