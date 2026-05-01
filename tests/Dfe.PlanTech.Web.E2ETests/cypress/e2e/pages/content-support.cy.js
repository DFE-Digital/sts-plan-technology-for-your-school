describe('Content & Support Page Richtext components', () => {
  const url = '/information-asset-register';

  beforeEach(() => {
    cy.visit(url);
  });

  it('renders attachment component', () => {
    cy.get('.attachment').should('exist');
    cy.get('.attachment-link').should('exist').and('have.attr', 'href');
  });
});
