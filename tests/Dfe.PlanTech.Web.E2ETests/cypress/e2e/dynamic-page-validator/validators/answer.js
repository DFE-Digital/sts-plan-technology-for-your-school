import { CleanText } from "../helpers/index.js";

export const validateAnswers = ({ answers }) => {
    for (const answer of answers) {
        cy.get(
            "div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label"
        ).contains(CleanText(answer.text));
    }
}