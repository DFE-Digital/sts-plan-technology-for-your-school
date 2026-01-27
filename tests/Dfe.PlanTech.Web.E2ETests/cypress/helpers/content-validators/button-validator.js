export function ValidateButtonWithLink(content) {
  const button = validateBaseButton(content);

  button.should('have.attr', 'href').and('include', content.fields.href);
}

function validateBaseButton(content) {
  const classAssertion = getClassAssertion(content);

  const button = cy.get('a.govuk-button').contains(content.fields.button.fields.value);

  button.should(classAssertion, 'govuk-button--start');

  return button;
}

export function ValidateButtonWithEntryReference(content) {
  const button = validateBaseButton(content);

  button.should('have.attr', 'href').then((href) => {
    if (href.indexOf(content.fields.linkToEntry.fields.href) != -1) {
      expect(href).to.include(content.fields.linkToEntry.fields.href);
    } else if (href.indexOf('next-question') != -1) {
      expect(href).to.include('next-question');
    } else {
      throw new Error(`Could not find button with HREF ${content.fields.linkToEntry.fields.href}`);
    }
  });
}

function getClassAssertion(content) {
  return content.fields.button.fields.isStartButton ? 'have.class' : 'not.have.class';
}
