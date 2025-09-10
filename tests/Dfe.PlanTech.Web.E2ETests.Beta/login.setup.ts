import { chromium } from '@playwright/test';
import fs from 'fs';
import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(process.cwd(), '.env') });

const storageDir = path.resolve(__dirname, 'storage');
if (!fs.existsSync(storageDir)) {
  fs.mkdirSync(storageDir);
}

function buildUrl(base: string | undefined, pathPart: string): string {
  if (!base) throw new Error('URL env var is missing');
  return new URL(pathPart.replace(/^\//, ''), base.endsWith('/') ? base : base + '/').toString();
}

async function loginAndSaveSession(
  email: string,
  password: string,
  loginUrl: string,
  outputFilename: string
) {
  const isCI = !!process.env.CI;
  const headless = process.env.HEADLESS ? process.env.HEADLESS === 'true' : isCI; // CI => headless
  const slowMo = process.env.SLOWMO ? Number(process.env.SLOWMO) : (isCI ? 0 : 100);

  const browser = await chromium.launch({ headless, slowMo });

  const ignoreHTTPSErrors = process.env.CI === 'true';
  const context = await browser.newContext({ ignoreHTTPSErrors });

  const page = await context.newPage();

  try {
    await page.goto(loginUrl, { waitUntil: 'domcontentloaded' });

    await page.locator('input#username').waitFor({ timeout: 10000 });
    await page.fill('input#username', email);
    const usernameSubmit = page.locator('button.govuk-button').first();
    await usernameSubmit.click();

    await page.locator('input#password').waitFor({ timeout: 10000 });
    await page.fill('input#password', password);
    const passwordSubmit = page.locator('div.govuk-button-group button.govuk-button').first();
    await passwordSubmit.click();

  //Click the cookies banners so we get the cookie preferences set in the storage state.json
    await page.locator('button[name="accept-cookies"]').first().click();
    await page.locator('button[name="hide-cookies"]').first().click();

    // Settle the app
    await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => {});

    // Persist storage state
    const outputPath = path.resolve(storageDir, outputFilename);
    await context.storageState({ path: outputPath });
    console.log(`Saved storage state for ${outputFilename}`);
  } finally {
    await context.close().catch(() => {});
    await browser.close().catch(() => {});
  }
}

(async () => {
  const loginUrl = buildUrl(process.env.URL, 'home');

  const users = [
    { envEmail: 'DSI_SCHOOL_EMAIL', envPassword: 'DSI_SCHOOL_PASSWORD', output: 'school.json' },
    { envEmail: 'DSI_MAT_EMAIL', envPassword: 'DSI_MAT_PASSWORD', output: 'mat.json' },
    { envEmail: 'DSI_NOORG_EMAIL', envPassword: 'DSI_NOORG_PASSWORD', output: 'no-org.json' },
  ] as const;

  for (const user of users) {
    const email = process.env[user.envEmail];
    const password = process.env[user.envPassword];

    if (!email || !password) {
      console.warn(`Missing credentials for ${user.output} – skipped`);
      continue;
    }

    await loginAndSaveSession(email, password, loginUrl, user.output);
  }
})();