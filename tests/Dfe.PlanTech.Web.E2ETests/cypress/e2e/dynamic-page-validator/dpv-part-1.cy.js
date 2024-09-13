import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateNavigationLinks, validateNonAuthorisedPages, validateSections, validateSelfAssessmentPage } from "./validators/index.js";


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
        section.getMinimumPathsForQuestions();
        it(`${section.name} should have every question with correct content`, () => {
            cy.loginWithEnv(`${selfAssessmentSlug}`);
            validateSections(section, section.minimumPathsToNavigateQuestions, dataMapper);
        });
    });
});
