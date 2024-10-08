import "./commands.js";
import 'cypress-axe';

Cypress.on("fail", (e, runnable) => {
    cy.log("runnable", runnable.title);
    cy.log("message", e.message);
    cy.log("error", e);
});
