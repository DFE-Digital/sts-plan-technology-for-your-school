function ValidateTitle(title) {
  cy.get("h1.govuk-heading-xl")
    .should("exist")
    .and("have.text", title.fields.text);
}

export default ValidateTitle;
