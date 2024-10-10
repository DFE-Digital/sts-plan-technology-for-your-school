import contentfulExport from "contentful-export";

const getBooleanValue = (obj, prop) => obj[prop] === 'true' || obj[prop] === true;

const getBooleanValueFromEnv = (prop) => getBooleanValue(process.env, prop);

const DefaultExportOptions = {
  content: true,
  contentmodel: true,
  webhooks: false,
  roles: false,
  editorinterfaces: false,
  tags: false,
};


/**
 * Exports Contentful data.
 *
 * This function takes no parameters and returns the exported data.
 *
 * @return {object} The exported Contentful data.
 */
export default function exportContentfulData({ spaceId, deliveryToken, managementToken, environment, exportDirectory = "./output", exportOptions = DefaultExportOptions }) {
  console.log('exporting with options', exportOptions);
  const options = {
    spaceId: spaceId ?? process.env.SPACE_ID,
    deliveryToken: deliveryToken ?? process.env.DELIVERY_TOKEN,
    managementToken: managementToken ?? process.env.MANAGEMENT_TOKEN,
    environmentId: environment ?? process.env.ENVIRONMENT,
    host: "api.contentful.com",
    skipEditorInterfaces: !exportOptions.editorinterfaces,
    skipRoles: !exportOptions.roles,
    skipWebhooks: !exportOptions.webhooks,
    skipContentModel: !exportOptions.contentmodel,
    skipContent: !exportOptions.content,
    skipTags: !exportOptions.tags,
    exportDir: exportDirectory,
    saveFile: getBooleanValueFromEnv('SAVE_FILE'),
    includeDrafts: getBooleanValueFromEnv('USE_PREVIEW'),
  };

  console.log('contentful export options', options);
  return contentfulExport(options);
}
