const SELF_ASSESSMENT_URL = Cypress.env("URL") + "/self-assessment";

Cypress.Commands.add("login", (email, password, url) => {
    cy.visit(Cypress.env("URL"));

    cy.visit(url);

    cy.session([email, password, url], () => {
        cy.origin(Cypress.env("DSiUrl"), { args: { email, password } },({ email, password }) => {
            cy.get("input#username").type(email);
            cy.get("input#password").type(password);
            cy.get("div.govuk-button-group button.govuk-button").click();
        });

    }, {
        cacheAcrossSpecs: true
    });
});

Cypress.Commands.add("loginWithEnv", (url) => cy.login(Cypress.env("Username"), Cypress.env("Password"), url));