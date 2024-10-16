# Webhook creator

Simple Javascript script that creates a Contentful webhook, ensuring it follows the same pattern across environments

## Usage

1. Install required packages by running `npm install`
2. Copy `.env.example` and rename it to `.env`
3. Fill in the required variables as described below
4. Build the project by running `npm run build`
5. Execute the script by running `node ./dist/create-contentful-webhook.js`

## Environment Variables

| Variable         | Description                         | Example                     |
| ---------------- | ----------------------------------- | --------------------------- |
| SPACE_ID         | Your Contentful space ID            |                             |
| ENVIRONMENT_ID   | Your Contentful environment ID      |                             |
| MANAGEMENT_TOKEN | Contentful management token         |                             |
| WEBHOOK_NAME     | Name of webhook to create           |                             |
| WEBHOOK_URL      | URL for the webhook to send data to | https://example.com/webhook |
| WEBHOOK_API_KEY  |                                     |                             |