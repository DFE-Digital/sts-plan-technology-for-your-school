import ValidateContent from "./content-validator";
import ValidateTitle from "./title-validator";

function ValidatePage(slug, page) {
  if (!page.fields.requiresAuthorisation) {
    ShouldMatchUrl(slug);
  }

  if (page.fields.title) {
    ValidateTitle(page.fields.title);
  }

  for (const content of page.fields.beforeTitleContent) {
    ValidateContent(content);
  }

  for (const content of page.fields.content) {
    ValidateContent(content);
  }
}

function ShouldMatchUrl(url) {
  cy.location().should((loc) => {
    expect(loc.pathname).to.equal(url);
  });
}

export default ValidatePage;
