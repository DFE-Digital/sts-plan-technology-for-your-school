import { ValidateContent } from "../helpers/index.js";

export const validateRecommendationIntro = (introPage, recommendationUrl) => {
    const { slug, header, content } = introPage;
    
    cy.url().should("include", `${recommendationUrl}/${slug}`);

    cy.get("h1.govuk-heading-xl").contains(header);
 
    if (content) {
        for (const component of content) {
            if (!component) {
                console.log(`Content is missing in exported data.`, introPage);
                continue;
            }
            ValidateContent(component)
        }
    }
}