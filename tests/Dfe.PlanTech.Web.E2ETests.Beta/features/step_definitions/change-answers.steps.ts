import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

function toKebabCase(input: string): string {
  return input
    .toLowerCase()
    .replace(/[^a-z0-9\s]/g, '') // remove non-alphanumeric except spaces
    .trim()
    .replace(/\s+/g, '-');
}

Then('I click the view or update self-assessment link for {string}', async function (sectionName: string) {
  const lowerCaseSection = sectionName.toLowerCase();
  const kebabCaseSection = toKebabCase(sectionName);
  const expectedPath = `/${kebabCaseSection}/change-answers`;
  const expectedLinkText = `View or update your self-assessment for ${lowerCaseSection}`;

  const link = this.page.getByRole('link', { name: expectedLinkText });
  await expect(link).toHaveCount(1);

  await Promise.all([
    this.page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
    link.click(),
  ]);

  const currentUrl = this.page.url();
  expect(currentUrl.endsWith(expectedPath)).toBe(true);
});

Then('I should see the back to recommendations button for {string}', async function (sectionName: string) {
  const kebabCaseSection = toKebabCase(sectionName);
  const expectedHref = `/recommendations/from-section/${kebabCaseSection}`;

  const backButton = this.page.getByRole('button', { name: 'Back to recommendations' });

  // Check button exists and has correct text
  await expect(backButton).toHaveCount(1);
  await expect(backButton).toHaveText('Back to recommendations');

  // Check href
  const actualHref = await backButton.getAttribute('href');
  expect(actualHref).toBe(expectedHref);
});

Then('I click the change link on change answers for {string} and I should see the question heading', async function (questionText: string) {
  const changeLink = this.page.locator(`a[title="${questionText}"]`);

  await expect(changeLink).toHaveCount(1);
  
  await Promise.all([
    this.page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
    changeLink.click(),
  ]);

  const questionHeading = this.page.locator('legend h1.govuk-fieldset__heading');
  await expect(questionHeading).toHaveText(questionText);
});


Then('I should see the following change answer rows:', async function (dataTable) {
  const rows = this.page.locator('dl.govuk-summary-list > div');
  const expectedRows = dataTable.hashes(); 

  const actualCount = await rows.count();
  expect(actualCount).toBe(expectedRows.length);

  for (let i = 0; i < expectedRows.length; i++) {
    const row = rows.nth(i);

    const expectedQuestion = expectedRows[i]['Question'];
    const expectedAnswer = expectedRows[i]['Answer'];
    const expectedHref = expectedRows[i]['Href'];

    const questionLocator = row.locator('p.govuk-\\!-font-weight-bold');
    const answerLocator = row.locator('p.govuk-body');
    const changeLink = row.locator('a.govuk-link');

    const actualQuestion = await questionLocator.textContent();
    const actualAnswer = await answerLocator.textContent();
    const actualHref = await changeLink.getAttribute('href');

    expect(actualQuestion?.trim()).toBe(expectedQuestion);
    expect(actualAnswer?.trim()).toBe(expectedAnswer);
    expect(actualHref).toBe(`${expectedHref}?returnTo=ChangeAnswers`);
  }
});


Then('I change the answer to {string} and continue all other questions', async function (answerLabel: string) {
  const page = this.page;

  const radioLabel = page.getByLabel(answerLabel, { exact: true });
  await expect(radioLabel).toHaveCount(1);
  await radioLabel.check();

  await page.getByRole('button', { name: 'Continue' }).click();

  while (true) {
    const summaryList = page.locator('dl.govuk-summary-list');
    if (await summaryList.count() > 0) {
      break; 
    }

    const radioInputs = page.locator('input[type="radio"]');
    if (await radioInputs.count() === 0) {
      throw new Error('No radio buttons found — unexpected page layout');
    }

    await radioInputs.first().check();

    const continueButton = page.getByRole('button', { name: 'Continue' });
    await continueButton.click();
  }
});