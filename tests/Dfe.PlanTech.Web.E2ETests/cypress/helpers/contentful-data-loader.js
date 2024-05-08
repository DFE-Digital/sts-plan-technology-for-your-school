import { contentful } from "./contentful";
import { DataMapper } from "export-processor";

const getContentfulData = () => {
  if (!contentful ||
    !contentful.contentTypes || contentful.contentTypes.length == 0 ||
    !contentful.entries || contentful.entries.length == 0) {
    return;
  }

  return new DataMapper(contentful);
};

const contentfulData = getContentfulData();

export default contentfulData;
