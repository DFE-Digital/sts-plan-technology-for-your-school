import DataMapper from "../helpers/contentful-helpers/data-mapper.mjs";

import { contentful } from "../../../../contentful.mjs";
import ValidateContent from "../helpers/content-validators/content-validator.mjs";
import ValidateTitle from "../helpers/content-validators/title-validator.mjs";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    dataMapper = new DataMapper(contentful);
  });

  it.skip("Should work for unauthorised pages", () => {
    for (const [pageId, page] of dataMapper.pages) {
      if (page.fields.requiresAuthorisation) {
        continue;
      }

      const slug = `/${page.fields.slug.replace("/", "")}`;
      cy.visit(slug);
      ShouldMatchUrl(slug);

      if (page.fields.title) {
        ValidateTitle(page.fields.title);
      }

      const contents = page.fields.content;

      for (const content of contents) {
        ValidateContent(content);
      }
    }
  });
});

function ShouldMatchUrl(url) {
  cy.location().should((loc) => {
    expect(loc.pathname).to.equal(url);
  });
}
