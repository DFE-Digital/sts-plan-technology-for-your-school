import "dotenv/config";
import DataMapper from "#src/data-mapper";
import ExportContentfulData from "./exporter.js";
import { getContentsWithReferences } from "#src/migrations/get-contents-with-references";
import { postEntriesToWebhook } from "#src/migrations/post-content-to-webhook";

const contentfulData = await ExportContentfulData();

const entries = [...contentfulData.entries.map(entry => ({ ...entry }))];
const dataMapper = new DataMapper(contentfulData);

const contents = getContentsWithReferences({ dataMapper, entries });

await postEntriesToWebhook({ contents });