import contentfulExport from "contentful-export";

const getBooleanValue = (obj, prop) => obj[prop] === 'true' || obj[prop] === true;

const getBooleanValueFromEnv = (prop) => getBooleanValue(process.env, prop);

/**
 * Exports Contentful data.
 *
 * This function takes no parameters and returns the exported data.
 *
 * @return {object} The exported Contentful data.
 */
export default function exportContentfulData({ spaceId, deliveryToken, managementToken, environment, exportDirectory } = {}) {
  const options = {
    spaceId: spaceId ?? process.env.SPACE_ID,
    deliveryToken: deliveryToken ?? process.env.DELIVERY_TOKEN,
    managementToken: managementToken ?? process.env.MANAGEMENT_TOKEN,
    environmentId: environment ?? process.env.ENVIRONMENT,
    host: "api.contentful.com",
    skipEditorInterfaces: true,
    skipRoles: true,
    skipWebhooks: true,
    exportDir: exportDirectory ?? "./output",
    saveFile: getBooleanValueFromEnv('SAVE_FILE'),
    includeDrafts: getBooleanValueFromEnv('USE_PREVIEW'),
  };

  return contentfulExport(options);
}
