import { ExportContentfulData, DataMapper } from "export-processor";

const retrieveContentfulData = async () => {
  const contentfulData = await ExportContentfulData();

  const dataMapper = new DataMapper(contentfulData);

  return dataMapper;
};

export { retrieveContentfulData };