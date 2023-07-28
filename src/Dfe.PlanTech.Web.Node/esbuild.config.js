import * as esbuild from 'esbuild';
import { sassPlugin } from 'esbuild-sass-plugin';
import { cpSync, readdirSync } from 'fs';

//Build JS
await esbuild.build({
  entryPoints: ['scripts/app.mjs'],
  bundle: true,
  minify: true,
  sourcemap: true,
  target: ['chrome58', 'firefox57', 'safari11', 'edge16'],
  outfile: 'out/scripts/app.js',
})

//Builds SASS
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

//Copy assets
//DFE
const dfeDir = "./node_modules/dfe-frontend-alpha/packages/assets";
readdirSync(dfeDir).forEach(file => {
  if (file.indexOf(".png") == -1)
  {
    return;
  }

  cpSync(dfeDir + "/" + file, "./out/assets/images/" + file);
})

const govukDir = "./node_modules/govuk-frontend/govuk/assets/";
const targetFolders = ["images", "fonts"];
const ignoreFiles = ["favicon.ico"];

for (const folder of targetFolders) {
  const path = govukDir + folder;

  readdirSync(path).forEach(file => {
    if (ignoreFiles.some(ignoreFile => ignoreFile == file)) {
      return;
    }

    cpSync(`${path}/${file}`, `./out/assets/${folder}/${file}`);
  })
  
}
