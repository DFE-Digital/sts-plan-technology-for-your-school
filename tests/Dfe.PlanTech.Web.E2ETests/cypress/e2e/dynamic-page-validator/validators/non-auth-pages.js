import { ValidatePage } from "../helpers/index.js";

export const validateNonAuthorisedPages = (dataMapper) => {
    Array.from(dataMapper?.pages ?? [])
        .map(([_, page]) => page)
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
