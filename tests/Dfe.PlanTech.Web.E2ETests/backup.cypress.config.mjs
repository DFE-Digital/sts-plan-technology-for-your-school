import { defineConfig } from 'cypress';
import { resolve as _resolve } from 'path';
import { retrieveContentfulData } from "./cypress/helpers/retrieve-contentful-data";

export default defineConfig({
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
          console.log(message);

          return null;
        },
        table(message) {
          console.table(message);

          return null;
        },
        fetchContentfulData() {
          return retrieveContentfulData();
        }
      });
    },
  },
});
