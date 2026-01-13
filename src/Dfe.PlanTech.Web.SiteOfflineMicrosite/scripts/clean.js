import { rmSync, existsSync, readdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const projectRoot = join(__dirname, '..');

const pathsToClean = [
  join(projectRoot, 'wwwroot', 'css'),
  join(projectRoot, 'wwwroot', 'js'),
  join(projectRoot, 'wwwroot', 'assets')
];

console.log('Cleaning generated assets...');

for (const path of pathsToClean) {
  if (existsSync(path)) {
    const items = readdirSync(path);
    for (const item of items) {
      if (item !== '.gitkeep') {
        const itemPath = join(path, item);
        rmSync(itemPath, { recursive: true, force: true });
      }
    }
    console.log(`✓ Cleaned ${path} (preserved .gitkeep)`);
  } else {
    console.log(`⊘ ${path} does not exist`);
  }
}

console.log('✓ Clean complete');

