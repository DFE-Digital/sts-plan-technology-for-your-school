const selectFirstRadioButton = () => cy.get("form div.govuk-radios div.govuk-radios__item input")
  .first()
  .click();

const saveAndContinue = () => cy.get("form button.govuk-button")
  .contains("Save and continue")
  .click();

Cypress.Commands.add("selectFirstRadioButton", selectFirstRadioButton);
Cypress.Commands.add("saveAndContinue", saveAndContinue);
