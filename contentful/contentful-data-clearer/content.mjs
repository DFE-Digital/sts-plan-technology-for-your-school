import contentful from 'contentful-management';

/**
 * Deletes content from Contentful based on the given client and content types.
 *
 * @param {Object} options - The options for deleting content.
 * @param {contentful.ClientAPI} options.client - The Contentful client.
 * @param {import('./types').ContentfulEnvironmentOptions} options.envOptions - Environment options for the Contentful client.
 * @param {(string[] | undefined)} options.contentTypes - The array of content types to delete. If undefined, will delete _all_ content
 * @return {Promise} A promise that resolves when the content is deleted.
 */
export function deleteContentfulContent({ client, envOptions, contentTypes }) {
  if (!client) {
    throw `Contentful client not provided`;
  }

  if (!envOptions || !envOptions.spaceId || !envOptions.environmentId) {
    throw `Contentful environment options not provided`;
  }

  if (!contentTypes || !Array.isArray(contentTypes) || contentTypes.length == 0) {
    return deleteContentFromContentful({ client });
  }
  else {
    return deleteContentByContentTypes({ client, contentTypes });
  }
}

async function deleteContentByContentTypes({ client, envOptions, contentTypes }) {
  for (const contentType of contentTypes) {
    console.log(`Deleting content for ${contentType}`);
    await deleteContentFromContentful({ client, envOptions, query: { content_type: contentType } });
  }
}

async function deleteContentFromContentful({ client, envOptions, query }) {
  let limit = 100;

  while (true) {
    const entries = await client.entry.getMany({
      ...envOptions,
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
      await unpublishAndDeleteEntry({ entry, client });
    }
  }
}

async function unpublishAndDeleteEntry({ entry, client, envOptions }) {
  if (entry.sys.publishedVersion) {
    await client.entry.unpublish({
      ...envOptions,
      entryId: entry.sys.id,
    });
  }

  await client.entry.delete({
    ...envOptions,
    entryId: entry.sys.id
  });
}
