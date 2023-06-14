const PARENT_SELECTOR = "details.govuk-details";

describe("dropdown", () => {
  const url = Cypress.env("URL") + "/self-assessment";

  beforeEach(() => {
    cy.visit(url);
  });

  it("should navigate back to self-assessment on first question", () => {
    cy.get("ul.app-task-list__items > li a").first().click();

    cy.get("a.govuk-back-link")
      .should("exist")
      .and("have.attr", "href")
      .and("match", /\/self-assessment$/);
  });

  it("should navigate back to second question from first question", () => {
    cy.get("ul.app-task-list__items > li a").first().click();

    cy.location("href").then((href) => {
      cy.get("form div.govuk-radios div.govuk-radios__item input")
        .first()
        .click();

      cy.get("form button.govuk-button").click();

      cy.get("a.govuk-back-link")
        .should("exist")
        .and("have.attr", "href")
        .and("equal", href);
    });
  });

  it("should navigate back to self-assessment after navigating back to first question", () => {
    cy.get("ul.app-task-list__items > li a").first().click();

    cy.location("href").then((href) => {
      cy.get("form div.govuk-radios div.govuk-radios__item input")
        .first()
        .click();

      cy.get("form button.govuk-button").click();

      cy.get("a.govuk-back-link")
        .should("exist")
        .and("have.attr", "href")
        .and("equal", href);

      cy.get("a.govuk-back-link").click();

      cy.get("a.govuk-back-link")
        .should("exist")
        .and("have.attr", "href")
        .and("include", "self-assessment");
    });
  });
});
