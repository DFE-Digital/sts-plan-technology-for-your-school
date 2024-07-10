import { CleanText } from "../../../helpers/text-helpers";

export const validateAnswers = (matchingQuestion) => {
    for (const answer of matchingQuestion.answers) {
        cy.get(
            "div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label"
        ).contains(CleanText(answer.text));
    }
}