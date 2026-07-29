# Webhook Creator

A TypeScript tool for idempotently creating or updating a Contentful webhook. Used during environment provisioning to ensure all environments have a consistently configured webhook pointing at the application's CMS ingress endpoint.

## What it does

The tool performs an **upsert**: it fetches the list of existing webhooks, looks for one whose URL matches `WEBHOOK_URL`, and either updates it in-place or creates a new one. This means it is safe to run repeatedly without creating duplicates.

The webhook it creates is configured to:
- Fire on all `Entry.*` topic changes
- Filter by environment ID (so a single Contentful space doesn't flood multiple environments)
- Send a `Bearer` authorisation header to authenticate with the application's webhook API

## Setup

```bash
cd contentful/webhook-creator
cp .env.example .env   # fill in values
npm install
```

## Environment variables

| Variable | Description | Example |
|---|---|---|
| `SPACE_ID` | Contentful space ID | `abc123` |
| `ENVIRONMENT_ID` | Contentful environment ID used to filter webhook events | `master` |
| `ENVIRONMENT_NAME` | Display label appended to the webhook name | `Dev` |
| `MANAGEMENT_TOKEN` | Contentful management API token | `CFPAT-...` |
| `WEBHOOK_NAME` | Prefix for the webhook name. Combined with `ENVIRONMENT_NAME` | `Plan Tech` |
| `WEBHOOK_URL` | The target URL the webhook will POST to | `https://plan-tech.example.com/api/webhook` |
| `WEBHOOK_API_KEY` | Bearer token sent in the `Authorization` header | `super-secret-key` |

## Running

```bash
npm run create-webhook
```

This compiles the TypeScript and runs `dist/create-contentful-webhook.js`.

## Language and tooling

TypeScript 5, compiled with `tsc`. Runtime: Node.js ES modules (`"type": "module"`). No test suite.

## See also

- [Service Bus infrastructure](../../src/Dfe.PlanTech.Infrastructure.ServiceBus/README.md) — processes the webhook events this tool configures
- [Terraform infrastructure](../../terraform/README.md) — webhook is also provisioned as part of the container-app module
- [Contentful tooling overview](../README.md)
