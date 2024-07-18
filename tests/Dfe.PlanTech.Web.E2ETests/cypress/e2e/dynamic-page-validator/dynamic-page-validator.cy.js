import { selfAssessmentSlug } from "../../helpers/page-slugs.js";
import DataMapper from "export-processor/data-mapper.js";
import { validateSections } from "./dpv-validators/section-validator.js";
import { validateNonAuthorisedPages } from "./dpv-validators/non-auth-pages-validator.js";
import { validateNavigationLinks } from "./dpv-validators/nav-links-validator.js";
import { validateRecommendationForMaturity } from "./dpv-validators/recommendations-validator.js";
import { validateSelfAssessmentPageOnStart } from "./dpv-validators/self-assessment-page-validators.js";

const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));
/*
describe("Navigation links and non-authorised pages", () => {

    it("Should render navigation links", async () => {
        validateNavigationLinks(dataMapper);
    });

    validateNonAuthorisedPages(dataMapper);

});

describe("Self assessment page", () => {

    it("Should validate self-assessment page", () => {
        validateSelfAssessmentPageOnStart(dataMapper);        
    });

});

describe("Sections and all-questions paths", () => {

    (dataMapper?.mappedSections ?? []).forEach((section) => {

        it(`${section.name} should have every question with correct content`, () => {
            cy.loginWithEnv(`${selfAssessmentSlug}`);
            validateSections(section, section.minimumPathsToNavigateQuestions, dataMapper);
        });
    });
});
*/
describe("Recommendations", () => {

    (dataMapper?.mappedSections ?? []).forEach((section) => {
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