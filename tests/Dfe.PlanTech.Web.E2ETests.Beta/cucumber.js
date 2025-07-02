module.exports = {
  default: [
    '--require-module', 'ts-node/register',
    '--require', 'features/step_definitions/**/*.ts',
    '--require', 'features/support/hooks.ts',    
    '--require', 'features/support/world.ts',
    '--publish-quiet'
  ]
};

