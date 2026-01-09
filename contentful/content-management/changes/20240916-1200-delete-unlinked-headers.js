import deleteEntry from '../helpers/delete-entry.js';
import getClient from '../helpers/get-client.js';

export default async function () {
  console.log(`Deleting unlinked headers`);

  const client = await getClient();
  const headers = await client.entry.getMany({
    query: {
      content_type: 'header',
    },
  });

  for (const header of headers.items) {
    console.log(`Processing header ${header.sys.id}`);
    const linked = await client.entry.getMany({
      query: {
        links_to_entry: header.sys.id,
      },
    });

    if (!linked.items.length) {
      console.log(`Deleting header ${header.sys.id} as it has no links to it.`);
      await deleteEntry(header);
    }
  }
}
