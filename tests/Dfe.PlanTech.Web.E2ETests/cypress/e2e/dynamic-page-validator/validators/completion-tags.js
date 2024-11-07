import { CleanText, getSubmissionTimeText } from "../helpers/index.js";

export const validateCompletionTags = (section, introPage) => {
    const time = new Date();
    const timePlusOneMinute = new Date(time).setMinutes(time.getMinutes() + 1);
    const lateTime = new Date(timePlusOneMinute);

    const expectedPath =
        `/${section.name.trim()}/recommendation/${introPage.slug.trim()}`
            .toLowerCase()
            .replace(/ /g, "-");

    cy.get("a.govuk-link")
        .contains(section.name.trim())
        .should("have.attr", "href").and("include", `${section.name.trim().toLowerCase().replace(/ /g, "-")}`);

    cy.get("a.govuk-link")
        .contains(section.name.trim())
        .parent()
        .next()
        .within(() => {
            cy.get("strong.app-task-list__tag").invoke("text")
                .then((text) => {
                    cy.wrap(CleanText(text)).should("be.oneOf", [ getSubmissionTimeText(time), getSubmissionTimeText(lateTime) ]);
                });
        })

    cy.get("a.govuk-link")
        .contains(section.name.trim())
        .parent().next().next()
        .within(() => {
            cy.get("strong.app-task-list__tag").should("include.text", "New").and("have.class", "govuk-tag--yellow");
            cy.get("a.govuk-button").contains("View recommendation")
                .should("have.attr", "href")
                .and("include", expectedPath);
        })
}
