import { validateRecommendationChunks, validateRecommendationIntro } from "./index.js";

/**
 * Validates a recommendation for a specific maturity level.
 *
 * @param {Object} section - the section containing the recommendations
 * @param {string} maturity - the maturity level to validate
 * 
 */


export const validateAndTestRecommendations = (section, maturity, path) => {
    const sectionSlug = section.interstitialPage.fields.slug

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

    const recommendationUrl = `${sectionSlug}/recommendation` 

    it(`${section.name} should retrieve recommendation intro for ${maturity} maturity, with correct content`, () => {
        validateRecommendationIntro(introPage, recommendationUrl);
    });

    it(`${section.name} should retrieve correct recommendation chunks with correct content`, () => {
        validateRecommendationChunks(introPage, chunks);
    });
}
