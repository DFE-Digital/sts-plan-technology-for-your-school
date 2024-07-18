import ValidateContent from "../../../helpers/content-validators/content-validator.js";

export const ValidateRecommendationIntro = (introPage, recommendationUrl) => {
    const { slug, header, content } = introPage;
    
    cy.url().should("include", `${recommendationUrl}/${slug}`);

    cy.get("h1.govuk-heading-xl").contains(header);
 
    if (content) {
        for (const component of content) {
            if (!component) {
                console.log(`content is missing in page.`, introPage);
                continue;
            }
            ValidateContent(component)
        }
    }
}