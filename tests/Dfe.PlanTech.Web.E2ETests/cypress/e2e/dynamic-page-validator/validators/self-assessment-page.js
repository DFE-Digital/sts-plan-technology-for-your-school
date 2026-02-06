import { FindPageForSlug, ValidatePage, dataLoaded, homePageSlug } from '../helpers/index.js';

export const validateSelfAssessmentPage = (dataMapper) => {
  if (!dataLoaded(dataMapper)) {
    return;
  }

  const slug = homePageSlug.replace('/', '');
  const selfAssessmentPage = FindPageForSlug({ slug, dataMapper });

  if (!selfAssessmentPage) {
    throw new Error(
      `Could not find self-assessment page; not found page with slug ${homePageSlug}`,
    );
  }

  cy.loginWithEnv(homePageSlug);
  ValidatePage(slug, selfAssessmentPage);
};
