import { Section } from "./section.mjs";
import fs from "fs";

/**
 * DataMapper class for mapping and combining data from a file
 */
export default class DataMapper {
  // Maps for different content types
  answers = new Map();
  pages = new Map();
  questions = new Map();
  recommendations = new Map();
  sections = new Map();

  // Map of content type to corresponding Map
  contentTypeMappings = {
    answer: this.answers,
    page: this.pages,
    question: this.questions,
    recommendationPage: this.recommendations,
    section: this.sections,
  };

  /**
   * Get the mapped sections
   * @returns {IterableIterator<Section>} Iterator for mapped sections
   */
  get mappedSections() {
    return this.sectionsToClasses(this.sections);
  }

  /**
   * Constructor for DataMapper
   * @param {string} filePath - Path to the file to map data from
   */
  constructor(filePath) {
    this.mapData(filePath);
  }

  /**
   * Map data from the provided file
   * @param {string} filePath - Path to the file to map data from
   */
  mapData(filePath) {
    const fileContents = fs.readFileSync(filePath, "utf-8");
    const jsonObject = JSON.parse(fileContents);

    this.mapEntries(jsonObject.entries);
    this.combineEntries();
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
    const setForContentType = this.contentTypeMappings[contentType];
    if (!setForContentType) return;

    // Add the entry to the set
    const id = entry.sys.id;
    this.stripLocalisationFromAllFields(entry);
    setForContentType.set(id, entry);
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
    for (const [id, recommendation] of this.recommendations) {
      recommendation.fields.page = this.pages.get(
        recommendation.fields.page.sys.id
      );
    }

    for (const [id, question] of this.questions) {
      question.fields.answers = this.copyRelationships(
        question.fields.answers,
        this.answers
      );
    }

    for (const [id, section] of this.sections) {
      section.fields.questions = this.copyRelationships(
        section.fields.questions,
        this.questions
      );

      section.fields.recommendations = this.copyRelationships(
        section.fields.recommendations,
        this.recommendations
      );
    }
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
