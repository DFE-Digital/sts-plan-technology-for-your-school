import { Given, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Given('I visit a non-existent page', async function () {
  await this.page.goto(`${process.env.URL}some-slug-that-doesnt-exist`);
});

Then('I should see a contact us link', async function () {
  const container = this.page.getByRole('main');
  const link = container.locator(`a:has-text("contact us")`);
  await expect(link).toBeVisible();
});
