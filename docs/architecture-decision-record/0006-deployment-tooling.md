# 0006 - Deployment Tooling

- **Status**: Accepted

## Context and Problem Statement

How to ensure new versions of the application are consistently packaged, tested and deployed?

## Decision Drivers

- Within DfE’s Technical Guidance
- Open source as per Service Standard
- Compatible with selected hosting platform
- Support for automated application, security and accessibility testing
- Enables quick and frequent releases

## Considered Options

- GitHub Actions
- Azure DevOps

## Decision Outcome

Chosen option: Continuous Integration and Continuous Deployment with [GitHub Actions](https://github.com/features/actions) as this is open source.
