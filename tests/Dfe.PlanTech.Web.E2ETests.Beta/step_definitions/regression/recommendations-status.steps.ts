import { Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then(
  'recommendation {string} should have status {string}',
  async function (recommendationHeader: string, expectedStatus: string) {
    const link = this.page.locator(`a[data-recommendation-header^="${recommendationHeader}"]`);

    await expect(link, `No recommendation found for "${recommendationHeader}"`).toHaveCount(1);

    const row = this.page.locator('tr.govuk-table__row', { has: link });

    const statusTag = row.locator('.govuk-tag');

    await expect(statusTag).toHaveText(expectedStatus);
  },
);

When(
  'I change the recommendation status to {string} and save it',
  async function (newStatus: string) {
    await this.page.getByRole('radio', { name: newStatus }).check();

    await this.page.getByRole('button', { name: 'Save' }).click();

    await this.page.waitForLoadState('networkidle');
  },
);
