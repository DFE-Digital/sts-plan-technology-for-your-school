describe("landing page", () => {
    const url = "/self-assessment";

    beforeEach(() => {
        cy.loginWithEnv(url);
        cy.clickFirstSection();
    });

    it("should have content", () => {
        cy.get("rich-text").should("exist");
    });

    it("should have button which links to a question", () => {
        cy.get("a.govuk-button.govuk-link").should("exist");
        cy.get("a.govuk-button.govuk-link").should("have.attr", "href").and("include", "question");
    });

    it("should link back to self-assessment", () => {
        cy.get("a.govuk-back-link").should("exist");
        cy.get("a.govuk-back-link").should("have.attr", "href").and("include", "self-assessment");
    });
});
