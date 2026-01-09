import { continueButtonText } from '../helpers/constants';

const selectFirstRadioButton = () => {
  const questionWithAnswer = {
    question: '',
    answer: '',
  };

  cy.get('main form')
    .then(($form) => {
      cy.wrap($form)
        .find('h1')
        .should('exist')
        .invoke('text')
        .then((question) => (questionWithAnswer.question = question.trim()));

      cy.wrap($form)
        .find('div.govuk-radios div.govuk-radios__item')
        .should('exist')
        .and('length.of.at.least', 2)
        .first()
        .then((item) => {
          cy.wrap(item)
            .find('label')
            .invoke('text')
            .then((answer) => (questionWithAnswer.answer = answer.trim()));

          cy.wrap(item).find('input', { force: true }).should('exist');

          cy.wrap(item).find('label').click();
        });
    })
    .then(() => cy.wrap(questionWithAnswer));
};

const saveAndContinue = () =>
  cy.get('form button.govuk-button').contains(continueButtonText).click();

Cypress.Commands.add('selectFirstRadioButton', selectFirstRadioButton);
Cypress.Commands.add('saveAndContinue', saveAndContinue);
