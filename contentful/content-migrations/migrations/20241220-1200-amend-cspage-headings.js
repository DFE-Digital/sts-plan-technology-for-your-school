module.exports = function (migration, {makeRequest}) {
    const targetContentType = migration.editContentType("ContentSupportPage");

    targetContentType.createField('subtitle', {
        name: 'Subtitle',
        type: 'Text',
        required: false,
        localized: false,
        validations: [],
        disabled: false,
        omitted: false,
    });

	migration.transformEntries({
        contentType: "ContentSupportPage",
        from: ["Heading"],
        to: ["subtitle"],
        transformEntryForLocale: async (fromFields, locale) => {
            const transformedEntry = {};

            const headingEntry = fromFields["Heading"] ? fromFields["Heading"][locale] : null;

            if (headingEntry) {
                const headingId = headingEntry.sys.id;
                const heading = await makeRequest({
                    method: 'GET',
                    url: `/entries/${headingId}`,
                });

                if (heading.fields["Subtitle"] && heading.fields["Subtitle"][locale]) {
                    transformedEntry.subtitle = heading.fields["Subtitle"][locale];
                }
            }

            return transformedEntry;
        },
    });
};