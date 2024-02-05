import DataMapper from "../../../../contentful/export-processor/data-mapper";
import { contentful } from "./contentful";
import ValidatePage from "../helpers/content-validators/page-validator";

describe("Pages should have content", () => {
  let dataMapper;

  before(() => {
    if (contentful && contentful.entries && contentful.entries.length > 0) {
      dataMapper = new DataMapper(contentful);
    }
  });

  it.skip("Should work for unauthorised pages", () => {
    if (dataMapper?.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    for (const [pageId, page] of dataMapper.pages) {
      if (page.fields.requiresAuthorisation) {
        continue;
      }

      const slug = `/${page.fields.slug.replace("/", "")}`;
      cy.visit(slug);
      ValidatePage(slug, page);
    }
  });

  it.skip("Should validate self-assessment page", () => {
    if (dataMapper?.pages == null) {
      console.log("Datamapper has not processed data correctly");
      return;
    }

    const slug = "self-assessment";
    cy.loginWithEnv(`/${slug}`);

    const selfAssessmentPage = FindPageForSlug({ slug, dataMapper });

    if (!selfAssessmentPage) {
      throw new Error(
        `Could not find self-assessment page; not found page with slug ${slug}`
      );
    }

    ValidatePage(slug, selfAssessmentPage);
  });
});

function FindPageForSlug({ slug, dataMapper }) {
  for (const [id, page] of dataMapper.pages) {
    if (page.fields.slug == slug) {
      return page;
    }
  }

  return null;
}
