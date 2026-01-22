require('dotenv/config');
const getClient = require('./helpers/get-client');
const validateEnvironment = require('./helpers/validate-environment');
const deleteContentfulContent = require('./helpers/delete-all-content-and-content-types');

async function deleteAllContentfulData() {
  const client = await getClient();
  const contentTypesToDelete = process.env.CONTENT_TYPES_TO_DELETE?.split(',');

  console.log(
    `Deleting ${process.env.CONTENT_TYPES_TO_DELETE ?? 'all data'} from the following environment: ${process.env.ENVIRONMENT}`,
  );

  try {
    await deleteContentfulContent({ client, contentTypesToDelete });
    console.log(`Content deleted successfully for ${process.env.ENVIRONMENT}`);
  } catch (error) {
    console.error('Error during deletion:', error);
    throw error;
  }
}

deleteAllContentfulData();
