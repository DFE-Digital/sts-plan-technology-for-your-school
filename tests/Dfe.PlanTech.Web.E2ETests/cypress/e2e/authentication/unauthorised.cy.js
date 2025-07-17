import { homePageSlug } from "../../helpers/page-slugs.js";

describe("Tests for users who are unauthorised, but authenticated", () => {
  beforeEach(() => {
    cy.loginWithEnv(homePageSlug, { userHasOrg: false });
  });

  it("Should redirect user to no org error page if user has no organisation", () => {
    cy.url().should('include', 'dsi-error-not-associated-organisation');
  });
});
