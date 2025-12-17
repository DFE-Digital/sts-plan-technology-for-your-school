import { Given, When, Then, DataTable } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

const CONTENTFUL_COMPONENTS_PATH = 'component-testing';

Given('I am on the Contentful components test page', async function () {
  await this.page.goto(`${process.env.URL}${CONTENTFUL_COMPONENTS_PATH}`);
});

Then('I should see bold rich text with text {string}', async function (text: string) {
  const bold = this.page.locator('[class*="govuk-!-font-weight-bold"]', { hasText: text });
  await expect(bold).toBeVisible();
});

Then(
  'I should see a rich text paragraph with text {string}',
  async function (text: string) {
    const paragraph = this.page.locator('main p', { hasText: text });
    await expect(paragraph).toBeVisible();
  }
);

Then(
  'I should see a hyperlink with text {string} and href {string}',
  async function (linkText: string, href: string) {
    const link = this.page.getByRole('link', { name: linkText });
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', href);
  }
);

Then(
  'I should see headings with the following texts:',
  async function (dataTable: DataTable) {
    const expectedHeadings = dataTable.raw().flat();
    for (const text of expectedHeadings) {
      const heading = this.page
        .locator('h1, h2, h3, h4, h5, h6')
        .filter({ hasText: text });
      await expect(heading, `Heading not found: ${text}`).toBeVisible();
    }
  }
);

Then(
  'I should see an unordered list with items:',
  async function (dataTable: DataTable) {
    const items = dataTable.raw().flat();

    const ul = this.page
      .locator('main ul')                
      .filter({ hasText: items[0] })   
      .first();

    await expect(ul, 'Expected unordered list was not found').toBeVisible();

    for (const item of items) {
      const li = ul.locator('li', { hasText: item });
      await expect(li, `List item not found: ${item}`).toBeVisible();
    }
  }
);


Then(
  'I should see an ordered list with items:',
  async function (dataTable: DataTable) {
    const items = dataTable.raw().flat();

    const ol = this.page.locator('ol').first();
    await expect(ol).toBeVisible();

    for (const item of items) {
      const li = ol.locator('li', { hasText: item });
      await expect(li, `OL item not found: ${item}`).toBeVisible();
    }
  }
);


Then(
  'I should see a table with column headings:',
  async function (dataTable: DataTable) {
    const headers = dataTable.raw().flat();
    const table = this.page.locator('table.govuk-table').first();
    await expect(table).toBeVisible();

    for (const headerText of headers) {
      const headerCell = table
        .locator('thead .govuk-table__header')
        .filter({ hasText: headerText });
      await expect(headerCell, `Missing table header: ${headerText}`).toBeVisible();
    }
  }
);

Then(
  'I should see a table row with values:',
  async function (dataTable: DataTable) {
    const [rowValues] = dataTable.raw(); // single row
    const table = this.page.locator('table.govuk-table').first();
    const rows = table.locator('tbody .govuk-table__row');
    const rowCount = await rows.count();

    let matchFound = false;

    for (let i = 0; i < rowCount; i++) {
      const row = rows.nth(i);
      const cells = await row.locator('th, td').allInnerTexts();
      const trimmedCells = cells.map((c: string) => c.trim());

      if (trimmedCells.length === rowValues.length &&
          trimmedCells.every((value: string, idx: number) => value === rowValues[idx])) {
        matchFound = true;
        break;
      }
    }

    expect(matchFound).toBeTruthy();
  }
);


Then('I should see a notification banner with title {string}', async function (title: string) {
  const banner = this.page.locator('.govuk-notification-banner');
  await expect(banner).toBeVisible();

  const titleElement = banner.locator('.govuk-notification-banner__title');
  await expect(titleElement).toHaveText(title);
});

Then('I should see notification banner text {string}', async function (text: string) {
  const banner = this.page.locator('.govuk-notification-banner');
  const content = banner.locator('.govuk-notification-banner__content');

  await expect(content).toContainText(text);
});

Then(
  'the notification banner should contain a hyperlink with text {string} and href {string}',
  async function (text: string, href: string) {
    const banner = this.page.locator('.govuk-notification-banner');
    const link = banner.getByRole('link', { name: text });

    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', href);
  }
);

Then(
  'the notification banner should contain bold text {string}',
  async function (text: string) {
    const banner = this.page.locator('.govuk-notification-banner');

    const bold = banner.locator('[class*="govuk-!-font-weight-bold"]', { hasText: text });

    await expect(
      bold,
      `Bold text not found in notification banner: ${text}`
    ).toBeVisible();
  }
);


Then(
  'I should see inset text {string}',
  async function (text: string) {
    const inset = this.page.locator('.govuk-inset-text');
    await expect(inset).toBeVisible();
    await expect(inset).toHaveText(text);
  }
);


Then(
  'I should see an attachment titled {string}',
  async function (title: string) {
    const attachmentTitle = this.page
      .locator('.attachment .attachment-title a')
      .filter({ hasText: title });
    await expect(attachmentTitle).toBeVisible();
  }
);

Then(
  'the attachment should have file type {string} and size {string}',
  async function (fileType: string, fileSize: string) {
    const metadata = this.page.locator(
      '.attachment .attachment-metadata'
    ).first(); // first metadata block 

    await expect(metadata).toContainText(fileType);
    await expect(metadata).toContainText(fileSize);
  }
);

Then(
  'the attachment should have last updated text {string}',
  async function (lastUpdated: string) {
    const lastUpdatedMeta = this.page
      .locator('.attachment .attachment-metadata')
      .nth(1); // second metadata block

    await expect(lastUpdatedMeta).toContainText(lastUpdated);
  }
);


function getAccordionSection(page: any, heading: string) {
  const accordion = page.locator('.govuk-accordion').first();

  const section = accordion
    .locator('.govuk-accordion__section')
    .filter({ hasText: heading });

  return section;
}




function getAccordionSectionContent(page: any, heading: string) {
  const section = getAccordionSection(page, heading);
  return section.locator('.govuk-accordion__section-content').first();
}


When(
  'I expand the accordion section {string}',
  async function (heading: string) {
    const section = getAccordionSection(this.page, heading);

    await expect(
      section,
      `Accordion section not found: ${heading}`
    ).toHaveCount(1);

    const button = section.locator('.govuk-accordion__section-button').first();
    await expect(button).toBeVisible();
    await button.click();
  }
);

Then(
  'the accordion section {string} should contain text {string}',
  async function (heading: string, text: string) {
    const content = getAccordionSectionContent(this.page, heading);
    await expect(content).toContainText(text);
  }
);


Then(
  'the accordion section {string} should contain bold text {string}',
  async function (heading: string, text: string) {
    const content = getAccordionSectionContent(this.page, heading);

    const bold = content.locator(
      '[class*="govuk-!-font-weight-bold"]',
      { hasText: text }
    );

    await expect(
      bold,
      `Bold text not found in section "${heading}": ${text}`
    ).toBeVisible();
  }
);


Then(
  'the accordion section {string} should contain a hyperlink with text {string} and href {string}',
  async function (heading: string, linkText: string, href: string) {
    const content = getAccordionSectionContent(this.page, heading);

    const link = content.getByRole('link', { name: linkText });
    await expect(
      link,
      `Link "${linkText}" not found in section "${heading}"`
    ).toBeVisible();
    await expect(link).toHaveAttribute('href', href);
  }
);


Then(
  'the accordion section {string} should contain an unordered list with items:',
  async function (heading: string, dataTable: DataTable) {
    const items = dataTable.raw().flat();
    const content = getAccordionSectionContent(this.page, heading);

    const ul = content
      .locator('ul')
      .filter({ hasText: items[0] })
      .first();

    await expect(
      ul,
      `Unordered list with first item "${items[0]}" not found in section "${heading}"`
    ).toBeVisible();

    for (const item of items) {
      const li = ul.locator('li', { hasText: item });
      await expect(
        li,
        `List item "${item}" not found in section "${heading}"`
      ).toBeVisible();
    }
  }
);


Then(
  'the accordion section {string} should contain an ordered list with items:',
  async function (heading: string, dataTable: DataTable) {
    const items = dataTable.raw().flat();
    const content = getAccordionSectionContent(this.page, heading);

    const ol = content
      .locator('ol')
      .filter({ hasText: items[0] })
      .first();

    await expect(
      ol,
      `Ordered list with first item "${items[0]}" not found in section "${heading}"`
    ).toBeVisible();

    for (const item of items) {
      const li = ol.locator('li', { hasText: item });
      await expect(
        li,
        `Numbered list item "${item}" not found in section "${heading}"`
      ).toBeVisible();
    }
  }
);




Then('I should see the large header {string}', async function (text: string) {
  const header = this.page.locator('h1.govuk-heading-l', { hasText: text });
  await expect(header).toBeVisible();
});