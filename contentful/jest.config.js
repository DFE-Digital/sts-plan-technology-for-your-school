export default {
    transform: {
        "^.+\\.ts$": [
            "ts-jest",
            {
                useESM: true,
            },
        ],
    },
    moduleDirectories: [
        "content-management/node_modules",
        "content-migrations/node_modules",
        "export-processor/node_modules",
        "webhook-creator/node_modules",
        "node_modules",
    ],
    moduleNameMapper: {
        "/changes/test-change.js$":
            "<rootDir>/tests/__mocks__/content-management/test-change.mock.js",
    },
    testEnvironment: "node",
    preset: "ts-jest",
};
