
import { When, Then, DataTable } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

// Verify the status update page
Then(
  'I should see the multi status update page',
  async function () {
    await expect(
      this.page.getByRole('heading', {
        name: 'Have your schools completed this recommendation?',
      })
    ).toBeVisible();

    await expect(
      this.page.locator('input[name="SelectedStatus"]')
    ).toHaveCount(3);

    await expect(
      this.page.locator('input[name="SelectedStatus"][value="NotStarted"]')
    ).toBeChecked();

    await expect(this.page.locator('#Notes')).toBeVisible();

    await expect(
      this.page.getByRole('button', { name: 'Save' })
    ).toBeVisible();
  }
);

// Verify selected schools have carried through
Then(
  'I should see the following schools selected for status update:',
  async function (dataTable: DataTable) {
    const schools = dataTable.hashes().map(row => row.school);

    const form = this.page.locator(
      'form[action$="/update-schools-status"]'
    );

    const schoolList = form.locator('.govuk-list--bullet li');

    await expect(schoolList).toHaveCount(schools.length);

    for (const school of schools) {
      await expect(
        schoolList.getByText(school, { exact: true })
      ).toBeVisible();
    }

    await expect(
      form.locator('input[name="SelectedSchoolsRefs"]')
    ).toHaveCount(schools.length);

    await expect(
      this.page.locator('.govuk-details__summary-text')
    ).toContainText(
      `You are updating the recommendation status for ${schools.length} schools`
    );

    const details = this.page.locator('.govuk-details__text');

    for (const school of schools) {
      await expect(details).toContainText(school);
    }
  }
);

// Select recommendation status
When(
  'I select the recommendation status {string}',
  async function (status: string) {
    const statusValues: Record<string, string> = {
      'Not started': 'NotStarted',
      'In progress': 'InProgress',
      'Complete': 'Complete',
    };

    const value = statusValues[status];

    if (!value) {
      throw new Error(`Unknown recommendation status: ${status}`);
    }

    const radio = this.page.locator(
      `input[name="SelectedStatus"][value="${value}"]`
    );

    const id = await radio.getAttribute('id');

    if (!id) {
      throw new Error(`Radio ID not found for status: ${status}`);
    }

    await this.page.locator(`label[for="${id}"]`).click();

    await expect(radio).toBeChecked();
  }
);

// Enter comment
When(
  'I enter the recommendation status comment {string}',
  async function (comment: string) {
    const notes = this.page.locator('#Notes');

    await notes.fill(comment);

    await expect(notes).toHaveValue(comment);
  }
);

// Verify successful confirmation
Then(
  'I should see the multi status update confirmation for {int} schools',
  async function (schoolCount: number) {
    await expect(
      this.page.locator('.govuk-panel--confirmation')
    ).toBeVisible();

    await expect(
      this.page.locator('.govuk-panel__title')
    ).toHaveText(
      `Recommendation status updated for ${schoolCount} schools`
    );

    await expect(
      this.page.getByRole('heading', {
        name: 'Your schools\' progress',
      })
    ).toBeVisible();
  }
);

// Verify final school statuses
Then(
  'I should see the following school recommendation statuses:',
  async function (dataTable: DataTable) {
    const schools = dataTable.hashes();

    const today = new Date().toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      timeZone: 'Europe/London',
    });

    const table = this.page.locator(
      'table.group-recommendation-progress'
    );

    await expect(table).toBeVisible();

    const rows = table.locator('tbody tr');

    await expect(rows).toHaveCount(schools.length);

    for (const { school, lastUpdated, status } of schools) {
      const row = rows.filter({
        has: this.page.locator('td').getByText(school, {
          exact: true,
        }),
      });

      await expect(row).toHaveCount(1);

      await expect(row.locator('td').first())
        .toHaveText(school);

      const expectedDate =
        lastUpdated.toLowerCase() === 'today'
          ? today
          : lastUpdated;

      await expect(
        row.locator('.group-recommendation-progress__last-updated')
      ).toHaveText(expectedDate);

      await expect(
        row.locator('.group-recommendation-progress__status .govuk-tag')
      ).toHaveText(status);
    }
  }
);
