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

  it("Should have a Part Of header", () => {
    cy.get("span.app-step-nav-header__part-of").should("exist");
  });

  it("Should have Content", () => {
      cy.get("div.recommendation-piece-content").should("exist");
  });

  it("Should have recommendations sidebar Part Of header", () => {
      cy.get("h2.app-step-nav-related__heading").should("exist");
  });

  it("Should have recommendations accordion", () => {
      cy.get("#step-by-step-navigation").should("exist");
  });

  it("Should have a Show/Hide All chevron and text", () => {
      cy.get("span.app-step-nav__chevron").should("exist")
      cy.get("span.app-step-nav__button-text").should("exist");
  })

  it("Should have individual recommendation panel in accordion", () => {
      cy.get("li.app-step-nav__step").should("exist");
  })

  it("Should have a link to print open the page in another tab in a checklist format", () => {
      cy.get("a.govuk-link")
          .contains("Share or download this recommendation in a checklist format")
          .should("have.attr", "target", "_blank")
  })

    //Concertina display
    it("Should change text and chevron direction of Show/Hide All on click", () => {
        cy.get("span.js-step-controls-button-icon").should("have.class", "app-step-nav__chevron--down");
        cy.get("span.app-step-nav__button-text--all").should("contain", "Show");
        cy.get("button.app-step-nav__button--controls")
            .should("have.attr", "aria-expanded", "false")
            .click()
            .then(() => {
                cy.get("span.app-step-nav__chevron--down").should("not.exist");
                cy.get("span.app-step-nav__button-text--all").should("contain", "Hide");
                cy.get("button.app-step-nav__button--controls").should("have.attr", "aria-expanded", "true")
            })
    })

    it("Should show/hide all recommendations", () => {
        cy.get("li.app-step-nav__step").each(() => {
            cy.get("div.app-step-nav__panel").should("have.class", "js-hidden");
            cy.get("span.app-step-nav__chevron").should("have.class", "app-step-nav__chevron--down");
            cy.get("span.app-step-nav__button-text--all").should("contain", "Show");
            return;
        })
        cy.get("button.app-step-nav__button--controls")
            .should("have.attr", "aria-expanded", "false")
            .click()
        cy.get("li.app-step-nav__step").each(() => {
            cy.get("div.app-step-nav__panel").should("not.have.class", "js-hidden");
            cy.get("span.app-step-nav__chevron").should("not.have.class", "app-step-nav__chevron--down");
            cy.get("span.app-step-nav__button-text--all").should("contain", "Hide");
            return;
        })
    })

    it("Should show/hide individual recommendations", () => {
        cy.get("li.app-step-nav__step")
            .first()
            .should("not.have.class", "step-is-shown")
            .click()
            .should("have.class", "step-is-shown")
            .find("div.app-step-nav__panel")
                .should("not.have.class", "js-hidden");
    })

    it("Should change Show/Hide All back to Show All when individual recommendation is hidden", () => {
        cy.get("button.app-step-nav__button--controls").click();
        cy.get("span.app-step-nav__button-text--all").should("contain", "Hide");
        cy.get("li.app-step-nav__step").first().click();
        cy.get("span.app-step-nav__button-text--all").should("contain", "Show");
    })

    it("Should number recommendations from 1 in ascending order", () => {
        cy.get("ol.app-step-nav__steps").children().each(($li, index) => {
            cy.wrap($li).find("span.app-step-nav__circle-background").contains(`${index + 1}`)
        })
    })

    //Links
    it("Should have no broken links", () => {
        cy.get(".govuk-main-wrapper").within(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                cy.request({ url: $link.prop("href") });
            });
        });
        cy.get(".app-step-nav-header").within(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                cy.request({url: $link.prop("href")});
            });
        });
    });

  //Accessibility
  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
