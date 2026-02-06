import { DataTable, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { textToHyphenatedUrl } from '../../helpers/url';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';

Then(
  'I should see the paragraph text {string} inside the recommendation content',
  async function (expectedText: string) {
    const paragraph = this.page.locator('.recommendation-piece-content p', {
      hasText: expectedText,
    });
    await expect(paragraph).toBeVisible();
  },
);

Then(
  'I should see a visible Next pagination link with text {string}',
  async function (nextLinkText: string) {
    const link = this.page.locator('a.govuk-pagination__link:visible');
    const linkText = this.page.locator('span.govuk-pagination__link-label:visible');

    await expect(link).toHaveCount(1);
    await expect(link.first().locator('span.govuk-pagination__link-title:visible')).toContainText(
      'Next',
    );
    await expect(linkText).toHaveText(nextLinkText);
  },
);

Then(
  'I should see a visible Previous pagination link with text {string}',
  async function (expectedLabel: string) {
    const container = this.page.locator('.recommendation-piece-container:visible');
    const titleSpan = container
      .locator('.govuk-pagination__link-title')
      .filter({ hasText: 'Previous' })
      .first();
    const labelSpan = container
      .locator('.govuk-pagination__link-label')
      .filter({ hasText: expectedLabel })
      .first();

    await expect(titleSpan).toContainText('Previous');
    await expect(labelSpan).toContainText(expectedLabel);
  },
);

Then('I click the next recommendation link', async function () {
  const link = this.page.locator('a.govuk-pagination__link[rel="next"]:visible');
  await link.click();
});

Then('I click the previous recommendation link', async function () {
  const link = this.page.locator('a.govuk-pagination__link[rel="prev"]:visible');
  await link.click();
});

Then(
  'I should see the recommendation caption text {string}',
  async function (expectedText: string) {
    const caption = this.page.locator(
      'div.recommendation-piece-content span.govuk-caption-xl:visible',
    );
    await expect(caption).toBeVisible();
    await expect(caption).toHaveText(expectedText);
  },
);

Then(
  'I should see the print recommendations caption text {string}',
  async function (expectedText: string) {
    const caption = this.page.locator('div.recommendation-content span.govuk-caption-xl:visible');
    await expect(caption).toBeVisible();
    await expect(caption).toHaveText(expectedText);
  },
);

Then('I should see the related actions sidebar', async function () {
  const sidebar = this.page.locator('.govuk-grid-column-one-third-from-desktop.govuk-float-right');
  await expect(sidebar).toBeVisible();
  await expect(sidebar.locator('h2')).toHaveText('Related actions');
});

Then(
  'I should see the related actions links for category {string} section {string} recommendation {string}',
  async function (categoryName: string, sectionName: string, recommendationSlug: string) {
    const container = this.page.locator(
      '.govuk-grid-column-one-third-from-desktop.govuk-float-right',
    );
    await expect(container).toBeVisible();

    // check heading is there
    const heading = container.locator('h2.govuk-heading-m');
    await expect(heading).toHaveText('Related actions');

    const slugSectionName = textToHyphenatedUrl(sectionName);
    const slugCategoryName = textToHyphenatedUrl(categoryName);
    const expectedPrintAllSlug = `/${slugCategoryName}/${slugSectionName}/recommendations/${recommendationSlug}/print-all`;
    const expectedPrintRecommendationSlug = `/${slugCategoryName}/${slugSectionName}/recommendations/${recommendationSlug}/print`;

    const sectionNameLowercase = sectionName.toLowerCase();

    // check print all recommendations url
    const printTopicLink = container.getByRole('link', {
      name: `Print your school's ${sectionNameLowercase} recommendations`,
    });
    await expect(printTopicLink).toBeVisible();
    await expect(printTopicLink).toHaveAttribute('href', expectedPrintAllSlug);

    // check the print this recommendation url
    const printThisRecommendation = container.getByRole('link', {
      name: `Print this recommendation`,
    });
    await expect(printThisRecommendation).toBeVisible();
    await expect(printThisRecommendation).toHaveAttribute('href', expectedPrintRecommendationSlug);
  },
);

Then(
  'I should see a link to the recommendation {string}',
  async function (recommendationTitle: string) {
    const link = this.page.getByRole('link', {
      name: recommendationTitle,
    });

    await expect(link).toBeVisible();
  },
);
