const os = require('os');
const reporter = require('multiple-cucumber-html-reporter');

const isCI = !!process.env.CI;

reporter.generate({
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
