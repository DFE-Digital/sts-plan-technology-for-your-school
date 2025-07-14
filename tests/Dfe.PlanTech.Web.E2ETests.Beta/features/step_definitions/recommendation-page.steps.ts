import { When, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see the paragraph text {string} inside the recommendation content', async function (expectedText: string) {
  const paragraph = this.page.locator('.recommendation-piece-content p', { hasText: expectedText });
  await expect(paragraph).toBeVisible();
});


Then('I should see a visible Next pagination link with text {string}', async function (nextLinkText:string) {
  const link = this.page.locator('a.govuk-pagination__link:visible');
  const linkText = this.page.locator('span.govuk-pagination__link-label:visible');

  await expect(link).toHaveCount(1);
  await expect(link.first().locator('span.govuk-pagination__link-title:visible')).toContainText('Next');
  await expect(linkText).toHaveText(nextLinkText);
});

Then('I should see a visible Previous pagination link with text {string}', async function (expectedLabel: string) {
  const container = this.page.locator('.recommendation-piece-container:visible');
  const titleSpan = container.locator('.govuk-pagination__link-title').filter({ hasText: 'Previous' }).first();
  const labelSpan = container.locator('.govuk-pagination__link-label').filter({ hasText: expectedLabel }).first();

  await expect(titleSpan).toContainText("Previous");
  await expect(labelSpan).toContainText(expectedLabel);
});

Then('I click the next recommendation link', async function () {
  const link = this.page.locator('a.govuk-pagination__link[rel="next"]:visible');
  await link.click();

});

Then('I should see the recommendation caption text {string}', async function (expectedText: string) {
  const caption = this.page.locator('div.recommendation-piece-content span.govuk-caption-xl:visible');
  await expect(caption).toBeVisible();
  await expect(caption).toHaveText(expectedText);
});

Then('I should see the related actions sidebar', async function () {
  const sidebar = this.page.locator('div.govuk-grid-column-one-third:visible');
  await expect(sidebar).toBeVisible();
  await expect(sidebar.locator('h2')).toHaveText('Related actions');
});

Then('the sidebar link with text {string} should have href {string}', async function (linkText: string, expectedHref: string) {
  const link = this.page.locator('div.govuk-grid-column-one-third a:visible', { hasText: linkText });
  await expect(link).toBeVisible();
  await expect(link).toHaveAttribute('href', expectedHref);
});