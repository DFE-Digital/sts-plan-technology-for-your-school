type WebhookCreateRequestOptions = {
  ENVIRONMENT_ID: string;
  ENVIRONMENT_NAME: string;
  MANAGEMENT_TOKEN: string;
  SPACE_ID: string
  WEBHOOK_API_KEY: string;
  WEBHOOK_NAME: string;
  WEBHOOK_URL: string;
}

type Result<T> = {
  success: boolean,
  result?: T,
  error?: string
}

export type {
  Result,
  WebhookCreateRequestOptions
}