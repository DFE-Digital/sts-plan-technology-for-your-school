const navigateToRecommendationPage = () => {
  let selectedQuestionsWithAnswers = []; // Not needed by the calling class so just define it here.
  return cy
    .navigateToCheckAnswersPage(selectedQuestionsWithAnswers)
    .then((res) => cy.wrap(res))
    .then(() => cy.submitAnswers())
    .then((onSelfAssessmentPage) => {
      if (!onSelfAssessmentPage) {
        return Promise.resolve();
      }

      cy.get('a[href*="/recommendation/"]').first().click();
    });
};

const navigateThroughSection = () => {
  let selectedQuestionsWithAnswers = []; // Not needed by the calling class so just define it here.
  return cy
    .navigateToCheckAnswersPage(selectedQuestionsWithAnswers)
    .then((res) => cy.wrap(res))
    .then(() => cy.submitAnswers())
    .then((onSelfAssessmentPage) => {
      if (!onSelfAssessmentPage) {
        return Promise.resolve();
      }
    });
};

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
Cypress.Commands.add("navigateToCheckAnswersPage", navigateToCheckAnswersPage);
Cypress.Commands.add("navigateThroughQuestions", navigateThroughQuestions);
Cypress.Commands.add("submitAnswers", submitAnswers);
Cypress.Commands.add("navigateThroughSection", navigateThroughSection);
