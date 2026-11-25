
import { DataTable, Then, When } from '@cucumber/cucumber';
import { expect, Page } from '@playwright/test';

function getSchoolRow(page: Page, school: string) {
  return page
    .locator('#select-statuses-table tbody tr')
    .filter({
      has: page.locator('td.schools').getByText(school, { exact: true }),
    });
}

Then(
  'I should see the following schools available for multi status update:',
  async function (dataTable: DataTable) {
    for (const { school } of dataTable.hashes()) {
      const row = getSchoolRow(this.page, school);

      await expect(row).toBeVisible();
      await expect(row.locator('input[name="SelectedSchoolsRefs"]')).toBeEnabled();
    }
  }
);

When(
  'I select the following schools for multi status update:',
   async function (dataTable: DataTable) {
    for (const { school } of dataTable.hashes()) {
      const row = this.page
        .locator('#select-statuses-table tbody tr')
        .filter({
          has: this.page.locator('td.schools').getByText(school, {
            exact: true,
          }),
        });

      const checkbox = row.locator('input[name="SelectedSchoolsRefs"]');

      if (!(await checkbox.isChecked())) {
        await row.locator('label.govuk-checkboxes__label').click();
      }

      await expect(checkbox).toBeChecked();
    }
  }
);

Then(
  'the following schools should be selected for multi status update:',
  async function (dataTable: DataTable) {
    const selectedSchools = dataTable.hashes().map(({ school }) => school);
    const rows = this.page.locator('#select-statuses-table tbody tr');

    for (const row of await rows.all()) {
      const school = (await row.locator('td.schools').innerText()).trim();
      const checkbox = row.locator('input[name="SelectedSchoolsRefs"]');

      if (selectedSchools.includes(school)) {
        await expect(checkbox).toBeChecked();
      } else {
        await expect(checkbox).not.toBeChecked();
      }
    }
  }
);


When(
  'I deselect all schools for multi status update',
  async function () {
    const selectAllCheckbox = this.page.locator(
      '#select-statuses-checkboxes-all'
    );

    await expect(selectAllCheckbox).toBeAttached();

    if (await selectAllCheckbox.isChecked()) {
      await selectAllCheckbox.uncheck({ force: true });
    }

    await expect(selectAllCheckbox).not.toBeChecked();
  }
);

Then(
  'all schools should be deselected for multi status update',
  async function () {
    const checkboxes = this.page.locator(
      '#select-statuses-table tbody input[name="SelectedSchoolsRefs"]'
    );

    const count = await checkboxes.count();

    expect(count).toBeGreaterThan(0);

    for (let i = 0; i < count; i++) {
      await expect(checkboxes.nth(i)).not.toBeChecked();
    }
  }
);



When(
  'I select all schools for multi status update',
  async function () {
    const selectAllCheckbox = this.page.locator(
      '#select-statuses-checkboxes-all'
    );

    await expect(selectAllCheckbox).toBeAttached();

    if (!(await selectAllCheckbox.isChecked())) {
      await selectAllCheckbox.check({ force: true });
    }

    await expect(selectAllCheckbox).toBeChecked();
  }
);

Then(
  'all schools should be selected for multi status update',
  async function () {
    const checkboxes = this.page.locator(
      '#select-statuses-table tbody input[name="SelectedSchoolsRefs"]'
    );

    const count = await checkboxes.count();

    expect(count).toBeGreaterThan(0);

    for (let i = 0; i < count; i++) {
      await expect(checkboxes.nth(i)).toBeChecked();
    }
  }
);


Then(
  'I should see the following schools and their recommendation statuses:',
  async function (dataTable: DataTable) {
    const schools = dataTable.hashes();

    const today = new Date().toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      timeZone: 'Europe/London',
    });

    const table = this.page.locator('#select-statuses-table');

    await expect(table.locator('tbody tr')).toHaveCount(schools.length);

    for (const { school, lastUpdated, status } of schools) {
      const row = table.locator('tbody tr').filter({
        has: this.page.locator('td.schools').getByText(school, {
          exact: true,
        }),
      });

      await expect(row).toBeVisible();

      await expect(row.locator('td.schools')).toHaveText(school);

      await expect(row.locator('td.update')).toHaveText(
        lastUpdated.toLowerCase() === 'today' ? today : lastUpdated
      );

      await expect(row.locator('td.status .govuk-tag')).toHaveText(status);
    }
  }
);


Then(
  'the select all checkbox should be checked',
  async function () {
    await expect(
      this.page.locator('#select-statuses-checkboxes-all')
    ).toBeChecked();
  }
);

Then(
  'the select all checkbox should not be checked',
  async function () {
    await expect(
      this.page.locator('#select-statuses-checkboxes-all')
    ).not.toBeChecked();
  }
);

When(
  'I deselect the school {string}',
  async function (school: string) {
    const row = this.page
      .locator('#select-statuses-table tbody tr')
      .filter({
        has: this.page.locator('td.schools').getByText(school, {
          exact: true,
        }),
      });

    const checkbox = row.locator('input[name="SelectedSchoolsRefs"]');

    if (await checkbox.isChecked()) {
      await row.locator('label.govuk-checkboxes__label').click();
    }

    await expect(checkbox).not.toBeChecked();
  }
);


Then(
  'I should see the validation error {string}',
  async function (errorMessage: string) {
    const errorSummary = this.page.locator('.govuk-error-summary');

    await expect(errorSummary).toBeVisible();
    await expect(
      errorSummary.locator('.govuk-error-summary__title')
    ).toHaveText('There is a problem');

    await expect(
      errorSummary.locator('.govuk-error-summary__list a')
    ).toHaveText(errorMessage);

    await expect(
      this.page.locator('#select-schools-update-status-error')
    ).toContainText(errorMessage);

    await expect(
      this.page.locator('.govuk-form-group--error')
    ).toBeVisible();
  }
);
