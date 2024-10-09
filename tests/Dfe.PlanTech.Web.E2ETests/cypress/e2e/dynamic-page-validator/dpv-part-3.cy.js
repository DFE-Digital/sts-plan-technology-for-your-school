import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { quickNavigateToRecommendations } from "./validators/index.js";

const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Remaining-answer paths", { testIsolation: false }, () => {

    before(() => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        section.getMinimumPathsForRecommendations();
        section.getPathsForAllAnswers();
        section.checkAllChunksTested();

        describe(`${section.name} recommendations`, { testIsolation: false }, () => {

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

        section.pathsForAllPossibleAnswers.forEach((userJourney, index) => {
            const { path, maturity } = userJourney
            describe(`${section.name} should retrieve correct recommendations for additional path ${index + 1} of ${section.pathsForAllPossibleAnswers.length}`, () => {  
                quickNavigateToRecommendations(section, [path], maturity)
            })
        });
        });
    });
});
