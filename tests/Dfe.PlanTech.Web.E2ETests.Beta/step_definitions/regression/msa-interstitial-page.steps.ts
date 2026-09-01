import { Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then(
  'I should see the MAT self-assessment school selection content',
  async function () {
    await expect(
      this.page.getByRole('heading', {
        name: 'Submit a self-assessment for your schools',
        level: 2,
      }),
    ).toBeVisible();

    await expect(
      this.page.getByText(
        'You can submit the self-assessment for one or more schools.',
        { exact: true },
      ),
    ).toBeVisible();

    await expect(
      this.page.getByText(
        'If a self-assessment has been started but not submitted for a school, you can check their answers or submit a new self-assessment for them.',
        { exact: true },
      ),
    ).toBeVisible();
  },
);

Then(
  'I should see the following school self-assessment statuses:',
  async function (dataTable) {
    const table = this.page.getByRole('table');

    await expect(table).toBeVisible();

    await expect(
      table.getByRole('columnheader', {
        name: 'School',
      }),
    ).toBeVisible();

    await expect(
      table.getByRole('columnheader', {
        name: 'Self-assessment submission',
      }),
    ).toBeVisible();

    for (const { school, status } of dataTable.hashes()) {
      const row = table.getByRole('row').filter({
        hasText: school,
      });

      await expect(row).toBeVisible();
      await expect(row).toContainText(school);

      if (status === 'Answers in progress') {
        const statusLink = row.getByRole('link', {
          name: 'Answers in progress',
          exact: true,
        });

        await expect(statusLink).toBeVisible();
      } else {
        await expect(
          row.getByText(status, {
            exact: true,
          }),
        ).toBeVisible();
      }
    }
  },
);

Then(
  'I should see the continue button for selecting schools',
  async function () {
    const continueButton = this.page.getByRole('link', {
      name: 'Continue',
      exact: true,
    });

    await expect(continueButton).toBeVisible();

    await expect(continueButton).toHaveAttribute(
      'href',
      /\/groups\/.*\/self-assessment\/select-schools$/,
    );
  },
);