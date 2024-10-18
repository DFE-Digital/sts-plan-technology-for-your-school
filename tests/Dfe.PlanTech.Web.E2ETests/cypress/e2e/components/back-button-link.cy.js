describe("Back button link", () => {
  beforeEach(() => {
    cy.visit("/");
    cy.visit("/cookies");
  });

  it("Displays a single back button, with correct 'Back' text'", () => {
    cy.get("a.govuk-back-link").should("exist").and("be.visible").should('have.length', 1).invoke("text").should('equal', "Back");
  });
});
