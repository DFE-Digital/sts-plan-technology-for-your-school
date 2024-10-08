import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateRecommendationForMaturity, validateSections } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Recommendations", { testIsolation: false }, () => {
    it('Logs in correctly', () => {
        cy.loginWithEnv(`/${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        section.getMinimumPathsForRecommendations();
        Object.keys(section.minimumPathsForRecommendations).forEach((maturity) => {
            validateSections(section, [section.minimumPathsForRecommendations[maturity]], dataMapper, (path) => {
                validateRecommendationForMaturity(section, maturity, path);
            });
        });
    });
});
