import { MediaQuery } from "svelte/reactivity";

// Single shared reactive source for behavioral (not just cosmetic) breakpoint
// switches — e.g. master-detail pane swapping. Cosmetic changes should use
// Tailwind `md:` / `pointer-coarse:` classes directly instead.
export const isNarrow = new MediaQuery("(max-width: 767px)");
