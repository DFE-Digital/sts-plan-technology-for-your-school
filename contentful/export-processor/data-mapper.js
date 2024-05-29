import { Section } from "./content-types/section.js";
import ContentType from "./content-types/content-type.js";
import SubtopicRecommendation from "./content-types/subtopic-recommendation.js";
import ErrorLogger from "./errors/error-logger.js";

/**
 * DataMapper class for mapping and combining data from a file
 */
export default class DataMapper {
  // Maps for different content types
  contents = new Map();

  contentTypes = new Map();

  _alreadyMappedSections;
  /**
   * Get the mapped sections
   * @returns {IterableIterator<Section>} Iterator for mapped sections
   */

  get mappedSections() {
    if (!this._alreadyMappedSections)
      this._alreadyMappedSections = Array.from(
        this.sectionsToClasses(this.contents.get("section"))
      );

    return this._alreadyMappedSections;
  }

  get pages() {
    return this.contents.get("page");
  }

  /**
   * Constructor for DataMapper
   * @param {string} filePath - Path to the file to map data from
   */
  constructor({ entries, contentTypes }) {
    this.mapData(entries, contentTypes);
  }

  /**
   * Map data from the provided file
   * @param {string} filePath - Path to the file to map data from
   */
  mapData(entries, contentTypes) {
    this.mapContentTypes(contentTypes);
    this.mapEntries(entries);
    this.combineEntries();

    console.log('contents', this.contents);
  }

  mapContentTypes(contentTypes) {
    for (const contentType of contentTypes) {
      const mapped = new ContentType(contentType);
      this.contentTypes.set(mapped.id, mapped);
    }
  }

  /**
   * Map entries from the provided data
   * @param {Array} entries - Array of entries to map
   */
  mapEntries(entries) {
    for (const entry of entries) {
      this.tryAddToSet(entry);
    }
  }

  /**
   * Try to add an entry to the corresponding set based on content type
   * @param {object} entry - Entry to add to the set
   */
  tryAddToSet(entry) {
    // Get content type of the entry
    const contentType = entry.sys.contentType.sys.id;

    // Get the set for the content type
    const setForContentType = this.getContentTypeSet(contentType);
    // Add the entry to the set
    const id = entry.sys.id;

    this.stripLocalisationFromAllFields(entry);

    setForContentType.set(id, entry);
  }

  getContentTypeSet(contentType) {
    let setForContentType = this.contents.get(contentType);

    if (!setForContentType) {
      setForContentType = new Map();
      this.contents.set(contentType, setForContentType);
    }

    return setForContentType;
  }

  /**
   * Strip localisation from all fields of an entry
   * @param {object} entry - Entry to strip localisation from
   */
  stripLocalisationFromAllFields(entry) {
    const mappedFields = {};
    Object.entries(entry.fields).forEach(([key, value]) => {
      mappedFields[key] = this.stripLocalisationFromField(value);
    });
    entry.fields = mappedFields;
  }

  /**
   * Convert sections to classes
   * @param {Map} sections - Map of sections to convert
   * @returns {IterableIterator<Section>} Iterator for mapped sections
   */
  *sectionsToClasses(sections) {
    if (!sections || sections.length == 0) {
      return;
    }

    const subtopicRecommendations = Array.from(this.contents.get("subtopicRecommendation"));
    if (!subtopicRecommendations || subtopicRecommendations.length == 0) {
      throw `No subtopic recommendations found`;
    }

    for (const [id, subtopicRecommendation] of subtopicRecommendations) {
      const mapped = new SubtopicRecommendation(subtopicRecommendation);

      yield mapped.subtopic;
    }
  }

  /**
   * Convert section to minimal section info; only information we care for writing to file
   * @param {Section} section - Section to convert
   * @param {boolean} writeAllPossiblePaths - Whether to write all possible paths
   * @returns {object} Minimal section info
   */
  convertToMinimalSectionInfo(section, writeAllPossiblePaths = false) {
    return {
      section: section.name,
      allPathsStats: section.stats,
      minimumQuestionPaths: section.minimumPathsToNavigateQuestions,
      minimumRecommendationPaths: section.minimumPathsForRecommendations,
      allPossiblePaths: writeAllPossiblePaths
        ? section.paths.map((path) => {
          var result = {
            recommendation:
              path.recommendation != null
                ? {
                  name: path.recommendation?.displayName,
                  maturity: path.recommendation?.maturity,
                }
                : null,
            path: path.pathWithTextOnly,
          };

          return result;
        })
        : undefined,
    };
  }

  /**
   * Combine entries for all tracked content types (i.e. answers, pages, questions, recommendations, sections)
   */
  combineEntries() {
    for (const [contentTypeId, contents] of this.contents.entries()) {
      const contentType = this.contentTypes.get(contentTypeId);

      for (const [id, entry] of contents) {
        this.mapRelationshipsForEntry(entry, contentType);
      }
    }
  }

  mapRelationshipsForEntry(entry, contentType) {
    Object.entries(entry.fields).forEach(([key, value]) => {
      const referencedTypesForField =
        contentType.getReferencedTypesForField(key);

      if (!referencedTypesForField) return;

      if (Array.isArray(value)) {
        entry.fields[key] = value.map((item) => {
          const matching = this.getMatchingContentForReference(referencedTypesForField, item);

          if (!matching) {
            logMissingContent(referencedTypesForField, item, entry);
          }

          return matching;
        }).filter(item => !!item);
      } else {
        entry.fields[key] = this.getMatchingContentForReference(
          referencedTypesForField,
          value
        );

        if (!entry.fields[key]) {
          logMissingContent(referencedTypesForField, key, entry);
        }
      }
    });
  }

  getContentForFieldId(referencedTypesForField, id) {
    return referencedTypesForField
      .map((type) => {
        const matchingContents = this.contents.get(type);
        const matchingContent = matchingContents.get(id);

        return matchingContent;
      })
      .find((matching) => matching != null);
  }

  getMatchingContentForReference(referencedTypesForField, reference) {
    const referenceId = reference["sys"]?.["id"];

    if (!referenceId) {
      console.log("no reference", reference);
      return;
    }

    return this.getContentForFieldId(referencedTypesForField, referenceId);
  }

  /**
   * Strip localisation from a field
   * @param {object} obj - Object to strip localisation from
   */
  stripLocalisationFromField(obj) {
    return obj["en-US"];
  }

  /**
   * Copies relationships from parent to children.
   *
   * @param {type} parent - description of parameter
   * @param {type} children - description of parameter
   * @return {type} relationships copied from parent to children
   */
  copyRelationships(parent, children) {
    return parent.map((child) => children.get(child.sys.id));
  }
}
function logMissingContent(referencedTypesForField, item, entry) {
  ErrorLogger.addError({
    id: entry.sys.id,
    contentType: entry.sys.contentType.sys.id,
    message: `Could not find matching content for ${referencedTypesForField} ${item?.sys?.id ?? item}`
  });
}

