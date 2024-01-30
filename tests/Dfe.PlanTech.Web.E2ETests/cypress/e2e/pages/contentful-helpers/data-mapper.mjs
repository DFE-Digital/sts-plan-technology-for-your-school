import { Section } from "./section.mjs";
import fs from "fs";
import ContentType from "./content-type.mjs";

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
        this.sectionsToClasses(this.contents["section"])
      );

    return this._alreadyMappedSections;
  }

  get pages() {
    return this.contents["page"];
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
    let setForContentType = this.contents[contentType];

    if (!setForContentType) {
      setForContentType = new Map();
      this.contents[contentType] = setForContentType;
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
    for (const [id, section] of sections) {
      yield new Section(section);
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
              path: path.path.map((s) => {
                return {
                  question: s.question.text,
                  answer: s.answer.text,
                };
              }),
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
    for (const [contentTypeId, contents] of Object.entries(this.contents)) {
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
        entry.fields[key] = value.map((item) =>
          this.getMatchingContentForReference(referencedTypesForField, item)
        );
      } else {
        entry.fields[key] = this.getMatchingContentForReference(
          referencedTypesForField,
          value
        );
      }
    });
  }

  getContentForFieldId(referencedTypesForField, id) {
    const matchingItem = referencedTypesForField
      .map((type) => {
        const matchingContents = this.contents[type];
        const matchingContent = matchingContents.get(id);

        return matchingContent;
      })
      .find((matching) => matching != null);

    if (!matchingItem) {
      console.error(`Error finding ${id} for ${referencedTypesForField}`);
    }

    return matchingItem;
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
