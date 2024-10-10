/**
 * Click first section link on self-assessment page
 */

Cypress.Commands.add("checkSectionStatus", (sectionName, selfAssessmentSlug) => {
    let inProgress = false;
    cy.visit(`${selfAssessmentSlug}`)
    cy.get("a.govuk-link")
        .contains(sectionName.trim())
        .parent()
        .next()
        .within(() => {
            cy.get("strong.app-task-list__tag").invoke("text")
                .then((text) => {
                    inProgress = text.includes("in progress");
                });
        })
        .then(() => {
            return cy.wrap(inProgress);
        });
});
