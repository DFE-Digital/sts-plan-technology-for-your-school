describe("Question page", () => {
  const url = "/self-assessment";

  beforeEach(() => {
    cy.loginWithEnv(url);

    //Navigate to first section
    cy.clickFirstSection();
    

    //Navigate to first question
    cy.clickContinueButton();

    cy.injectAxe();
  });

  it("should have inline error when trying to submit without answering any questions", () => {
    cy.url().then(firstUrl => { 
      cy.get("form button.govuk-button")
          .contains("Save and continue")
          .click();

      cy.location("pathname", { timeout: 60000 })
          .should("not.equal", firstUrl)
          .and("match", /broadband-connection\/broadband-contract-review/g)

      cy.get("form div.govuk-form-group--error")
          .should("exist"); 
    });
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
    cy.url().then(firstUrl => {

      cy.get("form div.govuk-radios div.govuk-radios__item input")
        .first()
        .click();

      cy.get("form button.govuk-button")
        .contains("Save and continue")
        .click();

      cy.location("pathname", { timeout: 60000 })
        .should("not.include", firstUrl);
    });
  });

  it("should have back button", () => {
    cy.get("a.govuk-back-link")
      .contains("Back")
      .should("exist");
  });

  it("should have back button that navigates to last question once submitted", () => {
    cy.url().then(firstUrl => {
      //Select first radio button
      cy.get("form div.govuk-radios div.govuk-radios__item input")
        .first()
        .click();

      //Submit
      cy.get("form button.govuk-button")
        .contains("Save and continue")
        .click();

      //Ensure path changes
      cy.location("pathname", { timeout: 60000 })
        .should("not.equal", firstUrl)
        .and("match", /check-answers/g)

      cy.get("a.govuk-back-link")
        .should("exist")
        .and("have.attr", "href")
        .and("include", firstUrl);
    });
  });

  it("passes accessibility tests", () => {
    cy.runAxe();
  });
});