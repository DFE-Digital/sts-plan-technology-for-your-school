# Contentful Tests

Shared Jest test suite for the Node.js tools in the `contentful/` directory. Tests live here rather than inside each individual tool to allow a single `npm test` command at the `contentful/` level to run all of them.

## Test coverage

| Directory | What is tested |
|---|---|
| `tests/content-management/` | Change scripts, `get-client.js`, `delete-entry.js`, `import-content.js` |
| `tests/content-migration/` | Content type and main migration runner |
| `tests/export-processor/` | `DataMapper`, path calculators, user journey, content type models, error logger |
| `tests/webhook-creator/` | `create-contentful-webhook.ts` upsert logic |

## Helpers

Shared test helpers live in `helpers/`:

| File | Purpose |
|---|---|
| `helpers/helpers.js` | `randomRange` — generates a random-length array using `@faker-js/faker` |
| `helpers/content-generator.js` | Generates fake Contentful entry data from a content type schema |
| `helpers/content-generators/` | Per-field-type value generators (array, boolean, link, rich text, symbol, text) |
| `helpers/content-type-cleaner.js` | Strips content type metadata for test assertions |
| `helpers/content-type-helper.js` | Looks up content type definitions |

The `__mocks__/` directory contains a mock change script used by the `content-management` tests.

## Running tests

From the `contentful/` root:

```bash
npm run test
```

## Debugging in VS Code

1. Create `.vscode/launch.json` in the **repository root** (if it doesn't already exist) with the following configuration, adjusting the `args` path to the test file you want to debug:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "node",
      "request": "launch",
      "name": "Debug Jest Test",
      "program": "${workspaceFolder}/contentful/content-management/node_modules/jest/bin/jest.js",
      "args": [
        "tests/tests/export-processor/content-types/section.test.js",
        "--runInBand"
      ],
      "console": "integratedTerminal",
      "internalConsoleOptions": "neverOpen",
      "cwd": "${workspaceFolder}/contentful",
      "runtimeArgs": [
        "--experimental-vm-modules"
      ]
    }
  ]
}
```

2. Add a breakpoint to any `.test.js` file.
3. Open the **Run and Debug** panel in VS Code, select **Debug Jest Test**, and click the green play button.
