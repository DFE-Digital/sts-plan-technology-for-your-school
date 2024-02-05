import DataMapper from "../helpers/contentful-helpers/data-mapper";

import { contentful } from "./contentful";
import ValidateContent from "../helpers/content-validators/content-validator";
import ValidateTitle from "../helpers/content-validators/title-validator";

describe("Pages should have content", () => {
  before(() => {});

  it.skip("Should work for unauthorised pages", () => {
    const dataMapper = new DataMapper(contentful);

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
