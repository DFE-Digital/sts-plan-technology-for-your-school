import { Then, DataTable, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";


Then(
  "the recommendations table should list recommendations in this order:",
  async function (table: DataTable) {
    const page = this.page;

    // expected list (first column)
    const expected = table.raw().flat();

    // get the first column
    const recommendationLinks = page.locator(
      ".recommendation-action-header table.govuk-table tbody.govuk-table__body tr.govuk-table__row td.govuk-table__cell:first-child a.govuk-link"
    );

    const count = await recommendationLinks.count();
    expect(count).toBeGreaterThanOrEqual(expected.length);

    const actual = [];
    for (let i = 0; i < expected.length; i++) {
      const text = await recommendationLinks.nth(i).innerText();
      actual.push(text);
    }

    expect(actual).toEqual(expected);
  }
);

When(
  'I sort recommendations by {string} and apply',
  async function (sortOption: string) {
    const page = this.page;

    const form = page.locator("form", {
      has: page.getByLabel("Sort recommendations by"),
    });

    // select sort dropdown
    await form.locator("#sort").selectOption({ label: sortOption });

    // check it is selected
    await expect(form.locator("#sort")).toHaveValue(sortOption);

    // click apply
    await Promise.all([
      page.waitForLoadState("networkidle").catch(() => {}),
      form.getByRole("button", { name: "Apply" }).click(),
    ]);
  }
);