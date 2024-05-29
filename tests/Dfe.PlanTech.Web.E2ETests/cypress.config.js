const { defineConfig } = require('cypress');
const retrieveContentfulData = require('./cypress/helpers/retrieve-contentful-data');

module.exports = defineConfig({
  chromeWebSecurity: false,
  reporter: "cypress-multi-reporters",
  reporterOptions: {
    "configFile": "reporter-config.json"
  },
  retries: {
    runMode: 1
  },
  e2e: {
    setupNodeEvents(on, config) {
      on('task', {
        log(message) {
          console.log(message);

          return null;
        },
        table(message) {
          console.table(message);

          return null;
        },
        async fetchContentfulData() {
          const exportProcessor = await import('export-processor');

          const data = await exportProcessor.ExportContentfulData({
            spaceId: config.env.SPACE_ID,
            managementToken: config.env.MANAGEMENT_TOKEN,
            deliveryToken: config.env.DELIVERY_TOKEN,
            environment: config.env.CONTENTFUL_ENVIRONMENT,
          });

          return data;
        }
      });
    },
  },
});


