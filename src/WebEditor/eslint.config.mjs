// @ts-check

import eslint from "@eslint/js";
import { defineConfig, globalIgnores } from "eslint/config";
import tseslint from "typescript-eslint";
import sveltePlugin from "eslint-plugin-svelte";
import { includeIgnoreFile } from "@eslint/compat";
import { fileURLToPath } from "node:url";
import globals from "globals";

const gitignorePath = fileURLToPath(new URL(".gitignore", import.meta.url));

export default defineConfig([
    {
        rules: {
            quotes: ["error", "double", { avoidEscape: true }],
            "object-curly-spacing": ["error", "always"],
            "no-extra-semi": "error",
            "semi": ["error", "always"],
            "indent": ["error", 4],
        },
    },
    includeIgnoreFile(gitignorePath, "Imported .gitignore patterns"),
    globalIgnores([".svelte-kit/**", "build/**"]),
    eslint.configs.recommended,
    tseslint.configs.recommended,
    {
        rules: {
            "@typescript-eslint/no-explicit-any": "off",
        },
    },
    ...sveltePlugin.configs.recommended,
    {
        // svelte/indent (below) supersedes the base indent rule inside .svelte
        // files — both firing at once fight over the same lines (e.g. switch/case)
        // and neither's --fix converges.
        files: ["**/*.svelte"],
        rules: {
            indent: "off",
        },
    },
    {
        files: ["**/*.svelte", "**/*.ts"],
        languageOptions: {
            globals: globals.browser,
            parserOptions: {
                parser: tseslint.parser,
            },
        },
        rules: {
            "svelte/no-navigation-without-resolve": "off",
            "svelte/indent": ["error", { indent: 4 }],
        },
    },
]);
