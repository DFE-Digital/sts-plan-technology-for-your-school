import { selfAssessmentPageNewRecommendation } from "./completion-tags-validator.js";

/**
 * Validates a recommendation for a specific maturity level.
 *
 * @param {Object} section - the section containing the recommendations
 * @param {string} maturity - the maturity level to validate
 */

export const validateRecommendationForMaturity = (section, maturity) => {
    selfAssessmentPageNewRecommendation(section, maturity);
}