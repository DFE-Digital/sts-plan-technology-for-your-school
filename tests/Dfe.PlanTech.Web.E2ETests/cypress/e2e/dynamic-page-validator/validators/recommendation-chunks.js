import { CleanText, ValidateContent, slugifyChunks } from "../helpers/index.js";


export const validateRecommendationChunks = (chunks) => {

    cy.get("h2.dfe-vertical-nav__theme").contains("List of actions").should("exist");

    chunks.forEach((chunk, i) => {
        const chunkSlug = `${slugifyChunks(chunk.header)}`;

        cy.get("ul.dfe-vertical-nav__section > li").eq(i).within(() => {
            cy.get(`a.dfe-vertical-nav__link:contains(${CleanText(chunk.header)})`).each($link => {
                expect($link).to.have.attr("href").contains(`#${chunkSlug}`);
            })
            .click({force: true}); // Avoids 'parent not visible error'
        })

        cy.url().should("include", slugifyChunks(chunk.header));
        
        cy.get(`[id*=${chunkSlug}]`).within(() => {

            cy.get("h1.govuk-heading-xl").contains(CleanText(chunk.header));

            cy.get("div.recommendation-piece-content");

            if (chunk.content) {
                for (const component of chunk.content) {
                    if (!component) {
                        console.log(`content is missing in page.`, introPage);
                        continue;
                    }
                    ValidateContent(component);
                }
            }
        })
    });   
}