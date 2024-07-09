import ValidatePage from "../../../helpers/content-validators/page-validator.js";
import { FindPageForSlug, selfAssessmentSlug } from "../../../helpers/page-slugs.js";
import { dataLoaded } from "../dpv-helpers/data-loaded-check.js";

export const validateSelfAssessmentPageOnStart = (dataMapper) => {
    if (!dataMapper || !dataLoaded(dataMapper.pages)) {
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