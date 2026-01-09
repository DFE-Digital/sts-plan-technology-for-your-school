import 'dotenv/config';
import contentfulImport from 'contentful-import';
import { existsSync } from 'fs';
import { getAndValidateClient } from './get-client.js';

export default async function importContentfulData() {
  const options = {
    contentFile: process.env.CONTENT_FILE,
    spaceId: process.env.SPACE_ID,
    managementToken: process.env.MANAGEMENT_TOKEN,
    environmentId: process.env.ENVIRONMENT,
    skipContentModel: process.env.SKIP_CONTENT_MODEL === 'true',
  };

  await getAndValidateClient();

  if (!existsSync(options.contentFile)) {
    throw new Error(`File not found: ${options.contentFile}`);
  }

  try {
    await contentfulImport(options);
    console.log(`Import completed successfully from ${options.contentFile}`);
  } catch (error) {
    console.error('Error during import:', error);
    throw error;
  }
}
