import dotenv from "dotenv";
import { Result, WebhookCreateRequestOptions } from "./types.js";

const getOptions = (): Result<WebhookCreateRequestOptions> => {
  dotenv.config();
  const variables: WebhookCreateRequestOptions = {
    MANAGEMENT_TOKEN: process.env.MANAGEMENT_TOKEN ?? "",
    ENVIRONMENT_ID: process.env.ENVIRONMENT_ID ?? "",
    SPACE_ID: process.env.SPACE_ID ?? "",
    ENVIRONMENT_NAME: process.env.ENVIRONMENT_NAME ?? "",
    WEBHOOK_API_KEY: process.env.WEBHOOK_API_KEY ?? "",
    WEBHOOK_NAME: process.env.WEBHOOK_NAME ?? "",
    WEBHOOK_URL: process.env.WEBHOOK_URL ?? "",
  };

  return validateOptions(variables);
};

const validateOptions = (
  WebhookCreateRequestOptions: WebhookCreateRequestOptions
): Result<WebhookCreateRequestOptions> => {
  const keys: (keyof WebhookCreateRequestOptions)[] = [
    "MANAGEMENT_TOKEN",
    "ENVIRONMENT_ID",
    "SPACE_ID",
    "ENVIRONMENT_NAME",
    "WEBHOOK_API_KEY",
    "WEBHOOK_NAME",
    "WEBHOOK_URL",
  ];

  const keyIsValid = (key: keyof WebhookCreateRequestOptions): boolean =>
    WebhookCreateRequestOptions[key] == "" ||
    WebhookCreateRequestOptions[key] == undefined;
  const createErrorMessage = (key: keyof WebhookCreateRequestOptions): string =>
    `Missing ${key}`;

  const errors = keys.filter(keyIsValid).map(createErrorMessage);

  if (errors.length > 0) {
    return {
      success: false,
      result: undefined,
      error: errors.join(","),
    };
  }

  return {
    success: true,
    result: WebhookCreateRequestOptions,
  };
};

export { getOptions };
