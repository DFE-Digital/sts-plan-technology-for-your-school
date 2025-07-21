import dotenv from 'dotenv';
dotenv.config();

import { setWorldConstructor, World } from '@cucumber/cucumber';
import { Page, BrowserContext } from '@playwright/test';

export class CustomWorld extends World {
  page!: Page;
  context!: BrowserContext;
}

setWorldConstructor(CustomWorld);
