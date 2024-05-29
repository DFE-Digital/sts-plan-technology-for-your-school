import { ReplaceWhiteSpace } from "../text-helpers.js";

export default function ValidateInsetTextContent({ fields }) {
  cy.get("div.govuk-inset-text").contains(
    ReplaceWhiteSpace(fields.text).trim()
  );
}
