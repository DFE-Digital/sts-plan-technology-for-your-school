import { FindPageForSlug, selfAssessmentSlug, ValidatePage } from "../helpers/index.js";
import { validateAndTestQuestionPages, validateCheckAnswersPage } from "./index.js";

/**
 * Validates sections using the given paths
 *
 * @param {Object} section - the section to validate
 * @param {Array} paths - the paths to navigate
 * @param {Function} validator - optional validation function to call at the end of every path
 */

export const validateAndTestSections = (section, paths, dataMapper) => {

    for (const path of paths) {

        it(`${section.name} should have interstitial page with correct content`, () => {
            cy.visit(`/${selfAssessmentSlug}`);

            // Navigate through interstitial page
            cy.get("div.govuk-summary-list__row > dt a").contains(section.name).click();
            cy.url().should("include", section.interstitialPage.fields.slug);

            // Validate interstititial page content
            const interstitialPage = FindPageForSlug({ slug: section.interstitialPage.fields.slug, dataMapper });
            ValidatePage(section.interstitialPage.fields.slug, interstitialPage);
        });

        it(`${section.name} should have every question with correct content`, () => {
            // Conduct self assessment according to path
            cy.get("a.govuk-button.govuk-link").contains("Continue").click();
            validateAndTestQuestionPages(path, section);
        });

        it(`${section.name} should have Check Answers page with correct content`, () => {
            // Validate check answers page (questions and answers correspond to those listed in path) and return to self assessment page
            validateCheckAnswersPage(path, section);
            cy.url().should("include", selfAssessmentSlug);
        });
    }
}
