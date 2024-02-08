import { ReplaceWhiteSpace } from "../text-helpers";

export default function ValidateInsetTextContent({ fields }) {
  cy.get("div.govuk-inset-text").contains(
    ReplaceWhiteSpace(fields.text).trim()
  );
}
