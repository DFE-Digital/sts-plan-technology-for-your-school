# Dfe.PlanTech.Web

## Overview

Presentation layer for DfE's Plan Technology for your School service.

Uses ASP .NET Core MVC.

## Setup

- Setup your SignIn secrets, using the instructions in the [Dfe.PlanTech.Infrastructure.SignIn](../Dfe.PlanTech.Infrastructure.SignIn/README.md) project
- Setup your Contentful secrets, using the instructions in the [Dfe.PlanTech.Infrastructure.Contentful](../Dfe.PlanTech.Infrastructure.Contentful/README.md) project

## Project References

- Dfe.PlanTech.Application
- Dfe.PlanTech.Domain
- Dfe.PlanTech.Infrastructure.Contentful
- Dfe.PlanTech.Infrastructure.Data
- Dfe.PlanTech.Infrastructure.SignIn

## C# Packages

| Name                                                                                  | Purpose                            |
| ------------------------------------------------------------------------------------- | ---------------------------------- |
| [GovUk.Frontend.AspNetCore](https://github.com/DFE-Digital/govuk-frontend-aspnetcore) | Front-end framework for components |

## Javascript + Styling Compiling

- Uses NodeJS, with [Gulp](https://gulpjs.com/), to compile Styles and Scripts
- Scripts are located in the `/Scripts` folder and CSS/SASS are located in the `/Styles` folder

### NodeJS Packages Used

| Name                                                             | Purpose                                            |
|------------------------------------------------------------------|----------------------------------------------------|
| [Gulp](https://gulpjs.com/)                                      | Compiling/minifying/combining/etc. SCSS + JS files |
| [govuk-frontend](https://www.npmjs.com/package/govuk-frontend)   | Gov.UK SCSS                                        |
| [dfe-frontend](https://www.npmjs.com/package/dfe-frontend)       | DFE SCSS overrides                                 |
| [gulp-csso](https://www.npmjs.com/package/gulp-csso)             | CSS minification                                   |
| [gulp-sass](https://www.npmjs.com/package/gulp-sass)             | SASS compiling                                     |
| [gulp-sourcemaps](https://www.npmjs.com/package/gulp-sourcemaps) | Sourcemaps creation                                |
