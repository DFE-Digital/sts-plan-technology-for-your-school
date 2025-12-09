import { Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see the beta header',
  async function () {
    //check the banner exists
    const header = this.page.locator('div.govuk-phase-banner');
    await expect(header).toBeVisible();

    // check tag
    const tag = header.locator('strong.govuk-tag');
    await expect(tag).toBeVisible();
    await expect(tag).toHaveText('Beta');

    // ensure the feedback link exists (this is hardcoded in the backend beta-header.cshtml file)
    const expectedHref = 'https://forms.office.com/e/Jk5PuNWvGe';
    const link = header.locator('a');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', expectedHref);
  }
);