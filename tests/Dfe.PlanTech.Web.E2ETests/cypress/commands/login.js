/**
 * Login to DFE
 */
Cypress.Commands.add("login", ({ email, password, url }) => {
    Cypress.on('uncaught:exception', (err, runnable) => {
        // returning false here prevents Cypress from
        // failing the test
        return false
    });
    
  cy.visit('/');
  cy.get("a.dfe-header__link.dfe-header__link--service").click();

  cy.origin(
    Cypress.env('DSi_Url'),
    { args: { email, password } },
    ({ email, password }) => {
        cy.get("input#username").type(email);
        cy.get("input#password").type(password);
        cy.get("div.govuk-button-group button.govuk-button").first().click();
    }
  )
  cy.url().should('contain', url);

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
        () =>  cy.login(args),
        {
            cacheAcrossSpecs: true,
        }
    )

    cy.visit(url);
    cy.url().should('contain', url);
});