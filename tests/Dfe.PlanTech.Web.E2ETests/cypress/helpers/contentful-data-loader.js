import { contentful } from "./contentful.js";

const getContentfulData = async () => {
  if (!contentful ||
    !contentful.contentTypes || contentful.contentTypes.length == 0 ||
    !contentful.entries || contentful.entries.length == 0) {
    return;
  }

  const dataMapper = await cy.task("fetchContentfulData");

  return dataMapper;
};

const contentfulData = getContentfulData();

export default contentfulData;
