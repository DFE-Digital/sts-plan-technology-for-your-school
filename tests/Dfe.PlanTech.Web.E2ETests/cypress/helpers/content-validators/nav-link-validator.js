export default function ValidateNavigationLink({ fields }) {
    if (fields.openInNewTab) {
        return;
    }

    cy.get(
      "ul.govuk-footer__inline-list li.govuk-footer__inline-list-item a.govuk-footer__link"
  )
    .contains(fields.displayText)
    .should("have.attr", "href")
    .and("include", fields.href);
}
