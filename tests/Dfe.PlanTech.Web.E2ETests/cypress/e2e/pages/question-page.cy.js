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
    let currentUrl;

    cy.url().then((url) => {
      currentUrl = url;
    });

    cy.get("#QuestionText")
      .invoke("val")
      .then((value) => {
        let questionText = value;
        cy.log(`Question Text is: ${questionText}`);

        cy.get("form button.govuk-button")
          .contains("Save and continue")
          .click();

        cy.get("form div.govuk-form-group--error").should("exist");

        cy.get("#QuestionText").should("have.value", questionText);

        cy.url().should("equal", currentUrl);
      });
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

  it("should have a panel that provides a copyable link for sharing the question", () => {
    cy.get("div.dfe-section-card")
      .should("exist")
      .within(($card) => {
        cy.get("h1").should("exist");
        cy.get("label").should("exist");
        cy.url().then($url => {
          // check share link matches that of the current page
          cy.get("input[name=questionLink]")
            .should("exist")
            .should("have.attr", "readonly")

          cy.get("input[name=questionLink]")
            .invoke("val")
            .should("equal", $url)

          // check that copy to clipboard button works
          cy.get("button")
            .should("exist")
            .click()
            .then(() => {
              cy.assertCopiedToClipboard($url)

              // and replaces the link and button with a success message
              cy.get("p").should("exist")
              cy.get("label").should("not.exist")
              cy.get("button").should("not.exist")
            })
        })
      })
  });

  it("should have submit button", () => {
    cy.get("form button.govuk-button").should("exist");
  });

  it("should navigate to next page on submit", () => {
    cy.url().then((firstUrl) => {
      cy.selectFirstRadioButton();

      cy.saveAndContinue();

      cy.location("pathname", {timeout: 60000}).should(
        "not.include",
        firstUrl
      );
    });
  });

  it("should have back button", () => {
    cy.get("a.govuk-back-link").contains("Back").should("exist");
  });

  it("should have back button that navigates to last question once submitted", () => {
    cy.url().then((firstUrl) => {
      //Select first radio button
      cy.selectFirstRadioButton();

      //continue
      cy.saveAndContinue();

      //Ensure path changes
      cy.location("pathname", {timeout: 60000}).should("not.equal", firstUrl);

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
