import * as os from 'node:os';
import { generate } from 'multiple-cucumber-html-reporter';

const isCI = Boolean(process.env.CI);

generate({
  jsonDir: 'reports',
  reportPath: './reports/html',
  metadata: {
    device: isCI ? 'CI/CD Pipeline' : os.hostname(),
    platform: {
      name: os.platform(),
      version: os.release(),
    },
  },
});