import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { validateAndTestSections } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Sections and all-questions paths", { testIsolation: false }, () => {

    before(() => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        describe(`${section.name} self-assessment and question pages`, () => {
            before(function () {
                const sectionSlug = section.interstitialPage.fields.slug;
                cy.checkSectionStatus(section.name, sectionSlug, selfAssessmentSlug)
                    .then((inProgress) => {
                        if (inProgress) {
                            console.log(`Skipping tests for section: ${section.name} (status is 'in progress')`);
                            this.skip();
                        }
                    });
            });
            validateAndTestSections(section, section.pathInfo.minimumPathsToNavigateQuestions, dataMapper);
        });
    });
});
