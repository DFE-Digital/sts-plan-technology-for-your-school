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

describe("Sections and all-questions paths", { testIsolation: false }, () => {
    it('Logs in correctly', () => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        section.getMinimumPathsForQuestions();
        validateSections(section, section.minimumPathsToNavigateQuestions, dataMapper);
    });
});
