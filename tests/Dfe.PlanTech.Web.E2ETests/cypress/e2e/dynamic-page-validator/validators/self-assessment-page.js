import { FindPageForSlug, ValidatePage, dataLoaded, selfAssessmentSlug } from "../helpers/index.js";

export const validateSelfAssessmentPage = (dataMapper) => {
    if (!dataLoaded(dataMapper)) {
        return;
    }

    const slug = selfAssessmentSlug.replace("/", "");
    const selfAssessmentPage = FindPageForSlug({ slug, dataMapper });

    if (!selfAssessmentPage) {
        throw new Error(
            `Could not find self-assessment page; not found page with slug ${selfAssessmentSlug}`
        );
    }

    cy.loginWithEnv(selfAssessmentSlug);
    ValidatePage(slug, selfAssessmentPage)
}