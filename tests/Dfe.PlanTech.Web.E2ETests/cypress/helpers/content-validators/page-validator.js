import { ValidateContent } from "./content-validator.js";
import ValidateTitle from "./title-validator.js";

export const ValidatePage = (slug, page) => {
  if (!page.fields.requiresAuthorisation) {
    ShouldMatchUrl(slug);
  }

  if (page.fields.title) {
    ValidateTitle(page.fields.title);
  }

  if (page.fields.beforeTitleContent) {
    for (const content of page.fields.beforeTitleContent) {
      if (!content) {
        console.log(`Before title content is missing in exported page data.`, page);
        continue;
      }
      ValidateContent(content);
    }
  }

  if (page.fields.content) {
    for (const content of page.fields.content) {
      if (!content) {
        console.log(`Content is missing in exported page data.`, page);
        continue;
      }
      ValidateContent(content);
    }
  }
}

function ShouldMatchUrl(url) {
  cy.location().should((loc) => {
    expect(loc.pathname).to.equal(url);
  });
}

export default ValidatePage;
