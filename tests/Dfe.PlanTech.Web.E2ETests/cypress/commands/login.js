Cypress.Commands.add("login", (email, password, url) => {
    cy.visit(url);
    cy.get("input#username").type(email);
    cy.get("input#password").type(password);
    cy.get("div.govuk-button-group button.govuk-button").first().click();
});

Cypress.Commands.add("loginWithEnv", (url) => cy.login(Cypress.env("Username"), Cypress.env("Password"), url));