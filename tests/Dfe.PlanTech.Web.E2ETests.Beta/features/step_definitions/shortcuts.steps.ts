import { Given, When } from "@cucumber/cucumber";
import { textToHyphenatedUrl } from "../../helpers/url";
import { expect } from "@playwright/test";

Given('I start an assessment on standard {string} section {string}', async function (standard: string, section: string) {

  //Go to page
   await this.page.goto(`${process.env.URL}self-assessment-testing`);

   //Select and click the card
  const standardCard = this.page.locator('.dfe-card', {
    has: this.page.locator('a', { hasText: standard }),
  }).first();

  await standardCard.locator('a').click();
  //Check the expected path
  const expectedPath = `${textToHyphenatedUrl(standard)}`;
  await this.page.waitForURL(`${process.env.URL}${expectedPath}`);

  //Find the and click the go to self assessment link

  const sectionCard = await this.page.locator('a', { hasText: section});
  await sectionCard.click();

  //Get the start self assessment button on interstitial page
  const startSelfAssessmentBtn = this.page.getByRole('button', { name: 'Start self-assessment' });
  const href = await startSelfAssessmentBtn.getAttribute('href');
  
  await startSelfAssessmentBtn.click();
});


Given('I click the submit and view recommendations button', async function () {
  const submitBtn = this.page.getByRole('button', { name: 'Submit and view recommendations' })
  await submitBtn.click();
});

Given('I start a test assessment on {string} standard {string} section with answers {string}', async function (standard:string, section: string, answers: string) {
 
  await startAndAnswerAssessment(this, standard, section, answers);

  // Final submit to reach recommendations
  const submitBtn = this.page.getByRole('button', { name: 'Submit and view recommendations' })
  await submitBtn.click();
  
});

Given('I start a test assessment on {string} standard {string} section with answers {string} and I do not click submit recommendations', async function (standard:string, section: string, answers: string) {
    await startAndAnswerAssessment(this, standard, section, answers);
});

async function startAndAnswerAssessment(context:any, standard: string, section:string,  answers:string ) {
  //Go to page
   await context.page.goto(`${process.env.URL}self-assessment-testing`);

   //Select and click the card
  const standardCard = context.page.locator('.dfe-card', {
    has: context.page.locator('a', { hasText: standard }),
  }).first();

  await standardCard.locator('a').click();
  //Check the expected path
  const expectedPath = `${textToHyphenatedUrl(standard)}`;
  await context.page.waitForURL(`${process.env.URL}${expectedPath}`);

  //Find the and click the go to self assessment link

  const sectionCard = await context.page.locator('a', { hasText: section});
  await sectionCard.click();

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