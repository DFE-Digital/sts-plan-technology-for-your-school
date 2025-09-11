import { Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';
import { getCurrentShortDate, normaliseShortDateTimeText } from '../../helpers/datetime';

Then(
  'I should see the {string} section {string} with description {string} and link href {string}',
  async function (state: 'not started' | 'in progress' | 'completed', heading: string, description: string, href: string) {
    const container = this.page.locator('#main-content');

    var currentDate = getCurrentShortDate();

    // heading
    const headingEl = container.getByRole('heading', { level: 2, name: heading });
    const descEl = headingEl.locator('xpath=following-sibling::p[1]');
    await expect(headingEl).toBeVisible();

    const sectionLower = heading.toLowerCase();

    if (state === 'not started') {

      const linkEl = headingEl.locator('xpath=following-sibling::p[a][1]/a');
      // check description
      const descEl = headingEl.locator('xpath=following-sibling::p[1]');
      await expect(descEl).toHaveText(description);

      await expect(linkEl).toBeVisible();
      await expect(linkEl).toHaveText(`Go to self-assessment for ${sectionLower}`);
      await expect(linkEl).toHaveAttribute('href', href);
      return;
    }

    if (state === 'in progress') {
          // check description
    await expect(descEl).toHaveText(description);

      // <p> with start date + continue link
      const inProgressPara = headingEl.locator(
        'xpath=following-sibling::p[contains(normalize-space(.), "A self-assessment was started on")]'
      ).first();

      expect(inProgressPara).toBeVisible();

      const text = (await inProgressPara.innerText()).trim();
      const normalisedText =  normaliseShortDateTimeText(text);
      // e.g. A self-assessment was started on 27 Aug 2025.
      const expectedText = `A self-assessment was started on ${currentDate}.`;
      expect(normalisedText).toMatch(expectedText);

      const link = inProgressPara.getByRole('link', {
        name: `Continue your self-assessment for ${sectionLower}`,
      });
      await expect(link).toBeVisible();
      await expect(link).toHaveAttribute('href', href);
      return;
    }

    if (state === 'completed') {
      // paragraph with completion date + view/update link follows elsewhere
      const completionPara = headingEl.locator(
        'xpath=following-sibling::p[contains(normalize-space(.), "was completed on")]'
      ).first();
      expect(completionPara).toBeVisible();

      const text = (await completionPara.innerText()).trim();
      const normalisedText =  normaliseShortDateTimeText(text);
      
      // e.g. The self-assessment for {sectionLower} was completed on 27 Aug 2025.
      const expectedText = `The self-assessment for ${sectionLower} was completed on ${currentDate}.`;
      expect(normalisedText).toMatch(expectedText);

      const viewLink = headingEl.locator(
        'xpath=following-sibling::p[a[contains(normalize-space(.), "View or update your self-assessment")]][1]/a'
      );
      await expect(viewLink).toBeVisible();
      await expect(viewLink).toHaveText(`View or update your self-assessment for ${sectionLower}`);
      await expect(viewLink).toHaveAttribute('href', href);
      return;
    }

    throw new Error(`Unknown state "${state}". Use "not started", "in progress", or "completed".`);
  }
);
function normaliseSeptemberShortDateTimeText(text: any) {
  throw new Error('Function not implemented.');
}

