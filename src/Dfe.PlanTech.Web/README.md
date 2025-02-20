# Dfe.PlanTech.Web

## Overview

Presentation layer for DfE's Plan Technology for your School service.

Uses ASP .NET Core MVC.

## Setup

- Setup your SignIn secrets, using the instructions in the [Dfe.PlanTech.Infrastructure.SignIn](../Dfe.PlanTech.Infrastructure.SignIn/README.md) project
- Setup your Contentful secrets, using the instructions in the [Dfe.PlanTech.Infrastructure.Contentful](../Dfe.PlanTech.Infrastructure.Contentful/README.md) project
- Setup your Redis connection, using the instructions in the [Dfe.PlanTech.Infrastructure.Redis](../Dfe.PlanTech.Infrastructure.Redis/README.md) project

The collection of all secrets is stored in the Azure keyvault, which you can use to speed up the setup process.

## Project References

- Dfe.PlanTech.Application
- Dfe.PlanTech.Domain
- Dfe.PlanTech.Infrastructure.Contentful
- Dfe.PlanTech.Infrastructure.Data
- Dfe.PlanTech.Infrastructure.SignIn
- Dfe.PlanTech.Infrastructure.Redis

## C# Packages

| Name                                                                                  | Purpose                            |
| ------------------------------------------------------------------------------------- | ---------------------------------- |
| [GovUk.Frontend.AspNetCore](https://github.com/DFE-Digital/govuk-frontend-aspnetcore) | Front-end framework for components |

## Javascript + Styling Compiling

- Uses NodeJS, with [Gulp](https://gulpjs.com/), to compile Styles and Scripts
- Scripts are located in the [/Scripts](src/Dfe.PlanTech.Web.Node/scripts) folder and CSS/SASS are located in the [/Styles](src/Dfe.PlanTech.Web.Node/styles) folder
- Guidance is provided in the [Readme](src/Dfe.PlanTech.Web.Node/README.md)

### NodeJS Packages Used

| Name                                                             | Purpose                                            |
| ---------------------------------------------------------------- | -------------------------------------------------- |
| [Gulp](https://gulpjs.com/)                                      | Compiling/minifying/combining/etc. SCSS + JS files |
| [govuk-frontend](https://www.npmjs.com/package/govuk-frontend)   | Gov.UK SCSS                                        |
| [dfe-frontend](https://www.npmjs.com/package/dfe-frontend)       | DFE SCSS overrides                                 |
| [gulp-csso](https://www.npmjs.com/package/gulp-csso)             | CSS minification                                   |
| [gulp-sass](https://www.npmjs.com/package/gulp-sass)             | SASS compiling                                     |
| [gulp-sourcemaps](https://www.npmjs.com/package/gulp-sourcemaps) | Sourcemaps creation                                |

## Using a local Redis or Database instance

- If you want to use a local instance of Redis, instructions are provided in the [Readme](../Dfe.PlanTech.Infrastructure.Redis/README.md)
- If you want to use a local database, the [SeedTestData Readme](../../tests/Dfe.PlanTech.Web.SeedTestData/README.md) contains instructions on setting up a local test database, and changing the connection string to use it
