import {
  CleanText,
  getSubmissionTimeText,
  viewRecommendationButtonText,
  newTagText,
} from '../helpers/index.js';

export const validateCompletionTags = (section) => {
  const time = new Date();
  const timePlusOneMinute = new Date(time).setMinutes(time.getMinutes() + 1);
  const timeMinusOneMinute = new Date(time).setMinutes(time.getMinutes() - 1);
  const lateTime = new Date(timePlusOneMinute);
  const earlyTime = new Date(timeMinusOneMinute);

  cy.get('a.govuk-link')
    .contains(section.name.trim())
    .should('have.attr', 'href')
    .and('include', `${section.name.trim().toLowerCase().replace(/ /g, '-')}`);

  const sectionSlug = section.interstitialPage.fields.slug;
  cy.findSectionLink(section.name, sectionSlug)
    .parent()
    .next()
    .within(() => {
      cy.get('strong.app-task-list__tag')
        .invoke('text')
        .then((text) => {
          cy.wrap(CleanText(text)).should('be.oneOf', [
            getSubmissionTimeText(time),
            getSubmissionTimeText(lateTime),
            getSubmissionTimeText(earlyTime),
          ]);
        });
    });

  cy.findSectionLink(section.name, sectionSlug)
    .parent()
    .next()
    .next()
    .within(() => {
      cy.get('strong.app-task-list__tag')
        .should('include.text', newTagText)
        .and('have.class', 'govuk-tag--yellow');
      cy.get('a.govuk-button').contains(viewRecommendationButtonText).should('have.attr', 'href');
    });
};
