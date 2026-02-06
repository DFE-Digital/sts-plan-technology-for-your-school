# 0009 - Container Registry

- **Status**: Accepted

## Context and Problem Statement

As the application will be containerised with Docker where is the best place solution to store the associated Docker images?

## Decision Drivers

- Within DfE’s Technical Guidance
- Compatible with selected hosting platform
- Simple with minimal configuration

## Considered Options

- Azure Container Registry
- Docker Hub

## Decision Outcome

Chosen option: [Azure Container Registry](https://azure.microsoft.com/en-us/products/container-registry) as this is within the Azure ecosystem making configuration simpler.
