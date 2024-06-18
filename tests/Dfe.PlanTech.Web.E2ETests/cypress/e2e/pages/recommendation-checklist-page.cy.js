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
        .contains("Share or download this recommendation in a checklist format")
        .then(($a) => {
          expect($a).to.have.attr('target','_blank')
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
      cy.get("div.recommendation-piece-content").should("exist");
  });

  it("Should have a banner showing printout time", () => {
    cy.get("#printed-date-time").contains("Date and time this document was printed");
  });

  it("Should have a print button", () => {
      cy.get("#recommendations-print-page").should("exist");
  });

  it("Should have a close button", () => {
      cy.get("#recommendations-close-page").should("exist");
  });

  //Accessibility
  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
