const fs = require("fs");

const ContentfulDataPath = "./cypress/fixtures/contentful-data.json";

let exportProcessor;

/**
 * Exports data from Contentful and writes to files for use by the dynamic-page-validator.cy.js tests
 * @param {*} config 
 */
async function loadAndSaveContentfulData(config) {
  console.log("Exporting/Loading Contentful data");
  try {
    exportProcessor = await import('export-processor');

    await fetchContentfulExport({ exportContentfulData: exportProcessor.ExportContentfulData, config });
  }
  catch (e) {
    console.error("Error loading Contentful data", e);
  }
}

async function fetchContentfulExport({ exportContentfulData, config }) {
  if (fs.existsSync(ContentfulDataPath)) {
    console.log(`Contentful export file at ${ContentfulDataPath} already exists - using it`);
    const json = fs.readFileSync(ContentfulDataPath, "utf-8");

    return JSON.parse(json);
  }

  if (!config.env.SPACE_ID || !config.env.MANAGEMENT_TOKEN || !config.env.DELIVERY_TOKEN || !config.env.CONTENTFUL_ENVIRONMENT) {
    console.log("Cannot fetch Contentful data; missing environment variable(s)");
    return;
  }

  const data = await exportContentfulData({
    spaceId: config.env.SPACE_ID,
    managementToken: config.env.MANAGEMENT_TOKEN,
    deliveryToken: config.env.DELIVERY_TOKEN,
    environment: config.env.CONTENTFUL_ENVIRONMENT,
  });

  /**As the data is passed to/from the dev server + browser, it has to be serialisable.
*    So we save the Contentful entries as JSON instead for read + usage by the test file later. */
  fs.writeFileSync(ContentfulDataPath, JSON.stringify(data));

  console.log("Saved Contentful data as " + ContentfulDataPath);
  return data;
}

const readContentfulDataFromJson = () => {
  if (!fs.existsSync(ContentfulDataPath)) {
    return;
  }

  const data = fs.readFileSync(ContentfulDataPath, "utf-8");

  return JSON.parse(data);
};

module.exports = {
  loadAndSave: loadAndSaveContentfulData,
  readFromJson: readContentfulDataFromJson
};