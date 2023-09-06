describe("Check answers page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);

    navigateToCheckAnswersPage();

    cy.url().should("contain", "check-answers");

    cy.injectAxe();
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });
});

const navigateToCheckAnswersPage = () => {
  cy.clickFirstSection();
  cy.clickContinueButton();

  navigateThroughQuestions();
}

const navigateThroughQuestions = () =>
  cy.get("main").then(($main) => $main.find("form div.govuk-radios").length > 0)
    .then((onQuestionPage) => {
      if (!onQuestionPage) {
        cy.log("no longer on question page");
        return Promise.resolve();
      }

      cy.log("On question page");

      cy.selectFirstRadioButton();
      cy.saveAndContinue();

      return navigateThroughQuestions();
    });
