const selectFirstRadioButton = () => {
  const questionWithAnswer = {
    question: "",
    answer: "",
  };

  cy.get("main form")
    .then(($form) => {
      cy.wrap($form)
        .find("h1")
        .should("exist")
        .invoke("text")
        .then((question) => (questionWithAnswer.question = question.trim()));

      cy.wrap($form)
        .find("div.govuk-radios div.govuk-radios__item")
        .should("exist")
        .and("length.of.at.least", 2)
        .first()
        .then((item) => {
          cy.wrap(item)
            .find("label")
            .invoke("text")
            .then((answer) => (questionWithAnswer.answer = answer.trim()));

          cy.wrap(item).find("input", {force: true}).should("exist");

          cy.wrap(item).find("label").click();
        });
    })
    .then(() => cy.wrap(questionWithAnswer));
};

const saveAndContinue = () =>
  cy.get("form button.govuk-button").contains("Save and continue").click();

const assertCopiedToClipboard = (text) => {
  cy.window().then(win => {
    win.navigator.clipboard.readText().then(text => {
      expect(value).to.eq(text)
    })
  })
}

Cypress.Commands.add("selectFirstRadioButton", selectFirstRadioButton);
Cypress.Commands.add("saveAndContinue", saveAndContinue);
Cypress.Commands.add("assertCopiedToClipboard", assertCopiedToClipboard)
