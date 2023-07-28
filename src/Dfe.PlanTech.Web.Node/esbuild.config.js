import * as esbuild from 'esbuild';
import { sassPlugin } from 'esbuild-sass-plugin';

await esbuild.build({
  entryPoints: ['scripts/app.mjs'],
  bundle: true,
  minify: true,
  sourcemap: true,
  target: ['chrome58', 'firefox57', 'safari11', 'edge16'],
  outfile: 'out/scripts/app.js',
})

await esbuild.build({
  entryPoints: ['styles/scss/application.scss'],
  bundle: true,
  minify: true,
  sourcemap: true,
  target: ['chrome58', 'firefox57', 'safari11', 'edge16'],
  external: ['/assets/*'],
  plugins: [sassPlugin({
    loader: { ".woff2": "file", ".png": "file" },
  })],
  outfile: 'out/styles/application.css'
});