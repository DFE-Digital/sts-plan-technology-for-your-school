describe("beta header", () => {
  const url = Cypress.env("URL");

  const expectedFeedbackLink = "https://feedback";

  const phaseBannerSelector = "div.govuk-phase-banner";

  beforeEach(() => {
    cy.visit(url);
  });

  it("displays phase banner", () => {
    cy.get(phaseBannerSelector).should("exist").and("be.visible");
  });

  it("should display 'Beta' tag", () => {
    const tag = cy.get(`${phaseBannerSelector} strong.govuk-tag`);

    tag.should("exist").and("be.visible");

    tag.should("have.text", "Beta");
  });

  it("should link to feedback url", () => {
    const feedbackLink = cy.get(`${phaseBannerSelector} a`);

    feedbackLink
      .should("exist")
      .and("have.attr", "href")
      .and("include", expectedFeedbackLink);
  });

  it("should fail", () => {
    cy.get(`div.not-a-real-selector a.not-a-real-link`).should("exist");
  });
});
