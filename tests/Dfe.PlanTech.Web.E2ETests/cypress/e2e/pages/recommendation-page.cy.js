/*
import { homePageSlug, recommendationSlug } from "../../helpers/page-slugs";
import { printAllRecommendationsText, paginationNext, paginationPrevious, recommendationsToSelfAssessmentText }  from "../../helpers/constants"

describe("Recommendation Page", () => {

    before(() => {
        cy.loginWithEnv(homePageSlug);

        cy.completeFirstSubtopic();
    });

    beforeEach(() => {
        cy.loginWithEnv(homePageSlug);
        cy.navigateToRecommendationPage();

        cy.url().should("contain", recommendationSlug);

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
    });

  it("Should have a link to open the page in printout format", () => {
      cy.get("a.govuk-link")
          .contains(printAllRecommendationsText)
  })

    //Links
    it("Should have no broken internal links", () => {
        cy.get(".govuk-main-wrapper").each(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                const href = $link.prop("href");
                // For internal links, check that they work - some external links block access from tools like cypress
                if (
                    href &&
                    (href.startsWith("/") ||
                        href.includes(window.location.origin))
                ) {
                    cy.request({ url: href });
                }
            });
        });

        cy.get(".recommendation-content").within(() => {
            cy.get("a").each(($link) => {
                cy.wrap($link).should("have.attr", "href");
                const href = $link.prop("href");
                if (
                    href &&
                    (href.startsWith("/") ||
                        href.includes(window.location.origin))
                ) {
                    cy.request({ url: href });
                }
            });
        });
    });

    //Pagination
    it("Should have pagination", () => {
        //Check there's a "Next" pagination link, and that there's only one visible
        cy.get("a.govuk-pagination__link")
            .filter(":visible")
            .should("have.length", 1)
            .then(($element) => {
                cy.wrap($element)
                    .find("span.govuk-pagination__link-title")
                    .should("exist")
                    .and("contain", paginationNext);

                cy.wrap($element).click();
            });

        //Check that there's now two pagination links; next and previous
        cy.get("a.govuk-pagination__link")
            .filter(":visible")
            .should("have.length", 2)
            .then(($element) => {
                cy.wrap($element)
                    .find("span.govuk-pagination__link-title")
                    .should("exist")
                    .and("contain", paginationNext);

                cy.wrap($element)
                    .find("span.govuk-pagination__link-title")
                    .should("exist")
                    .and("contain", paginationPrevious);
            });
    });

    // back button
    it("should have bespoke back button", () => {
        //Should not have normal back button
        cy.get("section.dfe-page-header a.govuk-back-link").should("not.exist");

        //Should have back button to homepage on the top of the vertical nav
        cy.get(
            "nav.dfe-vertical-nav div.dfe-vertical-nav__back-button a.govuk-back-link"
        )
            .contains(recommendationsToSelfAssessmentText)
            .should("exist")
            .and("have.attr", "href")
            .and("include", homePageSlug);
    });
});
*/
