import { Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then('I should see a link to log into DfE Sign-in', async function () {
  const loginLink = this.page.getByRole('link', { name: 'DfE Sign-in account (opens in new tab)' });

  await expect(loginLink).toBeVisible();
  await expect(loginLink).toHaveAttribute('href', 'https://services.signin.education.gov.uk/');
  await expect(loginLink).toHaveAttribute('target', '_blank');
  await expect(loginLink).toHaveAttribute('rel', 'noopener');
});