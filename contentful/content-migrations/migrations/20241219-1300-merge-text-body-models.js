const { v4: uuidv4 } = require('uuid');

module.exports = function (migration) {
    const targetContentType = "textBody";

    migration.transformEntriesToType({
        sourceContentType: "CSBodyText",
        targetContentType: targetContentType,
        from: ["InternalName", "RichText"],
		to: ["internalName", "richText"],
        updateReferences: true,
        identityKey: function () {
            return uuidv4();
        },
        transformEntryForLocale: function (fromFields, currentLocale) {
            const internalName = fromFields.InternalName?.[currentLocale];
            const richText = fromFields.RichText?.[currentLocale];
            
            if (!internalName && !richText) {
                // Skip transformation if both fields are missing or undefined
                return null;
            }
            return {
                internalName: fromFields.InternalName[currentLocale],
                richText: fromFields.RichText[currentLocale],
            };
        }
    });
};