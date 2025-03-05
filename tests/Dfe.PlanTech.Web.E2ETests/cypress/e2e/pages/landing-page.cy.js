import { selfAssessmentSlug } from "../../helpers/page-slugs";

describe("landing page", () => {
  beforeEach(() => {
    cy.visit("/");
    cy.injectAxe();
  });

  it("should contain title", () => {
    cy.get("h1.govuk-heading-xl").should("exist");
  });

  it("should contain headings", () => {
    cy.get("h2.govuk-heading-l")
      .should("exist")
      .and("have.length.of.at.least", 2);
  });

  it("should contain text bodies", () => {
    cy.get("p").should("exist").and("have.length.of.at.least", 4);
  });

  it("should have unordered list", () => {
    cy.get("ul").should("exist");
  });

  it("should have list items", () => {
    cy.get("ul li").should("exist").and("have.length.of.at.least", 4);
  });

  it("should have button", () => {
    cy.get("a.govuk-button--start.govuk-button").should("exist");
  });

  it("should direct to self-assessment page", () => {
    cy.get("a.govuk-button--start.govuk-button")
      .and("have.attr", "href")
      .and("equal", selfAssessmentSlug);
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });
});
