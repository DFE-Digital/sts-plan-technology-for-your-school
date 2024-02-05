function ValidateButtonWithLink(content) {
  const classAssertion = content.fields.button.fields.isStartButton
    ? "have.class"
    : "not.have.class";

  cy.get("a.govuk-button")
    .contains(content.fields.button.fields.value)
    .and(classAssertion, "govuk-button--start");
}

export default {
  ValidateButtonWithLink,
};
