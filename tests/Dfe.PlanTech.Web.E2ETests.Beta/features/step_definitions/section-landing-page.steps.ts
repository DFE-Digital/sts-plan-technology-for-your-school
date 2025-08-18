import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { Page } from '@playwright/test';

async function getConfirmationPanel(context: any, sectionName: string) {
  const expectedHeader = `Your self-assessment for ${sectionName.toLowerCase()} is complete`;
  const headerLocator = context.page.locator('h1.govuk-panel__title', {
    hasText: expectedHeader,
  });
  const bodyLocator = context.page.locator('.govuk-panel__body', {
    hasText: 'Recommendations are available',
  });

  return {headerLocator, bodyLocator};
}


Then('I should see the confirmation panel for {string}',
  async function (sectionName: string) {
    const {headerLocator, bodyLocator} = await getConfirmationPanel(this, sectionName);
    
    await expect(headerLocator).toBeVisible();
    await expect(bodyLocator).toBeVisible();
  }
);

Then('I should not see the confirmation panel for {string}', async function (sectionName: string) {
    const {headerLocator, bodyLocator} = await getConfirmationPanel(this, sectionName);
    
    await expect(headerLocator).not.toBeVisible();
    await expect(bodyLocator).not.toBeVisible();
}
);

Then('I should see a link to the recommendation {string}',
  async function (recommendationText: string) {

    const recommendationLink = this.page.locator('ul.govuk-task-list a.govuk-task-list__link', {
      hasText: recommendationText,
    });

    await expect(recommendationLink).toBeVisible();
  }
);

When('I click the first recommendation link on the section landing page', async function () {
  const firstLink = this.page.locator('a.govuk-task-list__link').first();
  await firstLink.click();
});

When('I click the recommendation link {string} on the section landing page', async function (linkText: string) {
  const link = this.page.locator('a.govuk-task-list__link', {
    hasText: linkText,
  });

  await link.first().click();
}
);

Then('I should see the completed self-assessment message for {string}', async function (sectionName: string) {
  //format the date
  const today = new Date();
  const day = today.getDate();
  const month = today.toLocaleString('en-GB', { month: 'short' });
  const year = today.getFullYear();
  const expectedDate = `${day} ${month} ${year}`;

  // check the self assessment completed text
  const expectedText = `The self-assessment for ${sectionName} was completed on ${expectedDate}.`;
  const completionParagraph = this.page.locator('p', { hasText: expectedText });
  await expect(completionParagraph).toBeVisible();

  // check the view link is completed
  const viewLink = this.page.getByRole('link', { name: `View or update your self-assessment for ${sectionName}` });
  await expect(viewLink).toBeVisible();
});

Then('I should not see any recommendation links', async function () {
  const recommendationLinks = this.page.locator('ul.govuk-task-list a.govuk-task-list__link');

  await expect(recommendationLinks).toHaveCount(0);
});