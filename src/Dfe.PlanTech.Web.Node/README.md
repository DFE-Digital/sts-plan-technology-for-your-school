# Dfe.PlanTech.Web.Node

This project contains any additional Javascript and CSS/SASS files necessary for the main project.

In addition, it downloads the Gov.UK & DFE stylings, bundles them and copies them to the main project as well.

## Packages Used

| Name           | Purpose                                                  |
| -------------- | -------------------------------------------------------- |
| govuk-frontend | Gov.UK front-end CSS/JS files                            |
| dfe-frontend   | DFE front-end stylings to build ontop of the Gov.UK ones |
| esbuild        | Compiling, bundling, minifying                           |
| sass           |                                                          |
| fs             | Copy/moving files after build                            |

## Usage

1. Add JS scripts to the `scripts` folder and initialise/reference/etc. them in the `scripts/app.js` file
2. Add CSS/SASS/SCSS scripts to the `styles` folder and import them into `styles/application.scss` file
3. Install all required Node packages by `npm install`
4. Compile the JS + CSS files by running `npm run build`. This also copies the generated files into the correct places in the `~/src/Dfe.PlanTech.Web/wwwroot/` folder as well

There is also a GitHub action setup that will automatically execute step 4 on a PR when changes have been made in this folder.

You can also run the `dotnet build /p:buildWebAssets=true` command from `Dfe.PlanTech.Web` to build the project and the above dependencies.
