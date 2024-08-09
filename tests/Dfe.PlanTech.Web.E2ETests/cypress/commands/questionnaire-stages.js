const navigateToRecommendationPage = () => {
    cy.get('a[href*="/recommendation/"]').first().click();
};

const navigateToSecondRecommendationPage = () => {
  cy.get('a[href*="/recommendation/"]').eq(1).click();
};

const completeFirstSubtopic = () => {
  let selectedQuestionsWithAnswers = [];
    cy.clickFirstSection();
    cy.clickContinueButton();

  return cy
    .navigateToCheckAnswersPage(selectedQuestionsWithAnswers)
    .then((res) => cy.wrap(res))
    .then(() => cy.submitAnswers());
}

const completeSecondSubtopic = () => {
  let selectedQuestionsWithAnswers = [];
    cy.clickSecondSection();
    cy.clickContinueButton();

  return cy
    .navigateToCheckAnswersPage(selectedQuestionsWithAnswers)
    .then((res) => cy.wrap(res))
    .then(() => cy.submitAnswers());
}

const navigateToCheckAnswersPage = (selectedQuestionsWithAnswers) => {
  return cy
    .navigateThroughQuestions(selectedQuestionsWithAnswers)
    .then(() => cy.wrap(selectedQuestionsWithAnswers));
};

const navigateThroughQuestions = (selectedQuestionsWithAnswers) => {
  return cy
    .get("main")
    .then(($main) => $main.find("form div.govuk-radios").length > 0)
    .then((onQuestionPage) => {
      if (!onQuestionPage) {
        return Promise.resolve();
      }

      cy.selectFirstRadioButton().then((questionWithAnswer) =>
        selectedQuestionsWithAnswers.push(questionWithAnswer)
      );
      cy.saveAndContinue();

      return navigateThroughQuestions(selectedQuestionsWithAnswers);
    })
    .then(() => cy.wrap(selectedQuestionsWithAnswers));
};

const submitAnswers = () =>
  cy.get("button.govuk-button").contains("Save and continue").click();

Cypress.Commands.add(
  "navigateToRecommendationPage",
  navigateToRecommendationPage
);
Cypress.Commands.add(
  "navigateToSecondRecommendationPage",
  navigateToSecondRecommendationPage
);
Cypress.Commands.add("completeFirstSubtopic", completeFirstSubtopic);
Cypress.Commands.add("completeSecondSubtopic", completeSecondSubtopic);
Cypress.Commands.add("navigateToCheckAnswersPage", navigateToCheckAnswersPage);
Cypress.Commands.add("navigateThroughQuestions", navigateThroughQuestions);
Cypress.Commands.add("submitAnswers", submitAnswers);
