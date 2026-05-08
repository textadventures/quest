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
        files: ["**/*.svelte", "**/*.ts"],
        languageOptions: {
            globals: globals.browser,
            parserOptions: {
                parser: tseslint.parser,
            },
        },
        rules: {
            "svelte/no-navigation-without-resolve": "off",
        },
    },
]);
