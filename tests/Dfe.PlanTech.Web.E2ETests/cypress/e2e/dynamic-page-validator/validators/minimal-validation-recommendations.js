import { selfAssessmentSlug } from "../helpers/index.js";
import { validateAndTestRecommendations, validateQuestionPages } from "./index.js";

/**
 * Navigates through sections according the given paths, using minimal validation
 *
 * @param {Object} section - the section to validate
 * @param {Array} paths - the paths to navigate
 * 
 */

export const minimalSectionValidationForRecommendations = (section, paths, maturity) => {

    for (const path of paths) {

        it(`Navigates through ${section.name} using ${maturity} path`, () => {

            cy.visit(`/${selfAssessmentSlug}`);

            // Navigate through interstitial page
            const sectionSlug = section.interstitialPage.fields.slug;
            cy.findSectionLink(section.name, sectionSlug).click();
            cy.get("a.govuk-button.govuk-link").contains("Continue").click();

            // Conduct self assessment according to path
            validateQuestionPages(path, section)

            // Navigate through Check Answers page and return to self assessment page
            cy.url().should("include", `${sectionSlug}/check-answers`);
            cy.get("button.govuk-button").contains("Save and continue").click();
            cy.url().should("include", selfAssessmentSlug);
        });

        // Validate recommendations
        validateAndTestRecommendations(section, maturity, path);
    }
}
