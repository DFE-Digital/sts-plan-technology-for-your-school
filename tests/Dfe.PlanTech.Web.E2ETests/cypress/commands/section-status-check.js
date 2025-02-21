/**
 * Click first section link on self-assessment page
 */

Cypress.Commands.add("checkSectionStatus", (sectionName, sectionSlug, selfAssessmentSlug) => {
    let inProgress = false;
    cy.visit(`${selfAssessmentSlug}`)

    cy.findSectionLink(sectionName, sectionSlug)
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
