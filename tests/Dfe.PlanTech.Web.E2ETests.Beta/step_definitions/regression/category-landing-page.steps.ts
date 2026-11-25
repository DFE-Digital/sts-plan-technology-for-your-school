import { Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';


async function getConfirmationPanel(
  context: any,
  sectionName: string,
  schoolCount: number = 1
) {
  const panel = context.page.locator('.govuk-panel--confirmation');

  const headerLocator = panel.locator('h1.govuk-panel__title', {
    hasText: 'Self-assessment submitted',
  });

  const bodyLocator = panel.locator('.govuk-panel__body', {
    hasText: `${sectionName} has been submitted for ${schoolCount} ${schoolCount === 1 ? 'school' : 'schools'}`,
  });

  return { headerLocator, bodyLocator };
}


Then('I should see the confirmation panel for {string}', async function (sectionName: string) {
  const { headerLocator, bodyLocator } = await getConfirmationPanel(this, sectionName);

  await expect(headerLocator).toBeVisible();
  await expect(bodyLocator).toBeVisible();
});

Then('I should not see the confirmation panel for {string}', async function (sectionName: string) {
  const { headerLocator, bodyLocator } = await getConfirmationPanel(this, sectionName);

  await expect(headerLocator).not.toBeVisible();
  await expect(bodyLocator).not.toBeVisible();
});

Then(
  'I should see a link to the recommendation {string}',
  async function (recommendationTitle: string) {
    const link = this.page.getByRole('link', {
      name: recommendationTitle,
    });

    await expect(link).toBeVisible();
  },
);

When('I click the first recommendation link on the category landing page', async function () {
  const firstLink = this.page.locator('.recommendation-action-header').getByRole('link').first();

  await firstLink.click();
});

When(
  'I click the recommendation link {string} on the category landing page',
  async function (linkText: string) {
    const link = this.page
      .locator('.recommendation-action-header')
      .getByRole('link', { name: linkText });

    await Promise.all([
      this.page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
      link.click(),
    ]);
  },
);



Then('I should not see any recommendation links', async function () {
  const recommendationLinks = this.page.locator('ul.govuk-task-list a.govuk-task-list__link');

  await expect(recommendationLinks).toHaveCount(0);
});


Then(
  'I should see the completed self-assessment message for section {string} and establishment {string}',
  async function (sectionName: string, establishmentName: string) {
    // format todays date
    var currentDate = getCurrentShortDate();

    const sectionNameLower = sectionName.toLowerCase();
    const expectedTextRaw = `The self-assessment for ${sectionNameLower} was submitted on ${currentDate} by ${establishmentName}.`;

    // check the self assessment completed text
    const completionParagraph = this.page.locator('p', {
      hasText: `The self-assessment for ${sectionNameLower} was submitted on`,
    });

    await expect(completionParagraph).toBeVisible();

    // now fetch and normalise both
    const actualText = normaliseShortDateTimeText(await completionParagraph.innerText());
    const expectedText = normaliseShortDateTimeText(expectedTextRaw);

    // assert equality
    expect(actualText).toBe(expectedText);

    // check the view link is completed
    const viewLink = this.page.getByRole('link', { name: `View answers for ${sectionNameLower}` });
    await expect(viewLink).toBeVisible();
  },
);