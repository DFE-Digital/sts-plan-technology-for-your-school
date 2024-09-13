import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateRecommendationForMaturity, validateSections } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Remaining-answer paths", () => {

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        section.getMinimumPathsForRecommendations();
        section.getPathsForAllAnswers();
        section.checkAllChunksTested();
        section.pathsForAllPossibleAnswers.forEach((userJourney, index) => {
            const { path, maturity } = userJourney
            it(`${section.name} should retrieve correct recommendations for additional path ${index + 1} of ${section.pathsForAllPossibleAnswers.length}`, () => {
                cy.loginWithEnv(`${selfAssessmentSlug}`);
                validateSections(section, [path], dataMapper, (path) => {
                    validateRecommendationForMaturity(section, maturity, path);
                });
            })
        });
    });
});
