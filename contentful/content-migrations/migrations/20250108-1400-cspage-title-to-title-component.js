const { v4: uuidv4 } = require('uuid');

module.exports = function (migration) {
  migration.transformEntriesToType({
    sourceContentType: 'CSHeading',
    targetContentType: 'title',
    from: ['Title'],
    identityKey: function () {
      return uuidv4();
    },
    transformEntryForLocale: (fromFields, currentLocale) => {
      const titleText = fromFields.Title[currentLocale];
      if (!titleText) {
        return;
      }
      return {
        internalName: `${titleText} - csPage - title`,
        text: titleText,
      };
    },
    shouldPublish: false,
    updateReferences: true,
  });
};
