import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { minimalSectionValidationForRecommendations } from "./validators/index.js";

const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Remaining-answer paths", { testIsolation: false }, () => {

    before(() => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        describe(`${section.name} recommendations`, { testIsolation: false }, () => {

            before(function () {
                cy.checkSectionStatus(section.name, selfAssessmentSlug)
                    .then((inProgress) => {
                        if (inProgress) {
                            console.log(`Skipping tests for section: ${section.name} (status is 'in progress')`);
                            this.skip();
                        }
                    });
            });

            section.pathInfo.pathsForAllPossibleAnswers.forEach((userJourney, index) => {
                const { path, maturity } = userJourney
                describe(`${section.name} should retrieve correct recommendations for additional path ${index + 1} of ${section.pathsForAllPossibleAnswers.length}`, () => {
                    minimalSectionValidationForRecommendations(section, [path], maturity)
                });
            });
        });
    });
});
