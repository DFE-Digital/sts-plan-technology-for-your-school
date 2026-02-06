/**
 * Click first section link on self-assessment page
 */
export const clickFirstSection = () => {
  cy.get('div.govuk-summary-list__row > dt a').first().click();
};

/**
 * Find link to section on self assessment page
 */

export const findSectionLink = (sectionName, sectionSlug) => {
  cy.get(`a.govuk-link[href="/${sectionSlug}"]`).each(($el) => {
    const linkText = $el.text().trim();
    if (linkText === sectionName) {
      return cy.wrap($el);
    }
  });
};

Cypress.Commands.add('clickFirstSection', clickFirstSection);
Cypress.Commands.add('findSectionLink', findSectionLink);
