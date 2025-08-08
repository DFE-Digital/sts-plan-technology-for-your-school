import { Given, When } from "@cucumber/cucumber";
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

Given('I click the submit and view recommendations button', async function () {
  const submitBtn = this.page.locator('button[value="GetRecommendation"]');
  await submitBtn.click();
});

Given('I start a test assessment on {string} with answers {string}', async function (section: string, answers: string) {
 
  await startAndAnswerAssessment(this, section, answers);

  // Final submit to reach recommendations
  const submitBtn = this.page.locator('button[value="GetRecommendation"]');
  await submitBtn.click();
});

Given('I start a test assessment on {string} with answers {string} and I do not click submit recommendations', async function (section: string, answers: string) {
    await startAndAnswerAssessment(this, section, answers);
});

async function startAndAnswerAssessment(context:any, section:string, answers:string ) {
  //Go to page
   await context.page.goto(`${process.env.URL}self-assessment-testing`);

   //Select and click the card
  const sectionCard = context.page.locator('.dfe-card', {
    has: context.page.locator('a', { hasText: section }),
  }).first();

  await sectionCard.locator('a').click();
  //Check the expected path
  const expectedPath = `${textToHyphenatedUrl(section)}`;
  await context.page.waitForURL(`${process.env.URL}${expectedPath}`);

  //Get the start self assessment button on interstitial page
  const startSelfAssessmentBtn = context.page.getByRole('button', { name: 'Start self-assessment' });
  const href = await startSelfAssessmentBtn.getAttribute('href');
  
  await startSelfAssessmentBtn.click();

  const answerSequence = answers.split(',').map(a => a.trim());

  for (const answerIndex of answerSequence) {
    const index = parseInt(answerIndex, 10) - 1;
    const radioButtons = context.page.locator('input[type="radio"]');
    await radioButtons.nth(index).check();
    await context.page.getByRole('button', { name: 'Continue' }).click();
  }
}