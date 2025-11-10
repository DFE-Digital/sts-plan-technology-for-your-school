import { DataTable, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { textToHyphenatedUrl } from '../../helpers/url';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';

Then('I should see the paragraph text {string} inside the recommendation content', async function (expectedText: string) {
  const paragraph = this.page.locator('.recommendation-piece-content p', { hasText: expectedText });
  await expect(paragraph).toBeVisible();
});

Then('I should see a visible Next pagination link with text {string}', async function (nextLinkText: string) {
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

Then('I click the previous recommendation link', async function () {
  const link = this.page.locator('a.govuk-pagination__link[rel="prev"]:visible');
  await link.click();
});

Then('I should see the recommendation caption text {string}', async function (expectedText: string) {
  const caption = this.page.locator('div.recommendation-piece-content span.govuk-caption-xl:visible');
  await expect(caption).toBeVisible();
  await expect(caption).toHaveText(expectedText);
});

Then('I should see the print recommendations caption text {string}', async function (expectedText: string) {
  const caption = this.page.locator('div.recommendation-content span.govuk-caption-xl:visible');
  await expect(caption).toBeVisible();
  await expect(caption).toHaveText(expectedText);
});

Then('I should see the related actions sidebar', async function () {
  const sidebar = this.page.locator('div.govuk-grid-column-one-third:visible');
  await expect(sidebar).toBeVisible();
  await expect(sidebar.locator('h2')).toHaveText('Related actions');
});

Then('I should see the related actions links for category {string} section {string}', async function (categoryName: string, sectionName: string) {

  const container = this.page.locator('.govuk-grid-column-one-third.govuk-float-right');
  await expect(container).toBeVisible();

  // check heading is there
  const heading = container.locator('h2.govuk-heading-m');
  await expect(heading).toHaveText('Related actions');

  const slugSectionName = textToHyphenatedUrl(sectionName);
  const slugCategoryName = textToHyphenatedUrl(categoryName);
  const expectedSelfAssessmentHref = `/${slugCategoryName}/${slugSectionName}/change-answers`;
  const expectedPrintHref = 'print';

  const sectionNameLowercase = sectionName.toLowerCase();

  // check the view or update your self assessment url
  const viewUpdateLink = container.getByRole('link', {
    name: `View or update your self-assessment for ${sectionNameLowercase}`,
  });
  await expect(viewUpdateLink).toBeVisible();
  await expect(viewUpdateLink).toHaveAttribute('href', expectedSelfAssessmentHref);

  // check the print all recommendations url
  const printLink = container.getByRole('link', {
    name: `Print all recommendations for ${sectionNameLowercase}`,
  });
  await expect(printLink).toBeVisible();
  await expect(printLink).toHaveAttribute('href', expectedPrintHref);
});

Then('I click the print all recommendations link in the related actions', async function () {

  const container = this.page.locator('.govuk-grid-column-one-third.govuk-float-right');
  const printLink = container.locator('a', { hasText: 'Print all recommendations for' });
  printLink.click();
});

Then('I should see the print this page button', async function () {
  const printButton = this.page.locator('.noprint #print-page-button');
  await expect(printButton).toBeVisible();
})

Then('I should see the completed self assessment text on the print recommendations page', async function (sectionName: string) {
   var currentDate = getCurrentShortDate();

  const sectionLower = sectionName.toLowerCase();

  const container = this.page.locator('#main-content');
  const completionPara = container.locator(
    'xpath=following-sibling::p[contains(normalize-space(.), "was completed on")]'
  ).first();

  expect(completionPara).toBeVisible();

  const text = (await completionPara.innerText()).trim();
  const normalisedText =  normaliseShortDateTimeText(text);
  
  // e.g. The self-assessment for {sectionLower} was completed on 27 Aug 2025.
  const expectedText = `The self-assessment for ${sectionLower} was completed on ${currentDate}.`;
  expect(normalisedText).toMatch(expectedText);

  const viewText = container.locator(
    'xpath=following-sibling::p[contains(normalize-space(.), "View or update your self-assessment")][1]/a'
  );
  await expect(viewText).toBeVisible();
  await expect(viewText).toHaveText(`View or update your self-assessment for ${sectionLower}`);
})

Then('I should see the print page recommendation text {string}',
  async function (recommendationText: string) {

    const recommendationLink = this.page.locator('ul.govuk-task-list p', {
      hasText: recommendationText,
    });

    await expect(recommendationLink).toBeVisible();
  }
);

Then(
  'I should see a recommendation with heading {string} and content containing {string} on the print recommendation page',
  async function (heading: string, content: string) {
    // the header wrapper that HAS the exact h1 text, then its immediate sibling content
    const contentBlock = this.page.locator(
      'div.recommendation-action-header:has(h1.govuk-heading-m:has-text("' + heading + '")) + div.recommendation-action-content'
    );

    await expect(contentBlock).toBeVisible();
    await expect(contentBlock).toContainText(content);
  }
);

Then('the print recommendation page self assessment summary should show:', async function (table: DataTable) {
  for (const row of table.hashes()) {
    const question = row.question.trim();
    const answer = row.answer.trim();

    // Locate the <dt> by question text
    const dt = this.page.locator('dt.govuk-summary-list__key', { hasText: question }).first();
    await expect(dt, `Question not found: ${question}`).toBeVisible();

    // The matching <dd> is the immediate sibling
    const dd = dt.locator('xpath=following-sibling::dd[1]');
    await expect(dd, `Answer missing for question: ${question}`).toBeVisible();

    // Use contains to be resilient to whitespace/newlines
    await expect(dd).toContainText(answer);
  }
});

When('I click the print this page button', async function () {
  // stub out window print before calling
  await this.page.evaluate(() => {
    (window as any).__printCalled = false;
    window.print = () => {
      (window as any).__printCalled = true;
    };
  });

  // click the print button
  await this.page.locator('#print-page-button').click();
});

Then('the print dialog should be triggered', async function () {
  const printCalled = await this.page.evaluate(() => (window as any).__printCalled);
  expect(printCalled).toBeTruthy();
});