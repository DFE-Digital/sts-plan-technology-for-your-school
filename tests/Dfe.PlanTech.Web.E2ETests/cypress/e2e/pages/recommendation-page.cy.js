describe("Recommendation Page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);

    cy.url().should("contain", "self-assessment");

    cy.clickFirstSection();
    cy.clickContinueButton();

    cy.navigateToRecommendationPage();

    cy.url().should("contain", "recommendation");

    cy.injectAxe();
  });

  it("Should Have a part of header", () => {
    cy.get("span.app-step-nav-header__part-of").should("exist");
  });

  it("Should Have Back Button", () => {
    cy.get('a:contains("Back")')
      .should("exist")
      .should("have.attr", "href")
      .and("include", "/self-assessment");
  });

  it("Should Have Content", () => {
    cy.get("p").should("exist");
  });

  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
