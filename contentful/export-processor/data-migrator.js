import "dotenv/config";
import DataMapper from "./data-mapper.js";
import ExportContentfulData from "./exporter.js";
import { getContentsWithReferences } from "./migrations/get-contents-with-references.js";
import { postEntriesToWebhook } from "./migrations/post-content-to-webhook.js";

const contentfulData = await ExportContentfulData();

const entries = [...contentfulData.entries.map(entry => ({ ...entry }))];
const dataMapper = new DataMapper(contentfulData);

const contents = getContentsWithReferences({ dataMapper, entries });

await postEntriesToWebhook({ contents });