import { rmSync, existsSync, readdirSync, statSync, mkdirSync, writeFileSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const projectRoot = join(__dirname, '..');

const pathsToClean = [
  join(projectRoot, 'wwwroot', 'css'),
  join(projectRoot, 'wwwroot', 'js'),
  join(projectRoot, 'wwwroot', 'assets')
];

console.log('Cleaning generated assets...');

pathsToClean.forEach(path => {
  if (existsSync(path)) {
    const items = readdirSync(path);
    items.forEach(item => {
      if (item !== '.gitkeep') {
        const itemPath = join(path, item);
        rmSync(itemPath, { recursive: true, force: true });
      }
    });
    console.log(`✓ Cleaned ${path} (preserved .gitkeep)`);
  } else {
    console.log(`⊘ ${path} does not exist`);
  }
});

console.log('✓ Clean complete');

