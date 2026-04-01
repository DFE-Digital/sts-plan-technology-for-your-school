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
  outputFilename: string,
) {
  const isCI = !!process.env.CI;
  const headless = process.env.HEADLESS ? process.env.HEADLESS === 'true' : isCI; // CI => headless
  const slowMo = process.env.SLOWMO ? Number(process.env.SLOWMO) : isCI ? 0 : 100;


  const browser = await chromium.launch({ headless, slowMo });

  const ignoreHTTPSErrors = process.env.CI === 'true';
  const context = await browser.newContext({ ignoreHTTPSErrors });
  const cookieUrl = new URL(loginUrl).origin;

  await context.addCookies([
    {
      name: 'user_cookie_preferences',
      value: JSON.stringify({
        IsVisible: false,
        UserAcceptsCookies: true,
      }),
      url: cookieUrl,
      httpOnly: false,
      secure: cookieUrl.startsWith('https://'),
      sameSite: 'Lax',
    },
  ]);

  const page = await context.newPage();

  try {
    await page.goto(loginUrl, { waitUntil: 'domcontentloaded' });

    try {
      await page.locator('input#username').waitFor({ timeout: 10000 });
    } catch (err) {
      console.error('Timeout waiting for #username');
      console.error('Final URL:', page.url());

      throw err;
    }

    await page.fill('input#username', email);
    const usernameSubmit = page.locator('button.govuk-button').first();
    await usernameSubmit.click();

    await page.locator('input#password').waitFor({ timeout: 10000 });
    await page.fill('input#password', password);
    const passwordSubmit = page.locator('div.govuk-button-group button.govuk-button').first();
    await passwordSubmit.click();

    // Settle the app
    await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => { });

    // Persist storage state
    const outputPath = path.resolve(storageDir, outputFilename);
    await context.storageState({ path: outputPath });
    console.log(`Saved storage state for ${outputFilename}`);
  } finally {
    await context.close().catch(() => { });
    await browser.close().catch(() => { });
  }
}

(async () => {
  const loginUrl = buildUrl(process.env.URL, 'home');

  console.log('MOCK_AUTH_MODE:', process.env.MOCK_AUTH_MODE);

  const isMockAuth = process.env.MOCK_AUTH_MODE == 'true';

  if (!isMockAuth) {
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

  }
  else {

    await loginAndSaveMockSession('school', process.env.URL as string, 'school.json');
    await loginAndSaveMockSession('mat', process.env.URL as string, 'mat.json');
    await loginAndSaveMockSession('noorg', process.env.URL as string, 'no-org.json');
  }


})();


async function loginAndSaveMockSession(
  organisationType: string,
  loginUrl: string,
  outputFilename: string,
) {
  const isCI = !!process.env.CI;
  const headless = process.env.HEADLESS ? process.env.HEADLESS === 'true' : isCI; // CI => headless
  const slowMo = process.env.SLOWMO ? Number(process.env.SLOWMO) : isCI ? 0 : 100;

  const browser = await chromium.launch({ headless, slowMo });

  const ignoreHTTPSErrors = process.env.CI === 'true';
  const context = await browser.newContext({ ignoreHTTPSErrors });

  const cookieUrl = new URL(loginUrl).origin;

  await context.addCookies([
    {
      name: 'e2e_user',
      value: organisationType,
      url: cookieUrl,
      httpOnly: false,
      secure: cookieUrl.startsWith('https://'),
      sameSite: 'Lax',
    },
    {
      name: 'e2e_key',
      value: process.env.MOCK_AUTH_CLIENT_SECRET as string,
      url: cookieUrl,
      httpOnly: false,
      secure: cookieUrl.startsWith('https://'),
      sameSite: 'Lax',
    },
    {
      name: 'user_cookie_preferences',
      value: JSON.stringify({
        IsVisible: false,
        UserAcceptsCookies: true,
      }),
      url: cookieUrl,
      httpOnly: false,
      secure: cookieUrl.startsWith('https://'),
      sameSite: 'Lax',
    },
  ]);

  const page = await context.newPage();

  try {

    await page.goto(loginUrl, { waitUntil: 'domcontentloaded' });

    try {
      await page.locator('.govuk-heading-xl').waitFor({ timeout: 10000 });
    } catch (err) {
      console.error('Timeout waiting for h1 header');
      console.error('Final URL:', page.url());

      throw err;
    }

    const submit = page.getByRole('button', { name: 'Go to DfE Sign-in' });
    await submit.click();

    // Settle the app
    await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => { });

    // Persist storage state
    const outputPath = path.resolve(storageDir, outputFilename);
    await context.storageState({ path: outputPath });
    console.log(`Saved storage state for ${outputFilename}`);
  } finally {
    await context.close().catch(() => { });
    await browser.close().catch(() => { });
  }
}
