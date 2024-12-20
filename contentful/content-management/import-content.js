require('dotenv').config();

const contentfulImport = require('contentful-import');
const fs = require('fs');
const validateEnvironment = require("./helpers/validate-environment");

async function importContentfulData() {
  const options = {
    contentFile: process.env.CONTENT_FILE,
    spaceId: process.env.SPACE_ID,
    managementToken: process.env.MANAGEMENT_TOKEN,
    environmentId: process.env.ENVIRONMENT,
	  skipContentModel: process.env.SKIP_CONTENT_MODEL === 'true' ? true : false
  };

  await validateEnvironment();

  if (!fs.existsSync(options.contentFile)) {
    throw new Error(`File not found: ${options.contentFile}`);
  }

  console.log("Attempting import with the following options:", options)

  try {
    await contentfulImport(options);
    console.log(`Import completed successfully from ${options.contentFile}`);
  } catch (error) {
    console.error('Error during import:', error);
    throw error;
  }
}

importContentfulData()