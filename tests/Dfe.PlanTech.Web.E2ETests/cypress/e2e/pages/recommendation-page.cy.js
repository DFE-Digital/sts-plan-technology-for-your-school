describe("Recommendation Page", () => {
  const url = "/self-assessment";

  before(() => {
    cy.loginWithEnv(url);

    cy.completeFirstSubtopic();
  });
  
  beforeEach(() => {
    cy.loginWithEnv(url);
    cy.navigateToRecommendationPage();

    cy.url().should("contain", "recommendation");

    cy.injectAxe();
  });

  //Structure elements
  it("Should have a Part Of header", () => {
    cy.get("span.app-step-nav-header__part-of").should("exist");
  });

  it("Should have Content", () => {
      cy.get("div.recommendation-piece-content").should("exist");
  });

  it("Should have recommendations sidebar Part Of header", () => {
      cy.get("h2.app-step-nav-related__heading").should("exist");
  });

  it("Should have recommendations accordion", () => {
      cy.get("#step-by-step-navigation").should("exist");
  });

    it("Should have a Show All Actions chevron and text", () => {
        cy.get("span.app-step-nav__chevron").should("exist");
        cy.get("span.app-step-nav__button-text").should("exist");
    })

  //Accessibility
  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
