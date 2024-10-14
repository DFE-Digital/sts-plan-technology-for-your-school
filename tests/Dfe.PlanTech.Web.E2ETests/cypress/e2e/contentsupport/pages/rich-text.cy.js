describe('Rich Text Rendering', () => {
  beforeEach(() => {
    cy.visit('content/hello-world');
  });

  describe('Headings', () => {
    it('renders main heading', () => {
      cy.get('h1').should('have.text', 'Hello World');
    });

    it('renders sub headings', () => {
      cy.get('h2').should('contain', 'Heading 2');
      cy.get('h3').should('contain', 'Heading 3');
      cy.get('h4').should('contain', 'Heading 4');
      cy.get('h5').should('contain', 'Heading 5');
      cy.get('h6').should('contain', 'Heading 6');
    });
  });

  describe('Text', () => {
    it('renders paragraph text', () => {
      cy.get('p').should(
        'contain',
        'Lorem ipsum dolor sit amet, consectetur adipiscing elit.'
      );
    });

    it('renders bold text', () => {
      cy.get('strong').should('contain', 'Bold text');
    });
  });

  describe('Lists', () => {
    it('renders ordered list with items', () => {
      cy.get('ol li').should('contain', 'ordered item 1').should('contain', 'ordered item 2');
    });

    it('renders unordered list with items', () => {
      cy.get('ul li').should('contain', 'unordered item 1').should('contain', 'ordered item 2');
    });
  });

  describe('Table', () => {
    it('renders table with headings', () => {
      cy.get('table thead tr').find('th').should('contain', 'Col 1 heading');
    });

    it('renders table with rows', () => {
      cy.get('table tbody tr').find('td').should('contain', 'Col 1 row 1');
    });
  });

  describe('Hyperlink', () => {
    it('renders hyperlink with href', () => {
      cy.get('a')
        .contains('A hyperlink')
        .should(
          'have.attr',
          'href',
          'https://technical-guidance.education.gov.uk/'
        );
    });
  });

  describe('Horizontal Rule', () => {
    it('renders hr', () => {
      cy.get('hr').should('exist');
      cy.get('h2').prev('hr').should('exist');
    });
  });

  describe('Attachment', () => {
    it('renders attachment', () => {
      cy.get('.attachment').should('exist');
      cy.get('.attachment-thumbnail').should('exist');
      cy.get('.attachment-details').should('exist');
      cy.get('.attachment-title').should('exist');
      cy.get('.attachment-metadata').should('exist');
      cy.get('.attachment-attribute').should('exist');
      cy.get('.attachment-link').should('exist');
      cy.get('.attachment-link').should('have.attr', 'download');
      cy.get('.attachment-link').should('have.attr', 'href');
      cy.get('.attachment-link').should('contain', 'Test csv');
      cy.get('.attachment-attribute').should('contain', 'CSV');
      cy.get('.attachment-attribute').should('contain', '18 KB');
      cy.get('.attachment-attribute[aria-label="updated date"]').should('exist');
    });
  });

  describe('Accordion', () => {
    beforeEach(() => {
      cy.get('#accordion-TestAccordion')
        .should('exist')
        .and('have.attr', 'data-module', 'govuk-accordion');
    });

    it('should have the accordion', () => {
      cy.get('#accordion-TestAccordion')
        .should('exist')
        .and('have.attr', 'data-module', 'govuk-accordion');
    });

    it('should toggle visibility of content when header is clicked', () => {
      const sectionButton = '#TestAccordionSection-1-heading';
      const sectionContent = '#TestAccordionSection-1-content';

      cy.get(sectionContent).should('be.hidden');
      cy.get(sectionButton).click();
      cy.get(sectionContent).should('not.be.hidden');
      cy.get(sectionButton).click();
      cy.get(sectionContent).should('be.hidden');
    });

    it('should have a show all button', () => {
      const showAllButton = '.govuk-accordion__show-all';
      const section1Content = '#TestAccordionSection-1-content';
      const section2Content = '#TestAccordionSection-2-content';

      cy.get(section1Content).should('be.hidden');
      cy.get(section2Content).should('be.hidden');
      cy.get(showAllButton).click();
      cy.get(section1Content).should('not.be.hidden');
      cy.get(section2Content).should('not.be.hidden');
    });
  });
});
