import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Given('I am on the accessibility page', async function () {
  await this.page.goto(`${process.env.URL}accessibility-statement`);
  await expect(this.page).toHaveURL(/\/accessibility-statement/);
});

Then('I should see a back link to the previous page', async function () {
  const backLink = this.page.locator('a#back-button-link', { hasText: /back/i });
  await expect(backLink).toBeVisible();

  const href = await backLink.getAttribute('href');
  expect(href).toContain('/');
});

Then('I should see explanatory accessibility content', async function () {
  const paragraphs = this.page.locator('main p');
  await expect(paragraphs.first()).toBeVisible();
  expect(await paragraphs.count()).toBeGreaterThan(3);
});

Then('I should see the following accessibility section headings:', async function (dataTable) {
  const rows = dataTable.raw().flat();
  for (const headingText of rows) {
    const heading = this.page.getByRole('heading', { name: headingText });
    await expect(heading).toBeVisible();
  }
});
