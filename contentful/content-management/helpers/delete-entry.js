import { getAndValidateClient } from "./get-client.js";

export default async function deleteEntry(entry) {
    const client = await getAndValidateClient();
    if (entry.sys.publishedVersion) {
        console.log(`unpublishing ${entry.sys.id}`);
        await client.entry.unpublish({
            entryId: entry.sys.id,
        });
    }
    console.log(`deleting ${entry.sys.id}`);
    await client.entry.delete({
        entryId: entry.sys.id,
    });
}
