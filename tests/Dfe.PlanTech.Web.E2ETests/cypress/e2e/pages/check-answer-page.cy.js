let selectedQuestionsWithAnswers = [];

describe("Check answers page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    selectedQuestionsWithAnswers = [];
    cy.loginWithEnv(url);

    navigateToCheckAnswersPage();

    cy.log(selectedQuestionsWithAnswers);

    cy.url().should("contain", "check-answers");

    cy.injectAxe();
  });

  it("submits answers", () => {
    submitAnswers();

    cy.url().should("contain", "self-assessment");
  });

  it("should show each selected question with answer", () => {
    cy.get("div.govuk-summary-list__row")
      .should("exist")
      .and("have.length", selectedQuestionsWithAnswers.length)
      .each((row) => {
        const questionWithAnswer = {
          question: null,
          answer: null,
        };

        cy.wrap(row)
          .find("dt.govuk-summary-list__key")
          .should("exist")
          .invoke("text")
          .then((question) => (questionWithAnswer.question = question.trim()));

        cy.wrap(row)
          .find("dd.govuk-summary-list__value")
          .invoke("text")
          .then((answer) => (questionWithAnswer.answer = answer.trim()));

        cy.wrap(questionWithAnswer).then(() => {
          cy.log(JSON.stringify(selectedQuestionsWithAnswers));

          const matchingQuestionWithAnswer = selectedQuestionsWithAnswers.find(
            (qwa) => {
              cy.log("looking for ", qwa);
              return qwa.question == questionWithAnswer.question.trim();
            }
          );

          expect(matchingQuestionWithAnswer.answer).to.equal(
            questionWithAnswer.answer
          );
        });
      });
  });

  it("shows notification banner after submitting answers", () => {
    submitAnswers();

    cy.get("div.govuk-notification-banner__header").should("exist");
  });

  it("each change answer link should have correct attributes", () => {
    cy.get("a")
      .contains("Change")
      .each((link) => {
        cy.wrap(link).should("have.attr", "aria-label");
        cy.wrap(link).should("have.attr", "title");
      });
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });
});

const navigateToCheckAnswersPage = () => {
  cy.clickFirstSection();
  cy.clickContinueButton();

  return navigateThroughQuestions();
};

const navigateThroughQuestions = () => {
  cy.get("main")
    .then(($main) => $main.find("form div.govuk-radios").length > 0)
    .then((onQuestionPage) => {
      if (!onQuestionPage) {
        return Promise.resolve();
      }

      cy.selectFirstRadioButton().then((questionWithAnswer) =>
        selectedQuestionsWithAnswers.push(questionWithAnswer)
      );
      cy.saveAndContinue();

      return navigateThroughQuestions();
    })
    .then(() => {

    });
};

const submitAnswers = () =>
  cy.get("button.govuk-button").contains("Save and Submit").click();
