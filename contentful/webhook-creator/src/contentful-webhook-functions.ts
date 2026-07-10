import type {
  CreateWebhooksProps,
  Space,
  WebhookFilter,
  WebHooks,
  Collection,
  WebhookProps,
} from 'contentful-management';
import type { WebhookCreateRequestOptions } from './types';

/**
 * Fetches webhook options from env variables, then creates a webhook.
 * @returns
 */
const createOrUpdateWebhook = async () => {
  try {
    const { getOptions } = await import('./options');
    const { result: options, success, error } = getOptions();

    if (error || !success || !options) {
      const errorMessage = `Error retrieving environment variable options: ${
        error ?? 'unknown error'
      }`;
      console.error(errorMessage);
      throw Error(errorMessage);
    }

    const webhookOptions = createWebhookOptions(options);

    const space = await getContentfulSpaceClient(options);

    if (!space) {
      return;
    }

    console.log('Fetching existing Contentful webhooks');
    const webhooks = await space.getWebhooks();

    const matchingWebhook = getExistingWebhookByUrl(webhooks, options);

    if (matchingWebhook) {
      console.log('Found existing webhook');
      await updateWebhook(webhookOptions, matchingWebhook);
      return;
    }

    await createWebhook(space, webhookOptions);
  } catch (err) {
    console.error(`Error creating or updating webhook`, err);
    throw Error(err);
  }
};

const getContentfulSpaceClient = async (options: WebhookCreateRequestOptions): Promise<Space> => {
  const { createClient } = await import('contentful-management');

  try {
    console.log('Creating Contentful management client');

    const client = createClient({
      accessToken: options.MANAGEMENT_TOKEN,
    });

    return await client.getSpace(options.SPACE_ID);
  } catch (err) {
    console.error(`Error creating Contentful space client`, err);
    throw Error(err);
  }
};

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
  const authHeader = {
    key: 'Authorization',
    value: `Bearer ${options.WEBHOOK_API_KEY}`,
    secret: true,
  };
  const filterByEnvironmentId: WebhookFilter = {
    equals: [{ doc: 'sys.environment.sys.id' }, options.ENVIRONMENT_ID],
  };
  const topics = ['Entry.*'];

  const webhookOptions: CreateWebhooksProps = {
    filters: [filterByEnvironmentId],
    name: `${options.WEBHOOK_NAME} - ${options.ENVIRONMENT_NAME}`,
    url: options.WEBHOOK_URL,
    topics: topics,
    headers: [authHeader],
  };

  return webhookOptions;
};

const updateWebhook = async (options: CreateWebhooksProps, webhook: WebHooks) => {
  try {
    webhook.headers = options.headers ?? [];
    webhook.url = options.url;
    webhook.name = options.name;
    webhook.filters = options.filters ?? [];
    webhook.topics = options.topics ?? [];

    console.log('Starting update of existing webhook', webhook);

    await webhook.update();

    console.log(`Successfully updated webhook with ID: ${webhook.sys.id}`);
  } catch (err) {
    console.error(`Error updating webhook`, err);
    throw Error(err);
  }
};

/**
 * Returns existing webhook that matches the webhook URL
 */
const getExistingWebhookByUrl = (
  webhooks: Collection<WebHooks, WebhookProps>,
  options: WebhookCreateRequestOptions,
) => webhooks.items.find((webhook) => webhook.url == options.WEBHOOK_URL);

const createWebhook = async (space: Space, webhookOptions: CreateWebhooksProps) => {
  try {
    console.log(`Starting creation of new webhook`);
    const result = await space.createWebhook(webhookOptions);
    console.log(`Successfully created webhook`, result);
    return result;
  } catch (err) {
    console.error(`Error creating webhook`, err);
    throw Error(err);
  }
};

export {
  createOrUpdateWebhook,
  createWebhook,
  createWebhookOptions,
  getExistingWebhookByUrl,
  getContentfulSpaceClient,
};
