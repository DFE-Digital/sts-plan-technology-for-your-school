# 0034 - Entity Mapping

* **Status**: accepted

## Context and Problem Statement

How do we best map data to/from Contentful to our database schemas?

## Decision Drivers

- Open source
- Development best practices (maintainability, usability, reliability, etc.)

## Considered Options

- Manual (e.g. class per thing)
- Reflection where possible
- Automapper

## Decision Outcome

Using custom solution in Azure Functions, as this requires special work regarding relationships.

Otherwise, using AutoMapper for converting from DB -> Contentful/view models.