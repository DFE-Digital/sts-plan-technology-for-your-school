describe("Footer", () => {
    const FOOTER_SELECTOR = "footer.govuk-footer";
    const URLS = ["/", "/content/hello-world"];

    URLS.map((url) => {
        describe(`${url} footer`, () => {
            beforeEach(() => {
                cy.visit(url);
            });

            it("displays footer", () => {
                cy.get(FOOTER_SELECTOR).should("exist").and("be.visible");
            });

            it("has footer links", () => {
                const footerLinks = cy.get(
                    `${FOOTER_SELECTOR} ul.govuk-footer__inline-list a.govuk-footer__link`
                );

                footerLinks
                    .should("exist")
                    .and("be.visible")
                    .and("contain", "Cookies");
            });
        });
    });
});
