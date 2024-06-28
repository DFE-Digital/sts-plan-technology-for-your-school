function ValidateSection(section, listItem) {
    cy.task("log", section.fields.name);
    cy.wrap(listItem) //div govuk-summary-list__row
        .find("dt.govuk-summary-list__key")
    .contains(section.fields.name);

  cy.wrap(listItem).find("strong.govuk-tag").should("exist"); //TODO: Validate actual status of section
}

export default ValidateSection;
