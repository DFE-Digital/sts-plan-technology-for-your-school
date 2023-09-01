describe("Accessibility Page", () => {
    const url = "/self-assessment";

    beforeEach(() => {
        cy.loginWithEnv(url);
        cy.get("footer.govuk-footer ul.govuk-footer__inline-list a.govuk-footer__link").contains("Accessibility").click();
        cy.url().should("contain", "/accessibility");
        cy.injectAxe();
    });

    it("Should Have Heading", () => {
        cy.get("h1.govuk-heading-xl")
            .should("exist")
            .and("have.text", "Accessibility")
    });

    it("Should Have Home Button", () => {
        cy.get('a:contains("Home")')
            .should("exist")
            .should("have.attr", "href")
            .and("include", "self-assessment")
    });

    it("Should Have Content", () => {
        cy.get("rich-text").should("exist");
    });

    it("Passes Accessibility Testing", () => {
        cy.runAxe();
    });
});