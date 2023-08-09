Cypress.Commands.add("login", ({ email, password, url }) => {
    cy.visit(url);
    cy.get("input#username").type(email);
    cy.get("input#password").type(password);
    cy.get("div.govuk-button-group button.govuk-button").first().click();

    cy.origin(Cypress.env("URL"), { args: { url } }, ({ url }) => {
        cy.url().should("contain", url);
    });
});

Cypress.Commands.add("loginWithEnv", (url) => {
    const args = {
        email: Cypress.env("Username"),
        password: Cypress.env("Password"),
        url
    };

    cy.session(
        args.email,
        () => {
            cy.login(args);
        },
        {
            cacheAcrossSpecs: true,
        }
    )

    cy.visit(url);
});