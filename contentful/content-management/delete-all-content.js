require('dotenv/config');
const readline = require('readline');
const getClient = require('./helpers/get-client');
const validateEnvironment = require('./helpers/validate-environment');
const deleteContentfulContent = require('./helpers/delete-all-content-and-content-types');

function confirm(question) {
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      rl.close();
      resolve(answer.trim().toLowerCase() === 'y');
    });
  });
}

async function deleteAllContentfulData() {
  const client = await getClient();
  const contentTypesToDelete = process.env.CONTENT_TYPES_TO_DELETE?.split(',');
  const target = process.env.CONTENT_TYPES_TO_DELETE ?? 'ALL DATA';

  const confirmed = await confirm(
    `WARNING: About to delete ${target} from environment '${process.env.ENVIRONMENT}'. This cannot be undone. Type 'y' to confirm: `,
  );

  if (!confirmed) {
    console.log('Aborted.');
    return;
  }

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
