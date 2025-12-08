// cucumber.js
module.exports = {
  smoke: [
    '--require-module', 'ts-node/register',
    '--require', 'step_definitions/smoke-tests/**/*.ts',
    '--require', 'support/hooks.ts',
    '--require', 'support/world.ts',
    '--tags', '@smoke',
    '--publish-quiet'
  ],
  regression: [
    '--require-module', 'ts-node/register',
    '--require', 'step_definitions/regression/**/*.ts',
    '--require', 'support/hooks.ts',
    '--require', 'support/world.ts',
    '--tags', 'not @smoke',
    '--publish-quiet'
  ]
};
