// @ts-check
import { defineConfig } from 'astro/config';

// https://astro.build/config
export default defineConfig({
    build: {
        format: "file"
    },
    trailingSlash: "never",
    markdown: {
        shikiConfig: {
            theme: 'catppuccin-latte'
        }
    }
});
