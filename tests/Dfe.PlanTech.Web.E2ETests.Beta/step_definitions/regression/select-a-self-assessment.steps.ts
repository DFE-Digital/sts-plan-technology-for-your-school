import { DataTable, Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then('I should see the select a self assessment page', async function () {
  await expect(this.page.getByRole('heading', { name: 'Select a self-assessment' })).toBeVisible();
});

Then('I should see the following self assessments:', async function (dataTable: DataTable) {
  const assessmentNames = dataTable.raw().flat();

  for (const assessmentName of assessmentNames) {
      const link = this.page.getByRole("link", { name: assessmentName });
    await expect(link, `School link not visible: ${assessmentName}`).toBeVisible();
  }
});

Then('I select the self assessment {string}', async function (assessmentName: string) {
      const link = this.page.getByRole("link", { name: assessmentName });
      link.click();
});