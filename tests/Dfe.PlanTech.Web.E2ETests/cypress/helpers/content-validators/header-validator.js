import { CleanText } from '../text-helpers.js';

function ValidateHeader(content) {
  const tag = content.fields.tag;
  const expectedClass = GetExpectedClass(content.fields.size.toLowerCase());

  const expectedText = CleanText(content.fields.text);

  return cy.get(`${tag}`).contains(expectedText.trim()).should('have.class', expectedClass);
}

function GetExpectedClass(size) {
  const sizeLetters = size == 'extralarge' ? 'xl' : size.substring(0, 1);

  const expectedClass = 'govuk-heading-' + sizeLetters;
  return expectedClass;
}

export default ValidateHeader;
