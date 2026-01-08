# plan-technology-for-your-school

Web application to help schools plan a technology roadmap

[![Code PR Check](https://github.com/DFE-Digital/plan-technology-for-your-school/actions/workflows/code-pr-check.yml/badge.svg)](https://github.com/DFE-Digital/plan-technology-for-your-school/actions/workflows/code-pr-check.yml)
[![Terraform PR Check](https://github.com/DFE-Digital/plan-technology-for-your-school/actions/workflows/terraform-pr-check.yml/badge.svg?branch=main)](https://github.com/DFE-Digital/plan-technology-for-your-school/actions/workflows/terraform-pr-check.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_plan-technology-for-your-school&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_plan-technology-for-your-school)

## Requirements

- .Net 8.0 and any supported IDE for DEV running.

## Running Locally

- The startup project is [./src/Dfe.PlanTech.Web](./src/Dfe.PlanTech.Web) with setup guidance in the [Readme](./src/Dfe.PlanTech.Web/README.md)

## Documentation

- [Authentication](./docs/Authentication.md)
- [Architecture Decision Records](./docs/adr/README.md)
- [Contentful](./docs/cms/README.md)
- [Conventions](./docs/Conventions.md)
- [Database Upgrades](./src/Dfe.PlanTech.DatabaseUpgrader/README.md)
- [GitHub Workflows](./docs/GitHub-Workflows.md)
- [Terraform Main](./terraform/container-app/README.md)
- [Terraform Configuration](./terraform/container-app/terraform-configuration.md)
- [Migration Scripts](./contentful/migration-scripts/README.md)
