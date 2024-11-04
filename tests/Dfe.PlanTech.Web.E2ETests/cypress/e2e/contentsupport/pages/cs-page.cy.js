describe('C&S Page', () => {
  beforeEach(() => {
    cy.visit('content/digital-technology-asset-register');
    cy.injectAxe();
  });

  describe('Headings', () => {
    it('should have a div with class dfe-page-header', () => {
      cy.get('div.dfe-width-container').should('exist');
    });

    it('renders main heading', () => {
      cy.get('h1').should('exist');
    });
  });

  describe('Text', () => {
    it('renders paragraph text', () => {
      cy.get('p').should('exist');
    });
  });

  it('should have no accessibility violations', () => {
    cy.checkA11y();
  });
});