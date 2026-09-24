import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { getCurrentShortDate, normaliseShortDateTimeText } from "../../helpers/datetime";

Then(
  'I should see the select schools self-assessment page',
  async function () {
    await expect(
      this.page.getByRole('heading', {
        name: 'Which schools do you want to submit the self-assessment for?',
        level: 1,
      }),
    ).toBeVisible();

    await expect(
      this.page.getByText(
        "Select one, more or all schools who haven't already submitted a self-assessment",
        { exact: true },
      ),
    ).toBeVisible();

    await expect(
      this.page.getByText(
        'Starting a new self-assessment will replace any previous answers.',
        { exact: true },
      ),
    ).toBeVisible();

    await expect(
      this.page.getByRole('button', {
        name: 'Continue',
      }),
    ).toBeVisible();
  },
);

Then(
  'I should see the following schools available for self-assessment:',
  async function (dataTable) {
    for (const { school } of dataTable.hashes()) {
      const checkbox = this.page.getByRole('checkbox', {
        name: school,
        exact: true,
      });

      await expect(checkbox).toBeVisible();
      await expect(checkbox).toBeEnabled();
    }
  },
);

When(
  'I select the following schools:',
  async function (dataTable) {
    for (const { school } of dataTable.hashes()) {
      await this.page
        .getByRole('checkbox', {
          name: school,
          exact: true,
        })
        .check();
    }
  },
);

Then(
  'the following schools should be selected:',
  async function (dataTable) {
    for (const { school } of dataTable.hashes()) {
      await expect(
        this.page.getByRole('checkbox', {
          name: school,
          exact: true,
        }),
      ).toBeChecked();
    }
  },
);

When(
  'I select all schools without a submission',
  async function () {
    const checkbox = this.page.getByRole('checkbox', {
      name: 'Submit self-assessment for all schools without a submission',
      exact: true,
    });

    await checkbox.check();
  },
);

Then(
  'the all schools checkbox should be selected',
  async function () {
    await expect(
      this.page.getByRole('checkbox', {
        name: 'Submit self-assessment for all schools without a submission',
        exact: true,
      }),
    ).toBeChecked();
  },
);

Then(
  'all individual school checkboxes should be unselected',
  async function () {
    const individualSchoolCheckboxes = this.page.locator(
      'input[name="SelectedSchoolsRefs"]:not([value="all"])',
    );

    const count = await individualSchoolCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await expect(individualSchoolCheckboxes.nth(i)).not.toBeChecked();
    }
  },
);

When(
  'I select the school {string}',
  async function (school: string) {
    await this.page
      .getByRole('checkbox', {
        name: school,
        exact: true,
      })
      .check();
  },
);

Then(
  'the school {string} should be selected',
  async function (school: string) {
    await expect(
      this.page.getByRole('checkbox', {
        name: school,
        exact: true,
      }),
    ).toBeChecked();
  },
);

Then(
  'the all schools checkbox should be unselected',
  async function () {
    await expect(
      this.page.getByRole('checkbox', {
        name: 'Submit self-assessment for all schools without a submission',
        exact: true,
      }),
    ).not.toBeChecked();
  },
);

When(
  'I click the {string} button',
  async function (buttonName: string) {
    const button = this.page.getByRole('button', {
      name: buttonName,
      exact: true,
    });

    await expect(button).toBeVisible();
    await button.click();
  },
);

Then(
  'the school {string} should show self-assessment started text',
  async function (schoolName: string) {
    const checkbox = this.page.getByRole('checkbox', {
      name: schoolName,
      exact: true,
    });

    var currentDate = getCurrentShortDate(false);
    
    await expect(checkbox).toBeVisible();

    const checkboxItem = checkbox.locator('xpath=..');

    const startedHint = checkboxItem.locator(
      'xpath=following-sibling::div[contains(@class, "govuk-checkboxes__hint")][1]',
    );

    await expect(startedHint).toBeVisible();
    await expect(startedHint).toHaveText(
      `Self-assessment started on ${currentDate}`,
    );
  },
);