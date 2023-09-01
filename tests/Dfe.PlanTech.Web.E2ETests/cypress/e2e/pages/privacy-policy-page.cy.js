describe("Privacy Policy Page", () => {
    const url = "/privacy";

    beforeEach(() => {
        cy.visit(url);
        cy.injectAxe();
    });

    it("Should Have Heading", () => {
        cy.get("h1.govuk-heading-xl")
            .should("exist")
            .and("have.text", "Privacy Policy")
    });

    it("Should Have Home Button", () => {
        cy.get('a:contains("Home")')
            .should("exist")
            .should("have.attr", "href")
            .and("include", "/")
    });

    it("Should Have Content", () => {
        cy.get("rich-text").should("exist");
    });

    it("Passes Accessibility Testing", () => {
        cy.runAxe();
    });
});