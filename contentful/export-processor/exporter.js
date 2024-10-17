import contentfulExport from "contentful-export";
import { getArgumentValue } from "./options-helpers.js";

const DefaultExportOptions = {
  content: true,
  contentmodel: true,
  webhooks: false,
  roles: false,
  editorinterfaces: false,
  tags: false,
};

const getBooleanValueFromEnv = (prop) => getArgumentValue(process.env, prop, true);

/**
 * Exports Contentful data.
 *
 * This function takes no parameters and returns the exported data.
 *
 * @return {object} The exported Contentful data.
 */
export default function exportContentfulData({ spaceId, deliveryToken, managementToken, environment, saveFile, usePreview, exportDirectory = "./output", exportOptions = DefaultExportOptions }) {
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
    saveFile: saveFile !== undefined ? saveFile : getBooleanValueFromEnv('SAVE_FILE'),
    includeDrafts: usePreview !== undefined ? usePreview : getBooleanValueFromEnv('USE_PREVIEW'),
  };

  return contentfulExport(options);
}
