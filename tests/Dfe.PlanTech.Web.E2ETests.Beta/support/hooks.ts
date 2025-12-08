import {
  Before,
  BeforeAll,
  AfterAll,
  After,
  Status,
  ITestCaseHookParameter,
} from '@cucumber/cucumber';
import { chromium, Browser, BrowserContext, Page } from '@playwright/test';
import path from 'path';
import fs from 'fs';
import { clearTestEstablishmentData } from '../clearTestDataSqlAzure';
import { setDefaultTimeout } from '@cucumber/cucumber';
import { MAT_SCHOOLS } from '../constants/matConstants';

type MatSchoolKey = keyof typeof MAT_SCHOOLS;

setDefaultTimeout(60 * 1000)

let browser: Browser;

function ensureDirExists(dirPath: string) {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
  }
}

BeforeAll(async () => {
  const isHeadless = process.env.HEADLESS == 'true';
  browser = await chromium.launch({ headless: isHeadless, slowMo: 150 });
});

Before(async function (scenario: ITestCaseHookParameter) {
const tag = scenario.pickle.tags.find(t =>
  t.name.startsWith('@user-')
);

const userType = tag
  ? tag.name.replace('@user-', '')
  : 'school'; 

  const storagePath = path.resolve(__dirname, `../storage/${userType}.json`);

  const shouldRecord = this.parameters?.record === true;

  const contextOptions: Parameters<typeof browser.newContext>[0] = {
    storageState: storagePath,
    ignoreHTTPSErrors: true,
  };

  if (shouldRecord) {
    const tempVideoDir = path.resolve(__dirname, '../videos/temp');
    ensureDirExists(tempVideoDir);
    contextOptions.recordVideo = {
      dir: tempVideoDir,
      size: { width: 1280, height: 720 },
    };
  }

  const context = await browser.newContext(contextOptions);

  context.setDefaultTimeout(30_000);
  context.setDefaultNavigationTimeout(45_000);

  if (shouldRecord) {
    await context.tracing.start({ screenshots: true, snapshots: true });
  }

  const page = await context.newPage();

  this.context = context;
  this.page = page;
  this.shouldRecord = shouldRecord;

  const resetTag = scenario.pickle.tags.find(t =>
    t.name === '@clear-data-school' || t.name === '@clear-data-mat'
  );

  if (resetTag) {
    const tagName = resetTag.name;

    let establishmentRef: string | undefined;

    if (tagName === '@clear-data-school') {
      establishmentRef = process.env.DSI_SCHOOL_ESTABLISHMENT_REF;
    } else if (tagName === '@clear-data-mat') {
      establishmentRef = process.env.DSI_MAT_ESTABLISHMENT_REF;
    }

    if (!establishmentRef) {
      throw new Error(`No establishmentRef found for tag ${tagName}. Check the environment variables.`);
    }

    console.log(`Clearing establishment data for establishmentRef: ${establishmentRef}`);

    await clearTestEstablishmentData(establishmentRef);
  }

  const schoolTag = scenario.pickle.tags.find(t =>
    t.name.startsWith('@selected-school-')
  );

  if (schoolTag) {
    const rawKey = schoolTag.name.replace('@selected-school-', '');
    const keyFromTag = rawKey.toUpperCase() as MatSchoolKey;

    if (!(keyFromTag in MAT_SCHOOLS)) {
      throw new Error(
        `Unknown school key "${keyFromTag}" in tag ${schoolTag.name}. ` +
        `Expected one of: ${Object.keys(MAT_SCHOOLS).join(', ')}`
      );
    }

    this.selectedSchool = MAT_SCHOOLS[keyFromTag];

    if (this.selectedSchool) {
      console.log(`Auto-selecting school: ${this.selectedSchool.NAME}`);

      await this.page.goto(`${process.env.URL}home`);

      await this.page.getByRole('button', { name: this.selectedSchool.NAME }).click();

      console.log(`Navigated to dashboard for: ${this.selectedSchool.NAME}`);
    }

    console.log(`Selected school via tag: ${this.selectedSchool.NAME}`);
  }

});

After(async function (scenario: ITestCaseHookParameter) {
  const scenarioName = scenario.pickle.name.replace(/[^a-zA-Z0-9]/g, '_');
  const featureName = path.basename(scenario.pickle.uri || 'unknown', '.feature');

  const shouldRecord = this.shouldRecord;
  const page = this.page as Page | undefined;
  const context = this.context as BrowserContext | undefined;

  const baseDir = path.resolve(__dirname, `../`);
  const videoDir = path.join(baseDir, 'videos', featureName);
  const screenshotDir = path.join(baseDir, 'screenshots', featureName);
  const traceDir = path.join(baseDir, 'traces', featureName);

  if (shouldRecord) {
    ensureDirExists(videoDir);
    ensureDirExists(screenshotDir);
    ensureDirExists(traceDir);

    if (page) {
      try {
        const screenshotPath = path.join(screenshotDir, `${scenarioName}.png`);
        await page.screenshot({ path: screenshotPath });
        console.log(`Screenshot saved: ${screenshotPath}`);
      } catch (err) {
        console.warn(`Failed to take screenshot for ${scenarioName}:`, err);
      }
    }

    try {
      if (context && context.tracing) {
        const tracePath = path.join(traceDir, `${scenarioName}.zip`);
        await context.tracing.stop({ path: tracePath });
        console.log(`🧪 Trace saved: ${tracePath}`);
      }
    } catch (err) {
      console.warn(`Failed to stop tracing for ${scenarioName}:`, err);
    }
  }

  let savedVideo = false;
  if (page) {
    await page.close();
    if (shouldRecord) {
      const video = page.video();
      if (video) {
        try {
          const newVideoPath = path.join(videoDir, `${scenarioName}.webm`);
          await video.saveAs(newVideoPath);
          await video.delete();
          savedVideo = true;
          console.log(`Video saved: ${newVideoPath}`);
        } catch (err) {
          console.warn(`Failed to save video for ${scenarioName}:`, err);
        }
      }
    }
  }

  await context?.close();
});

AfterAll(async () => {
  try {
    if (browser) {
      await Promise.all(browser.contexts().map(c => c.close().catch(() => { })));
      await browser.close();
    }
  } catch (e) {
    console.warn('Browser close error:', e);
  }
});
