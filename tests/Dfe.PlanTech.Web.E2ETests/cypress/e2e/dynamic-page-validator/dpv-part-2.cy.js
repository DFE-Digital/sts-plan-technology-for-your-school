import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateRecommendationForMaturity, validateSections } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Recommendations", () => {

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        section.getMinimumPathsForRecommendations();
        Object.keys(section.minimumPathsForRecommendations).forEach((maturity) => {

            it(`${section.name} should retrieve correct recommendation for ${maturity} maturity, and all content is valid`, () => {
                cy.loginWithEnv(`${selfAssessmentSlug}`);
                validateSections(section, [section.minimumPathsForRecommendations[maturity]], dataMapper, (path) => {
                    validateRecommendationForMaturity(section, maturity, path);
                });
            });
        });
    });
});
