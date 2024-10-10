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

    before(() => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        describe(`${section.name} self-assessment and question pages`, () => {
            before(function () {
                cy.checkSectionStatus(section.name, selfAssessmentSlug)
                    .then((inProgress) => {
                        if (inProgress) {
                            console.log(`Skipping tests for section: ${section.name} (status is 'in progress')`);
                            this.skip();
                        }
                    });
            });
            section.getMinimumPathsForQuestions();
            validateSections(section, section.minimumPathsToNavigateQuestions, dataMapper);
        });
    });
});
