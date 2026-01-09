import { Given, Then } from '@cucumber/cucumber';

Given('I visit the landing page', async function () {
  await this.page.goto(`${process.env.URL}`);
});
