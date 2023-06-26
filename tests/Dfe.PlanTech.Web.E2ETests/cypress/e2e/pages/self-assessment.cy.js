describe("landing page", () => {
  const url = Cypress.env("URL") + "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);
  });

  it("should have heading", () => {
    cy.origin(Cypress.env("URL"), () => {
      cy.get("h1.govuk-heading-xl")
        .should("exist")
        .and("have.text", "Technology self-assessment");
    });
  });

  it("should contain categories", () => {
    cy.origin(Cypress.env("URL"), () => {
      cy.get("h2.govuk-heading-m").should("exist");
    });
  });

  it("should contain sections", () => {
    cy.origin(Cypress.env("URL"), () => {
      cy.get("ul.app-task-list__items > li")
        .should("exist")
        .and("have.length.greaterThan", 1);
    });
  });

  it("each section should link to a question", () => {
    cy.origin(Cypress.env("URL"), () => {
      cy.get("ul.app-task-list__items > li a").each((link) => {
        cy.wrap(link)
          .should("have.attr", "href")
          .and("match", /\/question/);
      });
    });
  });
});
