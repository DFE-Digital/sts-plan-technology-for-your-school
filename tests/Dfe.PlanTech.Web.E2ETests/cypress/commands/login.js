/**
 * Login to DFE
 */
Cypress.Commands.add("login", ({ email, password, url }) => {
    Cypress.on('uncaught:exception', () => {
        // returning false here prevents Cypress from
        // failing the test
        return false
    });

  cy.visit(url);
  cy.get("input#username").type(email);
  cy.get("button.govuk-button").first().click();
  cy.get("input#password", { timeout: 4000 }).type(password);
    cy.get("div.govuk-button-group button.govuk-button").first().click();
    cy.wait(4000);
});

/**
 * Login to DFE using values from the environment variables
 */
Cypress.Commands.add("loginWithEnv", (url) => {
    const args = {
        email: Cypress.env("DSi_Email"),
        password: Cypress.env("DSi_Password"),
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
    .then(() => {
        cy.visit(url);
    })
});
