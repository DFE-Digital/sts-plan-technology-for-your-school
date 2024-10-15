import { validateCompletionTags, validateRecommendationChunks, validateRecommendationIntro } from "./index.js";

/**
 * Validates a recommendation for a specific maturity level.
 *
 * @param {Object} section - the section containing the recommendations
 * @param {string} maturity - the maturity level to validate
 * 
 */


export const validateAndTestRecommendations = (section, maturity, path) => {
    const introPage = section.recommendation.intros.find(
        (recommendation) => recommendation.maturity == maturity
    );

    if (!introPage)
        throw new Error(
            `Couldn't find a recommendation for maturity ${maturity} in ${section.name}`,
            section
        );

    // Get chunks for path
    const chunks = section.recommendation.section.getChunksForPath(path)

    const recommendationUrl = `${section.interstitialPage.fields.slug}/recommendation` 

    it(`${section.name} should should show recent completion tags on self-assessment page`, () => {
        // Validate self-assessment page post-section completion
        validateCompletionTags(section, introPage);
    });

    it(`${section.name} should retrieve recommendation intro for ${maturity} maturity, with correct content`, () => {
        cy.get("a.govuk-link")
            .contains(section.name.trim())
            .parent().next().next()
            .within(() => {
                cy.get("a.govuk-button").contains("View Recommendation").click();
            })
        validateRecommendationIntro(introPage, recommendationUrl);
    });

    it(`${section.name} should retrieve correct recommendation chunks with correct content`, () => {
        validateRecommendationChunks(introPage, chunks);
    });
}
