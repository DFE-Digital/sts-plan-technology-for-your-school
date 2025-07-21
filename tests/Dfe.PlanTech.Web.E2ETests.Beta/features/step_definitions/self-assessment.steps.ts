import { Given, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

const selfAssessmentSlug = 'self-assessment';

Given('I am logged in and visit the self-assessment page', async function () {
  const page = this.page;
  await page.goto(`${process.env.URL}${selfAssessmentSlug}`);
});

Then('each section card should link to its details page', async function () {
  const page = this.page;
  const links = page.locator('div.dfe-card > a');
  const count = await links.count();

  for (let i = 0; i < count; i++) {
    const href = await links.nth(i).getAttribute('href');
    expect(href).not.toBeNull();
    expect(href).not.toEqual('');
  }
});

Then('I should see multiple section cards on the page', async function () {
  const sections = this.page.locator('div.dfe-card');
  const count = await sections.count();
  expect(count).toBeGreaterThan(1);
});