import { contentful } from "./contentful";
import { DataMapper } from "export-processor";

const getContentfulData = () => {
  return new DataMapper(contentful);
};

const contentfulData = getContentfulData();

export default contentfulData;
