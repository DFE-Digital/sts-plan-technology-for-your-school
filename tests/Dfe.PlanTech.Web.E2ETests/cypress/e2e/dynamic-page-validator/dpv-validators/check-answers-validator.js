export const validateCheckAnswersPage = (path, section) => {

    for (const q of path) {
        cy.get("div.govuk-summary-list__row dt.govuk-summary-list__key.spacer")
            .contains(q.question.text.trim())
            .siblings("dd.govuk-summary-list__value.spacer")
            .contains(q.answer.text);
    }

    cy.url().should("include", `${section.interstitialPage.fields.slug}/check-answers`);

    cy.get("button.govuk-button").contains("Save and continue").click();
}