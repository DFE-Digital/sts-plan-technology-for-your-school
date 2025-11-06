import { cpSync, existsSync, mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const projectRoot = join(__dirname, '..');

// Note: GOV.UK Frontend assets are provided by the C#/dotnet GovUk.Frontend.AspNetCore NuGet package
// and served from _content/GovUk.Frontend.AspNetCore/

// Copy DfE Frontend logo assets
const dfeAssetsSource = join(projectRoot, 'node_modules', 'dfe-frontend', 'packages', 'assets');
const dfeImagesDest = join(projectRoot, 'wwwroot', 'assets', 'images');

if (existsSync(dfeAssetsSource)) {
  if (!existsSync(dfeImagesDest)) {
    mkdirSync(dfeImagesDest, { recursive: true });
  }
  // Copy DfE logo files to images folder
  const logoFiles = ['dfe-logo.png', 'dfe-logo-alt.png'];
  for (const file of logoFiles) {
    const src = join(dfeAssetsSource, file);
    const dest = join(dfeImagesDest, file);
    if (existsSync(src)) {
      cpSync(src, dest);
    }
  }
  console.log('✓ DfE Frontend logo assets copied');
}

