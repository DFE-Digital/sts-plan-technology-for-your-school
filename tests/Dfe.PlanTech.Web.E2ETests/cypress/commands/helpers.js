/**
 * Click first section link on self-assessment page
 */
export const clickContinueButton = () => {
  cy.get('a.govuk-button').contains('Continue').click();
}

Cypress.Commands.add("clickContinueButton", clickContinueButton);

