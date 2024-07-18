import ValidatePage from "../../../helpers/content-validators/page-validator.js";
import { selfAssessmentSlug, FindPageForSlug } from "../../../helpers/page-slugs.js";
import { validateCheckAnswersPage } from "./check-answers-validator.js";
import { navigateAndValidateQuestionPages } from "./questions-validator.js";

/**
 * Validates sections using the given paths
 *
 * @param {Object} section - the section to validate
 * @param {Array} paths - the paths to navigate
 * @param {Function} validator - optional validation function to call at the end of every path
 */

export const validateSections = (section, paths, dataMapper, validator) => {
    cy.visit(`/${selfAssessmentSlug}`);

    for (const path of paths) {

        // Navigate through interstitial page
        cy.get("div.govuk-summary-list__row > dt a").contains(section.name).click();
        cy.url().should("include", section.interstitialPage.fields.slug);

        // Validate interstititial page content
        const interstitialPage = FindPageForSlug({ slug: section.interstitialPage.fields.slug, dataMapper });
        ValidatePage(section.interstitialPage.fields.slug, interstitialPage);

        // Conduct self assessment according to path
        cy.get("a.govuk-button.govuk-link").contains("Continue").click();
        navigateAndValidateQuestionPages(path, section);

        // Validate check answers page (questions and answers correspond to those listed in path) and return to self assessment page
        validateCheckAnswersPage(path, section);
        cy.url().should("include", selfAssessmentSlug);

        // Call recommendations validation function(s) if applicable
        if (validator) {
            validator(path);
        }
    }
}