import { Then } from "@cucumber/cucumber";
import { expect } from "@playwright/test";

Then('I click the view all recommendations answers link for {string}',
  async function (category: string) {
    const link = this.page.locator(
      `a`, {hasText: `View answers for ${category}`}
    )

    await expect(link).toBeVisible();
    await link.click();
  }
);

Then('I should see the question {string} with the answer {string}',
   async function (question: string, answer: string) { 

    // const contentBlock = this.page.locator(
    //   // `div:has(p.govuk-!-font-weight-bold:has-text("${question}"))`
    //   `div:has-text("${question}")`
    // );

    const content = this.page.locator('p.govuk-\\!-font-weight-bold').filter({
      hasText: question
    })

    const contentBlock = content.locator('xpath=ancestor::div[1]')

    await expect(contentBlock).toBeVisible();
    await expect(contentBlock).toContainText(answer);
});