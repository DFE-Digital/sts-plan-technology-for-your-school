import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then('I should see the question help text {string}', async function (expectedText: string) {
  const caption = this.page.locator('.govuk-hint');
  await expect(caption).toBeVisible();
  await expect(caption).toHaveText(expectedText);
});

Then('I should see the following radio options:', async function (dataTable) {
    // get all the radio options
  const radios = this.page.locator('.govuk-radios__item');
  await expect(radios).toHaveCount(dataTable.raw().length);

  // loop through the options we expect
  for (const [optionText] of dataTable.raw()) {
    const option = this.page.getByRole('radio', { name: optionText });
    await expect(option).toBeVisible();
  }
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