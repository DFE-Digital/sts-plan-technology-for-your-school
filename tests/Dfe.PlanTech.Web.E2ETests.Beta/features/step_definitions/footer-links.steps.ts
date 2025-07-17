import { Given, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see the footer', async function () {
  const footer = this.page.locator('footer.govuk-footer');
  await expect(footer).toBeVisible();
});

Then('I should see a footer link with text {string} and href containing {string}', async function (linkText: string, expectedHref: string) {
  const link = this.page.locator('footer.govuk-footer a.govuk-footer__link', {
    hasText: linkText,
  });

  await expect(link).toBeVisible();
  await expect(link).toHaveAttribute('href', expect.stringContaining(expectedHref));
});
