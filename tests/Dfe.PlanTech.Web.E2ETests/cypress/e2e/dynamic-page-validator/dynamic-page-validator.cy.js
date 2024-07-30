import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateNavigationLinks, validateNonAuthorisedPages, validateRecommendationForMaturity, validateSections, validateSelfAssessmentPage } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Navigation links and non-authorised pages", () => {

    it("Should render navigation links", () => {
        validateNavigationLinks(dataMapper);
    });

    validateNonAuthorisedPages(dataMapper);

});

describe("Self assessment page", () => {
    
    it("Should validate self-assessment page", () => {
        validateSelfAssessmentPage(dataMapper);        
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