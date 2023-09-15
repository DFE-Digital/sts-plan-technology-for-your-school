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

  it("submits answers", () => {
    submitAnswers();

    cy.url().should("contain", "self-assessment");
  });

  it("shows notification banner after submitting answers", () => {
    submitAnswers();

    cy.get("div.govuk-notification-banner__header").should("exist");
  });

  it("navigates to correct page when clicking change", () => {
    cy.get("a:nth-child(1)").contains("Change").click();
    cy.url().should("contains", "broadband-contract-review");
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
function submitAnswers() {
  cy.get("button.govuk-button").contains("Save and Submit").click();
}

