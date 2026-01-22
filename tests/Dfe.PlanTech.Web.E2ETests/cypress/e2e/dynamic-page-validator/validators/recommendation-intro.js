import {
  ValidateContent,
  recommendationIntroFragment,
  recommendationsSidebarTitle,
  recommendationIntroTitle,
} from '../helpers/index.js';

export const validateRecommendationIntro = (introPage, recommendationUrl) => {
  const { slug, header, content } = introPage;

  cy.url().should('include', `${recommendationUrl}/${slug}`);

  cy.get('h1.govuk-heading-xl').contains(header.fields.text);

  cy.get('h2.dfe-vertical-nav__theme').contains(recommendationsSidebarTitle).should('exist');

  cy.get('ul.dfe-vertical-nav__section > li')
    .eq(0)
    .within(() => {
      cy.get('a.dfe-vertical-nav__link')
        .contains(recommendationIntroTitle)
        .should('have.attr', 'href', recommendationIntroFragment);
    });

  if (content) {
    for (const component of content) {
      if (!component) {
        console.log(`Content is missing in exported data.`, introPage);
        continue;
      }
      ValidateContent(component);
    }
  }
};
