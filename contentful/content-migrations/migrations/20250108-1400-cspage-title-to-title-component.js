const MurmurHash3 = require('imurmurhash');

module.exports = function (migration) {
    migration.transformEntriesToType({
        sourceContentType: 'CSHeading',
        targetContentType: 'title',
        from: ['Title'],
        identityKey: (fields) => {
            
            return `${MurmurHash3(fields.Title['en-US']).result().toString()}-${Date.now()}`;
        },
        transformEntryForLocale: (fromFields, currentLocale) => {
            const titleText = fromFields.Title[currentLocale];
            if (!titleText) {
                return; 
            }
            return {
                internalName: `${titleText} - csPage - title`, 
                text: titleText 
            };
        },
        shouldPublish: false,
        updateReferences: true
    });
};
