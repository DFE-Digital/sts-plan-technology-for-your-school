/**
 * We do not use underlining in the service, but there is underlining in some rich text fields. This is causing errors to be logged.
 * This script finds all textBody entries with underlining, removes it and republishes if the entry was previously published.
 */

import getClient from "../helpers/get-client.js";

export default async function () {
    const client = await getClient();
    await updateEntries(client);
}

const CONTENT_TYPE_ID = "textBody";
const RICH_TEXT_FIELD_ID = "richText";
const LOCALE = "en-US";
const DRY_RUN = false;

/**
 * Recursively removes underline marks from Rich Text node
 */
function removeUnderline(node) {
    if (Array.isArray(node)) {
        return node.map(removeUnderline);
    }

    if (typeof node === "object" && node !== null) {
        if (Array.isArray(node.marks)) {
            node.marks = node.marks.filter(m => m.type !== "underline");
        }

        if (Array.isArray(node.content)) {
            node.content = node.content.map(removeUnderline);
        }
    }

    return node;
}

async function getAndUpdateEntry(fullEntry, status, cleaned) {
    fullEntry.fields[RICH_TEXT_FIELD_ID][LOCALE] = cleaned;

    const updatedEntry = await fullEntry.update();

    if (status === "published") {
        try {
            console.log(`Existing entry ${fullEntry.sys.id} had status 'published', re-publishing'`)
            await updatedEntry.publish();
        } catch (err) {
            console.log(`  Publish failed for ${fullEntry.sys.id}: ${err.message}`);
        }
    } else {
        console.log(
            `Entry ${fullEntry.sys.id} has status '${status}', skipping publish step.`
        );
    }
}


/**
 * Goes through all textBody entries removing underlining marks and replacing
 *
 * @param {ClientAPI} client - Contentful management client instance.
 */
async function updateEntries(client) {
    const entries = await client.getEntries({
        limit: 1000,
        content_type: CONTENT_TYPE_ID,
        "sys.archivedAt[exists]": false
    });

    let updated = 0;

    for (const entry of entries.items) {
        const fullEntry = await client.getEntry(entry.sys.id);

        if (fullEntry.sys.archivedAt) continue;

        const sysObj = fullEntry.sys;
        const fields = fullEntry.fields || {};
        const fieldByLocale = fields[RICH_TEXT_FIELD_ID];
        const fieldStatus = sysObj.fieldStatus;

        let status;
        if (fieldStatus && fieldStatus["*"]) {
            status = fieldStatus["*"][LOCALE] || "None";
        } else {
            status = "None";
        }

        if (!fieldByLocale || typeof fieldByLocale !== "object") continue;

        const richText = fieldByLocale[LOCALE];
        if (!richText) continue;

        const original = JSON.parse(JSON.stringify(richText));
        const cleaned = removeUnderline(JSON.parse(JSON.stringify(richText)));

        const changed = JSON.stringify(original) !== JSON.stringify(cleaned);
        if (!changed) continue;

        if (!DRY_RUN) {
            await getAndUpdateEntry(fullEntry, status, cleaned)
        }
        else if (DRY_RUN) {
            if (status === "published") {
                console.log(`Published entry ${entry.sys.id}, publishing update`)
            } else {
                console.log(`Entry ${entry.sys.id} has status '${status}', skipping publish step.`)
            }
        }

        updated++;
    }

    console.log(`Done. Entries updated: ${updated}`);
}
