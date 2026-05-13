import { Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Then(
  "I set the email to {string}",
  async function (email: string) {
    const page = this.page;

    const notesBox = page.locator("#recipient-email-0");
    await notesBox.fill(email);

    await expect(notesBox).toHaveValue(email);
  }
);

Then(
  "I set the name to {string}",
  async function (name: string) {
    const page = this.page;

    const notesBox = page.locator("#name-of-user");
    await notesBox.fill(name);

    await expect(notesBox).toHaveValue(name);
  }
);

Then(
  "I set the note to {string}",
  async function (note: string) {
    const page = this.page;

    const notesBox = page.locator("#user-message");
    await notesBox.fill(note);

    await expect(notesBox).toHaveValue(note);
  }
);

When(
  "I click the send email button",
  async function () {
    const page = this.page;

    const submitButton = page.getByRole("button", { name: "Send email"});

    await submitButton.click()
  }
);

Then(
  "I see the email has been sent header with the email {string}",
  async function(email: string) {
    const page = this.page;
    
    const headerLocator = page.locator('h2.govuk-panel__title', {
    hasText: "An email has been sent to:",
    });

    const divLocator = page.locator('div.govuk-panel__body', {
    hasText: email,
  });
  }
);
