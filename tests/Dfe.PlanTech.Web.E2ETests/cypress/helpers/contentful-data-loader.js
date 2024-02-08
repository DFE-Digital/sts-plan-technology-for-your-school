import { contentful } from "./contentful";
import DataMapper from "../../../../contentful/export-processor/data-mapper";

const getContentfulData = () => {
  return new DataMapper(contentful);
};

const contentfulData = getContentfulData();

export default contentfulData;
