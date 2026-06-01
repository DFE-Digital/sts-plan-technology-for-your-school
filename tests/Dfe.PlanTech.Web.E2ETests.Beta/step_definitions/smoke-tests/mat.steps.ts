import { Given, When, Then, DataTable } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then('I should see the selected school name {string}', async function (schoolName: string) {
  const nameElement = this.page.locator('.school-ident .school-ident-name');
  await expect(nameElement).toBeVisible();
  await expect(nameElement).toHaveText(schoolName);
});

Then('I should see the select a school heading', async function () {
  await expect(this.page.getByRole('heading', { name: 'Select a school', level: 2 })).toBeVisible();
});

Then('I should see the following schools:', async function (dataTable: DataTable) {
  const schoolRows = dataTable.raw().flat();

  for (const schoolRow of schoolRows) {
    const possibleSchoolNames = schoolRow
      .split(' OR ')
      .map(name => name.trim());

    let schoolFound = false;

    for (const schoolName of possibleSchoolNames) {
      const button = this.page.getByRole('button', { name: schoolName });

      if (await button.isVisible().catch(() => false)) {
        schoolFound = true;
        break;
      }
    }

    expect(
      schoolFound,
      `None of the expected school names were visible: ${possibleSchoolNames.join(', ')}`
    ).toBe(true);
  }
});

Then('I should see the following school progress:', async function (dataTable: DataTable) {
  const rows = dataTable.hashes();

  for (const row of rows) {
    const possibleSchoolNames = row['School name']
      .split(' OR ')
      .map(name => name.trim());

    let matchingForm = null;

    for (const schoolName of possibleSchoolNames) {
      const form = this.page.locator('form').filter({
        has: this.page.getByRole('button', { name: schoolName }),
      });

      if (await form.count() > 0) {
        matchingForm = form;
        break;
      }
    }

    expect(
      matchingForm,
      `No matching school form found for: ${possibleSchoolNames.join(', ')}`
    ).toBeTruthy();

    const progress = matchingForm!.locator('p');

    await expect(progress).toContainText(
      'recommendations completed or in progress'
    );
  }
});

Given('I am on the select a school page', async function () {
  // If login already lands you here, just assert
  await expect(this.page.getByRole('heading', { name: 'Select a school' })).toBeVisible();
});

When('I select the school {string}', async function (schoolName: string) {
  await this.page.getByRole('button', { name: schoolName }).click();
});

Then(
  'the selected school cookie {string} should have URN {string} and name {string}',
  async function (cookieName: string, urn: string, schoolName: string) {
    const context = this.page.context();
    const cookies = await context.cookies();
    const cookie = cookies.find((c: { name: string }) => c.name === cookieName);

    expect(cookie, `Cookie "${cookieName}" not found`).toBeDefined();

    // e.g. cookie {"Urn":"900006","Name":"DSI TEST Establishment (001) Community School (01)"}
    let parsed: any;

    try {
      // decode the cookie
      const decodedValue = decodeURIComponent(cookie!.value);
      parsed = JSON.parse(decodedValue);
    } catch {
      // fallback in case json is raw in cookie
      parsed = JSON.parse(cookie!.value);
    }

    expect(parsed.Urn).toBe(urn);
    expect(parsed.Name).toBe(schoolName);
  },
);
