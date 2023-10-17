describe("Recommendation Page", () => {
    const url = "/self-assessment";

    before(() => {
        cy.loginWithEnv(url);
    });
    
    beforeEach(() => {
        cy.visitSaPageWithRetry(url, 3);
        
        cy.url().should("contain", "self-assessment");

        cy.clickFirstSection();
        cy.clickContinueButton();

        cy.navigateToRecommendationPage();

        cy.url().should("contain", "recommendation");

        cy.injectAxe();
    });

    it("Should Have Heading", () => {
        cy.get("h1.govuk-heading-xl")
            .should("exist")
    });

    it("Should Have Back Button", () => {
        cy.get('a:contains("Back")')
            .should("exist")
            .should("have.attr", "href")
            .and("include", "/self-assessment")
    });

    it("Should Have Content", () => {
        cy.get("rich-text").should("exist");
    });

    it("Passes Accessibility Testing", () => {
        cy.runAxe();
    });

});