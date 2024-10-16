import { WebhookHeader, WebHooks } from "contentful-management/dist/typings/entities/webhook";
import { getOptions } from "./options";
import { WebhookCreateRequestOptions } from "./types";
import { createClient, CreateWebhooksProps, Space, WebhookFilter } from "contentful-management";

/**
 * Fetches webhook options from env variables, then creates a webhook.
 * @returns 
 */
const createOrUpdateWebhook = async () => {
  const { result: options, success, error } = getOptions();

  if (error || !success || !options) {
    console.error(`Error retrieving environment variable options: ${error ?? "unknown error"}`);
    return;
  }

  const space = await getContentfulSpaceClient(options);
  const webhookOptions = createWebhookOptions(options);

  const webhooks = await space.getWebhooks();

  const matchingWebhook = webhooks.items.find((webhook) => webhook.url == options.WEBHOOK_URL);

  if (matchingWebhook) {
    await updateWebhook(webhookOptions, matchingWebhook);
    return;
  }

  await createWebhook(space, webhookOptions);
}


const getContentfulSpaceClient = (options: WebhookCreateRequestOptions): Promise<Space> => {
  try {
    const client = createClient(
      {
        accessToken: options.MANAGEMENT_TOKEN,
      });

    return client.getSpace(options.SPACE_ID);
  }
  catch (err) {
    console.error(`Error creating Contentful space client`, err);
    throw err;
  }
}



/**
 * Creates webhook creation options where:
 * - We set an authorization header with a bearer token
 * - We enable all "entry" topic changes
 * - We filter out changes by specific environment
 * 
 * @param {WebhookCreateRequestOptions} options 
 * @returns {CreateWebhooksProps}
 */
const createWebhookOptions = (options: WebhookCreateRequestOptions): CreateWebhooksProps => {
  const authHeader: WebhookHeader = { key: "Authorization", value: `Bearer ${options.WEBHOOK_API_KEY}`, secret: true };
  const filterByEnvironmentId: WebhookFilter = { equals: [{ doc: "sys.environment.sys.id" }, options.ENVIRONMENT_ID] };
  const topics = ["Entry.*"];

  const webhookOptions: CreateWebhooksProps = {
    filters: [filterByEnvironmentId],
    name: `${options.WEBHOOK_NAME} - ${options.ENVIRONMENT_NAME}`,
    url: options.WEBHOOK_URL,
    topics: topics,
    headers: [authHeader]
  };

  return webhookOptions;
}

const updateWebhook = async (options: CreateWebhooksProps, webhook: WebHooks) => {
  try {
    webhook.headers = options.headers ?? [];
    webhook.url = options.url;
    webhook.name = options.name;
    webhook.filters = options.filters ?? [];
    webhook.topics = options.topics ?? [];

    await webhook.update();
    console.log(`Successfully updated webhook with ID: ${webhook.sys.id}`);
  }
  catch (err) {
    console.error(`Error updating webhook`, err);
  }
}

const createWebhook = async (space: Space, webhookOptions: CreateWebhooksProps) => {
  try {
    const result = await space.createWebhook(webhookOptions);
    console.log(`Successfully created webhook with ID: ${result.sys.id}`);
  }
  catch (err) {
    console.error(`Error creating webhook`, err);
  }
}


createOrUpdateWebhook();
