import { inProgressText } from '../helpers/constants';

/**
 * Click first section link on self-assessment page
 */

Cypress.Commands.add('checkSectionStatus', (sectionName, sectionSlug, homePageSlug) => {
  let inProgress = false;
  cy.visit(`${homePageSlug}`);

  cy.findSectionLink(sectionName, sectionSlug)
    .parent()
    .next()
    .within(() => {
      cy.get('strong.app-task-list__tag')
        .invoke('text')
        .then((text) => {
          inProgress = text.includes(inProgressText);
        });
    })
    .then(() => {
      return cy.wrap(inProgress);
    });
});
