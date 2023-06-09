describe("landing page", () => {
  const url = Cypress.env("URL") + "/self-assessment";

  beforeEach(() => {
    cy.visit(url);
  });

  it("should have heading", () => {
    cy.get("h1.govuk-heading-xl")
      .should("exist")
      .and("have.text", "Self assessment");
  });
});
