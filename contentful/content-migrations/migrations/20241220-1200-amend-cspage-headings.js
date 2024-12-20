module.exports = function (migration, {makeRequest}) {
    const targetContentType = migration.editContentType("ContentSupportPage");

    targetContentType.createField('title', {
        name: 'Title',
        type: 'Symbol',
        required: true,
        localized: false,
        validations: [],
        disabled: false,
        omitted: false,
    });

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
        to: ["title", "subtitle"],
        transformEntryForLocale: async (fromFields, locale) => {
            const transformedEntry = {};

            const headingEntry = fromFields["Heading"] ? fromFields["Heading"][locale] : null;

            if (headingEntry) {
                const headingId = headingEntry.sys.id;
                const heading = await makeRequest({
                    method: 'GET',
                    url: `/entries/${headingId}`,
                });

                if (heading.fields["Title"] && heading.fields["Title"][locale]) {
                    transformedEntry.title = heading.fields["Title"][locale];
                }

                if (heading.fields["Subtitle"] && heading.fields["Subtitle"][locale]) {
                    transformedEntry.subtitle = heading.fields["Subtitle"][locale];
                } else {
                    transformedEntry.subtitle = null;
                }
            }

            return transformedEntry;
        },
    });

    // migration.deleteContentType('CSHeading');
};