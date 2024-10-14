import { CleanText } from "../helpers/index.js";
import { validateAnswers } from "./index.js";

export const extraQuestionTests = (matchingQuestion, section) => {
    // Url contains question slug
    cy.url().should("include", `/${section.interstitialPage.fields.slug}/${matchingQuestion.slug}`);
    // Page contains question help text
    if (matchingQuestion.helpText) {
        cy.get("div.govuk-hint").contains(CleanText(matchingQuestion.helpText));
    }
    // Contains each answer
    validateAnswers(matchingQuestion);
}
