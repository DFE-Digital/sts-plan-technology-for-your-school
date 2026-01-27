import fs from 'fs';

const SleepTimeInMs = {
  BetweenGroups: 2000,
  BetweenContent: 50,
  OnError: 5000,
};

const MaxRetryCount = 5;

const insertedGroups = [];

export async function postEntriesToWebhook({ contents }) {
  let groupsMigrated = 1;
  let insertedItems = [];

  console.log(`Have ${contents.length} initial content to migrate`);

  while (contents.length > 0) {
    console.log(`Beginning migration of content. Running group ${groupsMigrated}`);

    const withoutReferences = getContentWithMigratedReferences(contents);

    console.log(`Found ${withoutReferences.length} entries for migrating`);

    const results = await getContentsAndPostToWebhook(withoutReferences);

    insertedGroups.push({
      groupNumber: groupsMigrated,
      entries: results.map((result) => ({
        ...result,
        entry: {
          contentType: result.entry.sys?.contentType?.sys?.id,
          id: result.entry?.sys?.id,
          name: result.entry.fields.internalName?.['en-US'],
        },
      })),
    });

    insertedItems = [...insertedItems, ...withoutReferences.map((content) => content.id)];

    console.log(
      `POSTed ${withoutReferences.length} items - total inserted now ${insertedItems.length}`,
    );

    contents = [...removeMigratedContent(contents, insertedItems)];

    console.log('items remaining ' + contents.length);

    groupsMigrated++;

    await sleep(SleepTimeInMs.BetweenGroups);
  }

  fs.writeFileSync('inserted-groups.json', JSON.stringify(insertedGroups));
}

function removeMigratedContent(contentWithReferences, insertedItems) {
  return contentWithReferences
    .filter((content) => !insertedItems.includes(content.id))
    .map((content) => removeMigratedReferences(content, insertedItems));
}

function removeMigratedReferences(content, insertedItems) {
  return {
    ...content,
    referencedByIds: content.referencedByIds.filter((id) => !insertedItems.includes(id)),
    referencedIds: content.referencedIds.filter((id) => !insertedItems.includes(id)),
  };
}

/**
 * Gets content with no remaining references
 * @param {*} contentWithReferences
 * @returns
 */
function getContentWithMigratedReferences(contentWithReferences) {
  return contentWithReferences.filter((content) => content.referencedIds.length == 0);
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Loops through content, posts content entry to webhook with minor wait inbetween
 * @param {*} content
 */
async function getContentsAndPostToWebhook(content) {
  const results = [];
  for (let x = 0; x < content.length; x++) {
    const entry = content[x].entry;

    const result = await tryPostContent(entry);
    results.push(result);

    await sleep(SleepTimeInMs.BetweenContent);
  }

  return results;
}

/**
 * Posts content to webhook, and retries on error.
 * @param {*} entry
 * @returns
 */
async function tryPostContent(entry) {
  let retryCount = 0;
  let errorSleepTime = SleepTimeInMs.OnError;

  while (true) {
    const result = await postToWebook(entry);

    if (result && result.success) {
      return result;
    }

    console.log('Error posting entry');
    retryCount++;

    if (retryCount >= MaxRetryCount) {
      console.log('Reached max retry count');
      return result;
    }

    console.log(`Waiting ${errorSleepTime / 1000} seconds`);
    await sleep(errorSleepTime);
    errorSleepTime *= 5;
    retryCount++;
  }
}

async function postToWebook(content) {
  const minifiedContent = minifyContent(content);

  const body = JSON.stringify(minifiedContent);

  try {
    const result = await fetch(process.env.FUNCTION_APP_URL, {
      body: body,
      method: 'POST',
      headers: {
        'X-Contentful-Topic': 'ContentManagement.Entry.publish',
        'X-Contentful-Webhook-Name': 'Azure Function - Staging',
        'X-Contentful-Webhook-Request-Attempt': '1',
        'X-Contentful-Event-Datetime': '2024-05-14T09:21:37.356Z',
        'Content-Type': 'application/vnd.contentful.management.v1+json',
        'X-Contentful-CRN':
          'crn:contentful:::content:spaces/py5afvqdlxgo/environments/master/entries/16FlMU8PF9zYVK9Cxqh0TK',
        'X-Contentful-Idempotency-Key':
          '984411c33c2bb09cd835ff44e66d74ea65b65247e3640e9698b19f85762b272f',
      },
    });

    if (!result.ok) {
      throw 'error ' + result.status + ' ' + result.text();
    }

    return {
      success: true,
      entry: minifiedContent,
    };
  } catch (e) {
    console.log(
      'Error posting content ' + content.sys.id + '  ' + e,
      content,
      JSON.stringify(content),
    );
    return {
      success: false,
      entry: minifiedContent,
      error: e,
    };
  }
}

/**
 * Removes various unneeded fields from content, then returns it as JSON string
 * @param {*} content
 * @returns
 */
function minifyContent(content) {
  return {
    sys: {
      id: content.sys.id,
      type: content.sys.type,
      contentType: content.sys.contentType,
    },
    fields: content.fields,
  };
}
