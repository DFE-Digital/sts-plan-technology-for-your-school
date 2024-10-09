import 'cypress-axe';

describe('Rich Text Rendering', () => {
  beforeEach(() => {
    cy.visit('content/hello-world');
    cy.injectAxe();
  });

  it('should have no accessibility violations', () => {
    cy.checkA11y();
  });
});
