import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Given('I am on the cookies page', async function () {
  await this.page.goto(`${process.env.URL}cookies`);
});

Then('I should see a back link with href containing {string}', async function (hrefPart: string) {
  const backLink = this.page.locator('a.govuk-back-link');
  await expect(backLink).toBeVisible();
  const href = await backLink.getAttribute('href');
  expect(href).toContain(hrefPart);
});

Then('I should see multiple paragraphs of explanatory text', async function () {
  const paragraphs = this.page.locator('main p');
  expect(await paragraphs.count()).toBeGreaterThan(2);
});

Then(
  'I should see a cookie preferences form with {int} radio options',
  async function (count: number) {
    const radios = this.page.locator('form .govuk-radios__item');
    await expect(radios).toHaveCount(count);
  },
);

When('I choose to accept cookies and save settings', async function () {
  await this.page.locator('input#analytics-cookies-yes').check();
  await this.page.getByRole('button', { name: 'Save cookie settings' }).click();
});

When('I choose to reject cookies and save settings', async function () {
  await this.page.locator('input#analytics-cookies-no').check();
  await this.page.getByRole('button', { name: 'Save cookie settings' }).click();
});

Then('I should see a notification banner confirming the action', async function () {
  const banner = this.page.locator('.govuk-notification-banner__header');
  await expect(banner).toBeVisible();
});

Then('Google Tag Manager should be enabled', async function () {
  await this.page.waitForFunction(
    () => {
      return !!document.querySelector('script[src*="googletagmanager"]');
    },
    { timeout: 10000 },
  );

  const hasMeta = await this.page.locator('meta[name="google-site-verification"]').count();
  expect(hasMeta).toBeGreaterThan(0);

  const hasScript = await this.page.locator('head script[src*="googletagmanager"]').count();
  expect(hasScript).toBeGreaterThan(0);

  const noScriptText = await this.page.locator('noscript').allTextContents();
  const hasGtmNoScript = noScriptText.some((t: string | string[]) =>
    t.includes('www.googletagmanager.com'),
  );
  expect(hasGtmNoScript).toBe(true);
});

Then('Google Tag Manager should be disabled', async function () {
  await expect(this.page.locator('meta[name="google-site-verification"]')).toHaveCount(0);
  const noScriptText = await this.page.locator('noscript').allTextContents();
  const hasGTM = noScriptText.some((t: string | string[]) =>
    t.includes('www.googletagmanager.com'),
  );
  expect(hasGTM).toBe(false);
  await expect(this.page.locator('head script[src*="googletagmanager"]')).toHaveCount(0);
});
