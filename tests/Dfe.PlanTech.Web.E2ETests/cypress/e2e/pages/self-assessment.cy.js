describe("Self-assessment page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);

    cy.injectAxe();
  });

  it("should have heading", () => {
    cy.get("h1.govuk-heading-xl")
      .should("exist")
      .and("have.text", "Technology self‑assessment");
  });

  it("should contain categories", () => {
    cy.get("h2.govuk-heading-m").should("exist");
  });

  it("should contain sections", () => {
    cy.get("ul.app-task-list__items > li")
      .should("exist")
      .and("have.length.greaterThan", 1);
  });

  it("each section should link to a page", () => {
    cy.get("ul.app-task-list__items > li a").each((link) => {
      cy.wrap(link).should("have.attr", "href").and("not.be.null");
    });
  });

  it("passes accessibility tests", () => {
    cy.injectAxe();
    cy.runAxe();
  });

  it("should show recommendation banner", () => {
    cy.clickFirstSection();
    cy.clickContinueButton();
    cy.navigateThroughSection();

    cy.get(".govuk-notification-banner__content").should("exist");

    cy.get(".govuk-notification-banner__content a")
      .should("have.attr", "href")
      .and("include", "#recommendations-section");
  });
});
