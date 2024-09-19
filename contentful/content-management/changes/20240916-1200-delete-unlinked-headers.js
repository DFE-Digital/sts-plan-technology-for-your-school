const contentful = require("contentful-management");
const deleteEntry = require("../helpers/delete-entry");
const getClient = require("../helpers/get-client");

module.exports = async function () {
    const client = getClient();
    const headers = await client.entry.getMany({
        query: {
            content_type: "header",
        },
    });
    for (const header of headers.items) {
        const linked = await client.entry.getMany({
            query: {
                links_to_entry: header.sys.id,
            },
        });
        if (!linked.items.length) {
            await deleteEntry(header);
        }
    }
};
