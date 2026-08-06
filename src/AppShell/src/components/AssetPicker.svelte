<script lang="ts">
    import { assets, uploadAsset, resolveAssetUrl, parseAssetSource, putAssetText } from "$lib/editor-store";
    import Combobox from "./Combobox.svelte";
    import FileIcon from "@lucide/svelte/icons/file";

    let { value, source = null, creatable = false, readonly = false, exclude = [], onchange, onEnter, class: className = "input text-xs py-0.5 px-1.5 w-full min-w-0", containerClass = "" }: {
        value: string;
        source?: string | null;
        // When true, typing a filename that isn't an existing asset creates a blank one under
        // that name instead of leaving the attribute pointing at nothing — e.g. the Javascript
        // src picker, where "type a new name" is how you make a new file (no separate button).
        creatable?: boolean;
        // Locks the control down to a plain icon + filename, no Combobox/Upload — for controls
        // like the Javascript src file, which can't be repointed once set (see PropertyEditor's
        // "lockedAfterCreate" handling; add a new element to point elsewhere).
        readonly?: boolean;
        // Asset keys to hide from the dropdown — e.g. .js files already pointed to by another
        // Javascript element, which would be pointless to attach a second time.
        exclude?: string[];
        onchange: (value: string) => void;
        onEnter?: () => void;
        class?: string;
        // Applied to the row div alongside the default shrink-safe layout — pass "w-full" when
        // the caller's row has no other flexible sibling and wants this control to fill it
        // (e.g. PropertyEditor), and leave unset for inline/wrapping layouts (e.g. ScriptEditor)
        // where growing to fill would force everything else in the row onto the next line.
        containerClass?: string;
    } = $props();

    let filter = $derived(parseAssetSource(source));
    let options = $derived(
        $assets
            .filter(a => filter.matches(a.key) && !exclude.includes(a.key))
            .map(a => ({ value: a.key, label: a.key }))
    );

    let thumbUrl = $state<string | null>(null);

    $effect(() => {
        const key = value;
        if (!key || filter.kind !== "image") {
            thumbUrl = null;
            return;
        }
        resolveAssetUrl(key).then((url) => {
            if (value === key) thumbUrl = url;
        });
    });

    let inputEl = $state<HTMLInputElement>();
    let uploading = $state(false);

    async function handleUpload(e: Event) {
        const target = e.target as HTMLInputElement;
        const file = target.files?.[0];
        target.value = "";
        if (!file) return;
        uploading = true;
        try {
            await uploadAsset(file);
            onchange(file.name);
        } finally {
            uploading = false;
        }
    }

    // Calls onchange synchronously first, so callers that key off the new value right away (e.g.
    // Combobox's onEnter, fired immediately after this in the same keydown) always see it —
    // the blank-asset creation below is fire-and-forget rather than gating onchange behind it.
    function handleComboboxChange(v: string) {
        onchange(v);
        if (creatable && v && !$assets.some(a => a.key === v)) {
            void putAssetText(v, "");
        }
    }
</script>

<div class="flex items-center gap-1.5 min-w-0 {containerClass}">
    {#if filter.kind === "image" && value}
        {#if thumbUrl}
            <img src={thumbUrl} alt="" class="h-6 w-6 object-cover rounded border border-surface-200-800 shrink-0" />
        {:else}
            <span class="h-6 w-6 rounded border border-surface-200-800 shrink-0"></span>
        {/if}
    {:else if value}
        <span class="shrink-0 opacity-70" title={value}><FileIcon size={14} aria-hidden="true" /></span>
    {/if}
    {#if readonly}
        <span class="text-xs truncate flex-1 min-w-0" title="Can't be changed after creation — add a new element to point elsewhere">{value}</span>
    {:else}
        <Combobox {value} {options} onchange={handleComboboxChange} {onEnter} class={className} wrapperClass="flex-1 min-w-0" />
        <button
            type="button"
            class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 whitespace-nowrap shrink-0"
            onclick={() => inputEl?.click()}
            disabled={uploading}
            title="Upload a new file"
        >{uploading ? "Uploading…" : "Upload…"}</button>
        <input bind:this={inputEl} type="file" accept={filter.accept} class="hidden" onchange={handleUpload} />
    {/if}
</div>
