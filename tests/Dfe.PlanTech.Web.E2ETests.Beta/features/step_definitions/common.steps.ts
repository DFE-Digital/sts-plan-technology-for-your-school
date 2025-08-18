import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { textToHyphenatedUrl } from '../../helpers/url'
import { text } from 'node:stream/consumers';

Given('I visit the homepage', async function () {
  await this.page.goto(`${process.env.URL}home`);
});

Given('I visit the self-assessment-testing page', async function () {
  await this.page.goto(`${process.env.URL}self-assessment-testing`);
});

Then('I should see the page heading {string}', async function (expectedHeading: string) {
  const heading = this.page.locator('h1.govuk-heading-xl:visible');
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(expectedHeading);
});

Then('I should see the section heading {string}', async function (expectedHeading: string) {
  const heading = this.page.locator('h3.govuk-body-l:visible');
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(expectedHeading);
});

Then('I should see the caption heading {string}', async function (expectedHeading: string) {
  const heading = this.page.locator('h3.govuk-caption-xl:visible');
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(expectedHeading);
});

Then('I should see the question heading {string}', async function (expectedHeading: string) {
  const heading = this.page.locator('h1.govuk-fieldset__heading');
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(expectedHeading);
});


Then('the page should be accessible', async function () {
  const results = await new AxeBuilder({ page: this.page }).analyze();

  if (results.violations.length > 0) {
    console.warn('\ Accessibility violations found:');
    results.violations.forEach(v => {
      console.warn(`- ${v.id}: ${v.description}`);
      console.warn(`  Help: ${v.helpUrl}`);
    });
  } else {
    console.log('No accessibility violations found.');
  }
});

Then('I should see a link with text {string}', async function (text: string) {
  const link = this.page.locator(`a:has-text("${text}")`);
  await expect(link).toBeVisible();
});

Then('I should see the main page heading', async function () {
  const heading = this.page.locator('h1.govuk-heading-xl');
  await expect(heading).toBeVisible();
});

Then('I should see multiple feature headings', async function () {
  const headings = this.page.locator('h2.govuk-heading-l');
  const count = await headings.count();
  expect(count).toBeGreaterThanOrEqual(2);
});

Then('I should see multiple explanatory text blocks', async function () {
  const paragraphs = this.page.locator('p');
  const count = await paragraphs.count();
  expect(count).toBeGreaterThanOrEqual(4);
});

Then('I should see multiple list items', async function () {
  const items = this.page.locator('ul li');
  const count = await items.count();
  expect(count).toBeGreaterThanOrEqual(4);
});

Then('I should see a start button', async function () {
  const button = this.page.locator('a.govuk-button--start.govuk-button');
  await expect(button).toBeVisible();
});

Then('the start button should link to {string}', async function (expectedHref: string) {
  const button = this.page.locator('a.govuk-button--start.govuk-button');
  const href = await button.getAttribute('href');
  expect(href).toBe(expectedHref);
});

Then('I should see a back link to {string}', async function (href: string) {
  const backLink = this.page.locator('#back-button-link');
  await expect(backLink).toBeVisible();
  const actualHref = await backLink.getAttribute('href');
  expect(actualHref).toContain(href);
});

Then('I should see a section heading with text {string}', async function (heading: string) {
  const headingLocator = this.page.locator(`h2.govuk-heading-l:has-text("${heading}")`);
  await expect(headingLocator).toBeVisible();
});

Then('I should see a non govuk section heading with text {string}', async function (heading: string) {
  const headingLocator = this.page.locator(`h2:has-text("${heading}")`);
  await expect(headingLocator).toBeVisible();
});

Given('I am on the {string} page', async function (path: string) {
  await this.page.goto(`${process.env.URL}${path}`);
});

Then('I should be on the URL containing {string}', async function (expectedPath: string) {
  await this.page.waitForURL(`**${expectedPath}`);
  await expect(this.page.url()).toContain(expectedPath);
});

Given('I am on the self-assessment testing page and click on the category {string}', async function (section: string) {
  await this.page.goto(`${process.env.URL}self-assessment-testing`);

  const firstCard = this.page.locator(`.dfe-grid-container`).first();
  const firstCardLink = firstCard.locator(`a`);

  const linkText = await firstCardLink.textContent();
  if (!linkText) {
    throw new Error('Could not get text from first card link');
  }

  await firstCardLink.click();

  const expectedPath = `${textToHyphenatedUrl(linkText)}`;
  await this.page.waitForURL(`${process.env.URL}${expectedPath}`);
  expect(this.page.url()).toContain(textToHyphenatedUrl(linkText));
});

Then('I should see a button with the text {string}', async function (linkText: string) {
  const button = this.page.locator('a.govuk-button.govuk-link', { hasText: linkText });
  await expect(button).toHaveCount(1);
  await expect(button).toBeVisible();
});

Then('I should see the GOV.UK footer with expected links', async function () {
  const footer = this.page.locator('footer.govuk-footer');
  await expect(footer).toBeVisible();

  const expectedLinks: Record<string, string> = {
    'Cookies': '/cookies',
    'Privacy notice': '/privacy-notice',
    'Contact us': 'https://schooltech.support.education.gov.uk/hc/en-gb/requests/new?ticket_form_id=22398112507922',
    'Accessibility statement': '/accessibility-statement',
  };

  for (const [text, href] of Object.entries(expectedLinks)) {
    const link = footer.locator('a.govuk-footer__link', { hasText: text });
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', expect.stringContaining(href));
  }
});

Then('I click the go to self-assessment link for {string}',
  async function (sectionName: string) {
    const link = this.page.getByRole('link', {
      name: `Go to self-assessment for ${sectionName}`,
    });

    await link.click();
  }
);

When('I refresh the page', async function () {
  await this.page.reload();
});

Then('I should see a paragraph with text {string}', async function (paragraphText: string) {
  const paragraph = this.page.locator('p', { hasText: paragraphText });
  await expect(paragraph).toBeVisible();
});


When('I click the back to {string} link', async function (standardName: string) {
  const link = this.page.getByRole('link', { name: `Back to ${standardName}` });
  await link.click();
});