/**
 * Login to DFE
 */
Cypress.Commands.add("login", ({ email, password, url }) => {
    Cypress.on('uncaught:exception', (err, runnable) => {
        // returning false here prevents Cypress from
        // failing the test
        return false
    });
    
    cy.visit(url);
    cy.get("input#username").type(email);
    cy.get("input#password").type(password);
    cy.get("div.govuk-button-group button.govuk-button").first().click();

    cy.origin(Cypress.env("URL"), { args: { url } }, ({ url }) => {
        cy.url().should("contain", url);
    });
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

    cy.visit(url);
});

Cypress.Commands.add('visitSaPageWithRetry', (url, maxRetries) => {
    cy.log(`Visiting ${url}`);
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
        cy.visit(url, { failOnStatusCode: false }); 

        cy.get('h1.govuk-heading-xl')
            .should('exist')
            .and('have.text', 'Technology self‑assessment');

        if (attempt < maxRetries) {
            cy.log('navigating to page successful');
            return; 
        }
    }
    
    cy.log(`failed to navigate to page after ${maxRetries} attempts`);
    throw new Error(`Failed to visit ${url} after multiple retries`);
});