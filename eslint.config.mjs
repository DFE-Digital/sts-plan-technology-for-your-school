import globals from "globals";
import pluginJs from "@eslint/js";
import pluginCypress from "eslint-plugin-cypress/flat";
import jest from "eslint-plugin-jest";

const ignores = [
    "**/node_modules/",
    "**/utils/**/*",
    "**/*.min.js",
    "**/*.bundle.js",
    "**/boostrap/**/*",
    "**/wwwroot/**/*",
];

const esConfig = {
    files: ["**/*.js"],
    languageOptions: {
        ecmaVersion: 2022,
        sourceType: "module",
        globals: {
            ...globals.browser,
            ...globals.node,
            ...globals.jest,
        },
    },
};

export default [
    pluginJs.configs.recommended,
    pluginCypress.configs.globals,
    esConfig,
    {
        rules: {
            "no-extra-boolean-cast": "off",
        },
    },
    {
        files: ["**/*.test.js"],
        plugins: {
            jest: jest,
        },
    },
    {
        ignores: [...ignores],
    },
];
