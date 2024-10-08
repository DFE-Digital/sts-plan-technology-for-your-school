describe('Feedback banner', () => {
  it("should be visible when tracking consented", () => {
    cy.visit('content/hello-world',
      {
        headers: {
          'Cookie': '.AspNet.Consent=yes'
        }
      }
    );

    cy.get("div#feedback-banner")
      .should('exist')
      .not("govuk-visually-hidden")
      .should('not.have.attr', 'aria-hidden');
  });

  it("should not exist when tracking consent not given", () => {
    cy.visit('content/hello-world',
      {
        headers: {
          'Cookie': '.AspNet.Consent=no'
        }
      }
    );

    cy.get("div#feedback-banner")
      .should('not.exist');
  });

});