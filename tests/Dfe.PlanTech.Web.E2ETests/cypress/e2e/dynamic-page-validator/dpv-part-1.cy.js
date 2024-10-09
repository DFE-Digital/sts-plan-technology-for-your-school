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
            // Establish section status using self-assessment page tag
            let inProgress = false;
            before(() => {
                cy.visit(`${selfAssessmentSlug}`)
                cy.get("a.govuk-link")
                    .contains(section.name.trim())
                    .parent()
                    .next()
                    .within(() => {
                        cy.get("strong.app-task-list__tag").invoke("text")
                            .then((text) => {
                                inProgress = text.includes("in progress");
                            });
                    });
            });

            // Skip any sections that are 'In Progress'
            before(function () {
                cy.wrap(null).then(() => {
                    if (inProgress) {
                        console.log(`Skipping all tests for section: ${section.name} (status is 'in progress'')`);
                        this.skip();
                    }
                });
            });
            section.getMinimumPathsForQuestions();
            validateSections(section, section.minimumPathsToNavigateQuestions, dataMapper);
        });
    });
});
