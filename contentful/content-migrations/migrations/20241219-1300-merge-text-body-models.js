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
            return {
                internalName: fromFields.InternalName[currentLocale],
                richText: fromFields.RichText[currentLocale],
            };
        }
    });
};