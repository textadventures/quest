<script lang="ts">
    import { checkProblems, isCheckingGame, checkGame, goToProblem } from "#lib/editor-store.js";
    import { showToast } from "#lib/toast.js";
    import { t, tPlural } from "#lib/i18n/index.js";
    import { problemMessage } from "#lib/check-problem-text.js";
    import type { CheckProblem } from "#lib/types.js";
    import X from "@lucide/svelte/icons/x";
    import RefreshCw from "@lucide/svelte/icons/refresh-cw";
    import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
    import CircleCheck from "@lucide/svelte/icons/circle-check";

    // The problems `qv check` finds, docked under the tree and properties so the list stays in
    // view while each one is fixed. onnavigate lets the page show the properties pane on mobile.
    interface Props {
        onclose: () => void;
        onnavigate?: () => void;
    }
    const { onclose, onnavigate }: Props = $props();

    async function open(problem: CheckProblem) {
        if (await goToProblem(problem)) {
            onnavigate?.();
        } else {
            showToast(t("checkPanel.notFound", { element: problem.element ?? "" }), "error");
        }
    }
</script>

<section class="check-panel flex max-h-[35vh] shrink-0 flex-col border-t border-surface-200-800 bg-surface-50-950" aria-labelledby="check-panel-title">
    <div class="flex items-center gap-2 px-3 py-1.5">
        <h2 id="check-panel-title" class="text-xs font-semibold uppercase text-surface-600-400">{t("checkPanel.title")}</h2>
        <p class="check-panel-status min-w-0 flex-1 truncate text-xs text-surface-600-400" role="status">
            {#if $isCheckingGame}
                {t("checkPanel.checking")}
            {:else if $checkProblems && $checkProblems.length > 0}
                {tPlural("checkPanel.summary", $checkProblems.length)}
            {:else if $checkProblems}
                <span class="inline-flex items-center gap-1 text-success-600-400"><CircleCheck size={12} /> {t("checkPanel.none")}</span>
            {/if}
        </p>
        <button type="button" class="check-panel-again btn btn-xs preset-tonal" disabled={$isCheckingGame} onclick={() => void checkGame()}>
            <RefreshCw size={12} /> {t("checkPanel.checkAgain")}
        </button>
        <button type="button" class="check-panel-close inline-flex size-6 items-center justify-center rounded-container text-surface-600-400 hover:bg-surface-200-800 hover:text-primary-500" title={t("common.close")} aria-label={t("common.close")} onclick={onclose}>
            <X size={14} />
        </button>
    </div>
    {#if $checkProblems && $checkProblems.length > 0}
        <ul class="check-panel-list overflow-y-auto pb-1 text-sm">
            {#each $checkProblems as problem, i (i)}
                <li>
                    <button
                        type="button"
                        class="check-problem flex w-full items-start gap-2 px-3 py-1 text-left hover:bg-surface-100-900 disabled:cursor-default disabled:hover:bg-transparent"
                        disabled={!problem.element || $isCheckingGame}
                        onclick={() => void open(problem)}
                    >
                        <TriangleAlert size={14} class="mt-0.5 shrink-0 text-warning-600-400" />
                        <span class="min-w-0">
                            <span class="font-medium">{problem.element ?? t("checkPanel.game")}</span>
                            {#if problem.attribute}<span class="font-mono text-xs text-surface-600-400"> {problem.attribute}</span>{/if}
                            {#if problem.filename}<span class="text-xs text-surface-600-400"> {t("checkPanel.inLibrary", { filename: problem.filename })}</span>{/if}
                            <span class="check-problem-message block break-words text-surface-700-300">{problemMessage(problem)}</span>
                        </span>
                    </button>
                </li>
            {/each}
        </ul>
    {/if}
</section>
