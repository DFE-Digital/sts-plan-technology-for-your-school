import { CleanText } from '../text-helpers.js';
function ValidateTitle(title) {
  const expectedText = CleanText(title.fields.text);

  cy.get('h1.govuk-heading-xl').should('exist').and('have.text', expectedText);
}

export default ValidateTitle;
