describe("landing page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);
    cy.get("ul.app-task-list__items > li a").first().click();
    cy.get('a.govuk-button').contains('Continue').click();
  });

  it("should contain form", () => {
    cy.get("form").should("exist");
  });

  it("should contain heading", () => {
    cy.get("form h1.govuk-fieldset__heading").should("exist");

    cy.get("form h1.govuk-fieldset__heading")
      .invoke("text")
      .should("have.length.greaterThan", 1);
  });

  it("should contain answers", () => {
    cy.get("form div.govuk-radios div.govuk-radios__item")
      .should("exist")
      .and("have.length.greaterThan", 1)
      .each((item) => {
        cy.wrap(item)
          .get("label")
          .should("exist")
          .invoke("text")
          .should("have.length.greaterThan", 1);
      });
  });

  it("should have submit button", () => {
    cy.get("form button.govuk-button").should("exist");
  });

  it("should navigate to next page on submit", () => {
    const path = cy.location("pathname");

    cy.get("form div.govuk-radios div.govuk-radios__item input")
      .first()
      .click();

    cy.get("form button.govuk-button")
      .contains("Save and continue")
      .click();

    cy.location("pathname", { timeout: 60000 })
      .should("not.include", path)
      .and("include", "/question");
  });

  it("should have back button", () => {
    cy.get("a.govuk-back-link")
      .should("exist")
      .invoke("text")
      .should("equal", "Back");
  });

  it("should have back button that navigates to last question once submitted", () => {
    const FIRST_QUESTION = "question"; //TODO;
    
    cy.get("form div.govuk-radios div.govuk-radios__item input")
      .first()
      .click();

    cy.get("form button.govuk-button").click();

    cy.location("pathname", { timeout: 60000 })
      .should("not.include", FIRST_QUESTION)
      .and("include", "/question");

    cy.get("a.govuk-back-link")
      .should("exist")
      .and("have.attr", "href")
      .and("include", FIRST_QUESTION);
  });

});
