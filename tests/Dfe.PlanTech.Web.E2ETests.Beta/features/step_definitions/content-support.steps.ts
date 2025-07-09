import { Given, When, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Given('I visit the content support page', async function () {
  await this.page.goto(`${process.env.URL}information-asset-register`);
});

Then('I should see an attachment block', async function () {
  await expect(this.page.locator('.attachment')).toBeVisible();
});

Then('the attachment should include a downloadable link', async function () {
  const link = this.page.locator('.attachment-link');
  await expect(link).toBeVisible();
  const href = await link.getAttribute('href');
  expect(href).not.toBeNull();
  expect(href).not.toEqual('');
});

Then('I should see an accordion component', async function () {
  await expect(this.page.locator('.govuk-accordion')).toBeVisible();
});

Then('I should see a control labelled {string}', async function (text: string) {
  const control = this.page.locator('.govuk-accordion__controls');
  await expect(control).toBeVisible();
  await expect(control).toContainText(text);
});

Then('I should see multiple accordion body sections', async function () {
  const sections = this.page.locator('.govuk-accordion__section .govuk-body');
  expect(await sections.count()).toBeGreaterThan(1);
});

Then('I should see multiple accordion toggle labels', async function () {
  const toggles = this.page.locator('.govuk-accordion__section-toggle-text');
  expect(await toggles.count()).toBeGreaterThan(1);
});

Then('all accordion sections should be collapsed', async function () {
  const contents = this.page.locator('.govuk-accordion__section-content');
  const count = await contents.count();
  for (let i = 0; i < count; i++) {
    const section = contents.nth(i);
    const hidden = await section.getAttribute('hidden');
    expect(hidden).not.toBeNull();
  }
});

When('I expand the first accordion section', async function () {
  const button = this.page.locator('.govuk-accordion__section-button').first();
  await button.click();
});

When('I collapse the first accordion section', async function () {
  const button = this.page.locator('.govuk-accordion__section-button').first();
  await button.click();
});

Then('the first accordion section should be expanded', async function () {
  const content = this.page.locator('.govuk-accordion__section-content').first();
  const hidden = await content.getAttribute('hidden');
  expect(hidden).toBeNull();
});

Then('the first accordion section should be collapsed', async function () {
  const content = this.page.locator('.govuk-accordion__section-content').first();
  const hidden = await content.getAttribute('hidden');
  expect(hidden).not.toBeNull();
});
