import { selfAssessmentPageNewRecommendation } from "./completion-tags-validator.js";
import { ValidateRecommendationChunks } from "./recommendation-chunks-validator.js";
import { ValidateRecommendationIntro } from "./recommendation-intro-validator.js";

/**
 * Validates a recommendation for a specific maturity level.
 *
 * @param {Object} section - the section containing the recommendations
 * @param {string} maturity - the maturity level to validate
 * 
 */

export const validateRecommendationForMaturity = (section, maturity, path) => {
    const introPage = section.recommendation.intros.find(
        (recommendation) => recommendation.maturity == maturity
    );

    // Get chunks for path
    const chunks = section.recommendation.section.getChunksForPath(path)

    const recommendationUrl = `${section.interstitialPage.fields.slug}/recommendation` 

    // Validate self-assessment page post-section completion
    selfAssessmentPageNewRecommendation(section, maturity);

    cy.get("a.govuk-link")
        .contains(section.name.trim())
        .parent().next().next()
        .within(() => {
            cy.get("a.govuk-button").contains("View Recommendation").click();
        })

    ValidateRecommendationIntro(introPage, recommendationUrl);
    ValidateRecommendationChunks(chunks);
}