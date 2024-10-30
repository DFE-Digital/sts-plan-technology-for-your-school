describe('Print button', () => {
  beforeEach(() => {
    cy.visit('content/hello-world', {
      onBeforeLoad(win) {
        //Stub the print functionality so we can see if it was called
        //Note: could spy instead, but I don't want the print dialog to actually show.
        cy.stub(win, 'print', () => { });
      },
    });
  });

  it("should be visible", () => {
    cy.get("button.print-button")
      .should('exist')
      .not("govuk-visually-hidden")
      .should('not.have.attr', 'aria-hidden');

    cy.get("button#print-link").should("exist");
  });

  it("should print on click", () => {
    cy.get("button#print-link").click();
    cy.window().its("print").should('be.called');
  });
});
