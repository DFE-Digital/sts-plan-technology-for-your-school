import { WebhookHeader } from "contentful-management/dist/typings/entities/webhook";
import { getOptions } from "./options";
import { WebhookCreateRequestOptions } from "./types";
import { createClient, CreateWebhooksProps, WebhookFilter } from "contentful-management";

/**
 * Fetches webhook options from env variables, then creates a webhook.
 * @returns 
 */
const createWebhook = async () => {
  const { result: options, success, error } = getOptions();

  if (error || !success || !options) {
    console.error(`Error retrieving environment variable options: ${error ?? "unknown error"}`);
    return;
  }

  const client = createClient(
    {
      accessToken: options.MANAGEMENT_TOKEN
    },
    {
      type: 'plain',
      defaults: {
        spaceId: options.SPACE_ID,
        environmentId: options.ENVIRONMENT_ID
      },
    }
  )

  try {
    const webhookOptions = createWebhookOptions(options);
    const result = await client.webhook.create({}, webhookOptions);

    console.log(`Successfully created webhook with ID: ${result.sys.id}`);
  }
  catch (err) {
    console.error(`Error creating webhook`, err);
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

createWebhook();

