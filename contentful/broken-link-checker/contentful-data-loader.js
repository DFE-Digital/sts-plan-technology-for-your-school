import { existsSync, readFileSync, writeFileSync, mkdirSync } from 'fs';
import path from 'path';

const ContentfulDataPath = './export/contentful-data.json';

mkdirSync(path.dirname(ContentfulDataPath), { recursive: true });

let exportProcessor;

/**
 * Exports data from Contentful and writes to files for use by the dynamic-page-validator.cy.js tests
 * @param {*} config
 */
export async function loadAndSaveContentfulData(config) {
  console.log('Exporting/Loading Contentful data');
  try {
    exportProcessor = await import('export-processor');

    await fetchContentfulExport({
      exportContentfulData: exportProcessor.ExportContentfulData,
      config,
    });
  } catch (e) {
    console.error('Error loading Contentful data', e);
  }
}

async function fetchContentfulExport({ exportContentfulData, config }) {
  if (existsSync(ContentfulDataPath)) {
    console.log(
      `Contentful export file at ${ContentfulDataPath} already exists - using it. Delete the .json if you require a fresh import.`,
    );
    const contents = readFileSync(ContentfulDataPath, 'utf-8');

    const json = JSON.parse(contents);

    const jsonValid = json.entries?.length > 0 && json.contentTypes?.length > 0;

    if (jsonValid) {
      return json;
    }
  }

  if (!config.env.SPACE_ID || !config.env.MANAGEMENT_TOKEN || !config.env.CONTENTFUL_ENVIRONMENT) {
    console.log('Cannot fetch Contentful data; missing environment variable(s)');
    return;
  }

  const data = await exportContentfulData({
    spaceId: config.env.SPACE_ID,
    managementToken: config.env.MANAGEMENT_TOKEN,
    environment: config.env.CONTENTFUL_ENVIRONMENT,
  });

  /**As the data is passed to/from the dev server + browser, it has to be serialisable.
   *    So we save the Contentful entries as JSON instead for read + usage by the test file later. */
  writeFileSync(ContentfulDataPath, JSON.stringify(data));

  console.log('Saved Contentful data as ' + ContentfulDataPath);
  return data;
}

export const readContentfulDataFromJson = () => {
  if (!existsSync(ContentfulDataPath)) {
    return;
  }

  const data = readFileSync(ContentfulDataPath, 'utf-8');

  return JSON.parse(data);
};
