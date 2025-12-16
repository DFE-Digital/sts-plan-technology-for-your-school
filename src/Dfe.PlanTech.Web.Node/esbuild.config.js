import * as esbuild from "esbuild";
import { sassPlugin } from "esbuild-sass-plugin";
import { copyFileSync, cpSync, readdirSync } from "fs";
import { parse } from "path";

//Build main JS
await esbuild.build({
  entryPoints: ["scripts/app.js"],
  bundle: true,
  minify: true,
  sourcemap: true,
  outfile: "out/js/app.js",
});

//Build extra JS
const jsFilePaths = [
  "./node_modules/@govuk-prototype-kit/step-by-step/javascripts/step-by-step-navigation.js",
  "./node_modules/@govuk-prototype-kit/step-by-step/javascripts/step-by-step-polyfills.js",
];

const jsEntryPoints = Object.fromEntries(
  new Map(jsFilePaths.map((path) => [parse(path).name, path]))
);

await esbuild.build({
  entryPoints: jsEntryPoints,
  bundle: true,
  minify: true,
  sourcemap: true,
  outdir: "out/js/",
});

await esbuild.build({
  entryPoints: ["styles/scss/application.scss"],
  bundle: true,
  minify: true,
  sourcemap: true,
  target: ["chrome58", "firefox57", "safari11", "edge16"],
  external: ["/assets/*"],
  plugins: [
    sassPlugin({
      loader: { ".woff2": "file", ".png": "file" },
    }),
  ],
  outfile: "out/css/application.css",
});

await esbuild.build({
  entryPoints: ["styles/scss/step-by-step.scss"],
  bundle: true,
  minify: true,
  sourcemap: true,
  target: ["chrome58", "firefox57", "safari11", "edge16"],
  external: ["/assets/*"],
  plugins: [
    sassPlugin({
      loader: { ".woff2": "file", ".png": "file" },
    }),
  ],
  outfile: "out/css/step-by-step.css",
});

//Copy assets
//DFE
const dfeDir = "./node_modules/dfe-frontend/packages/assets";
readdirSync(dfeDir).forEach((file) => {
  if (file.indexOf(".png") == -1) {
    return;
  }

  cpSync(dfeDir + "/" + file, "./out/assets/images/" + file, {
    overwrite: true,
  });
});

//GOVUK
const govukDir = "./node_modules/govuk-frontend/dist/govuk/assets/";
const targetFolders = ["images", "fonts"];
//const ignoreFiles = ["favicon.ico"];

for (const folder of targetFolders) {
  const path = govukDir + folder;

  readdirSync(path).forEach((file) => {
    // if (ignoreFiles.some((ignoreFile) => ignoreFile == file)) {
    //   return;
    // }

    cpSync(`${path}/${file}`, `./out/assets/${folder}/${file}`, {
      overwrite: true,
    });

    cpSync(`${govukDir}rebrand`, `./out/assets/rebrand`, {
      overwrite: true,
      recursive: true
    });
  });
}

copyFileSync(
  "./node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.js",
  "./out/js/govuk-frontend.min.js"
);

copyFileSync(
  "./node_modules/dfe-frontend/dist/dfefrontend.min.js",
  "./out/js/dfefrontend.min.js"
);

//Inter (mandated font)
cpSync(`./fonts/`, `./out/assets/fonts`, {
  overwrite: true,
  recursive: true
});

//Copy to Dfe.PlanTech.Web
const targetDir = "../Dfe.PlanTech.Web/wwwroot";
const folders = ["css", "assets", "js"];

for (const folder of folders) {
  const path = `./out/${folder}`;

  cpSync(path, `${targetDir}/${folder}`, { recursive: true, overwrite: true });
}
