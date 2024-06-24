/**
 * Click first section link on self-assessment page
 */
export const clickFirstSection = () => {
    cy.get("div.govuk-summary-list__row > dt a")
        .first()
        .click();
}

Cypress.Commands.add("clickFirstSection", clickFirstSection);

