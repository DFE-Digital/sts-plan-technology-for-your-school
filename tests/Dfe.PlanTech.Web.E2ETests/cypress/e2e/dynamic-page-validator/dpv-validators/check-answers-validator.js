import { CleanText } from "../../../helpers/text-helpers";

export const validateCheckAnswersPage = (path, section) => {

    for (const q of path) {
        cy.get("div.govuk-summary-list__row dt.govuk-summary-list__key.spacer")
            .contains(CleanText(q.question.text))
            .siblings("dd.govuk-summary-list__value.spacer")
            .contains(CleanText(q.answer.text));
    }

    cy.url().should("include", `${section.interstitialPage.fields.slug}/check-answers`);

    cy.get("button.govuk-button").contains("Save and continue").click();
}