import { DataTable, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { textToHyphenatedUrl } from '../../helpers/url';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';


Then('I click the print all recommendations link in the related actions for {string}', async function (topic:string) {

  const container = this.page.locator('.govuk-grid-column-one-third-from-desktop.govuk-float-right');
  const printLink = container.locator('a', { hasText: `Print your school's ${topic.toLowerCase()} recommendations` });
  await expect(printLink).toBeVisible();
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

Then(
  'I should see the print page recommendation text {string}',
  async function (recommendationText: string) {

    const recommendation = this.page.locator(
      '.recommendation-action-header td.govuk-table__cell div',
      { hasText: recommendationText }
    );

    await expect(recommendation).toBeVisible();
  }
);


Then(
  'I should see a recommendation with heading {string} and content containing {string} on the print recommendation page',
  async function (heading: string, content: string) {
    // find the header matching the h1
    const contentBlock = this.page.locator(
      `div.recommendation-piece-content:has(h1.govuk-heading-l:has-text("${heading}"))`
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


Then('recommendation status is shown as {string}', async function (expectedStatus: string) {
  const statusTag = this.page.locator(
    '.recommendation-piece-content table.govuk-table tbody tr .govuk-tag'
  );

  await expect(statusTag).toHaveText(expectedStatus);
});

Then(
  'recommendation status in the header matches the selected status in the form',
  async function () {
    // Header status (the tag in the top table)
    const headerTag = this.page.locator(
      '.recommendation-piece-content table.govuk-table tbody tr .govuk-tag'
    );
    const headerStatus = (await headerTag.innerText()).replace(/\s+/g, ' ').trim();

    // Selected radio in the form
    const checkedRadio = this.page.locator('input[name="SelectedStatus"]:checked');
    await expect(checkedRadio).toHaveCount(1);

    const radioId = await checkedRadio.getAttribute('id');
    const label = this.page.locator(`label[for="${radioId}"]`);
    const formStatus = (await label.innerText()).replace(/\s+/g, ' ').trim();

    expect(formStatus).toBe(headerStatus);
  }
);

Then('status last updated date is {string}', async function (expectedDate: string) {
  const lastUpdatedCell = this.page
    .locator('.recommendation-piece-content table.govuk-table tbody tr .govuk-table__cell')
    .nth(1);

  await expect(lastUpdatedCell).toHaveText(expectedDate);
});

