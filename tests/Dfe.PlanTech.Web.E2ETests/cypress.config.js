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
    runMode: 1
  },
  e2e: {
    setupNodeEvents(on, _) {
      on('task', {
        log(message) {
          console.log(message)

          return null
        },
        table(message) {
          console.table(message)

          return null
        }
      }),
        on('file:preprocessor', webpackPreprocessor(webpackOptions))
    },
  },
})
