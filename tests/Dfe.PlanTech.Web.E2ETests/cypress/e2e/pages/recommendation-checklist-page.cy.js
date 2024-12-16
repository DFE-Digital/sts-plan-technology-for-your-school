describe("Recommendation Checklist Page", () => {
  const url = "/self-assessment";

  before(() => {
    cy.loginWithEnv(url);
    cy.completeFirstSubtopic();
  });

  beforeEach(() => {
    cy.loginWithEnv(url);
    cy.navigateToRecommendationPage();
    cy.get("a.govuk-link")
        .contains("View and print all recommendations")
        .then(($a) => {
          $a.attr('target', '_self')
        })
        .click()
    cy.url().should("contain", "print");
    cy.injectAxe();
  });

  //Structure elements
  it("Should not have DfE header", () => {
    cy.get("header.dfe-header").should("not.exist");
  });

  it("Should not have Gov.uk footer", () => {
      cy.get("footer.govuk-footer").should("not.exist");
  });

  it("Should have Content", () => {
      cy.get("div.recommendation-action-header").should("exist");
      cy.get("div.recommendation-action-content").should("exist");
  });

  it("Should have a banner showing printout time", () => {
    cy.get("#printed-date-time").contains("Date and time this document was printed");
  });

  it("Should have a print button", () => {
      cy.get("#print-page-button").should("exist");
  });

  it("Should Have Back Button", () => {
    cy.get('a:contains("Back")')
      .should("exist")
      .should("have.attr", "href")
      .and("include", "/");
  });

  //Accessibility
  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
