describe("landing page", () => {
  const url = Cypress.env("URL") + "/self-assessment";

  beforeEach(() => {
    cy.visit(url);
  });

  it("should have heading", () => {
    cy.get("h1.govuk-heading-xl")
      .should("exist")
      .and("have.text", "Self-Assessment");
  });

  it("should contain categories", () => {
    cy.get("h2.govuk-heading-m").should("exist");
  });

  it("should contain sections", () => {
    cy.get("ul.app-task-list__items > li")
      .should("exist")
      .and("have.length.greaterThan", 1);
  });

  it("each section should link to a question", () => {
    cy.get("ul.app-task-list__items > li a").each((link) => {
      cy.wrap(link)
        .should("have.attr", "href")
        .and("match", /\/question/);
    });
  });
});
