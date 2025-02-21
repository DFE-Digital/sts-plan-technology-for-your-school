import { FindPageForSlug, selfAssessmentSlug, ValidatePage } from "../helpers/index.js";
import { validateCompletionTags, validateQuestionPages, validateCheckAnswersPage, validateAnswersHintAndUrl } from "./index.js";

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

            const sectionSlug = section.interstitialPage.fields.slug;
            cy.findSectionLink(section).click();
                
            cy.url().should("include", sectionSlug);

            // Validate interstititial page content
            const interstitialPage = FindPageForSlug({ slug: sectionSlug, dataMapper });
            ValidatePage(sectionSlug, interstitialPage);
        });

        it(`${section.name} should have every question with correct content`, () => {
            // Conduct self assessment according to path
            cy.get("a.govuk-button.govuk-link").contains("Continue").click();
            validateQuestionPages(path, section, validateAnswersHintAndUrl);
        });

        it(`${section.name} should have Check Answers page with correct content`, () => {
            // Validate check answers page (questions and answers correspond to those listed in path) and return to self assessment page
            validateCheckAnswersPage(path, section);
            cy.url().should("include", selfAssessmentSlug);
        });

        it(`${section.name} should should show recent completion tags on self-assessment page`, () => {
            // Validate self-assessment page post-section completion
            validateCompletionTags(section);
        });
    }
}
