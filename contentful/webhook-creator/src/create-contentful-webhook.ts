import { createOrUpdateWebhook } from './contentful-webhook-functions';

console.log('Starting upsert of Contentful webhook');
await createOrUpdateWebhook();
console.log('Finished upsert of Contentful webhook');
