/**
 * Click first section link on self-assessment page
 */
export const clickFirstSection = () => {
    cy.get("div.govuk-summary-list__row > dt a")
        .first()
        .click();
}
/**
 * Click second section link on self-assessment page
 */
export const clickSecondSection = () => {
    cy.get("div.govuk-summary-list__row > dt a")
        .eq(1)
        .click();
}

Cypress.Commands.add("clickFirstSection", clickFirstSection);
Cypress.Commands.add("clickSecondSection", clickSecondSection);
