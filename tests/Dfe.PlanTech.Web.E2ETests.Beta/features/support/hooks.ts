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

let browser: Browser;

/**
 * Ensure a directory exists. Create it recursively if it doesn't.
 */
function ensureDirExists(dirPath: string) {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
  }
}

BeforeAll(async () => {
  const isHeadless = process.env.HEADLESS == 'true';
  //browser = await chromium.launch({ headless: isHeadless });
  browser = await chromium.launch({ headless: isHeadless });
});

Before(async function (scenario: ITestCaseHookParameter) {
  const tag = scenario.pickle.tags.find((t) => t.name.startsWith('@user-'));
  const userType = tag ? tag.name.replace('@user-', '') : 'school';
  const storagePath = path.resolve(__dirname, `../../storage/${userType}.json`);

  const shouldRecord = this.parameters?.record === true;

  const contextOptions: Parameters<typeof browser.newContext>[0] = {
    storageState: storagePath,
  };

  if (shouldRecord) {
    contextOptions.recordVideo = {
      dir: path.resolve(__dirname, '../../videos/temp'), // Temporary folder — will rename later
    };
  }

  const context = await browser.newContext(contextOptions);

  if (shouldRecord) {
    await context.tracing.start({ screenshots: true, snapshots: true });
  }

  const page = await context.newPage();

  this.context = context;
  this.page = page;
  this.shouldRecord = shouldRecord;
});

After(async function (scenario: ITestCaseHookParameter) {
  const scenarioName = scenario.pickle.name.replace(/[^a-zA-Z0-9]/g, '_');
  const featureName = path.basename(scenario.pickle.uri || 'unknown', '.feature');

  const shouldRecord = this.shouldRecord;
  const page = this.page;
  const context = this.context;

  const baseDir = path.resolve(__dirname, `../../`);
  const videoDir = path.join(baseDir, 'videos', featureName);
  const screenshotDir = path.join(baseDir, 'screenshots', featureName);
  const traceDir = path.join(baseDir, 'traces', featureName);

  if (shouldRecord) {
    ensureDirExists(videoDir);
    ensureDirExists(screenshotDir);
    ensureDirExists(traceDir);

    if (scenario.result?.status === Status.FAILED) {
      if (page) {
        try {
          const screenshotPath = path.join(screenshotDir, `${scenarioName}.png`);
          await page.screenshot({ path: screenshotPath });
          console.log(`Screenshot saved: ${screenshotPath}`);
        } catch (err) {
          console.warn(`Failed to take screenshot for ${scenarioName}:`, err);
        }
      } else {
        console.warn(`Page not available for screenshot in: ${scenarioName}`);
      }

      if (context) {
        try {
          const tracePath = path.join(traceDir, `${scenarioName}.zip`);
          await context.tracing.stop({ path: tracePath });
          console.log(`🧪 Trace saved: ${tracePath}`);
        } catch (err) {
          console.warn(`Failed to stop tracing for ${scenarioName}:`, err);
        }
      }
    } else {
      await context?.tracing.stop();
    }
  }

  await page?.close();
  await context?.close();

  if (shouldRecord && page) {
    const video = page.video();
    if (video) {
      try {
        const videoPath = await video.path();
        const newVideoPath = path.join(videoDir, `${scenarioName}.webm`);
        await fs.promises.rename(videoPath, newVideoPath);
        console.log(`Video saved: ${newVideoPath}`);
      } catch (err) {
        console.warn(`Failed to save video for ${scenarioName}:`, err);
      }
    }
  }
});

AfterAll(async () => {
  await browser?.close();
});
