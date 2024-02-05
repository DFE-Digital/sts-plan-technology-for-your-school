import { ReplaceNonBreakingHyphen } from "../text-helpers.js";

function ValidateHeader(content) {
  const tag = content.fields.tag;
  const expectedClass = GetExpectedClass(content.fields.size.toLowerCase());

  const expectedText = ReplaceNonBreakingHyphen(content.fields.text);

  return cy
    .get(`header-component ${tag}`)
    .contains(expectedText)
    .should("have.class", expectedClass);
}

function GetExpectedClass(size) {
  const sizeLetters = size == "extra large" ? "xl" : size.substring(0, 1);

  const expectedClass = "govuk-heading-" + sizeLetters;
  return expectedClass;
}

export default ValidateHeader;
