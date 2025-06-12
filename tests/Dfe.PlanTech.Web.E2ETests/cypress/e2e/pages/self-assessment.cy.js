/*
import { selfAssessmentSlug } from "../../helpers/page-slugs.js";

describe("Self-assessment page", () => {

  beforeEach(() => {
    cy.loginWithEnv(selfAssessmentSlug);
  });

  it("should have heading", () => {
    cy.get("h1.govuk-heading-xl").should("exist");
  });

  it("should contain categories", () => {
    cy.get("h2.govuk-heading-m").should("exist");
  });

  it("should contain sections", () => {
    cy.get("div.govuk-summary-list__row > dt")
      .should("exist")
      .and("have.length.greaterThan", 1);
  });

  it("each section should link to a page", () => {
    cy.get("div.govuk-summary-list__row > dt a").each((link) => {
      cy.wrap(link).should("have.attr", "href").and("not.be.null");
    });
  });

  it("passes accessibility tests", () => {
    cy.injectAxe();
    cy.runAxe();
  });
});
*/
