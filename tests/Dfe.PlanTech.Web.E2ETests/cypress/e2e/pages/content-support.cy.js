describe('Content & Support Page Richtext components', () => {
  const url = '/information-asset-register';

  beforeEach(() => {
    cy.visit(url);
  });

  it('renders attachment component', () => {
    cy.get('.attachment').should('exist');
    cy.get('.attachment-link').should('exist').and('have.attr', 'href');
  });

  it('renders accordion components', () => {
    cy.get('.govuk-accordion').should('exist');
    cy.get('.govuk-accordion__controls').should('exist').contains('span', 'Show all sections');
    cy.get('.govuk-accordion__section').should('have.length', 2);
    cy.get('.govuk-accordion__section-toggle-text').should('have.length', 2);
  });

  it('hides section content by default', () => {
    cy.get('.govuk-accordion__section-content').each(($el) => {
      cy.wrap($el).should('have.attr', 'hidden');
    });
  });

  it('expands and collapses a section', () => {
    cy.get('.govuk-accordion__section-button').first().as('firstButton');
    cy.get('.govuk-accordion__section-content').first().as('firstContent');

    //expand
    cy.get('@firstButton').click();
    cy.get('@firstContent').should('not.have.attr', 'hidden');

    //collapse
    cy.get('@firstButton').click();
    cy.get('@firstContent').should('have.attr', 'hidden');
  });
});
