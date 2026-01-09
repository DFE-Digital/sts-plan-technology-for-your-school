/**
 * @typedef {Object} ContentTypeDefinition
 * @property {string[]} [manyChildReferences] - Collection of field IDs where the field is an array of entity references
 */

/**
 * @typedef {Record<string, ContentTypeDefinition>} AllContentTypesMap
 */

/**
 * A map of all available content types in the system.
 *
 * @type {AllContentTypesMap}
 */
const AllContentTypes = {
  button: {},
  buttonWithEntryReference: {},
  buttonWithLink: {},
  category: { manyChildReferences: ['content', 'sections'] },
  componentDropDown: {},
  csLink: {},
  header: {},
  insetText: {},
  navigationLink: {},
  page: {
    manyChildReferences: ['content', 'beforeTitleContent'],
  },
  question: {
    manyChildReferences: ['answers'],
  },
  recommendationChunk: {
    manyChildReferences: ['answers', 'content'],
  },
  recommendationIntro: {
    manyChildReferences: ['content'],
  },
  recommendationSection: {
    manyChildReferences: ['answers', 'chunks'],
  },
  section: {
    manyChildReferences: ['questions'],
  },
  subtopicRecommendation: {
    manyChildReferences: ['intros'],
  },
  textBody: {},
  title: {},
  warningComponent: {},
};

/**
 * Executes a callback function for each content type in the `AllContentTypes` map.
 *
 * @param {function(string, ContentTypeDefinition): void} callback - The callback function to execute for each content type. It receives the content type key and its definition as arguments.
 */
const runForEachContentType = (callback) =>
  Object.entries(AllContentTypes).forEach(([contetTypeId, contentType]) =>
    callback(contetTypeId, contentType),
  );

module.exports = {
  runForEachContentType,
  AllContentTypes,
};
