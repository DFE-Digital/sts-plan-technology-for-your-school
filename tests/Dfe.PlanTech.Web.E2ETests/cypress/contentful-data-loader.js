const fs = require("fs");

const ContentfulDataPath = "./cypress/contentful-data.json";
const SectionsFixturePath = "./cypress/fixtures/sections.json";

/**
 * Exports data from Contentful and writes to files for use by the dynamic-page-validator.cy.js tests
 * @param {*} config 
 */
async function loadAndSaveContentfulData(config) {
  console.log("Exporting/Loading Contentful data");
  try {
    const exportProcessor = await import('export-processor');

    const data = await fetchContentfulExport({ exportContentfulData: exportProcessor.ExportContentfulData, config });

    if (!data) {
      return;
    }

    console.log("Loaded data. Creating fixtures");
    saveSectionsIfNotExist(data);
  }
  catch (e) {
    console.error("Error loading Contentful data", e);
  }
}

async function fetchContentfulExport({ exportContentfulData, config }) {
  if (fs.existsSync(ContentfulDataPath)) {
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

  return data;
}

const readContentfulDataFromJson = () => {
  if (!fs.existsSync(ContentfulDataPath)) {
    return;
  }

  const data = fs.readFileSync(ContentfulDataPath, "utf-8");

  return JSON.parse(data);
};

/**
 * Cypress tests can only be dynamically generated from data already existing before any specs run.
 * Whilst we could retrieve this information with `cy.task`, this would mean most of the tests would be unable
 * to be generated, as this is only ran at the start of the tests. Therefore we extract the minimum amount of
 * data needed to generate the tests,
 */
function saveSectionsIfNotExist(data) {
  if (fs.existsSync(SectionsFixturePath)) {
    return;
  }

  const dataMapper = new exportProcessor.DataMapper(data);

  const sectionFixtures = dataMapper.mappedSections.map(section => ({
    name: section.name,
    minimumPathsForRecommendations: section.minimumPathsForRecommendations,
    id: section.id
  }));

  fs.writeFileSync(SectionsFixturePath, JSON.stringify(sectionFixtures));
}

module.exports = {
  loadAndSave: loadAndSaveContentfulData,
  readFromJson: readContentfulDataFromJson
};