import { Given, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see the phase banner', async function () {
  const banner = this.page.locator('div.govuk-phase-banner');
  await expect(banner).toBeVisible();
});

Then('I should see the phase banner tag with text {string}', async function (expectedText: string) {
  const tag = this.page.locator('div.govuk-phase-banner strong.govuk-tag');
  await expect(tag).toBeVisible();
  await expect(tag).toHaveText(expectedText);
});

Then('I should see a feedback link in the phase banner with href containing {string}', async function (expectedHref: string) {
  const link = this.page.locator('div.govuk-phase-banner a');
  await expect(link).toBeVisible();
  await expect(link).toHaveAttribute('href', expect.stringContaining(expectedHref));
});
