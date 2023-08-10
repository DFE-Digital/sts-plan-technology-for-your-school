/**
 * Click first section link on self-assessment page
 */
const clickFirstSection = () => {
    cy.get("ul.app-task-list__items > li a")
        .first()
        .click();
}

Cypress.Commands.add("clickFirstSection", () => clickFirstSection());

