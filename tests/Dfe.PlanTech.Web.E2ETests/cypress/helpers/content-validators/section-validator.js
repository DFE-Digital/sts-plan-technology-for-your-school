function ValidateSection(section, listItem) {
  cy.wrap(listItem)
    .find("span.app-task-list__task-name")
    .contains(section.fields.name);

  cy.wrap(listItem).find("strong.govuk-tag").should("exist"); //TODO: Validate actual status of section
}

export default ValidateSection;
