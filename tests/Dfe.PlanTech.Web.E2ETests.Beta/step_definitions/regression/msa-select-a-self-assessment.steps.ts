import { Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then(
  'I should see the following self-assessments in the {string} section:',
  async function (category: string, dataTable) {
    const heading = this.page.getByRole('heading', {
      name: category,
      level: 2,
    });

    const taskList = heading.locator('xpath=following-sibling::ul[1]');

    for (const { 'self-assessment': selfAssessment } of dataTable.hashes()) {
      await expect(
        taskList.getByText(selfAssessment, { exact: true }),
      ).toBeVisible();
    }
  },
);
Then(
  'I should see the {string} self-assessment section',
  async function (category: string) {
    const heading = this.page.getByRole('heading', {
      name: category,
      level: 2,
    });

    await expect(heading).toBeVisible();
  },
);

Then(
  'the {string} self-assessment status should be {string}',
  async function (section: string, expectedStatus: string) {
    // use replace with a global regex instead of replaceAll for older TS lib targets
    const statusId = `${section.toLowerCase().replace(/ /g, '-')}-status`;

    const status = this.page.locator(`#${statusId}`);

    await expect(status).toBeVisible();
    await expect(status).toHaveText(expectedStatus);
  },
);

Then(
  'I should see the following links in the {string} self-assessment section:',
  async function (category: string, dataTable) {
    const heading = this.page.getByRole('heading', {
      name: category,
      level: 2,
    });

    const taskList = heading.locator('xpath=following-sibling::ul[1]');

    for (const { text, url } of dataTable.hashes()) {
      const link = taskList.getByRole('link', {
        name: text,
        exact: true,
      });

      await expect(link).toBeVisible();
      await expect(link).toHaveAttribute('href', url);
    }
  },
);

Then(
  '{string} should not be a link in the {string} self-assessment section',
  async function (section: string, category: string) {
    const heading = this.page.getByRole('heading', {
      name: category,
      level: 2,
    });

    const taskList = heading.locator('xpath=following-sibling::ul[1]');

    await expect(
      taskList.getByText(section, { exact: true }),
    ).toBeVisible();

    await expect(
      taskList.getByRole('link', {
        name: section,
        exact: true,
      }),
    ).toHaveCount(0);
  },
);
