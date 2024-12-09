describe("not found", () => {
    beforeEach(() => {
        cy.visit("/some-slug-that-doesnt-exist");
        cy.injectAxe();
    });

    it("should contain heading", () => {
        cy.get("h1.govuk-heading-xl")
            .should("exist")
            .and("contain", "Page not found");
    });

    it("should contain text bodies", () => {
        cy.get("p")
            .should("exist")
            .and("have.length.of.at.least", 3);
    });

    it("should have contact us link", () => {
        cy.get("a").should("exist").and("contain", "contact us");
    });
});
