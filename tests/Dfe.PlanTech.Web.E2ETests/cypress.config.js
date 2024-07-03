const { defineConfig } = require('cypress');
const webpackPreprocessor = require('@cypress/webpack-preprocessor');
const path = require('path');
const webpackOptions = webpackPreprocessor.defaultOptions;

Object.assign(webpackOptions.webpackOptions, {
  resolve: {
    ...webpackOptions.webpackOptions.resolve,
    alias: {
      "export-processor": path.resolve(__dirname, '../../contentful/export-processor')
    }
  },
});


module.exports = defineConfig({
  chromeWebSecurity: false,
  video: true,
  reporter: "cypress-multi-reporters",
  reporterOptions: {
    "configFile": "reporter-config.json"
  },
  retries: {
    runMode: 3,
  },
  //Timeouts
  //Original values: https://docs.cypress.io/guides/references/configuration#Timeouts
  defaultCommandTimeout: 5000,
  execTimeout: 75000,
  taskTimeout: 75000,
  pageLoadTimeout: 75000,
  requestTimeout: 7000,
  responseTimeout: 38000,
  e2e: {
    setupNodeEvents(on, _) {
      on('task', {
        log(message) {
          console.log(message);

          return null;
        },
        table(message) {
          console.table(message);

          return null;
        }
      }),
        on('file:preprocessor', webpackPreprocessor(webpackOptions));
    },
  },
});
