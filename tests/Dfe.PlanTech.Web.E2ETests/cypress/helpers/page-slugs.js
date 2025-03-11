export const selfAssessmentSlug = "/self-assessment";

export const FindPageForSlug = ({ slug, dataMapper }) => {
    for (const [, page] of dataMapper.pages.entries()) {
        if (page.fields.slug == slug) {
            return page;
        }
    }
    return null;
}

export const recommendationIntroFragment = "#overview";
export const checkAnswersSlug = "/check-answers";
export const recommendationSlug = "/recommendation";
export const cookiesSlug = "/cookies";
export const printRecommendationsSlug = "/print";
