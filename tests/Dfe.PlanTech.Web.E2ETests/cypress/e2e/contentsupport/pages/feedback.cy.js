describe('Feedback banner', () => {
  it("should be visible when tracking consented", () => {
    cy.visit('content/hello-world',
      {
        headers: {
          'Cookie': 'user_cookies_preferences=%7B%22UserAcceptsCookies%22%3Atrue%2C%22IsVisible%22%3Afalse%7D'
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
          'Cookie': 'user_cookies_preferences=%7B%22UserAcceptsCookies%22%3Afalse%2C%22IsVisible%22%3Afalse%7D'
        }
      }
    );

    cy.get("div#feedback-banner")
      .should('not.exist');
  });

});
