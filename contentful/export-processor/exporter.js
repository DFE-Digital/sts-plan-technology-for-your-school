import contentfulExport from "contentful-export";

/**
 * Exports Contentful data.
 *
 * This function takes no parameters and returns the exported data.
 *
 * @return {object} The exported Contentful data.
 */
export default function exportContentfulData() {
  const options = {
    spaceId: process.env.SPACE_ID,
    deliveryToken: process.env.DELIVERY_TOKEN,
    managementToken: process.env.MANAGEMENT_TOKEN,
    environmentId: process.env.ENVIRONMENT,
    host: "api.contentful.com",
    skipEditorInterfaces: true,
    skipRoles: true,
    skipWebhooks: true,
    saveFile: process.env.SAVE_FILE === "true" || process.env.SAVE_FILE === true,
    includeDrafts: process.env.USE_PREVIEW === "true" || process.env.USE_PREVIEW === true,
  };

  return contentfulExport(options);
}