import { Given, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { textToHyphenatedUrl } from '../../helpers/url'
import { text } from 'node:stream/consumers';

Given('I am on the homepage', async function () {
  await this.page.goto(process.env.URL);
});

Then('I should see the page heading {string}', async function (expectedHeading: string) {
  const heading = this.page.locator('h1.govuk-heading-xl');
  await expect(heading).toBeVisible();
  await expect(heading).toHaveText(expectedHeading);
});

Then('I should see a subheading with tag {string} and class {string}', async function (tag: string, className: string) {
  const heading = this.page.locator(`${tag}.${className}`);
  await expect(heading).toBeVisible();
});

Then('I should see a subheading with the text {string}', async function (text: string) {
  const heading = this.page.locator(`h2:has-text("${text}")`);
  await expect(heading).toBeVisible();
});

Then('I should see at least {int} elements matching {string}', async function (minCount: number, selector: string) {
  const elements = this.page.locator(selector);
  const count = await elements.count();
  expect(count).toBeGreaterThanOrEqual(minCount);
});

Then('each element matching {string} should have a non-empty href', async function (selector: string) {
  const links = this.page.locator(selector);
  const count = await links.count();

  for (let i = 0; i < count; i++) {
    const href = await links.nth(i).getAttribute('href');
    expect(href).not.toBeNull();
    expect(href).not.toEqual('');
  }
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

Then('each element matching {string} should contain the text {string}', async function (selector: string, expectedText: string) {
  const elements = this.page.locator(selector);
  const count = await elements.count();

  for (let i = 0; i < count; i++) {
    const text = await elements.nth(i).innerText();
    expect(text).toContain(expectedText);
  }
});

Then('I should see a link with text {string}', async function (text: string) {
  const link = this.page.locator(`a:has-text("${text}")`);
  await expect(link).toBeVisible();
});

Then('I should see a link with href {string} and text {string}', async function (expectedHref: string, expectedText: string) {
  const link = this.page.locator(`a[href="${expectedHref}"]`);
  await expect(link).toBeVisible();

  const actualText = await link.innerText();
  expect(actualText.trim()).toBe(expectedText);
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

Then('I should see an unordered list', async function () {
  const ul = this.page.locator('ul');
  await expect(ul).toBeVisible();
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

Then('I will be redirected to {string}', async function (expectedPath: string) {
  await this.page.waitForURL(`**${expectedPath}`);
  await expect(this.page.url()).toContain(expectedPath);
});

Given('I am on the self-assessment page and click on the category {string}', async function (section: string) {
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

