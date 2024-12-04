/**
 * Contentful migrations can break when entries have links to content that is "Missing or Inaccessible"
 * This script finds all entries which link to deleted/removed content and removes those references.
 */

const getClient = require("../helpers/get-client");

module.exports = async function () {
    const client = await getClient();
    await removeInvalidReferences(client);
};

/**
 * Goes through all entries removing invalid links within them
 *
 * @param {ClientAPI} client - Contentful management client instance.
 */
async function removeInvalidReferences(client) {
    const contentTypes = await client.contentType.getMany();
    const entryIds = await getAllValidEntryIds(client);

    for (const contentType of contentTypes.items) {
        await removeInvalidReferencesForContentType({
            client: client,
            validEntryIds: entryIds,
            contentType: contentType.sys.id,
        });
    }
}

/**
 * Finds all valid entryIds within contentful. This has to be split up by content type due to a max limit of 1000 items
 *
 * @param {ClientAPI} client - Contentful management client instance.
 */
async function getAllValidEntryIds(client) {
    console.log("Finding all valid entryIds");
    const contentTypes = await client.contentType.getMany();
    const entryIds = new Set();

    for (const contentType of contentTypes.items) {
        const entries = await client.entry.getMany({
            query: { limit: 1000, content_type: contentType.sys.id },
        });
        entries.items.map((entry) => entryIds.add(entry.sys.id));
    }

    return entryIds;
}

/**
 * Removes any invalid references for all entries of type contentType in Contentful
 * This could have been done without querying on contentType, however at most 1000 entries can be queried
 * at any one time, which isn't enough to return all of them unless limited by type
 *
 * @param {ClientAPI} params.client - Contentful management client instance.
 * @param {Set(string)} params.validEntryIds - ALl valid entry ids.
 * @param {string} params.contentType - The content type for which entries are being handled.
 */
async function removeInvalidReferencesForContentType({
    client,
    validEntryIds,
    contentType,
}) {
    console.log(`Processing contentType: ${contentType}`);
    const entries = await client.entry.getMany({
        query: { limit: 1000, content_type: contentType },
    });
    for (const entry of entries.items) {
        for (const [key, fieldValue] of Object.entries(entry.fields)) {
            for (const [locale, value] of Object.entries(fieldValue)) {
                processField({
                    client: client,
                    entry: entry,
                    validEntryIds: validEntryIds,
                    locale: locale,
                    fieldKey: key,
                    fieldValue: value,
                });
            }
        }
    }
}

/**
 * Processes a field in a Contentful entry and removes any invalid links.
 *
 * @param {ClientAPI} params.client - Contentful management client instance
 * @param {object} params.entry - The Contentful entry object being processed.
 * @param {Set<string>} params.validEntryIds - All valid entry Ids.
 * @param {string} params.locale - Locale of the field value.
 * @param {string} params.fieldKey - Key of the field in the entry.
 * @param {any} params.fieldValue - Value of the field in the entry.
 */
async function processField({
    client,
    entry,
    validEntryIds,
    locale,
    fieldKey,
    fieldValue,
} = {}) {
    if (Array.isArray(fieldValue)) {
        await removeMissingReferencesInArray({
            client: client,
            entry: entry,
            validEntryIds: validEntryIds,
            locale: locale,
            fieldKey: fieldKey,
            fieldItems: fieldValue,
        });
    } else if (fieldKey.sys?.type === "Link") {
        await removeLinkIfMissing({
            client: client,
            entry: entry,
            validEntryIds: validEntryIds,
            fieldKey: fieldKey,
        });
    }
}

/**
 * Checks a particular link on a contentful entry and removes it if it is missing/invalid.
 *
 * @param {ClientAPI} params.client - Contentful management client instance
 * @param {object} params.entry - The Contentful entry object being processed.
 * @param {Set<string>} params.validEntryIds - All valid entry Ids.
 * @param {string} params.fieldKey - Key of the field in the entry.
 */
async function removeLinkIfMissing({
    client,
    entry,
    validEntryIds,
    fieldKey,
} = {}) {
    if (!validEntryIds.has(fieldKey.sys?.id)) {
        console.log(
            `Removing invalid link in entry "${entry.sys.id}" field "${fieldKey}" value "${fieldKey.sys.id}}".`
        );
        delete entry.fields[fieldKey.sys?.id];
        await client.entry.update(
            { entryId: entry.sys.id },
            {
                sys: entry.sys,
                fields: entry.fields,
            }
        );
    }
}

/**
 * Removes any invalid/missing content items with in an array of entries.
 *
 * @param {ClientAPI} params.client - Contentful management client instance
 * @param {object} params.entry - The Contentful entry object being processed.
 * @param {Set<string>} params.validEntryIds - All valid entry Ids.
 * @param {string} params.locale - Locale of the field value.
 * @param {string} params.fieldKey - Key of the field in the entry.
 * @param {object[]} params.fieldItems - Value of the field in the entry.
 */
async function removeMissingReferencesInArray({
    client,
    entry,
    validEntryIds,
    locale,
    fieldKey,
    fieldItems,
} = {}) {
    let updated = false;
    let validReferences = [];
    for (const reference of fieldItems) {
        if (reference.sys?.type === "Link") {
            if (validEntryIds.has(reference.sys?.id)) {
                validReferences.push(reference);
            } else {
                console.log(
                    `Removing invalid link in entry "${entry.sys.id}" array "${fieldKey}" value "${reference.sys.id}}".`
                );
                updated = true;
            }
        }
    }
    if (updated) {
        entry.fields[fieldKey][locale] = validReferences;
        await client.entry.update(
            { entryId: entry.sys.id },
            {
                sys: entry.sys,
                fields: entry.fields,
            }
        );
    }
}
