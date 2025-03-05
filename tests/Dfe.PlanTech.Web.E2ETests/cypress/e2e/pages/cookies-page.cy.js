import { cookiesSlug } from "../../helpers/page-slugs";
import { backText, saveCookieSettings, cookiesLinkText } from "../../helpers/constants";

describe("Cookies Page", () => {
  const url = "/";

  beforeEach(() => {
    cy.visit(url);
    cy.get(
      "footer.govuk-footer ul.govuk-footer__inline-list a.govuk-footer__link"
    )
      .contains(cookiesLinkText)
      .click();
    cy.url().should("contain", cookiesSlug);
    cy.injectAxe();
  });

  it("Should Have Heading", () => {
    cy.get("h1.govuk-heading-xl").should("exist");
  });

  it("Should Have Back Button", () => {
    cy.get(`a:contains(${backText})`)
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
    cy.get("form div.govuk-radios div.govuk-radios__item input").first().click();

    cy.get("form button.govuk-button").contains(saveCookieSettings).click();

    cy.get("div.govuk-notification-banner__header").should("exist");
    cy.get("form div.govuk-radios div.govuk-radios__item input").eq(0).should("have.attr", "checked");

    cy.get("noscript").contains("www.googletagmanager.com").should("exist");

    if (Cypress.env("APP_ENVIRONMENT") != "staging") {
      cy.get('meta[name="google-site-verification"]').should('exist');
    }

    cy.get('head script[src*="www.googletagmanager.com"]').should("exist");
  });

  it("Should reject cookies", () => {
    cy.get("form div.govuk-radios div.govuk-radios__item").eq(1).click();

    cy.get("form button.govuk-button").contains(saveCookieSettings).click();

    cy.get("div.govuk-notification-banner__header").should("exist");
    cy.get("form div.govuk-radios div.govuk-radios__item input").eq(1).should("have.attr", "checked");

    const noScriptElementExists = Cypress.$("noscript").toArray().some(el => el.innerHTML.indexOf("www.googletagmanager.com") > -1);
    if (noScriptElementExists) {
      throw new Error("Found GTM <noscript> element but should be rejected");
    }

    cy.get('meta[name="google-site-verification"]').should('not.exist');
    cy.get('head script[src*="www.googletagmanager.com"]').should("not.exist");
  });

  it("Passes Accessibility Testing", () => {
    cy.runAxe();
  });
});
