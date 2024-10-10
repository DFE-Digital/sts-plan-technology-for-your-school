import DataMapper from "export-processor/data-mapper.js";
import { selfAssessmentSlug } from "./helpers/index.js";
import { quickNavigateToRecommendations } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Recommendations", { testIsolation: false }, () => {

    before(() => {
        cy.loginWithEnv(`${selfAssessmentSlug}`);
    });

    (dataMapper?.mappedSections ?? []).forEach((section) => {
        describe(`${section.name} recommendations`, { testIsolation: false }, () => {

            // Establish section status using self-assessment page tag    
            before(function () {
                cy.checkSectionStatus(section.name, selfAssessmentSlug)
                    .then((inProgress) => {
                        if (inProgress) {
                            console.log(`Skipping tests for section: ${section.name} (status is 'in progress')`);
                            this.skip();
                        }
                    });
            });

            Object.keys(section.minimumPathsForRecommendations).forEach((maturity) => {
                quickNavigateToRecommendations(section, [section.minimumPathsForRecommendations[maturity]], maturity);
            });
        });
    });
});
