const { defineConfig } = require('cypress');
const { loadAndSave, readFromJson } = require("./cypress/contentful-data-loader");

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
    specPattern: [
      /**
       * For some reason the dynamic page validator would hang when trying to create a session.
       * So we order it last so that a session already exists.
       */
      "cypress/e2e/components/*.cy.js",
      "cypress/e2e/pages/*.cy.js",
      "cypress/e2e/*.cy.js",
    ],
    setupNodeEvents(on, config) {
      /**
       * Load Contentful data after browser launched. See comments on contentfulDataAndSaveToJson for explanation.
       * 
       * Ideally this would run before the browser loads, incase (some how), all the tests are run before the 
       * data is needed. However, when tried with "before:browser:launch" the browser didn't load for me, and when
       * running with "dev-server:start" hanged.
       */
      on("after:browser:launch", async () => {
        await loadAndSave(config);
      }),
        on('task', {
          log(message) {
            console.log(message);

            return null;
          },
          table(message) {
            console.table(message);

            return null;
          },
          readContentfulDataJson() {
            return readFromJson();
          },
        });
    },
  },
});

