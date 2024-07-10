import { CleanText } from "../../../helpers/text-helpers.js";
import { validateAnswers } from "./answer-validator.js";

export const navigateAndValidateQuestionPages = (path, section) => {

    for (const question of path) {
        const matchingQuestion = section.questions.find((q) => q.text === question.question.text);

        if (!matchingQuestion) {
            throw new Error(
                `Couldn't find matching question for ${question.question.text}`
            );
        }

        // Url contains question slug
        cy.url().should("include", `/${section.interstitialPage.fields.slug}/${matchingQuestion.slug}`);

        // Page contains question
        cy.get("h1.govuk-fieldset__heading").contains(CleanText(question.question.text));

        // Page contains question help text
        if (matchingQuestion.helpText) {
            cy.get("div.govuk-hint").contains(CleanText(matchingQuestion.helpText));
        }
        // Contains each answer
        validateAnswers(matchingQuestion);

        //Select answer for path and continue
        cy.get("div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label")
            .contains(CleanText(question.answer.text))
            .click();
        cy.get("button.govuk-button").contains("Save and continue").click();
    }
}