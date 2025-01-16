/**
 * Deletes content from Contentful based on the given client and content types.
 *
 * @param {Object} options - The options for deleting content.
 * @param {contentful.ClientAPI} options.client - The Contentful client.
 * @param {import('./types').ContentfulEnvironmentOptions} options.envOptions - Environment options for the Contentful client.
 * @param {(string[] | undefined)} options.contentTypes - The array of content types to delete. If undefined, will delete _all_ content
 * @return {Promise} A promise that resolves when the content is deleted.
 */
module.exports = async function deleteContentfulContent({ client, contentTypes }) {
    if (!client) {
        throw `Contentful client not provided`;
    }

    if (!contentTypes || !Array.isArray(contentTypes) || contentTypes.length == 0) {
        return await deleteContentFromContentful({ client });
    }
    else {
        return await deleteContentByContentTypes({ client, contentTypes });
    }
}

async function deleteContentByContentTypes({ client, contentTypes }) {
    for (const contentType of contentTypes) {
        console.log(`Deleting content for ${contentType}`);
        await deleteContentFromContentful({ client, query: { content_type: contentType } });
    }
}

async function deleteContentFromContentful({ client, envOptions, query }) {
    let limit = 100;

    while (true) {
        const entries = await client.entry.getMany({
            query: {
                skip: 0,
                limit: limit,
                ...query
            },
        });

        if (entries.items.length == 0) {
            break;
        }

        console.log(`Deleting ${entries.items.length} items out of ${entries.total}`);

        for (const entry of entries.items) {
            console.log(entry)
            await unpublishAndDeleteEntry({ entry, client });
        }
    }
}

async function unpublishAndDeleteEntry({ entry, client }) {
    if (entry.sys.publishedVersion) {
        await client.entry.unpublish({
            entryId: entry.sys.id,
        });
    }

    await client.entry.delete({
        entryId: entry.sys.id
    });
}
