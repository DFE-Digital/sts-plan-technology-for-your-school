const navigateToRecommendationPage = () => {
    cy.get('a[href*="/recommendation/"]').first().click();
};

const completeFirstSubtopic = () => {
  let selectedQuestionsWithAnswers = [];
    cy.clickFirstSection();
    cy.clickContinueButton();

  return cy
    .navigateToCheckAnswersPage(selectedQuestionsWithAnswers)
    .then((res) => cy.wrap(res))
    .then(() => cy.submitAnswersAndGoToSelfAssessment());
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

const submitAnswersAndGoToSelfAssessment = () => cy.get("button.govuk-button").contains("Submit and go to self-assessment topics").click();
const submitAnswersAndGoToRecommendation = () => cy.get("button.govuk-button").contains("Submit and view recommendation").click();

Cypress.Commands.add(
  "navigateToRecommendationPage",
  navigateToRecommendationPage
);
Cypress.Commands.add("completeFirstSubtopic", completeFirstSubtopic);
Cypress.Commands.add("navigateToCheckAnswersPage", navigateToCheckAnswersPage);
Cypress.Commands.add("navigateThroughQuestions", navigateThroughQuestions);
Cypress.Commands.add("submitAnswersAndGoToSelfAssessment", submitAnswersAndGoToSelfAssessment);
Cypress.Commands.add("submitAnswersAndGoToRecommendation", submitAnswersAndGoToRecommendation);
