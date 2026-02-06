import { CleanText, submitToSelfAssessmentButtonText, checkAnswersSlug } from '../helpers/index.js';

export const validateCheckAnswersPage = (path, section) => {
  cy.url().should('include', `${section.interstitialPage.fields.slug}${checkAnswersSlug}`);

  for (const question of path) {
    cy.get('div.govuk-summary-list__row dt.govuk-summary-list__key.spacer')
      .contains(CleanText(question.question.text))
      .siblings('dd.govuk-summary-list__value.spacer')
      .contains(CleanText(question.answer.text));
  }

  cy.get('button.govuk-button').contains(submitToSelfAssessmentButtonText).click();
};
