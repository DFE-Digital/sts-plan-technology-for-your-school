import { Given } from "@cucumber/cucumber";
import { textToHyphenatedUrl } from "../../helpers/url";
import { expect } from "@playwright/test";

Given('I start an assessment on category {string}', async function (section: string) {
  //Go to the self-assessment-testing page
  await this.page.goto(`${process.env.URL}self-assessment-testing`);

  //Locate and click on the card from the parameter passed in
  const sectionCard = this.page.locator('.dfe-card', {
  has: this.page.locator('a', { hasText: section }),
  }).first();

  const sectionCardLink = sectionCard.locator(`a`);

  await sectionCardLink.click();

  //Check the expected path
  const expectedPath = `${textToHyphenatedUrl(section)}`;
  await this.page.waitForURL(`${process.env.URL}${expectedPath}`);

  //Get the start self assessment button on interstitial page
  const startSelfAssessmentBtn = this.page.getByRole('button', { name: 'Start self-assessment' });
  const href = await startSelfAssessmentBtn.getAttribute('href');
  
  //Assert the path is correct on the button.
  expect(href).toBe(`/${expectedPath}/next-question`)
  
  startSelfAssessmentBtn.click();
});