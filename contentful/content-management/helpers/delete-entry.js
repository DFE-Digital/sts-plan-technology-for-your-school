const contentful = require("contentful-management");
const getClient = require("../helpers/get-client");

module.exports = async function deleteEntry(entry) {
    const client = getClient();
    if (!!entry.sys.publishedVersion) {
        console.log(`unpublishing ${entry.sys.id}`);
        await client.entry.unpublish({
            entryId: entry.sys.id,
        });
    }
    console.log(`deleting ${entry.sys.id}`);
    await client.entry.delete({
        entryId: entry.sys.id,
    });
};
