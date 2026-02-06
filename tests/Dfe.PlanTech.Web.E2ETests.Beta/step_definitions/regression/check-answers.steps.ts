import { Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see a print button', async function () {
  await expect(this.page.locator('#print-page-button')).toBeVisible();
});

Then('I should see the following ordered check answer rows:', async function (dataTable) {
  const rows = this.page.locator('div.govuk-summary-list__row');
  const expectedRows = dataTable.hashes();

  const actualCount = await rows.count();
  expect(actualCount).toBe(expectedRows.length);

  for (let i = 0; i < expectedRows.length; i++) {
    const row = rows.nth(i);

    const expectedQuestion = expectedRows[i]['Question'];
    const expectedAnswer = expectedRows[i]['Answer'];
    const expectedHref = expectedRows[i]['Href'];

    const questionText = await row.locator('.govuk-summary-list__key').textContent();
    const answerText = await row.locator('.govuk-summary-list__value').textContent();
    const changeLink = row.locator('.govuk-summary-list__actions a');

    expect(questionText?.trim()).toBe(expectedQuestion);
    expect(answerText?.trim()).toBe(expectedAnswer);

    // Check link exists
    expect(await changeLink.count()).toBeGreaterThan(0);

    // Check the change link matches
    const actualHref = await changeLink.getAttribute('href');
    expect(actualHref).toBe(expectedHref);
  }
});

Then('I should see a view recommendations button', async function () {
  const form = this.page.locator('form[action="/ConfirmCheckAnswers"]');

  await expect(form).toHaveCount(1);
  await expect(form).toHaveAttribute('method', /post/i);
  await expect(form).toHaveAttribute('class', /noprint/);

  // check the hidden input fields
  const hiddenFields = [
    'submissionId',
    'sectionName',
    'sectionSlug',
    'categorySlug',
    '__RequestVerificationToken',
  ];
  for (const name of hiddenFields) {
    await expect(form.locator(`input[type="hidden"][name="${name}"]`)).toHaveCount(1);
  }

  // check the heading
  await expect(form.locator('h2.govuk-heading-l')).toHaveCount(1);

  // check the submit & view recommendations button
  const submitButton = form.locator('button.govuk-button');
  await expect(submitButton).toHaveCount(1);
  await expect(submitButton).toHaveAttribute('type', 'submit');
  await expect(submitButton).toContainText('Submit and view recommendations');

  await expect(
    this.page.getByRole('button', { name: 'Submit and view recommendations' }),
  ).toBeVisible();
});

Then(
  'I click the change link on check answers for {string} and should see the question heading',
  async function (questionText: string) {
    const changeLink = this.page.locator(`.govuk-summary-list__actions a[title="${questionText}"]`);
    await expect(changeLink).toHaveCount(1); // ensure it exists
    await changeLink.click();

    await this.page.waitForLoadState('domcontentloaded');

    const questionHeading = this.page.locator('legend h1.govuk-fieldset__heading');
    await expect(questionHeading).toHaveText(questionText);
  },
);
