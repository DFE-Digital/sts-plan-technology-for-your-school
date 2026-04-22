import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import type { Page } from "@playwright/test";
import { MAT_SCHOOLS_BY_URN } from "../../constants/matConstants";

function normalizeSpaces(s: string) {
  return s.replace(/\u00a0/g, " ").replace(/\s+/g, " ").trim();
}

function accordion(page: Page) {
  return page.locator("#accordion-default.govuk-accordion");
}

function showAllButton(page: Page) {
  return accordion(page).locator("button.govuk-accordion__show-all");
}

function sectionToggleButtonByHeading(page: Page, headingText: string) {
  // The visible heading text is inside the section button (contains an h3 in your markup).
  return accordion(page).locator("button.govuk-accordion__section-button", { hasText: headingText });
}

async function sectionContentByHeading(page: Page, headingText: string) {
  const btn = sectionToggleButtonByHeading(page, headingText);
  await expect(btn).toHaveCount(1);

  const contentId = await btn.getAttribute("aria-controls");
  if (!contentId) throw new Error(`No aria-controls found for section heading "${headingText}"`);

  return page.locator(`#${contentId}`);
}

function mostRecentSectionButton(page: any) {
  // first section in the accordion = most recent (top of list)
  return page
    .locator("#accordion-default .govuk-accordion__section")
    .first()
    .locator("button.govuk-accordion__section-button");
}

async function mostRecentSectionContent(page: any) {
  const btn = mostRecentSectionButton(page);
  await expect(btn).toHaveCount(1);

  const contentId = await btn.getAttribute("aria-controls");
  if (!contentId) throw new Error("Most recent accordion section button is missing aria-controls");

  return page.locator(`#${contentId}`);
}

async function ensureMostRecentSectionExpanded(page: any) {
  const btn = mostRecentSectionButton(page);
  const expanded = (await btn.getAttribute("aria-expanded")) === "true";
  if (!expanded) await btn.click();
  await expect(btn).toHaveAttribute("aria-expanded", "true");
}

function monthYearFromHeading(headingText: string): { month: string, year: string } {
  // "February 2026 activity"
  const m = headingText.trim().match(/^([A-Za-z]+)\s+(\d{4})\s+activity$/i);
  if (!m) throw new Error(`Could not parse month/year from heading "${headingText}"`);
  return { month: m[1], year: m[2] };
}

async function ensureSectionExpanded(page: Page, headingText: string) {
  const btn = sectionToggleButtonByHeading(page, headingText);
  await expect(btn).toHaveCount(1);

  const expanded = (await btn.getAttribute("aria-expanded")) === "true";
  if (!expanded) await btn.click();
  await expect(btn).toHaveAttribute("aria-expanded", "true");
}

async function ensureSectionCollapsed(page: Page, headingText: string) {
  const btn = sectionToggleButtonByHeading(page, headingText);
  await expect(btn).toHaveCount(1);

  const expanded = (await btn.getAttribute("aria-expanded")) === "true";
  if (expanded) await btn.click();
  await expect(btn).toHaveAttribute("aria-expanded", "false");
}


//show/hide all 

When("I expand the most recent recent activity section", async function () {
  const page = this.page;
  await ensureMostRecentSectionExpanded(page);
});

When("I click {string} on the recent activity accordion", async function (label: string) {
  const page = this.page;

  const btn = showAllButton(page);
  await expect(btn).toHaveCount(1);

  // check that the visible label is in ".govuk-accordion__show-all-text"
  await expect(btn).toContainText(label);
  await btn.click();

  // check button is visible after click
  await expect(btn).toBeVisible();
});

Then("all recent activity accordion sections should be expanded", async function () {
  const page = this.page;

  const buttons = accordion(page).locator("button.govuk-accordion__section-button");
  const count = await buttons.count();
  expect(count).toBeGreaterThan(0);

  for (let i = 0; i < count; i++) {
    await expect(buttons.nth(i)).toHaveAttribute("aria-expanded", "true");
  }
});

Then("all recent activity accordion sections should be collapsed", async function () {
  const page = this.page;

  const buttons = accordion(page).locator("button.govuk-accordion__section-button");
  const count = await buttons.count();
  expect(count).toBeGreaterThan(0);

  for (let i = 0; i < count; i++) {
    await expect(buttons.nth(i)).toHaveAttribute("aria-expanded", "false");
  }
});

// toggle sections

When("I expand the {string} section in recent activity", async function (headingText: string) {
  const page = this.page;
  await ensureSectionExpanded(page, headingText);
});

When("I collapse the {string} section in recent activity", async function (headingText: string) {
  const page = this.page;
  await ensureSectionCollapsed(page, headingText);
});

Then("the {string} section in recent activity should be expanded", async function (headingText: string) {
  const page = this.page;
  const btn = sectionToggleButtonByHeading(page, headingText);
  await expect(btn).toHaveAttribute("aria-expanded", "true");
});

Then("the {string} section in recent activity should be collapsed", async function (headingText: string) {
  const page = this.page;
  const btn = sectionToggleButtonByHeading(page, headingText);
  await expect(btn).toHaveAttribute("aria-expanded", "false");
});

Then(
  "the most recent section should contain a recent activity entry with status {string} and question {string} dated today",
  async function (
    status: string,
    questionText: string
  ) {
    const page = this.page;

    // format today's date like the ui as "16 February 2026"
    const now = new Date();

    const month = now.toLocaleDateString("en-GB", {
      month: "long"
    });

    const year = now.toLocaleDateString("en-GB", {
      year: "numeric"
    });

    const sectionHeading = `${month} ${year} activity`;


    // expand the section
    await ensureMostRecentSectionExpanded(page);
    const content = await mostRecentSectionContent(page);

    const formattedToday = now.toLocaleDateString("en-GB", {
      day: "numeric",
      month: "long",
      year: "numeric",
    });

    // check the date heading exists
    const dateHeading = content.getByRole("heading", {
      level: 3,
      name: formattedToday,
    });

    await expect(dateHeading).toBeVisible();

    // status + school validation
    const statusAndSchool = content.locator("p").filter({
      hasText: "Status of",
    });

    await expect(statusAndSchool).toContainText(`Status of '${status}'`);

    var schoolName = MAT_SCHOOLS_BY_URN[process.env.DSI_SCHOOL_ESTABLISHMENT_REF as string].NAME;

    await expect(statusAndSchool).toContainText(`by ${schoolName}.`);

    // question validation
    const questionStrong = content.locator("p strong", {
      hasText: questionText,
    });

    await expect(questionStrong).toBeVisible();

    // month check
    const monthYear = monthYearFromHeading(sectionHeading);
    const headingMonth = monthYear.month.toLowerCase();
    const currentMonth = now.toLocaleDateString("en-GB", {
      month: "long",
    }).toLowerCase();

    const headingYear = monthYear.year.toLowerCase();
    const currentYear = now.toLocaleDateString("en-GB", {
      year: "numeric"
    });

    expect(currentMonth).toBe(headingMonth);
    expect(currentYear).toBe(headingYear);

  }
);


When(
  "I set the recommendation status to {string} with note {string} and submit",
  async function (status: string, note: string) {
    const page = this.page;

    // select the radio by visible label
    await page.getByLabel(status, { exact: true }).check();

    // assert it is selected
    await expect(page.getByLabel(status, { exact: true })).toBeChecked();

    // add the notes
    const notesBox = page.locator("#Notes");
    await notesBox.fill(note);

    await expect(notesBox).toHaveValue(note);

    // submit the form
    const submitButton = page.getByRole("button", { name: /save/i });

    await Promise.all([
      this.page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
      await submitButton.click()
    ]);
  }
);

Then(
  "the section should show a status change to {string} with note {string} dated today",
  async function (status: string, note: string) {
    const page = this.page;

    // format today's date like the ui as "16 February 2026"
    const now = new Date();
    const todayText = now.toLocaleDateString("en-GB", {
      day: "numeric",
      month: "long",
      year: "numeric",
    });

    const month = now.toLocaleDateString("en-GB", {
      month: "long"
    });

    const year = now.toLocaleDateString("en-GB", {
      year: "numeric"
    });

    const sectionHeading = `${month} ${year}`;

    // expand the section
    await ensureSectionExpanded(page, sectionHeading);
    const content = await sectionContentByHeading(page, sectionHeading);

    // Find the *first* "Status changed to ..." entry (usually the latest at the top),
    // but we'll assert it matches the status+note you pass in.
    const statusChangedP = content
      .locator("p")
      .filter({ hasText: /Status changed to/i })
      .first();

    await expect(statusChangedP).toBeVisible();

    // strong contains the status
    const strong = statusChangedP.locator("strong");
    await expect(strong).toBeVisible();

    const actualStatusRaw = await strong.innerText();
    const actualStatus = normalizeSpaces(actualStatusRaw);
    expect(actualStatus).toBe(normalizeSpaces(status));

    // note is the next paragraph after the status line
    const noteP = statusChangedP.locator("xpath=following-sibling::p[1]");
    await expect(noteP).toBeVisible();

    const actualNoteRaw = await noteP.innerText();
    const actualNote = normalizeSpaces(actualNoteRaw);
    expect(actualNote).toBe(normalizeSpaces(note));

    // the date for this entry is 
    const dateHeadingForEntry = statusChangedP.locator(
      "xpath=preceding::h3[contains(@class,'govuk-heading-s')][1]"
    );

    await expect(dateHeadingForEntry).toHaveText(todayText);

    // month in accordion matches current month
    const monthYear = monthYearFromHeading(sectionHeading);
    const headingMonth = monthYear.month.toLowerCase();
    const currentMonth = now.toLocaleDateString("en-GB", {
      month: "long",
    }).toLowerCase();

    const headingYear = monthYear.year.toLowerCase();
    const currentYear = now.toLocaleDateString("en-GB", {
      year: "numeric"
    });

    expect(currentMonth).toBe(headingMonth);
    expect(currentYear).toBe(headingYear);
  }
);
