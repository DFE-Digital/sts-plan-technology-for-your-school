import contentful from 'contentful-management';
import 'dotenv/config';
import { deleteContentfulContent } from "./content.mjs";

const envOptions = {
  spaceId: process.env.CONTENTFUL_SPACE_ID,
  environmentId: process.env.CONTENTFUL_ENVIRONMENT,
};

const client = contentful.createClient(
  {
    accessToken: process.env.CONTENTFUL_MASTER_KEY,
  },
  {
    type: 'plain',
    defaults: envOptions
  }
);

const contentTypesToDelete = process.env.CONTENT_TYPES_TO_DELETE?.split(",");

await deleteContentfulContent({ client, envOptions, contentTypesToDelete });