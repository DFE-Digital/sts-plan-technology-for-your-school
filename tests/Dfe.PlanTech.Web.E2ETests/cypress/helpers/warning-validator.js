import ValidateRichTextContent from "./content-validators/rich-text-content-validator";

export default function ValidateWarningComponent({ fields, sys }) {
  cy.get("div.govuk-warning-text").then((warning) => {
    warning.find("span.govuk-warning-text__icon").contains("!");
    warning
      .find(
        "strong.govuk-warning-text__text span.govuk-warning-text__assistive"
      )
      .contains("Warning");

    ValidateRichTextContent(fields.richText);
  });
}
