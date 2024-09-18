module.exports = function (migration, { makeRequest }) {
    const contentType = "recommendationChunk";
    const recommendationChunk = migration.editContentType(contentType);

    // Add new header as a short text field
    recommendationChunk.createField("newHeader", {
        name: "Header",
        type: "Symbol",
        required: true,
    });

    // Populate new field with text from existing linked header
    migration.transformEntries({
        contentType: contentType,
        from: ["header"],
        to: ["newHeader"],
        transformEntryForLocale: async (from, locale) => {
            if (!from.header) {
                return { newHeader: null };
            }
            const headerId = from.header[locale].sys.id;
            const header = await makeRequest({
                method: "GET",
                url: `/entries/${headerId}`,
            });
            return {
                newHeader:
                    header && header.fields["text"]
                        ? header.fields["text"][locale]
                        : null,
            };
        },
    });

    // delete the old header and replace it with the new one
    recommendationChunk.deleteField("header");
    recommendationChunk.changeFieldId("newHeader", "header");
    recommendationChunk.moveField("header").afterField("internalName");
    recommendationChunk.resetFieldControl("header");
};
