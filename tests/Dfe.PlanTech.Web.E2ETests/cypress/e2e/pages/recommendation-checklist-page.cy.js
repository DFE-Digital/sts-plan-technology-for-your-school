/*
import { printAllRecommendationsText, printDateTime, backText, printRecommendationsAnswerHeader } from "../../helpers/constants";
import { selfAssessmentSlug, printRecommendationsSlug } from "../../helpers/page-slugs"; 

describe("Recommendation Checklist Page", () => {

  before(() => {
    cy.loginWithEnv(selfAssessmentSlug);
    cy.completeFirstSubtopic();
  });

  beforeEach(() => {
    cy.loginWithEnv(selfAssessmentSlug);
    cy.navigateToRecommendationPage();
    cy.get("a.govuk-link")
        .contains(printAllRecommendationsText)
        .then(($a) => {
          $a.attr('target', '_self')
        })
        .click()
    cy.url().should("contain", printRecommendationsSlug);
    cy.injectAxe();
  });

  //Structure elements
  it("Should not have DfE header", () => {
    cy.get("header.dfe-header").should("not.exist");
  });

  it("Should not have Gov.uk footer", () => {
      cy.get("footer.govuk-footer").should("not.exist");
  });

  it("Should have Content", () => {
      cy.get("div.recommendation-action-header").should("exist");
      cy.get("div.recommendation-action-content").should("exist");
  });

  it("Should have a banner showing printout time", () => {
    cy.get("#printed-date-time").contains(printDateTime);
  });

  it("Should show answers to questions", () => {
      cy.get("#checkYourAnswers-page").should("exist").within(() => {
          cy.get("h1").contains(printRecommendationsAnswerHeader);
          cy.get("div.govuk-summary-list__row").should("exist");
          cy.get("dt.govuk-summary-list__key").should("exist");
          cy.get("dd.govuk-summary-list__value").should("exist");
      });
   });

  it("Should have a print button", () => {
      cy.get("#print-page-button").should("exist");
  });

  it("Should Have Back Button", () => {
    cy.get(`a:contains(${backText})`)
      .should("exist")
      .should("have.attr", "href")
      .and("include", "/");
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });
});
*/
