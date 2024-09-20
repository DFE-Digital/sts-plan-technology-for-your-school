describe("Cookies Page", () => {
  const url = "/";

  beforeEach(() => {
    cy.visit(url);
    cy.get(
      "footer.govuk-footer ul.govuk-footer__inline-list a.govuk-footer__link"
    )
      .contains("Cookies")
      .click();
    cy.url().should("contain", "/cookies");
    cy.injectAxe();
  });

  it("Should Have Heading", () => {
    cy.get("h1.govuk-heading-xl").should("exist");
  });

  it("Should Have Back Button", () => {
    cy.get('a:contains("Back")')
      .should("exist")
      .should("have.attr", "href")
      .and("include", "/");
  });

  it("Should Have Content", () => {
    cy.get("p").should("exist");
  });

  it("Should have change cookie preferences form with options", () => {
    cy.get("form div.govuk-radios div.govuk-radios__item")
      .should("exist")
      .and("have.length", 2)
      .each((item) => {
        cy.wrap(item)
          .get("label")
          .should("exist")
          .invoke("text")
          .should("have.length.greaterThan", 1);
      });
  });

  it("Should accept cookies", () => {
    cy.get("form div.govuk-radios div.govuk-radios__item").first().click();

    cy.get("form button.govuk-button").contains("Save cookie settings").click();

    cy.get("div.govuk-notification-banner__header").should("exist");
    cy.get("form div.govuk-radios div.govuk-radios__item input").eq(1).should("have.attr", "checked");
  });

  it("Should reject cookies", () => {
    cy.get("form div.govuk-radios div.govuk-radios__item").eq(1).click();

    cy.get("form button.govuk-button").contains("Save cookie settings").click();

    cy.get("div.govuk-notification-banner__header").should("exist");
    cy.get("form div.govuk-radios div.govuk-radios__item input").eq(1).should("have.attr", "checked");
  });

  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
