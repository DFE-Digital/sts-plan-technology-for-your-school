describe("beta header", () => {
  const expectedFeedbackLink = "https://forms.office.com/e/Jk5PuNWvGe";

  const phaseBannerSelector = "div.govuk-phase-banner";

  beforeEach(() => {
    cy.visit("/");
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
});
