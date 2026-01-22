import { Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';

Then(
  'I should see the {string} section {string} with description {string} and link href {string}',
  async function (
    state: 'not started' | 'in progress' | 'completed',
    heading: string,
    description: string,
    href: string,
  ) {
    const container = this.page.locator('#main-content');
    const currentDate = getCurrentShortDate();

    const headingEl = container.getByRole('heading', { level: 2, name: heading });
    await expect(headingEl).toBeVisible();

    const sectionLower = heading.toLowerCase();
    const descEl = headingEl.locator('xpath=following-sibling::p[1]');

    if (state === 'not started') {
      // first paragraph is the description
      await expect(descEl).toHaveText(description);

      const linkEl = headingEl.locator('xpath=following-sibling::p[a][1]/a');
      await expect(linkEl).toBeVisible();
      await expect(linkEl).toHaveText(`Go to self-assessment for ${sectionLower}`);
      await expect(linkEl).toHaveAttribute('href', href);
      return;
    }

    if (state === 'in progress') {
      // description no longer lives in p[1] – page shows "started on" text and a separate link

      // Paragraph with the "started on" text
      const inProgressPara = headingEl
        .locator(
          'xpath=following-sibling::p[contains(normalize-space(.), "A self-assessment was started on")]',
        )
        .first();

      await expect(inProgressPara).toBeVisible();

      const text = (await inProgressPara.innerText()).trim();
      const normalisedText = normaliseShortDateTimeText(text);
      const expectedText = `A self-assessment was started on ${currentDate}.`;
      expect(normalisedText).toMatch(expectedText);

      // Next paragraph that actually contains the link
      const link = headingEl.locator('xpath=following-sibling::p[a][1]/a');

      await expect(link).toBeVisible();
      await expect(link).toHaveText(`Continue your self-assessment for ${sectionLower}`);
      await expect(link).toHaveAttribute('href', href);
      return;
    }

    if (state === 'completed') {
      const completionPara = headingEl
        .locator('xpath=following-sibling::p[contains(normalize-space(.), "was completed on")]')
        .first();

      await expect(completionPara).toBeVisible();

      const completionText = (await completionPara.innerText()).trim();
      const normalisedText = normaliseShortDateTimeText(completionText);
      const expectedText = `The self-assessment for ${sectionLower} was completed on ${currentDate}.`;
      expect(normalisedText).toMatch(expectedText);

      const viewLink = headingEl.locator(
        'xpath=following-sibling::p[a[contains(normalize-space(.), "View answers for")]][1]/a',
      );
      await expect(viewLink).toBeVisible();
      await expect(viewLink).toHaveText(`View answers for ${sectionLower}`);
      await expect(viewLink).toHaveAttribute('href', href);
      return;
    }

    throw new Error(`Unknown state "${state}". Use "not started", "in progress", or "completed".`);
  },
);

async function getConfirmationPanel(context: any, sectionName: string) {
  const expectedHeader = `Your self-assessment for ${sectionName.toLowerCase()} is complete`;
  const headerLocator = context.page.locator('h1.govuk-panel__title', {
    hasText: expectedHeader,
  });
  const bodyLocator = context.page.locator('.govuk-panel__body', {
    hasText: 'Recommendations are available',
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

When(
  'I click the recommendation link {string} on the category landing page',
  async function (linkText: string) {
    const link = this.page
      .locator('.recommendation-action-header')
      .getByRole('link', { name: linkText });

    await link.click();
  },
);

When('I click the first recommendation link on the category landing page', async function () {
  const firstLink = this.page.locator('.recommendation-action-header').getByRole('link').first();

  await firstLink.click();
});

Then('I should not see any recommendation links', async function () {
  const recommendationLinks = this.page.locator('ul.govuk-task-list a.govuk-task-list__link');

  await expect(recommendationLinks).toHaveCount(0);
});
