const PARENT_SELECTOR = "details.govuk-details";

describe("dropdown", () => {
  const url = Cypress.env("/");

  beforeEach(() => {
    cy.visit(url);
  });

  it("should exist on page", () => {
    cy.get(PARENT_SELECTOR).should("exist");
  });

  it("should not be expanded on start", () => {
    cy.get(PARENT_SELECTOR).should("not.have.attr", "open");
  });

  it("should expand once clicked", () => {
    cy.get(PARENT_SELECTOR).then(($parent) => {
      //Get title and click

      cy.wrap($parent).get("span.govuk-details__summary-text").click();
      cy.wrap($parent).should("have.attr", "open");
    });
  });
});
