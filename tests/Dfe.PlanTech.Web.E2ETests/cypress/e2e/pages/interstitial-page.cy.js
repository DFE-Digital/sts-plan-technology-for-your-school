describe("landing page", () => {
    const url = Cypress.env("URL") + "/broadband-connection";
    const selfassessmentUrl = Cypress.env("URL") + "/self-assessment";

    beforeEach(() => {
        cy.loginWithEnv(url);
    });

    it("should have content", () => {
        cy.origin(Cypress.env("URL"), () => {
            cy.get("rich-text").should("exist");
        });
    });

    it("should have button which links to a question", () => {
        cy.origin(Cypress.env("URL"), () => {
            cy.get("a.govuk-button.govuk-link").should("exist");
            cy.get("a.govuk-button.govuk-link").should("have.attr", "href").and("include", "question");
        });
    });
});
