<script lang="ts">
    import { assets, uploadAsset, resolveAssetUrl, parseAssetSource } from "$lib/editor-store";
    import Combobox from "./Combobox.svelte";

    let { value, source = null, onchange, class: className = "input text-xs py-0.5 px-1.5 w-auto min-w-24" }: {
        value: string;
        source?: string | null;
        onchange: (value: string) => void;
        class?: string;
    } = $props();

    let filter = $derived(parseAssetSource(source));
    let options = $derived(
        $assets.filter(a => filter.matches(a.key)).map(a => ({ value: a.key, label: a.key }))
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

    let inputEl: HTMLInputElement;
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
</script>

<div class="flex items-center gap-1.5">
    {#if filter.kind === "image" && value}
        {#if thumbUrl}
            <img src={thumbUrl} alt="" class="h-6 w-6 object-cover rounded border border-surface-200-800 shrink-0" />
        {:else}
            <span class="h-6 w-6 rounded border border-surface-200-800 shrink-0"></span>
        {/if}
    {:else if value}
        <span class="text-xs shrink-0" title={value}>📄</span>
    {/if}
    <Combobox {value} {options} {onchange} class={className} />
    <button
        type="button"
        class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 whitespace-nowrap shrink-0"
        onclick={() => inputEl.click()}
        disabled={uploading}
        title="Upload a new file"
    >{uploading ? "Uploading…" : "Upload…"}</button>
    <input bind:this={inputEl} type="file" accept={filter.accept} class="hidden" onchange={handleUpload} />
</div>
