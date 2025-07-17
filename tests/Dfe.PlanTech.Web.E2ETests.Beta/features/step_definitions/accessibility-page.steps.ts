import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';


Given('I am on the accessibility page', async function () {
  await this.page.goto(`${process.env.URL}accessibility-statement`);
  await expect(this.page).toHaveURL(/\/accessibility-statement/);
});

Then('I should see the accessibility page heading', async function () {
  const heading = this.page.getByRole('heading', { level: 1 });
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(/Accessibility statement for Plan technology for your school/);
});

Then('I should see a back link to the previous page', async function () {
  const backLink = this.page.getByRole('link', { name: /back/i });
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
