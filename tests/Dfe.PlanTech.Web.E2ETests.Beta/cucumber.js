module.exports = {
  default: [
    '--require-module', 'ts-node/register',
    '--require', 'features/step_definitions/**/*.ts',
    '--require', 'support/hooks.ts',    
    '--require', 'support/world.ts',
    '--publish-quiet'
  ]
};

