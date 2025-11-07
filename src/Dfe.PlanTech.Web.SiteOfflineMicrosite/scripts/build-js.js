import { build } from 'esbuild';
import { fileURLToPath } from 'node:url';
import { dirname } from 'node:path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

try {
  await build({
    entryPoints: ['src/assets/js/site.js'],
    bundle: true,
    minify: true,
    outfile: 'wwwroot/js/site.js',
    format: 'iife',
    target: ['es2015'],
    platform: 'browser'
  });

  console.log('✓ JavaScript built successfully');
} catch (error) {
  console.error('Error building JavaScript:', error);
  process.exit(1);
}

