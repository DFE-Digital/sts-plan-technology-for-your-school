let dataMapper = null;

let exportContentfulData = null;

const retrieveContentfulData = async ({ contentfulExporter, dataMapper }) => {
  if (dataMapper) {
    return dataMapper;
  }

  const contentfulData = await contentfulExporter();

  dataMapper = new DataMapper(contentfulData);

  return dataMapper;
};

module.exports = retrieveContentfulData;