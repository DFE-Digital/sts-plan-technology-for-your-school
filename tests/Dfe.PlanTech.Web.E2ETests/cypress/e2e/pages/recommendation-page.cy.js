describe("Recommendation Page", () => {
  const url = "/self-assessment";

  before(() => {
    cy.loginWithEnv(url);

    cy.completeFirstSubtopic();
  });
  
  beforeEach(() => {
    cy.loginWithEnv(url);
    cy.navigateToRecommendationPage();

    cy.url().should("contain", "recommendation");

    cy.injectAxe();
  });

  //Structure elements
  it("Should have DfE header", () => {
    cy.get("header.dfe-header").should("exist");
  });

    it("Should have Gov.uk footer", () => {
        cy.get("footer.govuk-footer").should("exist");
    });

  it("Should have Content", () => {
      cy.get("div.recommendation-piece-content").should("exist");
  });

  it("Should have vertical navigation bar", () => {
      cy.get("nav.dfe-vertical-nav").should("exist");
  });

  it("Should have sections in navigation bar", () => {
      cy.get("li.dfe-vertical-nav__section-item").should("exist");
  })

  it("Should have a link to print open the page in another tab in a checklist format", () => {
      cy.get("a.govuk-link")
          .contains("Share or download this recommendation in a checklist format")
          .should("have.attr", "target", "_blank")
  })

    //Links
    it("Should have no broken links", () => {
        cy.get(".govuk-main-wrapper").within(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                cy.request({ url: $link.prop("href") });
            });
        });
        cy.get(".recommendation-content").within(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                cy.request({url: $link.prop("href")});
            });
        });
    });

    // back button
    it("should have back button", () => {
      cy.get("a.govuk-back-link").contains("Back").should("exist");
    });

    it ("should have back button that goes to back to self assessment page", () => {
      cy.get("a.govuk-back-link")
        .should("exist")
        .and("have.attr", "href")
        .and("include", url);
     });

  //Accessibility
  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
