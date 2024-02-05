function ValidateHeader(content) {
  const tag = content.fields.tag;
  const expectedClass = GetExpectedClass(content.fields.size.toLowerCase());

  cy.get(tag).contains(content.fields.text).should("have.class", expectedClass);
}

function GetExpectedClass(size) {
  const sizeLetters = size == "extra large" ? "xl" : size.substring(0, 1);

  const expectedClass = "govuk-heading-" + sizeLetters;
  return expectedClass;
}

export default ValidateHeader;
