import DataMapper from "./contentful-helpers/data-mapper.mjs";

import { contentful } from "./contentful";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    dataMapper = new DataMapper(contentful);
  });

  it("Should work for unauthorised pages", () => {
    for (const [pageId, page] of dataMapper.pages) {
      if (page.fields.requiresAuthorisation) {
        continue;
      }

      const slug = `/${page.fields.slug.replace("/", "")}`;
      cy.visit(slug);
      ShouldMatchUrl(slug);

      const contents = page.fields.content;

      for (const content of contents) {
        switch (content.sys.contentType.sys.id) {
          case "header": {
            const tag = content.fields.tag;
            cy.get(tag).contains(content.fields.text);
            break;
          }
        }
      }
    }
  });
});
function ShouldMatchUrl(url) {
  cy.location().should((loc) => {
    expect(loc.pathname).to.equal(url);
  });
}

function TestContent() {}

function ShouldBeAuthorised(page) {
  cy.request({
    url: page.fields.slug,
    followRedirect: false, // turn off following redirects
  }).then((resp) => {
    // redirect status code is 302
    expect(resp.status).to.eq(302);
    expect(resp.redirectedToUrl).to.contain(
      "https://pp-oidc.signin.education.gov.uk/"
    );
  });
}
