import { changeAnswerText, newTagText } from "../../helpers/constants";
import { checkAnswersSlug, recommendationSlug } from "../../helpers/page-slugs";

let selectedQuestionsWithAnswers = [];
let changeLinkHref;

describe("Check answers page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);

    cy.clickFirstSection();
    cy.clickContinueButton();

    cy.navigateToCheckAnswersPage(selectedQuestionsWithAnswers);

    cy.log(selectedQuestionsWithAnswers);

    cy.url().should("contain", checkAnswersSlug);

    cy.injectAxe();
  });

  it("should show each selected question with answer", () => {
    cy.get("div.govuk-summary-list__row")
      .should("exist")
      //.and("have.length", selectedQuestionsWithAnswers.length)
      .each((row) => {
        //Get question and answer tecxt for each row
        const questionWithAnswer = {
          question: null,
          answer: null,
        };

        cy.wrap(row)
          .find("dt.govuk-summary-list__key.spacer")
          .should("exist")
          .invoke("text")
          .then((question) => (questionWithAnswer.question = question.trim()));

        cy.wrap(row)
          .find("dd.govuk-summary-list__value.spacer")
          .invoke("text")
          .then((answer) => (questionWithAnswer.answer = answer.trim()));

        //Ensure it matches one of the items in array
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

          //Has "Change" me link with accessibility attributes
          cy.wrap(row)
            .find("a")
            .contains(changeAnswerText)
            .and("contain", questionWithAnswer.question)
            .get('span[class="govuk-visually-hidden"]')
            .invoke("text")
            .should("have.length.greaterThan", 1);

          cy.wrap(row)
            .find("a")
            .contains(changeAnswerText)
            .and("have.attr", "title")
            .then((title) =>
              expect(title.trim()).to.equal(questionWithAnswer.question)
            );
        });
      });
  });

  it("Should have a print button", () => {
      cy.get("#print-page-button").should("exist");
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });

  it("navigates to correct page when clicking change", () => {
    cy.get("a:nth-child(1)")
      .contains(changeAnswerText)
      .invoke("attr", "href")
      .then((href) => {
        changeLinkHref = href;
        cy.log("Captured href: " + changeLinkHref);

        cy.get("a:nth-child(1)").contains("Change").click();

        cy.url().should("contain", changeLinkHref);
      });
  });

  it("submits answers, and can go directly to the recommendation page", () => {
    cy.url().then(url => {
        let sectionSlug = url.split("/")[3];
        cy.submitAnswersAndGoToRecommendation();
        cy.url().should("contain", `${sectionSlug}${recommendationSlug}/`);
    });
  })

  //This needs to be last on this test run, so that the question-page tests have a clean slate to work from!
  it("submits answers, shows notification and preserves notification", () => {
    cy.submitAnswersAndGoToSelfAssessment();

    cy.get(".govuk-tag--yellow")
      .should("exist")
      .and("contain", newTagText);

    cy.reload();

    cy.get(".govuk-tag--yellow")
      .should("exist")
      .and("contain", newTagText);
  });
});
