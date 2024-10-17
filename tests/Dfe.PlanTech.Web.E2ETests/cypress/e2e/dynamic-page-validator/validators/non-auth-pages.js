import { ValidatePage } from "../helpers/index.js";

export const validateAndTestNonAuthorisedPages = (dataMapper) => {
    Array.from(dataMapper?.pages ?? [])
        .map(([, page]) => page)
        .filter((page) => !page.fields.requiresAuthorisation)
        .forEach((page) => {
            it(
                "Should have correct content on non-authorised pages. Testing " +
                page.fields.internalName,
                () => {
                    const slug = `/${page.fields.slug.replace("/", "")}`;
                    cy.visit(slug);
                    ValidatePage(slug, page);
                }
            );
        });
}
