# Webhook creator

Simple Javascript script that creates a Contentful webhook, ensuring it follows the same pattern across environments

## Usage

1. Install required packages by running `npm install`
2. Copy `.env.example` and rename it to `.env`
3. Fill in the required variables as described below
4. Create the webhook by running `npm run create-webhook`  

## Environment Variables

| Variable         | Description                                                                                                            | Example                           |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| SPACE_ID         | Your Contentful space ID                                                                                               | coolspaceid                       |
| ENVIRONMENT_ID   | Your Contentful environment ID                                                                                         | master                            |
| ENVIRONMENT_NAME | The name that the webhook is for - should match the Azure environment but doesn't really matter. Used for webhook name | Dev                               |
| MANAGEMENT_TOKEN | Contentful management token                                                                                            | CFPAT-secrettokenhere             |
| WEBHOOK_NAME     | Prefix for the webhook name. Combined with ENVIRONMENT_NAME                                                            | Plan Tech                         |
| WEBHOOK_URL      | URL for the webhook to send data to                                                                                    | https://plan-tech.com/api/webhook |
| WEBHOOK_API_KEY  | The API key for the webhook                                                                                            | super-secret-bearer-token         |