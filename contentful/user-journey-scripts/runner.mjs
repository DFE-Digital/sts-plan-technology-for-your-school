import { contentful } from "./contentful.mjs";
import DataMapper from "./data-mapper.mjs";

const dataMapper = new DataMapper(contentful);

const test = dataMapper.mappedSections;

console.log("");
