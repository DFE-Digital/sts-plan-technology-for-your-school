require('dotenv').config();

const contentfulImport = require('contentful-import');
const fs = require('fs');
const getClient = require("./helpers/get-client");
const deleteContentfulContent = require("./helpers/delete-all-content-and-content-types");

async function importContentfulData() {
  const options = {
    contentFile: process.env.CONTENT_FILE,
    spaceId: process.env.SPACE_ID,
    managementToken: process.env.MANAGEMENT_TOKEN,
    environmentId: process.env.ENVIRONMENT,
	  skipContentModel: process.env.SKIP_CONTENT_MODEL === 'true' ? true : false
  };
  const client = await getClient();

  if (!fs.existsSync(options.contentFile)) {
    throw new Error(`File not found: ${options.contentFile}`);
  }

  if (process.env.DELETE_ALL_DATA == 'true') {
    console.log(`Deleting all existing data from ${options.environmentId}`);
    await deleteContentfulContent({ client: client });
  }

  console.log("Starting import with the following options:", options)

  try {
    await contentfulImport(options);
    console.log(`Import completed successfully from ${options.contentFile}`);
  } catch (error) {
    console.error('Error during import:', error);
    throw error;
  }
}

importContentfulData()