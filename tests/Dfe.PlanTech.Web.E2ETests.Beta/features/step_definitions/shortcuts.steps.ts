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

Given('I start a test assessment on {string} with answers {string}', async function (section: string, answers: string) {
  await this.page.goto(`${process.env.URL}self-assessment-testing`);
  const sectionCard = this.page.locator('.dfe-card', {
    has: this.page.locator('a', { hasText: section }),
  }).first();

  await sectionCard.locator('a').click();
  //Check the expected path
  const expectedPath = `${textToHyphenatedUrl(section)}`;
  await this.page.waitForURL(`${process.env.URL}${expectedPath}`);

  //Get the start self assessment button on interstitial page
  const startSelfAssessmentBtn = this.page.getByRole('button', { name: 'Start self-assessment' });
  const href = await startSelfAssessmentBtn.getAttribute('href');
  
  await startSelfAssessmentBtn.click();

  const answerSequence = answers.split(',').map(a => a.trim());

  for (const answerIndex of answerSequence) {
    const index = parseInt(answerIndex, 10) - 1;
    const radioButtons = this.page.locator('input[type="radio"]');
    await radioButtons.nth(index).check();
    await this.page.getByRole('button', { name: 'Continue' }).click();
  }

  // Final submit to reach recommendations
  const submitBtn = this.page.locator('button[value="GetRecommendation"]');
  await submitBtn.click();
});