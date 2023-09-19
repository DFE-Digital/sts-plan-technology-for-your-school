const { defineConfig } = require('cypress')

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
          console.log(message)

          return null
        },
        table(message) {
          console.table(message)

          return null
        }
      })
    },
  },
})
