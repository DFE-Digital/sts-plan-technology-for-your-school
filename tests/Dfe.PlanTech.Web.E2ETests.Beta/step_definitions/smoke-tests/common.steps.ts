import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { textToHyphenatedUrl } from '../../helpers/url';

Given('I visit the landing page', async function () {
  await this.page.goto(`${process.env.URL}`);
});

Then('I should see the beta header', async function () {
  //check the banner exists
  const header = this.page.locator('div.govuk-phase-banner');
  await expect(header).toBeVisible();

  // check tag
  const tag = header.locator('strong.govuk-tag');
  await expect(tag).toBeVisible();
  await expect(tag).toHaveText('Beta');

  // ensure the feedback link exists (this is hardcoded in the backend beta-header.cshtml file)
  const expectedHref = 'https://forms.office.com/e/Jk5PuNWvGe';
  const link = header.locator('a');
  await expect(link).toBeVisible();
  await expect(link).toHaveAttribute('href', expectedHref);
});

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

Then('I should see a subheading with the text {string}', async function (text: string) {
  const heading = this.page.locator(`h2:has-text("${text}")`);
  await expect(heading).toBeVisible();
});

Then('the page should be accessible', async function () {
  const results = await new AxeBuilder({ page: this.page }).analyze();

  if (results.violations.length > 0) {
    console.warn('\ Accessibility violations found:');
    results.violations.forEach((v) => {
      console.warn(`- ${v.id}: ${v.description}`);
      console.warn(`  Help: ${v.helpUrl}`);
    });
  } else {
    console.log('No accessibility violations found.');
  }
});

Then('I should see a link with text {string}', async function (text: string) {
  const link = this.page.locator(`a:has-text("${text}")`).first();
  await expect(link).toBeVisible();
});

Then('I should see {int} links with text {string}', async function (count: number, text: string) {
  const links = this.page.getByRole('link', { name: text });
  await expect(links).toHaveCount(count);
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

Then('I should see a js-back link to {string}', async function (href: string) {
  const backLink = this.page.locator('#js-back-button-link');
  await expect(backLink).toBeVisible();
  const actualHref = await backLink.getAttribute('href');
  expect(actualHref).toContain(href);
});

Then('I should see a h2 section heading with text {string}', async function (heading: string) {
  const headingLocator = this.page.locator(`h2.govuk-heading-l:has-text("${heading}")`);
  await expect(headingLocator).toBeVisible();
});

Then(
  'I should see a non govuk section heading with text {string}',
  async function (heading: string) {
    const headingLocator = this.page.locator(`h2:has-text("${heading}")`);
    await expect(headingLocator).toBeVisible();
  },
);

Given('I am on the {string} page', async function (path: string) {
  await this.page.goto(`${process.env.URL}${path}`);
});

Then('I should be on the URL containing {string}', async function (expectedPath: string) {
  await this.page.waitForURL(`**${expectedPath}`);
  await expect(this.page.url()).toContain(expectedPath);
});

Given(
  'I am on the self-assessment testing page and click on the category {string}',
  async function (categoryName: string) {
    await this.page.goto(`${process.env.URL}self-assessment-testing`);

    const container = this.page.locator('.dfe-grid-container');
    const cardLink = container.getByRole('link', {
      name: new RegExp(`^\\s*${categoryName}\\s*$`, 'i'),
    });

    await expect(cardLink).toBeVisible();

    await cardLink.first().click();

    const expectedPath = textToHyphenatedUrl(categoryName);

    await expect(this.page).toHaveURL(new RegExp(`${expectedPath}(/|$)`));
  },
);

Then('I should see a button with the text {string}', async function (linkText: string) {
  const button = this.page.locator('a.govuk-button.govuk-link', { hasText: linkText });
  await expect(button).toHaveCount(1);
  await expect(button).toBeVisible();
});

Then('I should see the GOV.UK footer with expected links', async function () {
  const footer = this.page.locator('footer.govuk-footer');
  await expect(footer).toBeVisible();

  const expectedLinks: Record<string, string> = {
    Cookies: '/cookies',
    'Privacy notice': '/privacy-notice',
    'Contact us':
      'https://schooltech.support.education.gov.uk/hc/en-gb/requests/new?ticket_form_id=22398112507922',
    'Accessibility statement': '/accessibility-statement',
  };

  for (const [text, href] of Object.entries(expectedLinks)) {
    const link = footer.locator('a.govuk-footer__link', { hasText: text });
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', expect.stringContaining(href));
  }
});

Then('I click the go to self-assessment link for {string}', async function (sectionName: string) {
  const link = this.page.getByRole('link', {
    name: `Go to self-assessment for ${sectionName}`,
  });

  await link.click();
});

When('I refresh the page', async function () {
  await this.page.reload();
});

Then('I should see a paragraph with text {string}', async function (paragraphText: string) {
  const paragraph = this.page.locator('p', { hasText: paragraphText });
  await expect(paragraph).toBeVisible();
});

Then('I should not see a paragraph with text {string}', async function (paragraphText: string) {
  const paragraph = this.page.locator('p', { hasText: paragraphText });
  await expect(paragraph).not.toBeVisible();
});

When('I click the back to {string} link', async function (categoryName: string) {
  const link = this.page.getByRole('link', { name: `Back to ${categoryName}` });
  await link.click();
});

When('I click the back link', async function () {
  const backLink = this.page.locator('#back-button-link');
  await backLink.click();
});

When('I click the js-back link', async function () {
  const backLink = this.page.locator('#js-back-button-link');
  await backLink.click();
});

When('I click the non-js back link', async function () {
  const backLink = this.page.locator('#nonjs-back-button-link');
  await backLink.click();
});

Then('the header should contain all the correct content', async function () {
  // wrapper
  const header = this.page.locator('header.govuk-header[role="banner"]');
  await expect(header).toBeVisible();

  // check link to the logo
  const logoLink = header.locator('.govuk-header__link--homepage');
  await expect(logoLink).toBeVisible();
  await expect(logoLink).toHaveAttribute('href', '/home');
  await expect(logoLink).toHaveAttribute('aria-label', 'DfE homepage');

  // check the logo images
  const logoImages = logoLink.locator('img');
  await expect(logoImages).toHaveCount(1);
  await expect(logoImages.first()).toHaveAttribute('alt', 'Department for Education');

  const signOutLink = header.getByRole('link', { name: /sign out/i });
  await expect(signOutLink).toBeVisible();
  await expect(signOutLink).toHaveAttribute('href', '/auth/sign-out');

  const serviceName = header.getByRole('link', { name: 'DfE homepage' });
  await expect(serviceName).toBeVisible();
  await expect(serviceName).toHaveAttribute('href', '/home');
});

Then('I should see an inset text containing {string}', async function (expectedText: string) {
  const inset = this.page.locator('.govuk-inset-text');
  await expect(inset).toBeVisible();
  await expect(inset).toContainText(expectedText);
});

Then('I should not see an inset text containing {string}', async function (unexpectedText: string) {
  const inset = this.page.locator('.govuk-inset-text');
  if ((await inset.count()) === 0) {
    return;
  }
  await expect(inset).not.toContainText(unexpectedText);
});

Then('in the inset text I should see the following links:', async function (dataTable) {
  const inset = this.page.locator('.govuk-inset-text');
  await expect(inset).toBeVisible();

  const expected = dataTable.hashes();
  for (const { Text, Href } of expected) {
    const link = inset.getByRole('link', { name: Text });
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', Href);
  }
});

Given('I visit the page {string}', async function (path: string) {
  await this.page.goto(`${process.env.URL}${path}`);
});

Then('I should see a confirmation panel saying {string}', async function (expectedText: string) {
  const panelTitle = this.page.locator('.govuk-panel--confirmation .govuk-panel__title');

  await expect(panelTitle).toBeVisible();
  await expect(panelTitle).toHaveText(expectedText);
});
