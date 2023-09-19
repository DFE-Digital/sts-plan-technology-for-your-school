describe("Footer", () => {
  const FOOTER_SELECTOR = "footer.govuk-footer";
  const FOOTER_LINK_SELECTOR = `${FOOTER_SELECTOR} ul.govuk-footer__inline-list a.govuk-footer__link`;
  beforeEach(() => {
    cy.visit("/");
  });

  it("displays footer", () => {
    cy.get(FOOTER_SELECTOR).should("exist").and("be.visible");
  });

  it("has footer links", () => {
    cy.get(FOOTER_LINK_SELECTOR)
      .should("exist")
      .and("be.visible")
      .and("contain", "Cookies");
  });

  it("should have a link that opens in new tab", () => {
    cy.get(`${FOOTER_LINK_SELECTOR}[target]`).should("exist");
  });
});
