import { chromium } from '@playwright/test';
import fs from 'fs';
import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(process.cwd(), '.env') });

const storageDir = path.resolve(__dirname, 'storage');

if (!fs.existsSync(storageDir)) {
  fs.mkdirSync(storageDir);
}

async function loginAndSaveSession(email: string, password: string, loginUrl: string, outputFilename: string) {
const browser = await chromium.launch({ headless: false, slowMo: 100 });
  const context = await browser.newContext();
  const page = await context.newPage();

  await page.goto(loginUrl);

  await page.locator('input#username').fill(email);
  await page.locator('button.govuk-button').first().click();

  await page.locator('input#password').waitFor({ timeout: 4000 });
  await page.locator('input#password').fill(password);
  await page.locator('div.govuk-button-group button.govuk-button').first().click();

  await page.waitForLoadState('networkidle');

  const outputPath = path.resolve(storageDir, outputFilename);
  await context.storageState({ path: outputPath });

  console.log(`Saved storage state for ${outputFilename}`);
  await browser.close();
}

(async () => {
  const loginUrl = `${process.env.URL}self-assessment`; 

  const users = [
    {
      envEmail: 'DSI_SCHOOL_EMAIL',
      envPassword: 'DSI_SCHOOL_PASSWORD',
      output: 'school.json',
    },
    {
      envEmail: 'DSI_MAT_EMAIL',
      envPassword: 'DSI_MAT_PASSWORD',
      output: 'mat.json',
    },
    {
      envEmail: 'DSI_NOORG_EMAIL',
      envPassword: 'DSI_NOORG_PASSWORD',
      output: 'no-org.json',
    },
  ];

  for (const user of users) {
    const email = process.env[user.envEmail];
    const password = process.env[user.envPassword];

    if (!email || !password) {
      console.warn(`Missing credentials for ${user.output} - skipped`);
      continue;
    }

    await loginAndSaveSession(email, password, loginUrl, user.output);
  }
})();
