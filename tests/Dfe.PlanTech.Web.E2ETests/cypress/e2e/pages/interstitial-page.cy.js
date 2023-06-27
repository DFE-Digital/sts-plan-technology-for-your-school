describe("landing page", () => {
    const url = Cypress.env("URL") + "/broadband-connection";
    const selfassessmentUrl = Cypress.env("URL") + "/self-assessment";

    it("should have content", () => {
        cy.loginWithEnv(url);

        cy.origin(Cypress.env("URL"), () => {
            cy.get("rich-text").should("exist");
        });
    });

    it("should have button which links to a question", () => {
        cy.loginWithEnv(url);

        cy.origin(Cypress.env("URL"), () => {
            cy.get("a.govuk-button.govuk-link").should("exist");
            cy.get("a.govuk-button.govuk-link").should("have.attr", "href").and("include", "question");
        });
    });

    it("should link back to self-assessment", () => {
        cy.loginWithEnv(selfassessmentUrl);

        cy.origin(Cypress.env("URL"), () => {

            cy.get("ul.app-task-list__items  a").first().click();

            cy.get("a.govuk-back-link").should("exist");
            cy.get("a.govuk-back-link").should("have.attr", "href").and("include", "self-assessment");
        });
    });
});
