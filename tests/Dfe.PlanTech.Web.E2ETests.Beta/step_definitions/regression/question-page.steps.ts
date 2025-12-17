import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then('I should see the question help text {string}', async function (expectedText: string) {
  const caption = this.page.locator('.govuk-hint');
  await expect(caption).toBeVisible();
  await expect(caption).toHaveText(expectedText);
});

When('I choose the radio option {string}', async function (optionText: string) {
  const radio = this.page.getByRole('radio', { name: optionText });
  await expect(radio).toBeVisible();

  await radio.check();
  await expect(radio).toBeChecked();
});

Then('the radio option {string} should be selected', async function (optionText: string) {
  const radio = this.page.getByRole('radio', { name: optionText });
  await expect(radio).toBeChecked();
});

Then('no radio option should be selected', async function () {
  const radios = this.page.locator('.govuk-radios__input');
  const count = await radios.count();

  for (let i = 0; i < count; i++) {
    await expect(radios.nth(i)).not.toBeChecked();
  }
});


Then('exactly one radio option should be selected', async function () {
  // Count radios that are checked
  const checkedRadios = this.page.locator('.govuk-radios__input:checked');
  await expect(checkedRadios).toHaveCount(1);
});


Then('exactly one radio option should be selected and it should be {string}', async function (expectedText: string) {
  // Ensure exactly one is checked
  const checkedRadios = this.page.locator('.govuk-radios__input:checked');
  await expect(checkedRadios).toHaveCount(1);

  // Verify the selected one has the expected label (accessible name)
  const selectedByName = this.page.getByRole('radio', { name: expectedText, checked: true });
  await expect(selectedByName).toHaveCount(1);
});

Then('I should see a continue button that submits to {string}', async function (expectedAction: string) {
  const form = this.page.locator(`form[action="${expectedAction}"]`);
  await expect(form).toBeVisible();
  await expect(form).toHaveAttribute('method', /post/i);

  // Ensure the button is present and correct
  const button = form.getByRole('button', { name: 'Continue' });
  await expect(button).toBeVisible();
  await expect(button).toHaveAttribute('type', 'submit');
});

When('I click the continue button', async function () {
  const button = this.page.getByRole('button', { name: 'Continue' });
  await expect(button).toBeVisible();

  await button.click();
});


Then('the question answer radio options should appear in this order:', async function (dataTable) {
  const labels = this.page.locator('.govuk-radios__item .govuk-radios__label');
  const actual = (await labels.allInnerTexts()).map((t: string) => t.trim());

  const expected = dataTable.raw().map(([text]: [string]) => text.trim());

  //check count
  await expect(labels).toHaveCount(expected.length);

  //check order
  expect(actual).toEqual(expected);
});

Then('I should see the following radio options in order:', async function (dataTable) {
  const expected = dataTable.raw().map((row: string[]) => row[0].trim());
  const labels = this.page.locator('.govuk-radios__item .govuk-radios__label');

  // check count
  await expect(labels).toHaveCount(expected.length);

  // check the text
  await expect(labels).toHaveText(expected);

  // check that the input radios actually exist
  for (const name of expected) {
    await expect(this.page.getByRole('radio', { name })).toBeVisible();
  }
});