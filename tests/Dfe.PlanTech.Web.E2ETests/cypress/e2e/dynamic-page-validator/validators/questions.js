import { CleanText } from "../helpers/index.js";

export const navigateAndValidateQuestionPages = (path, section, extraTests) => {

    for (const question of path) {
        const matchingQuestion = section.questions.find((q) => q.text === question.question.text);

        if (!matchingQuestion) {
            throw new Error(
                `Couldn't find matching question for ${question.question.text}`
            );
        }

        if (extraTests) {
            extraTests(matchingQuestion, section);
        }

        // Page contains question
        cy.get("h1.govuk-fieldset__heading").contains(CleanText(question.question.text));

        //Select answer for path and continue
        cy.get("div.govuk-radios div.govuk-radios__item label.govuk-radios__label.govuk-label")
            .contains(CleanText(question.answer.text))
            .click();
        cy.get("button.govuk-button").contains("Save and continue").click();
    }
}
